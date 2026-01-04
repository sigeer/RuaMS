using Application.Core.Login;
using Application.Core.Login.ServerTransports;
using Dto;
using System.Threading.Tasks;

namespace Application.Module.Family.Master
{
    public class MasterFamilyModuleTransport: MasterServerTransportBase
    {
        public MasterFamilyModuleTransport(MasterServer masterServer) : base(masterServer)
        {
        }

        internal async Task Send(UseEntitlementResponse useEntitlementResponse)
        {
            await SendMessageN(1, useEntitlementResponse, [useEntitlementResponse.Request.MatserId]);
        }

        internal void SendReputationChanged(int cid, ReputationChangedMessage reputationChangedMessage)
        {
            throw new NotImplementedException();
        }
    }
}
