using Application.Core.Game.Gameplay;
using Application.Core.Server.life;
using Application.Shared.GameProps;
using Application.Shared.Net;

namespace Application.Core.Channel.Net.Packets
{
    /// <summary>
    /// Buff 相关的封包。
    ///
    /// 客户端的 GIVE_BUFF（本地，自己看自己）和 GIVE_FOREIGN_BUFF（远程，别人看自己）
    /// 是两套不同的结构：
    /// <list type="bullet">
    /// <item>本地：<c>CWvsContext::OnTemporaryStatSet</c> → <c>sub_781D0E</c>（DecodeForLocal），
    /// 每个普通字段固定为 short(值) + int(来源) + int(时长)，见 <see cref="GiveBuff"/>。</item>
    /// <item>远程：<c>CUserPool::OnUserRemotePacket</c> → <c>sub_98385D</c> → <c>sub_788156</c>（DecodeForRemote），
    /// 每个字段的长度、是否携带数据都不一样，见 <see cref="GiveRemoteBuff(int, BuffParameter)"/>。</item>
    /// </list>
    /// 两个包共用同一份 128 位掩码（<see cref="WriteStatMask(OutPacket, IEnumerable{BuffStat})"/>）
    /// 和同一批状态位，但<b>字段顺序是各自独立的</b>：本地包按 <see cref="ClientFieldOrder"/>，
    /// 远程包按 <see cref="ClientRemoteFields"/>。远程包只发其中一部分字段、且每一位的字节数不同。
    ///
    /// <see cref="BuffStat"/> 的值就是客户端掩码位，所以这里不再做任何换算。
    /// </summary>
    internal class BuffPackets
    {
        #region 掩码 / 字段顺序

        /// <summary>
        /// 写 16 字节掩码。客户端的 UINT128 是按 dword 逆序存放的
        /// （<c>UINT128::UINT128(unsigned long)</c> 里 <c>this[3] = value</c>），
        /// 所以服务端 writeLong(firstmask) + writeLong(secondmask) 与客户端位号的对应是：
        /// <code>
        /// firstmask  bit 0..31  ← 客户端位 96..127
        /// firstmask  bit 32..63 ← 客户端位 64..95
        /// secondmask bit 0..31  ← 客户端位 32..63
        /// secondmask bit 32..63 ← 客户端位 0..31
        /// </code>
        /// （例：MONSTER_RIDING = 位 85 → firstmask bit 53，正是 <c>sub_98385D</c> 里
        /// <c>sub_78D977(_, 3) = 1&lt;&lt;85</c> 判定的那一位。）
        /// </summary>
        internal static void WriteStatMask(OutPacket p, IEnumerable<BuffStat> statups)
        {
            long firstmask = 0;
            long secondmask = 0;
            foreach (BuffStat stat in statups)
            {
                SetMaskBit(ref firstmask, ref secondmask, (int)stat);
            }
            p.writeLong(firstmask);
            p.writeLong(secondmask);
        }

