using Application.Core.Login;
using Application.Core.Login.ServerTransports;
using PlayerNPCProto;

namespace Application.Module.PlayerNPC.Master
{
    public class MasterTransport: MasterServerTransportBase
    {
        public MasterTransport(MasterServer masterServer) : base(masterServer)
        {
        }

        internal void BroadcastRemoveAllPlayerNpc(RemoveAllPlayerNPCResponse res)
        {
            BroadcastMessage(Common.BroadcastMessage.OnClearPlayerNpc, res);
        }

        internal void BroadcastRemovePlayerNpc(RemovePlayerNPCResponse res)
        {
            BroadcastMessage(Common.BroadcastMessage.OnRemovePlayerNpc, res);
        }

        internal void BroadcastRefreshMapData(UpdateMapPlayerNPCResponse res)
        {
            BroadcastMessage(Common.BroadcastMessage.OnMapPlayerNpcUpdate, res);
        }
    }
}
