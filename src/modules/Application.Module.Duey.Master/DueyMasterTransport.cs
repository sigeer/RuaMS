using Application.Core.Login;
using Application.Core.Login.ServerTransports;
using Application.EF.Entities;
using Application.Module.Duey.Common;

namespace Application.Module.Duey.Master
{
    public class DueyMasterTransport : MasterServerTransportBase
    {
        public DueyMasterTransport(MasterServer masterServer) : base(masterServer)
        {
        }

        internal void SendCreatePackage(ProtoModel.CreatePackageBroadcastProto response)
        {
            SendMessage(BroadcastType.OnDueyPackageCreation, response, [response.Package.SenderId, response.Package.ReceiverId]);
        }

        internal void SendDueyPackageRemoved(ProtoService.RemovePackageResponse response)
        {
            SendMessage(BroadcastType.OnDueyPackageRemove, response, [response.Request.MasterId]);
        }

        internal void SendTakeDueyPackage(ProtoService.TakeDueyPackageResponse response)
        {
            List<int> receivers = [];
            if (response.Package != null)
                receivers.Add(response.Package.ReceiverId);

            receivers.Add(response.Request.MasterId);
            SendMessage(BroadcastType.OnDueyPackageTaking, response, receivers.ToArray());
        }

        internal void SendDueyNotifyOnLogin(int receiverId, ProtoModel.DueyNotifyProto response)
        {
            SendMessage(BroadcastType.OnDueyNotify, response, [receiverId]);
        }
    }
}
