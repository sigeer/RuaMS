namespace Application.Core.Channel.Net.Packets
{
    /// <summary>
    /// CScriptMan::OnScriptMessage
    /// </summary>
    public static class ScriptPackets
    {
        private static OutPacket CreateNpcTalk(int npc, byte msgType, NpcTalkSpeaker speakerTypeId)
        {
            OutPacket p = OutPacket.create(SendOpcode.NPC_TALK);
            p.writeByte(4);
            p.writeInt(npc);
            p.writeByte(msgType);
            p.writeByte((byte)speakerTypeId);
            return p;
        }

        public static Packet Say(int npc, string talk, string endBytes, NpcTalkSpeaker speakerTypeId = 0, int speakerNpc = 0)
        {
            OutPacket p = CreateNpcTalk(npc, 0, speakerTypeId);
            if ((speakerTypeId & NpcTalkSpeaker.ExtraNpc) != 0)
            {
                p.writeInt(speakerNpc);
            }
            p.writeString(talk);
            p.writeBytes(HexTool.toBytes(endBytes));
            return p;
        }

        public static Packet AskYesNo(int npc, string talk, NpcTalkSpeaker speakerTypeId = 0)
        {
            OutPacket p = CreateNpcTalk(npc, 1, speakerTypeId);
            p.writeString(talk);
            return p;
        }

        public static Packet AcceptDecline(int npc, string talk, NpcTalkSpeaker speakerTypeId = 0)
        {
            OutPacket p = CreateNpcTalk(npc, 12, speakerTypeId);
            p.writeString(talk);
            return p;
        }

        public static Packet AskText(int npc, string talk, string def = "", NpcTalkSpeaker speakerTypeId = 0)
        {
            OutPacket p = CreateNpcTalk(npc, 2, speakerTypeId);
            p.writeString(talk);
            p.writeString(def);
            p.writeShort(0);
            p.writeShort(0);
            return p;
        }

        public static Packet AskBoxText(int npc, string talk, string def = "", NpcTalkSpeaker speakerTypeId = 0)
        {
            OutPacket p = CreateNpcTalk(npc, 13, speakerTypeId);
            p.writeString(talk);
            p.writeString(def);
            p.writeShort(0);
            p.writeShort(0);
            return p;
        }

        public static Packet AskNumber(int npc, string talk, int def, int min, int max, NpcTalkSpeaker speakerTypeId = 0)
        {
            OutPacket p = CreateNpcTalk(npc, 3, speakerTypeId);
            p.writeString(talk);
            p.writeInt(def);
            p.writeInt(min);
            p.writeInt(max);
            return p;
        }

        public static Packet AskMenu(int npc, string talk, NpcTalkSpeaker speakerTypeId = 0)
        {
            OutPacket p = CreateNpcTalk(npc, 4, speakerTypeId);
            p.writeString(talk);
            return p;
        }

        /// <summary>
        /// SP_3913_UI_UIWINDOWIMG_INITIALQUIZ_BACKGRND
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="resCode"></param>
        /// <param name="title"></param>
        /// <param name="problemText"></param>
        /// <param name="hintText"></param>
        /// <param name="minInput"></param>
        /// <param name="maxInput"></param>
        /// <param name="remainSeconds"></param>
        /// <returns></returns>
        public static Packet AskQuiz(int npc, int resCode, string title, string problemText, string hintText, int minInput, int maxInput, int remainSeconds)
        {
            OutPacket p = CreateNpcTalk(npc, 5, 0);
            p.writeByte(resCode);
            if (resCode == 0)
            {
                p.writeString(title);
                p.writeString(problemText);
                p.writeString(hintText);
                p.writeInt(minInput);
                p.writeInt(maxInput);
                p.writeInt(remainSeconds);
            }
            return p;
        }

        /// <summary>
        /// SP_3924_UI_UIWINDOWIMG_SPEEDQUIZ_BACKGRND
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="type">
        /// 1. mob; 2. npc; 2. item
        /// </param>
        /// <param name="answer">答案（根据答案生成题目）</param>
        /// <param name="correct"></param>
        /// <param name="remain">剩余题目数量？</param>
        /// <param name="remainSeconds">剩余时间</param>
        /// <returns></returns>
        public static Packet AskSpeedQuiz(int npc, int type, int answer, int correct, int remain, int remainSeconds)
        {
            OutPacket p = CreateNpcTalk(npc, 6, 0);
            p.writeBool(false);
            p.writeInt(type);
            p.writeInt(answer);
            p.writeInt(correct);
            p.writeInt(remain);
            p.writeInt(remainSeconds);
            return p;
        }

        public static Packet CloseSpeedQuiz(int npc)
        {
            OutPacket p = CreateNpcTalk(npc, 6, 0);
            p.writeBool(true);
            return p;
        }

        public static Packet AskAvatar(int npc, string talk, int[] styles)
        {
            OutPacket p = CreateNpcTalk(npc, 7, 0);
            p.writeString(talk);
            p.writeByte(styles.Length);
            foreach (int style in styles)
            {
                p.writeInt(style);
            }
            return p;
        }

        //public static Packet AskMembershopAvatar(int npc, string talk, int[] styles)
        //{
        //    OutPacket p = CreateNpcTalk(npc, 8, 0);
        //    p.writeString(talk);
        //    p.writeByte(styles.Length);
        //    foreach (int style in styles)
        //    {
        //        p.writeInt(style);
        //    }
        //    return p;
        //}

        public static Packet AskPet(int npc, string talk, IEnumerable<long> petIds)
        {
            OutPacket p = CreateNpcTalk(npc, 9, 0);
            p.writeString(talk);
            p.writeByte(petIds.Count());
            foreach (long petId in petIds)
            {
                p.writeLong(petId);
                p.writeByte(0); // 客户端未使用
            }
            return p;
        }

        public static Packet AskPetAll(int npc, string talk, IEnumerable<long> petIds)
        {
            OutPacket p = CreateNpcTalk(npc, 10, 0);
            p.writeString(talk);
            p.writeByte(petIds.Count());
            p.writeByte(0);
            foreach (long petId in petIds)
            {
                p.writeLong(petId);
                p.writeByte(0); // 客户端未使用
            }
            return p;
        }

        public static Packet AskSlideMenu(int npc, string talk, int fieldId = 0)
        {
            OutPacket p = CreateNpcTalk(npc, 14, 0);
            p.writeInt(fieldId);
            p.writeString(talk);
            return p;
        }

        public static Packet DimensionalMirror(string talk)
        {
            return AskSlideMenu(NpcId.DIMENSIONAL_MIRROR, talk);
        }
    }
}