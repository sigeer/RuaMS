using Application.Core.Game.Maps.MiniRoom;
using Application.Core.Models;
using tools;

namespace Application.Core.Channel.Net.Packets;

/// <summary>
/// Fredrick (弗兰德里/雇佣商店) 数据包构造器
/// </summary>
internal class FredrickPackets
{
    /// <summary>
    /// 打开 Fredrick 商店 (op 0x23) sub_799DA1
    /// 格式与 CTrunkDlg::SetGetItems 一致 (通过 PacketCreator.AddSetGetItems)
    /// </summary>
    public static Packet GetFredrick(RemoteHiredMerchantData store)
    {
        OutPacket p = OutPacket.create(SendOpcode.FREDRICK);
        p.writeByte(0x23);
        p.writeInt(NpcId.FREDRICK);
        // 原先的32272 相当于 slot = 0x10, mask = 0x7E
        PacketCreator.AddSetGetItems(p, store.Items.Length, store.Items, store.Meso);
        return p;
    }

    /// <summary>
    /// 费用确认弹窗 (op 0x24 = 36)
    /// 客户端显示:
    ///  SP_3490
    ///   feeMeso=0 → "Are you sure you want to retrieve them?"
    ///   feeMeso>0 → These were here for %d days, so the storing\r\nfee will cost you %d%% of the total price,\r\n%d mesos.\r\nAre you sure you want to retrieve them?
    ///                    SP_3490_THESE_WERE_HERE_FOR_D_DAYS_SO_THE_STORING_R_NFEE_WILL_COST_YOU_D_OF_THE_TOTAL_PR
    /// 用户点 Yes → 服务端收到 FREDRICK_ACTION + 0x1B
    /// </summary>
    /// <param name="storageDays">存储天数 (客户端上限 100)</param>
    public static Packet GetFredrickFeePrompt(int storageDays, int feeMeso)
    {
        OutPacket p = OutPacket.create(SendOpcode.FREDRICK);
        p.writeByte(0x24);
        // 手续费比例与存储天数一致，大于100时会删除，可共用一个值
        p.writeInt(storageDays);
        p.writeInt(feeMeso);
        return p;
    }

    /// <summary>
    /// 提示玩家商店还在营业中 (op 0x25 = 37)
    /// 客户端弹 CUtilDlgEx 对话框，显示频道和地图信息
    /// SP_3488
    /// </summary>
    /// <param name="mapId">商店所在地图 ID</param>
    /// <param name="channel">商店所在频道</param>
    public static Packet GetFredrickShopActive(int mapId, byte channel)
    {
        OutPacket p = OutPacket.create(SendOpcode.FREDRICK);
        p.writeByte(0x25);
        p.writeInt(NpcId.FREDRICK);          // SetUtilDlgEx 第3个参数是NpcId
        p.writeInt(mapId);
        p.writeByte(channel - 1);
        return p;
    }

    /// <summary>
    /// SP_3487_I_DONT_THINK_YOU_HAVE_ANY_ITEMS_OR_MONEY_TO_RETRIEVE_HERE_R_NTHIS_IS_WHERE_YOU_R
    /// </summary>
    /// <returns></returns>
    public static Packet Nothing()
    {
        OutPacket p = OutPacket.create(SendOpcode.FREDRICK);
        p.writeByte(0x25);
        p.writeInt(NpcId.FREDRICK);          // SetUtilDlgEx 第3个参数是NpcId
        p.writeInt(MapId.NONE);
        p.writeByte(0);
        return p;
    }

    /// <summary>
    /// 不可用提示 (op 0x26 = 38)
    /// 显示: SP_3489 "This is currently unavailable, please try again later"
    /// </summary>
    public static Packet GetFredrickUnavailable()
    {
        OutPacket p = OutPacket.create(SendOpcode.FREDRICK);
        p.writeByte(0x26);
        return p;
    }

    // 从弗兰德里取物品
    public const int FredrickMessage_RetrieveSuccess = 0x1E;
    public const int FredrickMessage_TooMuchMeso = 0x1F;
    public const int FredrickMessage_Unique = 0x20;
    public const int FredrickMessage_FeeRequired = 0x21;
    public const int FredrickMessage_InvFull = 0x22;

    /// <summary>
    /// sub_79B382 取回操作结果 (FREDRICK_MESSAGE = 0x136)
    /// 客户端 MessageID 310 接收: Decode1() - 30
    ///
    /// 有效值 (有对应的客户端字符串资源):
    ///   0x1E (30) = 成功 — SP_3492 "You have retrieved your items and mesos"
    ///   0x1F (31) = 金币过多 — SP_3493
    ///   0x20 (32) = 特殊物品不可 — SP_3494
    ///   0x21 (33) = 服务费不足 — SP_3495_DUE_TO_THE_LACK_OF_SERVICE_FEE_YOU_WERE_UNABLE_TO__R_NRETRIEVE_MESOS_OR_ITEMS
    ///   0x22 (34) = 背包已满 — SP_3496_UNABLE_TO_RETRIEVE_MESOS_AND_ITEMS_R_NDUE_TO_FULL_INVENTORY
    ///
    /// </summary>
    public static Packet FredrickMessage(byte operation)
    {
        OutPacket p = OutPacket.create(SendOpcode.FREDRICK_MESSAGE);
        p.writeByte(operation);
        return p;
    }


