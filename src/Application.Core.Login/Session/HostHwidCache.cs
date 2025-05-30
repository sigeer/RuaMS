using System.Collections.Concurrent;

namespace Application.Core.Login.Session;

public class HostHwidCache
{
    private ConcurrentDictionary<string, HostHwid> hostHwidCache = new(); // Key: remoteHost

    readonly MasterServer _server;
    readonly SessionDAO _sessionDAO;

    public HostHwidCache(MasterServer server, SessionDAO sessionDAO)
    {
        _server = server;
        _sessionDAO = sessionDAO;
    }

    public void clearExpired()
    {
        _sessionDAO.deleteExpiredHwidAccounts();

        DateTimeOffset now = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime());
        List<string> remoteHostsToRemove = new();
        foreach (var entry in hostHwidCache)
        {
            if (now > entry.Value.expiry)
            {
                remoteHostsToRemove.Add(entry.Key);
            }
        }

        foreach (string remoteHost in remoteHostsToRemove)
        {
            hostHwidCache.Remove(remoteHost, out _);
        }
    }

    public void addEntry(string remoteHost, Hwid hwid)
    {
        hostHwidCache[remoteHost] = new HostHwid(hwid, DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime()).AddDays(7));
    }

    public HostHwid? getEntry(string remoteHost)
    {
        return hostHwidCache.GetValueOrDefault(remoteHost);
    }

    public Hwid? removeEntryAndGetItsHwid(string remoteHost)
    {
        if (hostHwidCache.Remove(remoteHost, out var hostHwid))
            return hostHwid?.hwid;
        return null;
    }

    public Hwid? getEntryHwid(string remoteHost)
    {
        return hostHwidCache.GetValueOrDefault(remoteHost)?.hwid;
    }

}
