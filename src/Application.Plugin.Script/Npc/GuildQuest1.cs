using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using server.maps;


namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 9040000 — Guild Quest entrance / queue / join lobby
        public async Task guildquest1_enter()
        {
            var em = getEventManager("GuildQuest");
            if (em is not GuildQuestEventManager gqEm)
            {
                await SayOK("公会任务遇到了一个错误。");
                return;
            }

            var sel = await AskMenu("#e#b<公会任务：沙连尼安遗迹>\r\n#k#n\r\n通往沙连尼安的道路就在这里。你想做什么？#b\r\n#L0#注册公会进行公会任务#l\r\n#L1#加入你的公会的公会任务#l\r\n#L2#我想了解更多细节。#l");
            if (sel == 0)
            {
                if (!isGuildLeader())
                {
                    await SayOK("你的公会会长/副会长必须与我交谈，以注册公会参加公会任务。");
                    return;
                }

                if (gqEm.isQueueFull())
                {
                    await SayOK("这个频道的队列已经满了。请耐心等待一会儿，然后再试一次，或者尝试在另一个频道。");
                    return;
                }

                if (!await AskYesNo("你希望你的公会加入这个队列吗？"))
                {
                    return;
                }

                var entry = await gqEm.addGuildToQueue(getPlayer().getGuildId(), getPlayer().getId());
                if (entry > 0)
                {
                    await SayOK("您的公会已成功注册。一条消息将在您的聊天窗口中弹出，让您的公会了解注册状态。\r\n现在，#r重要#k：作为这个实例的队长者，#r在公会被召集进行策略时，您必须已经在该频道上#k。#b未能完成此操作将使#k您的公会注册作废，并立即召集下一个公会。还必须注意的是，如果您，作为这个实例的队长者，从策略时间结束到实例持续时间的任何时刻都不在场，将使任务中断，并立即将您的公会移出队列。");
                }
                else if (entry == 0)
                {
                    await SayOK("这个频道的队列已经满了。请耐心等待一会儿，然后再试一次，或者尝试在另一个频道。");
                }
                else
                {
                    await SayOK("您的公会已经在一个频道排队了。请等待您的公会轮到。");
                }
            }
            else if (sel == 1)
            {
                if (getPlayer().getGuildId() <= 0)
                {
                    await SayOK("如果你没有加入公会，就无法参与公会任务！");
                    return;
                }

                var eim = findGuildLobby(gqEm, getPlayer().getGuildId());
                if (eim == null)
                {
                    await SayOK("你的公会目前不在此频道上进行战略时间。请再次检查你的公会是否正在计划公会任务，如果是的话，请确认他们分配的频道。");
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
                var reqStr = "\r\n\r\n    队伍要求：\r\n\r\n"
                    + "     - 1 名 #r等级小于等于30级#k 的队员。\r\n"
                    + "     - 1 名拥有 #r隐身的飞侠#k 技能和 #r满级速度激发#k 的队员。\r\n"
                    + "     - 1 名拥有 #r满级瞬间移动#k 的法师队员。\r\n"
                    + "     - 1 名 #r远程攻击者#k，如弓箭手、刺客或枪手。\r\n"
                    + "     - 1 名拥有 #r良好跳跃技能#k 的队员，如满级飞叶的刺客或拥有翅膀的枪手。";
                await SayOK("#e#b<公会任务：沙连尼安遗迹>#k#n\r\n与你的公会成员一起合作，试图从骷髅的掌控中夺回鲁比安，通过团队合作克服沙连尼安墓穴内等待的许多谜题和挑战。完成任务实例后可以获得丰厚的奖励，并为你的公会积累公会点数。" + reqStr);
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
            if (!await AskYesNo("看来你已经探索完沙连尼安了，是吗？现在要回到招募地图吗？"))
                return;

            var eim = getEventInstance();
            if (eim != null && eim.isEventCleared())
            {
                var result = await eim.GiveClearReward(getPlayer());
                if (result != ClaimRewardResult.Success)
                {
                    await SayNext("看起来你的#r装备#k、#r消耗#k或#r其他#k背包中都没有空位。请先腾出一些空间。");
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
            var sel = await AskMenu("我们，公会联盟，一直在努力解读'翡翠石板'，一件珍贵的古老遗物，已经很久了。因此，我们发现沙连尼安，那个神秘的远古国度，沉睡在这里。我们还发现#t4001024#的线索，一件传说中的神话珠宝，可能就在沙连尼安的遗迹中。这就是为什么公会联盟开启公会任务，最终寻找#t4001024#。\r\n#b#L0# 沙连尼安是什么？#l\r\n#b#L1# #t4001024#？那是什么？#l\r\n#b#L2# 公会任务？#l\r\n#b#L3# 不，我现在没事了。#l");
            if (sel == 0)
            {
                await SayNext("沙连尼安是过去一个有控制金银岛每个地区的文明。魔像神殿，地牢深处的神殿，以及其他无人知晓建造者的古老建筑都是在沙连尼安时期建造的。");
                await SayNext("莎蕾尼安的最后一位国王是一位名叫莎蕾尼三世的绅士，显然他是一位非常睿智和富有同情心的国王。但有一天，整个王国崩溃了，对此没有任何解释。");
            }
            else if (sel == 1)
            {
                await SayOK("#t4001024# 是一颗传奇宝石，拥有它的人将获得永恒的青春。讽刺的是，似乎每个拥有 #t4001024# 的人最终都沦为了落魄之人，这或许可以解释夏雷尼安的衰落。");
            }
            else if (sel == 2)
            {
                await SayNext("我之前曾经派遣过一些探险者前往夏利安，但他们没有一个回来，这促使我们开始公会任务。我们一直在等待足够强大的公会来应对艰难的挑战，像你们这样的公会。");
                await SayNext("这个公会任务的最终目标是探索夏雷尼安并找到#t4001024#。这不是一个靠力量解决一切的任务。团队合作在这里更加重要。");
            }
            else
            {
                await SayOK("真的吗？如果你还有其他问题要问，随时都可以和我交谈。");
            }
        }


        // Npc: 9040003 — Sharen III's Soul (end of stage 4)
        public async Task guildquest1_NPC1()
        {
            var eim = getEventInstance();
            if (eim?.getProperty("stage4clear") == "true")
            {
                await SayOK("在我以为会是永恒的沉睡之后，我终于找到了一个能够拯救夏雷尼安的人。我终于可以安息了。");
                return;
            }

            if (!isEventLeader())
            {
                await SayOK("我需要你们队伍的领袖和我交谈，其他人不要。");
                return;
            }

            await SayNext("「在我以为会是永恒的沉睡之后，我终于找到了一个能够拯救夏雷尼安的人。这位老人现在将为你铺平道路，让你完成这项任务。」");
            await eim!.showClearEffect(true);
            // eim.giveEventPlayersStageReward(4) — not available in C# API
            await GainGuildGP(30);
            getMap().getReactorByName("ghostgate")?.forceHitReactor(1);
        }


        // Npc: 9040005 — Guild Quest leave NPC
        public async Task guildquest1_out()
        {
            if (await AskYesNo("你想退出公会任务吗？"))
            {
                await warp(101030104);
            }
        }


        // Npc: 9040006 — Fountain puzzle (stage 3)
        public async Task guildquest1_baseball()
        {
            if (getMap().getReactorByName("watergate")?.getState() > 0)
            {
                await SayOK("干得好。你可以继续进行下一阶段了。");
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
                await SayOK("请让你们的领袖和我交谈。");
                return;
            }

            var currentCombo = eim.getProperty("stage3combo");
            if (currentCombo == null || currentCombo == "reset")
            {
                var newCombo = makeStage3Combo();
                eim.setProperty("stage3combo", "" + newCombo);
                eim.setProperty("stage3attempt", "1");
                await SayOK("这个喷泉守护着通往王座房间的秘密通道。在这个区域里向家臣们献上物品以继续前行。家臣们会告诉你你的献礼是否被接受，如果没有，哪些家臣感到不满。你有七次尝试的机会。祝你好运。");
                return;
            }

            var attempt = int.Parse(eim.getProperty("stage3attempt")!);
            var combo = int.Parse(currentCombo);
            var guess = getGroundStage3Items();
            if (guess == null)
            {
                await SayOK("请确保你的尝试已经正确地放置在家臣面前，然后再和我交谈。");
                return;
            }

            if (combo == guess.Value)
            {
                getMap().getReactorByName("watergate")?.forceHitReactor(1);
                await eim.showClearEffect(true);
                await GainGuildGP(25);
                await removeStage3Items();
                await SayOK("干得好。你可以继续进行下一阶段了。");
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
                        result += correct + " 位家臣对他们的供品感到满意。\r\n";
                    if (incorrect > 0)
                        result += incorrect + " 位家臣收到了错误的供品。\r\n";
                    if (unknown > 0)
                        result += unknown + " 位家臣收到了未知的供品。\r\n";

                    var ord = attempt switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
                    result += "这是你的第 " + attempt + ord + " 次尝试。";

                    await spawnMobInMap(9300036, randX(), 150);
                    await spawnMobInMap(9300037, 400, 150);

                    await SayOK(result);
                    eim.setProperty("stage3attempt", "" + (attempt + 1));
                }
                else
                {
                    eim.setProperty("stage3combo", "reset");
                    await SayOK("你已经失败了测试。请冷静下来，稍后再试。");
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
            await SayOK("我与鲁比安搏斗失败，现在被囚禁在挡住我去路的大门中，我的身体被亵渎。然而，我的旧衣服中蕴含着神圣的力量。如果你能将衣服归还给我的身体，我应该能够打开大门。请快点！\r\n- 沙伦三世\r\nP.S. 我知道这有点挑剔，但你能把衣服从#bbottom到top#k归还给我的身体吗？谢谢你的帮助。");
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
                await SayOK("干得好。你可以继续进行下一阶段了。");
                return;
            }

            if (!isEventLeader())
            {
                await SayOK("我需要这个副本的队长和我交谈，其他人不要。");
                return;
            }

            var status = eim.getProperty("stage1status");
            if (status == null || status == "waiting")
            {
                var phase = eim.getProperty("stage1phase");
                var stage = phase == null ? 1 : int.Parse(phase);

                if (stage == 1)
                    await SayOK("在这个挑战中，我将展示周围雕像上的一个图案。当我说出指令时，请重复这个图案以继续前进。");
                else
                    await SayOK("我现在将为你呈现一个更难的谜题。祝你好运。");

                eim.setProperty("stage1phase", "" + stage);
                eim.setProperty("stage1status", "display");
                eim.setProperty("stage1combo", "");

                var reactors = getStage1Reactors();
                var combo = makeStage1Combo(reactors, stage);
                await mapMessage(5, "正在展示组合，请稍候。");
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
                        await SayOK("干得好。你可以继续进行下一阶段。");
                    }
                    else
                    {
                        await SayOK("很好。不过你还有更多任务要完成。当你准备好的时候再和我交谈。");
                        eim.setProperty("stage1phase", "" + (stage + 1));
                        await mapMessage(5, "你已经完成了第 " + stage + " 部分的门卫测试。");
                    }
                    eim.setProperty("stage1status", "waiting");
                }
                else
                {
                    await eim.showWrongEffect();
                    await SayOK("你已经失败了这次测试。");
                    await mapMessage(5, "你已失败门卫测试。");
                    eim.setProperty("stage1phase", "1");
                    eim.setProperty("stage1status", "waiting");
                }
            }
            else if (status == "display")
            {
                await SayOK("雕像正在按照图案工作。请稍等。");
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
                await SayOK("这是你的最后挑战。打败潜伏在鲁比安中的邪恶，让你的副本队长把它带回给我。就这样。");
                return;
            }

            if (!haveItem(4001024))
            {
                await SayOK("这是你的最后挑战。打败潜伏在鲁比安中的邪恶，并把它带回给我。就这样。");
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
            await SayOK("<通知> \r\n 你是属于一个拥有足够勇气和信任的公会吗？那就接受公会任务的挑战吧！\r\n\r\n#b参与条件：#k\r\n1. 公会必须至少有6名成员！\r\n2. 公会任务的队长必须是公会的会长或副会长！\r\n3. 如果参与公会任务的成员数量少于6人，或者队长决定提前结束任务，公会任务可能会提前结束！");
        }


        // Npc: 9040012 — Armor statue plaque (stage 2 info)
        public async Task guildquest1_knight()
        {
            await SayOK("这块牌匾的翻译如下：\r\n「沙连尼亚的骑士是自豪的战士。他们的朗基努斯长矛既是强大的武器，也是城堡防御的关键：通过从大厅最高处的平台上移除它们，他们可以封锁入侵者的入口。」\r\n似乎有些东西用英文刻在侧面上，几乎看不清：\r\n「邪恶偷走了长矛，被锁在障碍物后面。返回到塔顶。大长矛，从更高处抓取。」\r\n……显然，找到答案的人没有太多时间活下去。可怜的家伙。");
        }
    }
}
