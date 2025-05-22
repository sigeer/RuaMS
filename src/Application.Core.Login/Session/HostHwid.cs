using net.server.coordinator.session;

namespace Application.Core.Login.Session;

public record HostHwid(Hwid hwid, DateTimeOffset expiry);
