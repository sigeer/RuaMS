using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Players;
using Application.Core.Game.Relation;
using Application.Core.Scripting.Events;
using Application.Module.Marriage.Channel.Models;
using Application.Module.Marriage.Channel.Net;
using Application.Module.Marriage.Common.ErrorCodes;
using Application.Module.Marriage.Common.Models;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Item;
using Application.Shared.Items;
using Application.Utility.Compatible;
using AutoMapper;
using client.inventory;
using client.inventory.manipulator;
using MarriageProto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using scripting.Event;
using System.Collections.Concurrent;
using tools;

namespace Application.Module.Marriage.Channel
{
    public class WeddingManager
    {
        readonly ILogger<WeddingManager> _logger;
        readonly IChannelServerTransport _transport;
        readonly IMapper _mapper;
        readonly WorldChannelServer _server;
        readonly Configs _config;
        readonly MarriageManager _marriageManager;

        public WeddingManager(ILogger<WeddingManager> logger, IChannelServerTransport transport, IMapper mapper, WorldChannelServer server, IOptions<Configs> options, 
            MarriageManager marriageManager)
        {
            _logger = logger;
            _transport = transport;
            _mapper = mapper;
            _server = server;
            _config = options.Value;
            _marriageManager = marriageManager;
        }

        ConcurrentDictionary<int, WeddingChannelManager> _managerDic = new();

        public int GetBlessExp() => _config.WEDDING_BLESS_EXP;
        public bool IsBlesserShowFX() => _config.WEDDING_BLESSER_SHOWFX;

        public WeddingChannelManager GetWeddingManager(WorldChannel channel)
        {
            return _managerDic.GetOrAdd(channel.getId(), ActivatorUtilities.CreateInstance<WeddingChannelManager>(channel.LifeScope.ServiceProvider, channel));
        }


        public bool AcceptProposal(IPlayer sender, IPlayer receiver, int usedItem)
        {
            int newItemId = EngageItem.GetEngagementBoxId(usedItem);

            if (!InventoryManipulator.checkSpace(receiver.Client, newItemId, 1, "") || !InventoryManipulator.checkSpace(sender.Client, newItemId, 1, ""))
            {
                receiver.sendPacket(PacketCreator.enableActions());
                return false;
            }

            var res = _transport.CreateMarriageRelation(new MarriageProto.CreateMarriageRelationRequest { FromId = sender.Id, ToId = receiver.Id, ItemId = usedItem });
            if (res.Code == 0)
            {
                var marriageData = _mapper.Map<MarriageInfo>(res.Data);
                _marriageManager.SetPlayerMarriageInfo(marriageData);

                InventoryManipulator.removeById(sender.getClient(), InventoryType.USE, usedItem, 1, false, false);

                InventoryManipulator.addById(sender.Client, newItemId, 1);
                InventoryManipulator.addById(receiver.Client, (newItemId + 1), 1);

                sender.sendPacket(WeddingPackets.OnMarriageResult(marriageData));
                receiver.sendPacket(WeddingPackets.OnMarriageResult(marriageData));

                sender.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(receiver.Id, receiver.getMapId()));
                receiver.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(sender.Id, sender.getMapId()));

                return true;
            }

