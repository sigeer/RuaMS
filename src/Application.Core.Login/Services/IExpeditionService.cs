using Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Login.Services
{
    public interface IExpeditionService
    {
        ExpeditionCheckResponse CanStartExpedition(ExpeditionCheckRequest request);
        void RegisterExpedition(ExpeditionRegistry request);
    }

    public class DefaultExpeditionService : IExpeditionService
    {
        public ExpeditionCheckResponse CanStartExpedition(ExpeditionCheckRequest request)
        {
            return new ExpeditionCheckResponse { IsSuccess = true };
        }

        public void RegisterExpedition(ExpeditionRegistry request)
        {
            
        }
    }
}
