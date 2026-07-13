using Application.Core.Scripting.Events;
using server.life;
using server.maps;
using System.Drawing;


namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 2112013 — Investigation Result (Alcadno - stage 1 item)
        public async Task jnr_look()
        {
            var eim = getEventInstance();
            if (eim == null) return;

            var book = "stg1_b" + (getNpcObjectId() % 26);
            var res = eim.getIntProperty(book);
            if (res > -1)
            {
                eim.setIntProperty(book, -1);
                switch (res)
                {
                    case 0:
                        var mgain = (int)(500 * getPlayer().getMesoRate());
                        await SayNext("获得了" + mgain + "金币！");
                        await gainMeso(mgain);
                        break;
                    case 1:
                        var egain = (int)(500 * getPlayer().getExpRate());
                        await SayNext("获得了 " + egain + " 经验值！");
                        await gainExp(egain);
                        break;
                    case 2:
                        if (!canHold(4001130))
                        {
                            await SayOK("你收到了一封信，但是你的背包已经满了，所以你把它放了回去。");
                            return;
                        }
                        await gainItem(4001130, 1);
                        await SayNext("你找到了一封信，看起来是有意放在这里的。");
                        break;
                    case 3:
                        await SayNext("你找到了通往下一阶段的触发器。");
                        await eim.showClearEffect();
                        // eim.giveEventPlayersStageReward(1) — not available
                        eim.setIntProperty("statusStg1", 1);
                        await getMap().getReactorByName("d00")!.hitReactor(getClient());
                        break;
                }
            }
            else
            {
                await SayNext("这里什么都没有。");
            }
        }


        // Npc: 2112007 — Investigation Result (Zenumist - stage 1 item)
        public async Task rnj_look()
        {
            var eim = getEventInstance();
            if (eim == null) return;

            var book = "stg1_b" + (getNpcObjectId() % 26);
            var res = eim.getIntProperty(book);
            if (res > -1)
            {
                eim.setIntProperty(book, -1);
                switch (res)
                {
                    case 0:
                        var mgain = (int)(500 * getPlayer().getMesoRate());
                        await SayNext("获得了" + mgain + "金币！");
                        await gainMeso(mgain);
                        break;
                    case 1:
                        var egain = (int)(500 * getPlayer().getExpRate());
                        await SayNext("获得了 " + egain + " 经验值！");
                        await gainExp(egain);
                        break;
                    case 2:
                        if (!canHold(4001131))
                        {
                            await SayOK("你收到了一封信，但是它无法放进你的背包，所以你把它放了回去。");
                            return;
                        }
                        await gainItem(4001131, 1);
                        await SayNext("你找到了一封信，看起来是有意地放在这里的。");
                        break;
                    case 3:
                        await SayNext("你找到了通往下一阶段的触发器。");
                        await eim.showClearEffect();
                        // eim.giveEventPlayersStageReward(1) — not available
                        eim.setIntProperty("statusStg1", 1);
                        await getMap().getReactorByName("d00")!.hitReactor(getClient());
                        break;
                }
            }
            else
            {
                await SayNext("这里什么都没有。");
            }
        }


        // Npc: 2112000 — Yulete boss (Alcadno)
        public async Task yurete_mad()
        {
            await yureteBoss(926100203, 926100401, 9300139, 9300140);
        }

        // Npc: 2112010 — Yulete boss (Zenumist)
        public async Task yurete2_mad()
        {
            await yureteBoss(926110203, 926110401, 9300151, 9300152);
        }

        private async Task yureteBoss(int officeMap, int bossMap, int weakBossId, int strongBossId)
        {
            var eim = getEventInstance();
            if (eim == null) return;

            if (getMapId() == officeMap)
            {
                var state = eim.getIntProperty("yuleteTalked");
                if (state < 0)
                {
                    await SayOK("嘿，看来你们有了新伙伴。祝你们和他们玩得开心，我先礼貌地告辞了。");
                }
                else if (playersTooClose())
                {
                    await SayOK("哦，你好。自从你们进入这个范围以来，我一直在监视你们的行动。能够到达这里真是了不起，我对你们表示赞赏。现在看看时间，我现在有一个约会，恐怕我需要离开了。但不用担心，我的助手们会处理你们所有人。现在，如果你们允许的话，我就走了。");
                    eim.setIntProperty("yuleteTalked", -1);
                }
                else if (eim.getIntProperty("npcShocked") == 0)
                {
                    await SayOK("嗯~ 你可真是个狡猾的家伙？不过，这并不重要。自从你们进入这个区域以来，我一直在#b监视你们的行动#k。能够到达这里真是了不起，我对你们都表示赞赏。现在，看看时间，我现在有个约会，恐怕我得离开了。但不用担心，我的#r助手们#k会处理你们所有人。现在，如果你允许的话，我就走了。");
                    eim.setIntProperty("yuleteTalked", -1);
                }
                else
                {
                    await SayOK("哈哈！什么，怎么--你是怎么到这里来的？！我以为我已经封闭了所有的路径！不要紧，这种情况很快就会解决。伙计们：部署#rmaster武器#k！！你！是的，就是你。你难道不觉得这就此结束了吗，回头看看你的同伴，他们需要一些帮助！我现在就撤退。");
                    eim.setIntProperty("yuleteTalked", 1);
                }
            }
            else
            {
                if (eim.isEventCleared())
                {
                    await SayOK("不要啊... 我被打败了？但是为什么？我所做的一切都是为了更伟大的炼金术的发展！你们不能把我关起来，我所做的只是站在我这样的位置上的每个人都会做的事情！但是不，他们就决定阻碍科学的进步，只是因为它被认为是危险的？哦，得了吧！");
                }
                else
                {
                    var passed = eim.getIntProperty("yuletePassed");
                    if (passed < 0)
                    {
                        await SayOK("瞧！这就是马加提亚炼金术研究的顶峰！哈哈哈哈哈哈……");
                    }
                    else if (passed == 0)
                    {
                        await SayOK("你们真是一群讨厌的家伙，哎。好吧，我向你们展示我的最新武器，由最优秀的炼金术带来，#r弗兰肯罗伊德#k。");
                        await eim.dropMessage(5, "Yulete: I present you my newest weapon, brought by the finest alchemy, Frankenroid!");

                        var mapObj = await eim.getMapInstance(bossMap);
                        var boss = LifeFactory.Instance.GetMonsterTrust(weakBossId);
                        await mapObj.spawnMonsterOnGroundBelow(boss, new Point(250, 100));

                        eim.setIntProperty("statusStg7", 1);
                        eim.setIntProperty("yuletePassed", -1);
                    }
                    else
                    {
                        await SayOK("你们这些家伙真是让人头疼，哎。好吧，我向你们展示我的最新武器，由阿尔卡德诺和泽纳米斯最精湛的炼金术结合而成，那些令马加提亚社会中无聊的人们禁止携带的东西，#rmighty Frankenroid#k！");
                        await eim.dropMessage(5, "Yulete: I present you my newest weapon, brought by the finest combined alchemy of Alcadno's and Zenumist's, those that the boring people of Magatia societies have banned to bring along, the mighty Frankenroid!!");

                        var mapObj = await eim.getMapInstance(bossMap);
                        var boss = LifeFactory.Instance.GetMonsterTrust(strongBossId);
                        await mapObj.spawnMonsterOnGroundBelow(boss, new Point(250, 100));

                        eim.setIntProperty("statusStg7", 2);
                        eim.setIntProperty("yuletePassed", -1);
                    }
                }
            }
        }

        private bool playersTooClose()
        {
            var npcObj = getMap().getMapObject(getNpcObjectId());
            if (npcObj == null) return false;
            var npcPos = npcObj.getPosition();
            foreach (var chr in getMap().getAllPlayers())
            {
                var chrPos = chr.getPosition();
                var dist = Math.Sqrt(Math.Pow(npcPos.X - chrPos.X, 2) + Math.Pow(npcPos.Y - chrPos.Y, 2));
                if (dist < 310)
                    return true;
            }
            return false;
        }


        // Npc: 2112001 — Yulete defeated
        public async Task yurete_dead()
        {
            var sel = await AskMenu("被打败了...如此，尤莱特的传承终将终结。哦，这是多么的悲哀...希望你们现在很开心，因为我将度过余生在一个黑暗的地窖里。我所做的一切都是为了马加提亚的利益！！（哭泣）#Ll# 嘿，伙计，振作点！这里没有太多无法解决的损害。马加提亚制定了这些严厉的法律，是为了保护它的人民免受像这样的强大力量落入错误之手时所带来的危害。这并不是你的终结，接受社会的康复，一切都会好起来的！#l");
            await SayNext("…在我所做的一切之后，你们原谅我了吗？嗯，我想我被那种可以通过这种方式发现的巨大力量冲昏了头脑，也许他们说得对，人类不能简单地理解并运用这些力量，而不在途中腐化自己…我深感抱歉，为了弥补自己对每个人，我愿意在炼金术的进展中再次帮助各个组织。谢谢。");
            if (!isQuestCompleted(7770))
                await completeQuest(7770);
            await warp(926100600);
        }


        // Npc: 2112003 — Juliet entrance (Alcadno PQ)
        public async Task juliet_start()
        {
            await magatiaPQStart("MagatiaPQ_A", 261000021, "阿尔卡德诺",
                "我的心爱的罗密欧被绑架了！虽然他是泽尼玛斯的人，但我不能坐视不理，看着他因为这场愚蠢的冲突而受苦。我需要你和你的同伴们帮助我救他！拜托，帮帮我们！！请让你的#b队长#k和我交谈。");
        }

        // Npc: 2112004 — Romeo entrance (Zenumist PQ)
        public async Task romio_start()
        {
            await magatiaPQStart("MagatiaPQ_Z", 261000011, "泽尼姆斯特",
                "我心爱的朱丽叶被绑架了！虽然她是阿尔卡德诺家的人，但我不能坐视她因这场愚蠢的冲突而受苦。我需要你和你的同伴们帮助我救她！拜托，帮帮我们！！请让你的#b队长#k和我谈谈。");
        }

        private async Task magatiaPQStart(string eventName, int homeMap, string societyName, string intro)
        {
            if (getMapId() != homeMap)
            {
                if (await AskYesNo("我们必须继续战斗，拯救" + (societyName == "阿尔卡德诺" ? "罗密欧" : "朱丽叶") + "，请保持你的速度。如果你感觉不太好，无法继续，你的同伴和我都会理解……那么，你打算撤退吗？"))
                    await warp(societyName == "阿尔卡德诺" ? 926110700 : 926100700, 0);
                return;
            }

            var em = getEventManager(eventName);
            if (em == null)
            {
                await SayOK("玛加提亚组队任务（" + societyName + "）遇到了一个错误。");
                return;
            }

            if (isUsingOldPqNpcStyle())
            {
                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                return;
            }

            var sel = await AskMenu("#e#b<组队任务：罗密欧与朱丽叶>\r\n#k#n\r\n" + intro + "#b\r\n#L0#我想参加这个组队任务。\r\n#L1#我想了解更多细节。#l");
            if (sel == 0)
            {
                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
            }
            else
            {
                await SayOK("不久前，一位名叫尤利特的科学家因为他对阿尔卡德诺和泽诺米斯的合成炼金术的研究而被这个城镇放逐。由于这种组合所带来的巨大力量，根据法律是禁止研究的。然而，他无视了这项法律，同时进行了这两项研究。结果，他被流放了。\r\n他现在在报复，已经带走了我心爱的人，下一个目标是我，因为我们是玛加提亚的重要人物，是这两个社会的继承者。但我不害怕。我们必须不惜一切代价把他救回来！");
            }
        }


        // Npc: 2112005 — Juliet in-quest NPC (Alcadno)
        public async Task juliet()
        {
            await magatiaInQuestNPC("jnr3_out3", 4001130, 926110700);
        }

        // Npc: 2112006 — Romeo in-quest NPC (Zenumist)
        public async Task romio()
        {
            await magatiaInQuestNPC("rnj3_out3", 4001131, 926100700);
        }

        private async Task magatiaInQuestNPC(string doorReactor, int letterId, int retreatMap)
        {
            var eim = getEventInstance();
            if (eim == null) return;

            if (!eim.isEventCleared())
            {
                if (eim.getIntProperty("npcShocked") == 0 && haveItem(letterId, 1))
                {
                    await gainItem(letterId, -1);
                    eim.setIntProperty("npcShocked", 1);
                    await SayNext("哦？你给我信？像这样的时候，应该是什么…… 哇！伙计们，有大事发生了。集合起来，从现在开始，事情会比以往更加艰难！");
                    await eim.dropMessage(6, (letterId == 4001130 ? "Juliet" : "Romeo") + " seemed very much in shock after reading " + (letterId == 4001130 ? "Romeo" : "Juliet") + "'s Letter.");
                    return;
                }

                if (eim.getIntProperty("statusStg4") == 1)
                {
                    var door = getMap().getReactorByName(doorReactor);
                    if (door != null && door.getState() == 0)
                    {
                        await SayNext("让我为你开门。");
                        await door.hitReactor(getClient());
                    }
                    else
                    {
                        await SayNext("请快点，" + (letterId == 4001130 ? "罗密欧" : "朱丽叶") + "有麻烦了。");
                    }
                    return;
                }

                if (haveItem(4001134, 1) && haveItem(4001135, 1))
                {
                    if (isEventLeader())
                    {
                        await gainItem(4001134, -1);
                        await gainItem(4001135, -1);
                        await SayNext("太好了！你手头上有艾尔卡德诺和泽纳米斯特的文件。现在我们可以继续了。");

                        await eim.showClearEffect();
                        // eim.giveEventPlayersStageReward(4) — not available
                        eim.setIntProperty("statusStg4", 1);

                        await getMap().killAllMonsters();
                        await getMap().getReactorByName(doorReactor)!.hitReactor(getClient());
                    }
                    else
                    {
                        await SayOK("请让你们的队长把文件传给我。");
                    }
                    return;
                }

                if (await AskYesNo("我们必须继续战斗，拯救" + (letterId == 4001130 ? "罗密欧" : "朱丽叶") + "，请保持你的速度。如果你感觉不太好，无法继续，你的同伴和我都会理解……那么，你打算撤退吗？"))
                    await warp(retreatMap, 0);
            }
            else
            {
                await SayNext(eim.getIntProperty("escortFail") == 0
                    ? "最终，" + (letterId == 4001130 ? "罗密欧" : "朱丽叶") + "安全了！多亏了你的努力，我们才能将他从尤莱特的魔爪中解救出来，尤莱特现在将因为反抗马加提亚而受到审判。从现在开始，他将开始接受康复治疗，我们将密切关注他的努力，确保他将不再在未来制造麻烦。"
                    : (letterId == 4001130 ? "罗密欧" : "朱丽叶") + "现在安全了，尽管战斗对他造成了一定的伤害...多亏了你们的努力，我们才能将他从尤利特的魔爪中解救出来，尤利特现在将因其反抗马加提亚而受到审判。谢谢你们。");

                await SayNext("现在，请将这份礼物视为我们对你的感激之情的接受表示。");

                var rewardId = letterId == 4001130 ? 4001160 : 4001159;
                if (canHold(rewardId))
                {
                    await gainItem(rewardId, 1);
                    await warp(eim.getIntProperty("normalClear") == 1
                        ? (letterId == 4001130 ? 926110600 : 926100600)
                        : (letterId == 4001130 ? 926110500 : 926100500), 0);
                }
                else
                {
                    await SayOK("确保你的杂项物品栏有空间。");
                }
            }
        }


        // Npc: 2112008 — No JS file exists
        public Task juliet_dead()
        {
            return Task.CompletedTask;
        }

        // Npc: 2112009 — No JS file exists
        public Task romio_dead()
        {
            return Task.CompletedTask;
        }
    }
}
