using Application.Core.Server;
using Application.Shared.Battle.Skills;
using Application.Utility.Tickables;

namespace Application.Core.Game.Players.Tickables
{
    public abstract class EffectBase : ILoopTickable, ILifedTickable
    {
        protected Player _chr;
        public BuffStat BuffStat { get; }
        public IBuffSource BuffSource { get; }
        public long StartTime { get; }
        public int Value { get; set; }
        public bool bestApplied;
        public long ExpiredAt { get; }

        public EffectBase(Player chr, BuffStat buffStat, IBuffSource effect, long startTime, long expiredAt, int value)
        {
            _chr = chr;
            BuffStat = buffStat;
            BuffSource = effect;
            this.StartTime = startTime;
            ExpiredAt = expiredAt;
            this.Value = value;

            this.bestApplied = false;

            Period = 1_000;
        }

        public long Next { get; protected set; }

        public long Period { get; init; }

        public TickableStatus Status { get; set; }
        public virtual async Task OnTick(long now)
        {
            if (this.IsAvailable())
            {
                if (ExpiredAt <= now)
                {
                    Status = TickableStatus.Remove;
                    return;
                }

                await Process(now);
                Next = now + Period;
            }
        }

        /// <summary>
        /// 持续触发
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        protected virtual Task Process(long now)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 生效时触发
        /// </summary>
        /// <returns></returns>
        public virtual Task OnMounted()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 失效时触发
        /// </summary>
        /// <returns></returns>
        public virtual Task OnUnmounted()
        {
            return Task.CompletedTask;
        }

        public Packet EncodeForLocalOne(OutPacket p)
        {
            p.writeShort(Value);
            p.writeInt(BuffSource.GetEncodeId());
            p.writeInt((int)Math.Min((ExpiredAt - _chr.Client.CurrentServer.Node.getCurrentTime()), int.MaxValue));
            return p;
        }

        public void EncodeOneForRemote(OutPacket p)
        {
            switch (BuffStat)
            {
                case BuffStat.SPEED:
                case BuffStat.COMBO:
                    p.writeByte(Value);
                    break;
                case BuffStat.MORPH:
                case BuffStat.GHOST_MORPH:
                    p.writeShort(Value);
                    break;
                case BuffStat.WK_CHARGE:
                case BuffStat.STUN:
                case BuffStat.DARKNESS:
                case BuffStat.SEAL:
                case BuffStat.WEAKEN:
                case BuffStat.CURSE:
                case BuffStat.SEDUCE:
                case BuffStat.SHADOW_CLAW:
                case BuffStat.PUPPET:
                case BuffStat.AURA:
                case BuffStat.MAP_CHAIR:
                case BuffStat.CONFUSE:
                case BuffStat.RESPECT_PIMMUNE:
                case BuffStat.RESPECT_MIMMUNE:
                case BuffStat.DEFENSE_ATT:
                case BuffStat.DEFENSE_STATE:
                case BuffStat.BERSERK:
                case BuffStat.StopPotion:
                case BuffStat.StopMotion:
                case BuffStat.FEAR:
                case BuffStat.POISON:
                    if (BuffStat == BuffStat.POISON)
                    {
                        p.writeShort(Value);
                    }

                    p.writeInt(BuffSource.GetEncodeId());
                    break;
                case BuffStat.SHADOWPARTNER:
                case BuffStat.DARKSIGHT:
                case BuffStat.SOULARROW:
                case BuffStat.BERSERK_FURY:
                case BuffStat.DIVINE_BODY:
                case BuffStat.WIND_WALK:
                    break;
            }
        }

        public void EncodeDefense(OutPacket p)
        {
            p.writeByte(BuffStat == BuffStat.DEFENSE_ATT ? (BuffSource as StatEffect)!.DefenseAttChar : 0);
            p.writeByte(BuffStat == BuffStat.DEFENSE_STATE ? (BuffSource as StatEffect)!.DefenseStateChar : 0);
        }

        public virtual void EncodeSpecial(OutPacket p)
        {
            // sub_793EF2：int(值) + int(来源) + sub_77BBF1(byte 标志 + int 时间差)
            p.writeInt(Value);   //MONSTER_RIDING=itemId, HOMING_BEACON=x
            p.writeInt(BuffSource.GetEncodeId());

            // sub_77BBF1 和时间相关
            // v2 = CInPacket::Decode1(a1);
            p.writeByte(0);
            // v3 = CInPacket::Decode4(a1);
            p.writeInt(0);
            // v2 ? v1 - v3 : v1 + v3

            if (BuffStat == BuffStat.SPEED_INFUSION)
            {
                // *(this + 40) = sub_77BBF1(a2);
                p.writeByte(0);
                p.writeInt(0);
            }

            // sub_77C442
            if (BuffStat == BuffStat.HOMING_BEACON)
            {
                // mob object id
                p.writeInt(0);
            }

            if (BuffStat != BuffStat.MONSTER_RIDING && BuffStat != BuffStat.HOMING_BEACON)
            {
                p.writeShort((short)Math.Min((ExpiredAt - _chr.Client.CurrentServer.Node.getCurrentTime()) / 1000, short.MaxValue));
            }
        }
    }
}
