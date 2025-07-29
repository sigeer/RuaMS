/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */



using Application.Core.Game.Players;
using Application.Core.Managers;
using Application.Module.Marriage.Common.Models;
using Application.Shared.Constants.Item;
using Application.Shared.Net;
using client.inventory;
using Serilog;
using tools;

namespace Application.Module.Marriage.Channel.Net;




/**
 * CField_Wedding, CField_WeddingPhoto, CWeddingMan, OnMarriageResult, and all Wedding/Marriage enum/structs.
 *
 * @author Eric
 * <p>
 * Wishlists edited by Drago (Dragohe4rt)
 */
public class WeddingPackets : PacketCreator
{

    /*
        00000000 CWeddingMan     struc ; (sizeof=0x104)
        00000000 vfptr           dd ?                    ; offset
        00000004 ___u1           $01CBC6800BD386B8A8FD818EAD990BEC ?
        0000000C m_mCharIDToMarriageNo ZDictionary<unsigned long,unsigned long,unsigned long> ?
        00000024 m_mReservationPending ZDictionary<unsigned long,ZRef<GW_WeddingReservation>,unsigned long> ?
        0000003C m_mReservationPendingGroom ZDictionary<unsigned long,ZRef<CUser>,unsigned long> ?
        00000054 m_mReservationPendingBride ZDictionary<unsigned long,ZRef<CUser>,unsigned long> ?
        0000006C m_mReservationStartUser ZDictionary<unsigned long,unsigned long,unsigned long> ?
        00000084 m_mReservationCompleted ZDictionary<unsigned long,ZRef<GW_WeddingReservation>,unsigned long> ?
        0000009C m_mGroomWishList ZDictionary<unsigned long,ZRef<ZArray<ZXString<char> > >,unsigned long> ?
        000000B4 m_mBrideWishList ZDictionary<unsigned long,ZRef<ZArray<ZXString<char> > >,unsigned long> ?
        000000CC m_mEngagementPending ZDictionary<unsigned long,ZRef<GW_MarriageRecord>,unsigned long> ?
        000000E4 m_nCurrentWeddingState dd ?
        000000E8 m_dwCurrentWeddingNo dd ?
        000000EC m_dwCurrentWeddingMap dd ?
        000000F0 m_bIsReservationLoaded dd ?
        000000F4 m_dwNumGuestBless dd ?
        000000F8 m_bPhotoSuccess dd ?
        000000FC m_tLastUpdate   dd ?
        00000100 m_bStartWeddingCeremony dd ?
        00000104 CWeddingMan     ends
    */

    //public class Field_Wedding
    //{
    //    public int m_nNoticeCount;
    //    public int m_nCurrentStep;
    //    public int m_nBlessStartTime;
    //}

    //public class Field_WeddingPhoto
    //{
    //    public bool m_bPictureTook;
    //}

    //public class GW_WeddingReservation
    //{
    //    public int dwReservationNo;
    //    public int dwGroom, dwBride;
    //    public string sGroomName, sBrideName;
    //    public int usWeddingType;
    //}

    //public class WeddingWishList
    //{
    //    public IPlayer pUser;
    //    public int dwMarriageNo;
    //    public int nGender;
    //    public int nWLType;
    //    public int nSlotCount;
    //    public List<string> asWishList = new();
    //    public int usModifiedFlag; // dword
    //    public bool bLoaded;
    //}

    //public class GW_WeddingWishList
    //{
    //    public int WEDDINGWL_MAX = 0xA; // enum WEDDINGWL
    //    public int dwReservationNo;
    //    public byte nGender;
    //    public string sItemName;
    //}

    //public enum MarriageRequest
    //{
    //    AddMarriageRecord = 0x0,
    //    SetMarriageRecord = 0x1,
    //    DeleteMarriageRecord = 0x2,
    //    LoadReservation = 0x3,
    //    AddReservation = 0x4,
    //    DeleteReservation = 0x5,
    //    GetReservation = 0x6
    //}


    /**
     * <name> has requested engagement. Will you accept this proposal?
     *
     * @param name
     * @param playerid
     * @return mplew
     */
    public static Packet onMarriageRequest(string name, int playerid)
    {
        OutPacket p = OutPacket.create(SendOpcode.MARRIAGE_REQUEST);
        p.writeByte(0); //mode, 0 = engage, 1 = cancel, 2 = answer.. etc
        p.writeString(name); // name
        p.writeInt(playerid); // playerid 会不会是物品id？
        return p;
    }

