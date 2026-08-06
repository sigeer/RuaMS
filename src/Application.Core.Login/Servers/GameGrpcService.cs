using Application.Core.Login.Services;
using ItemService = Application.Core.Login.Services.ItemService;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Application.Core.Login.Servers
{
    internal class GameGrpcService : ProtoService.GameService.GameServiceBase
    {
        readonly MasterServer _server;
        readonly ItemService _itemService;
        readonly ShopManager _shopService;
        readonly InvitationService _invitationService;
        readonly IExpeditionService _expeditionService;
        readonly RankService _rankService;
        public GameGrpcService(MasterServer server, ItemService itemService, ShopManager shopService, InvitationService invitationService, IExpeditionService expeditionService, RankService rankService)
        {
            _server = server;
            _itemService = itemService;
            _shopService = shopService;
            _invitationService = invitationService;
            _expeditionService = expeditionService;
            _rankService = rankService;
        }

        public override Task<ProtoService.NameChangeResponse> ChangeName(ProtoService.NameChangeRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.CharacterManager.ChangeName(request));
        }

        public override Task<ProtoModel.BoolWrapper> CheckCharacterName(ProtoService.CheckCharacterNameRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ProtoModel.BoolWrapper { Value = _server.CharacterManager.CheckCharacterName(request.Name) });
        }

        public override Task<ProtoService.ExpeditionCheckResponse> CheckExpedition(ProtoService.ExpeditionCheckRequest request, ServerCallContext context)
        {
            return Task.FromResult(_expeditionService.CanStartExpedition(request));
        }

        public override Task<ProtoService.CommitRetrievedResponse> CommitRetrievedFromFredrick(ProtoService.CommitRetrievedRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerShopManager.CommitRetrieve(request));
        }

        public override Task<ProtoService.GetPLifeByMapIdResponse> GetLifeByMapId(ProtoService.GetPLifeByMapIdRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.ResourceDataManager.LoadMapPLife(request));
        }

        public override Task<ProtoService.GetAllPLifeResponse> GetAllPLife(ProtoService.GetAllPLifeRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.ResourceDataManager.GetAllPLife());
        }

        public override Task<ProtoService.GetShopResponse> GetShop(ProtoService.GetShopRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ProtoService.GetShopResponse { Data = _shopService.LoadFromDB(request.Id, request.IsShopId) });
        }

        public override Task<ProtoService.LoadCharacterRankResponse> LoadCharacterRank(ProtoService.LoadCharacterRankRequest request, ServerCallContext context)
        {
            return Task.FromResult(_rankService.LoadPlayerRanking(request.Count));
        }

        public override Task<ProtoModel.GacheponDataProto> LoadGachaponData(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.GachaponManager.GetGachaponData());
        }

        public override Task<ProtoModel.DropAllProto> LoadMobDropData(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.DropDataManager.LoadMobDropDto());
        }

        public override Task<ProtoModel.MonitorDataWrapperProto> LoadMonitor(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.SystemManager.LoadMonitorData());
        }

        public override Task<ProtoService.QueryMonsterCardDataResponse> LoadMonsterCardData(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_itemService.LoadMonsterCard());
        }

        public override Task<ProtoModel.RemoteHiredMerchantProto> LoadPlayerHiredMerchant(ProtoService.GetPlayerHiredMerchantRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerShopManager.GetPlayerHiredMerchant(request));
        }

        public override Task<ProtoModel.DropAllProto> LoadReactorDropData(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.DropDataManager.LoadAllReactorDrops());
        }

        public override Task<ProtoModel.ReactorSkillBookProto> LoadReactorSkillBookData(Empty request, ServerCallContext context)
        {
            var req = new ProtoModel.ReactorSkillBookProto();
            req.IdList.AddRange(_itemService.LoadReactorSkillBooks());
            return Task.FromResult(req);
        }


        public override Task<Empty> RegisterExpedition(ProtoModel.ExpeditionRegistry request, ServerCallContext context)
        {
            _expeditionService.RegisterExpedition(request);
            return Task.FromResult(new Empty());
        }

        public override Task<ProtoService.SetNoteReadResponse> SetNoteRead(ProtoService.SetNoteReadRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ProtoService.SetNoteReadResponse { Data = _server.NoteManager.SetRead(request.Id) });
        }

        public override Task<ProtoService.UseCdkResponse> UseCDK(ProtoService.UseCdkRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.RewardManager.UseCdk(request));
        }

        public override Task<ProtoModel.GetRewardsResponse> GetActiveRewards(ProtoModel.GetRewardsRequest request, ServerCallContext context)
        {
            return _server.RewardManager.GetActiveRewards(request);
        }

        public override Task<ProtoService.UseCdkResponse> TakeReward(ProtoService.UseIdRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.RewardManager.UseId(request));
        }

        public override async Task<ProtoModel.BoolWrapper> SendNote(ProtoService.SendNormalNoteRequest request, ServerCallContext context)
        {
            return new ProtoModel.BoolWrapper { Value = await _server.NoteManager.SendNormal(request.Message, request.FromId, request.ToName) };
        }

        #region PlayerNPC
        public override Task<Empty> CreatePlayerNPC(ProtoService.CreatePlayerNPCRequest request, ServerCallContext context)
        {
            _server.PlayerNPCManager.Create(request);
            return Task.FromResult(new Empty());
        }

        public override Task<ProtoService.CreatePlayerNPCPreResponse> CreatePlayerNPCCheck(ProtoService.CreatePlayerNPCPreRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerNPCManager.PreCreate(request));
        }

        public override Task<ProtoService.GetMapPlayerNPCListResponse> GetMapPlayerNPC(ProtoService.GetMapPlayerNPCListRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerNPCManager.GetMapData(request));
        }

        public override Task<ProtoService.GetAllPlayerNPCDataResponse> GetAllPlayerNPC(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_server.PlayerNPCManager.GetAllData());
        }

        public override Task<Empty> RemoveAll(Empty request, ServerCallContext context)
        {
            _server.PlayerNPCManager.RemoveAll();
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> RemoveByName(ProtoService.RemovePlayerNPCRequest request, ServerCallContext context)
        {
            _server.PlayerNPCManager.Remove(request);
            return Task.FromResult(new Empty());
        }
        #endregion
    }
}
