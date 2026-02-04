using Application.Core.Channel.Commands;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Packets;
using client;
using server.quest;
using ZLinq;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private List<KeyValuePair<DelayedQuestUpdate, object[]>> npcUpdateQuests = new();

        private ScheduledFuture? questExpireTask = null;
        public Dictionary<short, QuestStatus> Quests { get; set; } = new Dictionary<short, QuestStatus>();
        public bool HasExpireableQuest => Quests.Values.AsValueEnumerable().Any(x => x.getExpirationTime() > 0);

        public List<QuestStatus> getQuests()
        {
            return new(Quests.Values);
        }
        public List<QuestStatus> getStartedQuests()
        {
            return getQuests().Where(x => x.getStatus() == QuestStatus.Status.STARTED).ToList();
        }
        public List<QuestStatus> getCompletedQuests()
        {
            return getQuests().Where(x => x.getStatus() == QuestStatus.Status.COMPLETED).ToList();
        }
        public sbyte getQuestStatus(int quest)
        {
            QuestStatus? mqs = Quests.GetValueOrDefault((short)quest);
            if (mqs != null)
            {
                return (sbyte)mqs.getStatus();
            }
            else
            {
                return 0;
            }
        }

        public QuestStatus GetOrAddQuest(int quest)
        {
            return getQuest(Quest.getInstance(quest));
        }

        public QuestStatus getQuest(Quest quest)
        {
            short questid = quest.getId();
            return Quests.GetOrAdd(questid, new QuestStatus(quest, QuestStatus.Status.NOT_STARTED));
        }

        public void setQuestProgress(int id, int infoNumber, string progress)
        {
            QuestStatus qs = GetOrAddQuest(id);

            if (infoNumber > 0 && qs.getInfoNumber() == infoNumber)
            {
                QuestStatus iqs = GetOrAddQuest(infoNumber);
                iqs.setProgress(0, progress);
            }
            else
            {
                qs.setProgress(infoNumber, progress);   // quest progress is thoroughly a string match, infoNumber is actually another questid
            }

            announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, false);
            if (qs.getInfoNumber() > 0)
            {
                announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, true);
            }
        }

        public void awardQuestPoint(int awardedPoints)
        {
            if (YamlConfig.config.server.QUEST_POINT_REQUIREMENT < 1 || awardedPoints < 1)
            {
                return;
            }

            int delta;
            Fquest += awardedPoints;

            delta = Fquest / YamlConfig.config.server.QUEST_POINT_REQUIREMENT;
            Fquest %= YamlConfig.config.server.QUEST_POINT_REQUIREMENT;
            if (delta > 0)
            {
                gainFame(delta);
            }
        }

        //---- \/ \/ \/ \/ \/ \/ \/  NOT TESTED  \/ \/ \/ \/ \/ \/ \/ \/ \/ ----

        public void setQuestAdd(Quest quest, byte status, string customData)
        {
            if (!Quests.ContainsKey(quest.getId()))
            {
                QuestStatus stat = new QuestStatus(quest, (QuestStatus.Status)(status));
                stat.setCustomData(customData);
                Quests.AddOrUpdate(quest.getId(), stat);
            }
        }

        public QuestStatus? getQuestNAdd(Quest quest)
        {
            if (!Quests.ContainsKey(quest.getId()))
            {
                QuestStatus status = new QuestStatus(quest, QuestStatus.Status.NOT_STARTED);
                Quests.AddOrUpdate(quest.getId(), status);
                return status;
            }
            return Quests.GetValueOrDefault(quest.getId());
        }

        public QuestStatus? getQuestNoAdd(Quest quest)
        {
            return Quests.GetValueOrDefault(quest.getId());
        }

        public QuestStatus? getQuestRemove(Quest quest)
        {
            if (Quests.Remove(quest.getId(), out var d))
                return d;
            return null;
        }

        //---- /\ /\ /\ /\ /\ /\ /\  NOT TESTED  /\ /\ /\ /\ /\ /\ /\ /\ /\ ----

        public void raiseQuestMobCount(int id)
        {
            // It seems nexon uses monsters that don't exist in the WZ (except string) to merge multiple mobs together for these 3 monsters.
            // We also want to run mobKilled for both since there are some quest that don't use the updated ID...
            if (id == MobId.GREEN_MUSHROOM || id == MobId.DEJECTED_GREEN_MUSHROOM)
            {
                raiseQuestMobCount(MobId.GREEN_MUSHROOM_QUEST);
            }
            else if (id == MobId.ZOMBIE_MUSHROOM || id == MobId.ANNOYED_ZOMBIE_MUSHROOM)
            {
                raiseQuestMobCount(MobId.ZOMBIE_MUSHROOM_QUEST);
            }
            else if (id == MobId.GHOST_STUMP || id == MobId.SMIRKING_GHOST_STUMP)
            {
                raiseQuestMobCount(MobId.GHOST_STUMP_QUEST);
            }

            int lastQuestProcessed = 0;
            try
            {
                foreach (QuestStatus qs in getQuests())
                {
                    lastQuestProcessed = qs.getQuest().getId();
                    if (qs.getStatus() == QuestStatus.Status.COMPLETED || qs.getQuest().canComplete(this, null))
                    {
                        continue;
                    }

                    if (qs.progress(id))
                    {
                        announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, false);
                        if (qs.getInfoNumber() > 0)
                        {
                            announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning(e, "Player.mobKilled. chrId {CharacterId}, last quest processed: {LastQuestProcessed}", Id, lastQuestProcessed);
            }
        }

        private void announceUpdateQuestInternal(Player chr, KeyValuePair<DelayedQuestUpdate, object[]> questUpdate)
        {
            object[] objs = questUpdate.Value;

            switch (questUpdate.Key)
            {
                case DelayedQuestUpdate.UPDATE:
                    sendPacket(QuestPacket.UpdateQuest(chr, (QuestStatus)objs[0], (bool)objs[1]));
                    break;

                case DelayedQuestUpdate.FORFEIT:
                    sendPacket(QuestPacket.ForfeitQuest((short)objs[0]));
                    break;

                case DelayedQuestUpdate.COMPLETE:
                    sendPacket(QuestPacket.CompleteQuest((short)objs[0], (long)objs[1]));
                    break;

                case DelayedQuestUpdate.INFO:
                    QuestStatus qs = (QuestStatus)objs[0];
                    sendPacket(QuestPacket.UpdateQuestInfo(qs.getQuest().getId(), qs.getNpc()));
                    break;
            }
        }

        public void announceUpdateQuest(DelayedQuestUpdate questUpdateType, params object[] paramsValue)
        {
            KeyValuePair<DelayedQuestUpdate, object[]> p = new(questUpdateType, paramsValue);
            var c = this.getClient();
            if (c.NPCConversationManager != null)
            {
                npcUpdateQuests.Add(p);
            }
            else
            {
                announceUpdateQuestInternal(this, p);
            }
        }

        public void flushDelayedUpdateQuests()
        {
            List<KeyValuePair<DelayedQuestUpdate, object[]>> qmQuestUpdateList;

            qmQuestUpdateList = new(npcUpdateQuests);
            npcUpdateQuests.Clear();

            foreach (var q in qmQuestUpdateList)
            {
                announceUpdateQuestInternal(this, q);
            }
        }

        public void updateQuestStatus(QuestStatus qs)
        {
            Quests.AddOrUpdate(qs.getQuestID(), qs);
            if (qs.getStatus().Equals(QuestStatus.Status.STARTED))
            {
                announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, false);
                if (qs.getInfoNumber() > 0)
                {
                    announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, true);
                }
                announceUpdateQuest(DelayedQuestUpdate.INFO, qs);
            }
            else if (qs.getStatus().Equals(QuestStatus.Status.COMPLETED))
            {
                Quest mquest = qs.getQuest();
                short questid = mquest.getId();
                if (!mquest.isSameDayRepeatable() && !QuestFactory.Instance.isExploitableQuest(questid))
                {
                    awardQuestPoint(YamlConfig.config.server.QUEST_POINT_PER_QUEST_COMPLETE);
                }
                qs.setCompleted(qs.getCompleted() + 1);   // Jayd's idea - count quest completed

                announceUpdateQuest(DelayedQuestUpdate.COMPLETE, questid, qs.getCompletionTime());
                //announceUpdateQuest(DelayedQuestUpdate.INFO, qs); // happens after giving rewards, for non-next quests only
            }
            else if (qs.getStatus().Equals(QuestStatus.Status.NOT_STARTED))
            {
                announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, false);
                if (qs.getInfoNumber() > 0)
                {
                    announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, true);
                }
                // reminder: do not reset quest progress of infoNumbers, some quests cannot backtrack
            }
        }

        public void ClearExpiredQuests()
        {
            long timeNow = Client.CurrentServer.Node.getCurrentTime();
            var quests = Quests.AsValueEnumerable()
                .Where(x => x.Value.getExpirationTime() > 0 && x.Value.getExpirationTime() <= timeNow)
                .Select(x => x.Value.getQuest())
                .ToList();
            foreach (var item in quests)
            {
                expireQuest(item);
            }

            if (!HasExpireableQuest)
            {
                cancelQuestExpirationTask();
            }
        }

        public void expireQuest(Quest quest)
        {
            if (quest.forfeit(this))
            {
                sendPacket(QuestPacket.QuestExpire(quest.getId()));
            }
        }

        public void cancelQuestExpirationTask()
        {

            if (questExpireTask != null && !HasExpireableQuest)
            {
                questExpireTask.cancel(false);
                questExpireTask = null;
            }
        }

        public void forfeitExpirableQuests()
        {
            var expirableQuests = Quests.Values.Where(x => x.getExpirationTime() > 0).ToArray();
            foreach (var quest in expirableQuests)
            {
                quest.getQuest().forfeit(this);
            }
        }

        public void questExpirationTask()
        {
            if (HasExpireableQuest)
            {
                if (questExpireTask == null)
                {
                    questExpireTask = Client.CurrentServer.Node.TimerManager.register(
                        new NamedRunnable($"Player:{Id},{GetHashCode()}_QuestExpireTask", () =>
                        {
                            Client.CurrentServer.Post(new PlayerQuestExpiredCommand(this));
                        }), TimeSpan.FromSeconds(10));
                }
            }

        }


        private void registerQuestExpire(Quest quest)
        {
            if (questExpireTask == null)
            {
                questExpireTask = Client.CurrentServer.Node.TimerManager.register(
                    new NamedRunnable($"Player:{Id},{GetHashCode()}_QuestExpireTask", () =>
                    {
                        Client.CurrentServer.Post(new PlayerQuestExpiredCommand(this));
                    }), TimeSpan.FromSeconds(10));
            }
        }

        public void questTimeLimit(Quest quest, int seconds)
        {
            registerQuestExpire(quest);
            sendPacket(QuestPacket.AddQuestTimeLimit(quest.getId(), seconds * 1000));
        }

        public void questTimeLimit2(Quest quest, long expires)
        {
            long timeLeft = expires - Client.CurrentServer.Node.getCurrentTime();

            if (timeLeft <= 0)
            {
                expireQuest(quest);
            }
            else
            {
                registerQuestExpire(quest);
            }
        }

        public void reloadQuestExpirations()
        {
            foreach (QuestStatus mqs in getStartedQuests())
            {
                if (mqs.getExpirationTime() > 0)
                {
                    questTimeLimit2(mqs.getQuest(), mqs.getExpirationTime());
                }
            }
        }

    }
}