    /**
     * A quick rundown of how (I think based off of enough BMS searching) WeddingPhoto_OnTakePhoto works:
     * - We send this packet with (first) the Groom / Bride IGNs
     * - We then send a fieldId (unsure about this part at the moment, 90% sure it's the id of the map)
     * - After this, we write an integer of the amount of characters within the current map (which is the Cake Map -- exclude users within Exit Map)
     * - Once we've retrieved the size of the characters, we begin to write information about them (Encode their name, guild, etc info)
     * - Now that we've Encoded our character data, we begin to Encode the ScreenShotPacket which requires a TemplateID, IGN, and their positioning
     * - Finally, after encoding all of our data, we send this packet out to a MapGen application server
     * - The MapGen server will then retrieve the packet byte array and convert the bytes into a ImageIO 2D JPG output
     * - The result after converting into a JPG will then be remotely uploaded to /weddings/ with ReservedGroomName_ReservedBrideName to be displayed on the web server.
     * <p>
     * - Will no longer continue Wedding Photos, needs a WvsMapGen :(
     *
     * @param ReservedGroomName The groom IGN of the wedding
     * @param ReservedBrideName The bride IGN of the wedding
     * @param m_dwField         The current field id (the id of the cake map, ex. 680000300)
     * @param m_uCount          The current user count (equal to m_dwUsers.size)
     * @param m_dwUsers         The List of all Character guests within the current cake map to be encoded
     * @return mplew (MaplePacket) Byte array to be converted and read for byte[]->ImageIO
     */
    public static Packet onTakePhoto(string ReservedGroomName, string ReservedBrideName, int m_dwField, List<IPlayer> m_dwUsers)
    { // OnIFailedAtWeddingPhotos
        OutPacket p = OutPacket.create(SendOpcode.WEDDING_PHOTO);// v53 header, convert -> v83
        p.writeString(ReservedGroomName);
        p.writeString(ReservedBrideName);
        p.writeInt(m_dwField); // field id?
        p.writeInt(m_dwUsers.Count);

        foreach (IPlayer guest in m_dwUsers)
        {
            // Begin Avatar Encoding
            addCharLook(p, guest, false); // CUser::EncodeAvatar
            p.writeInt(30000); // v20 = *(_DWORD *)(v13 + 2192) -- new groom marriage ID??
            p.writeInt(30000); // v20 = *(_DWORD *)(v13 + 2192) -- new bride marriage ID??
            p.writeString(guest.getName());
            var guestGuild = guest.getGuild();
            p.writeString(guest.getGuildId() > 0 && guestGuild != null ? guestGuild.getName() : "");
            p.writeShort(guest.getGuildId() > 0 && guestGuild != null ? guestGuild.getLogoBG() : 0);
            p.writeByte(guest.getGuildId() > 0 && guestGuild != null ? guestGuild.getLogoBGColor() : 0);
            p.writeShort(guest.getGuildId() > 0 && guestGuild != null ? guestGuild.getLogo() : 0);
            p.writeByte(guest.getGuildId() > 0 && guestGuild != null ? guestGuild.getLogoColor() : 0);
            p.writeShort(guest.getPosition().X); // v18 = *(_DWORD *)(v13 + 3204);
            p.writeShort(guest.getPosition().Y); // v20 = *(_DWORD *)(v13 + 3208);
            // Begin Screenshot Encoding
            p.writeByte(1); // // if ( *(_DWORD *)(v13 + 288) ) { COutPacket::Encode1(&thisa, v20);
            // CPet::EncodeScreenShotPacket(*(CPet **)(v13 + 288), &thisa);
            p.writeInt(1); // dwTemplateID
            p.writeString(guest.getName()); // m_sName
            p.writeShort(guest.getPosition().X); // m_ptCurPos.x
            p.writeShort(guest.getPosition().Y); // m_ptCurPos.y
            p.writeByte(guest.getStance()); // guest.m_bMoveAction
        }

        return p;
    }

    /**
     * Enable spouse chat and their engagement ring without @relog
     *
     * @param marriageId
     * @param chr
     * @param wedding
     * @return mplew
     */
    public static Packet OnMarriageResult(Models.MarriageInfo info)
    {
        OutPacket p = OutPacket.create(SendOpcode.MARRIAGE_RESULT);
        p.writeByte(11);
        p.writeInt(info.Id);
        p.writeInt(info.HusbandId);
        p.writeInt(info.WifeId);
        p.writeShort((int)MarriageClientStatus.ENGAGED);
        p.writeInt(ItemId.WEDDING_RING_MOONSTONE); // Engagement Ring's Outcome (doesn't matter for engagement)
        p.writeInt(ItemId.WEDDING_RING_MOONSTONE); // Engagement Ring's Outcome (doesn't matter for engagement)
        p.writeFixedString(info.HusbandName);
        p.writeFixedString(info.WifeName);

        return p;
    }
    /**
     * To exit the Engagement Window (Waiting for her response...), we send a GMS-like pop-up.
     *
     * @param msg
     * @return mplew
     */
    public static Packet OnMarriageResult(byte msg)
    {
        OutPacket p = OutPacket.create(SendOpcode.MARRIAGE_RESULT);
        p.writeByte(msg);
        if (msg == 36)
        {
            p.writeByte(1);
            p.writeString("You are now engaged.");
        }
        return p;
    }

