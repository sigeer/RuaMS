using Application.EF.Entities;
using Application.Shared.Net;
using BBSProto;

namespace Application.Module.BBS.Channel.Net
{
    public class BBSPacketCreator
    {
        public static void addThread(OutPacket p, BBSThreadPreview rs)
        {
            p.writeInt(rs.ThreadId);
            p.writeInt(rs.PosterId);
            p.writeString(rs.Title);
            p.writeLong(PacketCommon.getTime(rs.Timestamp));
            p.writeInt(rs.Icon);
            p.writeInt(rs.ReplyCount);
        }

        public static Packet BBSThreadList(List<BBSThreadPreview> dataList, int start)
        {
            OutPacket p = OutPacket.create(SendOpcode.GUILD_BBS_PACKET);
            p.writeByte(0x06);
            if (dataList.Count == 0)
            {
                p.writeByte(0);
                p.writeInt(0);
                p.writeInt(0);
                return p;
            }
            int threadCount = dataList.Count;
            if (dataList[0].ThreadId == 0)
            { 
                //has a notice
                p.writeByte(1);
                addThread(p, dataList[0]);
                threadCount--; //one thread didn't count (because it's a notice)
            }
            else
            {
                p.writeByte(0);
            }
            if (start + 1 > threadCount)
            {
                start = 0;
            }
            p.writeInt(threadCount);
            p.writeInt(Math.Min(10, threadCount - start));
            for (int i = 0; i < Math.Min(10, threadCount - start); i++)
            {
                addThread(p, dataList[i]);
            }
            return p;
        }

        public static Packet showThread(BBSProto.BBSMainThread threadRS)
        {
            OutPacket p = OutPacket.create(SendOpcode.GUILD_BBS_PACKET);
            p.writeByte(0x07);
            p.writeInt(threadRS.ThreadId);
            p.writeInt(threadRS.PosterId);
            p.writeLong(PacketCommon.getTime(threadRS.Timestamp));
            p.writeString(threadRS.Title);
            p.writeString(threadRS.StartPost);
            p.writeInt(threadRS.Icon);

            int replyCount = threadRS.Replies.Count;
            p.writeInt(replyCount);

            foreach (var item in threadRS.Replies)
            {
                p.writeInt(item.ReplyId);
                p.writeInt(item.PosterId);
                p.writeLong(PacketCommon.getTime(item.Timestamp));
                p.writeString(item.Content);
            }
            return p;
        }
    }
}
