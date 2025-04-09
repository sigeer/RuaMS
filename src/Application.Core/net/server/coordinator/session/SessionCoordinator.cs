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


using Application.Core.Managers;
using Application.Core.scripting.npc;
using constants.id;
using net.server.coordinator.login;
using System.Collections.Concurrent;

namespace net.server.coordinator.session;


/**
 * @author Ronan
 */
public class SessionCoordinator
{
    private static ILogger log = LogFactory.GetLogger(LogType.Session);
    private static SessionCoordinator instance = new SessionCoordinator();

    public static SessionCoordinator getInstance()
    {
        return instance;
    }

    public enum AntiMulticlientResult
    {
        SUCCESS,
        REMOTE_LOGGEDIN,
        REMOTE_REACHED_LIMIT,
        REMOTE_PROCESSING,
        REMOTE_NO_MATCH,
        MANY_ACCOUNT_ATTEMPTS,
        COORDINATOR_ERROR
    }

    private SessionInitialization sessionInit = new SessionInitialization();
    private LoginStorage loginStorage = new LoginStorage();
    private Dictionary<int, IClient> onlineClients = new(); // Key: account id
    private HashSet<Hwid> onlineRemoteHwids = new(); // Hwid/nibblehwid
    private ConcurrentDictionary<string, IClient> loginRemoteHosts = new(); // Key: Ip (+ nibblehwid)
    private HostHwidCache hostHwidCache = new HostHwidCache();

    private SessionCoordinator()
    {
    }

