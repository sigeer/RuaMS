using client.inventory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Net.Packets
{
    // CWvsContext::OnMessage
    public class MessagePacket
    {
        #region CWvsContext::OnDropPickUpMessage

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="meso"></param>
        /// <returns></returns>
        public static Packet PickupMeso(int meso)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(0);

            p.writeByte(1);     // v3 = CInPacket::Decode1(a2);
            p.writeBool(false); // v4 = CInPacket::Decode1(v2);  true: SP_5239_A_PORTION_WAS_NOT_FOUND_AFTER_FALLING_ON_THE_GROUND
            p.writeInt(meso);   // a2 = CInPacket::Decode4(v2);
            p.writeShort(0);    // SP_291_INTERNET_CAFE_MESO_BONUS_D
            return p;
        }


        public static Packet PickupItem(int itemId, int quantity)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(0);

            p.writeByte(0);
            p.writeInt(itemId);
            p.writeInt(quantity);
            return p;
        }

        // // SP_5438
        public static Packet PickupItem(int itemId)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(0);

            p.writeByte(2);
            p.writeInt(itemId);
            return p;
        }

        // SP_2983_THIS_ITEM_IS_UNAVAILABLE_FOR_THE_PICK_UP
        public static Packet CannotPickupItem()
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(0);

            p.writeByte(-2);
            return p;
        }

        // SP_295_YOU_CANT_GET_ANYMORE_ITEMS
        public static Packet CannotGetNewItem()
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(0);

            p.writeByte(-1);
            return p;
        }

        // SP_5317_YOU_CANNOT_ACQUIRE_ANY_ITEMS
        // SP_5311_YOU_CANNOT_ACQUIRE_ANY_ITEMS_BECAUSE_THE_GAME_FILE_HAS_BEEN_DAMAGED_PLEASE_TRY_A
        public static Packet CannotGetAnyItem()
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(0);

            p.writeByte(-3);
            return p;
        }
        #endregion


        /// <summary>
        /// 
        /// CWvsContext::OnCashItemExpireMessage
        /// SP_296__S__HAS_PASSED_ITS_EXPIRATION_DATE_AND_WILL_BE_REMOVED_FROM_YOUR_INVENTORY
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Packet CashItemExpireMessage(int itemId)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(2);
            p.writeInt(itemId);
            return p;
        }

        /// <summary>
        /// 
        /// CWvsContext::OnIncSPMessage
        /// SP_288_YOU_HAVE_GAINED_FAME_D / SP_289_YOU_HAVE_LOST_FAME_D
        /// </summary>
        /// <param name="fame"></param>
        /// <returns></returns>
        public static Packet IncSPMessage(int fame)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(4);
            p.writeInt(fame);
            return p;
        }

        /// <summary>
        /// 
        /// CWvsContext::OnIncMoneyMessage SP_292_YOU_HAVE_LOST_MESOS_D / SP_290_YOU_HAVE_GAINED_MESOS_D CHATLOG_ADD
        /// </summary>
        /// <param name="meso"></param>
        /// <returns></returns>
        public static Packet IncMoneyMessage(int meso)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(5);
            p.writeInt(meso);
            return p;
        }

        public static Packet IncGPMessage(int gp)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(6);

            p.writeInt(gp);
            return p;
        }

        public static Packet GiveBuffMessage(int itemId)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(7);

            p.writeInt(itemId);
            return p;
        }

        /// <summary>
        /// 
        /// CWvsContext::OnGeneralItemExpireMessage SP_2886_THE_ITEM__S__HAS_BEEN_EXPIRED_AND_THEREFORE_DELETED_FROM_YOUR_INVENTORY
        /// </summary>
        /// <param name="itemIds"></param>
        /// <returns></returns>
        public static Packet GeneralItemExpireMessage(IEnumerable<int> itemIds)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(8);
            p.writeByte(itemIds.Count());
            foreach (var item in itemIds)
            {
                p.writeInt(item);
            }
            return p;
        }

        /// <summary>
        /// 
        /// CWvsContext::OnSystemMessage
        /// 效果同 serverNotice(5, str);
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Packet SystemMessage(string str)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(9);
            p.writeString(str);
            return p;
        }

        /// <summary>
        /// 
        /// CWvsContext::OnItemProtectExpireMessage 
        /// SP_5015_SS_SEAL_HAS_EXPIRED
        /// </summary>
        /// <param name="itemIds"></param>
        /// <returns></returns>
        public static Packet ItemProtectExpireMessage(IEnumerable<int> itemIds)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(11);
            p.writeByte(itemIds.Count());
            foreach (var item in itemIds)
            {
                p.writeInt(item);
            }
            return p;
        }

        public static Packet ItemExpireReplaceMessage(IEnumerable<string> itemExpiraMessages)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(12);
            p.writeByte(itemExpiraMessages.Count());
            foreach (var item in itemExpiraMessages)
            {
                p.writeString(item);
            }
            return p;
        }

        /// <summary>
        /// 
        /// CWvsContext::OnSkillExpireMessage
        /// SP_5240_S_HAS_DISAPPEARED_AS_THE_TIME_LIMIT_HAS_PASSED
        /// </summary>
        /// <param name="skills"></param>
        /// <returns></returns>
        public static Packet SkillExpireMessage(IEnumerable<int> skills)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(13);
            p.writeByte(skills.Count());
            foreach (var item in skills)
            {
                p.writeInt(item);
            }
            return p;
        }




    }
}
