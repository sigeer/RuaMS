using Application.Core.Game.Invites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Channel.Tasks
{
    public class InvitationTask : AbstractRunnable
    {
        readonly WorldChannelServer _server;

        public InvitationTask(WorldChannelServer server)
        {
            _server = server;
        }

        public override void HandleRun()
        {
            InviteType.TRADE.CheckExpired(_server.getCurrentTime());
        }
    }
}
