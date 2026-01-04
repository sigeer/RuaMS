using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Net.Packets;
using Application.Core.Channel.ResourceTransaction;
using Application.Core.Channel.Services;
using Application.Core.Game.Trades;
using AutoMapper;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using DueyDto;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using tools;

namespace Application.Core.Channel.DueyService
{
    public class DueyManager
    {
        readonly IMapper _mapper;
        readonly ILogger<DueyManager> _logger;
        readonly WorldChannelServer _server;
        readonly IItemDistributeService _distributeService;

        public DueyManager(
            WorldChannelServer server, IMapper mapper, ILogger<DueyManager> logger, IItemDistributeService distributeService)
        {
            _server = server;
            _mapper = mapper;
            _logger = logger;
            _distributeService = distributeService;
        }

        private async Task CreateDueyPackage(Player chr, int costMeso, int sendMesos, Item? item, string? sendMessage, string recipient, bool quick)
        {
            if (chr.TscRequest != null)
            {
                return;
            }

            List<Item> items = [];
            if (quick)
            {
                items.Add(new Item(ItemId.QUICK_DELIVERY_TICKET, 0, 1));
            }

            var builder = new ResourceConsumeBuilder().ConsumeMeso(costMeso);
            if (quick)
                builder.ConsumeItem(ItemId.QUICK_DELIVERY_TICKET, 1);

            chr.TscRequest = builder.Build();
            await _server.Transport.CreateDueyPackage(new DueyDto.CreatePackageRequest
            {
                SenderId = chr.Id,
                SendMeso = sendMesos,
                SendMessage = sendMessage,
                ReceiverName = recipient,
                Quick = quick,
                Item = _mapper.Map<Dto.ItemDto>(item),
            });
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

        public async Task DueySendItemFromInventory(IChannelClient c, sbyte invTypeId, short itemPos, short amount, int sendMesos, string? sendMessage, string recipient, bool quick)
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
                        await _server.AutoBanManager.Alert(AutobanFactory.PACKET_EDIT, c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit with Quick Delivery on duey.");
                        _logger.LogWarning("Chr {CharacterName} tried to use duey with too long of a text", c.OnlinedCharacter.getName());
                        await c.Disconnect(true, false);
                        return;
                    }

                    int fee = TradeManager.GetFee(sendMesos);
                    if (!quick)
                    {
                        fee += 5000;
                    }
                    else if (!c.OnlinedCharacter.haveItem(ItemId.QUICK_DELIVERY_TICKET))
                    {
                       await  _server.AutoBanManager.Alert(AutobanFactory.PACKET_EDIT, c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit with Quick Delivery on duey.");
                        _logger.LogWarning("Chr {CharacterName} tried to use duey with Quick Delivery without a ticket, mesos {Meso} and amount {Amount}", c.OnlinedCharacter.getName(), sendMesos, amount);
                        await c.Disconnect(true, false);
                        return;
                    }

                    long finalcost = (long)sendMesos + fee;
                    if (sendMesos < 0 || finalcost < 0 || finalcost > int.MaxValue || (amount < 1 && sendMesos == 0))
                    {
                       await  _server.AutoBanManager.Alert(AutobanFactory.PACKET_EDIT, c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit with duey.");
                        _logger.LogWarning("Chr {CharacterName} tried to use duey with mesos {Meso} and amount {Amount}", c.OnlinedCharacter.getName(), sendMesos, amount);
                        await c.Disconnect(true, false);
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
                        await CreateDueyPackage(c.OnlinedCharacter, (int)finalcost, sendMesos, item, sendMessage, recipient, quick);
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

        public async Task RemoveDueyPackage(Player chr, int packageId)
        {
            await _server.Transport.RequestRemovePackage(new DueyDto.RemovePackageRequest { MasterId = chr.Id, PackageId = packageId, ByReceived = false, });
        }

        public async Task TakePackage(Player chr, int packageId)
        {
            await _server.Transport.TakeDueyPackage(new DueyDto.TakeDueyPackageRequest { MasterId = chr.Id, PackageId = packageId });
        }

        public async Task SendTalk(IChannelClient c)
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

                    await _server.Transport.GetDueyPackagesByPlayerId(new GetPlayerDueyPackageRequest { ReceiverId = c.OnlinedCharacter.Id });
                }
                finally
                {
                    c.releaseClient();
                }
            }
        }
    }
}
