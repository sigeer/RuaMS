using Application.Core.Login.Dtos.Drop;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Constants.Item;
using Application.Shared.Message;
using Application.Utility.Compatible.Atomics;
using Application.Utility.Extensions;
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


        public int[] LoadReactorSkillBooks()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Reactordrops.Where(x => x.Itemid >= ItemId.SKILLBOOK_MIN_ITEMID && x.Itemid < ItemId.SKILLBOOK_MAX_ITEMID)
            .Select(x => x.Itemid)
            .ToArray();
        }

        public ProtoModel.SpecialCashItemListProto LoadSpecialCashItems()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var data = new ProtoModel.SpecialCashItemListProto();
            data.Items.AddRange(_mapper.Map<ProtoModel.SpecialCashItemProto[]>(dbContext.Specialcashitems.AsNoTracking().ToList()));
            return data;
        }


        AtomicBoolean isLocked = new AtomicBoolean();
        public ProtoService.CreateTVMessageResponse BroadcastTV(ProtoService.CreateTVMessageRequest request)
        {
            if (isLocked)
            {
                return new ProtoService.CreateTVMessageResponse { Code = 1 };
            }

            var master = _server.CharacterManager.FindPlayerById(request.MasterId)!;
            var response = new ProtoModel.CreateTVMessageBroadcastProto()
            {
                Master = _mapper.Map<ProtoModel.PlayerViewProto>(master),
                Request = request,
            };
            //var masterPartner = _server.CharacterManager.FindPlayerById(master.Character.PartnerId);
            //if (masterPartner != null)
            //    response.MasterPartner = _mapper.Map<ProtoModel.PlayerViewProto>(masterPartner);

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
            return new ProtoService.CreateTVMessageResponse();
        }


        async Task BroadcastTVFinish()
        {
            isLocked.Set(false);
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleTVMessageFinish);
        }

        public ProtoService.UseItemMegaphoneResponse BroadcastItemMegaphone(ProtoService.UseItemMegaphoneRequest request)
        {
            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null || master.Channel <= 0)
            {
                return new ProtoService.UseItemMegaphoneResponse() { Code = 1 };
            }

            var res = new ProtoModel.UseItemMegaphoneBroadcastProto() { Request = request, MasterChannel = master.Channel };
            _ = _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleItemMegaphone, res);

            return new ProtoService.UseItemMegaphoneResponse();
        }

        public ProtoService.QueryDropperByItemResponse LoadWhoDrops(ProtoService.QueryDropperByItemRequest request)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbList = dbContext.DropData.Where(x => x.Itemid == request.ItemId).Select(x => x.Dropperid).ToArray();
            var res = new ProtoService.QueryDropperByItemResponse();
            res.DropperIdList.AddRange(dbList);
            return res;

        }

        public ProtoService.QueryMonsterCardDataResponse LoadMonsterCard()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbList = dbContext.Monstercarddata.AsNoTracking().ToList();
            var res = new ProtoService.QueryMonsterCardDataResponse();
            res.List.AddRange(dbList.Select(x => new ProtoModel.MonsterCardDataProto { CardId = x.Cardid, MobId = x.Mobid }));
            return res;
        }
    }
}
