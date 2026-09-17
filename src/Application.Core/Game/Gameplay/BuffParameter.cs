using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Skills;
using Application.Core.Server;
using Application.Core.Server.life;
using Application.Shared.Battle.Skills;
using OpenTelemetry.Resources;
using System.Data.Common;
using static Application.Core.Server.partyquest.CarnivalFactory;

namespace Application.Core.Game.Gameplay
{
    /// <summary>
    /// 就算是同一个StatEffect，也可能会因为使用场景、随机等变量导致效果不同
    /// </summary>
    public record BuffParameter(int SourceId, int Duration,
        Dictionary<BuffStat, int> AllBuffStats, Dictionary<BuffStat, int> NormalBuffStats, Dictionary<BuffStat, int> SpecialBuffStats,
        byte DefenseAttChar, byte DefenseStateChar,
        int ExtraValue0)
    {

        public void EncodeSpecial(OutPacket p)
        {
            if (SpecialBuffStats.Count > 0)
            {
                foreach (BuffStat stat in BuffPackets.ClientSpecialFieldOrder)
                {
                    if (!SpecialBuffStats.TryGetValue(stat, out var value))
                    {
                        continue;
                    }

                    // sub_793EF2：int(值) + int(来源) + sub_77BBF1(byte 标志 + int 时间差)
                    p.writeInt(value);   //MONSTER_RIDING=itemId, HOMING_BEACON=x
                    p.writeInt(SourceId);

                    // sub_77BBF1 和时间相关
                    // v2 = CInPacket::Decode1(a1);
                    p.writeByte(0);
                    // v3 = CInPacket::Decode4(a1);
                    p.writeInt(0);
                    // v2 ? v1 - v3 : v1 + v3

                    if (stat == BuffStat.SPEED_INFUSION)
                    {
                        // *(this + 40) = sub_77BBF1(a2);
                        p.writeByte(0);
                        p.writeInt(0);
                    }

                    // sub_77C442
                    if (stat == BuffStat.HOMING_BEACON)
                    {
                        // mob object id
                        p.writeInt(ExtraValue0);
                    }

                    if (stat != BuffStat.MONSTER_RIDING && stat != BuffStat.HOMING_BEACON)
                    {
                        p.writeShort(Duration / 1000);
                    }
                }
            }
        }
    }

    public class BuffParameterBuilder
    {
        public BuffParameterBuilder(StatEffect source)
        {
            SetBuffStats = new();
            Source = source;

            SourceId = source.getSourceId();
            Duration = source.getBuffLocalDuration();
        }

        public BuffParameterBuilder(MobSkill mobSkill)
        {
            SetBuffStats = new();
            Source = mobSkill;

            SourceId = mobSkill.getId().getEncodedId();
            Duration = (int)mobSkill.getDuration();
        }

        public BuffParameterBuilder(MCSkill mcSkill): this(mcSkill.getSkill())
        {
        }

        public BuffParameter Build(Player chr)
        {
            byte DefenseAttChar = 0;
            byte DefenseStateChar = 0;
            Dictionary<BuffStat, int> _allBuffStats = new();
            if (AllBuffStats == null)
            {
                if (Source is StatEffect chrEffect)
                {
                    _allBuffStats = chrEffect.getStatups().ToDictionary(x => x.BuffState, x => x.Value);
                    DefenseAttChar = (byte)chrEffect.DefenseAttChar;
                    DefenseStateChar = (byte)chrEffect.DefenseStateChar;
                }
                else if (Source is MobSkill mobSkill)
                {
                    _allBuffStats[DiseaseInfo.GetBySkillTrust(mobSkill.getId().type)] = mobSkill.getX();
                }
            }

            foreach (var kw in SetBuffStats)
            {
                _allBuffStats[kw.Key] = kw.Value;
                chr.setBuffedValue(kw.Key, kw.Value);
            }

            Dictionary<BuffStat, int> normalBuffStats = [];
            Dictionary<BuffStat, int> specialBuffStats = [];
            foreach (var item in _allBuffStats)
            {
                if (Array.IndexOf(BuffPackets.ClientSpecialFieldOrder, item.Key) != -1)
                {
                    specialBuffStats[item.Key] = item.Value;
                }
                else
                {
                    normalBuffStats[item.Key] = item.Value;
                }
            }

            return new BuffParameter(
                SourceId,
                Duration,
                _allBuffStats,
                normalBuffStats,
                specialBuffStats,
                DefenseAttChar,
                DefenseStateChar,
                ExtraValue0
                );
        }
        ISkillEffect Source;
        public int SourceId { get; set; }
        public int Duration { get; set; }
        public Dictionary<BuffStat, int>? AllBuffStats { get; set; }
        public int ExtraValue0 { get; set; }

        public Dictionary<BuffStat, int> SetBuffStats { get; }
    }
}
