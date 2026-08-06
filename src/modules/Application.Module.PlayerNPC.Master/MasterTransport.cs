using Application.Core.Login;
using Application.Core.Login.ServerTransports;

namespace Application.Module.PlayerNPC.Master
{
    public class MasterTransport : MasterServerTransportBase
    {
        public MasterTransport(MasterServer masterServer) : base(masterServer)
        {
        }

        internal void BroadcastRemoveAllPlayerNpc(ProtoService.RemoveAllPlayerNPCResponse res)
        {
            // BroadcastMessage(Common.BroadcastMessage.OnClearPlayerNpc, res);
        }

        internal void BroadcastRemovePlayerNpc(ProtoService.RemovePlayerNPCResponse res)
        {
            // BroadcastMessage(Common.BroadcastMessage.OnRemovePlayerNpc, res);
        }

        internal void BroadcastRefreshMapData(ProtoService.UpdateMapPlayerNPCResponse res)
        {
            // BroadcastMessage(Common.BroadcastMessage.OnMapPlayerNpcUpdate, res);
        }
    }
}
