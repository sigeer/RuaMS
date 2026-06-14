using Application.Core.scripting.Events.Abstraction;
using Application.Core.Scripting.Events;

namespace Application.Core.scripting.Events.Instances
{
    public abstract class BehindPartyQuestEventInstanceManager : AbstractEventInstanceManager
    {
        public override BehindPartyQuestEventManager EventManager { get; }
        public BehindPartyQuestEventInstanceManager(BehindPartyQuestEventManager em, string name) : base(em, name)
        {
            EventManager = em;
        }


        #region Expedition

        public Dictionary<int, string> Banned { get; } = new();

        /// <summary>
        /// 开始战斗
        /// </summary>
        public void StartBattle()
        {
            if (InstanceStatus == InstanceStatus.Recruitment && EventManager.PrepareTime > 0)
            {
                InstanceStatus = InstanceStatus.Prepare;

                restartEventTimer(EventManager.PrepareTime * 1000);
                EventManager.GetTemplate.OnBattlePrepare(this);
                return;
            }

            if (InstanceStatus == InstanceStatus.Recruitment || InstanceStatus == InstanceStatus.Prepare)
            {
                InstanceStatus = Abstraction.InstanceStatus.InProgress;

                foreach (var chr in getPlayers())
                {
                    EventManager.Template.OnPlayerEntry(this, chr);
                }
                restartEventTimer(EventManager.EventTime * 1000);
                EventManager.GetTemplate.OnBattleStarted(this);
                return;
            }
        }

        public virtual void ban(int cid)
        {
            if (chars.TryGetValue(cid, out var chr))
            {
                Banned[cid] = chr.Name;

                exitPlayer(chr);

                EventManager.GetTemplate.OnPlayerBanned(this, chr);
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
