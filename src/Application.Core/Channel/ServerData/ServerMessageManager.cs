using tools;

namespace Application.Core.Channel.ServerData
{
    public class ServerMessageManager : TaskBase
    {
        private Dictionary<int, int> disabledServerMessages = new();
        private Lock srvMessagesLock = new ();

        readonly WorldChannelServer _server;

        public ServerMessageManager(WorldChannelServer server) : base($"ChannelServer:{server.ServerName}_{nameof(ServerMessageManager)}", TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10))
        {
            this._server = server;
        }

        public void resetDisabledServerMessages()
        {
            srvMessagesLock.Enter();
            try
            {
                disabledServerMessages.Clear();
            }
            finally
            {
                srvMessagesLock.Exit();
            }
        }

        public bool registerDisabledServerMessage(int chrid)
        {
            srvMessagesLock.Enter();
            try
            {
                bool alreadyDisabled = disabledServerMessages.ContainsKey(chrid);
                disabledServerMessages.AddOrUpdate(chrid, 0);

                return alreadyDisabled;
            }
            finally
            {
                srvMessagesLock.Exit();
            }
        }

        public bool unregisterDisabledServerMessage(int chrid)
        {
            srvMessagesLock.Enter();
            try
            {
                return disabledServerMessages.Remove(chrid, out var d);
            }
            finally
            {
                srvMessagesLock.Exit();
            }
        }

        protected override void HandleRun()
        {
            List<int> toRemove = new();

            srvMessagesLock.Enter();
            try
            {
                foreach (var dsm in disabledServerMessages)
                {
                    int b = dsm.Value;
                    if (b >= 4)
                    {
                        // ~35sec duration, 10sec update
                        toRemove.Add(dsm.Key);
                    }
                    else
                    {
                        disabledServerMessages[dsm.Key] = ++b;
                    }
                }

                foreach (int chrid in toRemove)
                {
                    disabledServerMessages.Remove(chrid);
                }
            }
            finally
            {
                srvMessagesLock.Exit();
            }

            foreach (int chrid in toRemove)
            {
                var chr = _server.FindPlayerById(chrid);

                if (chr != null && chr.isLoggedinWorld())
                {
                    chr.sendPacket(PacketCreator.serverMessage(_server.WorldServerMessage));
                }
            }
        }

    }
}
