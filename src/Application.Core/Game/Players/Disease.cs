using client;
using constants.skills;
using net.server;
using server;
using server.life;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private ScheduledFuture? _diseaseExpireTask = null;
        private Dictionary<Disease, KeyValuePair<DiseaseValueHolder, MobSkill>> diseases = new();
        private Dictionary<Disease, long> diseaseExpires = new();

        public bool hasDisease(Disease dis)
        {
            chLock.EnterReadLock();
            try
            {
                return diseases.ContainsKey(dis);
            }
            finally
            {
                chLock.ExitReadLock();
            }
        }

        public int getDiseasesSize()
        {
            chLock.EnterReadLock();
            try
            {
                return diseases.Count;
            }
            finally
            {
                chLock.ExitReadLock();
            }
        }

        public Dictionary<Disease, DiseaseExpiration> getAllDiseases()
        {
            chLock.EnterReadLock();
            try
            {
                long curtime = Server.getInstance().getCurrentTime();
                Dictionary<Disease, DiseaseExpiration> ret = new();

                foreach (var de in diseaseExpires)
                {
                    KeyValuePair<DiseaseValueHolder, MobSkill> dee = diseases.GetValueOrDefault(de.Key);
                    DiseaseValueHolder mdvh = dee.Key;

                    ret.AddOrUpdate(de.Key, new(mdvh.length - (curtime - mdvh.startTime), dee.Value));
                }

                return ret;
            }
            finally
            {
                chLock.ExitReadLock();
            }
        }

        public void silentApplyDiseases(Dictionary<Disease, DiseaseExpiration> diseaseMap)
        {
            chLock.EnterReadLock();
            try
            {
                long curTime = Server.getInstance().getCurrentTime();

                foreach (var di in diseaseMap)
                {
                    long expTime = curTime + di.Value.LeftTime;

                    diseaseExpires.AddOrUpdate(di.Key, expTime);
                    diseases.AddOrUpdate(di.Key, new(new DiseaseValueHolder(curTime, di.Value.LeftTime), di.Value.MobSkill));
                }
            }
            finally
            {
                chLock.ExitReadLock();
            }
        }

        public void announceDiseases()
        {
            HashSet<KeyValuePair<Disease, KeyValuePair<DiseaseValueHolder, MobSkill>>> chrDiseases;

            chLock.EnterReadLock();
            try
            {
                // Poison damage visibility and diseases status visibility, extended through map transitions thanks to Ronan
                if (!this.isLoggedinWorld())
                {
                    return;
                }

                chrDiseases = new(diseases);
            }
            finally
            {
                chLock.ExitReadLock();
            }

            foreach (var di in chrDiseases)
            {
                Disease disease = di.Key;
                MobSkill skill = di.Value.Value;
                List<KeyValuePair<Disease, int>> debuff = Collections.singletonList(new KeyValuePair<Disease, int>(disease, skill.getX()));

                if (disease != Disease.SLOW)
                {
                    MapModel.broadcastMessage(PacketCreator.giveForeignDebuff(Id, debuff, skill));
                }
                else
                {
                    MapModel.broadcastMessage(PacketCreator.giveForeignSlowDebuff(Id, debuff, skill));
                }
            }
        }

        public void collectDiseases()
        {
            foreach (IPlayer chr in MapModel.getAllPlayers())
            {
                int cid = chr.getId();

                foreach (var di in chr.getAllDiseases())
                {
                    Disease disease = di.Key;
                    MobSkill skill = di.Value.MobSkill;
                    List<KeyValuePair<Disease, int>> debuff = Collections.singletonList(new KeyValuePair<Disease, int>(disease, skill.getX()));

                    if (disease != Disease.SLOW)
                    {
                        this.sendPacket(PacketCreator.giveForeignDebuff(cid, debuff, skill));
                    }
                    else
                    {
                        this.sendPacket(PacketCreator.giveForeignSlowDebuff(cid, debuff, skill));
                    }
                }
            }
        }

        public void giveDebuff(Disease disease, MobSkill skill)
        {
            if (!hasDisease(disease) && getDiseasesSize() < 2)
            {
                if (!(disease == Disease.SEDUCE || disease == Disease.STUN))
                {
                    if (hasActiveBuff(Bishop.HOLY_SHIELD))
                    {
                        return;
                    }
                }

                chLock.EnterReadLock();
                try
                {
                    long curTime = Server.getInstance().getCurrentTime();
                    diseaseExpires.AddOrUpdate(disease, curTime + skill.getDuration());
                    diseases.AddOrUpdate(disease, new(new DiseaseValueHolder(curTime, skill.getDuration()), skill));
                }
                finally
                {
                    chLock.ExitReadLock();
                }

                if (disease == Disease.SEDUCE && chair.get() < 0)
                {
                    sitChair(-1);
                }

                List<KeyValuePair<Disease, int>> debuff = Collections.singletonList(new KeyValuePair<Disease, int>(disease, skill.getX()));
                sendPacket(PacketCreator.giveDebuff(debuff, skill));

                if (disease != Disease.SLOW)
                {
                    MapModel.broadcastMessage(this, PacketCreator.giveForeignDebuff(Id, debuff, skill), false);
                }
                else
                {
                    MapModel.broadcastMessage(this, PacketCreator.giveForeignSlowDebuff(Id, debuff, skill), false);
                }
            }
        }

        public void dispelDebuff(Disease debuff)
        {
            if (hasDisease(debuff))
            {
                long mask = (long)debuff.getValue();
                sendPacket(PacketCreator.cancelDebuff(mask));

                if (debuff != Disease.SLOW)
                {
                    MapModel.broadcastMessage(this, PacketCreator.cancelForeignDebuff(Id, mask), false);
                }
                else
                {
                    MapModel.broadcastMessage(this, PacketCreator.cancelForeignSlowDebuff(Id), false);
                }

                chLock.EnterReadLock();
                try
                {
                    diseases.Remove(debuff);
                    diseaseExpires.Remove(debuff);
                }
                finally
                {
                    chLock.ExitReadLock();
                }
            }
        }

        public void dispelDebuffs()
        {
            dispelDebuff(Disease.CURSE);
            dispelDebuff(Disease.DARKNESS);
            dispelDebuff(Disease.POISON);
            dispelDebuff(Disease.SEAL);
            dispelDebuff(Disease.WEAKEN);
            dispelDebuff(Disease.SLOW);    // thanks Conrad for noticing ZOMBIFY isn't dispellable
        }

        public void purgeDebuffs()
        {
            dispelDebuff(Disease.SEDUCE);
            dispelDebuff(Disease.ZOMBIFY);
            dispelDebuff(Disease.CONFUSE);
            dispelDebuffs();
        }

        public void cancelAllDebuffs()
        {
            chLock.EnterReadLock();
            try
            {
                diseases.Clear();
                diseaseExpires.Clear();
            }
            finally
            {
                chLock.ExitReadLock();
            }
        }

        public void cancelDiseaseExpireTask()
        {
            if (_diseaseExpireTask != null)
            {
                _diseaseExpireTask.cancel(false);
                _diseaseExpireTask = null;
            }
        }

        public void diseaseExpireTask()
        {
            if (_diseaseExpireTask == null)
            {
                _diseaseExpireTask = TimerManager.getInstance().register(() =>
                {
                    HashSet<Disease> toExpire = new();

                    chLock.EnterReadLock();
                    try
                    {
                        long curTime = Server.getInstance().getCurrentTime();

                        foreach (var de in diseaseExpires)
                        {
                            if (de.Value < curTime)
                            {
                                toExpire.Add(de.Key);
                            }
                        }
                    }
                    finally
                    {
                        chLock.ExitReadLock();
                    }

                    foreach (Disease d in toExpire)
                    {
                        dispelDebuff(d);
                    }

                }, 1500);
            }
        }
    }
}
