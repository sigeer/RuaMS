using Application.EF;
using Application.EF.Entities;
using Application.Shared.Message;
using AutoMapper;
using Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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

        public async Task AddReport(SendReportRequest request)
        {
            var res = new SendReportResponse() { MasterId = request.MasterId };
            var target = _server.CharacterManager.FindPlayerByName(request.Victim);
            if (target == null)
            {
                res.Code = 1;
                await _server.Transport.SendMessageN(ChannelRecvCode.HandleReportReceived, res, [res.MasterId]);
                return;
            }

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

            await _server.DropWorldMessage(6, $"{request.Victim} 被举报：{request.Text}", true);
            await _server.Transport.SendMessageN(ChannelRecvCode.HandleReportReceived, res, [res.MasterId]);
        }
    }
}
