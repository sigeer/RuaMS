using Application.Core.Game.Players.Tickables;
using System.Runtime.ConstrainedExecution;

namespace Application.Core.Channel.Net.Packets
{
    /// <summary>
    /// Buff 相关的封包。
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
            BuffStat.ITEM_UP_BY_ITEM, BuffStat.RESPECT_PIMMUNE, BuffStat.RESPECT_MIMMUNE, BuffStat.DEFENSE_ATT, BuffStat.DEFENSE_STATE,
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

        /// <summary>
        /// 客户端 <c>sub_77DC78</c> 判定的12个buff位：当这些buff中任何一个在掩码中置位时，
        /// 客户端在包尾额外读1字节（<c>SetSecondaryStatChangedPoint</c>）。
        /// <para>
        /// 这12位来自 <c>sub_77DC78</c> 内 <c>sub_873F63</c> 的或链：
        /// 种子是 <c>dword_BF0000</c>(bit 7)，再或上 <c>dword_BEFF50..dword_BEFFF0</c>。
        /// 注意 bit 7 = SPEED，早期文档漏掉了它，导致隐身术10级(speed=-12)因少发1字节而闪退。
        /// </para>
        /// </summary>
        public static readonly BuffStat[] MovementAffectingStat =
        {
            BuffStat.SPEED,         // bit 7
            BuffStat.JUMP,          // bit 8
            BuffStat.STUN,          // bit 17
            BuffStat.WEAKEN,        // bit 30
            BuffStat.SLOW,          // bit 32
            BuffStat.MORPH,         // bit 33
            BuffStat.MAPLE_WARRIOR, // bit 35
            BuffStat.GHOST_MORPH,   // bit 49
            BuffStat.SEDUCE,        // bit 39
            BuffStat.MONSTER_RIDING,// bit 85
            BuffStat.DASH2,         // bit 83
            BuffStat.DASH           // bit 84
        };

        /// <summary>
        /// 客户端 <c>sub_7938D6</c> 判定的12个buff位：当这些buff中任何一个在掩码中置位时，
        /// 客户端向服务端发送 opcode=0x6C 的统计更新确认包。
        /// 在 <c>OnTemporaryStatSet</c>（0xA206C7）和 <c>OnTemporaryStatReset</c>（0xA208BA）中调用。
        /// <para>
        /// 与 <see cref="MovementAffectingStat"/> 同理：或链的种子走 ECX(<c>dword_BEFF40</c> = bit 0)，
        /// 反编译视图看不到，须核对汇编码里 <c>mov ecx, offset dword_BEFF40</c>。
        /// </para>
        /// </summary>
        public static readonly BuffStat[] StatUpdateRequiredBuffStats =
        {
            BuffStat.WATK,          // bit 0
            BuffStat.MATK,          // bit 2
            BuffStat.ACC,           // bit 4
            BuffStat.AVOID,         // bit 5
            BuffStat.DARKNESS,      // bit 20
            BuffStat.COMBO,         // bit 21
            BuffStat.WK_CHARGE,     // bit 22
            BuffStat.MAPLE_WARRIOR, // bit 35
            BuffStat.SHARP_EYES,    // bit 37
            BuffStat.ECHO_OF_HERO,  // bit 47
            BuffStat.ENERGY_CHARGE, // bit 82
            BuffStat.ARAN_COMBO     // bit 68
        };

        #endregion

        #region EncodeForLocal


        public static Packet GiveBuff(Player chr, Dictionary<BuffStat, EffectBuff> buffs)
        {
            OutPacket p = OutPacket.create(SendOpcode.GIVE_BUFF);

            EncodeForLocal(p, buffs);

            // delay：(*(v19 + 32))(SkillLevel, v31, v32, v33, v34, v35, v36[0]); CUserLocal::OnTemporaryStatChanged
            // 似乎未被使用
            p.writeShort(0);

            // if ( sub_77DC78(v33) )
            if (MovementAffectingStat.Any(x => buffs.ContainsKey(x)))
            {
                // CUserLocal::SetSecondaryStatChangedPoint(v20, v21);
                p.writeByte(chr.getStance());
            }
            return p;
        }

