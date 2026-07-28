using Application.Core.Login.Models.Items;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using CashProto;
using ItemProto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.ServerData
{
    public class GiftManager : DataStorageBase<int, GiftModel, GiftEntity>
    {
        readonly MasterServer _server;
        readonly NoteManager _noteService;

        public GiftManager(IMapper mapper, IDbContextFactory<DBContext> dbContextFactory, MasterServer server, NoteManager noteService, ILogger<GiftManager> logger)
            : base(StorageCategory.Gift, dbContextFactory, mapper, logger)
        {
            _server = server;
            _noteService = noteService;
        }

        protected override int GetKey(GiftModel model) => model.Id;
        public CreateGiftResponse CreateGift(int fromId, string toName, int sn, int cashItemId, string message, bool createRing)
        {
            var receiver = _server.CharacterManager.FindPlayerByName(toName);
            if (receiver == null)
            {
                return new CreateGiftResponse { Code = 0xA9, Recipient = toName };
            }

            var sender = _server.CharacterManager.FindPlayerById(fromId)!;
            if (sender.Character.AccountId == receiver.Character.AccountId)
            {
                return new CreateGiftResponse { Code = 0xA8, Recipient = toName };
            }

            var ringModel = createRing ? _server.RingManager.CreateRing(cashItemId, sender.Character.Id, receiver.Character.Id) : null;
            var ringDto = _server.RingManager.MapDto(ringModel);
            var newModel = new GiftModel
            {
                Id = Interlocked.Increment(ref _localId),
                FromId = sender.Character.Id,
                Message = message,
                Sn = sn,
                ToId = receiver.Character.Id,
                RingSourceId = ringModel?.Id ?? 0,
            };
            SetDirty(newModel);

            if (!createRing)
                _ = _noteService.SendNormal(sender.Character.Name + " has sent you a gift! Go check out the Cash Shop.", sender.Character.Id, receiver.Character.Name);
            else
                _ = _noteService.SendWithFame(message, sender.Character.Id, receiver.Character.Name);



            return new CreateGiftResponse { Recipient = toName, RingSource = ringDto };
        }

        public GetMyGiftsResponse LoadGifts(GetMyGiftsRequest request)
        {
            var gifts = Query(x => x.ToId == request.MasterId && x.ClaimTime == null, x => x.ToId == request.MasterId && x.ClaimTime == null);
            var res = new GetMyGiftsResponse();
            res.List.AddRange(MapDto(gifts));
            return res;
        }

        List<ItemProto.GiftDto> MapDto(List<GiftModel> model)
        {
            var ringIdList = model.Select(x => x.RingSourceId).ToList();
            var rings = _server.RingManager.Query(x => ringIdList.Contains(x.Id), x => ringIdList.Contains(x.Id)).ToList();

            var list = _mapper.Map<List<ItemProto.GiftDto>>(model);
            foreach (var item in list)
            {
                item.FromName = _server.CharacterManager.GetPlayerName(item.From);
                item.ToName = _server.CharacterManager.GetPlayerName(item.To);
                item.Ring = _server.RingManager.MapDto(rings.FirstOrDefault(x => x.Id == item.RingSourceId));
            }
            return list;
        }

        public void CommitRetrieveGift(int[] giftIdArray)
        {
            var gifts = Query(x => giftIdArray.Contains(x.Id), x => giftIdArray.Contains(x.Id));
            foreach (var item in gifts)
            {
                item.ClaimTime = _server.GetCurrentTimeDateTimeOffset();
                SetRemoved(item);
            }
        }

        protected override void CommitRemove(DBContext dbContext, GiftEntity? dbModel, GiftModel localModel)
        {
            if (dbModel != null)
                dbModel.ClaimTime = localModel.ClaimTime;
        }
    }
}