            return false;
        }

        /// <summary>
        /// 申请举办婚礼，js调用
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="isCathedral"></param>
        /// <param name="isPremium"></param>
        public MarriageProto.ReserveWeddingResponse ReserveWedding(IPlayer chr, bool isCathedral, bool isPremium)
        {
            return _transport.ReserveWedding(
                new MarriageProto.ReserveWeddingRequest
                {
                    Channel = chr.Channel,
                    IsCathedral = isCathedral,
                    IsPremium = isPremium,
                    MasterId = chr.Id,
                });
        }

        public void TryInviteGuest(IPlayer chr, Item item, int marriageId, string guestName)
        {
            chr.UseItem(item, 1, () =>
            {
                var res = _transport.TryInviteGuest(new MarriageProto.InviteGuestRequest
                {
                    MarriageId = marriageId,
                    MasterId = chr.Id,
                    GuestName = guestName,
                });
                var code = (InviteErrorCode)res.Code;

                if (code == InviteErrorCode.GuestNotFound)
                {
                    chr.dropMessage(5, "Unable to find " + guestName + "!");
                    return false;
                }

                if (code == InviteErrorCode.MarriageNotFound)
                {
                    chr.dropMessage(5, $"Invitation was not sent to '{guestName}'. Either the time for your marriage reservation already came or it was not found.");
                    return false;
                }

                if (code == InviteErrorCode.DuplicateInvitation)
                {
                    chr.dropMessage(5, $"'{guestName}' is already invited for your marriage.");
                    return false;
                }

                if (code == InviteErrorCode.WeddingUnderway)
                {
                    chr.dropMessage(5, "Wedding is already under way. You cannot invite any more guests for the event.");
                    return false;
                }

                return true;
            });

        }

        public void OnGuestInvited(MarriageProto.InviteGuestCallback data)
        {
            string baseMessage = $"You've been invited to {data.GroomName} and {data.BrideName}'s Wedding!";
            var guestChr = _server.FindPlayerById(data.GuestId);
            if (guestChr != null && guestChr.isLoggedinWorld())
            {
                int newItemId = data.IsCathedral ? ItemId.RECEIVED_INVITATION_CATHEDRAL : ItemId.RECEIVED_INVITATION_CHAPEL;
                var newItem = Item.CreateVirtualItem(newItemId, 1);
                // GiftFrom应该是仅在现金道具上生效，在这里临时使用存放婚礼id
                newItem.setGiftFrom(data.WeddingId.ToString());

                if (InventoryManipulator.addFromDrop(guestChr.Client, newItem, false))
                {
                    guestChr.dropMessage(6, $"[Wedding] {baseMessage}");
                }

            }
        }

        public void TryGetInvitationInfo(IPlayer chr, int weddingId)
        {
            var res = _transport.TryGetInvitationInfo(new MarriageProto.LoadInvitationRequest { WeddingId = weddingId });
            if (res.MarriageId > 0)
            {
                chr.sendPacket(WeddingPackets.sendWeddingInvitation(res.GroomName, res.BrideName));
            }
        }

        public void BreakMarriageRing(IPlayer chr)
        {
            _transport.BreakMarriage(new MarriageProto.BreakMarriageRequest { MasterId = chr.Id });
        }

        public void OnMarriageBroken(MarriageProto.BreakMarriageCallback data)
        {
            if (data.Code != 0)
            {
                return;
            }

            _marriageManager.RemoveLocalData(data.MasterId, data.MasterPartnerId);

            var chr = _server.FindPlayerById(data.MasterId);
            if (chr != null)
            {
                _marriageManager.RemoveMarriageItems(chr);

                chr.dropMessage(5, "You have successfully break the "+ (data.Type == 0 ? "engagement." : "marriage") + " with " + data.MasterPartnerName + ".");
                //chr.sendPacket(Wedding.OnMarriageResult((byte) 0));
                chr.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(0, 0));
            }

            var partner = _server.FindPlayerById(data.MasterPartnerId);
            if (partner != null)
            {
                _marriageManager.RemoveMarriageItems(partner);

                partner.dropMessage(5, data.MasterName + " has decided to break up the " + (data.Type == 0 ? "engagement." : "marriage."));
                //partner.sendPacket(Wedding.OnMarriageResult((byte) 0)); ok, how to gracefully unengage someone without the need to cc?
                partner.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(0, 0));
            }

        }


        public bool HasWeddingRing(IPlayer chr)
        {
            int[] rings = { ItemId.WEDDING_RING_STAR, ItemId.WEDDING_RING_MOONSTONE, ItemId.WEDDING_RING_GOLDEN, ItemId.WEDDING_RING_SILVER };
            return rings.Any(x => chr.haveItemWithId(x));
        }

        public bool HasEngagement(IPlayer chr)
        {
            return ItemId.GetEngagementItems().Any(x => chr.haveItemWithId(x));
        }

        public List<Item> GetUnclaimedMarriageGifts(IPlayer chr)
        {
            return _mapper.Map<List<Item>>(_server.Transport.LoadItemFromStore(new ItemProto.LoadItemsFromStoreRequest { 
                Key = chr.Id, ItemFactory = ItemFactory.MARRIAGE_GIFTS.getValue() }).Items);
        }

        public void TakeItemFromGifts(IPlayer chr, int itemPos)
        {
            if (chr.Client.tryacquireClient())
            {
                var allItems = _mapper.Map<List<Item>>(_transport.LoadMarriageGifts(new LoadMarriageGiftsRequest { MasterId = chr.Id }));
                try
                {
                    var item = allItems.ElementAtOrDefault(itemPos);
                    if (item != null && InventoryManipulator.addFromDrop(chr.Client, item, true))
                    {
                        allItems.RemoveAt(itemPos);
                        StoreGifts(chr.Id, allItems);
                        chr.sendPacket(WeddingPackets.onWeddingGiftResult(0xF, Collections.singletonList(""), allItems));
                    }
                    else
                    {
                        chr.dropMessage(1, "Free a slot on your inventory before collecting this item.");
                        chr.sendPacket(WeddingPackets.onWeddingGiftResult(0xE, Collections.singletonList(""), allItems));
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    chr.dropMessage(1, "You have already collected this item.");
                    chr.sendPacket(WeddingPackets.onWeddingGiftResult(0xE, Collections.singletonList(""), allItems));
                }
                finally
                {
                    chr.Client.releaseClient();
                }
            }
        }

        public MarriageInstance? GetMarriageInstance(IPlayer chr)
        {
            return chr.getEventInstance() as MarriageInstance;
        }

        /// <summary>
        /// 脚本调用
        /// </summary>
        /// <param name="em"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public MarriageInstance CreateMarriageInstance(MarriageEventManager em, string name)
        {
            return em.newInstance(name) as MarriageInstance;
        }

        private void StoreGifts(int id, List<Item> items)
        {
            var request = new ItemProto.StoreItemsRequest();
            request.Key = id;
            request.ItemFactory = ItemFactory.MARRIAGE_GIFTS.getValue();
            request.Items.AddRange(_mapper.Map<Dto.ItemDto[]>(items));
            _server.Transport.SaveItems(request);
        }

        public void StoreGifts(MarriageInstance? marriage, bool isGroom)
        {
            if (marriage == null)
                return;

            var items = isGroom ? marriage.GroomGiftList : marriage.BrideGiftList;
            var cid = isGroom ? marriage.getIntProperty("groomId") : marriage.getIntProperty("brideId");

            StoreGifts(cid, items);
        }

        public void BroadcastWedding(BroadcastWeddingDto data)
        {
            foreach (var ch in _server.Servers.Values)
            {
                ch.dropMessage(6, $"{data.GroomName} and {data.BrideName}'s wedding is going to be started at " + (data.IsCathedral ? "Cathedral" : "Chapel") + $" on Channel {data.Channel}.");
            }
        }

        /// <summary>
        /// 通过背包的邀请函获取相应的婚礼信息
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="isCathedral"></param>
        /// <returns></returns>
        public List<WeddingInfo> GetWeddingMasterByGuestTicket(IPlayer chr, bool isCathedral)
        {
            var manager = GetWeddingManager(chr.getChannelServer());
            var itemId = isCathedral ? ItemId.RECEIVED_INVITATION_CATHEDRAL : ItemId.RECEIVED_INVITATION_CHAPEL;
            var allInvitations = chr.getInventory(InventoryType.ETC).list().Where(x => x.getItemId() == itemId).ToArray();
            if (allInvitations.Length == 0)
                return [];

            List<int> list = [];
            foreach (var item in allInvitations)
            {
                if (int.TryParse(item.getGiftFrom(), out var d))
                {
                    list.Add(d);
                }
            }

            return LoadWeddingInfoFromRemote(list.ToArray());
        }

        public List<WeddingInfo> LoadWeddingInfoFromRemote(params int[] weddingId)
        {
            var request = new LoadWeddingByIdRequest();
            request.Id.AddRange(weddingId);
            return _mapper.Map<List<WeddingInfo>>(_transport.LoadAllWeddingById(request));
        }

        public WeddingInfo? GetPlayerWeddingInfoFromAll(IPlayer chr)
        {
            var marriageInfo = _marriageManager.GetPlayerMarriageInfo(chr.Id);
            if (marriageInfo == null)
                return null;

            return LoadWeddingInfoFromRemote(marriageInfo.Id).FirstOrDefault();
        }

        /// <summary>
        /// 婚礼完成，发放戒指、
        /// </summary>
        /// <param name="player"></param>
        /// <param name="partner"></param>
        /// <param name="marriageRingItemId"></param>
        public void CompleteWedding(IPlayer player, IPlayer partner, int marriageRingItemId)
        {
            var marriageInfo = _marriageManager.GetPlayerMarriageInfo(player.Id);
            if (marriageInfo == null)
                return;

            var res = _transport.CompleteWedding(new CompleteWeddingRequest { MarriageId = marriageInfo.Id, MarriageItemId = marriageRingItemId });
            var ringSource = new RingSourceModel()
            {
                Id = res.RingSourceId,
                CharacterId1 = res.GroomId,
                CharacterName1 = res.GroomName,
                RingId1 = res.GroomRingId,
                CharacterId2 = res.BrideId,
                CharacterName2 = res.BrideName,
                RingId2 = res.BrideRingId,
                ItemId = res.MarriageItemId
            };
            _marriageManager.RemoveLocalData(marriageInfo);
            var ii = ItemInformationProvider.getInstance();

            Item ringObj = ii.getEquipById(marriageRingItemId);
            Equip ringEqp = (Equip)ringObj;
            ringEqp.SetRing(ringSource.RingId1, ringSource);
            player.addMarriageRing(ringEqp.Ring);
            InventoryManipulator.addFromDrop(player.Client, ringEqp, false);
            player.broadcastMarriageMessage();

            ringObj = ii.getEquipById(marriageRingItemId);
            ringEqp = (Equip)ringObj;
            ringEqp.SetRing(ringSource.RingId2, ringSource);
            partner.addMarriageRing(ringEqp.Ring);
            InventoryManipulator.addFromDrop(partner.Client, ringEqp, false);
            partner.broadcastMarriageMessage();
        }

        public void sendMarriageWishlist(IPlayer player, bool groom)
        {
            var marriage = GetMarriageInstance(player);
            if (marriage != null)
            {
                int cid = marriage.getIntProperty(groom ? "groomId" : "brideId");
                var chr = marriage.getPlayerById(cid);
                if (chr != null)
                {
                    if (chr.getId() == player.getId())
                    {
                        player.sendPacket(WeddingPackets.onWeddingGiftResult(0xA, marriage.getWishlistItems(groom), marriage.getGiftItems(player.Client, groom)));
                    }
                    else
                    {
                        marriage.setIntProperty("wishlistSelection", groom ? 0 : 1);
                        player.sendPacket(WeddingPackets.onWeddingGiftResult(0x09, marriage.getWishlistItems(groom), marriage.getGiftItems(player.Client, groom)));
                    }
                }
            }
        }

        public void sendMarriageGifts(IPlayer player, List<Item> gifts)
        {
            player.sendPacket(WeddingPackets.onWeddingGiftResult(0xA, Collections.singletonList(""), gifts));
        }

        public bool createMarriageWishlist(IPlayer player)
        {
            var marriage = GetMarriageInstance(player);
            if (marriage != null)
            {
                var groom = marriage.isMarriageGroom(player);
                if (groom != null)
                {
                    string wlKey;
                    if (groom.Value)
                    {
                        wlKey = "groomWishlist";
                    }
                    else
                    {
                        wlKey = "brideWishlist";
                    }

                    if (string.IsNullOrEmpty(marriage.getProperty(wlKey)))
                    {
                        player.sendPacket(WeddingPackets.sendWishList());
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
