using Application.Core.Channel.DueyService;
using tools;

namespace Application.Core.Channel.Net.Packets
{
    public class DueyPacketCreator
    {
        public static Packet removeItemFromDuey(bool remove, int Package)
        {
            OutPacket p = OutPacket.create(SendOpcode.PARCEL);
            p.writeByte(0x17);
            p.writeInt(Package);
            p.writeByte(remove ? 3 : 4);
            return p;
        }

        public static Packet sendDueyParcelReceived(string from, bool quick)
        {    // thanks inhyuk
            OutPacket p = OutPacket.create(SendOpcode.PARCEL);
            p.writeByte(0x19);
            p.writeString(from);
            p.writeBool(quick);
            return p;
        }

        public static Packet sendDueyParcelNotification(bool quick)
        {
            OutPacket p = OutPacket.create(SendOpcode.PARCEL);
            p.writeByte(0x1B);
            p.writeBool(quick);  // 0 : package received, 1 : quick delivery namespace return p;
            return p;
        }

        public static Packet sendDueyMSG(byte operation)
        {
            return sendDuey(operation, []);
        }

        public static Packet SendQuickly()
        {
            return sendDuey(0x1A, []);
        }

        public static Packet sendDuey(int operation, DueyPackageObject[] packages)
        {
            OutPacket p = OutPacket.create(SendOpcode.PARCEL);
            p.writeByte(operation);
            if (operation == DueyProcessorActions.TOCLIENT_OPEN_DUEY.getCode())
            {
                p.writeByte(0);
                p.writeByte(packages.Length);
                foreach (var dp in packages)
                {
                    p.writeInt(dp.PackageId);
                    p.writeFixedString(dp.SenderName);

                    p.writeInt(dp.Mesos);
                    p.writeLong(PacketCommon.getTime(dp.sentTimeInMilliseconds()));

                    var msg = dp.Message;
                    if (msg != null)
                    {
                        p.writeInt(1);
                        p.writeFixedString(msg, 200);
                    }
                    else
                    {
                        p.writeInt(0);
                        p.skip(200);
                    }

                    p.writeByte(0);
                    if (dp.Item != null)
                    {
                        p.writeByte(1);
                        PacketCreator.addItemInfo(p, dp.Item, true);
                    }
                    else
                    {
                        p.writeByte(0);
                    }
                }
                p.writeByte(0);
            }

            return p;
        }
    }
}
