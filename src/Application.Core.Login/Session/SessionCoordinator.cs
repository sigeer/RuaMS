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


using Application.Core.Client;
using Application.Core.Login.Client;
using Application.Core.scripting.npc;
using Application.EF;
using Application.Shared.Sessions;
using Application.Utility.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Core.Login.Session;


/**
 * @author Ronan
 */
public class SessionCoordinator
{
    readonly ILogger<SessionCoordinator> _logger;

    private SessionInitialization sessionInit;
    private LoginStorage loginStorage;
    private Dictionary<int, ILoginClient> onlineClients = new(); // Key: account id
    private HashSet<Hwid> onlineRemoteHwids = new(); // Hwid/nibblehwid
    private ConcurrentDictionary<string, ILoginClient> loginRemoteHosts = new(); // Key: Ip (+ nibblehwid)

    private HostHwidCache hostHwidCache;
    readonly SessionDAO _sessionDAO;
    readonly IDbContextFactory<DBContext> _dbContextFactory;
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

    /**
     * Overwrites any existing online client for the account id, making sure to disconnect it as well.
     */
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
    /// 与closeLoginSession的区别？
    /// </summary>
    /// <param name="client">ChannelClient</param>
    /// <param name="immediately"></param>
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

    public Hwid pickLoginSessionHwid(ILoginClient client)
    {
        string remoteHost = client.RemoteAddress;
        // thanks BHB, resinate for noticing players from same network not being able to login
        return hostHwidCache.removeEntryAndGetItsHwid(remoteHost);
    }

    public void clearExpiredHwidHistory()
    {
        hostHwidCache.clearExpired();
    }

    public void runUpdateLoginHistory()
    {
        loginStorage.clearExpiredAttempts();
    }

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

    public void printSessionTrace(IChannelClient c)
    {
        string str = "Opened server sessions:\r\n\r\n";

        if (onlineClients.Count > 0)
        {
            var elist = onlineClients.OrderBy(x => x.Key).ToList();

            str += ("Current online clients:\r\n");
            foreach (var e in elist)
            {
                str += ("  " + e.Key + "\r\n");
            }
        }

        if (onlineRemoteHwids.Count > 0)
        {
            List<Hwid> hwids = onlineRemoteHwids.OrderBy(x => x.hwid).ToList();

            str += ("Current online HWIDs:\r\n");
            foreach (Hwid s in hwids)
            {
                str += ("  " + s + "\r\n");
            }
        }

        if (loginRemoteHosts.Count > 0)
        {
            var elist = loginRemoteHosts.OrderBy(x => x.Key).ToList();

            str += ("Current login sessions:\r\n");
            foreach (var e in elist)
            {
                str += ("  " + e.Key + ", IP: " + e.Value.RemoteAddress + "\r\n");
            }
        }

        TempConversation.Create(c, NpcId.TEMPLE_KEEPER)?.RegisterTalk(str);
    }
}
