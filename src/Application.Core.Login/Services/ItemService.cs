using Application.Core.EF.Entities.Items;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Constants.Item;
using Application.Shared.Items;
using Application.Shared.Message;
using AutoMapper;
using BaseProto;
using Dto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using ItemProto;
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

        public Dto.DropAllDto LoadMobDropDto()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var mobDrop = dbContext.DropData.Where(x => x.Chance >= 0).AsNoTracking().ToList();
            var globalDrop = dbContext.DropDataGlobals.Where(x => x.Chance >= 0).AsNoTracking().ToList();
            var data = new DropAllDto();
            data.Items.AddRange(_mapper.Map<Dto.DropItemDto[]>(mobDrop));
            data.Items.AddRange(_mapper.Map<Dto.DropItemDto[]>(globalDrop));
            return data;
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

        bool isLocked = false;
        public void BroadcastTV(ItemProto.CreateTVMessageRequest request)
        {
            if (isLocked)
            {
                _server.Transport.BroadcastMessage(BroadcastType.OnTVMessage, new ItemProto.CreateTVMessageResponse()
                {
                    Code = 1,
                    MasterId = request.MasterId,
                    Transaction = _server.ItemTransactionManager.CreateTransaction(request.Transaction, ItemTransactionStatus.PendingForRollback)
                });
                return;
            }

            var master = _server.CharacterManager.FindPlayerById(request.MasterId)!;

            var messageDto = new ItemProto.TVMessage { Master = _mapper.Map<Dto.PlayerViewDto>(master), Type = request.Type };
            var masterPartner = _server.CharacterManager.FindPlayerById(master.Character.PartnerId);
            if (masterPartner != null)
                messageDto.MasterPartner = _mapper.Map<Dto.PlayerViewDto>(masterPartner);

            messageDto.MessageList.AddRange(request.MessageList);

            var tsc = _server.ItemTransactionManager.CreateTransaction(request.Transaction, ItemTransactionStatus.PendingForCommit);
            var response = new ItemProto.CreateTVMessageResponse()
            {
                Code = 0,
                MasterId = master.Character.Id,
                ShowEar = request.ShowEar,
                Data = messageDto,
                Transaction = tsc
            };

            _server.Transport.BroadcastMessage(BroadcastType.OnTVMessage, response);
            isLocked = true;

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

        public void BroadcastItemMegaphone(ItemProto.UseItemMegaphoneRequest request)
        {
            var res = new ItemProto.UseItemMegaphoneResponse();

            var master = _server.CharacterManager.FindPlayerById(request.MasterId)!;

            _server.Transport.BroadcastMessage(BroadcastType.OnItemMegaphone, new ItemProto.UseItemMegaphoneResponse
            {
                Code = 0,
                Data = new ItemProto.UseItemMegaphoneResult
                {
                    IsWishper = request.IsWishper,
                    Item = request.Item,
                    Message = request.Message,
                    SenderChannel = master.Channel,
                    SenderId = master.Character.Id,
                }
            });
        }

        public QueryDropperByItemResponse LoadWhoDrops(QueryDropperByItemRequest request)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbList = dbContext.DropData.Where(x => x.Itemid == request.ItemId).Select(x => x.Dropperid).ToArray();
            var res = new QueryDropperByItemResponse();
            res.DropperIdList.AddRange(dbList);
            return res;

        }

        public QueryMonsterCardDataResponse LoadMonsterCard()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbList = dbContext.Monstercarddata.AsNoTracking().ToList();
            var res = new QueryMonsterCardDataResponse();
            res.List.AddRange(dbList.Select(x => new MonsterCardData { CardId = x.Cardid, MobId = x.Mobid} ));
            return res;
        }
    }
}
