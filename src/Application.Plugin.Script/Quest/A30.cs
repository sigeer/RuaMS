using Application.Core.scripting.Infrastructure;
using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;
using Application.Shared.Constants;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Job;

namespace Application.Plugin.Script.Quest
{
    internal partial class QuestScript
    {
        // Quest: 2001 
        public async Task q2001e()
        {
            await SayNext("这...就是我儿子丢失的那张地契呀！而且你给我收集了盖房子需要的材料！ 真是太感谢你呀。这样我可以和我的亲戚一起住在#m102000000#了…!对了，这是我的一点心意…");
            if (getPlayer().getInventory(InventoryType.USE).getNumFreeSlot() < 1)
            {
                Popup("背包已满！");
                return;
            }

            var talkStr = "好...你选择你需要的卷轴。成功比率都是10%的。\r\n\r\n#r选择物品\r\n#b";
            var stance = getPlayer().getJobStyle();
            int[] vecItem;
            if (stance == Job.WARRIOR || stance == Job.BEGINNER)
            {
                vecItem = [2043002, 2043102, 2043202, 2044002, 2044102, 2044202, 2044402, 2044302];
            }
            else if (stance == Job.MAGICIAN)
            {
                vecItem = [2043702, 2043802];
            }
            else if (stance == Job.BOWMAN || stance == Job.CROSSBOWMAN)
            {
                vecItem = [2044502, 2044602];
            }
            else if (stance == Job.THIEF)
            {
                vecItem = [2043302, 2044702];
            }
            else
            {
                vecItem = [2044802, 2044902];
            }

            for (var i = 0; i < vecItem.Length; i++)
            {
                talkStr += "\r\n#L" + i + "# #i" + vecItem[i] + "# #t" + vecItem[i] + "#";
            }
            var selection = await SayOption(talkStr);
            var item = vecItem[selection];
            gainItem(item, 1);
            gainItem(4000022, -100);
            gainItem(4003000, -30);
            gainItem(4003001, -30);
            gainItem(4001004, -1);
            gainExp(20000);
            gainMeso(15000);
            gainFame(2);
            completeQuest();
        }
        // Quest: 2034 
        public async Task q2034e()
        {
            await SayNext("果然是你。我早就知道你很快可以完成~ 上次也是做得不错~ 真是了不起阿！作为谢礼，我应该送你礼物。#b#p1051000##k送了你一双鞋，希望对的你冒险之旅有帮助，赶快收下吧。");
            if (getPlayer().getInventory(InventoryType.EQUIP).getNumFreeSlot() < 1)
            {
                await SayOK("请清理一下装备栏以获得奖励。");
                return;
            }

            var stance = getPlayer().getJobStyle();
            int item;
            if (stance == Job.WARRIOR)
            {
                item = 1072003;
            }
            else if (stance == Job.MAGICIAN)
            {
                item = 1072077;
            }
            else if (stance == Job.BOWMAN || stance == Job.CROSSBOWMAN)
            {
                item = 1072081;
            }
            else if (stance == Job.THIEF)
            {
                item = 1072035;
            }
            else if (stance == Job.BRAWLER || stance == Job.GUNSLINGER)
            {
                item = 1072294;
            }
            else
            {
                item = 1072018;
            }

            gainItem(item, 1);
            gainItem(4000007, -150);
            gainExp(2200);
            completeQuest();

            await SayOK("那么如果有一天你想帮助别人的话，就来找我吧。这里有很多人需要帮助啊~~");
        }
        // Quest: 2124 
        public async Task q2124e()
        {
            if (!haveItem(4031619, 1))
            {
                await SayOK("请把#b#p2012019##k...带给我");
            }
            else
            {
                gainItem(4031619, -1);
                forceCompleteQuest();

                await SayOK("噢，你居然带来了#p2012019#k，谢谢。");
            }
        }
        // Quest: 2126 
        public async Task q2126e()
        {
            if (!haveItem(4031619, 1))
            {
                await SayOK("请把#b#p2012019##k...带给我。");
            }
            else
            {
                gainItem(4031619, -1);
                forceCompleteQuest();

                await SayOK("噢，你居然带来了#p2012019#k，谢谢。");
            }
        }
        // Quest: 2127 
        public async Task q2127e()
        {
            await SayOK("我看你已经准备好接受任务了。那么，请注意任务的细节。。。");
            forceCompleteQuest();
        }
        // Quest: 2147 
        public Task q2147s()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 2147 
        public Task q2147e()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 2148 
        public async Task q2148s()
        {
            await SayNext("无论这鬼树走到哪里，似乎都有蝙蝠陪伴着它。令人毛骨悚然。");
            forceCompleteQuest();
        }
        // Quest: 2149 
        public async Task q2149s()
        {
            await SayNext("据说每当有什么邪恶的东西扰乱这片土地时，一棵鬼树就会出现在这里。。。我们需要一个能保护我们村子的英雄！");
            //最近聽說東部岩山時常發生奇怪的襲擊事件，也許你可以留意一下…
            forceCompleteQuest();
        }
        // Quest: 2150 
        public async Task q2150s()
        {
            await SayNext("我告诉你，这棵树的树枝上有一条围巾。");
            forceCompleteQuest();
        }
        // Quest: 2151 
        public async Task q2151s()
        {
            await SayNext("这棵树有一个奇怪的雕刻，像一张吓人的脸。");
            forceCompleteQuest();
        }
        // Quest: 2152 
        public async Task q2152s()
        {
            await SayNext("那棵树。。。我以前听说过，我甚至研究过它的行为！如果我没记错，在某些条件下它会复活，开始吸取这些可怕的邪恶力量，这使得它们对附近的人和村子造成非常可怕的影响。");
            forceCompleteQuest();
        }
        // Quest: 2186 
        public async Task q2186e()
        {
            if (!isQuestCompleted(2186))
            {
                if (haveItem(4031853))
                {
                    if (canHold(2030019))
                    {
                        gainItem(4031853, -1);
                        gainExp(1700);
                        gainItem(2030019, 10);
                        forceCompleteQuest();

                        await SayOK("天哪，你找到我的眼镜了！谢谢，非常感谢。现在我又能看到一切了！");
                    }
                    else
                    {
                        await SayOK("请保留足够的背包空间获取奖励！");
                    }
                }
                else if (haveItem(4031854) || haveItem(4031855))
                {
                    //When I figure out how to make a completance with just a pickup xD
                    if (canHold(2030019))
                    {
                        if (haveItem(4031854))
                        {
                            gainItem(4031854, -1);
                        }
                        else
                        {
                            gainItem(4031855, -1);
                        }

                        gainExp(1000);
                        gainItem(2030019, 5);
                        forceCompleteQuest();

                        await SayOK("嗯，那不是我的眼镜。。。不过，谢谢你。");

                    }
                    else
                    {
                        await SayOK("请保留足够的背包空间获取奖励！");
                    }
                }
            }
        }
        // Quest: 2197 
        public async Task q2197e()
        {
            await SayNext("哦，你已经有怪物书了。祝你旅途好运~!");
            forceCompleteQuest();
        }
        // Quest: 2214 
        public async Task q2214e()
        {
            var hourDay = getHourOfDay();
            if (!(hourDay >= 17 && hourDay < 20))
            {
                await SayNext("(嗯，我在垃圾桶里找过，但找不到吉咪说的#t4031894#，也许不在里面。。。)");
                return;
            }

            if (!canHold(4031894, 1))
            {
                await SayNext("(嗯，我现在没有#t4031894#，我需要可用的背包的空间。)");
                return;
            }

            gainItem(4031894, 1);
            gainExp(20000);
            forceCompleteQuest();

            await SayNext("(啊，这里有张碎纸条。。。嗯，它包含了一些即将发生的计划的细节，#r#p1052002##k一定就是所需要的东西了。)");

        }
        // Quest: 2215 
        public async Task q2215e()
        {
            var hourDay = getHourOfDay();
            if (!(hourDay >= 17 && hourDay < 20))
            {
                await SayNext("(嗯，我在垃圾桶里找过，但找不到吉咪说的#t4031894#，也许不在里面。。。)");
                return;
            }

            if (getMeso() < 2000)
            {
                await SayNext("(抱歉，你的金币不足。)");
                return;
            }

            if (!canHold(4031894, 1))
            {
                await SayNext("(嗯，我现在没有#t4031894#，我需要可用的背包空间。)");
                return;
            }

            gainMeso(-2000);
            forceCompleteQuest();
            gainItem(4031894, 1);

            await SayNext("(好吧，现在我把钱存在那里，然后拿到报纸。。。就这样，好了。)");
        }

