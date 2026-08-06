using Application.Core.EF.Entities;
using Application.Core.EF.Entities.Gachapons;
using Application.Core.Login.Dtos.Account;
using Application.Core.Login.Dtos.Ban;
using Application.Core.Login.Dtos.CDK;
using Application.Core.Login.Dtos.Character;
using Application.Core.Login.Dtos.Drop;
using Application.Core.Login.Dtos.Gachapon;
using Application.Core.Login.Dtos.Item;
using Application.Core.Login.Dtos.Report;
using Application.Core.Login.Dtos.Shop;
using Application.Core.Login.Models;
using Application.EF;
using Application.EF.Entities;
using Application.Utility.Extensions;

namespace Application.Core.Login.Mappers
{
    internal class DtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ShopEntity, ShopResponseDto>().TwoWays();

            config.NewConfig<ShopItemEntity, ShopItemResponseDto>()
                .Map(dest => dest.ShopId, src => src.Shopid)
                .Map(dest => dest.Id, src => src.Shopitemid);

            config.NewConfig<DropDataEntity, DropResponseDto>()
                .Map(dest => dest.DropperId, src => src.Dropperid)
                .Map(dest => dest.MinCount, src => src.MinimumQuantity)
                .Map(dest => dest.MaxCount, src => src.MaximumQuantity)
                .Map(dest => dest.QuestId, src => src.Questid)
                .Map(dest => dest.ItemId, src => src.Itemid);

            config.NewConfig<DropDataGlobalEntity, DropResponseDto>()
                .Map(dest => dest.DropperId, src => src.Continent)
                .Map(dest => dest.MinCount, src => src.MinimumQuantity)
                .Map(dest => dest.MaxCount, src => src.MaximumQuantity)
                .Map(dest => dest.QuestId, src => src.Questid)
                .Map(dest => dest.ItemId, src => src.Itemid);

            config.NewConfig<ReactorDropEntity, DropResponseDto>()
                .Map(dest => dest.DropperId, src => src.Reactorid)
                .Map(dest => dest.MinCount, src => 1)
                .Map(dest => dest.MaxCount, src => 1)
                .Map(dest => dest.QuestId, src => src.Questid)
                .Map(dest => dest.ItemId, src => src.Itemid);

            #region Cdk
            config.NewConfig<RewardEntity, RewardResponseDto>();
            config.NewConfig<RewardEntity, RewardDetailResponseDto>();

            config.NewConfig<RewardItemEntity, RewardItemResponseDto>();

            config.NewConfig<CdkRecordModel, RewardRecordResponseDto>();
            #endregion

            config.NewConfig<GachaponPoolEntity, GachaponResponseDto>().TwoWays();
            config.NewConfig<GachaponPoolItemEntity, GachaponItemResponseDto>().TwoWays();
            config.NewConfig<GachaponPoolLevelChanceEntity, GachaponSettingResponseDto>().TwoWays();

            config.NewConfig<AccountCtrl, AccountPreviewResponseDto>();
            config.NewConfig<AccountCtrl, AccountResponseDto>();

            config.NewConfig<AccountBanEntity, BanResponseDto>();

            config.NewConfig<CharacterEntity, CharacterResponseDto>()
                .Map(dest => dest.JobName, src => JobFactory.GetById(src.JobId).name());

            config.NewConfig<ReportEntity, ReportResponseDto>();

            config.NewConfig<CreateItemRequestDto, ProtoModel.ItemProto>()
                .Map(dest => dest.Itemid, src => src.ItemId)
                .Map(dest => dest.Quantity, src => src.Quantity)
                .Map(dest => dest.Flag, src => src.Flag)
                .Map(dest => dest.EquipInfo, src => src.EquipInfo);
            config.NewConfig<CreateEquipRequestDto, ProtoModel.EquipProto>();
        }
    }
}
