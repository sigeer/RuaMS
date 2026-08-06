using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.ServerData
{
    public class RingManager : DataStorageBase<int, RingSourceModel, RingEntity>
    {
        readonly MasterServer _server;
        public RingManager(IDbContextFactory<DBContext> dbContextFactory, ILogger<RingManager> logger, IMapper mapper, MasterServer server)
            : base(StorageCategory.Ring, dbContextFactory, mapper, logger)
        {
            _server = server;
        }
        protected override int GetKey(RingSourceModel model) => model.Id;


        public RingSourceModel CreateRing(int itemId, int chr1, int chr2)
        {
            var model = new RingSourceModel()
            {
                Id = Interlocked.Increment(ref _localId),
                CharacterId1 = chr1,
                CharacterId2 = chr2,
                ItemId = itemId,
                RingId1 = Yitter.IdGenerator.YitIdHelper.NextId(),
                RingId2 = Yitter.IdGenerator.YitIdHelper.NextId(),
            };
            SetDirty(model);
            return model;
        }

        public ProtoModel.RingProto? MapDto(RingSourceModel? model)
        {
            if (model == null)
            {
                return null;
            }
            var item = _mapper.Map<ProtoModel.RingProto>(model);
            item.CharacterName1 = _server.CharacterManager.GetPlayerName(item.CharacterId1);
            item.CharacterName2 = _server.CharacterManager.GetPlayerName(item.CharacterId2);
            return item;
        }

        public List<ProtoModel.RingProto> LoadCharacterRings(int chrId)
        {
            var items = Query(x => x.CharacterId1 == chrId || x.CharacterId2 == chrId, x => x.CharacterId1 == chrId || x.CharacterId2 == chrId);
            return items.Select(x => MapDto(x)).OfType<ProtoModel.RingProto>().ToList();
        }
    }
}
