using Application.Core.Channel.Commands;
using Application.Core.Game.Players.PlayerProps;
using client;
using server.life;
using tools;
using ZLinq;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private ScheduledFuture? _diseaseExpireTask = null;

        public Dictionary<Disease, PlayerDisease> Diseases { get; } = new();
        public bool hasDisease(Disease dis)
        {
            return Diseases.ContainsKey(dis);
        }

        public int getDiseasesSize() => Diseases.Count;

        public void silentApplyDiseases(IEnumerable<Dto.DiseaseDto> diseaseMap)
        {
            foreach (var item in diseaseMap)
            {
                var disease = Disease.ordinal(item.DiseaseOrdinal);
                Diseases[disease] = new PlayerDisease(disease, item.StartTime, item.Length, MobSkillFactory.getMobSkillOrThrow((MobSkillType)item.MobSkillId, item.MobSkillLevel));
            }
        }

        public void announceDiseases()
        {
            // Poison damage visibility and diseases status visibility, extended through map transitions thanks to Ronan
            if (!this.isLoggedinWorld())
            {
                return;
            }

            var chrDiseases = Diseases.Values.ToList();

            foreach (var di in chrDiseases)
            {
                Disease disease = di.Disease;
                MobSkill skill = di.FromMobSkill;
                List<KeyValuePair<Disease, int>> debuff = Collections.singletonList(new KeyValuePair<Disease, int>(disease, skill.getX()));

                if (di.Disease != Disease.SLOW)
                {
                    MapModel.broadcastMessage(PacketCreator.giveForeignDebuff(Id, debuff, di.FromMobSkill));
                }
                else
                {
                    MapModel.broadcastMessage(PacketCreator.giveForeignSlowDebuff(Id, debuff, skill));
                }
            }
        }

        public void collectDiseases()
        {
            var chrDiseases = Diseases.Values.ToList();

            foreach (Player chr in MapModel.getAllPlayers())
            {
                int cid = chr.getId();

                foreach (var di in chrDiseases)
                {
                    Disease disease = di.Disease;
                    MobSkill skill = di.FromMobSkill;
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


                long curTime = Client.CurrentServer.Node.getCurrentTime();
                Diseases[disease] = new PlayerDisease(disease, curTime, skill.getDuration(), skill);

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

                Diseases.Remove(debuff);
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
            Diseases.Clear();
        }

        public void cancelDiseaseExpireTask()
        {
            if (_diseaseExpireTask != null)
            {
                _diseaseExpireTask.cancel(false);
                _diseaseExpireTask = null;
            }
        }

        public void ClearExpiredDisease()
        {
            long curTime = Client.CurrentServer.Node.getCurrentTime();
            var expired = Diseases.Values.AsValueEnumerable().Where(x => x.StartTime + x.Length <= curTime).Select(x => x.Disease).ToList();

            foreach (var item in expired)
            {
                dispelDebuff(item);
            }
        }

        public void diseaseExpireTask()
        {
            if (_diseaseExpireTask == null)
            {
                _diseaseExpireTask = Client.CurrentServer.Node.TimerManager.register(new NamedRunnable($"Player:{Id},{GetHashCode()}_DiseaseExpireTask", () =>
                {
                    Client.CurrentServer.Post(new PlayerDiseaseExpiredCommand(this));
                }), 1500);
            }
        }
    }
}
