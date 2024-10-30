using Application.Core.Game.Maps;
using net.packet;
using tools;

namespace Application.Core.Gameplay
{
    public class PlayerPetPickupProcessor : PlayerPickupProcessor
    {
        int _petIndex = -1;
        bool IsPetPickup => _petIndex > -1;

        public PlayerPetPickupProcessor(IPlayer player, int petIndex) : base(player)
        {
            _petIndex = petIndex;
        }

        protected override Packet GetPickupPacket(MapItem mapItem)
        {
            return PacketCreator.removeItemFromMap(mapItem.getObjectId(), IsPetPickup ? 5 : 2, _player.Id, IsPetPickup, _petIndex);
        }
        protected override bool Before(MapItem mapItem)
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

            return base.Before(mapItem);
        }
    }
}
