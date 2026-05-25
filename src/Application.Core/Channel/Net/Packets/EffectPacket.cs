using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Net.Packets
{
    // CUser::OnEffect
    public static class EffectPacket
    {
        // SP_2310
        public static Packet LevelUp()
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(0);
            return p;
        }

        public static Packet Skill()
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeInt(0);
            p.writeByte(0);
            p.writeByte(0);
            p.writeByte(0);
            return p;
        }

        public static Packet Item(int itemId, int quantity)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(3);
            p.writeByte(1);
            p.writeInt(itemId);
            p.writeInt(quantity);
            return p;
        }

        /// <summary>
        /// 0. SP_1687_EFFECT_PETEFFIMG_BASIC_LEVELUP -> Levelup
        /// 1. SP_1688_EFFECT_PETEFFIMG_BASIC_TELEPORT -> Teleport
        /// 2. warp
        /// 3. SP_1689_EFFECT_PETEFFIMG_BASIC_EVOLUTION -> Evolution
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="petIndex"></param>
        /// <returns></returns>
        static Packet Pet(byte effect, sbyte petIndex)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(4);
            p.writeByte(0);

            // if ( *(this[1975] + 8 * CInPacket::Decode1(v2) + 4) )
            p.writeSByte(petIndex); // Pet Index
            return p;
        }

        // SP_1687_EFFECT_PETEFFIMG_BASIC_LEVELUP
        public static Packet PetLevelUp(sbyte petIndex) => Pet(0, petIndex);


        // SP_1689_EFFECT_PETEFFIMG_BASIC_EVOLUTION
        public static Packet PetEvolution(sbyte petIndex) => Pet(3, petIndex);


        // SP_2967_THE_EXP_DID_NOT_DROP_AFTER_USING_S_ITEM
        public static Packet ExpDidNotDrop(int itemId)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(6);
            p.writeByte(0);
            p.skip(2);
            p.writeInt(itemId);
            return p;
        }

        public static Packet Portal()
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(7);
            return p;
        }
        // SP_2312_EFFECT_BASICEFFIMG_JOBCHANGED
        public static Packet ChangeJob()
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(8);
            return p;
        }
        // SP_2313_EFFECT_BASICEFFIMG_QUESTCLEAR
        public static Packet QuestClear()
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(9);
            return p;
        }
        // CUser::MakeIncDecHPEffect(this, v83, v198[0]);
        public static Packet DecHP(byte value)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(10);
            p.writeByte(value);
            return p;
        }
        //  SP_2435_EFFECT_ITEMEFFIMG_D
        public static Packet ItemEffect(int itemId)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(11);
            p.writeInt(itemId);
            return p;
        }

        // SP_2311_EFFECT_BASICEFFIMG_MONSTERBOOK_CARDGET
        public static Packet GainMonsterCard()
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(13);
            return p;
        }

        // SP_2314_EFFECT_BASICEFFIMG_ITEMLEVELUP
        public static Packet ItemLevelUp()
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(15);
            return p;
        }
        // SP_1246
        public static Packet Enchant(int success)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(16);
            p.writeInt(success);
            return p;
        }

        // SP_2315_EFFECT_BASICEFFIMG_INCEXP
        public static Packet IncExp()
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(17);
            return p;
        }

        public static Packet UsePath1(string str)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(18);
            p.writeString(str);
            return p;
        }

        // SP_4844_ITEM_CASH_0528IMG_08D_EFFECT
        public static Packet Cash528Effect(int itemId)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(20);
            p.writeInt(itemId);
            return p;
        }

        // SP_5241_YOU_HAVE_USED_1_WHEEL_OF_DESTINY_IN_ORDER_TO_REVIVE_AT_THE_CURRENT_MAP_D_LEFT
        public static Packet Wheel(int leftCount)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(21);
            p.writeByte(leftCount);
            return p;
        }

        public static Packet UsePath2(string path)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(23);
            p.writeString(path);
            p.writeInt(1); // unused
            return p;
        }

        // SP_5460_YOU_HAVE_REVIVED_ON_THE_CURRENT_MAP_THROUGH_THE_EFFECT_OF_THE_SPIRIT_STONE
        public static Packet ReviveThroughSpiritStone()
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_ITEM_GAIN_INCHAT);
            p.writeByte(26);
            return p;
        }
    }
}
