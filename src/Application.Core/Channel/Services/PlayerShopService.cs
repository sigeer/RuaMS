using Application.Core.Channel.Net.Packets;
using Application.Core.Models;
using Application.Core.ServerTransports;
using client.inventory;
using client.inventory.manipulator;
using ItemProto;
using Microsoft.Extensions.Logging;
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

        private byte CanRetrieveFromFredrick(Player chr, int netMeso, int fee, List<Item> items)
        {
            if (!Inventory.checkSpot(chr, items))
            {
                if (chr.canHoldUniques(items.Select(x => x.getItemId()).ToList()))
                {
                    return FredrickPackets.FredrickMessage_InvFull;
                }
                else
                {
                    return FredrickPackets.FredrickMessage_Unique;
                }
            }

            var leftMeso = netMeso - fee;
            if (chr.getMeso() + leftMeso < 0)
            {
                return FredrickPackets.FredrickMessage_FeeRequired;
            }

            if (!chr.canHoldMeso(leftMeso))
            {
                return FredrickPackets.FredrickMessage_TooMuchMeso;
            }

            return FredrickPackets.FredrickMessage_RetrieveSuccess;
        }

        public async Task SendFriankPacket(Player chr)
        {
            var res = _server.Transport.LoadPlayerHiredMerchant(new ItemProto.GetPlayerHiredMerchantRequest { MasterId = chr.Id });

            var remoteData = _mapper.Map<RemoteHiredMerchantData>(res);
            if (remoteData.MapId > 0)
            {
                await chr.SendPacket(FredrickPackets.GetFredrickShopActive(remoteData.MapId, (byte)remoteData.Channel));
            }
            else
            {
                if (remoteData.HasItem)
                {
                    await chr.SendPacket(FredrickPackets.GetFredrick(remoteData));
                }
                else
                {
                    await chr.SendPacket(FredrickPackets.Nothing());
                }
            }
        }


        public RemoteHiredMerchantData LoadPlayerHiredMerchant(Player chr)
        {
            var res = _server.Transport.LoadPlayerHiredMerchant(new ItemProto.GetPlayerHiredMerchantRequest { MasterId = chr.Id });

            return _mapper.Map<RemoteHiredMerchantData>(res);
        }

        public async Task FredrickRetrieveItems(IChannelClient c)
        {
            // thanks Gustav for pointing out the dupe on Fredrick handling
            {
                await c.tryacquireClient();
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

                        byte response = CanRetrieveFromFredrick(chr, res.Meso, res.FeeMeso, items);
                        if (response != FredrickPackets.FredrickMessage_RetrieveSuccess)
                        {
                            await chr.SendPacket(FredrickPackets.FredrickMessage(response));
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

                        await chr.GainMeso(res.Meso - res.FeeMeso);

                        foreach (var it in items)
                        {
                            await InventoryManipulator.addFromDrop(chr.Client, it, false);
                            _logger.LogDebug("Chr {CharacterName} gained {ItemQuantity}x {ItemName} ({ItemId})",
                                chr.getName(),
                                it.getQuantity(),
                                ClientCulture.SystemCulture.GetItemName(it.getItemId()),
                                it.getItemId());
                        }

                        await chr.SendPacket(FredrickPackets.FredrickMessage(response));
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
        public async Task<bool> CanHiredMerchant(Player chr)
        {
            var remoteData = LoadPlayerHiredMerchant(chr);
            if (remoteData.Channel > 0)
            {
                await chr.SendPacket(FredrickPackets.HasHiredMerchant(remoteData.MapId, (byte)remoteData.Channel));
                return false;
            }

            if (remoteData.HasItem)
            {
                await chr.SendPacket(FredrickPackets.retrieveFirstMessage());
                return false;
            }

            return true;
        }


        //public void OnHiredMerchantItemBuy(ItemProto.NotifyItemPurchasedResponse data)
        //{
        //    var owner = _server.FindPlayerById(data.OwnerId);
        //    if (owner != null)
        //    {
        //        string qtyStr = data.Quantity > 1 ? " x " + data.Quantity : "";
        //        owner.dropMessage(6,
        //            $"[Hired Merchant] Item '{owner.Client.CurrentCulture.GetItemName(data.ItemId)}'{qtyStr} has been sold for {data.GainedMeso} mesos. ({data.Left} left)");
        //    }

        //}

        internal async Task OwlSearch(Player chr, int useItemId, int searchItemId)
        {
            ItemProto.OwlSearchResponse data = _server.Transport.SendOwlSearch(
                new ItemProto.OwlSearchRequest { MasterId = chr.Id, UsedItemId = useItemId, SearchItemId = searchItemId });

            if (data.Items.Count > 0)
            {
                // 消耗道具
                await chr.GainItem(useItemId, -1, show: GainItemShow.ShowInChat);
            }

            await chr.SendPacket(PacketCreator.OwlOfMinerva(searchItemId, data));
            await chr.SendPacket(PacketCreator.enableActions());
        }
    }
}
