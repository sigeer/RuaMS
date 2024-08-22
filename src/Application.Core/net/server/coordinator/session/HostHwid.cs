namespace net.server.coordinator.session;

public record HostHwid(Hwid hwid, DateTimeOffset expiry)
{
    public static HostHwid createWithDefaultExpiry(Hwid hwid)
    {
        return new HostHwid(hwid, getDefaultExpiry());
    }

    public static DateTimeOffset getDefaultExpiry()
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(Server.getInstance().getCurrentTime()).AddDays(7);
    }
}
