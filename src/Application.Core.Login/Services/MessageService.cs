using Application.EF;
using Application.EF.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Services
{
    public class MessageService
    {
        readonly ILogger<MessageService> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;

        public MessageService(ILogger<MessageService> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
        }

        public void AddReport(int fromId, int toId, int reason, string description, string chatLog)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Reports.Add(new Report
            {
                Reporttime = DateTimeOffset.UtcNow,
                Reporterid = fromId,
                Victimid = toId,
                Reason = (sbyte)reason,
                Chatlog = chatLog,
                Description = description
            });
            dbContext.SaveChanges();
        }
    }
}
