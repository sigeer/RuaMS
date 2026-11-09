using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;
using Application.Core.Server.life;
using System.Drawing;

namespace Application.Plugin.Script.Features.MagatiaPQ
{
    /// <summary>
    /// 玛加提亚组队任务的 NPC 脚本。对应 JS <c>scripts/npc/2112000.js</c>、<c>2112001.js</c>、
    /// <c>2112003.js</c>、<c>2112004.js</c>、<c>2112005.js</c>、<c>2112006.js</c>、
    /// <c>2112007.js</c>、<c>2112010.js</c>、<c>2112011.js</c>、<c>2112013.js</c>、
    /// <c>2112018.js</c> 与 <c>MagatiaPassword.js</c>。
    /// </summary>
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        #region 第一阶段：调查书柜

        // Npc: 2112013 — Investigation Result（Alcadno，信是 4001130）
        public Task jnr_look() => Look(4001130, "你收到了一封信，但是你的背包已经满了，所以你把它放了回去。");

        // Npc: 2112007 — Investigation Result（Zenumist，信是 4001131）
        public Task rnj_look() => Look(4001131, "你收到了一封信，但是它无法放进你的背包，所以你把它放了回去。");

        async Task Look(int letterId, string noSlotMessage)
        {
            var eim = getEventInstance();
            if (eim == null) return;

            var book = "stg1_b" + (getNpcObjectId() % 26);
            var res = eim.getIntProperty(book);
            if (res <= -1)
            {
                await SayNext("这里什么都没有。");
                return;
            }

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
                    if (!canHold(letterId))
                    {
                        await SayOK(noSlotMessage);
                        return;
                    }
                    await gainItem(letterId, 1);
                    await SayNext("你找到了一封信，看起来是有意放在这里的。");
                    break;
                case 3:
                    await SayNext("你找到了通往下一阶段的触发器。");
                    await eim.showClearEffect();
                    await eim.GiveStageClearRewardAll(1);
                    eim.setIntProperty("statusStg1", 1);
                    await getMap().getReactorByName("d00")!.hitReactor(getClient());
                    break;
            }
        }

        #endregion

        #region 通关后领奖

        // Npc: 2112018 — Romeo & Juliet，玛加提亚 PQ 出口
        public async Task rnj_start()
        {
            var eim = GetEventInstanceTrust();
            if (eim.getIntProperty("escortFail") == 1)
            {
                await SayNext("多亏了你，我们得以再次团聚。尤莱特现在将因触犯马加提亚法律而被送进监狱。再次感谢你。");
            }
            else
            {
                await SayNext("谢谢你，因为你，我们得以再次团聚。尤勒特现在将进行康复，因为他的研究对我们镇的发展至关重要，他的所作所为都是出于对权力的贪婪，尽管是为了马加提亚的利益。再次感谢你。");
            }

            if (await eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
            {
                await warp(eim.getIntProperty("isAlcadno") == 0 ? 261000011 : 261000021);
            }
            else
            {
                await SayOK("请在领取奖励前为您的物品栏腾出一个空位。");
            }
        }

        #endregion

        #region 犹泰（Yulete）

        // Npc: 2112000 — Yulete 办公室（Zenumist，926100203）
        public Task yurete_mad() => YuleteBoss(MapBaseOf(isAlcadno: false), 9300139, 9300140);

        // Npc: 2112010 — Yulete 办公室（Alcadno，926110203）
        public Task yurete2_mad() => YuleteBoss(MapBaseOf(isAlcadno: true), 9300151, 9300152);

        static int MapBaseOf(bool isAlcadno) =>
            isAlcadno ? MagatiaPQFeature.AlcadnoMapBase : MagatiaPQFeature.ZenumistMapBase;

        async Task YuleteBoss(int mapBase, int weakBossId, int strongBossId)
        {
            var eim = getEventInstance();
            if (eim == null) return;

            if (getMapId() == mapBase + MagatiaPQEventBase.OfficeOffset)
            {
                var state = eim.getIntProperty("yuleteTalked");
                if (state == -1)
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
                return;
            }

            if (eim.isEventCleared())
            {
                await SayOK("不要啊... 我被打败了？但是为什么？我所做的一切都是为了更伟大的炼金术的发展！你们不能把我关起来，我所做的只是站在我这样的位置上的每个人都会做的事情！但是不，他们就决定阻碍科学的进步，只是因为它被认为是危险的？哦，得了吧！");
                return;
            }

            var passed = eim.getIntProperty("yuletePassed");
            if (passed == -1)
            {
                await SayOK("瞧！这就是马加提亚炼金术研究的顶峰！哈哈哈哈哈哈……");
                return;
            }

            var mapObj = await eim.getMapInstance(mapBase + 401);
            var weak = passed == 0;

            if (weak)
            {
                await SayOK("你们真是一群讨厌的家伙，哎。好吧，我向你们展示我的最新武器，由最优秀的炼金术带来，#r弗兰肯罗伊德#k。");
                await eim.dropMessage(5, "Yulete: I present you my newest weapon, brought by the finest alchemy, Frankenroid!");
            }
            else
            {
                await SayOK("你们这些家伙真是让人头疼，哎。好吧，我向你们展示我的最新武器，由阿尔卡德诺和泽纳米斯最精湛的炼金术结合而成，那些令马加提亚社会中无聊的人们禁止携带的东西，#rmighty Frankenroid#k！");
                await eim.dropMessage(5, "Yulete: I present you my newest weapon, brought by the finest combined alchemy of Alcadno's and Zenumist's, those that the boring people of Magatia societies have banned to bring along, the mighty Frankenroid!!");
            }

            await mapObj.spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(weak ? weakBossId : strongBossId), new Point(250, 100));

            eim.setIntProperty("statusStg7", weak ? 1 : 2);
            eim.setIntProperty("yuletePassed", -1);
        }

        // Npc: 2112001 — Yulete 被打败（Zenumist，926100500）
        public Task yurete_dead() => YuleteDead(MagatiaPQFeature.ZenumistMapBase + 600);

        // Npc: 2112011 — yurete2_dead 在 Features/Quest/Q7770NpcHandler.cs（任务 7770 相关）

        async Task YuleteDead(int targetMap)
        {
            await AskMenu(
                "被打败了...这就是犹泰的遗产将如何结束的方式，哦，这是多么的悲哀...希望你们现在很开心，因为我将度过余生在一个黑暗的地窖里。我所做的一切都是为了马加提亚的利益！！（哭泣）",
                ["嘿，伙计，振作点！这里没有太多无法解决的损害。马加提亚制定了这些严厉的法律，是为了保护它的人民免受像这样的强大力量落入错误之手时所带来的危害。这并不是你的终结，接受社会的康复，一切都会好起来的！"]);
            await SayNext("…在我所做的一切之后，你们原谅我了吗？嗯，我想我被那种可以通过这种方式发现的巨大力量冲昏了头脑，也许他们说得对，人类不能简单地理解并运用这些力量，而不在途中腐化自己…我深感抱歉，为了弥补自己对每个人，我愿意在炼金术的进展中再次帮助各个组织。谢谢。");

            if (!isQuestCompleted(7770))
            {
                await completeQuest(7770);
            }
            await warp(targetMap, 0);
        }

        bool playersTooClose()
        {
            var npcObj = getMap().getMapObject(getNpcObjectId());
            if (npcObj == null) return false;

            var npcPos = npcObj.getPosition();
            return getMap().getAllPlayers().Any(chr =>
            {
                var chrPos = chr.getPosition();
                return Math.Sqrt(Math.Pow(npcPos.X - chrPos.X, 2) + Math.Pow(npcPos.Y - chrPos.Y, 2)) < 310;
            });
        }

        #endregion

        #region 入口 NPC（朱丽叶 / 罗密欧）

        // Npc: 2112003 — Juliet entrance（Alcadno PQ）
        public Task juliet_start() => MagatiaPQStart(MagatiaPQFeature.AlcadnoEventName, 261000021, "阿尔卡德诺",
            "我心爱的罗密欧被绑架了！虽然他是泽尼玛斯的人，但我不能坐视不理，看着他因为这场愚蠢的冲突而受苦。我需要你和你的同伴们帮助我救他！拜托，帮帮我们！！请让你的#b队长#k和我交谈。");

        // Npc: 2112004 — Romeo entrance（Zenumist PQ）
        public Task romio_start() => MagatiaPQStart(MagatiaPQFeature.ZenumistEventName, 261000011, "泽尼姆斯特",
            "我心爱的朱丽叶被绑架了！虽然她是阿尔卡德诺家的人，但我不能坐视她因这场愚蠢的冲突而受苦。我需要你和你的同伴们帮助我救她！拜托，帮帮我们！！请让你的#b队长#k和我谈谈。");

        async Task MagatiaPQStart(string eventName, int homeMap, string societyName, string intro)
        {
            var partner = societyName == "阿尔卡德诺" ? "罗密欧" : "朱丽叶";
            var exitMap = societyName == "阿尔卡德诺" ? 926110700 : 926100700;

            if (getMapId() != homeMap)
            {
                if (await AskYesNo("我们必须继续战斗，拯救" + partner + "，请保持你的速度。如果你感觉不太好，无法继续，你的同伴和我都会理解……那么，你打算撤退吗？"))
                    await warp(exitMap, 0);
                return;
            }

            var em = GetEventManager(eventName);
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

        // Npc: 2112005 — Juliet in-quest（Alcadno）
        public Task juliet() => InQuestNpc("jnr3_out3", 4001130, 4001160, MagatiaPQFeature.AlcadnoMapBase);

        // Npc: 2112006 — Romeo in-quest（Zenumist）
        public Task romio() => InQuestNpc("rnj3_out3", 4001131, 4001159, MagatiaPQFeature.ZenumistMapBase);

        async Task InQuestNpc(string doorReactor, int letterId, int rewardId, int mapBase)
        {
            var eim = getEventInstance();
            if (eim == null) return;

            var partner = letterId == 4001130 ? "罗密欧" : "朱丽叶";

            if (!eim.isEventCleared())
            {
                if (eim.getIntProperty("npcShocked") == 0 && haveItem(letterId, 1))
                {
                    await gainItem(letterId, -1);
                    eim.setIntProperty("npcShocked", 1);
                    await SayNext("哦？你给我信？像这样的时候，应该是什么…… 哇！伙计们，有大事发生了。集合起来，从现在开始，事情会比以往更加艰难！");
                    await eim.dropMessage(6, (letterId == 4001130 ? "Juliet" : "Romeo") + " seemed very much in shock after reading " + partner + "'s Letter.");
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
                        await SayNext("请快点，" + partner + "有麻烦了。");
                    }
                    return;
                }

                if (haveItem(4001134, 1) && haveItem(4001135, 1))
                {
                    if (!isEventLeader())
                    {
                        await SayOK("请让你们的队长把文件传给我。");
                        return;
                    }

                    await gainItem(4001134, -1);
                    await gainItem(4001135, -1);
                    await SayNext("太好了！你手头上有艾尔卡德诺和泽纳米斯特的文件。现在我们可以继续了。");

                    await eim.showClearEffect();
                    await eim.GiveStageClearRewardAll(4);
                    eim.setIntProperty("statusStg4", 1);

                    await getMap().killAllMonsters();
                    await getMap().getReactorByName(doorReactor)!.hitReactor(getClient());
                    return;
                }

                if (await AskYesNo("我们必须继续战斗，拯救" + partner + "，请保持你的速度。如果你感觉不太好，无法继续，你的同伴和我都会理解……那么，你打算撤退吗？"))
                    await warp(mapBase + 700, 0);
                return;
            }

            // 已通关：发奖励并送回出口
            await SayNext(eim.getIntProperty("escortFail") == 0
                ? "最终，" + partner + "安全了！多亏了你的努力，我们才能将他从尤莱特的魔爪中解救出来，尤莱特现在将因为反抗马加提亚而受到审判。从现在开始，他将开始接受康复治疗，我们将密切关注他的努力，确保他将不再在未来制造麻烦。"
                : partner + "现在安全了，尽管战斗对他造成了一定的伤害...多亏了你们的努力，我们才能将他从尤利特的魔爪中解救出来，尤利特现在将因其反抗马加提亚而受到审判。谢谢你们。");

            await SayNext("现在，请将这份礼物视为我们对你的感激之情的接受表示。");

            if (!canHold(rewardId))
            {
                await SayOK("确保你的杂项物品栏有空间。");
                return;
            }

            await gainItem(rewardId, 1);
            // 正常通关（击破弱 boss）送到 600，直接打强 boss 的送到 500
            await warp(mapBase + (eim.getIntProperty("normalClear") == 1 ? 600 : 500), 0);
        }

        #endregion

        #region 其他

        // Npc: 2112008 — 无 JS 实现
        public Task juliet_dead() => Task.CompletedTask;

        // Npc: 2112009 — 无 JS 实现
        public Task romio_dead() => Task.CompletedTask;

        #endregion
    }
}
