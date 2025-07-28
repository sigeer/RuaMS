using Application.Core.Login;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Module.Marriage.Common.ErrorCodes;
using Application.Module.Marriage.Common.Models;
using Application.Module.Marriage.Master.Models;
using Application.Utility;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        public async Task InitializeAsync(DBContext dbContext)
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

            if (husband.Character.EffectMarriageId != 0)
                return new MarriageProto.CreateMarriageRelationResponse { Code = (int)ProposalErrorCode.AlreadyMarried };

            var wife = _server.CharacterManager.FindPlayerById(request.ToId);
            if (wife == null)
                return new MarriageProto.CreateMarriageRelationResponse { Code = (int)ProposalErrorCode.CharacterNotFound };

            if (wife.Character.EffectMarriageId != 0)
                return new MarriageProto.CreateMarriageRelationResponse { Code = (int)ProposalErrorCode.AlreadyMarried };

            var newModel = new MarriageModel()
            {
                Id = Interlocked.Increment(ref _localMarriageId),
                Time0 = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime()),
                Status = (int)MarriageStatusEnum.Engaged,
                Husbandid = request.FromId,
                Wifeid = request.ToId
            };
            husband.Character.EffectMarriageId = newModel.Id;
            wife.Character.EffectMarriageId = newModel.Id;

            SetDirty(newModel.Id, new StoreUnit<MarriageModel>(StoreFlag.AddOrUpdate, newModel));
            return new MarriageProto.CreateMarriageRelationResponse { MarriageId = newModel.Id };
        }

        public MarriageProto.BreakMarriageResponse BreakMarriage(MarriageProto.BreakMarriageRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null)
                return new MarriageProto.BreakMarriageResponse { Code = (int)BreakErrorCode.CharacterNotFound };

            var wedding = Query(x => x.Id == chr.Character.EffectMarriageId).FirstOrDefault();
            if (wedding == null)
            {
                return new MarriageProto.BreakMarriageResponse { Code = (int)BreakErrorCode.Single };
            }

            if (wedding != null)
            {
                var partnerId = wedding.Wifeid == request.MasterId ? wedding.Husbandid : wedding.Wifeid;
                var partner = _server.CharacterManager.FindPlayerById(partnerId);
                if (partner == null)
                {
                    return new MarriageProto.BreakMarriageResponse { Code = (int)BreakErrorCode.CharacterNotFound };
                }

                _transport.SendBreakMarriageCallback(new MarriageProto.BreakMarriageCallback
                {
                    Code = 0,
                    MasterId = request.MasterId,
                    MasterName = chr.Character.Name,
                    Type = wedding.Status,
                    MasterPartnerId = wedding.Wifeid == request.MasterId ? wedding.Husbandid : wedding.Wifeid,
                    MasterPartnerName = partner.Character.Name
                });

                chr.Character.EffectMarriageId = 0;
                partner.Character.EffectMarriageId = 0;

                wedding.Status = (int)MarriageStatusEnum.Divorced;
                wedding.Time2 = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime());
                SetDirty(wedding.Id, new StoreUnit<MarriageModel>(StoreFlag.AddOrUpdate, wedding));

                return new MarriageProto.BreakMarriageResponse { Code = 0 };
            }
            return new MarriageProto.BreakMarriageResponse() { Code = 2 };
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<MarriageModel>> updateData)
        {
            await dbContext.Marriages.Where(x => updateData.Keys.Contains(x.Marriageid)).ExecuteDeleteAsync();

            foreach (var item in updateData)
            {
                if (item.Value.Flag == StoreFlag.AddOrUpdate)
                {
                    var obj = item.Value.Data;
                    dbContext.Marriages.Add(new MarriageEntity(obj.Id, obj.Husbandid, obj.Wifeid, obj.Status, obj.Time0, obj.Time1, obj.Time2));
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
        internal bool CompleteMarriage(int id)
        {
            var model = Query(x => x.Id == id).FirstOrDefault();
            if (model == null || model.Status != 0)
                return false;

            model.Status = (int)MarriageStatusEnum.Married;
            model.Time1 = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime());
            SetDirty(model.Id, new StoreUnit<MarriageModel>(StoreFlag.AddOrUpdate, model));
            return true;
        }
    }
}
