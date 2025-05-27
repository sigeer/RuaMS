using Application.Core.Session;

namespace Application.Core.Login.Session;

public record HostHwid(Hwid hwid, DateTimeOffset expiry);