        bool isAllSubquestsDone()
        {
            for (var i = 2216; i <= 2219; i++)
            {
                if (!isQuestCompleted(i))
                {
                    return false;
                }
            }

            return true;
        }
        // Quest: 2216 
        public async Task q2216s()
        {
            await SayNext("我刚刚收到一个有趣的信息，#r多尔看起来就像是正常的鳄鱼#k，但是它块头大而且会使用魔法.…但它竟然还会讲国语？");
            forceCompleteQuest();
            gainExp(7000);

            if (isAllSubquestsDone() && haveItem(4031894))
            {
                gainItem(4031894, -1);
            }
        }
        // Quest: 2217 
        public async Task q2217s()
        {
            await SayNext("嘿，你注意到了吗，下水道里好像有臭味。。。");
            forceCompleteQuest();
            gainExp(7000);

            if (isAllSubquestsDone() && haveItem(4031894))
            {
                gainItem(4031894, -1);
            }
        }
        // Quest: 2218 
        public async Task q2218s()
        {
            await SayNext("喂，我感觉#r拉克里斯#k这些天的行为很奇怪? 我们应该看看她怎么了。");
            forceCompleteQuest();
            gainExp(7000);

            if (isAllSubquestsDone() && haveItem(4031894))
            {
                gainItem(4031894, -1);
            }
        }
        // Quest: 2219 
        public async Task q2219s()
        {
            await SayNext("你知道吗，他们说下水道里有人一直在尝试研发一种神奇的粉末，让我们可以长出来，是不是很好？");
            forceCompleteQuest();
            gainExp(7000);

            if (isAllSubquestsDone() && haveItem(4031894))
            {
                gainItem(4031894, -1);
            }
        }
        // Quest: 2228 
        public async Task q2228s()
        {
            await SayNext("谢谢你战胜了#r浮士德#k。我的灵魂终于能安息了。");
            forceCompleteQuest();
            gainFame(8);
        }
        // Quest: 2230 
        public async Task q2230e()
        {
            await SayOption("你好，旅行者。。。你终于来看我了。你履行职责了吗？\r\n #b#L0#什么职责？你是谁？#l#k");
            await SaySpeech([
                "你在口袋里找到宠物了吗？保护好宠物是你的职责。当你独自一人的时候，生活很艰难。在这样的时刻，没有什么比拥有一个朋友更能时刻陪伴着你。你听说过#b宠物#k吗\r\n人们养宠物是为了减轻负担、悲伤和孤独，因为知道你身边有人或事，真的会带来心灵的平静。但一切都有后果，随之而来的是责任。。。",
                    "养宠物需要承担巨大的责任。记住宠物也是生命的一种形式，你需要悉心喂养它，给它取一个好听的名字，与它分享你的想法，最终形成一种纽带。这就是主人对这些宠物的依恋。",
                    "我想把这个灌输给你，所以我送你一个我珍爱的孩子。你带来的宠物是#b蜗牛#k,通过魔法而生的生物。既然你把宠物带到这里时很小心，宠物很快就会孵化出来。",
                    "蜗牛是拥有许多技能的宠物。它会拾取物品，给你吃药水，还能做很多其他的事情。缺点是，由于蜗牛是从魔法中诞生的，它的寿命很短。一旦变成洋娃娃，就永远无法复活."
                ]);
            if (await SayYesNo("现在你明白了吗？每一个行动都会带来后果，宠物也不例外。蜗牛的很快就会孵化."))
            {
                if (canHold(5000054, 1))
                {
                    gainItem(4032086, -1); // Mysterious Egg * -1
                    forceCompleteQuest();
                    gainItem(5000054, 1, false, true, 5 * 60 * 60 * 1000);  // rune snail (5hrs), missing expiration time detected thanks to cljnilsson
                }
                else
                {
                    await SayNext("请在您尝试接收宠物之前，在您的特殊栏中至少留有一个位置。。。");
                    return;
                }
            }
        }
        // Quest: 2232 
        public Task q2232e()
        {
            //// TODO
            //var familyEntry = getPlayer().getFamilyEntry();
            //if (familyEntry != null && familyEntry.getJuniorCount() > 0)
            //{  // script found thanks to kvmba
            //    forceCompleteQuest();
            //    gainExp(3000);
            //    await SayNext("做得很好！");
            //}
            //else
            //{
            //    await SayNext("你还没有找到一个同学吗？");
            //}
            //dispose();
            return Task.CompletedTask;
        }
        // Quest: 2233 
        public Task q2233e()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 2234 
        public Task q2234e()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 2238 
        public Task q2238s()
        {
            forceStartQuest();
            return Task.CompletedTask;
        }
        // Quest: 2245 
        public async Task q2245s()
        {
            var em = GetSoloQuestEventManager(2245);
            var r = em.StartInstance(getPlayer());
            if (r == Core.scripting.Events.Abstraction.CreateInstanceResult.Success)
            {
                forceStartQuest();
            }
            else
            {
                await SayOK(em.HandleCreateInstanceResult(r, c));
            }
        }
        // Quest: 2251 
        public async Task q2251e()
        {
            if (!haveItem(4032399, 20))
            {
                await SayOK("请带20个#b#t4032399##k给我...#b#t4032399##k...  #i4032399#");
            }
            else
            {
                gainItem(4032399, -20);
                gainExp(8000);
                forceCompleteQuest();

                await SayOK("噢！你带来了20个#b#t4032399##k!谢谢你.");
            }
        }
        // Quest: 2291 
        public async Task q2291e()
        {
            if (!haveItem(4032521, 10))
            {
                await SayNext("嘿，你还没有得到#b10张#t4032521##k吗？");
                return;
            }
            else
            {

                await SayNext("你身上有#b#i4032521##k，很好，让我给你带路。");

                var em = GetSoloQuestEventManager(2291);
                if (em.StartInstance(getPlayer()) != Core.scripting.Events.Abstraction.CreateInstanceResult.Success)
                {
                    await SayOK("嗯...看起来前面的房间现在有点拥挤。请在这儿等一会，好吗？");
                    return;
                }

                gainItem(4032521, -10);
                forceCompleteQuest();
            }
        }
        // Quest: 2293 
        public async Task q2293s()
        {
            await SayNext("你还记得摇滚精神演奏的最后一首歌吗？我能想到他的几首歌，接下来模仿给你，仔细听完后告诉我是哪首歌。#b你只有一次机会#k，所以请谨慎做出选择。");
            forceStartQuest();
        }
        // Quest: 2293 
        public async Task q2293e()
        {
            var option = await SayOption("我给你一些样品试听。请选一个。在做你的选择之前请仔细听。\r\n\\t#b#L1#听第一首歌#l \r\n\\t#L2#听第二首歌#l \r\n\\t#L3#听第三首歌#l \r\n\\r\n\\t#e#L4#输入正确的歌曲。#l");
            switch (option)
            {
                case 1:
                    playSound("Party1/Failed");
                    await SayOK("令人尴尬的熟悉。。。");
                    break;
                case 2:
                    playSound("Coconut/Failed");
                    await SayOK("是这个吗？");
                    break;
                case 3:
                    playSound("quest2288/6");
                    await SayOK("你听到了吗?");
                    break; 
                case 4:
                    var answer = await SayInputNumber("现在，请告诉我答案。你只有#b一次机会#k，所以请明智地选择。请在聊天窗口输入#b1，2，或者3#k。\r\n", 1, 1, 3);
                    if (answer == 3)
                    {
                        forceCompleteQuest();
                        gainExp(32500);

                        await SayOK("所以这就是他演奏的那首歌。。。好吧，这毕竟不是我的歌，但我很高兴我现在可以肯定地知道。非常感谢。");
                    }
                    else
                    {
                        await SayOK("显然你不喜欢音乐。");
                    }
                    break;
                default:
                    break;
            }

        }
        // Quest: 2300 
        public async Task q2300s()
        {
            if (await SayAcceptDecline("现在你的强大了许多，我有一件事情想找你帮忙，你是否愿意听听？"))
            {
                await SayNext("故事发生在蘑菇王国，具体的事情我也不太清楚。但是好像很紧急。");
                await SayNext("我不知道事情的细节，所以想找你帮帮忙，你可能会需要更多的时间帮助蘑菇王国，我送你一封信，请你把它交给#b警卫队长#k。\r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0#\r\n#v4032375# #t4032375#");
                if (await SayYesNo("如果你现在想去蘑菇城堡的话，我可以送你去。你确定要去吗？"))
                {
                    if (canHold(4032375, 1))
                    {
                        if (!haveItem(4032375, 1))
                        {
                            gainItem(4032375, 1);
                        }

                        warp(106020000, 0);
                        forceStartQuest();
                    }
                    else
                    {
                        await SayOK("请在其他栏中留至少一个位置。");
                    }
                }
                else
                {
                    await SayOK("我有个请求，你愿意听我说说吗？");
                }
            }
            else
            {
                if (canHold(4032375, 1))
                {
                    await SayNext("你是谁？看你好像是冒险岛世界的冒险家。我们王国目前正面临巨大的危机，需要一位勇士来拯救我们。如果你没什么事的话，希望你能帮助我们。");
                }
                else
                {
                    await SayOK("请在其他栏中留至少一个位置。");
                }
            }
        }
        // Quest: 2300 
        public async Task q2300e()
        {
            if (!haveItem(4032375, 1))
            {
                await SayNext("你想做什么？");
                return;
            }
            else
            {
                await SaySpeech([
                    "嗯？那是#b转职官的推荐书#k吗？你是来拯救我们蘑菇王国的人吗？",
                        "好吧，既然你有转职教官的推荐信，我想你是一个很棒的人，很抱歉我没有自我介绍，我是包围蘑菇城堡的卫兵，正如你所看到的，这里是我们暂时的藏身之地，我们的情况很糟糕，尽管如此，欢迎你来到蘑菇王国！"]);
                gainItem(4032375, -1);
                gainExp(6000);
                forceCompleteQuest();
                forceStartQuest(2312);
            }
        }
        // Quest: 2301 
        public Task q2301s() => q2300s();
        // Quest: 2301 
        public Task q2301e() => q2300e();
        // Quest: 2302 
        public Task q2302s() => q2300s();
        // Quest: 2302 
        public Task q2302e() => q2300e();
        // Quest: 2303 
        public Task q2303s() => q2300s();
        // Quest: 2303 
        public Task q2303e() => q2300e();
        // Quest: 2304 
        public Task q2304s() => q2300s();
        // Quest: 2304 
        public Task q2304e() => q2300e();
        // Quest: 2305 
        public Task q2305s() => q2300s();
        // Quest: 2305 
        public Task q2305e() => q2300e();
        // Quest: 2306 
        public Task q2306s() => q2300s();
        // Quest: 2306 
        public Task q2306e() => q2300e();
        // Quest: 2307 
        public Task q2307s() => q2300s();
        // Quest: 2307 
        public Task q2307e() => q2300e();
        // Quest: 2308 
        public Task q2308s() => q2300s();
        // Quest: 2308 
        public Task q2308e() => q2300e();
        // Quest: 2309 
        public Task q2309s() => q2300s();
        // Quest: 2309 
        public Task q2309e() => q2300e();
        // Quest: 2310 
        public Task q2310s() => q2300s();
        // Quest: 2310 
        public Task q2310e() => q2300e();
        // Quest: 2314 
        public async Task q2314s()
        {
            if (await SayAcceptDecline("要想救出公主，必须对蘑菇森林进行调查。不知道企鹅王用了什么手段，在蘑菇森林设下了强力的结界，阻止外人进入蘑菇城。请你马上去进行调查。"))
            {
                forceStartQuest();
            }
            else
            {
                await SayOK("请不要抛弃我们蘑菇王国。");
            }
        }
        // Quest: 2322 
        public async Task q2322s()
        {
            if (await SayYesNo("即使穿越了结界，也不能完全放心。我们蘑菇王国的城墙经过特殊设计，绝对无法从外部侵入，想要进去会很困难。嗯……你能先去城墙外部调查一下吗？"))
            {
                forceStartQuest();
            }
        }
        // Quest: 2325 
        public async Task q2325e()
        {
            await SaySpeech([
                new SpeechText("我好……好害怕……请你一定要救救我……", 0),
                new SpeechText("别害怕，是#b#p1300005##k让我来找你的。", 2),
            ]);


            await SayOK("嗯？是哥哥让你来找我的？啊……终于得救了。真是谢谢你。");

            forceCompleteQuest();
            gainExp(6000);
        }
        // Quest: 2327 
        public async Task q2327s()
        {
            await SaySpeech([
                "嘿！谢谢你给我带来了#b#t4001317##k.",
                "我打算穿着#b#t4001317##k回去.给我一分钟穿上它。然后再跟你聊。。。"
                ]);
            forceStartQuest();
        }
        // Quest: 2332 
        public Task q2332s()
        {
            // TODO
            if (hasItem(4032388) && !isQuestStarted(2332))
            {
                forceStartQuest();
                getPlayer().showHint("我必须找到碧欧蕾塔公主！ (#b任务开始#k)");
            }

            return Task.CompletedTask;
        }
        // Quest: 2332 
        public Task q2332e()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 2333 
        public async Task q2333s()
        {
            if (await SayAcceptDecline("勇士，拜托你了！请你一定要拯救菇菇王国"))
            {
                await SayNext("#b蘑菇大臣#k是幕后策划的黑手！哦，不！他来了。。。");
                forceStartQuest();
            }
        }
        // Quest: 2333 
        public async Task q2333e()
        {
            await SayNext("天哪！ #b#h ##k 你居然打败了 #b蘑菇大臣#k.");

            gainExp(15000);
            forceCompleteQuest();
        }
        // Quest: 2334 
        public async Task q2334s()
        {
            await SaySpeech([
                new SpeechText("非常感谢你，#b#h ##k。你就是拯救我们帝国免于危险的英雄。我对你所做的一切感激不尽。我不知道该如何感谢你。请理解我为什么不能让你看到我的面孔。", 0),
                new SpeechText("说出来真是难为情，但自从我还是个婴儿的时候，我的家人就把我的面容遮掩起来，不让世人看见。他们害怕有人会无法自拔地爱上我。我已经习惯了这种生活，甚至对女性也感到害羞。我知道，把背对着救世主是很失礼的，但在我能够鼓起勇气与你面对面相见之前，我还需要一些时间。", 0),
                new SpeechText("我明白了...\r\n#b（哇，她究竟有多美？）", (int)NpcTalkSpeaker.Player),
                new SpeechText("#b（这是什么意思？）", (int)NpcTalkSpeaker.Player),
                new SpeechText("#b（在蘑菇世界里这也算是漂亮吗？！）", (int)NpcTalkSpeaker.Player),
                new SpeechText("我好害羞，都脸红了。总之，谢谢你，#b#h ##k。", 0),
                ]);

            forceStartQuest();
            gainExp(1000);
            forceCompleteQuest();

        }
        // Quest: 2335 
        public async Task q2335s()
        {
            await SayNext("这还不是结束，#b#h ##k。 #b首相#k 的手下仍然散布在城堡中。");
            if (await SayAcceptDecline("据我所知，靠近#b天空高楼 3#k附近有一群首相的手下。前几天我捡到了首相掉落的一把钥匙。拿去吧，用这把钥匙。"))
            {
                if (canHold(4032405))
                {
                    gainItem(4032405, 1);
                    forceStartQuest();

                    await SayNext("最后一次，祝你好运。");
                }
                else
                {
                    await SayOK("请确保你的其他栏有空位。");
                }
            }
        }
        // Quest: 2338 
        public async Task q2338s()
        {
            if (haveItem(2430014, 1))
            {
                await SayNext("这是好不容易制作出来的东西，希望你能小心一些。");
            }
            else
            {
                await SayNext("你把#b#t2430014##k弄丢了？");

                if (!canHold(2430014, 1))
                {
                    await SayNext("请在消耗栏留至少一个空位，好吗？");
                }
                else
                {
                    gainItem(2430014, 1);
                    forceCompleteQuest();
                }
            }
        }
        // Quest: 2342 
        public async Task q2342s()
        {
            if (isQuestStarted(2331) && !isQuestCompleted(2331))
            {
                if (!hasItem(4001318))
                {
                    if (canHold(4001318))
                    {
                        forceStartQuest();
                        gainItem(4001318, 1);
                        await SayOK("看起来你在与蘑菇大臣战斗时忘记拿起#b#t4001318##k了。给你，这对我们王国非常重要，请尽快把它交给我的父亲。");
                        forceCompleteQuest();
                    }
                    else
                    {
                        await SayOK("请确保你的其他栏有一个空位");
                        return;
                    }
                }
                else
                {
                    forceStartQuest();
                    await SayOK("你手中的#b#t4001318##k对我们王国非常重要，请尽快把它交给我的父亲把。");
                    forceCompleteQuest();
                }
            }
            else
            {
                await SayOK("你需要找回被盗的#r#t4001318##k");
                return;
            }
        }
        // Quest: 4647 
        public async Task q4647s()
        {
            if (await SayAcceptDecline("若你真的有心想要学习带领多只宠物的技能的话，就去想办法把宠物点心拿给我吧！"))
            {
                forceStartQuest();
            }
            else
            {
                await SayOK("没有付出是不会有收获的！你就再好好考虑看看吧！");
            }
        }
        // Quest: 4647 
        public async Task q4647e()
        {
            if (haveItem(5460000))
            {
                completeQuest();
                teachSkill(8, 1, 1, -1);
                gainItem(5460000, -1, false);

                await SayOK("你获得了宠物点心！谢谢，现在你可以同时携带多只宠物了！");
            }
            else
            {
                await SayOK("给我宠物点心！可以在商城中找到....");
            }
        }

    }
}