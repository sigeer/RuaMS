using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Shared.Constants.Map;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.PQ_Ariant
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 2101014
        public async Task aMatchEnt()
        {
            if (getPlayer().getMapId() == MapId.ARPQ_LOBBY)
            {
                var allAriant = Enumerable.Range(0, 3).ToDictionary(x => x, x => GetEventManager<AriantEventManager>(PQ_AriantFeature.EventNames[x]));
                var em = allAriant[0];

                if (getPlayer().getLevel() < em.MinLevel || getPlayer().getLevel() > em.MaxLevel)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Ariant_AlreadyOver));
                    return;
                }
                var allAriantKeys = allAriant.Keys.ToArray();
                foreach (var k in allAriantKeys)
                {
                    var eim = allAriant[k].GetOnlyEventInstanceManager<AriantEventInstanceManager>();
                    if (eim != null && eim.InstanceStatus != Core.scripting.Events.Abstraction.InstanceStatus.Recruitment)
                    {
                        allAriant.Remove(k);
                    }
                }

                if (allAriant.Count == 0)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.MC_AllBattle));
                    return;
                }

                var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Ariant_BattleArenaMenu),
                allAriant.ToDictionary(x => x.Key, x =>
                {
                    var eim = x.Value.GetOnlyEventInstanceManager<AriantEventInstanceManager>();
                    if (eim == null)
                    {
                        return $"Battle Arena ({x.Key + 1}) (Empty)";
                    }
                    else if (eim.isRegistering())
                    {
                        return this.GetTalkMessage(ScriptMessage.Ariant_BattleArenaEntry, x.Key + 1, eim.getLeader()!.Name);
                    }

                    return "";
                }));

                em = allAriant[option];
                var selectedExped = em.GetOnlyEventInstanceManager<AriantEventInstanceManager>();

                if (selectedExped == null)
                {
                    var inputNumber = await AskNumber(this.GetTalkMessage(ScriptMessage.Ariant_MaxParticipants, em.MinCount, em.MaxCount), em.MinCount, em.MinCount, em.MaxCount);
                    selectedExped = em.GetOnlyEventInstanceManager<AriantEventInstanceManager>();
                    if (selectedExped != null)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Ariant_AlreadySomeone));
                        return;
                    }

                    var r = await em.StartInstance(getPlayer());
                    if (r == Core.scripting.Events.Abstraction.CreateInstanceResult.Success)
                    {
                        selectedExped = em.GetOnlyEventInstanceManager<AriantEventInstanceManager>();

                        selectedExped.MaxCount = inputNumber;
                        await getPlayer().Notice(this.GetTalkMessage(ScriptMessage.Ariant_ArenaCreated));
                    }
                    else
                    {
                        await SayOK(em.HandleCreateInstanceResult(r, c));
                    }
                }
                else if (selectedExped.isRegistering())
                {
                    if (selectedExped.contains(getPlayer()))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Ariant_SorryYoure));
                        return;
                    }

                    var playerAdd = await em.GetTemplate.JoinMember(selectedExped, getPlayer());
                    await SayOK(em.GetTemplate.HandleJoinInstanceResult(playerAdd, c));
                }
            }
        }

        // Npc: 2101015
        public async Task aMatchScore()
        {
            switch (await AskMenu(this.GetTalkMessage(ScriptMessage.Ariant_Text, 
                [this.GetTalkMessage(ScriptMessage.Ariant_LikeCheck), this.GetTalkMessage(ScriptMessage.Ariant_LikeKnow)])))
            {
                case 0:
                    var apqpoints = getPlayer().AriantPoints;
                    if (apqpoints >= 100)
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Ariant_WowLooks));

                        getPlayer().AriantPoints -= 100;
                        await gainItem(3010018, 1);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Ariant_ScoreTooLow, apqpoints));
                        return;
                    }
                    break;
                case 1:
                    await SayOK(this.GetTalkMessage(ScriptMessage.Ariant_MainObjective));
                    break;
                default:
                    break;
            }
        }

        // Npc: 2101016
        public async Task aMatchRwd()
        {
            var eim = GetEventInstanceTrust<AriantEventInstanceManager>();
            var copns = eim.getAriantScore(getPlayer());
            if (copns < 1)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Ariant_TooBad));
                return;
            }
            var points = eim.getAriantRewardTier(getPlayer());
            await SayNext(this.GetTalkMessage(ScriptMessage.Ariant_RewardGranted, copns, points));

            var r = await eim.GiveClearReward(getPlayer(), points);
            if (r == ClaimRewardResult.Success)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Ariant_AlrightMake));
            }

        }

        // Npc: 2101017
        public async Task aMatchPlay()
        {
            var eim = GetEventInstanceTrust<AriantEventInstanceManager>();
            if (eim.isRegistering())
            {
                if (eim.isEventLeader(getPlayer()))
                {
                    switch (await AskMenu(this.GetTalkMessage(ScriptMessage.A30_WantHmmm), [
                        this.GetTalkMessage(ScriptMessage.Ariant_ViewCurrent), 
                        this.GetTalkMessage(ScriptMessage.Ariant_BanMembers), 
                        this.GetTalkMessage(ScriptMessage.Ariant_StartBattle), 
                        this.GetTalkMessage(ScriptMessage.Ariant_LeaveArena)]))
                    {
                        case 0:
                            await SayOK(string.Join("\r\n", eim.getPlayers().Select(x => x.Name)));
                            break;
                        case 1:
                            var members = eim.GetPlayerSortList();
                            var selected = await AskMenu(this.GetTalkMessage(ScriptMessage.Ariant_MemberList, members[0].Name), members.Skip(1).Select((x, idx) => $"{idx + 2}. {x.Name}")) + 1;
                            await eim.ban(members[selected].Id);
                            await SayOK(this.GetTalkMessage(ScriptMessage.Ariant_MemberBanned, members[selected].Name));
                            break;
                        case 2:
                            await eim.StartBattle();
                            break;
                        case 4:
                            await WarpOut();
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Ariant_HaveHeard));
                }
            }
            else
            {
                var gotTheBombs = eim.getProperty("gotBomb" + getChar().getId());
                if (gotTheBombs != null)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Ariant_AlreadyGave));
                }
                else if (canHoldAll([2270002, 2100067], [50, 5]))
                {
                    eim.setProperty("gotBomb" + getChar().getId(), "1");
                    await gainItem(2270002, 50);
                    await gainItem(2100067, 5);

                    await SayOK(this.GetTalkMessage(ScriptMessage.Ariant_HaveGiven));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Ariant_SeemsInventory));
                }
            }
        }

        // Npc: 2101018
        public async Task aMatchMove()
        {
            await SaySpeech([
            this.GetTalkMessage(ScriptMessage.Ariant_HavePrepared),
            this.GetTalkMessage(ScriptMessage.Ariant_AriantColiseum)

            ]);
            await AskMenu(this.GetTalkMessage(ScriptMessage.Ariant_StrongBrave));
            await SayNext(this.GetTalkMessage(ScriptMessage.Ariant_IllSend));
            getPlayer().SaveLocation(Shared.MapObjects.SavedLocationType.MIRROR);
            await warp(MapId.ARPQ_LOBBY, 3);
        }
    }
}
