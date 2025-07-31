using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Services;
using Application.Core.Client;
using Application.Core.Game.Players;
using Application.Core.Game.Trades;
using Application.Module.Duey.Channel.Models;
using Application.Module.Duey.Channel.Net;
using Application.Module.Duey.Common;
using Application.Shared.Constants;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Item;
using Application.Shared.Items;
using Application.Utility.Configs;
using AutoMapper;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using DueyDto;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Module.Duey.Channel
{
    public class DueyManager
    {
        readonly IMapper _mapper;
        readonly IChannelTransport _transport;
        readonly ILogger<DueyManager> _logger;
        readonly WorldChannelServer _server;
        readonly ItemTransactionService _itemStore;
        readonly IItemDistributeService _distributeService;

        public DueyManager(
            WorldChannelServer server, IMapper mapper, IChannelTransport transport, ILogger<DueyManager> logger, ItemTransactionService itemStore, IItemDistributeService distributeService)
        {
            _server = server;
            _mapper = mapper;
            _transport = transport;
            _logger = logger;
            _itemStore = itemStore;
            _distributeService = distributeService;
        }

        private void CreateDueyPackage(IPlayer chr, int costMeso, int sendMesos, Item? item, string? sendMessage, string recipient, bool quick)
        {
            List<Item> items = [];
            if (quick)
            {
                items.Add(new Item(ItemId.QUICK_DELIVERY_TICKET, 0, 1));
            }

            if(_itemStore.TryBeginTransaction(chr, items, costMeso, out var transaction))
            {
                _transport.CreateDueyPackage(new DueyDto.CreatePackageRequest
                {
                    SenderId = chr.Id,
                    SendMeso = sendMesos,
                    SendMessage = sendMessage,
                    ReceiverName = recipient,
                    Quick = quick,
                    Item = _mapper.Map<Dto.ItemDto>(item),
                    Transaction = transaction
                });
            }

        }

        public void OnDueyPackageCreated(DueyDto.CreatePackageResponse data)
        {
            if (data.Code == 0)
            {
                var chr = _server.FindPlayerById(data.Package.SenderId);
                if (chr != null)
                {
                    chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_SUCCESSFULLY_SENT.getCode()));
                }

                var receiver = _server.FindPlayerById(data.Package.ReceiverId);
                if (receiver != null)
                {
                    receiver.sendPacket(DueyPacketCreator.sendDueyParcelReceived(data.Package.SenderName, data.Package.Type));
                }
            }
            else
            {
                var sender = _server.FindPlayerById(data.Package.SenderId);
                if (sender != null)
                {
                    var dueyResponseCode = (SendDueyItemResponseCode)data.Code;
                    if (dueyResponseCode == SendDueyItemResponseCode.SameAccount)
                    {
                        sender.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_SAMEACC_ERROR.getCode()));
                    }
                    if (dueyResponseCode == SendDueyItemResponseCode.CharacterNotExisted)
                    {
                        sender.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_NAME_DOES_NOT_EXIST.getCode()));
                    }
                }

            }

            _itemStore.HandleTransaction(data.Transaction);
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
            // 仅发送金币
            return (0, null);
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
                        c.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_INCORRECT_REQUEST.getCode()));
                        return;
                    }

                    if (sendMessage != null && sendMessage.Length > 100)
                    {
                        _server.AutoBanManager.Alert(AutobanFactory.PACKET_EDIT, c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit with Quick Delivery on duey.");
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
                        _server.AutoBanManager.Alert(AutobanFactory.PACKET_EDIT, c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit with Quick Delivery on duey.");
                        _logger.LogWarning("Chr {CharacterName} tried to use duey with Quick Delivery without a ticket, mesos {Meso} and amount {Amount}", c.OnlinedCharacter.getName(), sendMesos, amount);
                        c.Disconnect(true, false);
                        return;
                    }

                    long finalcost = (long)sendMesos + fee;
                    if (sendMesos < 0 || finalcost < 0 || finalcost > int.MaxValue || (amount < 1 && sendMesos == 0))
                    {
                        _server.AutoBanManager.Alert(AutobanFactory.PACKET_EDIT, c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit with duey.");
                        _logger.LogWarning("Chr {CharacterName} tried to use duey with mesos {Meso} and amount {Amount}", c.OnlinedCharacter.getName(), sendMesos, amount);
                        c.Disconnect(true, false);
                        return;
                    }

                    if (c.OnlinedCharacter.getMeso() < finalcost)
                    {
                        c.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_NOT_ENOUGH_MESOS.getCode()));
                        return;
                    }

                    var (res, item) = RemoveFromInventoryForDuey(c, invTypeId, itemPos, amount);
                    if (res == 0)
                    {
                        CreateDueyPackage(c.OnlinedCharacter, (int)finalcost, sendMesos, item, sendMessage, recipient, quick);
                    }
                    else if (res > 0)
                    {
                        c.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_ENABLE_ACTIONS.getCode()));
                    }
                    else
                    {
                        c.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_INCORRECT_REQUEST.getCode()));
                    }
                }
                finally
                {
                    c.releaseClient();
                }
            }
        }

        public void RemoveDueyPackage(IPlayer chr, int packageId)
        {
            _transport.RequestRemovePackage(new DueyDto.RemovePackageRequest { MasterId = chr.Id, PackageId = packageId, ByReceived = false, });
        }

        public void OnDueyPackageRemoved(DueyDto.RemovePackageResponse data)
        {
            if (data.Code == 0)
            {
                var chr = _server.FindPlayerById(data.Request.MasterId);
                if (chr != null)
                    chr.sendPacket(DueyPacketCreator.removeItemFromDuey(!data.Request.ByReceived, data.Request.PackageId));
            }
        }

        public void TakePackage(IPlayer chr, int packageId)
        {
            _transport.TakeDueyPackage(new DueyDto.TakeDueyPackageRequest { MasterId = chr.Id, PackageId = packageId });
        }

        public void OnTakePackage(DueyDto.TakeDueyPackageResponse data)
        {
            var chr = _server.FindPlayerById(data.Package.ReceiverId);
            if (chr == null)
                return;

            if (data.Code == 0)
            {
                var dp = _mapper.Map<DueyPackageObject>(data.Package);

                var dpItem = dp.Item;

                if (!chr.canHoldMeso(dp.Mesos))
                {
                    chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_UNKNOWN_ERROR.getCode()));
                    _transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = false });
                    return;
                }

                if (dpItem != null)
                {
                    if (!chr.CanHoldUniquesOnly(dpItem.getItemId()))
                    {
                        chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_RECEIVER_WITH_UNIQUE.getCode()));
                        _transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = false });
                        return;
                    }

                    if (!chr.canHold(dpItem.getItemId(), dpItem.getQuantity()))
                    {
                        chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_NO_FREE_SLOTS.getCode()));
                        _transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = false });
                        return;
                    }
                }



                _transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = true});
                _distributeService.Distribute(chr, dpItem == null ? [] : [dpItem], dp.Mesos, 0, 0, "包裹满了");
            }
            else
            {
                chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_UNKNOWN_ERROR.getCode()));
                if (data.Code == 1)
                {
                    _logger.LogWarning("Chr {CharacterName} tried to receive package from duey with id {PackageId}", chr.Name, data.Request.PackageId);
                }
                if (data.Code == 2)
                {
                    _logger.LogWarning("Chr {CharacterName} tried to receive package from duey with receiverId {PackageId}", chr.Name, data.Request.PackageId);
                }
            }
        }

        public void SendTalk(IChannelClient c, bool quickDelivery)
        {
            if (c.tryacquireClient())
            {
                try
                {
                    long timeNow = c.CurrentServerContainer.getCurrentTime();
                    if (timeNow - c.OnlinedCharacter.getNpcCooldown() < YamlConfig.config.server.BLOCK_NPC_RACE_CONDT)
                    {
                        c.sendPacket(PacketCreator.enableActions());
                        return;
                    }
                    c.OnlinedCharacter.setNpcCooldown(timeNow);

                    if (quickDelivery)
                    {
                        c.sendPacket(DueyPacketCreator.sendDuey(0x1A, []));
                    }
                    else
                    {
                        c.sendPacket(DueyPacketCreator.sendDuey(DueyProcessorActions.TOCLIENT_OPEN_DUEY.getCode(), GetPlayerDueyPackages(c.OnlinedCharacter)));
                    }
                }
                finally
                {
                    c.releaseClient();
                }
            }
        }

        public DueyPackageObject[] GetPlayerDueyPackages(IPlayer chr)
        {
            return _mapper.Map<DueyPackageObject[]>(_transport.GetDueyPackagesByPlayerId(new GetPlayerDueyPackageRequest { ReceiverId = chr.Id }).List);
        }

        public void OnLoginDueyNotify(DueyDto.DueyNotifyDto data)
        {
            var receiver = _server.FindPlayerById(data.ReceiverId);
            if (receiver != null)
            {
                receiver.sendPacket(DueyPacketCreator.sendDueyParcelNotification(data.Type));
            }
        }
    }
}
