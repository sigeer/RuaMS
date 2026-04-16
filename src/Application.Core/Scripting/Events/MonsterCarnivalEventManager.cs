using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Abstraction;
using Application.Resources.Messages;
using tools;

namespace Application.Core.Scripting.Events
{
    public abstract class MonsterCarnivalEventManager : PartyQuestEventManager
    {
        public MonsterCarnivalEventManager(WorldChannel cserv, string name) : base(cserv, name)
        {
            EventTime = 180;
        }

        protected override AbstractEventInstanceManager CreateNewInstance(string instanceName)
        {
            throw new Exception("CPQ内置模板创建，不再通过这个方法创建");
        }


        //public bool Check2(MonsterCarnivalEventInstanceManager eim)
        //{
        //    try
        //    {
        //        var t0 = iv.CallFunction("getEligibleParty", eim.Team0.EligibleMembers, eim.Room, 1).ToObject<List<Player>>() ?? [];
        //        var t1 = iv.CallFunction("getEligibleParty", eim.Team1.EligibleMembers, eim.Room, 1).ToObject<List<Player>>() ?? [];

        //        return t0.Count == t1.Count && t0.Count >= eim.Room.MinCount;
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex, "Script: {Script}", _name);
        //        return false;
        //    }
        //}


        public override CreateInstanceResult StartInstance(Player chr, int level = 1, int lobby = -1)
        {
            var roomName = lobby.ToString();
            if (!instances.TryGetValue(roomName, out var instance))
                return CreateInstanceResult.Unknown;

            if (instance is not MonsterCarnivalEventInstanceManager eim)
                return CreateInstanceResult.Unknown;

            if (chr.Party == 0)
                return CreateInstanceResult.RequiredParty;
            if (!chr.isLeader())
                return CreateInstanceResult.RequiredLeader;

            var members = GetEligibleParty(chr);
            if (members.Count == 0)
                return CreateInstanceResult.Requirement;

            if (eim.CurrentStage != MonsterCarnivalStage.None)
                return CreateInstanceResult.LobbyLimited;

            eim.EnterLobby(new TeamRegistry(chr.Party, members, null));
            eim.setLeader(chr);
            return CreateInstanceResult.Success;
        }

        public MCJoinInstanceResult JoinInstance(Player chr, int lobby)
        {
            var roomName = lobby.ToString();
            if (!instances.TryGetValue(roomName, out var instance))
                return MCJoinInstanceResult.Unknown;

            if (instance is not MonsterCarnivalEventInstanceManager eim)
                return MCJoinInstanceResult.Unknown;

            if (chr.Party == 0)
                return MCJoinInstanceResult.RequiredParty;

            if (!chr.isLeader())
                return MCJoinInstanceResult.RequiredLeader;

            var members = GetEligibleParty(chr);
            if (members.Count == 0)
                return MCJoinInstanceResult.Requirement;

            if (eim.CurrentStage != MonsterCarnivalStage.Waiting)
                return MCJoinInstanceResult.NotInWaiting;

            if (eim.Team1 != null)
                return MCJoinInstanceResult.AnthorRequest;

            eim.Team1 = new TeamRegistry(chr.Party, members, null);
            // send challenge
            if (ChannelServer.NodeService.PluginManager.StartNpcConversation(
                eim.getLeader()!.Client,
                2042001,
                null,
                "mc_enter1"))
            {
                return MCJoinInstanceResult.Success;
            }
            return MCJoinInstanceResult.AnthorRequest;
        }

        public override void OnPlayerUnregister(AbstractEventInstanceManager eim, Player chr)
        {
            base.OnPlayerUnregister(eim, chr);

            var mcEim = eim as MonsterCarnivalEventInstanceManager;
            if (chr.Party == mcEim.Team0?.Team)
            {
                mcEim.Team0.EligibleMembers.Remove(chr);
            }
            if (chr.Party == mcEim.Team1?.Team)
            {
                mcEim.Team1.EligibleMembers.Remove(chr);
            }

            chr.ClearMC();
        }

        protected override void respawnStages(AbstractEventInstanceManager eim)
        {
            var e = (eim as MonsterCarnivalEventInstanceManager);
            e.EventMap.instanceMapRespawn();
            if (e.CurrentStage == MonsterCarnivalStage.Battle)
            {
                eim.Schedule(respawnStages, 10 * 1000);
            }
        }

