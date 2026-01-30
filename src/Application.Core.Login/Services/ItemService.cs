using Application.EF;
using Application.Shared.Constants.Item;
using Application.Shared.Message;
using Application.Utility.Compatible.Atomics;
using AutoMapper;
using BaseProto;
using Dto;
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

        public CashProto.SpecialCashItemListDto LoadSpecialCashItems()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var data = new CashProto.SpecialCashItemListDto();
            data.Items.AddRange(_mapper.Map<CashProto.SpecialCashItemDto[]>(dbContext.Specialcashitems.AsNoTracking().ToList()));
            return data;
        }


        AtomicBoolean isLocked = new AtomicBoolean();
        public ItemProto.CreateTVMessageResponse BroadcastTV(ItemProto.CreateTVMessageRequest request)
        {
            if (isLocked)
            {
                return new CreateTVMessageResponse { Code = 1 };
            }

            var master = _server.CharacterManager.FindPlayerById(request.MasterId)!;
            var response = new ItemProto.CreateTVMessageBroadcast()
            {
                Master = _mapper.Map<Dto.PlayerViewDto>(master),
                Request = request,
            };
            var masterPartner = _server.CharacterManager.FindPlayerById(master.Character.PartnerId);
            if (masterPartner != null)
                response.MasterPartner = _mapper.Map<Dto.PlayerViewDto>(masterPartner);

            _ = _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleTVMessageStart, response);
            isLocked.Set(true);

            int delay = 15;
            if (request.Type == 4)
            {
                delay = 30;
            }
            else if (request.Type == 5)
            {
                delay = 60;
            }
            _ = _server.TimerManager.ScheduleAsync("TV", BroadcastTVFinish, TimeSpan.FromSeconds(delay));
            return new CreateTVMessageResponse();
        }


        async Task BroadcastTVFinish()
        {
            isLocked.Set(false);
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleTVMessageFinish);
        }

        public ItemProto.UseItemMegaphoneResponse BroadcastItemMegaphone(ItemProto.UseItemMegaphoneRequest request)
        {
            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null || master.Channel <= 0)
            {
                return new UseItemMegaphoneResponse() { Code = 1 };
            }

            var res = new UseItemMegaphoneBroadcast() { Request = request, MasterChannel = master.Channel };
            _ = _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleItemMegaphone, res);

            return new UseItemMegaphoneResponse();
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
            res.List.AddRange(dbList.Select(x => new MonsterCardData { CardId = x.Cardid, MobId = x.Mobid }));
            return res;
        }
    }
}
