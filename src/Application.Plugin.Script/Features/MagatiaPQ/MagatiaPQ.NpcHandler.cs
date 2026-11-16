using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;
using Application.Core.Server.life;
using System.Drawing;
using Application.Plugin.Script;
using Application.Resources.Messages;
using Application.Core.scripting.Events.Templates;
using Application.Core.Scripting.Events;

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
        public Task jnr_look() => Look(4001130, this.GetTalkMessage(ScriptMessage.Magatia_LetterBackpackFull));

        // Npc: 2112007 — Investigation Result（Zenumist，信是 4001131）
        public Task rnj_look() => Look(4001131, this.GetTalkMessage(ScriptMessage.Magatia_LetterCannotHold));

        async Task Look(int letterId, string noSlotMessage)
        {
            var eim = getEventInstance();
            if (eim == null) return;

            var book = "stg1_b" + (getNpcObjectId() % 26);
            var res = eim.getIntProperty(book);
            if (res <= -1)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Magatia_InvestigateNothing));
                return;
            }

            eim.setIntProperty(book, -1);
            switch (res)
            {
                case 0:
                    var mgain = (int)(500 * getPlayer().getMesoRate());
                    await SayNext(this.GetTalkMessage(ScriptMessage.Magatia_EarnedMesos, mgain));
                    await gainMeso(mgain);
                    break;
                case 1:
                    var egain = (int)(500 * getPlayer().getExpRate());
                    await SayNext(this.GetTalkMessage(ScriptMessage.Magatia_EarnedExp, egain));
                    await gainExp(egain);
                    break;
                case 2:
                    if (!canHold(letterId))
                    {
                        await SayOK(noSlotMessage);
                        return;
                    }
                    await gainItem(letterId, 1);
                    await SayNext(this.GetTalkMessage(ScriptMessage.Magatia_FoundLetter));
                    break;
                case 3:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Magatia_FoundTrigger));
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
                await SayNext(this.GetTalkMessage(ScriptMessage.Magatia_ExitYuleteJailed));
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Magatia_ExitYuleteRehab));
            }

            if (await eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
            {
                await warp(eim.getIntProperty("isAlcadno") == 0 ? 261000011 : 261000021);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Magatia_NeedInventorySlot));
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
                    await SayOK(this.GetTalkMessage(ScriptMessage.Magatia_Yulete_LeavePolite));
                }
                else if (playersTooClose())
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Magatia_Yulete_Monitoring));
                    eim.setIntProperty("yuleteTalked", -1);
                }
                else if (eim.getIntProperty("npcShocked") == 0)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Magatia_Yulete_Sneaky));
                    eim.setIntProperty("yuleteTalked", -1);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Magatia_Yulete_DeployWeapon));
                    eim.setIntProperty("yuleteTalked", 1);
                }
                return;
            }

            if (eim.isEventCleared())
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Magatia_Yulete_Defeated));
                return;
            }

            var passed = eim.getIntProperty("yuletePassed");
            if (passed == -1)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Magatia_Yulete_Behold));
                return;
            }

            var mapObj = await eim.getMapInstance(mapBase + 401);
            var weak = passed == 0;

            if (weak)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Magatia_Yulete_Frankenroid));
                await eim.Pink(ScriptMessageCategories.ScriptMessage, ScriptMessage.MagatiaPQNpcHandler_YuletePresent);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Magatia_Yulete_MightyFrankenroid));
                await eim.Pink(ScriptMessageCategories.ScriptMessage, ScriptMessage.MagatiaPQNpcHandler_YuletePresentNewest);
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
                this.GetTalkMessage(ScriptMessage.Magatia_YuleteDefeatedMenu));
            await SayNext(this.GetTalkMessage(ScriptMessage.Magatia_Yulete_Regret));

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
        public Task juliet_start() => MagatiaPQStart(MagatiaPQFeature.AlcadnoEventName);

        // Npc: 2112004 — Romeo entrance（Zenumist PQ）
        public Task romio_start() => MagatiaPQStart(MagatiaPQFeature.ZenumistEventName);

        async Task MagatiaPQStart(string eventName)
        {
            var isAlcadno = eventName == MagatiaPQFeature.AlcadnoEventName;
            var eim = getEventInstance();
            if (eim != null)
            {
                if (await AskYesNo(this.GetTalkMessage(isAlcadno
                        ? ScriptMessage.Magatia_Juliet_Retreat
                        : ScriptMessage.Magatia_Romeo_Retreat)))
                    await eim.exitPlayer(getPlayer());
                return;
            }
            var em = GetEventManager<PartyQuestEventManager>(eventName);
            if (isUsingOldPqNpcStyle())
            {
                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                return;
            }

            var sel = await AskMenu(this.GetTalkMessage(isAlcadno ? ScriptMessage.Magatia_Juliet_EntryMenu: ScriptMessage.Magatia_Romeo_EntryMenu));
            if (sel == 0)
            {
                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
            }
            else
            {
                await SayOK(this.GetTalkMessage(isAlcadno ? ScriptMessage.Magatia_Juliet_Intro : ScriptMessage.Magatia_Romeo_Intro));
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

            var partner = letterId == 4001130 ? this.GetTalkMessage(ScriptMessage.Magatia_NameRomeo) : this.GetTalkMessage(ScriptMessage.Magatia_NameJuliet);

            if (!eim.isEventCleared())
            {
                if (eim.getIntProperty("npcShocked") == 0 && haveItem(letterId, 1))
                {
                    await gainItem(letterId, -1);
                    eim.setIntProperty("npcShocked", 1);
                    await SayNext(this.GetTalkMessage(ScriptMessage.Magatia_BroughtLetter));
                    await eim.LightBlue(chr => chr.GetTalkMessage(ScriptMessage.Magatia_ShockAfterLetter, letterId == 4001130 ? chr.GetTalkMessage(ScriptMessage.Magatia_NameJuliet) : chr.GetTalkMessage(ScriptMessage.Magatia_NameRomeo), partner));
                    return;
                }

                if (eim.getIntProperty("statusStg4") == 1)
                {
                    var door = getMap().getReactorByName(doorReactor);
                    if (door != null && door.getState() == 0)
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Magatia_OpenDoor));
                        await door.hitReactor(getClient());
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Magatia_PleaseHurry) + partner + this.GetTalkMessage(ScriptMessage.Magatia_Trouble));
                    }
                    return;
                }

                if (haveItem(4001134, 1) && haveItem(4001135, 1))
                {
                    if (!isEventLeader())
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Magatia_GiveDocsToLeader));
                        return;
                    }

                    await gainItem(4001134, -1);
                    await gainItem(4001135, -1);
                    await SayNext(this.GetTalkMessage(ScriptMessage.Magatia_GotBothDocs));

                    await eim.showClearEffect();
                    await eim.GiveStageClearRewardAll(4);
                    eim.setIntProperty("statusStg4", 1);

                    await getMap().killAllMonsters();
                    await getMap().getReactorByName(doorReactor)!.hitReactor(getClient());
                    return;
                }

                if (await AskYesNo(this.GetTalkMessage(letterId == 4001130
                        ? ScriptMessage.Magatia_Juliet_Retreat
                        : ScriptMessage.Magatia_Romeo_Retreat, partner)))
                    await warp(mapBase + 700, 0);
                return;
            }

            // 已通关：发奖励并送回出口
            await SayNext(eim.getIntProperty("escortFail") == 0
                ? this.GetTalkMessage(ScriptMessage.Magatia_ExitYuleteRehab)
                : this.GetTalkMessage(ScriptMessage.Magatia_ExitYuleteJailed));

            await SayNext(this.GetTalkMessage(ScriptMessage.Magatia_ThisGift));

            if (!canHold(rewardId))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Magatia_NeedEtcSlot));
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
