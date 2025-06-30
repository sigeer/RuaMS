using Application.Core.Channel;
using Application.Core.Channel.Transactions;
using Application.Core.Game.Items;
using Application.Core.Game.Players;
using Application.Core.Model;
using Application.Core.ServerTransports;
using Application.Shared;
using Application.Shared.Items;
using AutoMapper;
using client.inventory;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using server;
using tools;

namespace Application.Core.Servers.Services
{
    public class ItemService
    {
        readonly IMapper _mapper;
        readonly IChannelServerTransport _transport;
        readonly ILogger<ItemService> _logger;
        readonly WorldChannelServer _server;
        readonly ItemTransactionStore _itemStore;

        public ItemService(IMapper mapper, IChannelServerTransport transport, ILogger<ItemService> logger, WorldChannelServer server, ItemTransactionStore itemStore)
        {
            _mapper = mapper;
            _transport = transport;
            _logger = logger;
            _server = server;
            _itemStore = itemStore;
        }

        public List<SpecialCashItem> GetSpecialCashItems()
        {
            return _mapper.Map<List<SpecialCashItem>>(_transport.RequestSpecialCashItems().Items);
        }

        public GiftModel[] LoadPlayerGifts(int id)
        {
            var remoteData = _transport.LoadPlayerGifts(id);
            return _mapper.Map<GiftModel[]>(remoteData);
        }

        internal Shop GetShop(int id, bool isShopId)
        {
            return _mapper.Map<Shop>(_transport.GetShop(id, isShopId));
        }


        internal void UseCash_TV(IPlayer player, Item item, string? victim, List<string> messages, int tvType, bool showEar)
        {
            var request = new ItemDto.CreateTVMessageRequest { 
                MasterId = player.Id, 
                ToName = victim, 
                Type = tvType, 
                ShowEar = showEar,
                Transaction = _mapper.Map<ItemDto.ItemTransaction>(_itemStore.BeginTransaction(player, [item]))
            };
            request.MessageList.AddRange(messages);
            _transport.BroadcastTV(request);
        }

        public void OnBroadcastTV(ItemDto.CreateTVMessageResponse data)
        {
            if (data.Code == 0)
            {
                var noticeMsg = string.Join(" ", data.Data.MessageList);
                foreach (var ch in _server.Servers.Values)
                {
                    foreach (var chr in ch.Players.getAllCharacters())
                    {
                        chr.sendPacket(PacketCreator.enableTV());
                        chr.sendPacket(PacketCreator.sendTV(data.Data.Master, data.Data.MessageList.ToArray(), data.Data.Type <= 2 ? data.Data.Type : data.Data.Type - 3, data.Data.MasterPartner));

                        if (data.Data.Type >= 3)
                        {
                            chr.sendPacket(PacketCreator.serverNotice(3, data.Data.Master.Channel, CharacterViewDtoUtils.GetPlayerNameWithMedal(data.Data.Master) + " : " + noticeMsg, data.ShowEar));
                        }
                    }
                }
            }
            else
            {
                var master = _server.FindPlayerById(data.MasterId);
                if (master != null)
                {
                    master.dropMessage(1, "MapleTV is already in use.");
                }

            }

            var transactionOwner = _server.FindPlayerById(data.Transaction.PlayerId);
            if (transactionOwner != null)
            {
                var tsc = _mapper.Map<ItemTransaction>(data.Transaction);
                tsc.Player = transactionOwner;
                _itemStore.ProcessTransaction(tsc);
            }
        }

        public void OnBroadcastTVFinished(Empty data)
        {
            foreach (var ch in _server.Servers.Values)
            {
                ch.broadcastPacket(PacketCreator.removeTV());
            }
        }

        internal void UseCash_ItemMegaphone(IPlayer player, Item costItem, Item? item, string message, bool isWishper)
        {
            var transaction = _itemStore.BeginTransaction(player, [costItem]);

            var request = new ItemDto.UseItemMegaphoneRequest { 
                MasterId = player.Id, 
                Message = message, 
                Item = _mapper.Map<Dto.ItemDto>(item), 
                IsWishper = isWishper, 
                Transaction = _mapper.Map<ItemDto.ItemTransaction>(transaction)
            };
            _transport.SendItemMegaphone(request);
        }

        public void OnItemMegaphon(ItemDto.UseItemMegaphoneResponse data)
        {
            if (data.Code == 0)
            {
                foreach (var ch in _server.Servers.Values)
                {
                    ch.broadcastPacket(PacketCreator.itemMegaphone(data.Data.Message, data.Data.IsWishper, data.Data.SenderChannel, _mapper.Map<Item>(data.Data.Item)));
                }
            }
            else
            {
                
            }

            var transactionOwner = _server.FindPlayerById(data.Transaction.PlayerId);
            if (transactionOwner != null)
            {
                var tsc = _mapper.Map<ItemTransaction>(data.Transaction);
                tsc.Player = transactionOwner;
                _itemStore.ProcessTransaction(tsc);
            }
        }
    }
}
