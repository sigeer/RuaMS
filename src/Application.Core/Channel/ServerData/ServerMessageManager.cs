using Application.Core.Game.Controllers;
using tools;

namespace Application.Core.Channel.ServerData
{
    public class ServerMessageManager : TimelyControllerBase
    {
        private Dictionary<int, int> disabledServerMessages = new();
        private object srvMessagesLock = new object();

        readonly WorldChannel worldChannel;

        public ServerMessageManager(WorldChannel worldChannel) : base($"ServerMessageController_{worldChannel.InstanceId}", TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10))
        {
            this.worldChannel = worldChannel;
        }

        public void resetDisabledServerMessages()
        {
            Monitor.Enter(srvMessagesLock);
            try
            {
                disabledServerMessages.Clear();
            }
            finally
            {
                Monitor.Exit(srvMessagesLock);
            }
        }

        public bool registerDisabledServerMessage(int chrid)
        {
            Monitor.Enter(srvMessagesLock);
            try
            {
                bool alreadyDisabled = disabledServerMessages.ContainsKey(chrid);
                disabledServerMessages.AddOrUpdate(chrid, 0);

                return alreadyDisabled;
            }
            finally
            {
                Monitor.Exit(srvMessagesLock);
            }
        }

        public bool unregisterDisabledServerMessage(int chrid)
        {
            Monitor.Enter(srvMessagesLock);
            try
            {
                return disabledServerMessages.Remove(chrid, out var d);
            }
            finally
            {
                Monitor.Exit(srvMessagesLock);
            }
        }

        protected override void HandleRun()
        {
            List<int> toRemove = new();

            Monitor.Enter(srvMessagesLock);
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
                Monitor.Exit(srvMessagesLock);
            }

            foreach (int chrid in toRemove)
            {
                var chr = worldChannel.Players.getCharacterById(chrid);

                if (chr != null && chr.isLoggedinWorld())
                {
                    chr.sendPacket(PacketCreator.serverMessage(worldChannel.WorldServerMessage));
                }
            }
        }

    }
}
