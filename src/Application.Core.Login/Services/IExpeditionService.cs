namespace Application.Core.Login.Services
{
    public interface IExpeditionService
    {
        ProtoService.ExpeditionCheckResponse CanStartExpedition(ProtoService.ExpeditionCheckRequest request);
        void RegisterExpedition(ProtoModel.ExpeditionRegistry request);
    }

    public class DefaultExpeditionService : IExpeditionService
    {
        public ProtoService.ExpeditionCheckResponse CanStartExpedition(ProtoService.ExpeditionCheckRequest request)
        {
            return new ProtoService.ExpeditionCheckResponse { IsSuccess = true };
        }

        public void RegisterExpedition(ProtoModel.ExpeditionRegistry request)
        {

        }
    }
}
