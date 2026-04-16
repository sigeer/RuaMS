using Application.Core.Channel;
using Application.Core.Client;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using scripting.npc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Plugin.Script.Events
{
    internal class PQ_WuGong : PartyQuestEventManager
    {
        public PQ_WuGong(WorldChannel cserv) : base(cserv, nameof(PQ_WuGong))
        {
            MinCount = 1;
            MaxCount = 6;
            MinLevel = 25;
            MaxLevel = 90;
            EntryMap = 701010323;
            ExitMap = 701010320;
            RecruitMap = 701010322;
            ClearMap = 701010323;
            MinMap = 701010323;
            MaxMap = 701010323;
            EventTime = 30 * 60;
        }

        public override string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    return null;
                case CreateInstanceResult.RequiredParty:
                    return "该项#b秘密任务#k十分危险，你需要进行一个组队才能进入执行。";

                case CreateInstanceResult.RequiredLeader:
                    return "请让你的队长来开始这个任务。";

                case CreateInstanceResult.Requirement:
                    return "你目前无法执行这个#b秘密任务#k，因你不符合要求";

                case CreateInstanceResult.LobbyLimited:
                    return "已经有其他人员在执行#b秘密任务#k了，请稍后再试。";

                case CreateInstanceResult.Disposed:
                case CreateInstanceResult.Unknown:
                default:
                    return "未知错误";
            }
        }
    }
}
