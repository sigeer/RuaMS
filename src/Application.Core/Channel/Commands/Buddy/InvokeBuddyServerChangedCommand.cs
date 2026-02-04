using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeBuddyPacketCommand : IWorldChannelCommand
    {
        int id;
        IEnumerable<int> _ids;
        Packet p;

        public InvokeBuddyPacketCommand(int id, IEnumerable<int> ids, Packet p)
        {
            this.id = id;
            _ids = ids;
            this.p = p;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            foreach (var buddy in _ids)
            {
                var buddyChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(buddy);
                if (buddyChr != null && buddyChr.BuddyList.Contains(id))
                {
                    buddyChr.sendPacket(p);
                }
            }
            return;
        }
    }
}
