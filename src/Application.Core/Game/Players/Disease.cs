using Application.Core.Game.Players.PlayerProps;
using server.life;
using tools;
using ZLinq;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
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

        public async Task announceDiseases()
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
                    await BroadcastMap(PacketCreator.giveForeignDebuff(Id, debuff, di.FromMobSkill));
                }
                else
                {
                    await BroadcastMap(PacketCreator.giveForeignSlowDebuff(Id, debuff, skill));
                }
            }
        }

        public async Task collectDiseases()
        {
            foreach (Player chr in MapModel.getAllPlayers())
            {
                int cid = chr.getId();

                foreach (var di in chr.Diseases.Values.ToList())
                {
                    Disease disease = di.Disease;
                    MobSkill skill = di.FromMobSkill;
                    List<KeyValuePair<Disease, int>> debuff = Collections.singletonList(new KeyValuePair<Disease, int>(disease, skill.getX()));

                    if (disease != Disease.SLOW)
                    {
                        await this.SendPacket(PacketCreator.giveForeignDebuff(cid, debuff, skill));
                    }
                    else
                    {
                        await this.SendPacket(PacketCreator.giveForeignSlowDebuff(cid, debuff, skill));
                    }
                }
            }
        }

        public async Task giveDebuff(Disease disease, MobSkill skill)
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
                    await sitChair(-1);
                }

                List<KeyValuePair<Disease, int>> debuff = Collections.singletonList(new KeyValuePair<Disease, int>(disease, skill.getX()));
                await SendPacket(PacketCreator.giveDebuff(debuff, skill));

                if (disease != Disease.SLOW)
                {
                    await BroadcastMap(PacketCreator.giveForeignDebuff(Id, debuff, skill), Id);
                }
                else
                {
                    await BroadcastMap(PacketCreator.giveForeignSlowDebuff(Id, debuff, skill), Id);
                }
            }
        }

        public async Task dispelDebuff(Disease debuff)
        {
            if (hasDisease(debuff))
            {
                long mask = (long)debuff.getValue();
                await SendPacket(PacketCreator.cancelDebuff(mask));

                if (debuff != Disease.SLOW)
                {
                    await BroadcastMap(PacketCreator.cancelForeignDebuff(Id, mask), Id);
                }
                else
                {
                    await BroadcastMap(PacketCreator.cancelForeignSlowDebuff(Id), Id);
                }

                Diseases.Remove(debuff);
            }
        }

        public async Task dispelDebuffs()
        {
            await dispelDebuff(Disease.CURSE);
            await dispelDebuff(Disease.DARKNESS);
            await dispelDebuff(Disease.POISON);
            await dispelDebuff(Disease.SEAL);
            await dispelDebuff(Disease.WEAKEN);
            await dispelDebuff(Disease.SLOW);    // thanks Conrad for noticing ZOMBIFY isn't dispellable
        }

        public async Task purgeDebuffs()
        {
            await dispelDebuff(Disease.SEDUCE);
            await dispelDebuff(Disease.ZOMBIFY);
            await dispelDebuff(Disease.CONFUSE);
            await dispelDebuffs();
        }

        public void cancelAllDebuffs()
        {
            Diseases.Clear();
        }


        public async Task ClearExpiredDisease(long now)
        {
            var expired = Diseases.Values.AsValueEnumerable().Where(x => x.StartTime + x.Length <= now).Select(x => x.Disease).ToList();

            foreach (var item in expired)
            {
                await dispelDebuff(item);
            }
        }

        public async Task DebugListAllDisease()
        {
            await Debug(6, string.Join(", ", Diseases.Values
                      .Select(entry => $"type= {entry.Disease.name()}, active:{entry.StartTime + entry.Length >= getChannelServer().Node.getCurrentTime()}"))
              );
        }
    }
}