        /// <inheritdoc cref="WriteStatMask(OutPacket, IEnumerable{BuffStat})"/>
        internal static void WriteStatMask(OutPacket p, IEnumerable<int> bits)
        {
            long firstmask = 0;
            long secondmask = 0;
            foreach (int bit in bits)
            {
                SetMaskBit(ref firstmask, ref secondmask, bit);
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
        /// 客户端 SecondaryStat 字段顺序，也就是 <c>sub_781D0E</c> 里 82 次 <c>mask &amp; 常量</c> 判断的顺序
        /// （DecodeForLocal 的读取顺序）。远程包 DecodeForRemote 只读取其中一部分字段，
        /// 顺序是这份表的子序列。
        ///
        /// 注意它不是“位号升序”：客户端把后来新增的状态插在了中间，
        /// 例如 GHOST_MORPH(49) 紧跟 MORPH(33)、HPREC/MPREC(57/58) 排在 EXP_BUFF(74) 之后。
        /// </summary>
        public static readonly BuffStat[] ClientFieldOrder =
        [
            BuffStat.WATK, BuffStat.WDEF, BuffStat.MATK, BuffStat.MDEF,
            BuffStat.ACC, BuffStat.AVOID, BuffStat.HANDS, BuffStat.SPEED,
            BuffStat.JUMP, BuffStat.MAGIC_GUARD, BuffStat.DARKSIGHT, BuffStat.BOOSTER,
            BuffStat.POWERGUARD, BuffStat.HYPERBODYHP, BuffStat.HYPERBODYMP, BuffStat.INVINCIBLE,
            BuffStat.SOULARROW, BuffStat.STUN, BuffStat.POISON, BuffStat.SEAL,
            BuffStat.DARKNESS, BuffStat.COMBO, BuffStat.WK_CHARGE, BuffStat.DRAGONBLOOD,
            BuffStat.HOLY_SYMBOL, BuffStat.MESOUP, BuffStat.SHADOWPARTNER, BuffStat.PICKPOCKET,
            BuffStat.MESOGUARD, BuffStat.THAW, BuffStat.WEAKEN, BuffStat.CURSE,
            BuffStat.SLOW, BuffStat.MORPH,
            BuffStat.GHOST_MORPH,
            BuffStat.RECOVERY, BuffStat.MAPLE_WARRIOR, BuffStat.STANCE, BuffStat.SHARP_EYES,
            BuffStat.MANA_REFLECTION, BuffStat.SEDUCE, BuffStat.SHADOW_CLAW, BuffStat.INFINITY,
            BuffStat.HOLY_SHIELD, BuffStat.HAMSTRING, BuffStat.BLIND, BuffStat.CONCENTRATE,
            BuffStat.PUPPET, BuffStat.ECHO_OF_HERO,
            BuffStat.AURA, BuffStat.MAP_CHAIR, BuffStat.CONFUSE, BuffStat.MESO_UP_BY_ITEM,
            BuffStat.COUPON_EXP1, BuffStat.COUPON_EXP2, BuffStat.COUPON_EXP3, BuffStat.COUPON_DRP1, BuffStat.COUPON_DRP2,
            BuffStat.BERSERK_FURY, BuffStat.DIVINE_BODY, BuffStat.SPARK,
            BuffStat.FINALATTACK,
            BuffStat.WINDBREAKERFINAL, BuffStat.ELEMENTAL_RESET, BuffStat.WIND_WALK, BuffStat.EVENTRATE,
            BuffStat.ARAN_COMBO, BuffStat.COMBO_DRAIN, BuffStat.COMBO_BARRIER, BuffStat.BODY_PRESSURE,
            BuffStat.SMART_KNOCKBACK, BuffStat.BERSERK, BuffStat.EXP_BUFF,
            BuffStat.HPREC, BuffStat.MPREC,
            BuffStat.StopPotion, BuffStat.StopMotion, BuffStat.FEAR, BuffStat.EVANSLOW,
            BuffStat.MAGIC_SHIELD, BuffStat.MAGIC_RESISTANCE, BuffStat.SOULSTONE,
        ];

        /// <summary>
        /// 特殊 Buff 的读取顺序：客户端在 <c>sub_788156</c> / <c>sub_781D0E</c> 末尾固定按掩码位 82..88 循环 7 次，
        /// 每一位的字段长度不同（13/15/17/20 字节），见 <see cref="GiveRemoteBuff(int, BuffParameter)"/>。
        /// </summary>
        public static readonly BuffStat[] ClientSpecialFieldOrder =
        [
            BuffStat.ENERGY_CHARGE,
            BuffStat.DASH2,
            BuffStat.DASH,
            BuffStat.MONSTER_RIDING,
            BuffStat.SPEED_INFUSION,
            BuffStat.HOMING_BEACON,
            BuffStat.NOTDAMAGED,
        ];

        #endregion

        #region DecodeForLocal

        public static Packet GiveBuff(BuffParameter ctx)
        {
            OutPacket p = OutPacket.create(SendOpcode.GIVE_BUFF);
            WriteStatMask(p, ctx.AllBuffStats.Keys);

            // 客户端 sub_781D0E 是按固定顺序读取这 82 个普通字段的（见 ClientFieldOrder），
            // 不能按调用方给的 statups 顺序写，否则多状态 buff 会整体错位。
            if (ctx.NormalBuffStats.Count > 0)
            {
                foreach (BuffStat stat in ClientFieldOrder)
                {
                    if (!ctx.NormalBuffStats.TryGetValue(stat, out var value))
                    {
                        continue;
                    }
                    p.writeShort(value);
                    p.writeInt(ctx.SourceId);
                    p.writeInt(ctx.Duration);
                }
            }

            p.writeByte(ctx.DefenseAttChar);
            p.writeByte(ctx.DefenseStateChar);

            ctx.EncodeSpecial(p);

            // delay：客户端 CUser::OnTemporaryStatChanged(mask, tDelay, nPoint) 的 tDelay（0 = 不延迟）
            p.writeShort(0);
            return p;
        }

        #endregion

        #region DecodeForRemote

        /// <summary>远程包字段的读取方式，对应客户端的 CInPacket::DecodeXxx。</summary>
        private enum RemoteFieldType
        {
            /// <summary>只有掩码位，不读任何字节（客户端直接把标志位置 1）。</summary>
            Flag = 0,
            /// <summary>CInPacket::Decode1。</summary>
            Byte = 1,
            /// <summary>CInPacket::Decode2。</summary>
            Short = 2,
            /// <summary>CInPacket::Decode4。</summary>
            Int = 4,
            /// <summary>Decode2 + Decode4（只有 POISON 是这种结构）。</summary>
            ShortThenInt = 6,
        }

        /// <summary>
        /// 远程包会读到的 31 个字段，顺序就是客户端 <c>sub_788156</c> 里 31 次 <c>mask &amp; 常量</c> 的顺序，
        /// 其余状态在远程包里只有掩码位、没有数据（客户端也不会读）。
        ///
        /// <b>这个顺序不是 <see cref="ClientFieldOrder"/> 的子序列</b>：客户端本地包和远程包是两个各自写死顺序的函数，
        /// 远程包开头读的是 SPEED(7)、COMBO(21)、WK_CHARGE(22)、STUN(17)、DARKNESS(20)、SEAL(19)…
        /// 而本地包按状态序 7、8、9… 读。两者只有字段集合是重合的。
        ///
        /// 顺序本身用两个解码函数里引用的同一批掩码常量对照得出（例如远程第 4 个字段写槽位 185 =
        /// <c>0x2E4</c>，正是 <c>CUserLocal::IsStun</c> 读的那一格，对应 STUN）。
        /// </summary>
        private static readonly (BuffStat Stat, RemoteFieldType Type)[] ClientRemoteFields =
        [
            (BuffStat.SPEED, RemoteFieldType.Byte),
            (BuffStat.COMBO, RemoteFieldType.Byte),
            (BuffStat.WK_CHARGE, RemoteFieldType.Int),
            (BuffStat.STUN, RemoteFieldType.Int),
            (BuffStat.DARKNESS, RemoteFieldType.Int),
            (BuffStat.SEAL, RemoteFieldType.Int),
            (BuffStat.WEAKEN, RemoteFieldType.Int),
            (BuffStat.CURSE, RemoteFieldType.Int),
            (BuffStat.POISON, RemoteFieldType.ShortThenInt),   // 2 字节中毒值 + 4 字节来源
            (BuffStat.SHADOWPARTNER, RemoteFieldType.Flag),
            (BuffStat.DARKSIGHT, RemoteFieldType.Flag),
            (BuffStat.SOULARROW, RemoteFieldType.Flag),
            (BuffStat.MORPH, RemoteFieldType.Short),
            (BuffStat.GHOST_MORPH, RemoteFieldType.Short),
            (BuffStat.SEDUCE, RemoteFieldType.Int),
            (BuffStat.SHADOW_CLAW, RemoteFieldType.Int),
            (BuffStat.PUPPET, RemoteFieldType.Int),
            (BuffStat.AURA, RemoteFieldType.Int),
            (BuffStat.MAP_CHAIR, RemoteFieldType.Int),
            (BuffStat.CONFUSE, RemoteFieldType.Int),
            (BuffStat.COUPON_EXP2, RemoteFieldType.Int),
            (BuffStat.COUPON_EXP3, RemoteFieldType.Int),
            (BuffStat.COUPON_DRP1, RemoteFieldType.Int),
            (BuffStat.COUPON_DRP2, RemoteFieldType.Int),
            (BuffStat.BERSERK_FURY, RemoteFieldType.Flag),
            (BuffStat.DIVINE_BODY, RemoteFieldType.Flag),
            (BuffStat.WIND_WALK, RemoteFieldType.Flag),
            (BuffStat.BERSERK, RemoteFieldType.Int),
            (BuffStat.StopPotion, RemoteFieldType.Int),
            (BuffStat.StopMotion, RemoteFieldType.Int),
            (BuffStat.FEAR, RemoteFieldType.Int),
        ];

        public static Packet GiveRemoteBuff(int chrId, BuffParameter ctx)
        {
            OutPacket p = OutPacket.create(SendOpcode.GIVE_FOREIGN_BUFF);
            p.writeInt(chrId);

            WriteStatMask(p, ctx.AllBuffStats.Keys);

            // 顺序必须用远程包自己的顺序（见 ClientRemoteFields）；套用本地包顺序会把字段整体排错
            if (ctx.NormalBuffStats.Count > 0)
            {
                foreach (var (stat, fieldType) in ClientRemoteFields)
                {
                    if (!ctx.NormalBuffStats.TryGetValue(stat, out var value))
                    {
                        continue;
                    }

                    if (DiseaseInfo.IsDisease(stat))
                    {
                        if (stat == BuffStat.POISON)
                        {
                            p.writeShort((int)value);
                        }

                        p.writeInt(ctx.SourceId);
                        continue;
                    }

                    switch (fieldType)
                    {
                        case RemoteFieldType.Byte:
                            p.writeByte(value);
                            break;
                        case RemoteFieldType.Short:
                            p.writeShort(value);
                            break;
                        case RemoteFieldType.Int:
                            p.writeInt(ctx.SourceId);
                            break;
                        case RemoteFieldType.Flag:
                            break;
                    }
                }
            }

            p.writeByte(ctx.DefenseAttChar);
            p.writeByte(ctx.DefenseStateChar);

            ctx.EncodeSpecial(p);

            // delay：客户端 CUser::OnTemporaryStatChanged(mask, tDelay, nPoint) 的 tDelay（0 = 不延迟）
            p.writeShort(0);
            return p;
        }

        /// <summary>
        /// 隐身的buff，用于gm隐身
        /// </summary>
        /// <param name="chrId"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static Packet GiveRemoteHiddenBuff(int chrId)
        {
            OutPacket p = OutPacket.create(SendOpcode.GIVE_FOREIGN_BUFF);
            p.writeInt(chrId);

            WriteStatMask(p, [BuffStat.DARKSIGHT]);
            p.skip(4);
            return p;
        }
        #endregion
    }
}
