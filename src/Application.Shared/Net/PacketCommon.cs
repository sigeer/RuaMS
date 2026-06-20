using Application.Shared.Constants;
using Application.Shared.Constants.Item;
using Application.Shared.Net.Encryption;
using Application.Templates;
using Application.Templates.Character;
using Application.Templates.Item.Pet;
using Application.Templates.Providers;
using Application.Utility;
using DotNetty.Buffers;
using Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Net
{
    public class PacketCommon
    {
        // 2339/1/1 8:00:01
        private static long FT_UT_OFFSET = 116444736010800000L + DateTimeOffset.Now.Offset.Ticks; // normalize with timezone offset suggested by Ari
                                                                                                                         // 2448/1/1
        private static long DEFAULT_TIME = 150842304000000000L;//00 80 05 BB 46 E6 17 02
                                                               // 2268/12/31
        public static long ZERO_TIME = 94354848000000000L;//00 40 E0 FD 3B 37 4F 01
                                                          // 2447/12/31
        private static long PERMANENT = 150841440000000000L; // 00 C0 9B 90 7D E5 17 02

        public static long getTime(long utcTimestamp)
        {
            if (utcTimestamp < 0 && utcTimestamp >= -3)
            {
                if (utcTimestamp == -1)
                {
                    return DEFAULT_TIME;    //high number ll
                }
                else if (utcTimestamp == -2)
                {
                    return ZERO_TIME;
                }
                else
                {
                    return PERMANENT;
                }
            }

            return utcTimestamp * 10000 + FT_UT_OFFSET;
        }
        public static void addExpirationTime(OutPacket p, long time)
        {
            p.writeLong(getTime(time)); // offset expiration time issue found thanks to Thora
        }


        /// <summary>
        /// Sends a ping packet.
        /// </summary>
        /// <returns></returns>
        public static Packet getPing()
        {
            return OutPacket.create(SendOpcode.PING);
        }

        public static Packet getHello(InitializationVector sendIv, InitializationVector recvIv)
        {
            OutPacket p = new ByteBufOutPacket();
            p.writeShort(0x0E);
            p.writeShort(ServerConstants.VERSION);
            p.writeShort(1);
            p.writeByte(49);
            p.writeBytes(recvIv.getBytes());
            p.writeBytes(sendIv.getBytes());
            p.writeByte(8);
            return p;
        }

        public static Packet customPacket(byte[] packet)
        {
            OutPacket p = new ByteBufOutPacket();
            p.writeBytes(packet);
            return p;
        }

        /// <summary>
        /// Gets a server message packet.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Packet serverMessage(string message)
        {
            return serverMessage(4, 0, message, true, false, 0);
        }

        /// <summary>
        /// Gets a server notice packet.
        /// </summary>
        /// <param name="type">The type of the notice.
        /// <para>Possible values for <paramref name="type"/>:</para>
        /// <para>0 - Notice</para>
        /// <para>1 - Popup</para>
        /// <para>2 - Megaphone</para>
        /// <para>3 - SuperMegaphone</para>
        /// <para>4 - ScrollingMessage at top</para>
        /// <para>5 - PinkText</para>
        /// <para>6 - Lightblue Text</para>
        /// <para>7 - BroadCasting NPC</para>
        /// </param>
        /// <param name="message">The message to convey.</param>
        /// <param name="npc"></param>
        /// <returns></returns>
        public static Packet serverNotice(int type, string message, int npc = 0)
        {
            return serverMessage(type, 0, message, false, false, npc);
        }

        public static Packet serverNotice(int type, int channel, string message, bool smegaEar = false)
        {
            return serverMessage(type, channel, message, false, smegaEar, 0);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="channel">The channel this notice was sent on.</param>
        /// <param name="message">The message to convey.</param>
        /// <param name="servermessage">Is this a scrolling ticker?</param>
        /// <param name="megaEar"></param>
        /// <param name="npc"></param>
        /// <returns></returns>
        private static Packet serverMessage(int type, int channel, string message, bool servermessage, bool megaEar, int npc)
        {
            OutPacket p = OutPacket.create(SendOpcode.SERVERMESSAGE);
            p.writeByte(type);
            if (servermessage)
            {
                p.writeByte(1);
            }
            p.writeString(message);
            if (type == 3)
            {
                p.writeByte(channel - 1); // channel
                p.writeBool(megaEar);
            }
            else if (type == 6)
            {
                p.writeInt(0);
            }
            else if (type == 7)
            { // npc
                p.writeInt(npc);
            }
            return p;
        }

        public static Packet SendYellowTip(string tip)
        {
            OutPacket p = OutPacket.create(SendOpcode.SET_WEEK_EVENT_MESSAGE);
            p.writeByte(0xFF);
            p.writeString(tip);
            p.writeShort(0);
            return p;
        }

        [Obsolete("使用 RebroadcastMovementList")]
        public static void rebroadcastMovementListOld(OutPacket op, InPacket ip, int movementDataLength)
        {
            //movement command length is sent by client, probably not a big issue? (could be calculated on server)
            //if multiple write/reads are slow, could use (and cache?) a byte[] buffer
            for (long i = 0; i < movementDataLength; i++)
            {
                op.writeByte(ip.readByte());
            }
        }

        public static void RebroadcastMovementList(OutPacket op, InPacket ip, int movementDataLength)
        {
#if DEBUG
            MethodEventSource.Instance.TrackCall(nameof(PacketCommon.RebroadcastMovementList));
#endif
            op.WriteBytes(ip, movementDataLength);
        }

        public static void EncodeItem(OutPacket p, ItemDto item, AbstractItemTemplate itemTemplate, bool forLook = false)
        {
            bool isCash = itemTemplate.Cash;
            var pos = item.Position;
            var itemType = itemTemplate is PetItemTemplate ? 3 : (itemTemplate is EquipTemplate ? 1: 2);
            bool isEquip = itemType == 1;
            if (forLook)
            {
                if (isEquip)
                {
                    if (pos < 0)
                    {
                        pos *= -1;
                    }
                    p.writeShort(pos > 100 ? pos - 100 : pos);
                }
                else
                {
                    p.writeByte(pos);
                }
            }
            p.writeByte(itemType);
            p.writeInt(item.Itemid);
            p.writeBool(isCash);
            if (isCash)
            {
                p.writeLong(item.PetInfo != null ? item.PetInfo.Petid : (item.EquipInfo?.RingId ?? 0));
            }
            addExpirationTime(p, item.Expiration);
            if (item.PetInfo != null)
            {
                p.writeFixedString(item.PetInfo.Name);
                p.writeByte(item.PetInfo.Level);
                p.writeShort(item.PetInfo.Closeness);
                p.writeByte(item.PetInfo.Fullness);
                addExpirationTime(p, item.Expiration);
                p.writeShort(item.PetInfo.Flag); // PetAttribute noticed by lrenex & Spoon
                p.writeShort(0); // PetSkill
                p.writeInt(18000); // RemainLife
                p.writeShort(0); // attribute
                return;
            }
            if (item.EquipInfo == null)
            {
                p.writeShort(item.Quantity);
                p.writeString(item.Owner);
                p.writeShort(item.Flag); // flag

                if (ItemConstants.isRechargeable(item.Itemid))
                {
                    p.writeInt(2);
                    p.writeBytes(new byte[] { 0x54, 0, 0, 0x34 });
                }
                return;
            }
            else
            {
                var equip = item.EquipInfo;
                var template = (itemTemplate as EquipTemplate)!;

                p.writeByte(equip.Upgradeslots); // upgrade slots
                p.writeByte(equip.Level); // level
                p.writeShort(equip.Str); // str
                p.writeShort(equip.Dex); // dex
                p.writeShort(equip.Int); // int
                p.writeShort(equip.Luk); // luk
                p.writeShort(equip.Hp); // hp
                p.writeShort(equip.Mp); // mp
                p.writeShort(equip.Watk); // watk
                p.writeShort(equip.Matk); // matk
                p.writeShort(equip.Wdef); // wdef
                p.writeShort(equip.Mdef); // mdef
                p.writeShort(equip.Acc); // accuracy
                p.writeShort(equip.Avoid); // avoid
                p.writeShort(equip.Hands); // hands
                p.writeShort(equip.Speed); // speed
                p.writeShort(equip.Jump); // jump
                p.writeString(item.Owner); // owner name
                p.writeShort(item.Flag); //Item Flags

                if (isCash)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        p.writeByte(0x40);
                    }
                }
                else
                {
                    int itemLevel = equip.Itemlevel;
                    long expNibble = (ExpTable.getExpNeededForLevel(template.ReqLevel) * equip.Itemexp);
                    expNibble /= ExpTable.getEquipExpNeededForLevel(itemLevel);

                    p.writeByte(0);
                    p.writeByte(itemLevel); //Item Level
                    p.writeInt((int)expNibble);
                    p.writeInt(equip.Vicious); //WTF NEXON ARE YOU SERIOUS?
                    p.writeLong(0);
                }
                p.writeLong(PacketCommon.getTime(-2));
                p.writeInt(-1);
            }


        }

    }
}
