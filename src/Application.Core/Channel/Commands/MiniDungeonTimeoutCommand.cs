using server.maps;

namespace Application.Core.Channel.Commands
{
    internal class MiniDungeonTimeoutCommand : IWorldChannelCommand
    {
        MiniDungeon _dungeon;

        public MiniDungeonTimeoutCommand(MiniDungeon dungeon)
        {
            _dungeon = dungeon;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _dungeon.ProcessTimeout();
        }
    }
}
