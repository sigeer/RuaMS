using Application.Core.Channel.Commands;
using tools;

namespace Application.Core.Channel.ServerData
{
    public class ServerMessageManager
    {
        private Dictionary<int, int> disabledServerMessages = new();

        readonly WorldChannel _server;

        public ServerMessageManager(WorldChannel server)
        {
            this._server = server;
        }

        public void resetDisabledServerMessages()
        {
            disabledServerMessages.Clear();
        }

        public bool registerDisabledServerMessage(int chrid)
        {
            bool alreadyDisabled = disabledServerMessages.ContainsKey(chrid);
            disabledServerMessages.AddOrUpdate(chrid, 0);

            return alreadyDisabled;
        }

        public bool unregisterDisabledServerMessage(int chrid)
        {
            return disabledServerMessages.Remove(chrid, out var d);
        }

        public void HandleRun()
        {
            List<int> toRemove = new();

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

            foreach (int chrid in toRemove)
            {
                var chr = _server.getPlayerStorage().getCharacterById(chrid);

                if (chr != null && chr.isLoggedinWorld())
                {
                    chr.sendPacket(PacketCreator.serverMessage(_server.WorldServerMessage));
                }
            }
        }

    }
}
