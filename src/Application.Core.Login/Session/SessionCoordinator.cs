/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2016 - 2019 RonanLana

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using Application.Core.Login.Client;
using Application.EF;
using Application.Shared.Sessions;
using Application.Utility.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Application.Core.Login.Session;


/**
 * @author Ronan
 *
 * 登录服务器会话协调器 — 管理客户端登录/游戏会话的生命周期及并发冲突检测。
 *
 * 核心职责：
 *   1. 多客户端限制（Anti-Multiclient）：通过 HWID、IP + nibbleHWID、账户尝试频率三重维度
 *      阻止同一用户重复登录或同一机器多开。
 *   2. HWID 管理：追踪 HWID 与账户的关联关系，持久化到数据库，并支持定期过期清理。
 *   3. 会话状态维护：维护在线客户端字典、在线 HWID 集合、正在登录中的远程主机映射，
 *      确保同一账户/HWID/IP 不会同时建立多个会话。
 *   4. 从登录态到游戏态的无缝迁移：凭据校验通过后，将 HWID 从 "登录中" 集合迁移到 "游戏中" 集合，
 *      并记录主机缓存供后续快速校验。
 *   5. 日志追踪：提供快照方法 printSessionTrace() 输出当前在线客户端/HWID/登录会话明细，
 *      辅助运维排障。
 */
public class SessionCoordinator
{
    readonly ILogger<SessionCoordinator> _logger;

    /// <summary>会话初始化锁（按 remoteHost 粒度），防止同一个远端同时发起多个初始化请求。</summary>
    private SessionInitialization sessionInit;
    /// <summary>登录尝试频率存储，防止同一账户短时内大量重试。</summary>
    private LoginStorage loginStorage;
    /// <summary>已登录的在线客户端字典，Key = 账户 Id，Value = 登录客户端。</summary>
    private Dictionary<int, ILoginClient> onlineClients = new();
    /// <summary>当前在线（含登录中 + 游戏中）的 HWID 集合，用于去重。</summary>
    private HashSet<Hwid> onlineRemoteHwids = new();
    /// <summary>正在登录流程中的远程主机集合，Key = "IP(+nibbleHWID)"，防止同一主机重叠发起登录。</summary>
    private ConcurrentDictionary<string, ILoginClient> loginRemoteHosts = new();

    /// <summary>主机 → HWID 缓存：登录成功后记录，供后续同 IP 快速恢复 HWID。</summary>
    private HostHwidCache hostHwidCache;
    /// <summary>会话数据访问层，封装 HWID 关联表的 CRUD。</summary>
    readonly SessionDAO _sessionDAO;
    /// <summary>EF Core DbContext 工厂，按需创建作用域内上下文。</summary>
    readonly IDbContextFactory<DBContext> _dbContextFactory;
    /// <summary>HWID-账户关联的过期策略配置。</summary>
    readonly HwidAssociationExpiry _hwidAssociationExpiry;
    public SessionCoordinator(
        ILogger<SessionCoordinator> logger,
        HostHwidCache hostHwidCache,
        SessionDAO sessionDAO,
        IDbContextFactory<DBContext> dbContextFactory,
        HwidAssociationExpiry hwidAssociationExpiry,
        LoginStorage loginStorage,
        SessionInitialization sessionInitialization)
    {
        _logger = logger;
        this.hostHwidCache = hostHwidCache;
        _sessionDAO = sessionDAO;
        _dbContextFactory = dbContextFactory;
        _hwidAssociationExpiry = hwidAssociationExpiry;
        this.loginStorage = loginStorage;
        sessionInit = sessionInitialization;
    }

    /// <summary>
    /// 检查指定账户是否允许使用指定的 HWID 登录。
    /// 规则：
    ///   1. 如果该 HWID 已与该账户关联（HwidRelevance 中能查到匹配），则允许访问，
    ///      且非例行检查时递增关联新鲜度（relevance）。
    ///   2. 若未关联但关联总数未达上限（MAX_ALLOWED_ACCOUNT_HWID），也允许访问（首次绑定）。
    ///   3. 超过上限则拒绝。
    /// </summary>
    /// <param name="accountId">账户 Id</param>
    /// <param name="hwid">本次登录的 HWID</param>
    /// <param name="routineCheck">是否为例行检查（true 时不更新 relevance）</param>
    /// <returns>true 表示允许访问</returns>
    private bool attemptAccountAccess(int accountId, Hwid hwid, bool routineCheck)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            List<HwidRelevance> hwidRelevances = _sessionDAO.getHwidRelevance(dbContext, accountId);
            foreach (HwidRelevance hwidRelevance in hwidRelevances)
            {
                if (hwidRelevance.hwid.EndsWith(hwid.hwid))
                {
                    if (!routineCheck)
                    {
                        // better update HWID relevance as soon as the login is authenticated
                        var expiry = _hwidAssociationExpiry.getHwidAccountExpiry(hwidRelevance.relevance);
                        _sessionDAO.updateAccountAccess(dbContext, hwid, accountId, expiry, hwidRelevance.getIncrementedRelevance());
                    }

                    return true;
                }
            }

