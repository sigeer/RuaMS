using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Utility;
using LifeProto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using ZLinq;

namespace Application.Core.Login.ServerData
{
    public class PLifeDataManager : DataStorageBase<int, LifeProto.PLifeDto, PlifeEntity>
    {
        readonly MasterServer _server;

        public PLifeDataManager(ILogger<PLifeDataManager> logger, IMapper mapper, MasterServer server, IDbContextFactory<DBContext> dbContextFactory)
            : base(StorageCategory.PLife, dbContextFactory, mapper, logger)
        {
            _server = server;
        }

        protected override int GetKey(LifeProto.PLifeDto model) => model.Id;


        public LifeProto.GetPLifeByMapIdResponse LoadMapPLife(LifeProto.GetPLifeByMapIdRequest request)
        {
            var res = new LifeProto.GetPLifeByMapIdResponse();
            res.List.AddRange(_mapper.Map<LifeProto.PLifeDto[]>(Query(x => x.Map == request.MapId, x => x.MapId == request.MapId)));
            return res;
        }

        public LifeProto.GetAllPLifeResponse GetAllPLife()
        {
            var res = new LifeProto.GetAllPLifeResponse();
            res.List.AddRange(_mapper.Map<LifeProto.PLifeDto[]>(Query(x => true, x => true)));
            return res;
        }

        public async Task CreatePLife(LifeProto.CreatePLifeRequest request)
        {
            var newKey = Interlocked.Increment(ref _localId);
            request.Data.Id = newKey;
            SetDirty(request.Data);

            await _server.Transport.BroadcastPLifeCreated(request);
        }

        public async Task RemovePLife(LifeProto.RemovePLifeRequest request)
        {
            List<PLifeDto> toRemove = [];
            if (request.LifeId > 0)
            {
                toRemove = Query(x => x.Type == request.LifeType && x.Map == request.MapId && x.Life == request.LifeId,
                    x => x.Type == request.LifeType && x.MapId == request.MapId && x.LifeId == request.LifeId);
            }
            else
            {
                toRemove = Query(x => x.Type == request.LifeType && x.Map == request.MapId && x.X >= request.PosX - 50 && x.X <= request.PosX + 50 && x.Y >= request.PosY - 50 && x.Y <= request.PosY + 50,
                    x => x.Type == request.LifeType && x.MapId == request.MapId && x.X >= request.PosX - 50 && x.X <= request.PosX + 50 && x.Y >= request.PosY - 50 && x.Y <= request.PosY + 50);
            }

            foreach (var item in toRemove)
            {
                SetRemoved(item);
            }

            var res = new LifeProto.RemovePLifeResponse { MasterId = request.MasterId };
            res.RemovedItems.AddRange(_mapper.Map<LifeProto.PLifeDto[]>(toRemove));
            await _server.Transport.BroadcastPLifeRemoved(res);
        }
    }
}
