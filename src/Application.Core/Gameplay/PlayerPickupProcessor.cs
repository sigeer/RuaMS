using Application.Core.Game.Maps;
using Application.Core.Managers;
using client.inventory.manipulator;
using scripting.item;
using server;
using tools;

namespace Application.Core.Gameplay
{
    public class PlayerPickupProcessor : PlayerProcessor<MapItem>
    {

        int _petIndex = -1;
        bool IsPetPickup => _petIndex > -1;
        public PickupCheckFlags Flags { get; set; }
        public PlayerPickupProcessor(IPlayer player, int petIdex = -1) : base(player)
        {
            _petIndex = petIdex;
            Flags = PickupCheckFlags.CoolDown | PickupCheckFlags.Owner;
        }

        protected virtual Packet GetPickupPacket(MapItem mapItem) => PacketCreator.removeItemFromMap(mapItem.getObjectId(), IsPetPickup ? 5 : 2, _player.Id, IsPetPickup, _petIndex);

        protected override void Process(MapItem mapItem)
        {
            if (PickupMeso(mapItem) || PickupSpecialItem(mapItem) || PickupItem(mapItem))
            {
                _player.MapModel.pickItemDrop(GetPickupPacket(mapItem), mapItem);
            }
            _player.sendPacket(PacketCreator.enableActions());
        }

        protected override bool Before(MapItem mapItem)
        {
            if (IsPetPickup)
            {
                if (mapItem.getMeso() > 0)
                {
                    if (!_player.isEquippedMesoMagnet())
                    {
                        _player.sendPacket(PacketCreator.enableActions());
                        return false;
                    }

                    if (_player.isEquippedPetItemIgnore())
                    {
                        HashSet<int> petIgnore = _player.getExcludedItems();
                        if (petIgnore.Count > 0 && petIgnore.Contains(int.MaxValue))
                        {
                            _player.sendPacket(PacketCreator.enableActions());
                            return false;
                        }
                    }
                }
                else
                {
                    if (!_player.isEquippedItemPouch())
                    {
                        _player.sendPacket(PacketCreator.enableActions());
                        return false;
                    }

                    if (_player.isEquippedPetItemIgnore())
                    {
                        HashSet<int> petIgnore = _player.getExcludedItems();
                        if (petIgnore.Count > 0 && petIgnore.Contains(mapItem.getItemId()))
                        {
                            _player.sendPacket(PacketCreator.enableActions());
                            return false;
                        }
                    }
                }
            }

            // 掉落物捡取时间限制
            if (Flags.HasFlag(PickupCheckFlags.CoolDown) && (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - mapItem.getDropTime() < 400))
            {
                _player.sendPacket(PacketCreator.enableActions());
                return false;
            }

            // 所有权限制
            if (Flags.HasFlag(PickupCheckFlags.Owner) && !mapItem.canBePickedBy(_player))
            {
                _player.sendPacket(PacketCreator.enableActions());
                return false;
            }

            // 已经被捡取
            if (mapItem.isPickedUp())
            {
                _player.sendPacket(PacketCreator.showItemUnavailable());
                _player.sendPacket(PacketCreator.enableActions());
                return false;
            }

            // 道具需要空间但是空间不足
            if (mapItem.NeedCheckSpace && !InventoryManipulator.checkSpace(_player.Client, mapItem.getItemId(), mapItem.getItem().getQuantity(), mapItem.getItem().getOwner()))
            {
                _player.sendPacket(PacketCreator.getInventoryFull());
                _player.sendPacket(PacketCreator.getShowInventoryFull());
                _player.sendPacket(PacketCreator.enableActions());
                return false;
            }

            // 未知
            if (MapId.isSelfLootableOnly(_player.getMapId()))
            {
                // 其他人丢弃的道具
                if (mapItem.isPlayerDrop() && mapItem.getDropper().getObjectId() != _player.getObjectId())
                {
                    _player.sendPacket(PacketCreator.showItemUnavailable());
                    _player.sendPacket(PacketCreator.enableActions());
                    return false;
                }

                // 有物品脚本 或者 会被使用的特殊道具
                if (ItemId.HasScript(mapItem.getItemId()) || (mapItem.getItemId() / 1000000 == 2 && ItemInformationProvider.getInstance().isConsumeOnPickup(mapItem.getItemId())))
                {
                    _player.sendPacket(PacketCreator.enableActions());
                    return false;
                }
            }

            // 任务道具 已捡取足够 不需要继续捡取
            if (!_player.needQuestItem(mapItem.getQuest(), mapItem.getItemId()))
            {
                _player.sendPacket(PacketCreator.showItemUnavailable());
                _player.sendPacket(PacketCreator.enableActions());
                return false;
            }

            return true;
        }

        public override void Handle(MapItem? mapItem)
        {
            if (mapItem == null)
                return;

            mapItem.lockItem();
            try
            {
                base.Handle(mapItem);
            }
            finally
            {
                mapItem.unlockItem();
            }

        }

        private bool PickupMeso(MapItem mapItem)
        {
            var meso = mapItem.getMeso();
            if (meso > 0)
            {
                var mpcs = _player.getPartyMembersOnSameMap();
                if (mpcs.Count > 0)
                {
                    int mesosamm = mapItem.getMeso() / mpcs.Count;
                    foreach (IPlayer partymem in mpcs)
                    {
                        partymem.gainMeso(mesosamm, true, true, false);
                    }
                }
                else
                {
                    _player.gainMeso(meso, true, true, false);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// true：捡取完成，中断
        /// </summary>
        /// <param name="mapItem"></param>
        /// <returns></returns>
        private bool PickupSpecialItem(MapItem mapItem)
        {
            if (ItemId.isNxCard(mapItem.getItemId()))
            {
                // Add NX to account, show effect and make item disappear
                int nxGain = mapItem.getItemId() == ItemId.NX_CARD_100 ? 100 : 250;
                _player.getCashShop().gainCash(1, nxGain);

                if (YamlConfig.config.server.USE_ANNOUNCE_NX_COUPON_LOOT)
                {
                    _player.showHint("You have earned #e#b" + nxGain + " NX#k#n. (" + _player.getCashShop().getCash(CashShop.NX_CREDIT) + " NX)", 300);
                }
                return true;
            }

            else if (ItemId.HasScript(mapItem.getItemId()))
            {
                var info = ItemInformationProvider.getInstance().getScriptedItemInfo(mapItem.getItemId());
                if (info != null && info.runOnPickup())
                {
                    ItemScriptManager.getInstance().runItemScript(_player.Client, info);
                    return true;
                }
                else
                    return false;
            }

            else if (ItemId.isPet(mapItem.getItemId()))
            {
                InventoryManipulator.addById(_player.Client, mapItem.getItem().getItemId(), mapItem.getItem().getQuantity(), null);
                return true;
            }

            else
                return _player.applyConsumeOnPickup(mapItem.getItemId());
        }

        private bool PickupItem(MapItem mapItem)
        {
            if (InventoryManipulator.addFromDrop(_player.Client, mapItem.getItem(), true))
            {
                if (mapItem.getItemId() == ItemId.ARPQ_SPIRIT_JEWEL)
                {
                    _player.updateAriantScore();
                }
                return true;
            }
            return false;
        }
    }
}
