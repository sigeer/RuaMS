using Application.Core.Login;
using Application.Core.Login.ServerTransports;
using Application.EF.Entities;
using Application.Module.Duey.Common;
using DueyDto;

namespace Application.Module.Duey.Master
{
    public class DueyMasterTransport : MasterServerTransportBase
    {
        public DueyMasterTransport(MasterServer masterServer) : base(masterServer)
        {
        }

        internal void SendCreatePackage(CreatePackageBroadcast response)
        {
            SendMessage(BroadcastType.OnDueyPackageCreation, response, response.Package.SenderId, response.Package.ReceiverId);
        }

        internal void SendDueyPackageRemoved(RemovePackageResponse response)
        {
            SendMessage(BroadcastType.OnDueyPackageRemove, response, response.Request.MasterId);
        }

        internal void SendTakeDueyPackage(TakeDueyPackageResponse response)
        {
            List<int> receivers = [];
            if (response.Package != null)
                receivers.Add(response.Package.ReceiverId);

            receivers.Add(response.Request.MasterId);
            SendMessage(BroadcastType.OnDueyPackageTaking, response, receivers.ToArray());
        }

        internal void SendDueyNotifyOnLogin(int receiverId, DueyNotifyDto response)
        {
            SendMessage(BroadcastType.OnDueyNotify, response, receiverId);
        }
    }
}
