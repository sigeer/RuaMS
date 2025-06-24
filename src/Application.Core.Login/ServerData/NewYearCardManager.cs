using Application.Core.Login.Shared;
using Application.EF;
using Application.Shared.Constants;
using Application.Shared.NewYear;
using AutoMapper;
using Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Application.Core.Login.ServerData
{
    public class NewYearCardManager : StorageBase<int, NewYearCardModel>
    {
        readonly MasterServer _server;
        readonly IMapper _mapper;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        /// <summary>
        /// Key: Id
        /// </summary>
        ConcurrentDictionary<int, NewYearCardModel> _dataSource = [];
        int currentId = 1;

        public NewYearCardManager(MasterServer server, IMapper mapper, IDbContextFactory<DBContext> dbContextFactory)
        {
            _server = server;
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
        }

        public async Task Initialize(DBContext dbContext)
        {
            currentId = await dbContext.Newyears.MaxAsync(x => (int?)x.Id) ?? 0;
        }

        public NewYearCardModel? GetDataById(int id)
        {
            if (_dataSource.TryGetValue(id, out var d) && d != null)
                return d;

            using var dbContext = _dbContextFactory.CreateDbContext();
            d = (from a in dbContext.Newyears.Where(x => x.Id == id)
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
                 }).FirstOrDefault();
            _dataSource.TryAdd(id, d);
            return d;
        }

        /// <summary>
        /// 从数据库和内存中查询数据（内存中存在则以内存为主，否则读取数据库并且更新内存）
        /// </summary>
        /// <param name="efExpression"></param>
        /// <param name="cacheExpression"></param>
        /// <returns></returns>
        private List<NewYearCardModel> QueryData(Expression<Func<NewYearCardEntity, bool>> efExpression, Func<NewYearCardModel, bool> cacheExpression)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            // 从缓存中取出所有已加载的 Id
            var cachedIds = _dataSource.Keys.ToArray();

            // 查询数据库时排除缓存中的数据
            var coldData = _mapper.Map<List<NewYearCardModel>>(dbContext.Newyears
                .Where(efExpression)
                .Where(x => !cachedIds.Contains(x.Id))
                .AsNoTracking()
                .ToList());

            // 获取缓存中的数据
            var cachedData = _dataSource.Values
                .Where(cacheExpression)
                .ToList();

            // 同步数据库数据到缓存
            foreach (var data in coldData)
            {
                _dataSource.TryAdd(data.Id, data);
            }

            // 合并数据，业务统一处理
            return cachedData.Concat(coldData).ToList();
        }

        public List<NewYearCardModel> LoadPlayerNewYearCard(int chrId)
        {
            return QueryData(
                x => (x.SenderId == chrId || x.ReceiverId == chrId) && !x.ReceiverDiscard && !x.SenderDiscard,
                x => (x.SenderId == chrId || x.ReceiverId == chrId) && !x.ReceiverDiscard && !x.SenderDiscard).ToList();
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

            _dataSource[newCard.Id] = newCard;

            SetDirty(newCard.Id, newCard);

            _server.Transport.SendNewYearCards(new Dto.SendNewYearCardResponse { 
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

            SetDirty(card.Id, card);

            _server.Transport.SendNewYearCardReceived(
                new Dto.ReceiveNewYearCardResponse { 
                    Request = request, 
                    Code = (int)NewYearCardResponseCode.Success ,
                    Model = _mapper.Map<Dto.NewYearCardDto>(card)
                });
            return;
        }

        internal void NotifyNewYearCard()
        {
            var allData = QueryData(
                x => !x.Received && !x.SenderDiscard && !x.ReceiverDiscard,
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
                    cards.Remove(item);
                    SetDirty(item.Id, item);
                }
                response.UpdateList.AddRange(_mapper.Map<Dto.NewYearCardDto[]>(toRemove));
                _server.Transport.SendNewYearCardDiscard(response);
            }

        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, NewYearCardModel> updateData)
        {
            var dbData = await dbContext.Newyears.Where(x => updateData.Keys.Contains(x.Id)).ToListAsync();
            foreach (var item in updateData.Values)
            {
                var dbModel = dbData.FirstOrDefault(x => x.Id == item.Id);
                if (dbModel == null)
                {
                    dbModel = new NewYearCardEntity() { Id = item.Id };
                    dbContext.Newyears.Add(dbModel);
                }
                dbModel.SenderId = item.SenderId;
                dbModel.ReceiverId = item.ReceiverId;
                dbModel.TimeReceived = item.TimeReceived;
                dbModel.TimeSent = item.TimeSent;
                dbModel.ReceiverDiscard = item.ReceiverDiscard;
                dbModel.SenderDiscard = item.SenderDiscard;
                dbModel.Received = item.Received;
                dbModel.Message = item.Message;
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
