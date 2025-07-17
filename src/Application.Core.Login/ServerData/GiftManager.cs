using Application.Core.Login.Models;
using Application.Core.Login.Services;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Utility;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using CashProto;
using ItemProto;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Core.Login.ServerData
{
    public class GiftManager : StorageBase<int, GiftModel>
    {
        readonly IMapper _mapper;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly MasterServer _server;
        readonly NoteManager _noteService;

        int _localId = 0;

        public GiftManager(IMapper mapper, IDbContextFactory<DBContext> dbContextFactory, MasterServer server, NoteManager noteService)
        {
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
            _server = server;
            _noteService = noteService;
        }

        public async Task Initialize(DBContext dbContext)
        {
            _localId = await dbContext.Gifts.MaxAsync(x => (int?)x.Id) ?? 0;
        }
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

            var newId = Interlocked.Increment(ref _localId);
            var newModel = new GiftModel
            {
                Id = newId,
                From = sender.Character.Id,
                Message = message,
                Sn = sn,
                To = receiver.Character.Id,
                RingSourceId = ringModel?.Id ?? -1
            };
            SetDirty(newModel.Id, new StoreUnit<GiftModel>(StoreFlag.AddOrUpdate, newModel));

            if (!createRing)
                _noteService.SendNormal(sender.Character.Name + " has sent you a gift! Go check out the Cash Shop.", sender.Character.Id, receiver.Character.Name);
            else
                _noteService.SendWithFame(message, sender.Character.Id, receiver.Character.Name);

            return new CreateGiftResponse { Recipient = toName, RingSource = _mapper.Map<ItemProto.RingDto>(ringModel) };
        }

        public GetMyGiftsResponse LoadGifts(GetMyGiftsRequest request)
        {
            var gifts = Query(x => x.To == request.MasterId);
            var ringIds = gifts.Select(x => x.RingSourceId).ToArray();
            var rings = _server.RingManager.Query(x => ringIds.Contains(x.Id));

            var res = new GetMyGiftsResponse();
            foreach (var gift in gifts)
            {
                var dto = _mapper.Map<ItemProto.GiftDto>(gifts);

                var ring = rings.FirstOrDefault(x => x.Id == gift.RingSourceId);
                dto.Ring = _mapper.Map<ItemProto.RingDto>(ring);
                res.List.Add(dto);
            }
            return res;
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<GiftModel>> updateData)
        {
            var updateKeys = updateData.Keys.ToArray();
            await dbContext.Gifts.Where(x => updateKeys.Contains(x.Id)).ExecuteDeleteAsync();

            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                if (item.Flag == StoreFlag.AddOrUpdate && obj != null)
                {
                    var dbData = new GiftEntity(obj.Id, obj.To, obj.From, obj.Message, obj.Sn, obj.RingSourceId);
                    dbContext.Gifts.Add(dbData);
                }
            }

            await dbContext.SaveChangesAsync();
        }

        public override List<GiftModel> Query(Expression<Func<GiftModel, bool>> expression)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var entityExpression = _mapper.MapExpression<Expression<Func<GiftEntity, bool>>>(expression);

            return _mapper.Map<List<GiftModel>>(dbContext.Gifts.Where(entityExpression).ToList());
        }
    }
}
