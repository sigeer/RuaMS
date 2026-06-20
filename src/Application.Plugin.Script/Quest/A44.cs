using Application.Shared.Constants.Inventory;
using Application.Shared.GameProps;
using Application.Utility;

namespace Application.Plugin.Script.Quest
{

    // 武陵桃园+尼哈沙漠
    internal partial class QuestScript
    {
        // Quest: 2257 
        public async Task q2257e()
        {
            await forceCompleteQuest();

            await SayNext("嘿，你想搭便车去#r#m261000000##k吗？哦，来自#b#p2101013##k？");
        }
        // Quest: 2258 
        public async Task q2258s()
        {
            if (await SayAcceptDecline("猫鼬像野火一样散布谣言。。。通过勒索我和我的出租车服务，他们日复一日地把顾客从我身边带走。。。嘿，不要告诉任何人，如果你能帮我清理一些狸猫，我会告诉你关于蘑菇城堡的信息。"))
            {
                await forceStartQuest();

                await SayNext("太好了，你需要在#r5分钟#k内杀死#b40只狸猫#k。祝你好运！");
            }
        }
        // Quest: 2258 
        public async Task q2258e()
        {
            await forceCompleteQuest();
            await SayNext("你做到了! 嘿......#r狸猫#k在这附近可以听我们的谈话.我现在不想谈这个.");
        }
        // Quest: 2259 
        public async Task q2259s()
        {
            await forceStartQuest();

            await SayNext("好的，我们在#b#m260020700##k等你消息。要到达那里，请搭乘#r骆驼中巴#k前往#r玛加提亚#k，我会在那里等你，现在出发吧。");
        }
        // Quest: 2259 
        public async Task q2259e()
        {
            if (getMapId() == 260020000)
            {
                await SayNext("呃，你还在这里？想要到达#b#m260020700###k，可以搭乘#r骆驼中巴#k前往#r玛加提亚#k，我会在那里等你。现在出发吧。");
                return;
            }

            await forceCompleteQuest();
            await SayNext("哦，你来了。这附近很安全，没有猫鼬在这里窃听。你一定很适合去#r蘑菇城堡#k。等你到了再跟我谈谈。");
        }
        // Quest: 2260 
        public async Task q2260s()
        {
            await forceStartQuest();
            await SayNext("如果你进行了#b第2次转职#k，我会告诉你关于#b蘑菇城堡#k的一些信息。");
        }
        // Quest: 2260 
        public async Task q2260e()
        {
            if (getPlayer().getJob().Rank == 1)
            {
                await SayNext("呵呵，你还没有进行#r第2次转职#k吗？");
                return;
            }

            await forceCompleteQuest();
            await SayNext("好吧，你准备好去#b蘑菇城#k吗？蘑菇城的入口就在射手村西边的大树上面，沿着这条路很容易就能找到！");
        }

