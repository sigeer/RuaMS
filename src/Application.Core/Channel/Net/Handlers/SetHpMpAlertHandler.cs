namespace Application.Core.Channel.Net.Handlers
{
    public class SetHpMpAlertHandler : ChannelHandlerBase
    {
        public override void HandlePacket(InPacket p, IChannelClient c)
        {
            c.OnlinedCharacter.HpAlert = p.readByte();
            c.OnlinedCharacter.MpAlert = p.readByte();
        }
    }
}