    public static Packet WithdrawMeso()
    {
        OutPacket p = OutPacket.create(SendOpcode.PLAYER_INTERACTION);
        p.writeByte(PlayerInterAction.MERCHANT_MESO_RESULT.getCode());
        return p;
    }

    #region sub_518D5F

    public static Packet LeaveHiredMerchant(int slot, LeaveMiniRoomReason reason)
    {
        OutPacket p = OutPacket.create(SendOpcode.PLAYER_INTERACTION);
        p.writeByte(PlayerInterAction.EXIT.getCode());
        p.writeByte(slot);  // 店主0  // if ( nSlot._m_pStr == (char *)this->m_nMyPosition )
        p.writeByte((byte)reason);
        return p;
    }
    #endregion

    #region sub_5190E1 手动关店时返还物品
    // SP_3478_BOTH_THE_ITEMS_AND_MESOS_R_NARE_SUCCESSFULLY_RETRIEVED
    public static Packet RetrieveSuccess()
    {
        OutPacket p = OutPacket.create(SendOpcode.PLAYER_INTERACTION);
        p.writeByte(PlayerInterAction.REAL_CLOSE_MERCHANT.getCode());
        p.writeByte(0x0);
        return p;
    }

    // SP_3479_UNABLE_TO_RETRIEVE_THE_ITEMS_AND_MESOS_DUE_TO_THE_STORE_HAVING_TOO_MUCH_MONEY_R_
    public static Packet RetrieveFail_TooMuchMoney()
    {
        OutPacket p = OutPacket.create(SendOpcode.PLAYER_INTERACTION);
        p.writeByte(PlayerInterAction.REAL_CLOSE_MERCHANT.getCode());
        p.writeByte(0x1);
        return p;
    }
    // SP_3480_UNABLE_TO_RETRIEVE_THE_ITEMS_BECAUSE_ONE_OF_THE_ITEMS__BUT_THE_MESOS_WERE_RETRIE
    public static Packet RetrieveFail_Unique()
    {
        OutPacket p = OutPacket.create(SendOpcode.PLAYER_INTERACTION);
        p.writeByte(PlayerInterAction.REAL_CLOSE_MERCHANT.getCode());
        p.writeByte(0x2);
        return p;
    }
    // SP_3481_UNABLE_TO_RETRIEVE_THE_ITEMS_DUE_TO_FULL_INVENTORY_BUT_THE_MESOS_WERE_RETRIEVED_
    public static Packet RetrieveFail_InvFull()
    {
        OutPacket p = OutPacket.create(SendOpcode.PLAYER_INTERACTION);
        p.writeByte(PlayerInterAction.REAL_CLOSE_MERCHANT.getCode());
        p.writeByte(0x3);
        return p;
    }
    // SP_3482_UNABLE_TO_RETRIEVE_THE_ITEMS_FOR_UNKNOWN_REASONS_BUT_THE_MESOS_WERE_RETRIEVED_R_
    public static Packet RetrieveFail_Unknown()
    {
        OutPacket p = OutPacket.create(SendOpcode.PLAYER_INTERACTION);
        p.writeByte(PlayerInterAction.REAL_CLOSE_MERCHANT.getCode());
        p.writeByte(0x4);
        return p;
    }
    #endregion



    #region CWvsContext::OnEntrustedShopCheckResult
    // SP_3471_YOUR_STORE_IS_CURRENTLY_OPEN_R_NIN_CHANNEL_S_FREE_MARKET_D_R_NPLEASE_USE_THIS_AF
    public static Packet HasHiredMerchant(int mapId, byte channel)
    {
        OutPacket p = OutPacket.create(SendOpcode.ENTRUSTED_SHOP_CHECK_RESULT); // header.
        p.writeByte(8);
        p.writeInt(mapId);
        p.writeByte(channel - 1);
        return p;
    }

    // SP_3476_PLEASE_USE_THIS_AFTER_RETRIEVING_ITEMS_R_N_FROM_FREDRICK_OF_FREE_MARKET
    public static Packet retrieveFirstMessage()
    {
        OutPacket p = OutPacket.create(SendOpcode.ENTRUSTED_SHOP_CHECK_RESULT); // header.
        p.writeByte(9);
        return p;
    }

    // SP_3497_PLEASE_RETRIEVE_YOUR_ITEMS_FROM_FREDRICK
    public static Packet HiredMerchantCloseNotify()
    {
        OutPacket p = OutPacket.create(SendOpcode.ENTRUSTED_SHOP_CHECK_RESULT); // header.
        p.writeByte(15);
        return p;
    }

    // SP_3472_THE_STORE_IS_OPEN_AT_CHANNEL_S_R_NWOULD_YOU_LIKE_TO_CHANGE_TO_THAT_CHANNEL
    public static Packet remoteChannelChange(byte ch)
    {
        OutPacket p = OutPacket.create(SendOpcode.ENTRUSTED_SHOP_CHECK_RESULT); // header.
        p.writeByte(16);
        p.writeInt(0);//No idea yet
        p.writeByte(ch);
        return p;
    }


    // SP_3465_UNABLE_TO_USE_THIS_DUE_TO_THE_REMOTE_SHOP_NOT_BEING_OPEN
    public static Packet RemoteShopNotOpen()
    {
        OutPacket p = OutPacket.create(SendOpcode.ENTRUSTED_SHOP_CHECK_RESULT); // header.
        p.writeByte(16);
        p.writeInt(0);//No idea yet
        p.writeByte(0xFF);
        return p;
    }
    #endregion


}
