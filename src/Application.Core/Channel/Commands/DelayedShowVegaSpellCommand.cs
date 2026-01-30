using client.inventory;
using tools;
using static client.inventory.Equip;

namespace Application.Core.Channel.Commands
{
    internal class DelayedShowVegaSpellCommand : IWorldChannelCommand
    {
        Player _player;
        Equip _scrolled;
        int _oldLevel;

        public DelayedShowVegaSpellCommand(Player player, Equip scrolled, int oldLevel)
        {
            _player = player;
            _scrolled = scrolled;
            _oldLevel = oldLevel;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (!_player.isLoggedin())
            {
                return;
            }

            _player.toggleBlockCashShop();

            List<ModifyInventory> mods = new();
            mods.Add(new ModifyInventory(3, _scrolled));
            mods.Add(new ModifyInventory(0, _scrolled));
            _player.sendPacket(PacketCreator.modifyInventory(true, mods));

            var scrollResult = _scrolled.getLevel() > _oldLevel ? ScrollResult.SUCCESS : ScrollResult.FAIL;
            _player.getMap().broadcastMessage(PacketCreator.getScrollEffect(_player.Id, scrollResult, false, false));
            // 取背包装备栏而不是已装备栏，理论上不会出现eSlot < 0的情况？
            //if (eSlot < 0 && (scrollResult == ScrollResult.SUCCESS))
            //{
            //    _player.equipChanged();
            //}

            _player.sendPacket(PacketCreator.enableActions());
            return;
        }
    }
}
