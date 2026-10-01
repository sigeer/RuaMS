using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Players.Tickables;
using Application.Core.Server.life;
using Application.Utility.Tickables;
using client.inventory;
using tools;
using ZLinq;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        /// <summary>身上的疾病，key 是 <see cref="BuffStat"/> 里对应的状态位（元数据见 <see cref="DiseaseInfo"/>）。</summary>
        public Dictionary<BuffStat, EffectDebuff> Diseases { get; } = new();

        public bool hasDisease(BuffStat dis)
        {
            return Diseases.ContainsKey(dis);
        }

        public int getDiseasesSize() => Diseases.Count;

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

                var dis = new EffectDebuff(this, disease, skill, curTime, curTime + +skill.getDuration(), skill.getX());
                Diseases[disease] = dis;

                if (disease == BuffStat.SEDUCE && chair.get() < 0)
                {
                    await sitChair(-1);
                }

                await SendPacket(BuffPackets.GiveBuff(this, dis));

                var p = BuffPackets.GiveRemoteBuff(Id, dis);
                if (p != null)
                {
                    await BroadcastMap(p, Id);
                }
            }
        }

        public async Task DispelDebuffs(IEnumerable<BuffStat> debuffs)
        {
            if (debuffs.Count() == 0)
            {
                return;
            }

            foreach (var debuff in debuffs)
            {
                Diseases.Remove(debuff);
            }
            await SendPacket(BuffPackets.CancelBuff(this, debuffs));
            await BroadcastMap(BuffPackets.CancelRemoteBuff(Id, debuffs), Id);
        }

        public Task dispelDebuffs()
        {
            return DispelDebuffs([BuffStat.CURSE, BuffStat.DARKNESS, BuffStat.POISON, BuffStat.SEAL, BuffStat.WEAKEN, BuffStat.SLOW]);
        }

        public Task purgeDebuffs()
        {
            return DispelDebuffs([BuffStat.SEDUCE, BuffStat.ZOMBIFY, BuffStat.CONFUSE, 
                BuffStat.CURSE, BuffStat.DARKNESS, BuffStat.POISON, BuffStat.SEAL, BuffStat.WEAKEN, BuffStat.SLOW]);
        }

        public void cancelAllDebuffs()
        {
            Diseases.Clear();
        }


        public async Task ClearExpiredDisease()
        {
            var allDiseases = Diseases.Values.ToList();

            HashSet<BuffStat> toRemove = [];
            foreach (var bel in allDiseases)
            {
                if (bel.Status == TickableStatus.Remove)
                {
                    toRemove.Add(bel.BuffStat);
                }
            }

            await DispelDebuffs(toRemove);
        }

        public async Task DebugListAllDisease()
        {
            await Debug(6, string.Join(", ", Diseases.Values
                      .Select(entry => $"type= {entry.BuffStat}, active:{entry.ExpiredAt > getChannelServer().Node.getCurrentTime()}"))
              );
        }
    }
}