        public static void EncodeForLocal(OutPacket p, Dictionary<BuffStat, EffectBuff> buffs)
        {
            BuffPackets.WriteStatMask(p, buffs.Keys);

            foreach (BuffStat stat in BuffPackets.ClientFieldOrder)
            {
                if (!buffs.TryGetValue(stat, out var data))
                {
                    continue;
                }

                data.EncodeForLocalOne(p);
            }

            p.writeByte(buffs.GetValueOrDefault(BuffStat.DEFENSE_ATT)?.Effect?.DefenseAttChar ?? 0);
            p.writeByte(buffs.GetValueOrDefault(BuffStat.DEFENSE_STATE)?.Effect?.DefenseStateChar ?? 0);

            foreach (BuffStat stat in BuffPackets.ClientSpecialFieldOrder)
            {
                if (!buffs.TryGetValue(stat, out var data))
                {
                    continue;
                }

                data.EncodeSpecial(p);
            }
        }

        /// <summary>
        /// <c>CWvsContext::OnTemporaryStatSet</c> → <c>sub_781D0E</c>（DecodeForLocal），
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="buff"></param>
        /// <returns></returns>
        public static Packet GiveBuff(Player chr, EffectBase buff)
        {
            OutPacket p = OutPacket.create(SendOpcode.GIVE_BUFF);

            EncodeForLocal(p, buff);

            // delay：(*(v19 + 32))(SkillLevel, v31, v32, v33, v34, v35, v36[0]); CUserLocal::OnTemporaryStatChanged
            // 似乎未被使用
            p.writeShort(0);

            // if ( sub_77DC78(v33) )
            if (MovementAffectingStat.Any(x => x == buff.BuffStat))
            {
                // CUserLocal::SetSecondaryStatChangedPoint(v20, v21);
                p.writeByte(chr.getStance());
            }
            return p;
        }
        public static void EncodeForLocal(OutPacket p, EffectBase buff)
        {
            WriteStatMask(p, [buff.BuffStat]);

            if (Array.IndexOf(ClientFieldOrder, buff.BuffStat) != -1)
            {
                buff.EncodeForLocalOne(p);
            }

            buff.EncodeDefense(p);

            if (Array.IndexOf(ClientSpecialFieldOrder, buff.BuffStat) != -1)
            {
                buff.EncodeSpecial(p);
            }
        }

        public static Packet CancelBuff(Player chr, IEnumerable<BuffStat> statups)
        {
            OutPacket p = OutPacket.create(SendOpcode.CANCEL_BUFF);
            WriteStatMask(p, statups);

            // if ( sub_77DC78() )
            if (MovementAffectingStat.Any(x => statups.Contains(x)))
            {
                // CUserLocal::SetSecondaryStatChangedPoint(v20, v21);
                p.writeByte(chr.getStance());
            }
            return p;
        }
        #endregion

        #region EncodeForRemote

