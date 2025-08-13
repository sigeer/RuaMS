namespace Application.Core.Login.Services
{
    public interface IExpeditionService
    {
        ExpeditionProto.ExpeditionCheckResponse CanStartExpedition(ExpeditionProto.ExpeditionCheckRequest request);
        void RegisterExpedition(ExpeditionProto.ExpeditionRegistry request);
    }

    public class DefaultExpeditionService : IExpeditionService
    {
        public ExpeditionProto.ExpeditionCheckResponse CanStartExpedition(ExpeditionProto.ExpeditionCheckRequest request)
        {
            return new ExpeditionProto.ExpeditionCheckResponse { IsSuccess = true };
        }

        public void RegisterExpedition(ExpeditionProto.ExpeditionRegistry request)
        {

        }
    }
}
