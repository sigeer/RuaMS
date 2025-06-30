using Application.Core.EF.Entities.Items;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Constants.Item;
using Application.Shared.Items;
using Application.Shared.Message;
using AutoMapper;
using Dto;
using Google.Protobuf.WellKnownTypes;
using ItemDto;
using Microsoft.EntityFrameworkCore;
using ZLinq;

namespace Application.Core.Login.Services
{
    public class ItemService
    {
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;
        readonly MasterServer _server;

        public ItemService(IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, MasterServer server)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _server = server;
        }

        public Dto.DropAllDto LoadAllReactorDrops()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbData = dbContext.Reactordrops.Where(x => x.Chance >= 0).AsNoTracking().ToList();
            var data = new DropAllDto();
            data.Items.AddRange(_mapper.Map<Dto.DropItemDto[]>(dbData));
            return data;
        }

        public int[] LoadReactorSkillBooks()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Reactordrops.Where(x => x.Itemid >= ItemId.SKILLBOOK_MIN_ITEMID && x.Itemid < ItemId.SKILLBOOK_MAX_ITEMID)
            .Select(x => x.Itemid)
            .ToArray();
        }

        public Dto.SpecialCashItemListDto LoadSpecialCashItems()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var data = new Dto.SpecialCashItemListDto();
            data.Items.AddRange(_mapper.Map<Dto.SpecialCashItemDto[]>(dbContext.Specialcashitems.AsNoTracking().ToList()));
            return data;
        }

        public void InsertGift(int toId, string from, string message, int sn, long ringid = -1)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var giftModel = new GiftEntity(toId, from, message, sn, ringid);
            dbContext.Gifts.Add(giftModel);
            dbContext.SaveChanges();
        }

        public Dto.GiftDto[] LoadPlayerGifts(int playerId)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbData = (from a in dbContext.Gifts.AsNoTracking().Where(x => x.To == playerId)
                          join b in dbContext.Rings on a.Ringid equals b.Id into bss
                          from bs in bss.DefaultIfEmpty()
                          select new GiftRingPair(a, bs)).ToList();
            return _mapper.Map<Dto.GiftDto[]>(dbData);
        }

        public void ClearGifts(int[] giftIdArray)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Gifts.Where(x => giftIdArray.Contains(x.Id)).ExecuteDelete();
        }

        public int[] GetCardTierSize()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM monstercarddata GROUP BY floor(cardid / 1000);").ToArray();
        }

        bool isLocked = true;
        public void BroadcastTV(ItemDto.CreateTVMessageRequest request)
        {
            if (isLocked)
                return;

            var master = _server.CharacterManager.FindPlayerById(request.MasterId)!;

            var messageDto = new ItemDto.TVMessage { Master = _mapper.Map<Dto.PlayerViewDto>(master), Type = request.Type };
            var masterPartner = _server.CharacterManager.FindPlayerById(master.Character.PartnerId);
            if (masterPartner != null)
                messageDto.MasterPartner = _mapper.Map<Dto.PlayerViewDto>(masterPartner);

            messageDto.MessageList.AddRange(request.MessageList);

            var tsc = _server.ItemTransactionManager.CreateTransaction(request.Transaction, ItemTransactionStatus.PendingForCommit);
            var response = new ItemDto.CreateTVMessageResponse()
            {
                Code = 0,
                MasterId = master.Character.Id,
                ShowEar = request.ShowEar,
                Data = messageDto,
                Transaction = tsc
            };
           
            _server.Transport.BroadcastMessage(BroadcastType.OnTVMessage, response);

            int delay = 15;
            if (request.Type == 4)
            {
                delay = 30;
            }
            else if (request.Type == 5)
            {
                delay = 60;
            }
            _server.TimerManager.schedule(BroadcastTVFinish, TimeSpan.FromSeconds(delay));
        }

        void BroadcastTVFinish()
        {
            isLocked = false;
            _server.Transport.BroadcastMessage(BroadcastType.OnTVMessageFinish, new Empty());
        }

        public void BroadcastItemMegaphone(UseItemMegaphoneRequest request)
        {
            var res = new ItemDto.UseItemMegaphoneResponse();

            var master = _server.CharacterManager.FindPlayerById(request.MasterId)!;

            _server.Transport.BroadcastMessage(BroadcastType.OnItemMegaphone, new ItemDto.UseItemMegaphoneResponse
            {
                Code = 0,
                Data = new UseItemMegaphoneResult
                {
                    IsWishper = request.IsWishper,
                    Item = request.Item,
                    Message = request.Message,
                    SenderChannel = master.Channel,
                    SenderId = master.Character.Id,
                }
            });
        }
    }
}
