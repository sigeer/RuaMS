using Application.EF;
using Dto;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Events
{
    public abstract class MasterModule
    {
        protected readonly MasterServer _server;
        protected readonly ILogger<MasterModule> _logger;

        protected MasterModule(MasterServer server, ILogger<MasterModule> logger)
        {
            _server = server;
            _logger = logger;
        }

        public virtual void Initialize()
        {
            _logger.LogInformation("主模块 {Name} 加载完成", GetType().Assembly.FullName);
        }
        public virtual int DeleteCharacterCheck(int id)
        {
            return 0;
        }
        public virtual void OnPlayerDeleted(int id)
        {

        }

        public virtual Task SaveChangesAsync(DBContext dbContext)
        {
            return Task.CompletedTask;
        }
    }
}
