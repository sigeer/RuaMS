using Application.Core.Game.Players;
using net.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Channel
{
    public partial class WorldChannel
    {
        public void StashCharacterBuff(IPlayer player)
        {
            Server.getInstance().getPlayerBuffStorage().addBuffsToStorage(player.getId(), player.getAllBuffs());
        }

        public void StashCharacterDisease(IPlayer player)
        {
            Server.getInstance().getPlayerBuffStorage().addDiseasesToStorage(player.getId(), player.getAllDiseases());
        }

        public void LoadCharacterBuff(int character)
        {
            throw new NotImplementedException();
        }

        public void LoadCharacterDisease(int character)
        {
            throw new NotImplementedException();
        }
    }
}
