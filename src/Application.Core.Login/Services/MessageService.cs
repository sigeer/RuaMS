using Application.EF;
using Application.EF.Entities;
using AutoMapper;
using Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Services
{
    public class MessageService
    {
        readonly ILogger<MessageService> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;
        readonly MasterServer _server;

        public MessageService(ILogger<MessageService> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, MasterServer server)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _server = server;
        }

        public SendReportResponse AddReport(SendReportRequest request)
        {
            var target = _server.CharacterManager.FindPlayerByName(request.Victim);
            if (target == null)
                return new SendReportResponse { IsSuccess = false };

            using var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Reports.Add(new Report
            {
                Reporttime = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime()),
                Reporterid = request.MasterId,
                Victimid = target.Character.Id,
                Reason = (sbyte)request.Reason,
                Chatlog = request.ChatLog,
                Description = request.Text
            });
            dbContext.SaveChanges();

            var data = new SendReportBroadcast();
            data.GmId.AddRange(_server.CharacterManager.GetOnlinedGMs());
            data.Text = $"{request.Victim} 被举报：{request.Text}";
            _server.Transport.BroadcastReportNotify(data);
            return new SendReportResponse { IsSuccess = true };
        }
    }
}
