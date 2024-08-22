using System.Collections.Concurrent;

namespace net.server.coordinator.session;

public class HostHwidCache
{
    private ConcurrentDictionary<string, HostHwid> hostHwidCache = new(); // Key: remoteHost

    public void clearExpired()
    {
        SessionDAO.deleteExpiredHwidAccounts();

        DateTimeOffset now = DateTimeOffset.FromUnixTimeMilliseconds(Server.getInstance().getCurrentTime());
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
            hostHwidCache.Remove(remoteHost);
        }
    }

    public void addEntry(string remoteHost, Hwid hwid)
    {
        hostHwidCache.AddOrUpdate(remoteHost, HostHwid.createWithDefaultExpiry(hwid));
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
