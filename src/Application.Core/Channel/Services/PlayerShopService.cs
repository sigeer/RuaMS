using Application.Core.Channel.DataProviders;
using Application.Core.Game.Trades;
using Application.Core.Models;
using Application.Core.ServerTransports;
using Application.Shared.Constants.Item;
using AutoMapper;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using ItemProto;
using Microsoft.Extensions.Logging;
using Mysqlx.Crud;
using Org.BouncyCastle.Utilities.Collections;
using System.Data.Common;
using tools;

namespace Application.Core.Channel.Services
{
    public class PlayerShopService
    {
        readonly IChannelServerTransport _transport;
        readonly IMapper _mapper;
        readonly ILogger<PlayerShopService> _logger;
        readonly WorldChannelServer _server;

        public PlayerShopService(IChannelServerTransport transport, IMapper mapper, ILogger<PlayerShopService> logger, WorldChannelServer server)
        {
            _transport = transport;
            _mapper = mapper;
            _logger = logger;
            _server = server;
        }

        private byte CanRetrieveFromFredrick(IPlayer chr, int netMeso, List<Item> items)
        {
            if (!Inventory.checkSpot(chr, items))
            {
                if (chr.canHoldUniques(items.Select(x => x.getItemId()).ToList()))
                {
                    return 0x22;
                }
                else
                {
                    return 0x20;
                }
            }

            if (netMeso > 0)
            {
                if (!chr.canHoldMeso(netMeso))
                {
                    return 0x1F;
                }
            }
            else
            {
                if (chr.getMeso() < -1 * netMeso)
                {
                    return 0x21;
                }
            }

            return 0x0;
        }


        public RemoteHiredMerchantData LoadPlayerHiredMerchant(IPlayer chr)
        {
            var res = _server.Transport.LoadPlayerHiredMerchant(new ItemProto.GetPlayerHiredMerchantRequest { MasterId = chr.Id });

            return _mapper.Map<RemoteHiredMerchantData>(res);
        }

        public void FredrickRetrieveItems(IChannelClient c)
        {
            // thanks Gustav for pointing out the dupe on Fredrick handling
            if (c.tryacquireClient())
            {
                try
                {
                    var chr = c.OnlinedCharacter;

                    try
                    {
                        var res = LoadPlayerHiredMerchant(chr);
                        if (res.Channel > 0)
                        {
                            // 有正在营业的商店
                            return;
                        }

                        var items = _mapper.Map<List<Item>>(res.Items);

                        byte response = CanRetrieveFromFredrick(chr, res.Mesos, items);
                        if (response != 0)
                        {
                            chr.sendPacket(PacketCreator.fredrickMessage(response));
                            return;
                        }

                        CommitRetrievedResponse commitRes = _server.Transport.CommitRetrievedFromFredrick(new CommitRetrievedRequest
                        {
                            OwnerId = chr.Id,
                        });
                        if (!commitRes.IsSuccess)
                        {
                            return;
                        }

                        chr.GainMeso(res.Mesos, false);

                        var commitRequest = new CommitRetrievedRequest
                        {
                            OwnerId = chr.Id,
                        };

                        foreach (var it in items)
                        {
                            InventoryManipulator.addFromDrop(chr.Client, it, false);
                            var itemName = ItemInformationProvider.getInstance().getName(it.getItemId());
                            _logger.LogDebug("Chr {CharacterName} gained {ItemQuantity}x {ItemName} ({CharacterId})", chr.getName(), it.getQuantity(), itemName, it.getItemId());
                        }

                        chr.sendPacket(PacketCreator.fredrickMessage(0x1E));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                    }
                }
                finally
                {
                    c.releaseClient();
                }
            }
        }
        public PlayerHiredMerchantStatus CanHiredMerchant(IPlayer chr)
        {
            return (PlayerHiredMerchantStatus)_transport.CanHiredMerchant(new ItemProto.CanHiredMerchantRequest { MasterId = chr.Id }).Code;
        }


        public void OnHiredMerchantItemBuy(ItemProto.NotifyItemPurchasedResponse data)
        {
            var owner = _server.FindPlayerById(data.OwnerId);
            if (owner != null)
            {
                string qtyStr = data.Quantity > 1 ? " x " + data.Quantity : "";
                owner.dropMessage(6,
                    $"[Hired Merchant] Item '{ItemInformationProvider.getInstance().getName(data.ItemId)}'{qtyStr} has been sold for {data.GainedMeso} mesos. ({data.Left} left)");
            }

        }

        internal void OwlSearch(IPlayer chr, int useItemId, int searchItemId)
        {
            ItemProto.OwlSearchResponse data = _server.Transport.SendOwlSearch(
                new ItemProto.OwlSearchRequest { MasterId = chr.Id, UsedItemId = useItemId, SearchItemId = searchItemId });

            if (data.Items.Count > 0)
            {
                // 消耗道具
                chr.GainItem(useItemId, -1, false, true);
            }

            chr.sendPacket(PacketCreator.owlOfMinerva(useItemId, _mapper.Map<OwlSearchResult>(data)));
            chr.sendPacket(PacketCreator.enableActions());
        }
    }
}