        public override void OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            var e = (eim as MonsterCarnivalEventInstanceManager);
            chr.changeMap(e.LobbyMap);
        }

        public override void OnPlayerMapChanging(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            if (mapid < MinMap || mapid > MaxMap)
            {
                if (!eim.isEventCleared())
                {
                    if (player.MCTeam != null)
                        eim.Pink(nameof(ClientMessage.CPQ_PlayerExit), player.MCTeam.TeamFlag == 0 ? "TeamRed" : "TeamBlue");
                    End(eim);
                }
            }
        }

        public override void OnPlayerMapChanged(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            base.OnPlayerMapChanged(eim, player, mapid);

            if (player.MCTeam != null)
                player.sendPacket(PacketCreator.startMonsterCarnival(player));
        }

        public override void OnPlayerDisconnected(AbstractEventInstanceManager eim, Player player)
        {
            if (!eim.isEventCleared())
            {
                if (player.MCTeam != null)
                    eim.Pink(nameof(ClientMessage.CPQ_PlayerExit), player.MCTeam.TeamFlag == 0 ? "TeamRed" : "TeamBlue");
                End(eim);
            }
        }

        public override void OnPlayerLeftParty(AbstractEventInstanceManager eim, Player player)
        {
            if (!eim.isEventCleared())
            {
                if (player.MCTeam != null)
                    eim.Pink(nameof(ClientMessage.CPQ_PlayerExit), player.MCTeam.TeamFlag == 0 ? "TeamRed" : "TeamBlue");
                End(eim);
            }
        }

        public override void OnPlayerDied(AbstractEventInstanceManager eim, Player chr)
        {
            base.OnPlayerDied(eim, chr);

            var e = eim as MonsterCarnivalEventInstanceManager;
            int losing = e.EventMap.DeathCP;
            if (chr.AvailableCP < losing)
            {
                losing = chr.AvailableCP;
            }
            chr.gainCP(-losing);
            e.EventMap.broadcastMessage(PacketCreator.CPQ_PlayerDied(chr.Name, losing, chr.MCTeam!.TeamFlag));
        }

        public override void OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            base.OnMobKilled(eim, mob, killer);

            if (mob.getCP() > 0 && killer is Player chr)
            {
                chr.gainCP(mob.getCP());
            }
        }

        public override void ClearPQ(AbstractEventInstanceManager eim)
        {
            eim.setEventCleared();
        }

        public override void OnTimeOut(AbstractEventInstanceManager eim)
        {
            var e = (eim as MonsterCarnivalEventInstanceManager);
            switch (e.CurrentStage)
            {
                case MonsterCarnivalStage.None:
                    break;
                case MonsterCarnivalStage.Waiting:
                    End(eim);
                    break;
                case MonsterCarnivalStage.Matched:
                    // 再次检测
                    if (true)
                    {
                        eim.restartEventTimer(e.EventMap.TimeDefault * 1000 - 10000);
                        eim.startEvent();
                        respawnStages(eim);
                    }
                    else
                    {
                        End(eim);
                    }
                    break;
                case MonsterCarnivalStage.Battle:
                    if (!e.Complete())
                    {
                        e.Pink(nameof(ClientMessage.CPQ_ExtendTime));
                        e.restartEventTimer(e.EventMap.TimeExpand * 1000);
                    }
                    else
                    {
                        eim.Schedule(ClearPQ, 10_000);
                    }
                    break;
                case MonsterCarnivalStage.Completed:
                    break;
                default:
                    break;
            }
        }

        public override string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    return c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.CPQ_EntryLobby));
                case CreateInstanceResult.RequiredParty:
                    return "在你加入战斗之前，你需要先创建一个队伍！";
                case CreateInstanceResult.RequiredLeader:
                    return "如果你想开始战斗，让#b队长#k和我对话。";
                case CreateInstanceResult.Requirement:
                    return "队伍不满足条件。";
                case CreateInstanceResult.LobbyLimited:
                case CreateInstanceResult.Disposed:
                case CreateInstanceResult.Unknown:
                default:
                    return c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.CPQ_Error));
            }
        }
        public string? HandleJoinInstanceResult(MCJoinInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case MCJoinInstanceResult.Success:
                    return c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.CPQ_ChallengeRoomSent));

                case MCJoinInstanceResult.RequiredParty:
                    return "在你加入战斗之前，你需要先创建一个队伍！";

                case MCJoinInstanceResult.RequiredLeader:
                    return "如果你想开始战斗，让#b队长#k和我对话。";

                case MCJoinInstanceResult.Requirement:
                    return "队伍不满足条件。需要与被挑战的队伍人数一致！";

                case MCJoinInstanceResult.NotInWaiting:
                    return c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.CPQ_FindError));

                case MCJoinInstanceResult.AnthorRequest:
                    return c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.CPQ_ChallengeRoomAnswer));
                case MCJoinInstanceResult.Unknown:

                default:
                    return c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.CPQ_Error));
            }
        }
    }
}
