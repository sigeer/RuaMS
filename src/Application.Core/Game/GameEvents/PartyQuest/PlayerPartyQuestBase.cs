using Application.Core.Scripting.Events;
using scripting.Event;
using scripting.npc;

namespace Application.Core.Game.GameEvents.PartyQuest
{
    public abstract class PlayerPartyQuestBase
    {
        public string EventName { get; set; }
        public string EventFamily { get; set; }

        public int MinCount { get; set; }
        public int MaxCount { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public int EntryNpcId { get; set; }
        public int FirstMapId { get; set; }
        public int EndMapId { get; set; }
        public abstract int ClearMapId { get; }
        public Player Player { get; }
        public PartyQuestEventManager EventManager => Player.getChannelServer().getEventSM().getEventManager(EventName) as PartyQuestEventManager ?? throw new BusinessResException($"{EventName} 对应的脚本没有找到");
        protected PlayerPartyQuestBase(string name, string evtFamily, Player player)
        {
            EventName = name;
            EventFamily = evtFamily;
            Player = player;

            MinCount = EventManager.MinCount;
            MaxCount = EventManager.MaxCount;
            MinLevel = EventManager.MinLevel;
            MaxLevel = EventManager.MaxLevel;
            FirstMapId = EventManager.EntryMap;
            EndMapId = EventManager.ExitMap;
        }
        public void StartQuest()
        {
            if (!Player.isPartyLeader())
            {
                Player.dropMessage(1, "只有队长才能开启任务");
                return;
            }

            var effectTeam = FilterTeam();
            var teamEffectCount = effectTeam.Count;
            if (teamEffectCount < MinCount || teamEffectCount > MaxCount)
            {
                Player.dropMessage(1, "队伍人数或者等级不符合要求");
                return;
            }
            var r = EventManager.StartInstance(Player);
            Player.Pink(EventManager.HandleCreateInstanceResult(r, Player.Client) ?? "");

        }

        protected virtual void PassStage(AbstractEventInstanceManager eim, int curStg, int curMapId)
        {
            eim.setProperty(curStg + "stageclear", "true");
            eim.showClearEffect(true);

            eim.linkToNextStage(curStg, EventFamily, curMapId);  //opens the portal to the next map
        }

        public void CompleteStage()
        {
            var eim = Player.getEventInstance();
            if (eim != null)
            {
                var curMapId = Player.getMapId();
                if (curMapId <= ClearMapId)
                {
                    var curStg = GetStageFromMap(curMapId);
                    PassStage(eim, curStg, curMapId);

                    if (curMapId == ClearMapId)
                        CompleteQuest(eim);
                }
                else
                {
                    Player.dropMessage("自行离场");
                }
            }
            else
            {
                Player.dropMessage(1, "没有任务");
            }
        }

        public virtual void CompleteQuest(AbstractEventInstanceManager eim)
        {
            eim.clearPQ();
        }

        protected virtual List<Player> FilterTeam()
        {
            return Player.getParty()!.GetChannelMembers(Player.Client.CurrentServer).Where(x => x.Level >= MinLevel && x.Level <= MaxLevel).ToList();
        }

        public abstract int GetStageFromMap(int mapId);
    }

    public record MapStage(int MapId, int Stage);
}
