using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Gameplay;
using Application.Core.Game.Players.PlayerProps;
using Application.Core.Server.life;
using Application.Shared.GameProps;
using System.Numerics;
using tools;
using ZLinq;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        /// <summary>身上的疾病，key 是 <see cref="BuffStat"/> 里对应的状态位（元数据见 <see cref="DiseaseInfo"/>）。</summary>
        public Dictionary<BuffStat, PlayerDisease> Diseases { get; } = new();

        public bool hasDisease(BuffStat dis)
        {
            return Diseases.ContainsKey(dis);
        }

        public int getDiseasesSize() => Diseases.Count;

        public void silentApplyDiseases(IEnumerable<ProtoModel.DiseaseProto> diseaseMap)
        {
            foreach (var item in diseaseMap)
            {
                var disease = BuffStatUtils.FromBit(item.DiseaseBit);
                // 只认疾病位：旧存档里存的是 Disease 的声明序号，换算成位号后大多不是疾病位，这里直接丢弃
                if (disease == null || !DiseaseInfo.IsDisease(disease.Value))
                {
                    continue;
                }
                Diseases[disease.Value] = new PlayerDisease(disease.Value, item.StartTime, item.Length, MobSkillFactory.getMobSkillOrThrow((MobSkillType)item.MobSkillId, item.MobSkillLevel));
            }
        }

        /// <summary>
        /// 向其他玩家发送自己的debuff
        /// </summary>
        /// <returns></returns>
        public async Task announceDiseases(long now)
        {
            // Poison damage visibility and diseases status visibility, extended through map transitions thanks to Ronan
            if (!this.isLoggedinWorld())
            {
                return;
            }
            var chrDiseases = Diseases.Values.ToList();

            foreach (var di in chrDiseases)
            {
                var p = GetFromDisease(di, now);
                if (p != null)
                {
                    await BroadcastMap(BuffPackets.GiveRemoteBuff(Id, p));
                }
            }
        }

        /// <summary>
        /// 自己收取其他玩家的debuff
        /// </summary>
        /// <returns></returns>
        public async Task collectDiseases(long now)
        {
            foreach (Player chr in MapModel.getAllPlayers())
            {
                int cid = chr.getId();

                foreach (var di in chr.Diseases.Values.ToList())
                {
                    var p = GetFromDisease(di, now);
                    if (p != null)
                    {
                        await BroadcastMap(BuffPackets.GiveRemoteBuff(Id, p));
                    }
                }
            }
        }

        public async Task giveDebuff(BuffStat disease, MobSkill skill)
        {
            if (!hasDisease(disease) && getDiseasesSize() < 2)
            {
                if (!(disease == BuffStat.SEDUCE || disease == BuffStat.STUN))
                {
                    if (hasActiveBuff(Bishop.HOLY_SHIELD))
                    {
                        return;
                    }
                }


                long curTime = Client.CurrentServer.Node.getCurrentTime();
                var dis = new PlayerDisease(disease, curTime, skill.getDuration(), skill);
                Diseases[disease] = dis;

                if (disease == BuffStat.SEDUCE && chair.get() < 0)
                {
                    await sitChair(-1);
                }

                var p = GetFromDisease(dis, curTime);
                if (p != null)
                {
                    await SendPacket(BuffPackets.GiveBuff(p));
                    await BroadcastMap(BuffPackets.GiveRemoteBuff(Id, p));
                }
            }
        }

        public async Task dispelDebuff(BuffStat debuff)
        {
            if (hasDisease(debuff))
            {
                await SendPacket(PacketCreator.cancelDebuff(debuff));

                if (debuff != BuffStat.SLOW)
                {
                    await BroadcastMap(PacketCreator.cancelForeignDebuff(Id, debuff), Id);
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
            await dispelDebuff(BuffStat.CURSE);
            await dispelDebuff(BuffStat.DARKNESS);
            await dispelDebuff(BuffStat.POISON);
            await dispelDebuff(BuffStat.SEAL);
            await dispelDebuff(BuffStat.WEAKEN);
            await dispelDebuff(BuffStat.SLOW);    // thanks Conrad for noticing ZOMBIFY isn't dispellable
        }

        public async Task purgeDebuffs()
        {
            await dispelDebuff(BuffStat.SEDUCE);
            await dispelDebuff(BuffStat.ZOMBIFY);
            await dispelDebuff(BuffStat.CONFUSE);
            await dispelDebuffs();
        }

        public void cancelAllDebuffs()
        {
            Diseases.Clear();
        }


        public async Task ClearExpiredDisease(long now)
        {
            var expired = Diseases.Values.AsValueEnumerable().Where(x => x.StartTime + x.Length <= now).Select(x => x.Stat).ToList();

            foreach (var item in expired)
            {
                await dispelDebuff(item);
            }
        }

        public async Task DebugListAllDisease()
        {
            await Debug(6, string.Join(", ", Diseases.Values
                      .Select(entry => $"type= {entry.Stat}, active:{entry.StartTime + entry.Length >= getChannelServer().Node.getCurrentTime()}"))
              );
        }

        public BuffParameter? GetFromDisease(PlayerDisease data, long now)
        {
            var leftTime = data.Length - (now - data.StartTime);
            if (leftTime <= 0)
                return null;

            var builder = new BuffParameterBuilder(data.FromMobSkill);

            builder.Duration = (int)leftTime;
            return builder.Build(this);

        }
    }
}