    /**
     * The World Map includes 'loverPos' in which this packet controls
     *
     * @param partner
     * @param mapid
     * @return mplew
     */
    public static Packet OnNotifyWeddingPartnerTransfer(int partner, int mapid)
    {
        OutPacket p = OutPacket.create(SendOpcode.NOTIFY_MARRIED_PARTNER_MAP_TRANSFER);
        p.writeInt(mapid);
        p.writeInt(partner);
        return p;
    }

    /**
     * The wedding packet to display Pelvis Bebop and enable the Wedding Ceremony Effect between two characters
     * CField_Wedding::OnWeddingProgress - Stages
     * CField_Wedding::OnWeddingCeremonyEnd - Wedding Ceremony Effect
     *
     * @param setBlessEffect
     * @param groom
     * @param bride
     * @param step
     * @return mplew
     */
    public static Packet OnWeddingProgress(bool setBlessEffect, int groom, int bride, byte step)
    {
        OutPacket p = OutPacket.create(setBlessEffect ? SendOpcode.WEDDING_CEREMONY_END : SendOpcode.WEDDING_PROGRESS);
        if (!setBlessEffect)
        { // in order for ceremony packet to send, byte step = 2 must be sent first
            p.writeByte(step);
        }
        p.writeInt(groom);
        p.writeInt(bride);
        return p;
    }


    /// <summary>
    ///  When we open a Wedding Invitation, we display the Bride & Groom
    /// </summary>
    /// <param name="groom"></param>
    /// <param name="bride"></param>
    /// <returns></returns>
    public static Packet sendWeddingInvitation(string groom, string bride)
    {
        OutPacket p = OutPacket.create(SendOpcode.MARRIAGE_RESULT);
        p.writeByte(15);
        p.writeString(groom);
        p.writeString(bride);
        p.writeShort(1); // 0 = Cathedral Normal?, 1 = Cathedral Premium?, 2 = Chapel Normal?
        return p;
    }

    public static Packet sendWishList()
    {
        // fuck my life
        OutPacket p = OutPacket.create(SendOpcode.MARRIAGE_REQUEST);
        p.writeByte(9);
        return p;
    }


    /// <summary>
    /// Handles all of WeddingWishlist packets
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="itemnames"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public static Packet onWeddingGiftResult(byte mode, List<string> itemnames, List<Item>? items)
    {
        OutPacket p = OutPacket.create(SendOpcode.WEDDING_GIFT_RESULT);
        p.writeByte(mode);
        switch (mode)
        {
            case 0xC: // 12 : You cannot give more than one present for each wishlist 
            case 0xE: // 14 : Failed to send the gift.
                break;

            case 0x09:
                { // Load Wedding Registry
                    p.writeByte(itemnames.Count);
                    foreach (string names in itemnames)
                    {
                        p.writeString(names);
                    }
                    break;
                }
            case 0xA: // Load Bride's Wishlist 
            case 0xF: // 10, 15, 16 = CWishListRecvDlg::OnPacket
            case 0xB:
                { // Add Item to Wedding Registry 
                  // 11 : You have sent a gift | | 13 : Failed to send the gift. | 
                    if (mode == 0xB)
                    {
                        p.writeByte(itemnames.Count);
                        foreach (string names in itemnames)
                        {
                            p.writeString(names);
                        }
                    }
                    p.writeLong(32);
                    p.writeByte(items.Count);
                    foreach (Item item in items)
                    {
                        addItemInfo(p, item, true);
                    }
                    break;
                }
            default:
                {
                    Log.Logger.Warning("Unknown Wishlist Mode: {Mode}", mode);
                    break;
                }
        }
        return p;
    }

    public static Packet OnCoupleMessage(string fiance, string text, bool spouse)
    {
        OutPacket p = OutPacket.create(SendOpcode.SPOUSE_CHAT);
        p.writeByte(spouse ? 5 : 4); // v2 = CInPacket::Decode1(a1) - 4;
        if (spouse)
        { // if ( v2 ) {
            p.writeString(fiance);
        }
        p.writeByte(spouse ? 5 : 1);
        p.writeString(text);
        return p;
    }
}