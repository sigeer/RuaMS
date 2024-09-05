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
            Channel = null;
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
            Channel = channel;

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

        public static string? checkWorldTransferEligibility(DBContext dbContext, int characterId, int oldWorld, int newWorld)
        {
            if (!YamlConfig.config.server.ALLOW_CASHSHOP_WORLD_TRANSFER)
            {
                return "World transfers disabled.";
            }
            int accountId = -1;
            try
            {
                var charInfoFromDB = dbContext.Characters.Where(x => x.Id == characterId).Select(x => new { x.AccountId, x.Level, x.GuildId, x.GuildRank, x.PartnerId, x.FamilyId }).FirstOrDefault();
                if (charInfoFromDB == null)
                    return "Character does not exist.";
                accountId = charInfoFromDB.AccountId;
                if (charInfoFromDB.Level < 20)
                    return "Character is under level 20.";
                if (charInfoFromDB.FamilyId != -1)
                    return "Character is in family.";
                if (charInfoFromDB.PartnerId != 0)
                    return "Character is married.";
                if (charInfoFromDB.GuildId != 0 && charInfoFromDB.GuildRank < 2)
                    return "Character is the leader of a guild.";
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Change character name");
                return "SQL Error";
            }
            try
            {
                var accInfoFromDB = dbContext.Accounts.Where(x => x.Id == accountId).Select(x => new { x.Tempban }).FirstOrDefault();
                if (accInfoFromDB == null)
                    return "Account does not exist.";
                if (accInfoFromDB.Tempban != DateTimeOffset.MinValue && accInfoFromDB.Tempban != DefaultDates.getTempban())
                    return "Account has been banned.";
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Change character name");
                return "SQL Error";
            }
            try
            {
                var rowcount = dbContext.Characters.Where(x => x.AccountId == accountId && x.World == newWorld).Count();

                if (rowcount >= 3) return "Too many characters on destination world.";
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Change character name");
                return "SQL Error";
            }
            return null;
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
                    log.Error(e, "Failed to register world transfer for chr {CharacterName}", getName());
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
                    log.Error(e, "Failed to register world transfer for chr {CharacterName}", getName());
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Failed to get DB connection while registering world transfer");
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
                log.Error(e, "Failed to cancel pending world transfer for chr {CharacterName}", getName());
                return false;
            }
        }

        public static bool doWorldTransfer(DBContext dbContext, int characterId, int oldWorld, int newWorld, int worldTransferId)
        {
            int mesos = 0;
            try
            {
                var mesosFromDB = dbContext.Characters.Where(x => x.Id == characterId).Select(x => new { x.Meso }).FirstOrDefault();
                if (mesosFromDB == null)
                {
                    Log.Logger.Warning("Character data invalid for world transfer? chrId {CharacterId}", characterId);
                    return false;
                }
                mesos = mesosFromDB.Meso;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to do world transfer for chrId {CharacterId}", characterId);
                return false;
            }
            try
            {
                dbContext.Characters.Where(x => x.Id == characterId).ExecuteUpdate(x => x.SetProperty(y => y.World, newWorld)
                    .SetProperty(y => y.Meso, Math.Min(mesos, 1000000))
                    .SetProperty(y => y.GuildId, 0)
                    .SetProperty(y => y.GuildRank, 5));
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to update chrId {CharacterId} during world transfer", characterId);
                return false;
            }
            try
            {
                dbContext.Buddies.Where(x => x.CharacterId == characterId || x.BuddyId == characterId).ExecuteDelete();
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to delete buddies for chrId {CharacterId} during world transfer", characterId);
                return false;
            }
            if (worldTransferId != -1)
            {
                try
                {
                    dbContext.Worldtransfers.Where(x => x.Id == worldTransferId).ExecuteUpdate(x => x.SetProperty(y => y.CompletionTime, DateTimeOffset.Now));
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, "Failed to update world transfer for chrId {CharacterId}", characterId);
                    return false;
                }
            }
            return true;
        }
    }
}
