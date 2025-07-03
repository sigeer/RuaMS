using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Item;
using Application.Shared.Net;
using client.inventory.manipulator;
using constants.game;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Module.Maker.Channel.Net.Handlers
{
    public class MakerSkillHandler : ChannelHandlerBase
    {
        readonly MakerManager _service;
        readonly ItemInformationProvider ii;
        readonly ILogger<MakerSkillHandler> _logger;
        public MakerSkillHandler(MakerManager service, ItemInformationProvider itemInformationProvider, ILogger<MakerSkillHandler> logger)
        {
            _service = service;
            ii = itemInformationProvider;
            _logger = logger;
        }

        public override void HandlePacket(InPacket p, IChannelClient c)
        {
            if (c.tryacquireClient())
            {
                try
                {
                    int type = p.readInt();
                    int toCreate = p.readInt();
                    int toDisassemble = -1, pos = -1;
                    bool makerSucceeded = true;

                    MakerItemCreateEntry recipe;
                    Dictionary<int, short> reagentids = new();
                    int stimulantid = -1;

                    if (type == 3)
                    {   
                        // building monster crystal
                        int fromLeftover = toCreate;
                        toCreate = _service.getMakerCrystalFromLeftover(toCreate);
                        if (toCreate == -1)
                        {
                            c.sendPacket(PacketCreator.serverNotice(1, ii.getName(fromLeftover) + " is unavailable for Monster Crystal conversion."));
                            c.sendPacket(MakerPacketCreator.makerEnableActions());
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
                                c.sendPacket(PacketCreator.serverNotice(1, ii.getName(toCreate) + " is unavailable for Monster Crystal disassembly."));
                                c.sendPacket(MakerPacketCreator.makerEnableActions());
                                return;
                            }
                        }
                        else
                        {
                            c.sendPacket(PacketCreator.serverNotice(1, "An unknown error occurred when trying to apply that item for disassembly."));
                            c.sendPacket(MakerPacketCreator.makerEnableActions());
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
                                if (!c.getAbstractPlayerInteraction().haveItem(stimulantid))
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
                                int qty = c.getAbstractPlayerInteraction().getItemQuantity(r.Key);

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
                                    c.sendPacket(PacketCreator.serverNotice(1, "You can only use WATK and MATK Strengthening Gems on weapon items."));
                                    c.sendPacket(MakerPacketCreator.makerEnableActions());
                                    return;
                                }
                            }
                        }

                        recipe = _service.getItemCreateEntry(toCreate, stimulantid, reagentids);
                    }

                    short createStatus = _service.getCreateStatus(c, recipe);

                    switch (createStatus)
                    {
                        case -1:// non-available for Maker itemid has been tried to forge
                            _logger.LogWarning("Chr {CharacterName} tried to craft itemid {ItemId} using the Maker skill.", c.OnlinedCharacter.getName(), toCreate);
                            c.sendPacket(PacketCreator.serverNotice(1, "The requested item could not be crafted on this operation."));
                            c.sendPacket(MakerPacketCreator.makerEnableActions());
                            break;

                        case 1: // no items
                            c.sendPacket(PacketCreator.serverNotice(1, "You don't have all required items in your inventory to make " + ii.getName(toCreate) + "."));
                            c.sendPacket(MakerPacketCreator.makerEnableActions());
                            break;

                        case 2: // no meso
                            c.sendPacket(PacketCreator.serverNotice(1, "You don't have enough mesos (" + GameConstants.numberWithCommas(recipe.getCost()) + ") to complete this operation."));
                            c.sendPacket(MakerPacketCreator.makerEnableActions());
                            break;

                        case 3: // no req level
                            c.sendPacket(PacketCreator.serverNotice(1, "You don't have enough level to complete this operation."));
                            c.sendPacket(MakerPacketCreator.makerEnableActions());
                            break;

                        case 4: // no req skill level
                            c.sendPacket(PacketCreator.serverNotice(1, "You don't have enough Maker level to complete this operation."));
                            c.sendPacket(MakerPacketCreator.makerEnableActions());
                            break;

                        case 5: // inventory full
                            c.sendPacket(PacketCreator.serverNotice(1, "Your inventory is full."));
                            c.sendPacket(MakerPacketCreator.makerEnableActions());
                            break;

                        default:
                            if (toDisassemble != -1)
                            {
                                InventoryManipulator.removeFromSlot(c, InventoryType.EQUIP, (short)pos, 1, false);
                            }
                            else
                            {
                                foreach (var pair in recipe.getReqItems())
                                {
                                    c.getAbstractPlayerInteraction().gainItem(pair.ItemId, (short)-pair.Quantity, false);
                                }
                            }

                            int cost = recipe.getCost();
                            if (stimulantid == -1 && reagentids.Count == 0)
                            {
                                if (cost > 0)
                                {
                                    c.OnlinedCharacter.gainMeso(-cost, false);
                                }

                                foreach (var pair in recipe.getGainItems())
                                {
                                    c.OnlinedCharacter.setCS(true);
                                    c.getAbstractPlayerInteraction().gainItem(pair.ItemId, (short)pair.Quantity, false);
                                    c.OnlinedCharacter.setCS(false);
                                }
                            }
                            else
                            {
                                toCreate = recipe.getGainItems()[0].ItemId;

                                if (stimulantid != -1)
                                {
                                    c.getAbstractPlayerInteraction().gainItem(stimulantid, -1, false);
                                }
                                if (reagentids.Count > 0)
                                {
                                    foreach (var r in reagentids)
                                    {
                                        c.getAbstractPlayerInteraction().gainItem(r.Key, (short)(-1 * r.Value), false);
                                    }
                                }

                                if (cost > 0)
                                {
                                    c.OnlinedCharacter.gainMeso(-cost, false);
                                }
                                makerSucceeded = _service.addBoostedMakerItem(c, toCreate, stimulantid, reagentids);
                            }

                            // thanks inhyuk for noticing missing MAKER_RESULT packets
                            if (type == 3)
                            {
                                c.sendPacket(MakerPacketCreator.makerResultCrystal(recipe.getGainItems()[0].ItemId, recipe.getReqItems()[0].ItemId));
                            }
                            else if (type == 4)
                            {
                                c.sendPacket(MakerPacketCreator.makerResultDesynth(recipe.getReqItems()[0].ItemId, recipe.getCost(), recipe.getGainItems()));
                            }
                            else
                            {
                                c.sendPacket(
                                    MakerPacketCreator.makerResult(
                                        makerSucceeded, 
                                        recipe.getGainItems()[0].ItemId, 
                                        recipe.getGainItems()[0].Quantity, 
                                        recipe.getCost(), 
                                        recipe.getReqItems(), 
                                        stimulantid,
                                        reagentids.Keys.ToList()));
                            }

                            c.sendPacket(PacketCreator.showMakerEffect(makerSucceeded));
                            c.OnlinedCharacter.getMap().broadcastMessage(c.OnlinedCharacter, PacketCreator.showForeignMakerEffect(c.OnlinedCharacter.getId(), makerSucceeded), false);

                            if (toCreate == 4260003 && type == 3 && c.OnlinedCharacter.getQuestStatus(6033) == 1)
                            {
                                c.getAbstractPlayerInteraction().setQuestProgress(6033, 1);
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
