using Application.Core.Game.Players;
using Application.Core.Game.Skills;
using Application.Shared;
using Application.Shared.Login;
using Application.Utility.Compatible;
using client;
using net.server;
using server;
using server.life;
using tools;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Application.Core.Channel
{
    public partial class WorldChannel
    {
        public AccountLoginStatus UpdateAccountState(int accId, sbyte state)
        {
            return Transport.UpdateAccountState(accId, state);
        }
        public void StashCharacterBuff(IPlayer player)
        {
            Service.SaveBuff(player);
        }

        private List<KeyValuePair<long, PlayerBuffValueHolder>> getLocalStartTimes(List<PlayerBuffValueHolder> lpbvl)
        {
            long curtime = getCurrentTime();
            return lpbvl.Select(x => new KeyValuePair<long, PlayerBuffValueHolder>(curtime - x.usedTime, x)).OrderBy(x => x.Key).ToList();
        }

        public void RecoverCharacterBuff(IPlayer player)
        {
            var buffdto = Service.GetBuffFromStorage(player);
            var buffs = buffdto.Buffs.Select(x => new PlayerBuffValueHolder(x.UsedTime,
                x.IsSkill ? SkillFactory.GetSkillTrust(x.SourceId).getEffect(x.SkillLevel) : ItemInformationProvider.getInstance().getItemEffect(x.SourceId)!)).ToList();

            var timedBuffs = getLocalStartTimes(buffs);
            player.silentGiveBuffs(timedBuffs);

            var diseases = buffdto.Diseases.ToDictionary(
                x => Disease.ordinal(x.DiseaseOrdinal),
                x => new DiseaseExpiration(x.LeftTime, MobSkillFactory.getMobSkillOrThrow((MobSkillType)x.MobSkillId, x.MobSkillLevel)));

            player.silentApplyDiseases(diseases);

            foreach (var e in diseases)
            {
                var debuff = Collections.singletonList(new KeyValuePair<Disease, int>(e.Key, e.Value.MobSkill.getX()));
                player.sendPacket(PacketCreator.giveDebuff(debuff, e.Value.MobSkill));
            }
        }

        public void UpdateCouponConfig(Config.CouponConfig config)
        {
            ActiveCoupons = config.ActiveCoupons.ToList();
            CouponRates = config.CouponRates.ToDictionary();
        }
    }
}
