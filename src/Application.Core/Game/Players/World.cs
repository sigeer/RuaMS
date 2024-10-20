using client;
using Microsoft.EntityFrameworkCore;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        /// <summary>
        /// player client is currently trying to change maps or log in the game map
        /// </summary>
        private AtomicBoolean mapTransitioning = new AtomicBoolean(true);
        /// <summary>
        /// player is online, but on cash shop or mts
        /// </summary>
        private AtomicBoolean awayFromWorld = new AtomicBoolean(true);

        public void setDisconnectedFromChannelWorld()
        {
            setAwayFromChannelWorld(true);
        }
        public void setAwayFromChannelWorld()
        {
            setAwayFromChannelWorld(false);
        }
        private void setAwayFromChannelWorld(bool disconnect)
        {
            awayFromWorld.Set(true);

            if (!disconnect)
            {
                Client.getChannelServer().insertPlayerAway(Id);
            }
            else
            {
                Client.getChannelServer().removePlayerAway(Id);
            }
        }
        public bool isLoggedinWorld()
        {
            return this.isLoggedin() && !this.isAwayFromWorld();
        }

        public bool isAwayFromWorld()
        {
            return awayFromWorld.Get();
        }

        public void setEnteredChannelWorld(int channel)
        {
            awayFromWorld.Set(false);
            Client.getChannelServer().removePlayerAway(Id);

            if (PartySearch)
            {
                this.getWorldServer().getPartySearchCoordinator().attachPlayer(this);
            }
        }



        public int checkWorldTransferEligibility()
        {
            if (getLevel() < 20)
            {
                return 2;
            }
            else if (getClient().getTempBanCalendar() != null && getClient().getTempBanCalendar()!.Value.AddDays(30) < DateTimeOffset.Now)
            {
                return 3;
            }
            else if (isMarried())
            {
                return 4;
            }
            else if (getGuildRank() < 2)
            {
                return 5;
            }
            else if (getFamily() != null)
            {
                return 8;
            }
            else
            {
                return 0;
            }
        }

        public bool registerWorldTransfer(int newWorld)
        {
            try
            {
                using var dbContext = new DBContext();
                try
                {
                    //check for pending world transfer
                    var charTransfters = dbContext.Worldtransfers.Where(x => x.Characterid == getId()).Select(x => x.CompletionTime).ToList();
                    if (charTransfters.Any(x => x == null))
                        return false;
                    if (charTransfters.Any(x => x!.Value.AddMilliseconds(YamlConfig.config.server.WORLD_TRANSFER_COOLDOWN) > DateTimeOffset.Now))
                        return false;
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to register world transfer for chr {CharacterName}", getName());
                    return false;
                }

                try
                {
                    var dbModel = new Worldtransfer(getId(), (sbyte)getWorld(), (sbyte)newWorld);
                    dbContext.Worldtransfers.Add(dbModel);
                    dbContext.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to register world transfer for chr {CharacterName}", getName());
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to get DB connection while registering world transfer");
            }
            return false;
        }

        public bool cancelPendingWorldTranfer()
        {
            try
            {
                using var dbContext = new DBContext();
                return dbContext.Worldtransfers.Where(x => x.Characterid == getId() && x.CompletionTime == null).ExecuteDelete() > 0;
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to cancel pending world transfer for chr {CharacterName}", getName());
                return false;
            }
        }
    }
}
