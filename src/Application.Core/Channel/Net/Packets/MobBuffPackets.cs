using Application.Core.Game.Life;
using Application.Core.Game.Life.Monsters.TemporaryStat;

namespace Application.Core.Channel.Net.Packets
{
    /// <summary>
    /// Mob buff（<see cref="MonsterStatus"/>）相关的封包。
    /// </summary>
    internal class MobBuffPackets
    {
        #region 掩码 / 字段顺序

        /// <summary>
        internal static void WriteStatMask(OutPacket p, IEnumerable<MonsterStatus> statups)
        {
            long firstmask = 0;
            long secondmask = 0;
            foreach (var stat in statups)
            {
                SetMaskBit(ref firstmask, ref secondmask, (int)stat);
            }
            p.writeLong(firstmask);
            p.writeLong(secondmask);
        }

        private static void SetMaskBit(ref long firstmask, ref long secondmask, int bit)
        {
            if (bit >= 96)
            {
                firstmask |= 1L << (bit - 96);
            }
            else if (bit >= 64)
            {
                firstmask |= 1L << (bit - 32);
            }
            else if (bit >= 32)
            {
                secondmask |= 1L << (bit - 32);
            }
            else if (bit >= 0)
            {
                secondmask |= 1L << (bit + 32);
            }
        }


        /// <summary>
        /// 客户端 <c>MobStat::DecodeTemporary</c>（0x78B0B1）里 <c>mask &amp; dword_BEFxxx</c> 的判断顺序，
        /// 也就是包体 per-status 字段的写出顺序。客户端对每个置位读固定 2+4+2 字节，见 <see cref="EncodeForLocalOne"/>。
        /// <para>
        /// 它不是位号升序：位 18/19（WEAPON_IMMUNITY / MAGIC_IMMUNITY）排在位 16/17（DOOM / SHADOW_WEB）之前。
        /// </para>
        /// <para>
        /// 表外的位：位 20 / 23 / 27 客户端不读 per-status 字段（位 27 读的是 reflection 列表），
        /// 位 31 是视觉位（双 Decode1 → 层 Alpha），位 32..34 又各是一个 per-status 三元组——
        /// 但 <see cref="MonsterStatus"/> 只定义到位 30，这几位服务端永远置不起来，因此本表即完整。
        /// </para>
        /// </summary>
        internal static readonly MonsterStatus[] ClientFieldOrder =
        [
            MonsterStatus.WATK,               // 位 0  dword_BEFAB0
            MonsterStatus.WDEF,               // 位 1  dword_BEFAA0
            MonsterStatus.MATK,               // 位 2  dword_BEFA90（PHANTOM_IMPRINT 共用）
            MonsterStatus.MDEF,               // 位 3  dword_BEFA80
            MonsterStatus.ACC,                // 位 4  dword_BEFA70
            MonsterStatus.AVOID,              // 位 5  dword_BEFA60
            MonsterStatus.SPEED,              // 位 6  dword_BEFA50
            MonsterStatus.STUN,               // 位 7  dword_BEFA40
            MonsterStatus.FREEZE,             // 位 8  dword_BEFA30
            MonsterStatus.POISON,             // 位 9  dword_BEFA20
            MonsterStatus.SEAL,               // 位 10 dword_BEFA10
            MonsterStatus.SHOWDOWN,           // 位 11 dword_BEFA00
            MonsterStatus.WEAPON_ATTACK_UP,   // 位 12 dword_BEF9F0
            MonsterStatus.WEAPON_DEFENSE_UP,  // 位 13 dword_BEF9E0
            MonsterStatus.MAGIC_ATTACK_UP,    // 位 14 dword_BEF9D0
            MonsterStatus.MAGIC_DEFENSE_UP,   // 位 15 dword_BEF9C0
            MonsterStatus.WEAPON_IMMUNITY,    // 位 18 dword_BEF990
            MonsterStatus.MAGIC_IMMUNITY,     // 位 19 dword_BEF980
            MonsterStatus.DOOM,               // 位 16 dword_BEF9B0
            MonsterStatus.SHADOW_WEB,         // 位 17 dword_BEF9A0
            MonsterStatus.HARD_SKIN,          // 位 21 dword_BEF960
            MonsterStatus.NINJA_AMBUSH,       // 位 22 dword_BEF950
            MonsterStatus.VENOMOUS_WEAPON,    // 位 24 dword_BEF930
            MonsterStatus.BLIND,              // 位 25 dword_BEF920
            MonsterStatus.SEAL_SKILL,         // 位 26 dword_BEF910
            MonsterStatus.INERTMOB,           // 位 28 dword_BEF8F0
            MonsterStatus.WEAPON_REFLECT,     // 位 29 dword_BEF8E0
            MonsterStatus.MAGIC_REFLECT,      // 位 30 dword_BEF8D0
            MonsterStatus.Toss,               // 位 32 dword_BEF8B0
            MonsterStatus.NEUTRALISE,         // 位 33 dword_BEF8A0
            MonsterStatus.PHANTOM_IMPRINT,    // 位 34 dword_BEF890
            // 位 27 dword_BEF900
            // 位 31 dword_BEF8C0
        ];

