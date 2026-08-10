using DotNetty.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Net.Packets
{
    /// <summary>
    /// CField::OnFieldEffect (138)
    /// </summary>
    public class FieldEffectPacket
    {
        public static Packet Summon(int summonType, int x, int y)
        {
            OutPacket p = OutPacket.create(SendOpcode.FIELD_EFFECT);
            p.writeByte(0);
            p.writeInt(summonType);     // SP_1590_EFFECT_SUMMONIMG_D
            p.writeInt(x);
            p.writeInt(y);
            return p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isHeavyAndShort">(0:Light&long 1:Heavy&short)</param>
        /// <param name="delay">seconds</param>
        /// <returns></returns>
        public static Packet Tremble(bool isHeavyAndShort, int delay)
        {
            OutPacket p = OutPacket.create(SendOpcode.FIELD_EFFECT);
            p.writeByte(1);
            p.writeBool(isHeavyAndShort);
            p.writeInt(delay);
            return p;
        }

        public static OutPacket Object(string name)
        {
            OutPacket p = OutPacket.create(SendOpcode.FIELD_EFFECT);
            p.writeByte(2);
            p.writeString(name);    // CMapLoadable::SetObjectState(v44, -1);
            return p;
        }

        public static OutPacket Screen(string path)
        {
            OutPacket p = OutPacket.create(SendOpcode.FIELD_EFFECT);
            p.writeByte(3);
            p.writeString(path);    // CField::ShowScreenEffect(v38);
            return p;
        }

        public static OutPacket Sound(string path)
        {
            OutPacket p = OutPacket.create(SendOpcode.FIELD_EFFECT);
            p.writeByte(4);
            p.writeString(path);    // play_field_sound(v46, 0x64u);
            return p;
        }

        public static Packet ShowBossHP(int oid, int currHP, int maxHP, byte tagColor, byte tagBgColor)
        {
            OutPacket p = OutPacket.create(SendOpcode.FIELD_EFFECT);
            p.writeByte(5);
            p.writeInt(oid);
            p.writeInt(currHP);
            p.writeInt(maxHP);
            p.writeByte(tagColor);
            p.writeByte(tagBgColor);
            return p;
        }

        public static OutPacket Bgm(string path)
        {
            OutPacket p = OutPacket.create(SendOpcode.FIELD_EFFECT);
            p.writeByte(6);
            p.writeString(path);    // SP_1534_SOUND_  , CSoundMan::PlayBGM(a2, 1, 600, 0, 0);
            return p;
        }

        public static OutPacket RewardRullet(int i1, int i2, int i3)
        {
            OutPacket p = OutPacket.create(SendOpcode.FIELD_EFFECT);
            p.writeByte(7);
            p.writeInt(i1);
            p.writeInt(i2);
            p.writeInt(i3);    // CAnimationDisplayer::Effect_RewardRullet(v25, v26, v29, v38);
            return p;
        }
    }
}
