using Application.Core.Login.Services;
using Application.Shared.Message;
using BaseProto;
using Config;
using Dto;
using ExpeditionProto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InvitationProto;
using ItemProto;
using LifeProto;
using MessageProto;
using RankProto;

namespace Application.Core.Login.Servers
{
    internal class GameGrpcService : ServiceProto.GameService.GameServiceBase
    {
        readonly MasterServer _server;
        readonly ItemService _itemService;
        readonly ShopService _shopService;
        readonly InvitationService _invitationService;
        readonly IExpeditionService _expeditionService;
        readonly RankService _rankService;
        public GameGrpcService(MasterServer server, ItemService itemService, ShopService shopService, InvitationService invitationService, IExpeditionService expeditionService, RankService rankService)
        {
            _server = server;
            _itemService = itemService;
            _shopService = shopService;
            _invitationService = invitationService;
            _expeditionService = expeditionService;
            _rankService = rankService;
        }


        public override Task<CanHiredMerchantResponse> CanHiredMerchant(CanHiredMerchantRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerShopManager.CanHiredMerchant(request));
        }

        public override Task<NameChangeResponse> ChangeName(NameChangeRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CharacterManager.ChangeName(request));
        }

        public override Task<BoolWrapper> CheckCharacterName(ServiceProto.CheckCharacterNameRequest request, ServerCallContext context)
        {
            return Task.FromResult(new BoolWrapper { Value = _server.CharacterManager.CheckCharacterName(request.Name) });
        }

        public override Task<ExpeditionCheckResponse> CheckExpedition(ExpeditionCheckRequest request, ServerCallContext context)
        {
            return Task.FromResult(_expeditionService.CanStartExpedition(request));
        }

        public override Task<CommitRetrievedResponse> CommitRetrievedFromFredrick(CommitRetrievedRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerShopManager.CommitRetrieve(request));
        }

        public override Task<GetPLifeByMapIdResponse> GetLifeByMapId(GetPLifeByMapIdRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.ResourceDataManager.LoadMapPLife(request));
        }

        public override Task<GetAllPLifeResponse> GetAllPLife(GetAllPLifeRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.ResourceDataManager.GetAllPLife());
        }

        public override Task<GetShopResponse> GetShop(GetShopRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetShopResponse { Data = _shopService.LoadFromDB(request.Id, request.IsShopId) });
        }

        public override Task<LoadCharacterRankResponse> LoadCharacterRank(LoadCharacterRankRequest request, ServerCallContext context)
        {
            return Task.FromResult(_rankService.LoadPlayerRanking(request.Count));
        }

        public override Task<GacheponDataDto> LoadGachaponData(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.GachaponManager.GetGachaponData());
        }

        public override Task<DropAllDto> LoadMobDropData(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_itemService.LoadMobDropDto());
        }

        public override Task<MonitorDataWrapper> LoadMonitor(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.SystemManager.LoadMonitorData());
        }

        public override Task<QueryMonsterCardDataResponse> LoadMonsterCardData(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_itemService.LoadMonsterCard());
        }

        public override Task<RemoteHiredMerchantDto> LoadPlayerHiredMerchant(GetPlayerHiredMerchantRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerShopManager.GetPlayerHiredMerchant(request));
        }

        public override Task<DropAllDto> LoadReactorDropData(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_itemService.LoadAllReactorDrops());
        }

        public override Task<ReactorSkillBookDto> LoadReactorSkillBookData(Empty request, ServerCallContext context)
        {
            var req = new ReactorSkillBookDto();
            req.IdList.AddRange(_itemService.LoadReactorSkillBooks());
            return Task.FromResult(req);
        }


        public override Task<Empty> RegisterExpedition(ExpeditionRegistry request, ServerCallContext context)
        {
            _expeditionService.RegisterExpedition(request);
            return Task.FromResult(new Empty());
        }

        public override Task<SetNoteReadResponse> SetNoteRead(SetNoteReadRequest request, ServerCallContext context)
        {
            return Task.FromResult(new SetNoteReadResponse { Data = _server.NoteManager.SetRead(request.Id) });
        }

        public override Task<UseCdkResponse> UseCDK(UseCdkRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CDKManager.UseCdk(request));
        }

        #region PlayerNPC
        public override Task<Empty> CreatePlayerNPC(CreatePlayerNPCRequest request, ServerCallContext context)
        {
            _server.PlayerNPCManager.Create(request);
            return Task.FromResult(new Empty());
        }

        public override Task<CreatePlayerNPCPreResponse> CreatePlayerNPCCheck(CreatePlayerNPCPreRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerNPCManager.PreCreate(request));
        }

        public override Task<GetMapPlayerNPCListResponse> GetMapPlayerNPC(GetMapPlayerNPCListRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerNPCManager.GetMapData(request));
        }

        public override Task<GetAllPlayerNPCDataResponse> GetAllPlayerNPC(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerNPCManager.GetAllData());
        }

        public override Task<Empty> RemoveAll(Empty request, ServerCallContext context)
        {
            _server.PlayerNPCManager.RemoveAll();
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> RemoveByName(RemovePlayerNPCRequest request, ServerCallContext context)
        {
            _server.PlayerNPCManager.Remove(request);
            return Task.FromResult(new Empty());
        }
        #endregion
    }
}