    private static bool attemptAccountAccess(int accountId, Hwid hwid, bool routineCheck)
    {
        try
        {
            using var dbContext = new DBContext();
            List<HwidRelevance> hwidRelevances = SessionDAO.getHwidRelevance(dbContext, accountId);
            foreach (HwidRelevance hwidRelevance in hwidRelevances)
            {
                if (hwidRelevance.hwid.EndsWith(hwid.hwid))
                {
                    if (!routineCheck)
                    {
                        // better update HWID relevance as soon as the login is authenticated
                        var expiry = HwidAssociationExpiry.getHwidAccountExpiry(hwidRelevance.relevance);
                        SessionDAO.updateAccountAccess(dbContext, hwid, accountId, expiry, hwidRelevance.getIncrementedRelevance());
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
            log.Warning(e, "Failed to update account access. Account id: {AccountId}, nibbleHwid: {HWID}", accountId, hwid);
        }

        return false;
    }

    public static string getSessionRemoteHost(IClient client)
    {
        var hwid = client.getHwid();

        if (hwid != null)
        {
            return client.getRemoteAddress() + "-" + hwid.hwid;
        }
        else
        {
            return client.getRemoteAddress();
        }
    }

    /**
     * Overwrites any existing online client for the account id, making sure to disconnect it as well.
     */
    public void updateOnlineClient(IClient client)
    {
        if (client != null)
        {
            int accountId = client.getAccID();
            disconnectClientIfOnline(accountId);
            onlineClients.AddOrUpdate(accountId, client);
        }
    }

    private void disconnectClientIfOnline(int accountId)
    {
        var ingameClient = onlineClients.GetValueOrDefault(accountId);
        if (ingameClient != null)
        {     // thanks MedicOP for finding out a loss of loggedin account uniqueness when using the CMS "Unstuck" feature
            ingameClient.forceDisconnect();
        }
    }

    public bool canStartLoginSession(IClient client)
    {
        if (!YamlConfig.config.server.DETERRED_MULTICLIENT)
        {
            return true;
        }

        string remoteHost = getSessionRemoteHost(client);
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

            loginRemoteHosts.AddOrUpdate(remoteHost, client);
            return true;
        }
        finally
        {
            sessionInit.finalize(remoteHost);
        }
    }

    public void closeLoginSession(IClient client)
    {
        clearLoginRemoteHost(client);

        Hwid? nibbleHwid = client.getHwid();
        client.setHwid(null);
        if (nibbleHwid != null)
        {
            onlineRemoteHwids.Remove(nibbleHwid);

            if (client != null)
            {
                var loggedClient = onlineClients.GetValueOrDefault(client.getAccID());

                // do not remove an online game session here, only login session
                if (loggedClient != null && loggedClient.getSessionId() == client.getSessionId())
                {
                    onlineClients.Remove(client.getAccID());
                }
            }
        }
    }

    private void clearLoginRemoteHost(IClient client)
    {
        string remoteHost = getSessionRemoteHost(client);
        loginRemoteHosts.Remove(client.getRemoteAddress());
        loginRemoteHosts.Remove(remoteHost);
    }

    public AntiMulticlientResult attemptLoginSession(IClient client, Hwid hwid, int accountId, bool routineCheck)
    {
        if (!YamlConfig.config.server.DETERRED_MULTICLIENT)
        {
            client.setHwid(hwid);
            return AntiMulticlientResult.SUCCESS;
        }

        string remoteHost = getSessionRemoteHost(client);
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

            client.setHwid(hwid);
            onlineRemoteHwids.Add(hwid);

            return AntiMulticlientResult.SUCCESS;
        }
        finally
        {
            sessionInit.finalize(remoteHost);
        }
    }

    public AntiMulticlientResult attemptGameSession(IClient client, int accountId, Hwid hwid)
    {
        string remoteHost = getSessionRemoteHost(client);
        if (!YamlConfig.config.server.DETERRED_MULTICLIENT)
        {
            hostHwidCache.addEntry(remoteHost, hwid);
            hostHwidCache.addEntry(client.getRemoteAddress(), hwid); // no HWID information on the loggedin newcomer session...
            return AntiMulticlientResult.SUCCESS;
        }

        InitializationResult initResult = sessionInit.initialize(remoteHost);
        if (initResult != InitializationResult.SUCCESS)
        {
            return initResult.getAntiMulticlientResult();
        }

        try
        {
            var clientHwid = client.getHwid(); // thanks Paxum for noticing account stuck after PIC failure
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
            hostHwidCache.addEntry(client.getRemoteAddress(), hwid);
            associateHwidAccountIfAbsent(hwid, accountId);

            return AntiMulticlientResult.SUCCESS;
        }
        finally
        {
            sessionInit.finalize(remoteHost);
        }
    }

    private static void associateHwidAccountIfAbsent(Hwid hwid, int accountId)
    {
        try
        {
            using var dbContext = new DBContext();
            List<Hwid> hwids = SessionDAO.getHwidsForAccount(dbContext, accountId);

            bool containsRemoteHwid = hwids.Any(accountHwid => accountHwid.Equals(hwid));
            if (containsRemoteHwid)
            {
                return;
            }

            if (hwids.Count < YamlConfig.config.server.MAX_ALLOWED_ACCOUNT_HWID)
            {
                var expiry = HwidAssociationExpiry.getHwidAccountExpiry(0);
                SessionDAO.registerAccountAccess(dbContext, accountId, hwid, expiry);
            }
        }
        catch (Exception ex)
        {
            log.Warning(ex, "Failed to associate hwid {HWID} with account id {AccountId}", hwid, accountId);
        }
    }

    private static IClient? fetchInTransitionSessionClient(IClient? client)
    {
        Hwid hwid = SessionCoordinator.getInstance().getGameSessionHwid(client);
        if (hwid == null)
        {
            // maybe this session was currently in-transition?
            return null;
        }

        var fakeClient = Client.createMock();
        fakeClient.setHwid(hwid);
        var chrId = Server.getInstance().freeCharacteridInTransition(client);
        if (chrId != null)
        {
            try
            {
                fakeClient.setAccID(CharacterManager.LoadPlayerFromDB(chrId.Value, client, false).getAccountID());
            }
            catch (Exception sqle)
            {
                log.Error(sqle.ToString());
            }
        }

        return fakeClient;
    }

    public void closeSession(IClient? client, bool immediately = false)
    {
        if (client == null)
        {
            client = fetchInTransitionSessionClient(client);
        }
        if (client == null)
            return;

        var hwid = client.getHwid();
        client.setHwid(null); // making sure to clean up calls to this function on login phase
        if (hwid != null)
        {
            onlineRemoteHwids.Remove(hwid);
        }

        bool isGameSession = hwid != null;
        if (isGameSession)
        {
            onlineClients.Remove(client.getAccID());
        }
        else
        {
            var loggedClient = onlineClients.GetValueOrDefault(client.getAccID());

            // do not remove an online game session here, only login session
            if (loggedClient != null && loggedClient.getSessionId() == client.getSessionId())
            {
                onlineClients.Remove(client.getAccID());
            }
        }

        if (immediately)
        {
            client.closeSession();
        }
    }

    public Hwid pickLoginSessionHwid(IClient client)
    {
        string remoteHost = client.getRemoteAddress();
        // thanks BHB, resinate for noticing players from same network not being able to login
        return hostHwidCache.removeEntryAndGetItsHwid(remoteHost);
    }

    public Hwid getGameSessionHwid(IClient? client)
    {
        string remoteHost = getSessionRemoteHost(client);
        return hostHwidCache.getEntryHwid(remoteHost);
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

            log.Debug("Current online clients: {Clients}", commaSeparatedClients);
        }

        if (onlineRemoteHwids.Count > 0)
        {
            List<Hwid> hwids = onlineRemoteHwids.OrderBy(x => x.hwid).ToList();

            log.Debug("Current online HWIDs: {HWIDs}", string.Join(" ", hwids.Select(x => x.hwid)));
        }

        if (loginRemoteHosts.Count > 0)
        {
            var elist = loginRemoteHosts.OrderBy(x => x.Key).ToList();

            log.Debug("Current login sessions: {0}", string.Join(", ", elist.Select(x => $"({x.Key}, client: {x.Value})")));
        }
    }

    public void printSessionTrace(IClient c)
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
                str += ("  " + e.Key + ", IP: " + e.Value.getRemoteAddress() + "\r\n");
            }
        }

        TempConversation.Create(c, NpcId.TEMPLE_KEEPER)?.RegisterTalk(str);
    }
}
