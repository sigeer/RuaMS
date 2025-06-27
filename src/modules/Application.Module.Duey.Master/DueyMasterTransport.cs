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

        internal void SendCreatePackage(CreatePackageResponse response)
        {
            SendMessage(BroadcastType.OnDueyPackageCreation, response, response.Package.SenderId, response.Package.ReceiverId);
        }

        internal void SendDueyPackageRemoved(RemovePackageResponse response)
        {
            SendMessage(BroadcastType.OnDueyPackageRemove, response, response.Request.MasterId);
        }

        internal void SendTakeDueyPackage(TakeDueyPackageResponse response)
        {
            SendMessage(BroadcastType.OnDueyPackageTaking, response, response.Package.ReceiverId);
        }

        public void SendDueyNotification(DueyDto.DueyNotificationDto response)
        {
            SendMessage(BroadcastType.OnDueyNotification, response, response.ReceiverId);
        }

        internal void SendDueyNotifyOnLogin(int receiverId, DueyNotifyDto response)
        {
            SendMessage(BroadcastType.OnDueyNotify, response, receiverId);
        }
    }
}
