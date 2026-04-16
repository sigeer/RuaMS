using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal class PQ_Kerning : PartyQuestEventManager
    {
        public PQ_Kerning(WorldChannel cserv) : base(cserv, nameof(PQ_Henesys))
        {
            MinCount = 3;
            MaxCount = 6;
            MinLevel = 21;
            MaxLevel = 30;
            EntryMap = 103000800;
            ExitMap = 103000890;
            RecruitMap = 103000000;
            ClearMap = 103000805;
            MinMap = 103000800;
            MaxMap = 103000805;
            EventTime = 30 * 60;
        }

        protected override void respawnStages(AbstractEventInstanceManager eim)
        {
            eim.getMapInstance(103000800).instanceMapRespawn();
            eim.getMapInstance(103000805).instanceMapRespawn();

            eim.Schedule(respawnStages, 15 * 1000);
        }

        protected override void setEventRewards(AbstractEventInstanceManager eim)
        {
            int evLevel = 1;    //Rewards at clear PQ
            List<object> itemSet = [2040505, 2040514, 2040502, 2040002, 2040602, 2040402, 2040802, 1032009, 1032004, 1032005, 1032006, 1032007, 1032010, 1032002, 1002026, 1002089, 1002090, 2000003, 2000001, 2000002, 2000006, 2022003, 2022000, 2000004, 4003000, 4010000, 4010001, 4010002, 4010003, 4010004, 4010005, 4010006, 4010007, 4020000, 4020001, 4020002, 4020003, 4020004, 4020005, 4020006, 4020007, 4020008];
            List<object> itemQty = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 80, 80, 80, 50, 5, 15, 15, 30, 15, 15, 15, 15, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 3, 3];
            eim.setEventRewards(evLevel, itemSet, itemQty);

            List<object> expStages = [100, 200, 400, 800, 1500];    //bonus exp given on CLEAR stage signal
            eim.setEventClearStageExp(expStages);
        }
    }
}
