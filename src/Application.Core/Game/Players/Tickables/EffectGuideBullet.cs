using Application.Core.Server;

namespace Application.Core.Game.Players.Tickables
{
    public class EffectGuideBullet : EffectBuff
    {
        public int MobOId { get; set; }
        public EffectGuideBullet(Player chr, BuffStat buffStat, StatEffect effect, long startTime, long expiredAt, int value)
            : base(chr, buffStat, effect, startTime, expiredAt, value)
        {
        }

        public override void EncodeSpecial(OutPacket p)
        {
            // sub_793EF2：int(值) + int(来源) + sub_77BBF1(byte 标志 + int 时间差)
            p.writeInt(Value);   //MONSTER_RIDING=itemId, HOMING_BEACON=x
            p.writeInt(Effect.GetEncodeId());

            // sub_77BBF1 和时间相关
            // v2 = CInPacket::Decode1(a1);
            p.writeByte(0);
            // v3 = CInPacket::Decode4(a1);
            p.writeInt(0);
            // v2 ? v1 - v3 : v1 + v3

            // mob object id
            p.writeInt(MobOId);
        }
    }
}
