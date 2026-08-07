namespace Application.Shared.Servers
{
    public interface INodeServer
    {
        string ServerHost { get; }
        string ServerName { get; }
        List<ChannelConfig> ChannelConfigs { get; }
    }
}
