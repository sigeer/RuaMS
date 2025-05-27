using net.packet;

namespace Application.Core.Net
{
    public class CommonPacketCreator
    {

        // 2339/1/1 8:00:01
        private static long FT_UT_OFFSET = 116444736010800000L + TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).Ticks; // normalize with timezone offset suggested by Ari
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

        public static Packet customPacket(string packet)
        {
            OutPacket p = new ByteBufOutPacket();
            p.writeBytes(HexTool.toBytes(packet));
            return p;
        }

        public static Packet customPacket(byte[] packet)
        {
            OutPacket p = new ByteBufOutPacket();
            p.writeBytes(packet);
            return p;
        }

    }
}
