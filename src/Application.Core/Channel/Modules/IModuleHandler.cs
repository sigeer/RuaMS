namespace Application.Core.Channel.Modules
{
    public interface IModuleHandler
    {
        void Handler(InPacket p, IChannelClient c);
    }
}
