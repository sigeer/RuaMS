using Application.Core.Login.Dtos.Report;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Message;
using Application.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Services
{
    public class ReportService
    {
        readonly ILogger<ReportService> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;
        readonly MasterServer _server;

        public ReportService(ILogger<ReportService> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, MasterServer server)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _server = server;
        }

        public async Task AddReport(ProtoService.SendReportRequest request)
        {
            var res = new ProtoService.SendReportResponse() { MasterId = request.MasterId };
            var target = _server.CharacterManager.FindPlayerByName(request.Victim);
            if (target == null)
            {
                res.Code = 1;
                await _server.Transport.SendMessageN(ChannelRecvCode.HandleReportReceived, res, [res.MasterId]);
                return;
            }

            using var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Reports.Add(new ReportEntity
            {
                ReportTime = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime()),
                ReporterId = request.MasterId,
                VictimId = target.Character.Id,
                Reason = (sbyte)request.Reason,
                Chatlog = request.ChatLog,
                Description = request.Text
            });
            dbContext.SaveChanges();

            await _server.DropWorldMessage(6, $"{request.Victim} 被举报：{request.Text}", true);
            await _server.Transport.SendMessageN(ChannelRecvCode.HandleReportReceived, res, [res.MasterId]);
        }

        public (List<ReportResponseDto> Data, int Total) GetReportPagedData(int filterProcessed, int pageIndex, int pageSize)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var allData = dbContext.Reports.OrderByDescending(x => x.ReportTime).AsQueryable();
            if (filterProcessed == 0)
                allData = allData.Where(x => !x.Processed);
            else if (filterProcessed > 0)
                allData = allData.Where(x => x.Processed);

            var data = allData.ToPage(pageIndex, pageSize).ProjectToType<ReportResponseDto>().ToList();
            foreach (var item in data)
            {
                item.Reporter = _server.CharacterManager.GetCharacterDto(item.ReporterId);
                item.Victim = _server.CharacterManager.GetCharacterDto(item.VictimId);
            }
            return (data, allData.Count());
        }

        public void SetReportProcessed(int id)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Reports.Where(x => x.Id == id).ExecuteUpdate(x => x.SetProperty(y => y.Processed, true));
        }
    }
}
