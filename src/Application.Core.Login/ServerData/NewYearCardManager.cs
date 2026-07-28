using Application.Core.Login.Shared;
using Application.EF;
using Application.Shared.Message;
using Application.Shared.NewYear;
using Application.Utility;
using Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.ServerData
{
    public class NewYearCardManager : DataStorageBase<int, Dto.NewYearCardDto, NewYearCardEntity>
    {
        readonly MasterServer _server;

        public NewYearCardManager(MasterServer server, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, ILogger<NewYearCardManager> logger)
            : base(StorageCategory.NewYearCard, dbContextFactory, mapper, logger)
        {
            _server = server;
        }

        protected override int GetKey(NewYearCardDto model) => model.Id;

        protected override NewYearCardDto MapModel(NewYearCardEntity entities)
        {
            var item = base.MapModel(entities);
            item.SenderName = _server.CharacterManager.GetPlayerName(item.SenderId);
            item.ReceiverName = _server.CharacterManager.GetPlayerName(item.ReceiverId);
            return item;
        }

        public Dto.NewYearCardDto? GetDataById(int id)
        {
            return Find(id);
        }


        public List<Dto.NewYearCardDto> LoadPlayerNewYearCard(int chrId)
        {
            return Query(x => (x.SenderId == chrId || x.ReceiverId == chrId) && !x.ReceiverDiscard && !x.SenderDiscard,
                x => (x.SenderId == chrId || x.ReceiverId == chrId) && !x.ReceiverDiscard && !x.SenderDiscard);
        }


        public async Task SendNewYearCard(Dto.SendNewYearCardRequest request)
        {
            var fromPlayer = _server.CharacterManager.FindPlayerById(request.FromId)!;

            var toPlayer = _server.CharacterManager.FindPlayerByName(request.ToName);
            if (toPlayer == null)
            {
                await _server.Transport.SendNewYearCards(new Dto.SendNewYearCardResponse { Code = 0x13, Request = request });
                return;
            }

            if (toPlayer.Character.Id == request.FromId)
            {
                await _server.Transport.SendNewYearCards(new Dto.SendNewYearCardResponse { Code = 0xF, Request = request });
                return;
            }

            var newCard = new Dto.NewYearCardDto()
            {
                Id = Interlocked.Increment(ref _localId),
                Message = request.Message,
                SenderId = request.FromId,
                SenderName = fromPlayer.Character.Name,
                ReceiverId = toPlayer.Character.Id,
                ReceiverName = toPlayer.Character.Name,
                TimeSent = _server.getCurrentTime(),
            };

            SetDirty(newCard);

            await _server.Transport.SendNewYearCards(new Dto.SendNewYearCardResponse
            {
                Code = 0,
                Request = request,
                Model = newCard
            });
        }

        public async Task ReceiveNewYearCard(Dto.ReceiveNewYearCardRequest request)
        {
            var res = new Dto.ReceiveNewYearCardResponse { Request = request };
            var card = GetDataById(request.CardId);
            if (card == null || card.SenderDiscard)
            {
                res.Code = (int)NewYearCardResponseCode.Receive_AlreadyDiscard;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnNewYearCardReceived, res, [res.Request.MasterId]);
                return;
            }

            if (card.ReceiverId != request.MasterId)
            {
                res.Code = (int)NewYearCardResponseCode.Receive_AlreadyDiscard;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnNewYearCardReceived, res, [res.Request.MasterId]);
                return;
            }

            if (card.Received)
            {
                res.Code = (int)NewYearCardResponseCode.Receive_AlreadReceived;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnNewYearCardReceived, res, [res.Request.MasterId]);
                return;
            }

            card.Received = true;
            card.TimeReceived = _server.getCurrentTime();

            SetDirty(card);
            res.Model = card;
            res.Code = (int)NewYearCardResponseCode.Success;

            await _server.Transport.SendMessageN(ChannelRecvCode.OnNewYearCardReceived, res, [res.Request.MasterId, res.Model.SenderId]);
        }

        internal async Task NotifyNewYearCard()
        {
            var allData = Query(
                x => !x.Received && !x.SenderDiscard && !x.ReceiverDiscard,
                x => !x.Received && !x.SenderDiscard && !x.ReceiverDiscard);

            var allUnReceivedCards = allData
                .GroupBy(x => x.ReceiverId)
                .ToDictionary(x => x.Key, x => x.ToList());
            var response = new Dto.NewYearCardNotifyDto();
            foreach (var data in allUnReceivedCards)
            {
                var item = new NewYearCardNotifyItem { MasterId = data.Key };
                item.List.AddRange(data.Value);
                response.List.Add(item);
            }

            await _server.Transport.SendNewYearCardNotify(response);
        }

        public async Task DiscardNewYearCard(Dto.DiscardNewYearCardRequest request)
        {
            var response = new Dto.DiscardNewYearCardResponse { Code = 0 };

            var cards = LoadPlayerNewYearCard(request.MasterId);

            List<Dto.NewYearCardDto> toRemove = [];
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
                    SetDirty(item);
                }
                response.UpdateList.AddRange(toRemove);
                await _server.Transport.SendNewYearCardDiscard(response);
            }

        }
    }
}
