using Application.Shared.Net.Encryption;
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


        /// <summary>
        /// Sends a ping packet.
        /// </summary>
        /// <returns></returns>
        public static Packet getPing()
        {
            return OutPacket.create(SendOpcode.PING);
        }

        public static Packet getHello(short mapleVersion, InitializationVector sendIv, InitializationVector recvIv)
        {
            OutPacket p = new ByteBufOutPacket();
            p.writeShort(0x0E);
            p.writeShort(mapleVersion);
            p.writeShort(1);
            p.writeByte(49);
            p.writeBytes(recvIv.getBytes());
            p.writeBytes(sendIv.getBytes());
            p.writeByte(8);
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
    }
}
