/*
 This file is part of the OdinMS Maple Story Server
 Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
 Matthias Butz <matze@odinms.de>
 Jan Christian Meyer <vimes@odinms.de>

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU Affero General Public License as
 published by the Free Software Foundation version 3 as published by
 the Free Software Foundation. You may not use, modify or distribute
 this program under any other version of the GNU Affero General Public
 License.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Affero General Public License for more details.

 You should have received a copy of the GNU Affero General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Application.Core.Channel.DataProviders;
using Application.Core.Game.Packets;
using Application.Templates.Quest;
using client;
using server.quest.actions;
using server.quest.requirements;
using tools;
using static Application.Core.Game.Players.Player;
using static client.QuestStatus;

namespace server.quest;

/**
 * @author Matze
 * @author Ronan - support for medal quests
 */
public class Quest
{
    protected short id;
    protected int timeLimit, timeLimit2;
    protected Dictionary<QuestRequirementType, AbstractQuestRequirement> startReqs = new();
    protected Dictionary<QuestRequirementType, AbstractQuestRequirement> completeReqs = new();
    protected Dictionary<QuestActionType, AbstractQuestAction> startActs = new();
    protected Dictionary<QuestActionType, AbstractQuestAction> completeActs = new();
    protected List<int> relevantMobs = new();
    private bool autoStart;
    private bool autoPreComplete, autoComplete;
    private bool repeatable = false;
    public string Name { get; set; }
    public string? Parent { get; set; }
    public bool IsValid { get; } = true;

    public Quest(QuestTemplate template)
    {
        id = (short)template.TemplateId;
        autoComplete = template.Info.AutoComplete;
        autoStart = template.Info.AutoStart;
        autoPreComplete = template.Info.AutoPreComplete;
        Name = template.Info.Name;
        Parent = template.Info.Parent ?? "";
        timeLimit = template.Info.TimeLimit;
        timeLimit2 = template.Info.TimeLimit2;
        if (template.Info.ViewMedalItem > 0)
            QuestFactory.Instance.AddMedal(id, template.Info.ViewMedalItem);

        if (template.Check?.StartDemand != null)
        {
            var data = template.Check.StartDemand;
            repeatable = data.Interval > 0;
            if (data.DemandMob.Length > 0)
                relevantMobs.AddRange(data.DemandMob.Select(x => x.MobID));
            startReqs = GetRequirement(this, data);
        }

        if (template.Check?.EndDemand != null)
        {
            var data = template.Check.EndDemand;
            if (data.DemandMob.Length > 0)
                relevantMobs.AddRange(data.DemandMob.Select(x => x.MobID));
            completeReqs = GetRequirement(this, data);
        }


        if (template.Act?.StartAct != null)
        {
            var data = template.Act.StartAct;
            startActs = GetAction(this, data);
        }

        if (template.Act?.EndAct != null)
        {
            var data = template.Act.EndAct;
            completeActs = GetAction(this, data);
        }
    }


    public bool isAutoComplete()
    {
        return autoPreComplete || autoComplete;
    }

    public bool isAutoStart()
    {
        return autoStart;
    }

    public static Quest getInstance(int id)
    {
        return QuestFactory.Instance.GetInstance(id);
    }



    public bool isSameDayRepeatable()
    {
        if (!repeatable)
        {
            return false;
        }

        var ir = startReqs.GetValueOrDefault(QuestRequirementType.INTERVAL) as IntervalRequirement;
        return ir != null && ir.getInterval() < TimeSpan.FromHours(YamlConfig.config.server.QUEST_POINT_REPEATABLE_INTERVAL).TotalMilliseconds;
    }

    public bool canStartQuestByStatus(IPlayer chr)
    {
        QuestStatus mqs = chr.getQuest(this);
        return !(!mqs.getStatus().Equals(Status.NOT_STARTED) && !(mqs.getStatus().Equals(Status.COMPLETED) && repeatable));
    }