        /// <summary>
        /// 远程包会读到的 31 个字段，顺序就是客户端 <c>sub_788156</c> 里 31 次 <c>mask &amp; 常量</c> 的顺序，
        /// 其余状态在远程包里只有掩码位、没有数据（客户端也不会读）。
        /// </list>
        /// </summary>
        public static readonly BuffStat[] ClientRemoteFields =
        [
            BuffStat.SPEED,
            BuffStat.COMBO,
            BuffStat.WK_CHARGE,
            BuffStat.STUN,
            BuffStat.DARKNESS,
            BuffStat.SEAL,
            BuffStat.WEAKEN,
            BuffStat.CURSE,
            BuffStat.POISON,
            BuffStat.SHADOWPARTNER,
            BuffStat.DARKSIGHT,
            BuffStat.SOULARROW,
            BuffStat.MORPH,
            BuffStat.GHOST_MORPH,
            BuffStat.SEDUCE,
            BuffStat.SHADOW_CLAW,
            BuffStat.PUPPET,
            BuffStat.AURA,
            BuffStat.MAP_CHAIR,
            BuffStat.CONFUSE,
            BuffStat.RESPECT_PIMMUNE,
            BuffStat.RESPECT_MIMMUNE,
            BuffStat.DEFENSE_ATT,
            BuffStat.DEFENSE_STATE,
            BuffStat.BERSERK_FURY,
            BuffStat.DIVINE_BODY,
            BuffStat.WIND_WALK,
            BuffStat.BERSERK,
            BuffStat.StopPotion,
            BuffStat.StopMotion,
            BuffStat.FEAR,
        ];



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
            p.skip(4);  // defense * 2 + delay
            return p;
        }

        public static Packet? GiveRemoteBuff(int chrId, Dictionary<BuffStat, EffectBuff> allEffects)
        {
            if (ClientRemoteFields.All(item => !allEffects.ContainsKey(item)) 
                && ClientSpecialFieldOrder.All(item => !allEffects.ContainsKey(item)))
            {
                return null;
            }

            OutPacket p = OutPacket.create(SendOpcode.GIVE_FOREIGN_BUFF);
            p.writeInt(chrId);

            EncodeForRemote(p, allEffects);

            // delay：(*(v7 + 32))(this, v13[0], v13[1], v13[2], v13[3], v14, v15); -> CUserLocal::OnTemporaryStatChanged
            p.writeShort(0);
            return p;
        }

        public static Packet? GiveRemoteBuff(int chrId, EffectBase buff)
        {
            if (!ClientRemoteFields.Contains(buff.BuffStat) && !ClientSpecialFieldOrder.Contains(buff.BuffStat))
            {
                return null;
            }

            OutPacket p = OutPacket.create(SendOpcode.GIVE_FOREIGN_BUFF);
            p.writeInt(chrId);

            EncodeForRemote(p, buff);

            // delay：(*(v7 + 32))(this, v13[0], v13[1], v13[2], v13[3], v14, v15); -> CUserLocal::OnTemporaryStatChanged
            p.writeShort(0);
            return p;
        }

        public static void EncodeForRemote(OutPacket p, Dictionary<BuffStat, EffectBuff> buffs)
        {
            BuffPackets.WriteStatMask(p, buffs.Keys);

            foreach (BuffStat stat in BuffPackets.ClientRemoteFields)
            {
                if (!buffs.TryGetValue(stat, out var data))
                {
                    continue;
                }

                data.EncodeOneForRemote(p);
            }

            p.writeByte(buffs.GetValueOrDefault(BuffStat.DEFENSE_ATT)?.Effect?.DefenseAttChar ?? 0);
            p.writeByte(buffs.GetValueOrDefault(BuffStat.DEFENSE_STATE)?.Effect?.DefenseStateChar ?? 0);

            foreach (BuffStat stat in BuffPackets.ClientSpecialFieldOrder)
            {
                if (!buffs.TryGetValue(stat, out var data))
                {
                    continue;
                }

                data.EncodeSpecial(p);
            }
        }

        /// <summary>
        /// <c>CUserPool::OnUserRemotePacket</c> → <c>sub_98385D</c> → <c>sub_788156</c>（DecodeForRemote）
        /// </summary>
        /// <param name="p"></param>
        /// <param name="buff"></param>
        public static void EncodeForRemote(OutPacket p, EffectBase buff)
        {
            BuffPackets.WriteStatMask(p, [buff.BuffStat]);

            if (Array.IndexOf(ClientRemoteFields, buff.BuffStat) != -1)
            {
                buff.EncodeOneForRemote(p);
            }

            buff.EncodeDefense(p);

            if (Array.IndexOf(ClientSpecialFieldOrder, buff.BuffStat) != -1)
            {
                buff.EncodeSpecial(p);
            }
        }

        public static Packet CancelRemoteBuff(int chrId, IEnumerable<BuffStat> statups)
        {
            OutPacket p = OutPacket.create(SendOpcode.CANCEL_FOREIGN_BUFF);
            p.writeInt(chrId);
            WriteStatMask(p, statups);
            return p;
        }
        #endregion
    }
}
