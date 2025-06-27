using Application.Core.Channel;
using Application.Core.Game.Players;
using Application.Core.Game.Trades;
using Application.Core.Model;
using Application.Core.ServerTransports;
using Application.Shared.Items;
using AutoMapper;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using client.processor.npc;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Mozilla;
using server;
using System.Numerics;
using tools;

namespace Application.Core.Servers.Services
{
    public class ItemService
    {
        readonly IMapper _mapper;
        readonly IChannelServerTransport _transport;
        readonly ILogger<ItemService> _logger;
        readonly WorldChannelServer _server;

        public ItemService(IMapper mapper, IChannelServerTransport transport, ILogger<ItemService> logger, WorldChannelServer server)
        {
            _mapper = mapper;
            _transport = transport;
            _logger = logger;
            _server = server;
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
            Remove(player.Client, item.getPosition());
            var request = new Dto.CreateTVMessageRequest { MasterId = player.Id, ToName = victim, Type = tvType, ShowEar = showEar };
            request.MessageList.AddRange(messages);
            _transport.BroadcastTV(request);
        }

        private void Remove(IChannelClient c, short position)
        {
            Inventory cashInv = c.OnlinedCharacter.getInventory(InventoryType.CASH);
            cashInv.lockInventory();
            try
            {
                InventoryManipulator.removeFromSlot(c, InventoryType.CASH, position, 1, true, false);
            }
            finally
            {
                cashInv.unlockInventory();
            }
        }


        public void OnBroadcastTV(Dto.CreateTVMessageResponse data)
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
                            chr.sendPacket(PacketCreator.serverNotice(3, data.Data.Master.Channel, data.Data.Master.Character.NameWithMedal + " : " + noticeMsg, data.ShowEar));
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
        }

        public void OnBroadcastTVFinished(Empty data)
        {
            foreach (var ch in _server.Servers.Values)
            {
                ch.broadcastPacket(PacketCreator.removeTV());
            }
        }
    }
}
