
namespace Application.Module.Family.Common
{
    public interface IChannelFamilyPluginTransport
    {
        void AcceptFamily(int inviterId, int id);
        void Fork(Dto.CreateForkRequest createForkRequest);
        Dto.GetFamilyResponse GetFamily(int id);
        void SendDeclineSummon(Dto.DeclineSummonRequest declineSummonRequest);
        void UseEntitlement(Dto.UseEntitlementRequest useEntitlementRequest);
    }
}