        /// <summary>
        /// 客户端 <c>sub_77DC78</c> 判定的移动相关位：
        /// <c>dword_BEF878 = dword_BEFA50 | dword_BEFA40 | dword_BEFA30 | dword_BEF9B0 | dword_BEF8B0</c>
        /// = 位 6 SPEED | 位 7 STUN | 位 8 FREEZE | 位 16 DOOM | 位 32。
        /// <para>
        /// 命中时 <c>OnStatSet</c> / <c>OnStatReset</c> 会把整个包挂进 <c>CMob+0x530</c> 的 ReservedPacket 队列，
        /// 由 <c>CMob::Update</c> 延后处理；更关键的是 <c>ProcessStatSet</c> / <c>ProcessStatReset</c>
        /// 尾部会因此多读 1 字节（<c>CMovePath::SetStatChangedPoint</c>），
        /// 所以服务端必须补上这一字节，否则客户端越界读会抛 <c>ZException</c> 断开。
        /// </para>
        /// <para>
        /// 位 32 没有对应的 <see cref="MonsterStatus"/>（枚举只定义到位 30），这里省略。
        /// </para>
        /// </summary>
        internal static readonly MonsterStatus[] MovementAffectingStat =
        [
            MonsterStatus.SPEED,
            MonsterStatus.STUN,
            MonsterStatus.FREEZE,
            MonsterStatus.DOOM,
            MonsterStatus.Toss
        ];

        #endregion


        #region 0xF2 / 0xF3

        /// <summary>
        /// <c>APPLY_MONSTER_STATUS (0xF2)</c>，对应 <c>CMob::OnStatSet</c>（0x66C301）→ <c>ProcessStatSet</c>（0x671B71）。
        /// </para>
        /// </summary>
        internal static Packet ApplyMonsterStatus(Monster mob)
        {
            OutPacket p = OutPacket.create(SendOpcode.APPLY_MONSTER_STATUS);
            p.writeInt(mob.getObjectId());

            EncodeTemporary(p, mob.AllBuffs);

            // v11 = CInPacket::Decode2(a6);
            p.writeShort(0);
            // v12 = CInPacket::Decode1(a6);
            p.writeByte(mob.AllBuffs.Count);

            if (MovementAffectingStat.Any(x => mob.AllBuffs.ContainsKey(x)))
            {
                // CMovePath::SetStatChangedPoint
                p.writeByte(mob.getStance());
            }
            return p;
        }

        // MobStat::DecodeTemporary
        public static void EncodeTemporary(OutPacket p, Dictionary<MonsterStatus, MonsterBuffBase> buffs)
        {
            WriteStatMask(p, buffs.Keys);

            foreach (var stat in ClientFieldOrder)
            {
                if (!buffs.TryGetValue(stat, out var data))
                {
                    continue;
                }

                data.EncodeForLocalOne(p);
            }

            bool weaponReflect = buffs.ContainsKey(MonsterStatus.WEAPON_REFLECT);
            bool magicReflect = buffs.ContainsKey(MonsterStatus.MAGIC_REFLECT);

            //   UINT128::operator&(v12, dword_BEF8E0);
            //   if ( UINT128::operator bool() )
            //     this[107] = CInPacket::Decode4(a6);
            if (weaponReflect)
            {
                p.writeInt(0);
            }
            if (magicReflect)
            {
                p.writeInt(0);
            }
            if (weaponReflect || magicReflect)
            {
                p.writeInt(100);
            }
        }

        /// <summary>
        /// <c>CANCEL_MONSTER_STATUS (0xF3)</c>，对应 <c>CMob::OnStatReset</c>（0x66C424）→ <c>ProcessStatReset</c>（0x672111）。
        /// </summary>
        internal static Packet CancelMonsterStatus(int oid, IEnumerable<MonsterStatus> statups)
        {
            OutPacket p = OutPacket.create(SendOpcode.CANCEL_MONSTER_STATUS);
            p.writeInt(oid);
            WriteStatMask(p, statups);
            p.writeByte(0);

            if (MovementAffectingStat.Any(x => statups.Contains(x)))
            {
                // CMovePath::SetStatChangedPoint
                p.writeByte(0);
            }
            return p;
        }

        #endregion
    }
}
