using Application.Core.Datas;
using Application.Core.Game.Commands.Gm4;
using Application.Core.Game.Players;
using Application.Core.Game.TheWorld;
using Application.Core.Login.Datas;
using Application.EF;
using Application.Shared.Characters;
using Application.Shared.Items;
using AutoMapper;
using client.inventory;
using client.inventory.manipulator;
using client.processor.npc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using MySql.EntityFrameworkCore.Extensions;
using net.server;
using Serilog;
using System.Text.RegularExpressions;

namespace Application.Core.Login.Services
{
    public class CharacterService
    {
        readonly IMapper _mapper;
        readonly CharacterManager _characterManager;
        readonly AccountManager _accountManager;



        private static string[] BLOCKED_NAMES = {
            "admin", "owner", "moderator", "intern", "donor", "administrator", "FREDRICK", "help", "helper", "alert", "notice", "maplestory", "fuck", "wizet", "fucking",
            "negro", "fuk", "fuc", "penis", "pussy", "asshole", "gay", "nigger", "homo", "suck", "cum", "shit", "shitty", "condom", "security", "official", "rape", "nigga",
            "sex", "tit", "boner", "orgy", "clit", "asshole", "fatass", "bitch", "support", "gamemaster", "cock", "gaay", "gm", "operate", "master",
            "sysop", "party", "GameMaster", "community", "message", "event", "test", "meso", "Scania", "yata", "AsiaSoft", "henesys"};

        public CharacterService(IMapper mapper, CharacterManager characterManager, AccountManager accountManager)
        {
            _mapper = mapper;
            _characterManager = characterManager;
            _accountManager = accountManager;
        }

        public bool CheckCharacterName(string name)
        {
            // 禁用名
            if (BLOCKED_NAMES.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return false;

            var bLength = GlobalTools.Encoding.GetBytes(name).Length;
            if (bLength < 3 || bLength > 12)
                return false;

            if (!Regex.IsMatch(name, "^[a-zA-Z0-9\\u4e00-\\u9fa5]+$"))
                return false;

            using DBContext dbContext = new DBContext();
            if (dbContext.Characters.Any(x => x.Name == name))
                return false;

            return true;
        }


        public bool DeleteChar(int cid, int senderAccId)
        {
            if (!_accountManager.ValidAccountCharacter(senderAccId, cid))
            {    // thanks zera (EpiphanyMS) for pointing a critical exploit with non-authed character deletion request
                return false;
            }

            int accId = senderAccId;
            int world = 0;
            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();

                var characterModel = dbContext.Characters.Where(x => x.Id == cid).FirstOrDefault();
                if (characterModel == null)
                    return false;

                if (characterModel.AccountId != senderAccId)
                    return false;

                world = characterModel.World;

                var storage = Server.getInstance().getWorld(world).getPlayerStorage();
                var dbBuddyIdList = dbContext.Buddies.Where(x => x.CharacterId == cid).Select(x => x.BuddyId).ToList();
                dbBuddyIdList.ForEach(buddyid =>
                {
                    var buddy = storage.getCharacterById(buddyid);
                    if (buddy != null && buddy.IsOnlined)
                    {
                        buddy.deleteBuddy(cid);
                    }
                });
                dbContext.Buddies.Where(x => x.CharacterId == cid).ExecuteDelete();

                // TODO: 退出队伍

                // TODO: 退出家族

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
                var inventoryEquipList = dbContext.Inventoryequipments.Where(x => inventoryItemIdList.Contains(x.Inventoryitemid)).ToList();
                inventoryItems.ForEach(rs =>
                {
                    var ringsList = inventoryEquipList.Where(x => x.Inventoryitemid == rs.Inventoryitemid).Select(x => x.RingId).ToList();
                    ringsList.ForEach(ringid =>
                    {
                        if (ringid > -1)
                        {
                            dbContext.Rings.Where(x => x.Id == ringid).ExecuteDelete();
                            CashIdGenerator.freeCashId(ringid);
                        }
                    });

                    dbContext.Pets.Where(x => x.Petid == rs.Petid).ExecuteDelete();
                    CashIdGenerator.freeCashId(rs.Petid);
                });
                dbContext.Inventoryitems.RemoveRange(inventoryItems);
                dbContext.Inventoryequipments.RemoveRange(inventoryEquipList);

                dbContext.Medalmaps.Where(x => x.Characterid == cid).ExecuteDelete();
                dbContext.Questprogresses.Where(x => x.Characterid == cid).ExecuteDelete();
                dbContext.Queststatuses.Where(x => x.Characterid == cid).ExecuteDelete();

                FredrickProcessor.removeFredrickLog(dbContext, cid);   // thanks maple006 for pointing out the player's Fredrick items are not being deleted at character deletion

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

                _accountManager.UpdateAccountCharacterCacheByRemove(accId, cid);
                _characterManager.Remove(cid);
                return true;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
                return false;
            }
        }

        public List<IPlayer> GetCharactersView(int[] idList)
        {
            var dataList = _characterManager.GetCharactersView(idList);

            List<IPlayer> list = new List<IPlayer>();
            foreach (var c in dataList)
            {
                var player = _mapper.Map<IPlayer>(c.Character);
                Inventory inv = player.Bag[InventoryType.EQUIPPED];
                foreach (var equip in c.Items)
                {
                    var item = _mapper.Map<Equip>(equip);
                    inv.addItemFromDB(item);
                }
            }
            return list;
        }

    }
}
