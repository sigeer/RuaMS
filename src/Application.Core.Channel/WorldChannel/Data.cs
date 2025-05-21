using Application.Core.Game.Players;
using Application.Shared.Login;
using Application.Utility.Compatible;
using client;
using net.server;
using tools;

namespace Application.Core.Channel
{
    public partial class WorldChannel
    {
        public void SendLogoff(int id)
        {
            UpdateAccountState(id, LoginStage.LOGIN_NOTLOGGEDIN);
        }
        public void StashCharacterBuff(IPlayer player)
        {
            Server.getInstance().getPlayerBuffStorage().addBuffsToStorage(player.getId(), player.getAllBuffs());
        }

        public void StashCharacterDisease(IPlayer player)
        {
            Server.getInstance().getPlayerBuffStorage().addDiseasesToStorage(player.getId(), player.getAllDiseases());
        }

        private List<KeyValuePair<long, PlayerBuffValueHolder>> getLocalStartTimes(List<PlayerBuffValueHolder> lpbvl)
        {
            long curtime = getCurrentTime();
            return lpbvl.Select(x => new KeyValuePair<long, PlayerBuffValueHolder>(curtime - x.usedTime, x)).OrderBy(x => x.Key).ToList();
        }

        public void RecoverCharacterBuff(IPlayer player)
        {
            var buffs = Server.getInstance().getPlayerBuffStorage().getBuffsFromStorage(player.Id);
            if (buffs != null)
            {
                var timedBuffs = getLocalStartTimes(buffs);
                player.silentGiveBuffs(timedBuffs);
            }
        }

        public void RecoverCharacterDisease(IPlayer player)
        {
            var diseases = Server.getInstance().getPlayerBuffStorage().getDiseasesFromStorage(player.Id);
            if (diseases != null)
            {
                player.silentApplyDiseases(diseases);

                foreach (var e in diseases)
                {
                    var debuff = Collections.singletonList(new KeyValuePair<Disease, int>(e.Key, e.Value.MobSkill.getX()));
                    player.sendPacket(PacketCreator.giveDebuff(debuff, e.Value.MobSkill));
                }
            }
        }
    }
}
