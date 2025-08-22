using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.Shared.Constants;
using Application.Shared.NewYear;
using Application.Utility;
using Dto;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Core.Login.ServerData
{
    public class NewYearCardManager : StorageBase<int, NewYearCardModel>
    {
        readonly MasterServer _server;
        readonly IMapper _mapper;
        readonly IDbContextFactory<DBContext> _dbContextFactory;

        int currentId = 1;

        public NewYearCardManager(MasterServer server, IMapper mapper, IDbContextFactory<DBContext> dbContextFactory)
        {
            _server = server;
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
        }

        public override async Task InitializeAsync(DBContext dbContext)
        {
            currentId = await dbContext.Newyears.MaxAsync(x => (int?)x.Id) ?? 0;
        }

        public override List<NewYearCardModel> Query(Expression<Func<NewYearCardModel, bool>> expression)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dataFromDB = (from a in dbContext.Newyears.AsNoTracking().ProjectToType<NewYearCardModel>().Where(expression)
                              join b in dbContext.Characters on a.SenderId equals b.Id into bss
                              from bs in bss.DefaultIfEmpty()
                              join c in dbContext.Characters on a.ReceiverId equals c.Id into css
                              from cs in css
                              select new NewYearCardModel
                              {
                                  Id = a.Id,
                                  SenderId = a.SenderId,
                                  ReceiverId = a.ReceiverId,
                                  SenderName = bs == null ? StringConstants.CharacterUnknown : bs.Name,
                                  ReceiverName = cs == null ? StringConstants.CharacterUnknown : cs.Name,
                                  SenderDiscard = a.SenderDiscard,
                                  Message = a.Message,
                                  Received = a.Received,
                                  ReceiverDiscard = a.ReceiverDiscard,
                                  TimeReceived = a.TimeReceived,
                                  TimeSent = a.TimeSent
                              }).ToList();

            return QueryWithDirty(dataFromDB, expression.Compile());
        }

        public NewYearCardModel? GetDataById(int id)
        {
            return Query(x => x.Id == id).FirstOrDefault();
        }


        public List<NewYearCardModel> LoadPlayerNewYearCard(int chrId)
        {
            return Query(x => (x.SenderId == chrId || x.ReceiverId == chrId) && !x.ReceiverDiscard && !x.SenderDiscard);
        }


        public void SendNewYearCard(Dto.SendNewYearCardRequest request)
        {
            var fromPlayer = _server.CharacterManager.FindPlayerById(request.FromId)!;

            var toPlayer = _server.CharacterManager.FindPlayerByName(request.ToName);
            if (toPlayer == null)
            {
                _server.Transport.SendNewYearCards(new Dto.SendNewYearCardResponse { Code = 0x13, Request = request });
                return;
            }

            if (toPlayer.Character.Id == request.FromId)
            {
                _server.Transport.SendNewYearCards(new Dto.SendNewYearCardResponse { Code = 0xF, Request = request });
                return;
            }

            var newCard = new NewYearCardModel()
            {
                Id = Interlocked.Increment(ref currentId),
                Message = request.Message,
                SenderId = request.FromId,
                SenderName = fromPlayer.Character.Name,
                ReceiverId = toPlayer.Character.Id,
                ReceiverName = toPlayer.Character.Name,
                TimeSent = _server.getCurrentTime(),
            };

            SetDirty(newCard.Id, new Utility.StoreUnit<NewYearCardModel>(Utility.StoreFlag.AddOrUpdate, newCard));

            _server.Transport.SendNewYearCards(new Dto.SendNewYearCardResponse
            {
                Code = 0,
                Request = request,
                Model = _mapper.Map<Dto.NewYearCardDto>(newCard)
            });
        }

        public void ReceiveNewYearCard(Dto.ReceiveNewYearCardRequest request)
        {
            var card = GetDataById(request.CardId);
            if (card == null || card.SenderDiscard)
            {
                _server.Transport.SendNewYearCardReceived(new Dto.ReceiveNewYearCardResponse { Request = request, Code = (int)NewYearCardResponseCode.Receive_AlreadyDiscard });
                return;
            }

            if (card.ReceiverId == request.MasterId)
            {
                _server.Transport.SendNewYearCardReceived(new Dto.ReceiveNewYearCardResponse { Request = request, Code = (int)NewYearCardResponseCode.Receive_AlreadyDiscard });
                return;
            }

            if (card.Received)
            {
                _server.Transport.SendNewYearCardReceived(new Dto.ReceiveNewYearCardResponse { Request = request, Code = (int)NewYearCardResponseCode.Receive_AlreadReceived });
                return;
            }

            card.Received = true;
            card.TimeReceived = _server.getCurrentTime();

            SetDirty(card.Id, new Utility.StoreUnit<NewYearCardModel>(Utility.StoreFlag.AddOrUpdate, card));

            _server.Transport.SendNewYearCardReceived(
                new Dto.ReceiveNewYearCardResponse
                {
                    Request = request,
                    Code = (int)NewYearCardResponseCode.Success,
                    Model = _mapper.Map<Dto.NewYearCardDto>(card)
                });
            return;
        }

        internal void NotifyNewYearCard()
        {
            var allData = Query(
                x => !x.Received && !x.SenderDiscard && !x.ReceiverDiscard);

            var allUnReceivedCards = allData
                .GroupBy(x => x.ReceiverId)
                .ToDictionary(x => x.Key, x => _mapper.Map<Dto.NewYearCardDto[]>(x.ToArray()));
            var response = new Dto.NewYearCardNotifyDto();
            foreach (var data in allUnReceivedCards)
            {
                var item = new NewYearCardNotifyItem { MasterId = data.Key };
                item.List.AddRange(data.Value);
                response.List.Add(item);
            }

            _server.Transport.SendNewYearCardNotify(response);
        }

        public void DiscardNewYearCard(Dto.DiscardNewYearCardRequest request)
        {
            var response = new Dto.DiscardNewYearCardResponse { Code = 0 };

            var cards = LoadPlayerNewYearCard(request.MasterId);

            List<NewYearCardModel> toRemove = [];
            foreach (var card in cards)
            {
                if (request.IsSender && card.SenderId == request.MasterId)
                {
                    card.SenderDiscard = true;
                    card.Received = false;
                    toRemove.Add(card);
                }

                if (!request.IsSender && card.ReceiverId == request.MasterId)
                {
                    card.ReceiverDiscard = true;
                    card.Received = false;
                    toRemove.Add(card);
                }
            }

            if (toRemove.Count > 0)
            {
                foreach (var item in toRemove)
                {
                    SetRemoved(item.Id);
                }
                response.UpdateList.AddRange(_mapper.Map<Dto.NewYearCardDto[]>(toRemove));
                _server.Transport.SendNewYearCardDiscard(response);
            }

        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<NewYearCardModel>> updateData)
        {
            await dbContext.Newyears.Where(x => updateData.Keys.Contains(x.Id)).ExecuteDeleteAsync();
            foreach (var kv in updateData)
            {
                var item = kv.Value.Data;
                if (kv.Value.Flag == StoreFlag.AddOrUpdate && item != null)
                {
                    var dbModel = new NewYearCardEntity() { Id = item.Id };
                    dbModel.SenderId = item.SenderId;
                    dbModel.ReceiverId = item.ReceiverId;
                    dbModel.TimeReceived = item.TimeReceived;
                    dbModel.TimeSent = item.TimeSent;
                    dbModel.ReceiverDiscard = item.ReceiverDiscard;
                    dbModel.SenderDiscard = item.SenderDiscard;
                    dbModel.Received = item.Received;
                    dbModel.Message = item.Message;
                    dbContext.Newyears.Add(dbModel);
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
