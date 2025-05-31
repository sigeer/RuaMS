using Application.Core.Duey;
using Application.Core.Game.Trades;
using Application.Core.Model;
using Application.Core.ServerTransports;
using Application.Shared.Items;
using AutoMapper;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using client.processor.npc;
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

        public ItemService(IMapper mapper, IChannelServerTransport transport, ILogger<ItemService> logger)
        {
            _mapper = mapper;
            _transport = transport;
            _logger = logger;
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

        public bool CreateDueyPackage(int id, int sendMesos, Item? item, string? sendMessage, int recipient, bool quick)
        {
            return _transport.CreateDueyPackage(new Dto.CreatePackageRequest
            {
                SenderId = id,
                SendMeso = sendMesos,
                SendMessage = sendMessage,
                ReceiverId = recipient,
                Quick = quick,
                Item = _mapper.Map<Dto.ItemDto>(item)
            }).IsSuccess;
        }


        public Dto.CreatePackageCheckResponse CreateDueyPackageCheck(int senderId, string recipient)
        {
            return _transport.CreateDueyPackageFromInventoryCheck(new Dto.CreatePackageCheckRequest { SenderId = senderId, ReceiverName = recipient } );
        }

        internal void SendDueyNotification(string recipient)
        {
            _transport.SendDueyNotification(new Dto.SendDueyNotificationRequest() { CharacterName = recipient });
        }

        private (int, Item?) RemoveFromInventoryForDuey(IChannelClient c, sbyte invTypeId, short itemPos, short amount)
        {
            if (invTypeId > 0)
            {
                ItemInformationProvider ii = ItemInformationProvider.getInstance();

                InventoryType invType = InventoryTypeUtils.getByType(invTypeId);
                Inventory inv = c.OnlinedCharacter.getInventory(invType);

                Item? item;
                inv.lockInventory();
                try
                {
                    item = inv.getItem(itemPos);
                    if (item != null && item.getQuantity() >= amount)
                    {
                        if (item.isUntradeable() || ii.isUnmerchable(item.getItemId()))
                        {
                            return (-1, null);
                        }

                        if (ItemConstants.isRechargeable(item.getItemId()))
                        {
                            InventoryManipulator.removeFromSlot(c, invType, itemPos, item.getQuantity(), true);
                        }
                        else
                        {
                            InventoryManipulator.removeFromSlot(c, invType, itemPos, amount, true, false);
                        }

                        item = item.copy();
                    }
                    else
                    {
                        return (-2, null);
                    }
                }
                finally
                {
                    inv.unlockInventory();
                }

                KarmaManipulator.toggleKarmaFlagToUntradeable(item);
                item.setQuantity(amount);

                return (0, item);
                //if (!insertPackageItem(packageId, item))
                //{
                //    return 1;
                //}
            }

            _logger.LogError("装备栏的道具不能直接快递");
            return (-1, null);
        }

        public void DueySendItemFromInventory(IChannelClient c, sbyte invTypeId, short itemPos, short amount, int sendMesos, string? sendMessage, string recipient, bool quick)
        {
            if (c.tryacquireClient())
            {
                try
                {
                    if (c.OnlinedCharacter.isGM() && c.OnlinedCharacter.gmLevel() < YamlConfig.config.server.MINIMUM_GM_LEVEL_TO_USE_DUEY)
                    {
                        c.OnlinedCharacter.message("You cannot use Duey to send items at your GM level.");
                        _logger.LogWarning("GM {GM} tried to send a namespace to {Recipient}", c.OnlinedCharacter.getName(), recipient);
                        c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_INCORRECT_REQUEST.getCode()));
                        return;
                    }

                    if (sendMessage != null && sendMessage.Length > 100)
                    {
                        AutobanFactory.PACKET_EDIT.alert(c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit with Quick Delivery on duey.");
                        _logger.LogWarning("Chr {CharacterName} tried to use duey with too long of a text", c.OnlinedCharacter.getName());
                        c.Disconnect(true, false);
                        return;
                    }

                    int fee = TradeManager.GetFee(sendMesos);
                    if (!quick)
                    {
                        fee += 5000;
                    }
                    else if (!c.OnlinedCharacter.haveItem(ItemId.QUICK_DELIVERY_TICKET))
                    {
                        AutobanFactory.PACKET_EDIT.alert(c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit with Quick Delivery on duey.");
                        _logger.LogWarning("Chr {CharacterName} tried to use duey with Quick Delivery without a ticket, mesos {Meso} and amount {Amount}", c.OnlinedCharacter.getName(), sendMesos, amount);
                        c.Disconnect(true, false);
                        return;
                    }

                    long finalcost = (long)sendMesos + fee;
                    if (finalcost < 0 || finalcost > int.MaxValue || (amount < 1 && sendMesos == 0))
                    {
                        AutobanFactory.PACKET_EDIT.alert(c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit with duey.");
                        _logger.LogWarning("Chr {CharacterName} tried to use duey with mesos {Meso} and amount {Amount}", c.OnlinedCharacter.getName(), sendMesos, amount);
                        c.Disconnect(true, false);
                        return;
                    }

                    if (c.OnlinedCharacter.getMeso() < finalcost)
                    {
                        c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_NOT_ENOUGH_MESOS.getCode()));
                        return;
                    }

                    var checkResult = c.CurrentServer.ItemService.CreateDueyPackageCheck(c.OnlinedCharacter.Id, recipient);

                    if (checkResult.Code == (int)SendDueyItemResponseCode.CharacterNotExisted)
                    {
                        c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_NAME_DOES_NOT_EXIST.getCode()));
                        return;
                    }

                    if (checkResult.Code == (int)SendDueyItemResponseCode.SameAccount)
                    {
                        c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_SAMEACC_ERROR.getCode()));
                        return;
                    }

                    if (checkResult.Code != (int)SendDueyItemResponseCode.Success)
                    {
                        c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_ENABLE_ACTIONS.getCode()));
                        return;
                    }

                    c.OnlinedCharacter.gainMeso((int)-finalcost, false);
                    if (quick)
                    {
                        InventoryManipulator.removeById(c, InventoryType.CASH, ItemId.QUICK_DELIVERY_TICKET, 1, false, false);
                    }

                    var (res, item) = RemoveFromInventoryForDuey(c, invTypeId, itemPos, amount);
                    if (res == 0)
                    {
                        c.CurrentServer.ItemService.CreateDueyPackage(c.OnlinedCharacter.Id, sendMesos, item, sendMessage, checkResult.ReceiverId, quick);
                        c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_SUCCESSFULLY_SENT.getCode()));
                    }
                    else if (res > 0)
                    {
                        c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_ENABLE_ACTIONS.getCode()));
                    }
                    else
                    {
                        c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_INCORRECT_REQUEST.getCode()));
                    }

                    c.CurrentServer.ItemService.SendDueyNotification(recipient);
                }
                finally
                {
                    c.releaseClient();
                }
            }
        }
    }
}