    public bool canQuestByInfoProgress(IPlayer chr)
    {
        QuestStatus mqs = chr.getQuest(this);
        List<string> ix = mqs.getInfoEx();
        if (ix.Count > 0)
        {
            short questid = mqs.getQuestID();
            short infoNumber = mqs.getInfoNumber();
            if (infoNumber <= 0)
            {
                infoNumber = questid;  // on default infoNumber mimics questid
            }

            int ixSize = ix.Count;
            for (int i = 0; i < ixSize; i++)
            {
                string progress = chr.getAbstractPlayerInteraction().getQuestProgress(infoNumber, i);
                string ixProgress = ix.get(i);

                if (progress != (ixProgress))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool canStart(IPlayer chr, int npcid)
    {
        if (!canStartQuestByStatus(chr))
        {
            return false;
        }

        foreach (AbstractQuestRequirement r in startReqs.Values)
        {
            if (!r.check(chr, npcid))
            {
                return false;
            }
        }

        return canQuestByInfoProgress(chr);
    }

    public bool canComplete(IPlayer chr, int? npcid)
    {
        QuestStatus mqs = chr.getQuest(this);
        if (!mqs.getStatus().Equals(Status.STARTED))
        {
            return false;
        }

        foreach (AbstractQuestRequirement r in completeReqs.Values)
        {
            if (!r.check(chr, npcid))
            {
                return false;
            }
        }

        return canQuestByInfoProgress(chr);
    }

    public void start(IPlayer chr, int npc)
    {
        if (autoStart || canStart(chr, npc))
        {
            var acts = startActs.Values;
            foreach (AbstractQuestAction a in acts)
            {
                if (!a.check(chr, null))
                { // would null be good ?
                    return;
                }
            }
            foreach (AbstractQuestAction a in acts)
            {
                a.run(chr, null);
            }
            forceStart(chr, npc);
        }
    }

    public void complete(IPlayer chr, int npc)
    {
        complete(chr, npc, null);
    }

    public void complete(IPlayer chr, int npc, int? selection)
    {
        if (autoPreComplete || canComplete(chr, npc))
        {
            var acts = completeActs.Values;
            foreach (AbstractQuestAction a in acts)
            {
                if (!a.check(chr, selection))
                {
                    return;
                }
            }
            forceComplete(chr, npc);
            foreach (AbstractQuestAction a in acts)
            {
                a.run(chr, selection);
            }
            if (!this.hasNextQuestAction())
            {
                chr.announceUpdateQuest(DelayedQuestUpdate.INFO, chr.getQuest(this));
            }
        }
    }

    public void reset(IPlayer chr)
    {
        QuestStatus newStatus = new QuestStatus(this, QuestStatus.Status.NOT_STARTED);
        chr.updateQuestStatus(newStatus);
    }

    public bool forfeit(IPlayer chr)
    {
        if (!chr.getQuest(this).getStatus().Equals(Status.STARTED))
        {
            return false;
        }
        if (timeLimit > 0)
        {
            chr.sendPacket(QuestPacket.RemoveQuestTimeLimit(id));
        }
        QuestStatus newStatus = new QuestStatus(this, QuestStatus.Status.NOT_STARTED);
        newStatus.setForfeited(chr.getQuest(this).getForfeited() + 1);
        chr.updateQuestStatus(newStatus);
        return true;
    }

    public bool forceStart(IPlayer chr, int npc)
    {
        QuestStatus newStatus = new QuestStatus(this, QuestStatus.Status.STARTED, npc);

        QuestStatus oldStatus = chr.getQuest(this.getId());
        foreach (var e in oldStatus.getProgress())
        {
            newStatus.setProgress(e.Key, e.Value);
        }

        if (id / 100 == 35 && YamlConfig.config.server.TOT_MOB_QUEST_REQUIREMENT > 0)
        {
            int setProg = 999 - Math.Min(999, YamlConfig.config.server.TOT_MOB_QUEST_REQUIREMENT);

            foreach (int pid in newStatus.getProgress().Keys)
            {
                if (pid >= 8200000 && pid <= 8200012)
                {
                    string pr = StringUtil.getLeftPaddedStr(setProg.ToString(), '0', 3);
                    newStatus.setProgress(pid, pr);
                }
            }
        }

        newStatus.setForfeited(chr.getQuest(this).getForfeited());
        newStatus.setCompleted(chr.getQuest(this).getCompleted());

        if (timeLimit > 0)
        {
            newStatus.setExpirationTime(chr.Client.CurrentServerContainer.GetCurrentTimeDateTimeOffSet().AddSeconds(timeLimit).ToUnixTimeMilliseconds());
            chr.questTimeLimit(this, timeLimit);
        }
        if (timeLimit2 > 0)
        {
            newStatus.setExpirationTime(chr.Client.CurrentServerContainer.GetCurrentTimeDateTimeOffSet().AddSeconds(timeLimit2).ToUnixTimeMilliseconds());
            chr.questTimeLimit2(this, newStatus.getExpirationTime());
        }

        chr.updateQuestStatus(newStatus);

        return true;
    }

    public bool forceComplete(IPlayer chr, int npc)
    {
        if (timeLimit > 0)
        {
            chr.sendPacket(QuestPacket.RemoveQuestTimeLimit(id));
        }

        QuestStatus newStatus = new QuestStatus(this, QuestStatus.Status.COMPLETED, npc);
        newStatus.setForfeited(chr.getQuest(this).getForfeited());
        newStatus.setCompleted(chr.getQuest(this).getCompleted());
        newStatus.setCompletionTime(chr.Client.CurrentServerContainer.getCurrentTime());
        chr.updateQuestStatus(newStatus);

        chr.sendPacket(PacketCreator.showSpecialEffect(9)); // Quest completion
        chr.getMap().broadcastMessage(chr, PacketCreator.showForeignEffect(chr.getId(), 9), false); //use 9 instead of 12 for both
        return true;
    }

    public short getId()
    {
        return id;
    }

    public List<int> getRelevantMobs()
    {
        return relevantMobs;
    }

    public int getStartItemAmountNeeded(int itemid)
    {
        var req = startReqs.GetValueOrDefault(QuestRequirementType.ITEM);
        if (req is ItemRequirement ireq)
            return ireq.getItemAmountNeeded(itemid, false);
        return int.MinValue;

    }

    public int getCompleteItemAmountNeeded(int itemid)
    {
        var req = completeReqs.GetValueOrDefault(QuestRequirementType.ITEM);
        if (req is ItemRequirement ireq)
            return ireq.getItemAmountNeeded(itemid, true);
        return int.MaxValue;
    }

    public int getMobAmountNeeded(int mid)
    {
        var req = completeReqs.GetValueOrDefault(QuestRequirementType.MOB);
        if (req is MobRequirement mreq)
            return mreq.getRequiredMobCount(mid);
        return 0;
    }

    public short getInfoNumber(Status qs)
    {
        bool checkEnd = qs.Equals(Status.STARTED);
        Dictionary<QuestRequirementType, AbstractQuestRequirement> reqs = !checkEnd ? startReqs : completeReqs;

        var req = reqs.GetValueOrDefault(QuestRequirementType.INFO_NUMBER);
        if (req is InfoNumberRequirement inReq)
            return inReq.getInfoNumber();
        return 0;
    }

    public string getInfoEx(Status qs, int index)
    {
        return getInfoEx(qs).ElementAtOrDefault(index) ?? "";
    }

    public List<string> getInfoEx(Status qs)
    {
        bool checkEnd = qs.Equals(Status.STARTED);
        Dictionary<QuestRequirementType, AbstractQuestRequirement> reqs = !checkEnd ? startReqs : completeReqs;
        var req = reqs.GetValueOrDefault(QuestRequirementType.INFO_EX);
        if (req is InfoExRequirement ixReq)
            return ixReq.getInfo();
        return new();
    }

    public int getTimeLimit()
    {
        return timeLimit;
    }


    public ItemRequirement? GetItemRequirement()
    {
        return completeReqs.GetValueOrDefault(QuestRequirementType.ITEM) as ItemRequirement;
    }

    public MobRequirement? GetMobRequirement()
    {
        return completeReqs.GetValueOrDefault(QuestRequirementType.MOB) as MobRequirement;
    }


    private static Dictionary<QuestRequirementType, AbstractQuestRequirement> GetRequirement(Quest q, QuestDemand data)
    {
        var dict = new Dictionary<QuestRequirementType, AbstractQuestRequirement>();
        if (!string.IsNullOrEmpty(data.End))
            dict[QuestRequirementType.END_DATE] = new EndDateRequirement(q, data.End);
        if (data.Job.Length > 0)
            dict[QuestRequirementType.JOB] = new JobRequirement(q, data.Job);
        if (data.DemandQuest.Length > 0)
            dict[QuestRequirementType.QUEST] = new QuestRequirement(q, data.DemandQuest);
        if (data.FieldEnter.Length > 0)
            dict[QuestRequirementType.FIELD_ENTER] = new FieldEnterRequirement(q, data.FieldEnter);
        if (data.InfoNumber != null)
            dict[QuestRequirementType.INFO_NUMBER] = new InfoNumberRequirement(q, data.InfoNumber.Value);
        if (data.InfoEx.Length > 0)
            dict[QuestRequirementType.INFO_EX] = new InfoExRequirement(q, data.InfoEx);
        if (data.Interval != null)
            dict[QuestRequirementType.INTERVAL] = new IntervalRequirement(q, data.Interval.Value);
        if (data.QuestComplete != null)
            dict[QuestRequirementType.COMPLETED_QUEST] = new CompletedQuestRequirement(q, data.QuestComplete.Value);
        if (data.DemandItem.Length > 0)
            dict[QuestRequirementType.ITEM] = new ItemRequirement(q, data.DemandItem);
        if (data.LevelMax != null)
            dict[QuestRequirementType.MAX_LEVEL] = new MaxLevelRequirement(q, data.LevelMax.Value);
        if (data.LevelMin != null)
            dict[QuestRequirementType.MIN_LEVEL] = new MinLevelRequirement(q, data.LevelMin.Value);
        if (data.Meso != null)
            dict[QuestRequirementType.MESO] = new MesoRequirement(q, data.Meso.Value);
        if (data.MinMonsterBook != null)
            dict[QuestRequirementType.MESO] = new MonsterBookCountRequirement(q, data.MinMonsterBook.Value);
        if (data.PetTamenessMin != null)
            dict[QuestRequirementType.MIN_PET_TAMENESS] = new MinTamenessRequirement(q, data.PetTamenessMin.Value);
        if (data.DemandMob.Length > 0)
            dict[QuestRequirementType.MOB] = new MobRequirement(q, data.DemandMob);
        if (data.Meso != null)
            dict[QuestRequirementType.MONSTER_BOOK] = new MesoRequirement(q, data.Meso.Value);
        if (data.Npc != null)
            dict[QuestRequirementType.NPC] = new NpcRequirement(q, data.Npc.Value);
        if (data.Pet.Length > 0)
            dict[QuestRequirementType.PET] = new PetRequirement(q, data.Pet);
        if (data.Buff != null)
            dict[QuestRequirementType.BUFF] = new BuffRequirement(q, data.Buff.Value);
        if (data.ExceptBuff != null)
            dict[QuestRequirementType.EXCEPT_BUFF] = new BuffExceptRequirement(q, data.ExceptBuff.Value);
        if (data.StartScript != null)
            dict[QuestRequirementType.SCRIPT] = new ScriptRequirement(q, data.StartScript);
        if (data.EndScript != null)
            dict[QuestRequirementType.SCRIPT] = new ScriptRequirement(q, data.EndScript);

        return dict;
    }

    private static Dictionary<QuestActionType, AbstractQuestAction> GetAction(Quest q, QuestAct data)
    {
        Dictionary<QuestActionType, AbstractQuestAction> dict = new();
        if (data.BuffItemID != null)
            dict[QuestActionType.BUFF] = new BuffAction(q, data.BuffItemID.Value);
        if (data.Exp != null)
            dict[QuestActionType.EXP] = new ExpAction(q, data.Exp.Value);
        if (data.Fame != null)
            dict[QuestActionType.FAME] = new FameAction(q, data.Fame.Value);
        if (data.Items.Length > 0)
            dict[QuestActionType.ITEM] = new ItemAction(q, data.Items);
        if (data.Money != null)
            dict[QuestActionType.MESO] = new MesoAction(q, data.Money.Value);
        if (data.NextQuest != null)
            dict[QuestActionType.NEXTQUEST] = new NextQuestAction(q, data.NextQuest.Value);
        if (data.PetSkill != null)
            dict[QuestActionType.PETSKILL] = new PetSkillAction(q, data.PetSkill.Value);
        if (data.Quests.Length > 0)
            dict[QuestActionType.QUEST] = new QuestAction(q, data.Quests);
        if (data.Skills.Length > 0)
            dict[QuestActionType.SKILL] = new SkillAction(q, data.Skills);
        if (data.PetTameness != null)
            dict[QuestActionType.PETTAMENESS] = new PetTamenessAction(q, data.PetTameness.Value);
        if (data.PetSpeed != null)
            dict[QuestActionType.PETSPEED] = new PetSpeedAction(q, data.PetSpeed.Value);
        if (data.Info != null)
            dict[QuestActionType.INFO] = new InfoAction(q, data.Info);

        return dict;
    }

    public bool restoreLostItem(IPlayer chr, int itemid)
    {
        if (chr.getQuest(this).getStatus().Equals(QuestStatus.Status.STARTED))
        {
            var itemAct = startActs.GetValueOrDefault(QuestActionType.ITEM) as ItemAction;
            if (itemAct != null)
            {
                return itemAct.restoreLostItem(chr, itemid);
            }
        }

        return false;
    }


    public int getNpcRequirement(bool checkEnd)
    {
        Dictionary<QuestRequirementType, AbstractQuestRequirement> reqs = !checkEnd ? startReqs : completeReqs;
        var mqr = reqs.GetValueOrDefault(QuestRequirementType.NPC);
        if (mqr != null)
        {
            return ((NpcRequirement)mqr).get();
        }
        else
        {
            return -1;
        }
    }

    public bool hasScriptRequirement(bool checkEnd)
    {
        Dictionary<QuestRequirementType, AbstractQuestRequirement> reqs = !checkEnd ? startReqs : completeReqs;
        var mqr = reqs.GetValueOrDefault(QuestRequirementType.SCRIPT);

        if (mqr != null)
        {
            return ((ScriptRequirement)mqr).get();
        }
        else
        {
            return false;
        }
    }

    public bool hasNextQuestAction()
    {
        Dictionary<QuestActionType, AbstractQuestAction> acts = completeActs;
        var mqa = acts.GetValueOrDefault(QuestActionType.NEXTQUEST);

        return mqa != null;
    }

}
