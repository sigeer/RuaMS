namespace Application.Core.Channel.Net.Packets
{
    public class CashPackets
    {
        /// <summary>
        /// sub_47AC0A
        /// SP_532_YOU_HAVE_ADDED_A_NEW_CHARACTER_SLOT
        /// </summary>
        /// <param name="slots"></param>
        /// <returns></returns>
        public static Packet BoughtCharacterSlotSuccess(short slots)
        {
            OutPacket p = OutPacket.create(SendOpcode.CASHSHOP_OPERATION);

            p.writeByte(0x64);
            p.writeShort(slots);

            return p;
        }


        /// <summary>
        /// sub_47AC9B
        /// </summary>
        /// <param name="reason">
        /// </param>
        /// <returns></returns>
        public static Packet BoughtCharacterSlotFailed(byte reason)
        {
            OutPacket p = OutPacket.create(SendOpcode.CASHSHOP_OPERATION);

            p.writeByte(101);
            p.writeByte(reason);

            return p;
        }


        /// <summary>
        /// 购买额外项链槽
        /// </summary>
        /// <param name="days"></param>
        /// <param name="isExtend">延长</param>
        /// <returns></returns>
        public static Packet BoughtEquipExtraSlotSuccess(short days, bool isExtend)
        {
            OutPacket p = OutPacket.create(SendOpcode.CASHSHOP_OPERATION);

            p.writeByte(102);

            // SP_5209_THE_TIME_LIMIT_FOR_THE__R_N_S_SLOT_HAS_BEEN_EXTENDED_TO_DMONTH_DDATE_DYEAR_AT_DH
            // SP_5210_YOU_HAVE_ADDED_S_SLOTS
            p.writeShort(isExtend ? 1:0);
            p.writeShort(days);

            return p;
        }
    }
}
