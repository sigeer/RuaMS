using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeBuddyPacketCommand : IWorldChannelCommand
    {
        public string Name => nameof(InvokeBuddyPacketCommand);
        int id;
        IEnumerable<int> _ids;
        Packet p;

        public InvokeBuddyPacketCommand(int id, IEnumerable<int> ids, Packet p)
        {
            this.id = id;
            _ids = ids;
            this.p = p;
        }

        public void Execute(WorldChannel ctx)
        {
            foreach (var buddy in _ids)
            {
                var buddyChrActor = ctx.getPlayerStorage().GetCharacterActor(buddy);
                buddyChrActor?.Send(m =>
                {
                    var buddyChr = m.getCharacterById(buddy);
                    if (buddyChr != null && buddyChr.BuddyList.Contains(id))
                    {
                        buddyChr.sendPacket(p);
                    }
                });

            }
            return;
        }
    }
}