            if (hwidRelevances.Count < YamlConfig.config.server.MAX_ALLOWED_ACCOUNT_HWID)
            {
                return true;
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to update account access. Account id: {AccountId}, nibbleHwid: {HWID}", accountId, hwid);
        }

        return false;
    }

    /// <summary>
    /// 更新在线客户端记录。如果该账户已有在线客户端，先强制断开旧连接再替换。
    /// 用于 CMS "Unstuck" 等场景，确保同一账户只有一个活跃会话。
    /// </summary>
    public void updateOnlineClient(ILoginClient? client)
    {
        if (client != null && client.AccountEntity != null)
        {
            int accountId = client.AccountEntity.Id;

            var ingameClient = onlineClients.GetValueOrDefault(accountId);
            if (ingameClient != null)
            {
                // thanks MedicOP for finding out a loss of loggedin account uniqueness when using the CMS "Unstuck" feature
                ingameClient.ForceDisconnect();
            }
            onlineClients[accountId] = client;
        }
    }

    /// <summary>
    /// 检查并尝试注册一次登录会话的启动。
    /// 仅在 DETERRED_MULTICLIENT = true 时执行多客户端限制逻辑：
    ///   - 先通过 sessionInit 获取 remoteHost 粒度的初始化锁；
    ///   - 检查该 remoteHost 是否已有已知 HWID 且正处于在线状态 → 拒绝；
    ///   - 检查该 remoteHost 是否已在登录流程中 → 拒绝；
    ///   - 通过后将客户端注册到 loginRemoteHosts，表示 "正在登录中"。
    /// </summary>
    public bool canStartLoginSession(ILoginClient client)
    {
        if (!YamlConfig.config.server.DETERRED_MULTICLIENT)
        {
            return true;
        }

        string remoteHost = client.GetSessionRemoteHost();
        InitializationResult initResult = sessionInit.initialize(remoteHost);
        switch (initResult.getAntiMulticlientResult())
        {
            case AntiMulticlientResult.REMOTE_PROCESSING:
                return false;
            case AntiMulticlientResult.COORDINATOR_ERROR:
                return true;
        }

        try
        {
            var knownHwid = hostHwidCache.getEntry(remoteHost);
            if (knownHwid != null && onlineRemoteHwids.Contains(knownHwid.hwid))
                return false;
            else if (loginRemoteHosts.ContainsKey(remoteHost))
                return false;

            loginRemoteHosts[remoteHost] = client;
            return true;
        }
        finally
        {
            sessionInit.finalize(remoteHost);
        }
    }

    /// <summary>
    /// 关闭登录会话：从 loginRemoteHosts 移除记录，清除 HWID 关联的在线状态，
    /// 并清理 onlineClients（仅当该客户端仍在登录态且未被游戏会话接管时）。
    /// </summary>
    public void closeLoginSession(ILoginClient client)
    {
        string remoteHost = client.GetSessionRemoteHost();
        loginRemoteHosts.TryRemove(client.RemoteAddress, out var _);
        loginRemoteHosts.TryRemove(remoteHost, out var _);

        Hwid? nibbleHwid = client.Hwid;
        client.Hwid = null;
        if (nibbleHwid != null)
        {
            onlineRemoteHwids.Remove(nibbleHwid);

            if (client != null && client.AccountEntity != null)
            {
                var loggedClient = onlineClients.GetValueOrDefault(client.AccountEntity.Id);

                // do not remove an online game session here, only login session
                if (loggedClient != null && loggedClient.SessionId == client.SessionId)
                {
                    onlineClients.Remove(client.AccountEntity.Id);
                }
            }
        }
    }


    /// <summary>
    /// 执行登录会话尝试的完整检查（密码/PIC 验证通过后调用）。
    /// 检查链路（短路返回）：
    ///   1. loginStorage 防刷 —— 同一账户短时间内不能发起过多次登录请求。
    ///   2. 例行检查时校验账户-HWID 关联是否超限。
    ///   3. 检查该 HWID 是否已在线（不允许同一 HWID 双重登录）。
    ///   4. 最终校验账户-HWID 关联是否允许。
    /// 全部通过后记录 HWID => 客户端，并将 HWID 加入 onlineRemoteHwids。
    /// </summary>
    public AntiMulticlientResult attemptLoginSession(IClientBase client, Hwid hwid, int accountId, bool routineCheck)
    {
        if (!YamlConfig.config.server.DETERRED_MULTICLIENT)
        {
            client.Hwid = hwid;
            return AntiMulticlientResult.SUCCESS;
        }

        string remoteHost = client.GetSessionRemoteHost();
        InitializationResult initResult = sessionInit.initialize(remoteHost);
        if (initResult != InitializationResult.SUCCESS)
        {
            return initResult.getAntiMulticlientResult();
        }

        try
        {
            if (!loginStorage.registerLogin(accountId))
            {
                return AntiMulticlientResult.MANY_ACCOUNT_ATTEMPTS;
            }
            else if (routineCheck && !attemptAccountAccess(accountId, hwid, routineCheck))
            {
                return AntiMulticlientResult.REMOTE_REACHED_LIMIT;
            }
            else if (onlineRemoteHwids.Contains(hwid))
            {
                return AntiMulticlientResult.REMOTE_LOGGEDIN;
            }
            else if (!attemptAccountAccess(accountId, hwid, routineCheck))
            {
                return AntiMulticlientResult.REMOTE_REACHED_LIMIT;
            }

            client.Hwid = hwid;
            onlineRemoteHwids.Add(hwid);

            return AntiMulticlientResult.SUCCESS;
        }
        finally
        {
            sessionInit.finalize(remoteHost);
        }
    }

    /// <summary>
    /// 执行游戏会话尝试 —— 当角色选择完成后，从登录态切换到游戏态时调用。
    /// 流程：
    ///   1. 从客户端取出登录阶段记录的 HWID（clientHwid）。
    ///   2. 将登录阶段的 HWID 从 onlineRemoteHwids 移除（腾出位置）。
    ///   3. 校验登录 HWID 与新传入的 HWID 是否一致。
    ///   4. 检查该 HWID 是否已被其他游戏会话占用。
    ///   5. 通过后将 HWID 重新加入 onlineRemoteHwids（此时角色已进入游戏），
    ///      同时写入 hostHwidCache 供后续同 IP 快速查询，并持久化 HWID-账户关联。
    /// </summary>
    public AntiMulticlientResult attemptGameSession(ILoginClient client, int accountId, Hwid hwid)
    {
        string remoteHost = client.GetSessionRemoteHost();
        if (!YamlConfig.config.server.DETERRED_MULTICLIENT)
        {
            hostHwidCache.addEntry(remoteHost, hwid);
            hostHwidCache.addEntry(client.RemoteAddress, hwid); // no HWID information on the loggedin newcomer session...
            return AntiMulticlientResult.SUCCESS;
        }

        InitializationResult initResult = sessionInit.initialize(remoteHost);
        if (initResult != InitializationResult.SUCCESS)
        {
            return initResult.getAntiMulticlientResult();
        }

        try
        {
            var clientHwid = client.Hwid; // thanks Paxum for noticing account stuck after PIC failure
            if (clientHwid == null)
            {
                return AntiMulticlientResult.REMOTE_NO_MATCH;
            }

            onlineRemoteHwids.Remove(clientHwid);

            if (!hwid.Equals(clientHwid))
            {
                return AntiMulticlientResult.REMOTE_NO_MATCH;
            }
            else if (onlineRemoteHwids.Contains(hwid))
            {
                return AntiMulticlientResult.REMOTE_LOGGEDIN;
            }

            // assumption: after a SUCCESSFUL login attempt, the incoming client WILL receive a new IoSession from the game server

            // updated session CLIENT_HWID attribute will be set when the player log in the game
            onlineRemoteHwids.Add(hwid);
            hostHwidCache.addEntry(remoteHost, hwid);
            hostHwidCache.addEntry(client.RemoteAddress, hwid);
            associateHwidAccountIfAbsent(hwid, accountId);

            return AntiMulticlientResult.SUCCESS;
        }
        finally
        {
            sessionInit.finalize(remoteHost);
        }
    }

    /// <summary>
    /// 如果 HWID 尚未与该账户绑定，且绑定数未达上限，则将其持久化到数据库。
    /// 这在游戏会话启动时调用，确保每个新设备首次进入游戏时都会记录。
    /// </summary>
    private void associateHwidAccountIfAbsent(Hwid hwid, int accountId)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            List<Hwid> hwids = _sessionDAO.getHwidsForAccount(dbContext, accountId);

            bool containsRemoteHwid = hwids.Any(accountHwid => accountHwid.Equals(hwid));
            if (containsRemoteHwid)
            {
                return;
            }

            if (hwids.Count < YamlConfig.config.server.MAX_ALLOWED_ACCOUNT_HWID)
            {
                var expiry = _hwidAssociationExpiry.getHwidAccountExpiry(0);
                _sessionDAO.registerAccountAccess(dbContext, accountId, hwid, expiry);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to associate hwid {HWID} with account id {AccountId}", hwid, accountId);
        }
    }

    /// <summary>
    /// 通用会话关闭方法，同时覆盖登录态和游戏态两种情况。
    /// 与 closeLoginSession 的区别：
    ///   - closeLoginSession 只处理登录态关闭（不移除 onlineClients 中的游戏会话）。
    ///   - closeSession 通过 HWID 是否为 null 来区分当前是登录态还是游戏态：
    ///     * hwid != null → 游戏态：直接移除 onlineClients 中的条目。
    ///     * hwid == null → 登录态：仅当 matched SessionId 一致时才移除（避免误杀游戏会话）。
    ///   - 支持 immediately 参数：在关闭会话后同步关闭底层 Socket。
    /// </summary>
    /// <param name="client">待关闭的客户端</param>
    /// <param name="immediately">是否立即关闭 Socket</param>
    public void closeSession(ILoginClient? client, bool immediately = false)
    {
        if (client == null)
            return;

        var hwid = client.Hwid;
        client.Hwid = null;
        if (hwid != null)
        {
            onlineRemoteHwids.Remove(hwid);
        }

        if (client.AccountEntity != null)
        {
            bool isGameSession = hwid != null;
            if (isGameSession)
            {
                onlineClients.Remove(client.AccountEntity.Id);
            }
            else
            {
                var loggedClient = onlineClients.GetValueOrDefault(client.AccountEntity.Id);

                // do not remove an online game session here, only login session
                if (loggedClient != null && loggedClient.SessionId == client.SessionId)
                {
                    onlineClients.Remove(client.AccountEntity.Id);
                }
            }
        }

        if (immediately)
        {
            client.CloseSocket();
        }
    }

    /// <summary>
    /// 获取并移除缓存的 HWID。在登录会话阶段，客户端尚未携带 HWID，
    /// 此方法通过 hostHwidCache 从 remoteAddress 反查之前（同一 IP）登录过的 HWID。
    /// 解决同网络下玩家无法登录的问题（见 BHB, resinate 的反馈）。
    /// </summary>
    public Hwid pickLoginSessionHwid(ILoginClient client)
    {
        string remoteHost = client.RemoteAddress;
        // thanks BHB, resinate for noticing players from same network not being able to login
        return hostHwidCache.removeEntryAndGetItsHwid(remoteHost);
    }

    /// <summary>
    /// 清理过期的 HWID 主机缓存条目，定时由外部调用（如计划任务）。
    /// </summary>
    public void clearExpiredHwidHistory()
    {
        hostHwidCache.clearExpired();
    }

    /// <summary>
    /// 清理过期的登录尝试记录，释放 LoginStorage 中的已过期条目。
    /// </summary>
    public void runUpdateLoginHistory()
    {
        loginStorage.clearExpiredAttempts();
    }

    /// <summary>
    /// 输出当前会话状态的调试快照，包含：
    ///   - 所有在线客户端（AccountId 列表）
    ///   - 所有在线 HWID
    ///   - 所有正在登录的远程主机
    /// 用于运维排查多客户端冲突问题。
    /// </summary>
    public void printSessionTrace()
    {
        if (onlineClients.Count > 0)
        {
            var elist = onlineClients.ToList();
            string commaSeparatedClients = string.Join(", ",
                elist
                    .Select(x => x.Key)
                    .OrderBy(x => x)
                    .Select(x => x.ToString()));

            _logger.LogDebug("Current online clients: {Clients}", commaSeparatedClients);
        }

        if (onlineRemoteHwids.Count > 0)
        {
            List<Hwid> hwids = onlineRemoteHwids.OrderBy(x => x.hwid).ToList();

            _logger.LogDebug("Current online HWIDs: {HWIDs}", string.Join(" ", hwids.Select(x => x.hwid)));
        }

        if (loginRemoteHosts.Count > 0)
        {
            var elist = loginRemoteHosts.OrderBy(x => x.Key).ToList();

            _logger.LogDebug("Current login sessions: {0}", string.Join(", ", elist.Select(x => $"({x.Key}, client: {x.Value})")));
        }
    }


}
