using Application.Core.Game.Packets;
using client;
using server;
using server.quest;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private List<KeyValuePair<DelayedQuestUpdate, object[]>> npcUpdateQuests = new();

        private ScheduledFuture? questExpireTask = null;
        private Dictionary<Quest, long> questExpirations = new();
        private Dictionary<short, QuestStatus> quests;
        public Dictionary<short, QuestStatus> Quests
        {
            get => quests;
            set => quests = value;
        }

        public List<QuestStatus> getQuests()
        {
            lock (quests)
            {
                return new(quests.Values);
            }
        }
        public List<QuestStatus> getStartedQuests()
        {
            return getQuests().Where(x => x.getStatus() == QuestStatus.Status.STARTED).ToList();
        }
        public List<QuestStatus> getCompletedQuests()
        {
            return getQuests().Where(x => x.getStatus() == QuestStatus.Status.COMPLETED).ToList();
        }
        public byte getQuestStatus(int quest)
        {
            lock (quests)
            {
                QuestStatus? mqs = quests.GetValueOrDefault((short)quest);
                if (mqs != null)
                {
                    return (byte)mqs.getStatus();
                }
                else
                {
                    return 0;
                }
            }
        }

        public QuestStatus getQuest(int quest)
        {
            return getQuest(Quest.getInstance(quest));
        }

        public QuestStatus getQuest(Quest quest)
        {
            lock (quests)
            {
                short questid = quest.getId();
                QuestStatus? qs = quests.GetValueOrDefault(questid);
                if (qs == null)
                {
                    qs = new QuestStatus(quest, QuestStatus.Status.NOT_STARTED);
                    quests.AddOrUpdate(questid, qs);
                }
                return qs;
            }
        }

        public void setQuestProgress(int id, int infoNumber, string progress)
        {
            Quest q = Quest.getInstance(id);
            QuestStatus qs = getQuest(q);

            if (infoNumber > 0 && qs.getInfoNumber() == infoNumber)
            {
                Quest iq = Quest.getInstance(infoNumber);
                QuestStatus iqs = getQuest(iq);
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
            lock (quests)
            {
                Fquest += awardedPoints;

                delta = Fquest / YamlConfig.config.server.QUEST_POINT_REQUIREMENT;
                Fquest %= YamlConfig.config.server.QUEST_POINT_REQUIREMENT;
            }

            if (delta > 0)
            {
                gainFame(delta);
            }
        }

        //---- \/ \/ \/ \/ \/ \/ \/  NOT TESTED  \/ \/ \/ \/ \/ \/ \/ \/ \/ ----

        public void setQuestAdd(Quest quest, byte status, string customData)
        {
            lock (quests)
            {
                if (!quests.ContainsKey(quest.getId()))
                {
                    QuestStatus stat = new QuestStatus(quest, (QuestStatus.Status)(status));
                    stat.setCustomData(customData);
                    quests.AddOrUpdate(quest.getId(), stat);
                }
            }
        }

        public QuestStatus? getQuestNAdd(Quest quest)
        {
            lock (quests)
            {
                if (!quests.ContainsKey(quest.getId()))
                {
                    QuestStatus status = new QuestStatus(quest, QuestStatus.Status.NOT_STARTED);
                    quests.AddOrUpdate(quest.getId(), status);
                    return status;
                }
                return quests.GetValueOrDefault(quest.getId());
            }
        }

        public QuestStatus? getQuestNoAdd(Quest quest)
        {
            lock (quests)
            {
                return quests.GetValueOrDefault(quest.getId());
            }
        }

        public QuestStatus? getQuestRemove(Quest quest)
        {
            lock (quests)
            {
                if (quests.Remove(quest.getId(), out var d))
                    return d;
                return null;
            }
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
                lock (quests)
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
            }
            catch (Exception e)
            {
                Log.Warning(e, "IPlayer.mobKilled. chrId {CharacterId}, last quest processed: {LastQuestProcessed}", Id, lastQuestProcessed);
            }
        }

        private void announceUpdateQuestInternal(IPlayer chr, KeyValuePair<DelayedQuestUpdate, object[]> questUpdate)
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
                lock (npcUpdateQuests)
                {
                    npcUpdateQuests.Add(p);
                }
            }
            else
            {
                announceUpdateQuestInternal(this, p);
            }
        }

        public void flushDelayedUpdateQuests()
        {
            List<KeyValuePair<DelayedQuestUpdate, object[]>> qmQuestUpdateList;

            lock (npcUpdateQuests)
            {
                qmQuestUpdateList = new(npcUpdateQuests);
                npcUpdateQuests.Clear();
            }

            foreach (var q in qmQuestUpdateList)
            {
                announceUpdateQuestInternal(this, q);
            }
        }

        public void updateQuestStatus(QuestStatus qs)
        {
            lock (quests)
            {
                quests.AddOrUpdate(qs.getQuestID(), qs);
            }
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
                if (!mquest.isSameDayRepeatable() && !Quest.isExploitableQuest(questid))
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

        private void expireQuest(Quest quest)
        {
            if (quest.forfeit(this))
            {
                sendPacket(QuestPacket.QuestExpire(quest.getId()));
            }
        }

        public void cancelQuestExpirationTask()
        {
            Monitor.Enter(evtLock);
            try
            {
                if (questExpireTask != null)
                {
                    questExpireTask.cancel(false);
                    questExpireTask = null;
                }
            }
            finally
            {
                Monitor.Exit(evtLock);
            }
        }

        public void forfeitExpirableQuests()
        {
            Monitor.Enter(evtLock);
            try
            {
                foreach (Quest quest in questExpirations.Keys)
                {
                    quest.forfeit(this);
                }

                questExpirations.Clear();
            }
            finally
            {
                Monitor.Exit(evtLock);
            }
        }

        public void questExpirationTask()
        {
            Monitor.Enter(evtLock);
            try
            {
                if (questExpirations.Count > 0)
                {
                    if (questExpireTask == null)
                    {
                        questExpireTask = Client.CurrentServerContainer.TimerManager.register(() =>
                        {
                            runQuestExpireTask();

                        }, TimeSpan.FromSeconds(10));
                    }
                }
            }
            finally
            {
                Monitor.Exit(evtLock);
            }
        }

        private void runQuestExpireTask()
        {
            Monitor.Enter(evtLock);
            try
            {
                long timeNow = Client.CurrentServerContainer.getCurrentTime();
                List<Quest> expireList = new();

                foreach (var qe in questExpirations)
                {
                    if (qe.Value <= timeNow)
                    {
                        expireList.Add(qe.Key);
                    }
                }

                if (expireList.Count > 0)
                {
                    foreach (Quest quest in expireList)
                    {
                        expireQuest(quest);
                        questExpirations.Remove(quest);
                    }

                    if (questExpirations.Count == 0)
                    {
                        questExpireTask?.cancel(false);
                        questExpireTask = null;
                    }
                }
            }
            finally
            {
                Monitor.Exit(evtLock);
            }
        }

        private void registerQuestExpire(Quest quest, TimeSpan time)
        {
            Monitor.Enter(evtLock);
            try
            {
                if (questExpireTask == null)
                {
                    questExpireTask = Client.CurrentServerContainer.TimerManager.register(() =>
                    {
                        runQuestExpireTask();

                    }, TimeSpan.FromSeconds(10));
                }

                questExpirations.AddOrUpdate(quest, (long)(Client.CurrentServerContainer.getCurrentTime() + time.TotalMilliseconds));
            }
            finally
            {
                Monitor.Exit(evtLock);
            }
        }

        public void questTimeLimit(Quest quest, int seconds)
        {
            registerQuestExpire(quest, TimeSpan.FromSeconds(seconds));
            sendPacket(QuestPacket.AddQuestTimeLimit(quest.getId(), seconds * 1000));
        }

        public void questTimeLimit2(Quest quest, long expires)
        {
            long timeLeft = expires - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (timeLeft <= 0)
            {
                expireQuest(quest);
            }
            else
            {
                registerQuestExpire(quest, TimeSpan.FromMilliseconds(timeLeft));
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
