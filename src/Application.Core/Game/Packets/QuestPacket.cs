using client;

namespace Application.Core.Game.Packets
{
    public class QuestPacket
    {
        public static void AddQuestInfo(OutPacket p, IPlayer chr)
        {
            List<QuestStatus> started = chr.getStartedQuests();
            int startedSize = 0;
            foreach (QuestStatus qs in started)
            {
                if (qs.getInfoNumber() > 0)
                {
                    startedSize++;
                }
                startedSize++;
            }
            p.writeShort(startedSize);
            foreach (QuestStatus qs in started)
            {
                p.writeShort(qs.getQuest().getId());
                p.writeString(qs.getProgressData());

                short infoNumber = qs.getInfoNumber();
                if (infoNumber > 0)
                {
                    QuestStatus iqs = chr.GetOrAddQuest(infoNumber);
                    p.writeShort(infoNumber);
                    p.writeString(iqs.getProgressData());
                }
            }
            List<QuestStatus> completed = chr.getCompletedQuests();
            p.writeShort(completed.Count);
            foreach (QuestStatus qs in completed)
            {
                p.writeShort(qs.getQuest().getId());
                p.writeLong(PacketCommon.getTime(qs.getCompletionTime()));
            }
        }

        public static Packet ForfeitQuest(short quest)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(1);
            p.writeShort(quest);
            p.writeByte(0);
            return p;
        }

        public static Packet CompleteQuest(short quest, long time)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(1);
            p.writeShort(quest);
            p.writeByte(2);
            p.writeLong(PacketCommon.getTime(time));
            return p;
        }

        public static Packet UpdateQuestInfo(short quest, int npc)
        {
            OutPacket p = OutPacket.create(SendOpcode.UPDATE_QUEST_INFO);
            p.writeByte(8); //0x0A in v95
            p.writeShort(quest);
            p.writeInt(npc);
            p.writeInt(0);
            return p;
        }

        public static Packet AddQuestTimeLimit(short quest, int time)
        {
            OutPacket p = OutPacket.create(SendOpcode.UPDATE_QUEST_INFO);
            p.writeByte(6);
            p.writeShort(1);//Size but meh, when will there be 2 at the same time? And it won't even replace the old one :)
            p.writeShort(quest);
            p.writeInt(time);
            return p;
        }

        public static Packet RemoveQuestTimeLimit(short quest)
        {
            OutPacket p = OutPacket.create(SendOpcode.UPDATE_QUEST_INFO);
            p.writeByte(7);
            p.writeShort(1);//Position
            p.writeShort(quest);
            return p;
        }

        public static Packet UpdateQuest(IPlayer chr, QuestStatus qs, bool infoUpdate)
        {
            OutPacket p = OutPacket.create(SendOpcode.SHOW_STATUS_INFO);
            p.writeByte(1);
            if (infoUpdate)
            {
                QuestStatus iqs = chr.GetOrAddQuest(qs.getInfoNumber());
                p.writeShort(iqs.getQuestID());
                p.writeByte(1);
                p.writeString(iqs.getProgressData());
            }
            else
            {
                p.writeShort(qs.getQuest().getId());
                p.writeByte((int)qs.getStatus());
                p.writeString(qs.getProgressData());
            }
            p.skip(5);
            return p;
        }

        public static Packet UpdateQuestFinish(short quest, int npc, short nextquest)
        {
            //Check
            OutPacket p = OutPacket.create(SendOpcode.UPDATE_QUEST_INFO); //0xF2 in v95
            p.writeByte(8);//0x0A in v95
            p.writeShort(quest);
            p.writeInt(npc);
            p.writeShort(nextquest);
            return p;
        }

        public static Packet QuestError(short quest)
        {
            OutPacket p = OutPacket.create(SendOpcode.UPDATE_QUEST_INFO);
            p.writeByte(0x0A);
            p.writeShort(quest);
            return p;
        }

        public static Packet QuestFailure(byte type)
        {
            OutPacket p = OutPacket.create(SendOpcode.UPDATE_QUEST_INFO);
            p.writeByte(type);//0x0B = No meso, 0x0D = Worn by character, 0x0E = Not having the item ?
            return p;
        }

        public static Packet QuestExpire(short quest)
        {
            OutPacket p = OutPacket.create(SendOpcode.UPDATE_QUEST_INFO);
            p.writeByte(0x0F);
            p.writeShort(quest);
            return p;
        }


    }
}
