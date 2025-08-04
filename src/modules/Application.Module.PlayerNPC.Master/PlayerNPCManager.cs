using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Module.PlayerNPC.Master.Models;
using Application.Shared.Constants.Npc;
using Application.Utility;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using PlayerNPCProto;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace Application.Module.PlayerNPC.Master
{
    public class PlayerNPCManager : StorageBase<int, PlayerNpcModel>
    {
        readonly MasterTransport _transport;
        readonly IMapper _mapper;
        readonly IDbContextFactory<DBContext> _dbContextFactory;

        ConcurrentDictionary<int, int> _fields = new();

        int _currentId = NpcId.PlayerNpc_ObjectId_Base;
        public PlayerNPCManager(MasterTransport transport, IMapper mapper, IDbContextFactory<DBContext> dbContextFactory)
        {
            _transport = transport;
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
        }

        public override async Task InitializeAsync(DBContext dbContext)
        {
            _currentId = await dbContext.Playernpcs.MaxAsync(x => (int?)x.Id) ?? NpcId.PlayerNpc_ObjectId_Base;

            _fields = new(dbContext.PlayernpcsFields.ToList().ToDictionary(x => x.Map, x => (int)x.Step));
        }

        public override List<PlayerNpcModel> Query(Expression<Func<PlayerNpcModel, bool>> expression)
        {
            var entityExpression = _mapper.MapExpression<Expression<Func<PlayerNpcEntity, bool>>>(expression).Compile();
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbList = dbContext.Playernpcs.Where(entityExpression).ToList();
            var dbIdList = dbList.Select(x => x.Id).ToArray();

            var equips = dbContext.PlayernpcsEquips
                .Where(x => dbIdList.Contains(x.Npcid))
                .AsEnumerable()
                .GroupBy(x => x.Npcid)
                .ToDictionary(g => g.Key, g => g.ToList());

            var dataFromDB = _mapper.Map<List<PlayerNpcModel>>(dbList);

            foreach (var item in dataFromDB)
            {
                if (equips.TryGetValue(item.Id, out var eqList))
                {
                    item.Equips = _mapper.Map<List<PlayerNpcEquipModel>>(eqList);
                }
            }

            return QueryWithDirty(dataFromDB, expression.Compile());
        }

        public GetMapPlayerNPCListResponse GetMapData(GetMapPlayerNPCListRequest request)
        {
            var allData = Query(x => x.Map == request.MapId);

            var res = new GetMapPlayerNPCListResponse();
            res.List.AddRange(_mapper.Map<PlayerNPCDto[]>(allData));
            return res;
        }

        public void Remove(RemovePlayerNPCRequest request)
        {
            var willRemoed = Query(x => x.Name == request.TargetName);
            foreach (var item in willRemoed)
            {
                SetRemoved(item.Id);
            }

            var res = new RemovePlayerNPCResponse();
            res.List.AddRange(willRemoed.Select(x => new RemovePlayerNPCItemResponse { MapId = x.Map, ObjectId = x.Id }));
            _transport.BroadcastRemovePlayerNpc(res);
        }

        public void RemoveAll()
        {
            var willRemoed = Query(x => true);
            foreach (var item in willRemoed)
            {
                SetRemoved(item.Id);
            }

            var res = new RemoveAllPlayerNPCResponse();
            res.MapIdList.AddRange(willRemoed.Select(x => x.Map).ToHashSet());
            _transport.BroadcastRemoveAllPlayerNpc(res);
        }

        Lock createLock = new Lock();

        public CreatePlayerNPCPreResponse PreCreate(CreatePlayerNPCPreRequest request)
        {
            if (!createLock.TryEnter())
            {
                return new CreatePlayerNPCPreResponse { Code = 1 };
            }

            // 返回 script id, 
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbList = dbContext.Playernpcs.Where(x => x.Scriptid >= request.BranchSidStart && x.Scriptid < request.BranchSidEnd);

            var dataFromDB = _mapper.Map<List<PlayerNpcModel>>(dbList);

            var allData = QueryWithDirty(dataFromDB, x => x.Scriptid >= request.BranchSidStart && x.Scriptid < request.BranchSidEnd);

            var res = new CreatePlayerNPCPreResponse();
            res.UsedScriptIdList.AddRange(allData.Select(x => x.Scriptid).ToHashSet());
            res.NextPositionData = _fields.GetValueOrDefault(request.MapId, -1);
            res.MapId = request.MapId;
            return res;
        }

        public void Create(CreatePlayerNPCRequest request)
        {
            var newId = Interlocked.Increment(ref _currentId);
            var model = _mapper.Map<PlayerNpcModel>(request.NewData);
            model.Id = newId;

            if (model.IsHonor)
            {
                model.JobRank = 1;
                model.OverallRank = 1;
            }
            _fields[request.MapId] = request.NextStepData;

            var updatedList = _mapper.Map<List<PlayerNpcModel>>(request.UpdatedList);
            foreach (var item in updatedList)
            {
                SetDirty(item.Id, new StoreUnit<PlayerNpcModel>(StoreFlag.AddOrUpdate, item));
            }

            SetDirty(model.Id, new StoreUnit<PlayerNpcModel>(StoreFlag.AddOrUpdate, model));

            var res = new UpdateMapPlayerNPCResponse();
            res.MapId = request.MapId;
            res.UpdatedList.AddRange(request.UpdatedList);
            res.NewData = _mapper.Map<PlayerNPCDto>(model);
            _transport.BroadcastRefreshMapData(res);

            createLock.Exit();
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<PlayerNpcModel>> updateData)
        {
            var updateThreads = updateData.Keys.ToArray();
            await dbContext.Playernpcs.Where(x => updateThreads.Contains(x.Id)).ExecuteDeleteAsync();
            await dbContext.PlayernpcsEquips.Where(x => updateThreads.Contains(x.Npcid)).ExecuteDeleteAsync();

            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                if (item.Flag == StoreFlag.AddOrUpdate && obj != null)
                {
                    var dbData = _mapper.Map<PlayerNpcEntity>(obj);
                    dbContext.Playernpcs.Add(dbData);
                    dbContext.PlayernpcsEquips.AddRange(obj.Equips.Select(x => new PlayerNpcsEquipEntity() { Npcid = dbData.Id, Equipid = x.Equipid, Equippos = x.Equippos, Type = x.Type }));
                }
            }

            await dbContext.PlayernpcsFields.Where(x => _fields.Keys.Contains(x.Map)).ExecuteDeleteAsync();
            foreach (var item in _fields)
            {
                dbContext.PlayernpcsFields.Add(new PlayernpcsField { Map = item.Key, Step = (sbyte)item.Value });
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
