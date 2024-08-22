using client.inventory.manipulator;
using client.processor.npc;
using client;
using Microsoft.EntityFrameworkCore;
using net.server;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using YamlDotNet.Core.Tokens;

namespace Application.Core.Managers
{
    public class CharacterManager
    {
        private static string[] BLOCKED_NAMES = {"admin", "owner", "moderator", "intern", "donor", "administrator", "FREDRICK", "help", "helper", "alert", "notice", "maplestory", "fuck", "wizet", "fucking", "negro", "fuk", "fuc", "penis", "pussy", "asshole", "gay",
        "nigger", "homo", "suck", "cum", "shit", "shitty", "condom", "security", "official", "rape", "nigga", "sex", "tit", "boner", "orgy", "clit", "asshole", "fatass", "bitch", "support", "gamemaster", "cock", "gaay", "gm",
        "operate", "master", "sysop", "party", "GameMaster", "community", "message", "event", "test", "meso", "Scania", "yata", "AsiaSoft", "henesys"};
        public static bool CheckCharacterName(string name)
        {
            // 禁用名
            if (BLOCKED_NAMES.Contains(name.ToLower()))
                return false;

            if (!Regex.IsMatch(name, "[a-zA-Z0-9]{3,12}"))
                return false;

            using DBContext dbContext = new DBContext();
            if (dbContext.Characters.Any(x => x.Name == name))
                return false;

            return true;
        }

        public static bool DeleteCharacterFromDB(int cid)
        {
            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                var charModel = dbContext.Characters.FirstOrDefault(x => x.Id == cid);
                if (charModel == null)
                    return false;

                if (charModel.GuildId > 0 && charModel.GuildRank < 1)
                {
                    var guildInfo = dbContext.Guilds.FirstOrDefault(x => x.GuildId == charModel.GuildId);
                    if (guildInfo != null)
                    {
                        if (guildInfo.AllianceId > 0)
                        {
                            var allianceInfo = dbContext.Alliances.FirstOrDefault(x => x.Id == guildInfo.AllianceId);
                            if (allianceInfo != null)
                            {
                                if (charModel.AllianceRank == 1)
                                {
                                    dbContext.AllianceGuilds.Where(x => x.AllianceId == allianceInfo.Id).ExecuteDelete();
                                    dbContext.Alliances.Where(x => x.Id == allianceInfo.Id).ExecuteDelete();
                                }
                                else
                                {
                                    dbContext.AllianceGuilds.Where(x => x.GuildId == guildInfo.GuildId).ExecuteDelete();
                                }
                            }
                        }
                        dbContext.Guilds.Remove(guildInfo);
                    }
                }
                dbContext.Buddies.Where(x => x.CharacterId == cid || x.BuddyId == cid).ExecuteDelete();
                var threadIdList = dbContext.BbsThreads.Where(x => x.Postercid == cid).Select(x => x.Threadid).ToList();
                dbContext.BbsReplies.Where(x => threadIdList.Contains(x.Threadid)).ExecuteDelete();
                dbContext.BbsThreads.Where(x => x.Postercid == cid).ExecuteDelete();

                dbContext.Wishlists.Where(x => x.CharId == cid).ExecuteDelete();
                dbContext.Cooldowns.Where(x => x.Charid == cid).ExecuteDelete();
                dbContext.Playerdiseases.Where(x => x.Charid == cid).ExecuteDelete();
                dbContext.AreaInfos.Where(x => x.Charid == cid).ExecuteDelete();
                dbContext.Monsterbooks.Where(x => x.Charid == cid).ExecuteDelete();
                dbContext.Characters.Where(x => x.Id == cid).ExecuteDelete();
                dbContext.FamilyCharacters.Where(x => x.Cid == cid).ExecuteDelete();
                dbContext.Famelogs.Where(x => x.CharacteridTo == cid).ExecuteDelete();

                var inventoryItems = dbContext.Inventoryitems.Where(x => x.Characterid == cid).ToList();

                var inventoryItemIdList = inventoryItems.Select(x => x.Inventoryitemid).ToList();
                var ringIdList = dbContext.Inventoryequipments.Where(x => inventoryItemIdList.Contains(x.Inventoryitemid)).Select(x => x.RingId).ToList();
                dbContext.Rings.Where(x => ringIdList.Contains(x.Id)).ExecuteDelete();

                var inventoryPetIdList = inventoryItems.Select(x => x.Petid).ToList();
                dbContext.Pets.Where(x => inventoryPetIdList.Contains(x.Petid)).ExecuteDelete();

                dbContext.Inventoryequipments.Where(x => inventoryItemIdList.Contains(x.Inventoryitemid)).ExecuteDelete();
                dbContext.Inventoryitems.Where(x => x.Characterid == cid).ExecuteDelete();

                dbContext.Medalmaps.Where(x => x.Characterid == cid).ExecuteDelete();

                dbContext.Questprogresses.Where(x => x.Characterid == cid).ExecuteDelete();

                dbContext.Queststatuses.Where(x => x.Characterid == cid).ExecuteDelete();

                dbContext.Fredstorages.Where(x => x.Cid == cid).ExecuteDelete();

                var mtsCartIdList = dbContext.MtsCarts.Where(x => x.Cid == cid).Select(x => x.Id).ToList();
                dbContext.MtsItems.Where(x => mtsCartIdList.Contains(x.Id)).ExecuteDelete();
                dbContext.MtsCarts.Where(x => x.Cid == cid).ExecuteDelete();


                string[] toDel = { "famelog", "inventoryitems", "keymap", "queststatus", "savedlocations", "trocklocations", "skillmacros", "skills", "eventstats", "server_queue" };
                foreach (string s in toDel)
                {
                    dbContext.Database.ExecuteSqlRaw("DELETE FROM `" + s + "` WHERE characterid = @cid", new MySqlParameter("cid", cid));
                }
                dbContext.SaveChanges();
                dbTrans.Commit();
                return true;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
                return false;
            }
        }
    }
}
