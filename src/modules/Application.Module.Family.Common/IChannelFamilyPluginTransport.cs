using Dto;

namespace Application.Module.Family.Common
{
    public interface IChannelFamilyPluginTransport
    {
        void AcceptFamily(int inviterId, int id);
        void Fork(CreateForkRequest createForkRequest);
        Dto.GetFamilyResponse GetFamily(int id);
        void SendDeclineSummon(DeclineSummonRequest declineSummonRequest);
        void UseEntitlement(UseEntitlementRequest useEntitlementRequest);
    }
}
