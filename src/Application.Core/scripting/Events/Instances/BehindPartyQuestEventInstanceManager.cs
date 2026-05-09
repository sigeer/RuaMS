using Application.Core.Channel;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Scripting.Events;
using tools;

namespace Application.Core.scripting.Events.Instances
{
    public abstract class BehindPartyQuestEventInstanceManager : AbstractEventInstanceManager
    {
        public override BehindPartyQuestEventManager EventManager => (ChannelServer.getEventSM().getEventManager(EventName) as BehindPartyQuestEventManager)!;
        public BehindPartyQuestEventInstanceManager(WorldChannel worldChannel, string emName, string name) : base(worldChannel, emName, name)
        {
        }


        #region Expedition

        public Dictionary<int, string> Banned { get; } = new();

        /// <summary>
        /// 开始战斗
        /// </summary>
        public void StartBattle()
        {
            var em = (ChannelServer.getEventSM().getEventManager(EventName) as BehindPartyQuestEventManager)!;

            if (InstanceStatus == InstanceStatus.Recruitment && em.PrepareTime > 0)
            {
                InstanceStatus = InstanceStatus.Prepare;

                restartEventTimer(em.PrepareTime * 1000);
                em.OnBattlePrepare(this);
                return;
            }

            if (InstanceStatus == InstanceStatus.Recruitment || InstanceStatus == InstanceStatus.Prepare)
            {
                InstanceStatus = Abstraction.InstanceStatus.InProgress;

                foreach (var chr in getPlayers())
                {
                    em.OnPlayerEntry(this, chr);
                }
                restartEventTimer(em.EventTime * 1000);
                em.OnBattleStarted(this);
                return;
            }
        }

        public virtual void ban(int cid)
        {
            if (chars.TryGetValue(cid, out var chr))
            {
                Banned[cid] = chr.Name;

                exitPlayer(chr);

                EventManager.OnPlayerBanned(this, chr);
            }
            else
            {
                Banned[cid] = "";
            }
        }

        public bool contains(Player player)
        {
            return chars.ContainsKey(player.getId());
        }

        public bool isRegistering()
        {
            return InstanceStatus == Abstraction.InstanceStatus.Recruitment;
        }

        public bool isInProgress()
        {
            return InstanceStatus == Abstraction.InstanceStatus.InProgress;
        }

        #endregion
    }
}