        List<int> getOreArray()
        {
            List<int> ores = [];
            var y = 0;
            for (var x = 4020000; x <= 4020008; x++)
            {
                if (haveItem(x, 2))
                {
                    ores.Add(x);
                }
            }
            return ores;
        }
        string getOreString(List<int> ids)
        { // Parameter 'ids' is just the array of getOreArray()
            var thestring = "#b";
            string extra;
            for (int x = 0; x < ids.Count; x++)
            {
                extra = "#L" + x + "##t" + ids[x] + "##l\r\n";
                thestring += extra;
            }
            thestring += "#k";
            return thestring;
        }
        // Quest: 3301 
        public async Task q3301e()
        {
            var oreArray = getOreArray();
            if (oreArray.Count > 0)
            {
                var selection = await AskMenu("哦，看起来有人准备做交易了。你这么想加入蒙特鸠协会吗？我真的不理解你，但没关系。你会给我什么回报？\r\n" + getOreString(oreArray));

                if (!haveItem(oreArray[selection], 2))
                {
                    await SayNext("这是怎么回事，你没有#c#k。没有矿石就没交易。!");
                   
                    return;
                }

                await gainItem(oreArray[selection], -2); // Take 2 ores
                await forceCompleteQuest();

                await SayNext("请等一下我去拿个东西，以帮助您更容易通过蒙特鸠协会长的考验。");
            }
            else
            {
                await SayOK("这是什么，你没有#r宝石矿石#k。没有矿石，就没有交易。.");
                return;

            }
        }
        // Quest: 3303 
        public async Task q3303e()
        {
            var oreArray = getOreArray();
            if (oreArray.Count > 0)
            {
                var selection = await AskMenu("“哦，看起来有人准备做交易了。你这么想加入蒙特鸠协会吗？我真的不理解你，但没关系。你会给我什么回报？”\r\n" + getOreString(oreArray));
                if (!haveItem(oreArray[selection], 2))
                {

                    await SayNext("“这是什么，你没有#r宝石矿石#k。没有矿石就没有交易！”");
                   
                    return;
                }

                await gainItem(oreArray[selection], -2); // Take 2 ores
                await forceCompleteQuest();

                await SayNext("请等一下我去拿个东西，以帮助您更容易通过蒙特鸠协会长的考验.");

            }
            else
            {
                await SayOK("这是什么，你没有#r珠宝矿石#k。没有矿石，没有交易。”.");    // script would loop undefinitely at completion, thanks IxianMace for noticing
            }
        }
        // Quest: 3305 
        public async Task q3305s()
        {
            await SayNext("你把#b蒙特鸠#k披风弄丢了，我可以再给你做一个，但我需要一些材料。");
            if (await SayAcceptDecline("要制作新披风，我需要你给我带来#b5 #t4021003##k, #b10 #t4000021##k和#b10000 金币#k。"))
            {
                await forceStartQuest();

                await SayOK("等你收集到了所有材料再回来找我。");
            }
        }
        // Quest: 3306 
        public async Task q3306s()
        {
            await SayNext("你把#b卡帕莱特#k披风弄丢了，我可以再给你做一个，但我需要一些材料。");
            if (await SayAcceptDecline("要制作新披风，我需要你给我带来#b5 #t4021006##k, #b10 #t4000021##k和#b10000 金币#k。"))
            {
                await forceStartQuest();

                await SayOK("等你收集到了所有材料再回来找我。");
            }
        }
        // Quest: 3314 
        public async Task q3314e()
        {
            if (getPlayer().getBuffSource(BuffStat.HPREC) == 2022198)
            {
                if (canHoldAll([2050004, 2022224], [10, 20]))
                {
                    await SayNext("呼呼呼呼.... 看你面色苍白看来真的很有效果啊．这次的实验成功了！呃哈哈哈哈！果然可以用在能打倒洛伊德的坚强的人身上！......很惊讶的表情嘛？不用太担心．不是很危险的药…不，虽然是危险的药但是有解毒药…呼呼呼呼...如此一来，任意改变人体的状态会变得更为容易…这样...搞不好可以帮那家伙达成愿望...");

                    await gainExp(12500);
                    await gainItem(2050004, 10);

                    var i = Random.Shared.Next(5);
                    await gainItem(2022224 + i, 10);

                    await forceCompleteQuest();
                }
                else
                {
                    await SayNext("你的背包满了，清理背包后再来试试。");
                }
            }
            else
            {
                await SayNext("你看起来很正常，不是吗？我的实验对你没有任何可能的影响。去吃我给你的药水，给我看看效果，好吗？");
            }
        }
        // Quest: 3320 
        public async Task q3320s()
        {
            if (await SayAcceptDecline("我可能知道德朗博士的下落。你准备好被传送到那个地区了吗？"))
            {
                await forceStartQuest();
                await warp(926120200, 1);
            }
        }
        // Quest: 3321 
        public async Task q3321s()
        {
            await SaySpeech([
                "如你所知，我是德朗博士。曾经是艾尔卡德诺社会中的一名有影响力的炼金术士，但因为我实验的失败后果，现在整个马加提亚都可以看到。",
                "胡罗伊德，我的创造物，最初被设计用于家庭、科学和军事事务，然而主处理单元芯片的关键故障使它们变得不稳定和暴力，迅速引发混乱和灾难。因此，我被剥夺了艾尔卡德诺的炼金术士和研究员的地位，并被发出了逮捕令。",
                ]);
            if (await SayAcceptDecline("即便如此，现在我不能停下！我的创造物们仍然在城市中四处肆虐，每天造成破坏和伤亡，而我们几乎没有希望驱逐它们！它们还可以自我复制，普通武器无法阻止它们。我一直在不懈地研究一种一举消灭它们的方法，试图找到停止这种疯狂的办法。你一定能理解我的处境吧？"))
            {
                await SayNext("非常感谢你理解我的立场。你一定见过帕温，因此你知道我在哪里。让他意识到当前的情况。");
                await SayNext("哦，如果可以的话，我还有一个私人请求。我担心我的妻子，#b#p2111004##k。自从胡罗伊德事件以来，我没能向她传达消息，这一定对她造成了很大的负担... 如果可以的话，你能去#b家里#k找回#b银色吊坠#k，代我送给她吗？我很后悔没有在她的生日时立刻把这件东西送给她... 也许现在给她，能让她安心一晚上。");
                await SayNext("#r记住这个顺序！#k 我把吊坠藏在我家里，水管后面的一个容器里。水管必须按顺序打开：上、下、中。然后，输入秘密密码：'#rmy love Phyllia#k'。");

                await forceStartQuest();
            }

        }
        // Quest: 3353 
        public async Task q3353s()
        {
            await SayNext("“我明白了。德朗想阻止胡洛伊德造成更多的破坏，但社会希望立即将他关进监狱。所以他才躲在那里。”.");
            if (await SayAcceptDecline("那样的话，再去那里听德朗说更多细节，可以吗？"))
            {
                await forceStartQuest();
                await warp(926120200, 1);
            }
        }
        // Quest: 3354 
        public async Task q3354s()
        {
            if (await SayAcceptDecline("我有个请求。你能不能向 #bMaed#k 要一瓶我设计的药剂？显然，不要提到我让你这样做，那会是个问题。由于与 Huroids 的接触，#bKeeny#k 生病了，这让我非常烦恼，以至于我无法在我的研究上取得进展……请 #r把药剂带给她#k，这样我才能感觉好些，并开始取得进展。我指望你了."))
            {
                await forceStartQuest();
            }
        }
        // Quest: 3360 
        public async Task q3360s()
        {
            await SayNext("哦！终于来了！很高兴你来得及时。我有给你打开秘密通道的主钥匙！哈哈哈哈！是不是很惊奇？说出来很惊奇吧！");
            if (await SayAcceptDecline("好了，现在，这个钥匙非常长且复杂。我需要你牢牢记住它。我不会再重复，所以最好把它写在某个地方。你准备好了吗？"))
            {
                var pass = string.Join("", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".OrderBy(x => Guid.NewGuid()).Take(10));
                await SayOK("钥匙代码是 #b" + pass + "#k。记住了吗？把钥匙插入秘密通道的门上，你就可以自由地走进通道了。");

                await forceStartQuest();
                await setQuestProgress(3360, pass);
            }
            else
            {
                await SayNext("快点，快点。如果你不够聪明，就拿出笔和纸来吧！");
            }
        }
        // Quest: 3382 
        public async Task q3382e()
        {
            if (haveItem(4001159, 25) && haveItem(4001160, 25) && !haveItemWithId(1122010, true))
            {
                if (canHold(1122010))
                {
                    await gainItem(4001159, -25);
                    await gainItem(4001160, -25);
                    await gainItem(1122010, 1);
                    await forceCompleteQuest();

                    await SayOK("感谢你找回了这些弹珠。接受这个吊坠作为我的感激之情。");
                }
                else
                {
                    await SayNext("在领取奖励之前，请在你的装备栏中腾出一个空位。");
                    return;
                }
            }
            else if (haveItem(4001159, 10) && haveItem(4001160, 10))
            {
                if (canHold(2041212))
                {
                    await gainItem(4001159, -10);
                    await gainItem(4001160, -10);
                    await gainItem(2041212, 1);
                    await forceCompleteQuest();

                    await SayOK("感谢你找回了这些弹珠。这块石头，我给你的，可以用来提升 #b#t1122010##k 的属性。拿着它作为我的感激之情，并明智地使用它。");
                }
                else
                {
                    await SayNext("在领取奖励之前，请在你的消耗栏中腾出一个空位。");
                    return;
                }
            }
            else
            {
                await SayNext("我至少需要 #b10个#t4001159# 和 #t4001160##k 才能适当地奖励你。如果你带来了 #b25个#k，我可以用一件有价值的装备来奖励你。祝你一路顺风。");
                return;
            }
        }
        // Quest: 3833 
        public async Task q3833e()
        {
            await SayOK("太棒了！你拿到了我需要的草药。作为感谢，请收下这个物品来帮助你在旅途上。.");
            if (getPlayer().getInventory(InventoryType.USE).getNumFreeSlot() >= 2)
            {
                if (haveItem(4000294, 1000))
                {
                    await gainItem(4000294, -1000);
                    await gainItem(2040501, 1);
                    await gainItem(2000005, 50);
                    await gainExp(54000);
                    await forceCompleteQuest();
                }
                else if (haveItem(4000294, 600))
                {
                    await gainItem(4000294, -600);
                    await gainItem(2020013, 50);
                    await gainExp(54000);
                    await forceCompleteQuest();
                }
                else if (haveItem(4000294, 500))
                {
                    await gainItem(4000294, -500);
                    await gainExp(54000);
                    await forceCompleteQuest();
                }
                else if (haveItem(4000294, 100))
                {
                    await gainItem(4000294, -100);
                    await gainExp(45000);
                    await forceCompleteQuest();
                }
                else if (haveItem(4000294, 50))
                {
                    await gainItem(4000294, -50);
                    await gainItem(2020007, 50);
                    await gainExp(10000);
                    await forceCompleteQuest();
                }
                else if (haveItem(4000294, 1))
                {
                    await gainItem(4000294, -1);
                    await gainItem(2000000, 1);
                    await gainExp(10);
                    await forceCompleteQuest();
                }

            }
            else
            {
                await SayOK("在领取奖励之前，你需要在消耗栏中腾出#b2个空位#k？");
            }
        }
        // Quest: 3933 
        public async Task q3933s()
        {
            await SayNext("没想到你会这么的强…以你的水平也许可以成为沙子图团的团员也说不定。对沙子图团员来说，最重要的就是力量的强大，而你…看来已经具备了足够的实力！但我还是要再进行一次测试…如何？可以接受吗？");
            if (await SayAcceptDecline("若想要实际测试你的力量，应该需要亲自去体验吧？我想和你进行一场对战！别担心，我也不想伤害你…就用我的分身来对付你好了！可以马上进行对战吗？"))
            {
                await SayNext("很好，我喜欢你的自信。");
                // TODO
                if ((await getWarpMap(926000000)).getAllPlayers().Count > 0)
                {
                    await SayOK("此地图中当前有人，请稍后再试。");
                }
                else
                {
                    await forceStartQuest();

                    await warp(926000000, "st00");
                }
            }
        }
        // Quest: 3941 
        public async Task q3941s()
        {
            if (getPlayer().getBuffSource(BuffStat.MORPH) != 2210005)
            {
                await SayNext("这是什么？我不能简单地把女王的丝绸交给任何人，声称他们会立刻交给女王。离开我的视线。");
                return;
            }

            await forceStartQuest();
        }
        // Quest: 3941 
        public async Task q3941e()
        {
            if (getPlayer().getBuffSource(BuffStat.MORPH) != 2210005)
            {
                await SayNext("这是什么？我不能简单地把女王的丝绸交给任何人，声称他们会立刻交给女王。离开我的视线。");
                return;
            }

            if (canHold(4031571, 1))
            {
                await gainItem(4031571);

                await SayNext("拿去吧。请尽快交给女王，提古，如果事情延误她会很生气的。");
                await forceCompleteQuest();
            }
            else
            {
                await SayNext("嘿，你的背包空间不足。我会帮你留着，你整理好背包再来取...");
            }
        }
        // Quest: 3953 
        public async Task q3953e()
        {
            await AskMenu("如果你想说大宇是怪物, 我根本不想听, 你快走吧！……嗯？这不是锂吗？从颜色看, 应该是最高级的锂……状态也很好……嗯？你要把它给我？呵呵……锂的话, 我就不客气了. 对了……这是为什么呢？\r\n\r\n#L0##b我想告诉你大宇是怪物. #l \r\n\r\n#L1##b你听说前往沙漠的商团遭到了怪物的袭击吗？");
            await AskMenu("商团？……护卫的人手好像太少了. 火焰之路虽然没有太危险的怪物, 但也不能那样粗心大意……在沙漠里必须时刻保持警惕才行. \r\n\r\n#L0##b消灭了大宇, 就不会发生这种事情了. #l \r\n\r\n#L1##b这都是因为王妃对村子周围的治安疏于管理. #l");
            await AskMenu("没错！都是因为王妃！自从那个女人来了之后……原本很聪明的阿得拉８世全变了, 阿里安特也逐渐变得干旱！绿洲变成了荒漠！这都是因为那个女人！\r\n\r\n#L0##b王妃施行暴政, 不知道沙漠的守护神会怎么做. #l \r\n\r\n#L1##b必须尽快组织军队, 使国家摆脱王妃的压迫！#l");
            await AskMenu("……你说什么？大宇变成了怪物……他可是阿里安特的守护神啊……不过也是……阿里安特已经和过去不同了……\r\n\r\n#L0##b所以说嘛. 阿烈达王妃在吸收沙漠的精气, 大宇也失去了原来的灵性, 变成了怪物……#l");

            await forceCompleteQuest();
            await gainItem(4011008, -1);
            await gainExp(20000);

            await SayNext("对……也许你的话是对的. 阿里安特变成了这样……这也许是因为大宇变了的缘故. 也许大宇真的已经变成了怪物……就像年轻人说的那样, 到了除掉大宇的时候了……");
        }

    }
}