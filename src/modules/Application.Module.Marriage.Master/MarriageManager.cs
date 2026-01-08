using Application.Core.Login;
using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Module.Marriage.Common.ErrorCodes;
using Application.Module.Marriage.Common.Models;
using Application.Module.Marriage.Master.Models;
using Application.Utility;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using MarriageProto;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace Application.Module.Marriage.Master
{
    public class MarriageManager : StorageBase<int, MarriageModel>
    {
        readonly MasterServer _server;
        readonly MasterTransport _transport;
        readonly IMapper _mapper;
        readonly IDbContextFactory<DBContext> _dbContextFactory;

        int _localMarriageId = 0;

        public MarriageManager(MasterServer server, MasterTransport transport, IMapper mapper, IDbContextFactory<DBContext> dbContextFactory)
        {
            _server = server;
            _transport = transport;
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
        }

        public override async Task InitializeAsync(DBContext dbContext)
        {
            _localMarriageId = await dbContext.Marriages.MaxAsync(x => (int?)x.Marriageid) ?? 0;
        }

        /// <summary>
        /// 订婚
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MarriageProto.CreateMarriageRelationResponse CreateMarriageRelation(MarriageProto.CreateMarriageRelationRequest request)
        {
            var husband = _server.CharacterManager.FindPlayerById(request.FromId);
            if (husband == null)
                return new MarriageProto.CreateMarriageRelationResponse { Code = (int)ProposalErrorCode.CharacterNotFound };

            var marriageInfo = GetEffectMarriageModel(request.FromId);
            if (marriageInfo != null)
                return new MarriageProto.CreateMarriageRelationResponse { Code = (int)ProposalErrorCode.AlreadyMarried };

            var wife = _server.CharacterManager.FindPlayerById(request.ToId);
            if (wife == null)
                return new MarriageProto.CreateMarriageRelationResponse { Code = (int)ProposalErrorCode.CharacterNotFound };

            marriageInfo = GetEffectMarriageModel(request.ToId);
            if (marriageInfo != null)
                return new MarriageProto.CreateMarriageRelationResponse { Code = (int)ProposalErrorCode.AlreadyMarried };

            var newModel = new MarriageModel()
            {
                Id = Interlocked.Increment(ref _localMarriageId),
                Time0 = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime()),
                Status = (int)MarriageStatusEnum.Engaged,
                Husbandid = request.FromId,
                Wifeid = request.ToId,
                EngagementItemId = request.ItemId
            };

            SetDirty(newModel.Id, new StoreUnit<MarriageModel>(StoreFlag.AddOrUpdate, newModel));
            return new MarriageProto.CreateMarriageRelationResponse { Data = _mapper.Map<MarriageProto.MarriageDto>(newModel) };
        }

        public async Task BreakMarriage(MarriageProto.BreakMarriageRequest request)
        {
            var res = new MarriageProto.BreakMarriageResponse() { Request = request };
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null)
            {
                res.Code = (int)BreakErrorCode.CharacterNotFound;
                await _transport.SendBreakMarriageCallback(res);
                return;
            }

            var wedding = GetEffectMarriageModel(chr.Character.Id);
            if (wedding == null)
            {
                res.Code = (int)BreakErrorCode.Single;
                await _transport.SendBreakMarriageCallback(res);
                return;
            }

            var partnerId = wedding.Wifeid == request.MasterId ? wedding.Husbandid : wedding.Wifeid;
            var partner = _server.CharacterManager.FindPlayerById(partnerId);
            if (partner == null)
            {
                res.Code = (int)BreakErrorCode.CharacterNotFound;
                await _transport.SendBreakMarriageCallback(res);
                return;
            }

            wedding.Status = wedding.Status == (int)MarriageStatusEnum.Engaged ? (int)MarriageStatusEnum.EngagementCanceled : (int)MarriageStatusEnum.Divorced;
            wedding.Time2 = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime());
            wedding.RingSourceId = 0;

            SetDirty(wedding.Id, new StoreUnit<MarriageModel>(StoreFlag.AddOrUpdate, wedding));
            await _transport.SendBreakMarriageCallback(res);
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<MarriageModel>> updateData)
        {
            await dbContext.Marriages.Where(x => updateData.Keys.Contains(x.Marriageid)).ExecuteDeleteAsync();

            foreach (var item in updateData)
            {
                if (item.Value.Flag == StoreFlag.AddOrUpdate)
                {
                    var obj = item.Value.Data;
                    dbContext.Marriages.Add(new MarriageEntity(obj.Id, obj.Husbandid, obj.Wifeid, obj.Status, obj.Time0, obj.Time1, obj.Time2, obj.EngagementItemId, obj.RingSourceId));
                }
            }
            await dbContext.SaveChangesAsync();
        }

        public override List<MarriageModel> Query(Expression<Func<MarriageModel, bool>> expression)
        {
            var entityExpression = _mapper.MapExpression<Expression<Func<MarriageEntity, bool>>>(expression).Compile();
            using var dbContext = _dbContextFactory.CreateDbContext();

            var dbList = dbContext.Marriages.AsNoTracking().Where(entityExpression).ToList();
            var dataFromDB = _mapper.Map<List<MarriageModel>>(dbList);

            return QueryWithDirty(dataFromDB, expression.Compile());
        }

        public MarriageModel? GetEffectMarriageModel(int chrId)
        {
            return Query(x => (x.Husbandid == chrId || x.Wifeid == chrId) && x.Status != (int)MarriageStatusEnum.Divorced).FirstOrDefault();
        }

        public MarriageProto.LoadMarriageInfoResponse GetEffectMarriageModelRemote(MarriageProto.LoadMarriageInfoRequest request)
        {
            return new LoadMarriageInfoResponse() { Data = _mapper.Map<MarriageProto.MarriageDto>(GetEffectMarriageModel(request.MasterId)) };
        }

        public void CompleteMarriage(MarriageModel model, RingSourceModel ringSource)
        {
            model.RingSourceId = ringSource.Id;
            model.Status = (int)MarriageStatusEnum.Married;
            model.Time1 = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime());
            SetDirty(model.Id, new StoreUnit<MarriageModel>(StoreFlag.AddOrUpdate, model));
        }

        internal async Task NotifyPartner(int to, int chrId, int chrMap)
        {
            await _transport.SendPlayerTransfter(new PlayerTransferDto { ToPlayerId = to, PlayerId = chrId, MapId = chrMap });
        }

        public async Task SpouseChat(MarriageProto.SendSpouseChatRequest request)
        {
            var res = new SendSpouseChatResponse { Request = request };
            var marriageInfo = GetEffectMarriageModel(request.SenderId);
            if (marriageInfo == null)
            {
                res.Code = 1;
                await _transport.SendSpouseChat(res);
                return;
            }
            else
            {
                var partnerId = marriageInfo.GetPartnerId(request.SenderId);
                var partner = _server.CharacterManager.FindPlayerById(partnerId);
                if (partner == null || partner.Channel <= 0)
                {
                    res.Code = 2;
                    await _transport.SendSpouseChat(res);
                    return;
                }

                var chr = _server.CharacterManager.FindPlayerById(request.SenderId)!;
                res.SenderPartnerId = partnerId;
                res.SenderName = chr.Character.Name;
                await _transport.SendSpouseChat(res);
            }
        }
    }
}
