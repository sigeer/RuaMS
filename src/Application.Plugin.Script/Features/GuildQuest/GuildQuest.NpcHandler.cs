using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Core.Scripting.Events;
using Application.Core.Server.maps;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.GuildQuest
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 9040000 — Guild Quest entrance / queue / join lobby
        public async Task guildquest1_enter()
        {
            var em = GetEventManager<GuildQuestEventManager>(GuildQuestFeature.EventName);
            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.GuildQuest_EntryMenu));
            if (sel == 0)
            {
                if (!isGuildLeader())
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_NeedMaster));
                    return;
                }

                if (em.isQueueFull())
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_QueueFull));
                    return;
                }

                if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.GuildQuest_ConfirmJoinQueue, em.QueueSize)))
                {
                    return;
                }

                var entry = await em.addGuildToQueue(getPlayer().getGuildId(), getPlayer().getId());
                if (entry > 0)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Registered));
                }
                else if (entry == 0)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_QueueFull));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_AlreadyQueued));
                }
            }
            else if (sel == 1)
            {
                if (getPlayer().getGuildId() <= 0)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_NotInGuild));
                    return;
                }

                var eim = findGuildLobby(em, getPlayer().getGuildId());
                if (eim == null)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_NotStrategyTime));
                    return;
                }

                if (isLeader())
                {
                    var members = getPlayer().getPartyMembersOnline();
                    if (members.Count > 0)
                        await eim.registerParty(members);
                    else
                        await eim.registerPlayer(getPlayer());
                }
                else
                {
                    await eim.registerPlayer(getPlayer());
                }
            }
            else
            {
                var reqStr = this.GetTalkMessage(ScriptMessage.GuildQuest_Requirements);
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Intro));
            }
        }

        private static AbstractEventInstanceManager? findGuildLobby(GuildQuestEventManager em, int guildId)
        {
            foreach (var lobby in em.getInstances())
            {
                if (lobby.getIntProperty("guild") == guildId && lobby.getIntProperty("canJoin") == 1)
                    return lobby;
            }
            return null;
        }


        // Npc: 9040001 — Guild Quest exit / reward NPC
        public async Task guildquest1_clear()
        {
            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.GuildQuest_FinishedExploring)))
                return;

            var eim = getEventInstance();
            if (eim != null && eim.isEventCleared())
            {
                var result = await eim.GiveClearReward(getPlayer());
                if (result != ClaimRewardResult.Success)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.GuildQuest_NoFreeSlot));
                }
                else
                {
                    await warp(101030104);
                }
            }
            else
            {
                await warp(101030104);
            }
        }


        // Npc: 9040002 — Guild Quest info NPC (Shawn)
        public async Task guildquest1_comment()
        {
            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.GuildQuest_Shawn_Prompt));
            if (sel == 0)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.GuildQuest_Shawn_Sharenian));
                await SayNext(this.GetTalkMessage(ScriptMessage.GuildQuest_Shawn_LastKing));
            }
            else if (sel == 1)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Shawn_Rubian));
            }
            else if (sel == 2)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.GuildQuest_Shawn_Expeditions));
                await SayNext(this.GetTalkMessage(ScriptMessage.GuildQuest_Shawn_Goal));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Shawn_AskMore));
            }
        }


        // Npc: 9040003 — Sharen III's Soul (end of stage 4)
        public async Task guildquest1_NPC1()
        {
            var eim = getEventInstance();
            if (eim?.getProperty("stage4clear") == "true")
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_SharenSoul_Rest));
                return;
            }

            if (!isEventLeader())
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_NeedPartyLeader));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.GuildQuest_SharenSoul_PaveWay));
            await eim!.showClearEffect(true);
            // eim.giveEventPlayersStageReward(4) — not available in C# API
            await GainGuildGP(30);
            getMap().getReactorByName("ghostgate")?.forceHitReactor(1);
        }


        // Npc: 9040005 — Guild Quest leave NPC
        public async Task guildquest1_out()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.GuildQuest_ConfirmExit)))
            {
                await warp(101030104);
            }
        }


        // Npc: 9040006 — Fountain puzzle (stage 3)
        public async Task guildquest1_baseball()
        {
            if (getMap().getReactorByName("watergate")?.getState() > 0)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_StageClear));
                return;
            }

            var eim = getEventInstance();
            if (eim == null)
            {
                await warp(990001100);
                return;
            }

            if (!isEventLeader())
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_NeedLeader));
                return;
            }

            var currentCombo = eim.getProperty("stage3combo");
            if (currentCombo == null || currentCombo == "reset")
            {
                var newCombo = makeStage3Combo();
                eim.setProperty("stage3combo", "" + newCombo);
                eim.setProperty("stage3attempt", "1");
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Fountain_Intro));
                return;
            }

            var attempt = int.Parse(eim.getProperty("stage3attempt")!);
            var combo = int.Parse(currentCombo);
            var guess = getGroundStage3Items();
            if (guess == null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Fountain_Setup));
                return;
            }

            if (combo == guess.Value)
            {
                getMap().getReactorByName("watergate")?.forceHitReactor(1);
                await eim.showClearEffect(true);
                await GainGuildGP(25);
                await removeStage3Items();
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_StageClear));
            }
            else
            {
                if (attempt < 7)
                {
                    var comboArr = new int[4];
                    var guessArr = new int[4];
                    var correct = 0;
                    var unknown = 0;

                    for (var i = 0; i < 4; i++)
                    {
                        var guessIdx = (guess.Value / (int)Math.Pow(10, i)) % 10;
                        var comboIdx = (combo / (int)Math.Pow(10, i)) % 10;
                        if (guessIdx == comboIdx)
                            correct++;
                        else
                        {
                            guessArr[guessIdx]++;
                            comboArr[comboIdx]++;
                        }
                    }

                    for (var i = 0; i < 4; i++)
                    {
                        var diff = guessArr[i] - comboArr[i];
                        if (diff > 0)
                            unknown += diff;
                    }

                    var incorrect = 4 - correct - unknown;
                    var result = "";
                    if (correct > 0)
                        result += this.GetTalkMessage(ScriptMessage.GuildQuest_Satisfied, correct);
                    if (incorrect > 0)
                        result += this.GetTalkMessage(ScriptMessage.GuildQuest_WrongOffering, incorrect);
                    if (unknown > 0)
                        result += this.GetTalkMessage(ScriptMessage.GuildQuest_UnknownOffering, unknown);

                    var ord = attempt switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
                    result += this.GetTalkMessage(ScriptMessage.GuildQuest_AttemptNo, attempt);

                    await spawnMobInMap(9300036, randX(), 150);
                    await spawnMobInMap(9300037, 400, 150);

                    await SayOK(result);
                    eim.setProperty("stage3attempt", "" + (attempt + 1));
                }
                else
                {
                    eim.setProperty("stage3combo", "reset");
                    await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Fountain_Failed));
                    for (var i = 0; i < 6; i++)
                    {
                        await spawnMobInMap(9300036, randX(), 150);
                        await spawnMobInMap(9300037, randX(), 150);
                    }
                }

                await eim.showWrongEffect();
            }
        }

        private int makeStage3Combo()
        {
            var combo = 0;
            for (var i = 0; i < 4; i++)
                combo += (Random.Shared.Next(4) * (int)Math.Pow(10, i));
            return combo;
        }

        private int? getGroundStage3Items()
        {
            var map = getMap();
            var items = map.getItems();
            if (items.Count != 4)
                return null;

            var itemInArea = new[] { -1, -1, -1, -1 };
            foreach (var item in items)
            {
                var id = item.getItemId();
                if (id < 4001027 || id > 4001030)
                    continue;

                for (var i = 0; i < 4; i++)
                {
                    if (map.getArea(i).Contains(item.getPosition()))
                    {
                        itemInArea[i] = id - 4001027;
                        break;
                    }
                }
            }

            if (itemInArea.Any(x => x == -1))
                return null;

            return itemInArea[0] * 1000 + itemInArea[1] * 100 + itemInArea[2] * 10 + itemInArea[3];
        }

        private async Task removeStage3Items()
        {
            var map = getMap();
            foreach (var item in map.getItems())
            {
                var id = item.getItemId();
                if (id >= 4001027 && id <= 4001030)
                    await map.makeDisappearItemFromMap(item);
            }
        }

        private async Task spawnMobInMap(int mobId, int x, int y)
        {
            await getMap().spawnMonsterOnGroundBelow(mobId, x, y);
        }

        private static int randX()
        {
            return -350 + Random.Shared.Next(750);
        }


        // Npc: 9040007 — Sharen III's message (stage 4 door)
        public async Task guildquest1_will()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_SharenMessage));
        }


        // Npc: 9040009 — Statue puzzle (stage 1)
        public async Task guildquest1_statue()
        {
            var eim = getEventInstance();
            if (eim == null)
            {
                await warp(990001100);
                return;
            }

            if (eim.getProperty("stage1clear") == "true")
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_StageClear));
                return;
            }

            if (!isEventLeader())
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Statue_NeedLeader));
                return;
            }

            var status = eim.getProperty("stage1status");
            if (status == null || status == "waiting")
            {
                var phase = eim.getProperty("stage1phase");
                var stage = phase == null ? 1 : int.Parse(phase);

                if (stage == 1)
                    await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Statue_Intro));
                else
                    await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Statue_Harder));

                eim.setProperty("stage1phase", "" + stage);
                eim.setProperty("stage1status", "display");
                eim.setProperty("stage1combo", "");

                var reactors = getStage1Reactors();
                var combo = makeStage1Combo(reactors, stage);
                await this.Pink(this.GetTalkMessage(ScriptMessage.GuildQuest_ShowingCombo));
                var delay = 5000;
                for (var i = 0; i < combo.Count; i++)
                {
                    await getMap().getReactorByOid(combo[i])!.delayedHitReactor(getClient(), delay + 3500 * i);
                }
            }
            else if (status == "active")
            {
                var stage = int.Parse(eim.getProperty("stage1phase")!);
                if (eim.getProperty("stage1combo") == eim.getProperty("stage1guess"))
                {
                    if (stage == 3)
                    {
                        getMap().getReactorByName("statuegate")?.forceHitReactor(1);
                        await eim.showClearEffect(true);
                        await GainGuildGP(15);
                        await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_StageClear));
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Statue_More));
                        eim.setProperty("stage1phase", "" + (stage + 1));
                        await this.Pink(this.GetTalkMessage(ScriptMessage.GuildQuest_StatuePartDone, stage.ToString()));
                    }
                    eim.setProperty("stage1status", "waiting");
                }
                else
                {
                    await eim.showWrongEffect();
                    await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Statue_Failed));
                    await this.Pink(this.GetTalkMessage(ScriptMessage.GuildQuest_StatueFailedMsg));
                    eim.setProperty("stage1phase", "1");
                    eim.setProperty("stage1status", "waiting");
                }
            }
            else if (status == "display")
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Statue_Working));
            }
        }

        private List<int> getStage1Reactors()
        {
            var reactors = new List<int>();
            foreach (var mo in getMap().getReactors().OfType<Reactor>())
            {
                if (mo.getName() != "statuegate")
                    reactors.Add(mo.getObjectId());
            }
            return reactors;
        }

        private static List<int> makeStage1Combo(List<int> reactors, int stage)
        {
            var combo = new List<int>();
            while (combo.Count < stage + 3)
            {
                var chosen = reactors[Random.Shared.Next(reactors.Count)];
                if (!combo.Contains(chosen))
                    combo.Add(chosen);
            }
            return combo;
        }


        // Npc: 9040010 — Boss clear / Rubian hand-in
        public async Task guildquest1_bonus()
        {
            var eim = getEventInstance();
            if (eim == null)
            {
                await warp(990001100);
                return;
            }

            if (!isEventLeader())
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_FinalChallengeLeader));
                return;
            }

            if (!haveItem(4001024))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_FinalChallenge));
                return;
            }

            await removeAll(4001024);
            var prev = eim.setProperty("bossclear", "true", true);
            if (prev == null)
            {
                var start = long.Parse(eim.getProperty("entryTimestamp")!);
                var diff = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start;
                var points = 1000 - (int)(diff / (100 * 60));
                if (points < 100)
                    points = 100;
                await GainGuildGP(points);
            }

            await eim.clearPQ();
        }


        // Npc: 9040011 — Guild Quest info board
        public async Task guildquest1_board()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Notice));
        }


        // Npc: 9040012 — Armor statue plaque (stage 2 info)
        public async Task guildquest1_knight()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.GuildQuest_Plaque));
        }
    }
}
