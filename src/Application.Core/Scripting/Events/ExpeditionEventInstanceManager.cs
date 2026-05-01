using Application.Core.Channel;
using Application.Resources.Messages;
using tools;

namespace Application.Core.Scripting.Events
{
    public class ExpeditionEventInstanceManager : AbstractEventInstanceManager
    {
        public ExpeditionEventInstanceManager(WorldChannel worldChannel, string emName, string name) : base(worldChannel, emName, name)
        {
            Type = Shared.Events.EventInstanceType.Expedition;
        }


        #region Expedition
        bool registering = false;
        Dictionary<int, string> banned = new();

        public void finishRegistration()
        {
            registering = false;
        }

        public void StartBattle()
        {
            finishRegistration();
            broadcastExped(PacketCreator.removeClock());
            LightBlue(nameof(ClientMessage.Expedition_Start));

            restartEventTimer(EventManager.EventTime);
            ChannelServer.NodeActor
                .Send(s =>
                {
                    s.SendDropMessage(6, "[Expedition] " + EventName + " Expedition started with leader: " + getLeader().getName(), true);
                });
        }


        public string JoinMember(Player player)
        {
            if (!registering)
            {
                return "Sorry, this expedition is already underway. Registration is closed!";
            }

            if (banned.ContainsKey(player.getId()))
            {
                return "Sorry, you've been banned from this expedition by #b" + getLeader().getName() + "#k.";
            }

            if (chars.Count >= EventManager.MaxCount)
            {
                return player.GetMessageByKey(nameof(ClientMessage.Expedition_MemberFull));
            }

            int channel = ChannelServer.getId();

            if (!ChannelServer.NodeService.ExpeditionService.CanStartExpedition(player.getId(), channel, EventName))
            {
                // thanks Conrad, Cato for noticing some expeditions have entry limit
                return "Sorry, you've already reached the quota of attempts for this expedition! Try again another day...";
            }

            registerPlayer(player);

            player.sendPacket(PacketCreator.getClock((int)(getTimeLeft() / 1000)));
            LightBlue(nameof(ClientMessage.Expedition_Join), player.Name);
            return "You have registered for the expedition successfully!";
        }


        private void broadcastExped(Packet packet)
        {
            foreach (Player chr in getPlayers())
            {
                chr.sendPacket(packet);
            }
        }


        public void ban(int cid)
        {
            if (chars.TryGetValue(cid, out var chr))
            {
                banned[cid] = chr.Name;

                unregisterPlayer(chr);
                chr.sendPacket(PacketCreator.removeClock());
                chr.Notice("[Expedition] You have been banned from this expedition.");

                broadcastExped(PacketCreator.serverNotice(6, "[Expedition] " + chr.Name + " has been banned from the expedition."));
            }
            else
            {
                banned[cid] = "";
            }
        }

        //public void monsterKilled(Player chr, Monster mob)
        //{
        //    foreach (int expeditionBoss in EXPEDITION_BOSSES)
        //    {
        //        if (mob.getId() == expeditionBoss)
        //        {
        //            bossLogs.Add(">" + mob.getName() + " was killed after " + TimeUtils.GetTimeString(startTime) + " - " + leader.Client.CurrentServer.Node.GetCurrentTimeDateTimeOffset().ToString("HH:mm:ss") + "\r\n");
        //            return;
        //        }
        //    }
        //}


        public bool contains(Player player)
        {
            return chars.ContainsKey(player.getId());
        }

        public bool isRegistering()
        {
            return registering;
        }

        public bool isInProgress()
        {
            return !registering;
        }

        #endregion
    }
}
