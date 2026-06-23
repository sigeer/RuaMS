using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Item;
using Application.Shared.Net;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Module.Maker.Channel.Net.Handlers
{
    public class MakerSkillHandler : ChannelHandlerBase
    {
        readonly MakerManager _service;
        readonly ILogger<MakerSkillHandler> _logger;
        public MakerSkillHandler(MakerManager service, ILogger<MakerSkillHandler> logger)
        {
            _service = service;
            _logger = logger;
        }

        public override async Task HandlePacket(InPacket p, IChannelClient c)
        {
            {
                await c.tryacquireClient();
                try
                {
                    int type = p.readInt();
                    int toCreate = p.readInt();
                    int toDisassemble = -1, pos = -1;
                    bool makerSucceeded = true;

                    MakerItemCreateEntry? recipe = null;
                    Dictionary<int, short> reagentids = new();
                    int stimulantid = -1;

                    if (type == 3)
                    {
                        // building monster crystal
                        int fromLeftover = toCreate;
                        toCreate = _service.getMakerCrystalFromLeftover(toCreate);
                        if (toCreate == -1)
                        {
                            await c.SendPacket(PacketCreator.serverNotice(1, c.CurrentCulture.GetItemName(fromLeftover) + " is unavailable for Monster Crystal conversion."));
                            await c.SendPacket(MakerPacketCreator.makerEnableActions());
                            return;
                        }

                        recipe = _service.generateLeftoverCrystalEntry(fromLeftover, toCreate);
                    }
                    else if (type == 4)
                    {  // disassembling
                        p.readInt(); // 1... probably inventory type
                        pos = p.readInt();

                        var it = c.OnlinedCharacter.getInventory(InventoryType.EQUIP).getItem((short)pos);
                        if (it != null && it.getItemId() == toCreate)
                        {
                            toDisassemble = toCreate;

                            var pair = _service.generateDisassemblyInfo(toDisassemble);
                            if (pair != null)
                            {
                                recipe = _service.generateDisassemblyCrystalEntry(toDisassemble, pair.Value.Key, pair.Value.Value);
                            }
                            else
                            {
                                await c.SendPacket(PacketCreator.serverNotice(1, c.CurrentCulture.GetItemName(toCreate) + " is unavailable for Monster Crystal disassembly."));
                                await c.SendPacket(MakerPacketCreator.makerEnableActions());
                                return;
                            }
                        }
                        else
                        {
                            await c.SendPacket(PacketCreator.serverNotice(1, "An unknown error occurred when trying to apply that item for disassembly."));
                            await c.SendPacket(MakerPacketCreator.makerEnableActions());
                            return;
                        }
                    }
                    else
                    {
                        if (ItemConstants.isEquipment(toCreate))
                        {   // only equips uses stimulant and reagents
                            if (p.readByte() != 0)
                            {
                                // stimulant
                                stimulantid = _service.getMakerStimulant(toCreate);
                                if (!c.OnlinedCharacter.getAbstractPlayerInteraction().haveItem(stimulantid))
                                {
                                    stimulantid = -1;
                                }
                            }

                            int reagents = Math.Min(p.readInt(), _service.getMakerReagentSlots(toCreate));
                            for (int i = 0; i < reagents; i++)
                            {
                                // crystals
                                int reagentid = p.readInt();
                                if (ItemConstants.isMakerReagent(reagentid))
                                {
                                    var rs = reagentids.GetValueOrDefault(reagentid);
                                    reagentids[reagentid] = (short)(rs + 1);
                                }
                            }

                            List<KeyValuePair<int, short>> toUpdate = new();
                            foreach (var r in reagentids)
                            {
                                int qty = c.OnlinedCharacter.getItemQuantity(r.Key);

                                if (qty < r.Value)
                                {
                                    toUpdate.Add(new(r.Key, (short)qty));
                                }
                            }

                            // remove those not present on player inventory
                            if (toUpdate.Count > 0)
                            {
                                foreach (var rp in toUpdate)
                                {
                                    if (rp.Value > 0)
                                    {
                                        reagentids[rp.Key] = rp.Value;
                                    }
                                    else
                                    {
                                        reagentids.Remove(rp.Key);
                                    }
                                }
                            }

                            if (reagentids.Count > 0)
                            {
                                if (!_service.removeOddMakerReagents(toCreate, reagentids))
                                {
                                    await c.SendPacket(PacketCreator.serverNotice(1, "You can only use WATK and MATK Strengthening Gems on weapon items."));
                                    await c.SendPacket(MakerPacketCreator.makerEnableActions());
                                    return;
                                }
                            }
                        }

                        recipe = _service.getItemCreateEntry(toCreate, stimulantid, reagentids);
                    }

                    short createStatus = await _service.getCreateStatus(c, recipe);

                    switch (createStatus)
                    {
                        case -1:// non-available for Maker itemid has been tried to forge
                            _logger.LogWarning("Chr {CharacterName} tried to craft itemid {ItemId} using the Maker skill.", c.OnlinedCharacter.getName(), toCreate);
                            await c.SendPacket(PacketCreator.serverNotice(1, "The requested item could not be crafted on this operation."));
                            await c.SendPacket(MakerPacketCreator.makerEnableActions());
                            break;

                        case 1: // no items
                            await c.SendPacket(PacketCreator.serverNotice(1, "You don't have all required items in your inventory to make " + c.CurrentCulture.GetItemName(toCreate) + "."));
                            await c.SendPacket(MakerPacketCreator.makerEnableActions());
                            break;

                        case 2: // no meso
                            await c.SendPacket(PacketCreator.serverNotice(1, "You don't have enough mesos (" + c.CurrentCulture.Number(recipe!.getCost()) + ") to complete this operation."));
                            await c.SendPacket(MakerPacketCreator.makerEnableActions());
                            break;

                        case 3: // no req level
                            await c.SendPacket(PacketCreator.serverNotice(1, "You don't have enough level to complete this operation."));
                            await c.SendPacket(MakerPacketCreator.makerEnableActions());
                            break;

                        case 4: // no req skill level
                            await c.SendPacket(PacketCreator.serverNotice(1, "You don't have enough Maker level to complete this operation."));
                            await c.SendPacket(MakerPacketCreator.makerEnableActions());
                            break;

                        case 5: // inventory full
                            await c.SendPacket(PacketCreator.serverNotice(1, "Your inventory is full."));
                            await c.SendPacket(MakerPacketCreator.makerEnableActions());
                            break;

                        default:
                            if (toDisassemble != -1)
                            {
                                await InventoryManipulator.removeFromSlot(c, InventoryType.EQUIP, (short)pos, 1, false);
                            }
                            else
                            {
                                foreach (var pair in recipe!.getReqItems())
                                {
                                    await c.OnlinedCharacter.GainItem(pair.ItemId, (short)-pair.Quantity);
                                }
                            }

                            int cost = recipe!.getCost();
                            if (stimulantid == -1 && reagentids.Count == 0)
                            {
                                if (cost > 0)
                                {
                                    await c.OnlinedCharacter.GainMeso(-cost);
                                }

                                foreach (var pair in recipe.getGainItems())
                                {
                                    c.OnlinedCharacter.setCS(true);
                                    await c.OnlinedCharacter.GainItem(pair.ItemId, (short)pair.Quantity);
                                    c.OnlinedCharacter.setCS(false);
                                }
                            }
                            else
                            {
                                toCreate = recipe.getGainItems()[0].ItemId;

                                if (stimulantid != -1)
                                {
                                    await c.OnlinedCharacter.GainItem(stimulantid, -1);
                                }
                                if (reagentids.Count > 0)
                                {
                                    foreach (var r in reagentids)
                                    {
                                        await c.OnlinedCharacter.GainItem(r.Key, (short)(-1 * r.Value));
                                    }
                                }

                                if (cost > 0)
                                {
                                    await c.OnlinedCharacter.GainMeso(-cost);
                                }
                                makerSucceeded = await _service.addBoostedMakerItem(c, toCreate, stimulantid, reagentids);
                            }

                            // thanks inhyuk for noticing missing MAKER_RESULT packets
                            if (type == 3)
                            {
                                await c.SendPacket(MakerPacketCreator.makerResultCrystal(recipe.getGainItems()[0].ItemId, recipe.getReqItems()[0].ItemId));
                            }
                            else if (type == 4)
                            {
                                await c.SendPacket(MakerPacketCreator.makerResultDesynth(recipe.getReqItems()[0].ItemId, recipe.getCost(), recipe.getGainItems()));
                            }
                            else
                            {
                                await c.SendPacket(
                                    MakerPacketCreator.makerResult(
                                        makerSucceeded,
                                        recipe.getGainItems()[0].ItemId,
                                        recipe.getGainItems()[0].Quantity,
                                        recipe.getCost(),
                                        recipe.getReqItems(),
                                        stimulantid,
                                        reagentids.Keys.ToList()));
                            }

                            await c.SendPacket(PacketCreator.showMakerEffect(makerSucceeded));
                            await c.OnlinedCharacter.getMap().broadcastMessage(c.OnlinedCharacter, PacketCreator.showForeignMakerEffect(c.OnlinedCharacter.getId(), makerSucceeded), false);

                            if (toCreate == 4260003 && type == 3 && c.OnlinedCharacter.getQuestStatus(6033) == 1)
                            {
                                await c.OnlinedCharacter.getAbstractPlayerInteraction().setQuestProgress(6033, 1);
                            }
                            break;
                    }
                }
                finally
                {
                    c.releaseClient();
                }
            }
        }
    }
}
