using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Utility;
using AutoMapper;
using BaseProto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using ZLinq;

namespace Application.Core.Login.ServerData
{
    public class ResourceDataManager : StorageBase<int, UpdateField<PLifeDto>>
    {
        readonly ILogger<ResourceDataManager> _logger;
        readonly IMapper _mapper;
        readonly MasterServer _server;

        ConcurrentDictionary<int, PLifeDto> _pLifeData = new();
        int _localPLifeId = 0;

        public ResourceDataManager(ILogger<ResourceDataManager> logger, IMapper mapper, MasterServer server)
        {
            _logger = logger;
            _mapper = mapper;
            _server = server;
        }

        public async Task Initialize(DBContext dbContext)
        {
            var dbList = _mapper.Map<PLifeDto[]>(await dbContext.Plives.AsNoTracking().ToArrayAsync());
            foreach (var item in dbList)
            {
                _pLifeData[Interlocked.Increment(ref _localPLifeId)] = item;
            }
        }

        public GetPLifeByMapIdResponse LoadMapPLife(GetPLifeByMapIdRequest request)
        {
            var res = new GetPLifeByMapIdResponse();
            res.List.AddRange(_pLifeData.Where(x => x.Value.MapId == request.MapId).Select(x => x.Value));
            return res;
        }

        public void CreatePLife(CreatePLifeRequest request)
        {
            var newKey = Interlocked.Increment(ref _localPLifeId);
            _pLifeData[newKey] = request.Data;

            SetDirty(newKey, new UpdateField<PLifeDto>(UpdateMethod.AddOrUpdate, request.Data));

            _server.Transport.BroadcastPLifeCreated(request);
        }

        public void RemovePLife(RemovePLifeRequest request)
        {
            List<KeyValuePair<int, PLifeDto>> toRemove = [];
            if (request.LifeId > 0)
            {
                toRemove = _pLifeData.Where(x => x.Value.Type == request.LifeType && x.Value.MapId == request.MapId && x.Value.LifeId == request.LifeId).ToList();

            }
            else
            {
                toRemove = _pLifeData.Where(x => x.Value.Type == request.LifeType && x.Value.MapId == request.MapId && x.Value.LifeId == request.LifeId)
                    .Where(x => x.Value.X >= request.PosX - 50 && x.Value.X <= request.PosX + 50 && x.Value.Y >= request.PosY - 50 && x.Value.Y <= request.PosY + 50).ToList();
            }

            foreach (var item in toRemove)
            {
                _pLifeData.TryRemove(item.Key, out _);
                SetDirty(item.Key, new UpdateField<PLifeDto>(UpdateMethod.Remove, item.Value));
            }

            var res = new RemovePLifeResponse { MasterId = request.MasterId };
            res.RemovedItems.AddRange(toRemove.Select(x => x.Value));
            _server.Transport.BroadcastPLifeRemoved(res);
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, UpdateField<PLifeDto>> updateData)
        {
            var usedData = updateData.Values.Select(y => y.Data.Id).Where(x => x > 0).ToHashSet();
            await dbContext.Plives.Where(x => usedData.Contains(x.Id)).ExecuteDeleteAsync();
            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                if (item.Method == UpdateMethod.AddOrUpdate)
                {
                    var model = new PlifeEntity(obj.MapId, obj.LifeId, obj.Mobtime, obj.X, obj.Y, obj.Fh, obj.Type);
                    dbContext.Plives.Add(model);
                    await dbContext.SaveChangesAsync();
                    obj.Id = model.Id;
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
