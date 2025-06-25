using Application.Core.Channel;
using Application.Core.Duey;
using Application.Core.Game.Items;
using Application.Core.Game.Life;
using Application.Core.Game.Relation;
using Application.Core.Managers.Constants;
using Application.Core.Models;
using Application.Core.ServerTransports;
using Application.Shared.Constants.Job;
using Application.Shared.Items;
using Application.Shared.Team;
using AutoMapper;
using client.creator;
using client.inventory;
using Dto;
using net.packet.outs;
using net.server.guild;
using Org.BouncyCastle.Asn1.X509;
using server;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using tools;
using static Mysqlx.Notice.Warning.Types;

namespace Application.Core.Servers.Services
{
    /// <summary>
    /// 暂时放在Application.Core，之后会把Application.Core改成Application.Core.Channel
    /// </summary>
    public class ChannelService
    {
        readonly IChannelServerTransport _tranport;
        readonly CharacterService _characteService;
        readonly WorldChannel _server;
        readonly IMapper _mapper;

        public ChannelService(IChannelServerTransport tranport, CharacterService characteService, WorldChannel server, IMapper mapper)
        {
            _tranport = tranport;
            _characteService = characteService;
            _server = server;
            _mapper = mapper;
        }
        public void SetPlayerOnlined(int id)
        {
            _tranport.SetPlayerOnlined(id, _server.getId());
        }
        public Dto.PlayerGetterDto? GetPlayerData(string clientSession, int cid)
        {
            return _tranport.GetPlayerData(clientSession, _server.getId(), cid);
        }
        public void RemovePlayerIncomingInvites(int id)
        {
            _tranport.SendRemovePlayerIncomingInvites(id);
        }

        public void SaveChar(Player player, int? setChannel = null)
        {
            var dto = _characteService.Deserialize(player);
            if (setChannel != null)
            {
                dto.Channel = setChannel.Value;
            }
            _tranport.SendPlayerObject(dto);
        }

        public int CreatePlayer(int type, int accountId, string name, int face, int hair, int skin, int top, int bottom, int shoes, int weapon, int gender)
        {
            return CharacterFactory.GetNoviceCreator(type, this).createCharacter(accountId, name, face, hair, skin, top, bottom, shoes, weapon, gender);
        }

        public int CreatePlayer(IChannelClient client, int type, string name, int face, int hair, int skin, int gender, int improveSp)
        {
            var checkResult = _tranport.CreatePlayerCheck(new Dto.CreateCharCheckRequest { AccountId = client.AccountId, Name = name }).Code;
            if (checkResult != CreateCharResult.Success)
                return checkResult;

            return CharacterFactory.GetVeteranCreator(type, this).createCharacter(client.AccountId, name, face, hair, skin, gender, improveSp);
        }

        public int SendNewPlayer(IPlayer player)
        {
            var dto = _characteService.DeserializeNew(player);
            return _tranport.SendNewPlayer(dto).Code;
        }

        public void SaveBuff(IPlayer player)
        {
            _tranport.SendBuffObject(player.getId(), _characteService.DeserializeBuff(player));
        }

        public Dto.PlayerBuffSaveDto GetBuffFromStorage(IPlayer player)
        {
            return _tranport.GetBuffObject(player.Id);
        }

        public Item GenerateCouponItem(int itemId, short quantity)
        {
            CashItem it = new CashItem(77777777, itemId, 7777, ItemConstants.isPet(itemId) ? 30 : 0, quantity, true);
            return CashItem2Item(it);
        }

        public Item CashItem2Item(CashItem cashItem)
        {
            Item item;

            if (ItemConstants.isPet(cashItem.getItemId()))
            {
                item = new Pet(cashItem.getItemId(), 0, Yitter.IdGenerator.YitIdHelper.NextId());
            }
            else if (ItemConstants.getInventoryType(cashItem.getItemId()).Equals(InventoryType.EQUIP))
            {
                item = ItemInformationProvider.getInstance().getEquipById(cashItem.getItemId());
            }
            else
            {
                item = new Item(cashItem.getItemId(), 0, cashItem.getCount());
            }

            if (ItemConstants.EXPIRING_ITEMS)
            {
                if (cashItem.Period == 1)
                {
                    switch (cashItem.getItemId())
                    {
                        case ItemId.DROP_COUPON_2X_4H:
                        case ItemId.EXP_COUPON_2X_4H: // 4 Hour 2X coupons, the period is 1, but we don't want them to last a day.
                            item.setExpiration(_server.Container.getCurrentTime() + (long)TimeSpan.FromHours(4).TotalMilliseconds);
                            /*
                            } else if(itemId == 5211047 || itemId == 5360014) { // 3 Hour 2X coupons, unused as of now
                                    item.setExpiration(Server.getInstance().getCurrentTime() + HOURS.toMillis(3));
                            */
                            break;
                        case ItemId.EXP_COUPON_3X_2H:
                            item.setExpiration(_server.Container.getCurrentTime() + (long)TimeSpan.FromHours(2).TotalMilliseconds);
                            break;
                        default:
                            item.setExpiration(_server.Container.getCurrentTime() + (long)TimeSpan.FromDays(1).TotalMilliseconds);
                            break;
                    }
                }
                else
                {
                    item.setExpiration(_server.Container.getCurrentTime() + (long)TimeSpan.FromDays(cashItem.Period).TotalMilliseconds);
                }
            }

            item.setSN(cashItem.getSN());
            return item;
        }

        public Dictionary<int, List<DropEntry>> RequestAllReactorDrops()
        {
            var allItems = _tranport.RequestAllReactorDrops();
            return allItems.Items.GroupBy(x => x.DropperId).ToDictionary(x => x.Key, x => _mapper.Map<List<DropEntry>>(x.ToArray()));
        }

        internal DueyPackageObject[] GetDueyPackages(int id)
        {
            return _mapper.Map<DueyPackageObject[]>(_tranport.GetPlayerDueyPackages(id));
        }

        internal DueyPackageObject? GetDueyPackageByPackageId(int id)
        {
            return _mapper.Map<DueyPackageObject?>(_tranport.GetDueyPackageByPackageId(id));
        }

        internal int[] GetCardTierSize()
        {
            return _tranport.GetCardTierSize();
        }

        internal List<List<int>> GetMostSellerCashItems()
        {
            return _mapper.Map<List<List<int>>>(_tranport.GetMostSellerCashItems());
        }

        internal Dto.OwlSearchRecordDto[] GetOwlSearchedItems()
        {
            return _tranport.GetOwlSearchedItems().Items.ToArray();
        }

        internal void AddCashItemBought(int sn)
        {
            _tranport.AddCashItemBought(sn);
        }

        internal void SendTeamChat(string name, string chattext)
        {
            _tranport.SendTeamChat(name, chattext);
        }
    }
}
