using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Core.Scripting.Events;
using Application.Core.Server.life;
using System.Drawing;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.CWKPQ
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 9201107 
        public async Task glpqstatue0()
        {
            if (getMapId() == 610030500)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201107_WarriorRoom));
            }
            else if (getMapId() == 610030000)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201107_DeVrisiens));
            }
            else if (getMapId() == 610030510)
            {
                if (getMap().countMonsters() == 0)
                {
                    var eim = GetEventInstanceTrust();
                    var stgStatus = eim.getIntProperty("glpq5_room");
                    var jobNiche = getPlayer().getJob().GetJobNiche();

                    if ((stgStatus >> jobNiche) % 2 == 0)
                    {
                        if (canHold(4001259, 1))
                        {
                            await gainItem(4001259, 1);
                            await SayOK(this.GetTalkMessage(ScriptMessage.Cwkpq_GoodJob));

                            stgStatus += (1 << jobNiche);
                            eim.setIntProperty("glpq5_room", stgStatus);
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Cwkpq_MakeRoomEtc));
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Cwkpq_WeaponAlreadyTaken));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201107_EliminateGuardians));
                }
            }
        }


        // Npc: 9201108 
        public async Task glpqstatue1()
        {
            if (getMapId() == 610030500)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201108_MasterGuardian));
            }
            else if (getMapId() == 610030000)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201108_Lockewood));
            }
            else if (getMapId() == 610030540)
            {
                if (getMap().countMonsters() == 0)
                {
                    var eim = GetEventInstanceTrust();
                    var stgStatus = eim.getIntProperty("glpq5_room");
                    var jobNiche = getPlayer().getJob().GetJobNiche();

                    if ((stgStatus >> jobNiche) % 2 == 0)
                    {
                        if (canHold(4001258, 1))
                        {
                            await gainItem(4001258, 1);
                            await SayOK(this.GetTalkMessage(ScriptMessage.Cwkpq_GoodJob));

                            stgStatus += (1 << jobNiche);
                            eim.setIntProperty("glpq5_room", stgStatus);
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Cwkpq_MakeRoomEtc));
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Cwkpq_WeaponAlreadyTaken));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201108_EliminateMasterGuardians));
                }
            }
        }


        // Npc: 9201109 
        public async Task glpqstatue2()
        {
            if (getMapId() == 610030500)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201109_MageChamber));
            }
            else if (getMapId() == 610030000)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201109_Rafael));
            }
            else if (getMapId() == 610030521)
            {
                if (getMap().countMonsters() == 0)
                {
                    var eim = GetEventInstanceTrust();
                    var stgStatus = eim.getIntProperty("glpq5_room");
                    var jobNiche = getPlayer().getJob().GetJobNiche();

                    if ((stgStatus >> jobNiche) % 2 == 0)
                    {
                        if (canHold(4001257, 1))
                        {
                            await gainItem(4001257, 1);
                            await SayOK(this.GetTalkMessage(ScriptMessage.Cwkpq_GoodJob));

                            stgStatus += (1 << jobNiche);
                            eim.setIntProperty("glpq5_room", stgStatus);
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Cwkpq_MakeRoomEtc));
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Cwkpq_WeaponAlreadyTaken));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201109_EliminateMonsters));
                }
            }
        }


        // Npc: 9201110 
        public async Task glpqstatue3()
        {
            switch (getMapId())
            {
                case 610030500:
                    await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201110_ThiefChamber));
                    break;
                case 610030000:
                    await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201110_GrandmasterRyo));
                    break;
                case 610030530:
                    if (isAllReactorState(6108004, 1))
                    {
                        var eim = GetEventInstanceTrust();
                        var stgStatus = eim.getIntProperty("glpq5_room");
                        var jobNiche = getPlayer().getJob().GetJobNiche();

                        if ((stgStatus >> jobNiche) % 2 == 0)
                        {
                            if (canHold(4001256, 1))
                            {
                                await gainItem(4001256, 1);
                                await SayOK(this.GetTalkMessage(ScriptMessage.Cwkpq_GoodJob));

                                stgStatus += (1 << jobNiche);
                                eim.setIntProperty("glpq5_room", stgStatus);
                            }
                            else
                            {
                                await SayOK(this.GetTalkMessage(ScriptMessage.Cwkpq_MakeRoomEtc));
                            }
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Cwkpq_WeaponAlreadyTaken));
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201110_DestroyWatchfulEyes));
                    }
                    break;
            }
        }


        // Npc: 9201111 
        public async Task glpqstatue4()
        {
            if (getMapId() == 610030500)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201111_PirateChamber));
            }
            else if (getMapId() == 610030000)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201111_SteelFistJack));
            }
        }


        // Npc: 9201112 
        public async Task glpqStory()
        {
            switch (getMapId())
            {
                case 610030500:
                    await SayNext(this.GetTalkMessage(ScriptMessage.CWKPQ_9201112_SurprisedYouMadeIt));
                    await SayNext(this.GetTalkMessage(ScriptMessage.CWKPQ_9201112_FiveRooms));
                    await SayNext(this.GetTalkMessage(ScriptMessage.CWKPQ_9201112_SuspectWeapons));
                    await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201112_BringBackWeapons));
                    break;
                case 610030700:
                    await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201112_GoodWork));
                    break;
            }
        }


        // Npc: 9201113 — 红树林要塞（CWKPQ）远征队入口
        public async Task glpqStart()
        {
            var player = getPlayer();
            var em = GetEventManager<ExpeditionEventManager>(CWKPQFeature.EventName);

            if (player.getLevel() < em.MinLevel || player.getLevel() > em.MaxLevel)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201113_NotEligible));
                return;
            }

            var expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();

            if (expedition == null)
            {
                var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.CWKPQ_9201113_CreatePrompt,
                    em?.Template?.GetRequirementDescription(c) ?? ""));
                if (sel == 1)
                {
                    expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
                    if (expedition != null)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_SomeoneIsLeader));
                        return;
                    }

                    var r = await em.StartInstance(player);
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201113_NotForEveryone));
                }
                return;
            }

            if (expedition.isLeader(player))
            {
                if (expedition.isInProgress())
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_AlreadyInProgress));
                    return;
                }

                var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.Exped_Menu));
                if (sel == 1)
                {
                    var members = expedition.GetPlayerSortList();
                    if (members.Count == 1)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_OnlyMember));
                        return;
                    }

                    var text = this.GetTalkMessage(ScriptMessage.CWKPQ_9201113_MemberListHeader);
                    text += "\r\n		1." + members[0].Name;
                    for (var i = 1; i < members.Count; i++)
                    {
                        text += $"\r\n#b#L{i + 1}#{i + 1}. {members[i].Name}#l\r\n";
                    }

                    var kick = await AskMenu(text);
                    if (kick > 0)
                    {
                        var banned = members[kick - 1];
                        await expedition.ban(banned.Id);
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_Banned, banned.Name));
                    }
                }
                else if (sel == 2)
                {
                    if (expedition.getPlayers().Count < expedition.EventManager.MinCount)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_NeedMinPlayers, expedition.EventManager.MinCount));
                        return;
                    }

                    await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201113_GoodLuck));
                    await expedition.StartBattle();
                }
                else if (sel == 3)
                {
                    await player.getMap().LightBlue(ScriptMessageCategories.ScriptMessage, ScriptMessage.CWKPQ_9201113_LeaderQuit, expedition.getLeader().getName());
                    await expedition.DisposeAsync();
                    await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_ExpeditionEnded));
                }
                return;
            }

            if (expedition.isRegistering())
            {
                if (expedition.contains(player))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_AlreadyRegistered, expedition.getLeader().getName()));
                }
                else
                {
                    await SayOK(em.GetTemplate.HandleJoinInstanceResult(await em.GetTemplate.JoinMember(expedition, player), c));
                }
                return;
            }

            if (expedition.isInProgress())
            {
                if (expedition.contains(player))
                {
                    if (expedition.getIntProperty("canJoin") == 1)
                    {
                        await expedition.registerPlayer(player);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201113_PrayForBraveSouls));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201113_AnotherExpeditionStarted));
                }
            }
        }


        // Npc: 9201114 
        public async Task glpqEnter()
        {
            if (haveItem(3992041, 1))
            {
                await warp(610030020, "out00");
            }
            else
            {
                await this.Pink(ScriptMessageCategories.ScriptMessage, ScriptMessage.CWKPQ_9201114_GateLocked);
            }
        }


        // Npc: 9201115 
        [ScriptTag(["CWK"])]
        public async Task glpqStory2()
        {
            var eim = GetEventInstanceTrust();
            if (eim.getIntProperty("glpq6") == 3)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.CWKPQ_9201115_WellPlayed));
                return;
            }

            if (!isEventLeader())
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.CWKPQ_9201115_WantLeader));
                return;
            }

            if (eim.getIntProperty("glpq6") == 0)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.CWKPQ_9201115_Welcome));
                await SayNext(this.GetTalkMessage(ScriptMessage.CWKPQ_9201115_Feast));
                await SayNext(this.GetTalkMessage(ScriptMessage.CWKPQ_9201115_Escort));
                await this.LightBlue(ScriptMessageCategories.ScriptMessage, ScriptMessage.CWKPQNpcHandler_EngardeMaster);
                for (var i = 0; i < 10; i++)
                {
                    var mob = LifeFactory.Instance.GetMonsterTrust(9400594);
                    var xPos = -1337 + Random.Shared.Next(1337);
                    await getMap().spawnMonsterOnGroundBelow(mob, new Point(xPos, 276));
                }
                for (var i = 0; i < 20; i++)
                {
                    var mob = LifeFactory.Instance.GetMonsterTrust(9400582);
                    var xPos = -1337 + Random.Shared.Next(1337);
                    await getMap().spawnMonsterOnGroundBelow(mob, new Point(xPos, 276));
                }
                eim.setIntProperty("glpq6", 1);
            }
            else if (eim.getIntProperty("glpq6") == 1)
            {
                if (getMap().countMonsters() == 0)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201115_WhatIsThis));
                    await SayNext(this.GetTalkMessage(ScriptMessage.CWKPQ_9201115_NoMatter));
                    await this.LightBlue(ScriptMessageCategories.ScriptMessage, ScriptMessage.CWKPQNpcHandler_TwistedMasters);

                    //Margana
                    var mob = LifeFactory.Instance.GetMonsterTrust(9400590);
                    await getMap().spawnMonsterOnGroundBelow(mob, new Point(-22, 1));

                    //Red Nirg
                    var mob2 = LifeFactory.Instance.GetMonsterTrust(9400591);
                    await getMap().spawnMonsterOnGroundBelow(mob2, new Point(-22, 276));

                    //Hsalf
                    var mob4 = LifeFactory.Instance.GetMonsterTrust(9400593);
                    await getMap().spawnMonsterOnGroundBelow(mob4, new Point(496, 276));

                    //Rellik
                    var mob3 = LifeFactory.Instance.GetMonsterTrust(9400592);
                    await getMap().spawnMonsterOnGroundBelow(mob3, new Point(-496, 276));

                    eim.setIntProperty("glpq6", 2);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201115_IgnoreMeGuardians));
                }
            }
            else if (eim.getIntProperty("glpq6") == 2)
            {
                if (getMap().countMonsters() == 0)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201115_CannotBe));
                    await this.Pink(ScriptMessageCategories.ScriptMessage, ScriptMessage.CWKPQNpcHandler_PortalNext);
                    eim.setIntProperty("glpq6", 3);

                    await eim.showClearEffect(true);
                    await eim.GiveStageClearRewardAll(6);

                    await eim.clearPQ();
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.CWKPQ_9201115_IgnoreMeMasters));
                }
            }
        }
    }
}
