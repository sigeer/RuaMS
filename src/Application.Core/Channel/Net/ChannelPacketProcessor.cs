using Application.Core.Channel.Net.Handlers;
using Microsoft.Extensions.DependencyInjection;
using net.server.handlers;

namespace Application.Core.Channel.Net
{
    public class ChannelPacketProcessor : IPacketProcessor<IChannelClient>
    {
        readonly Dictionary<short, IPacketHandlerBase<IChannelClient>> _dataSource;

        public ChannelPacketProcessor(IServiceProvider sp)
        {
            _dataSource = new Dictionary<short, IPacketHandlerBase<IChannelClient>>()
            {
                { (short)RecvOpcode.PONG, sp.GetRequiredService<KeepAliveHandler<IChannelClient>>() },
                { (short)RecvOpcode.CUSTOM_PACKET, sp.GetRequiredService<CustomPacketHandler<IChannelClient>>() },

                {(short)RecvOpcode.NAME_TRANSFER, sp.GetRequiredService<TransferNameHandler>()},
                {(short)RecvOpcode.CHECK_CHAR_NAME, sp.GetRequiredService<TransferNameResultHandler>()},
                {(short)RecvOpcode.WORLD_TRANSFER, sp.GetRequiredService<TransferWorldHandler>()},
                {(short)RecvOpcode.CHANGE_CHANNEL, sp.GetRequiredService<ChangeChannelHandler>()},
                { (short)RecvOpcode.STRANGE_DATA, sp.GetRequiredService<LoginRequiringNoOpHandler>() },
                {(short)RecvOpcode.GENERAL_CHAT, sp.GetRequiredService<GeneralChatHandler>()},
                {(short)RecvOpcode.WHISPER, sp.GetRequiredService<WhisperHandler>()},
                {(short)RecvOpcode.NPC_TALK, sp.GetRequiredService<NPCTalkHandler>()},
                {(short)RecvOpcode.NPC_TALK_MORE, sp.GetRequiredService<NPCMoreTalkHandler>()},
                {(short)RecvOpcode.QUEST_ACTION, sp.GetRequiredService<QuestActionHandler>()},
                {(short)RecvOpcode.GRENADE_EFFECT, sp.GetRequiredService<GrenadeEffectHandler>()},
                {(short)RecvOpcode.NPC_SHOP, sp.GetRequiredService<NPCShopHandler>()},
                {(short)RecvOpcode.ITEM_SORT, sp.GetRequiredService<InventoryMergeHandler>()},
                {(short)RecvOpcode.ITEM_MOVE, sp.GetRequiredService<ItemMoveHandler>()},
                {(short)RecvOpcode.MESO_DROP, sp.GetRequiredService<MesoDropHandler>()},
                {(short)RecvOpcode.PLAYER_LOGGEDIN, sp.GetRequiredService<PlayerLoggedinHandler>()},
                {(short)RecvOpcode.CHANGE_MAP, sp.GetRequiredService<ChangeMapHandler>()},
                {(short)RecvOpcode.MOVE_LIFE, sp.GetRequiredService<MoveLifeHandler>()},
                {(short)RecvOpcode.CLOSE_RANGE_ATTACK, sp.GetRequiredService<CloseRangeDamageHandler>()},
                {(short)RecvOpcode.RANGED_ATTACK, sp.GetRequiredService<RangedAttackHandler>()},
                {(short)RecvOpcode.MAGIC_ATTACK, sp.GetRequiredService<MagicDamageHandler>()},
                {(short)RecvOpcode.TAKE_DAMAGE, sp.GetRequiredService<TakeDamageHandler>()},
                {(short)RecvOpcode.MOVE_PLAYER, sp.GetRequiredService<MovePlayerHandler>()},
                {(short)RecvOpcode.USE_CASH_ITEM, sp.GetRequiredService<UseCashItemHandler>()},
                {(short)RecvOpcode.USE_ITEM, sp.GetRequiredService<UseItemHandler>()},
                {(short)RecvOpcode.USE_RETURN_SCROLL, sp.GetRequiredService<UseItemHandler>()},
                {(short)RecvOpcode.USE_UPGRADE_SCROLL, sp.GetRequiredService<ScrollHandler>()},
                {(short)RecvOpcode.USE_SUMMON_BAG, sp.GetRequiredService<UseSummonBagHandler>()},
                {(short)RecvOpcode.FACE_EXPRESSION, sp.GetRequiredService<FaceExpressionHandler>()},
                {(short)RecvOpcode.HEAL_OVER_TIME, sp.GetRequiredService<HealOvertimeHandler>()},
                {(short)RecvOpcode.ITEM_PICKUP, sp.GetRequiredService<ItemPickupHandler>()},
                {(short)RecvOpcode.CHAR_INFO_REQUEST, sp.GetRequiredService<CharInfoRequestHandler>()},
                {(short)RecvOpcode.SPECIAL_MOVE, sp.GetRequiredService<SpecialMoveHandler>()},
                {(short)RecvOpcode.USE_INNER_PORTAL, sp.GetRequiredService<InnerPortalHandler>()},
                {(short)RecvOpcode.CANCEL_BUFF, sp.GetRequiredService<CancelBuffHandler>()},
                {(short)RecvOpcode.CANCEL_ITEM_EFFECT, sp.GetRequiredService<CancelItemEffectHandler>()},
                {(short)RecvOpcode.PLAYER_INTERACTION, sp.GetRequiredService<PlayerInteractionHandler>()},
                {(short)RecvOpcode.RPS_ACTION, sp.GetRequiredService<RPSActionHandler>()},
                {(short)RecvOpcode.DISTRIBUTE_AP, sp.GetRequiredService<DistributeAPHandler>()},
                {(short)RecvOpcode.DISTRIBUTE_SP, sp.GetRequiredService<DistributeSPHandler>()},
                {(short)RecvOpcode.CHANGE_KEYMAP, sp.GetRequiredService<KeymapChangeHandler>()},
                {(short)RecvOpcode.CHANGE_MAP_SPECIAL, sp.GetRequiredService<ChangeMapSpecialHandler>()},
                {(short)RecvOpcode.STORAGE, sp.GetRequiredService<StorageHandler>()},
                {(short)RecvOpcode.GIVE_FAME, sp.GetRequiredService<GiveFameHandler>()},
                {(short)RecvOpcode.PARTY_OPERATION, sp.GetRequiredService<PartyOperationHandler>()},
                {(short)RecvOpcode.DENY_PARTY_REQUEST, sp.GetRequiredService<DenyPartyRequestHandler>()},
                {(short)RecvOpcode.MULTI_CHAT, sp.GetRequiredService<MultiChatHandler>()},
                {(short)RecvOpcode.USE_DOOR, sp.GetRequiredService<DoorHandler>()},
                {(short)RecvOpcode.ENTER_MTS, sp.GetRequiredService<EnterMTSHandler>()},
                {(short)RecvOpcode.ENTER_CASHSHOP, sp.GetRequiredService<EnterCashShopHandler>()},
                {(short)RecvOpcode.DAMAGE_SUMMON, sp.GetRequiredService<DamageSummonHandler>()},
                {(short)RecvOpcode.MOVE_SUMMON, sp.GetRequiredService<MoveSummonHandler>()},
                {(short)RecvOpcode.SUMMON_ATTACK, sp.GetRequiredService<SummonDamageHandler>()},
                {(short)RecvOpcode.BUDDYLIST_MODIFY, sp.GetRequiredService<BuddylistModifyHandler>()},
                {(short)RecvOpcode.USE_ITEMEFFECT, sp.GetRequiredService<UseItemEffectHandler>()},
                {(short)RecvOpcode.USE_CHAIR, sp.GetRequiredService<UseChairHandler>()},
                {(short)RecvOpcode.CANCEL_CHAIR, sp.GetRequiredService<CancelChairHandler>()},
                {(short)RecvOpcode.DAMAGE_REACTOR, sp.GetRequiredService<ReactorHitHandler>()},
                {(short)RecvOpcode.GUILD_OPERATION, sp.GetRequiredService<GuildOperationHandler>()},
                {(short)RecvOpcode.DENY_GUILD_REQUEST, sp.GetRequiredService<DenyGuildRequestHandler>()},
                {(short)RecvOpcode.SKILL_EFFECT, sp.GetRequiredService<SkillEffectHandler>()},
                {(short)RecvOpcode.MESSENGER, sp.GetRequiredService<MessengerHandler>()},
                {(short)RecvOpcode.NPC_ACTION, sp.GetRequiredService<NPCAnimationHandler>()},
                {(short)RecvOpcode.CHECK_CASH, sp.GetRequiredService<TouchingCashShopHandler>()},
                {(short)RecvOpcode.CASHSHOP_OPERATION, sp.GetRequiredService<CashOperationHandler>()},
                {(short)RecvOpcode.COUPON_CODE, sp.GetRequiredService<CouponCodeHandler>()},
                {(short)RecvOpcode.SPAWN_PET, sp.GetRequiredService<SpawnPetHandler>()},
                {(short)RecvOpcode.MOVE_PET, sp.GetRequiredService<MovePetHandler>()},
                {(short)RecvOpcode.PET_CHAT, sp.GetRequiredService<PetChatHandler>()},
                {(short)RecvOpcode.PET_COMMAND, sp.GetRequiredService<PetCommandHandler>()},
                {(short)RecvOpcode.PET_FOOD, sp.GetRequiredService<PetFoodHandler>()},
                {(short)RecvOpcode.PET_LOOT, sp.GetRequiredService<PetLootHandler>()},
                {(short)RecvOpcode.AUTO_AGGRO, sp.GetRequiredService<AutoAggroHandler>()},
                {(short)RecvOpcode.MONSTER_BOMB, sp.GetRequiredService<MonsterBombHandler>()},
                {(short)RecvOpcode.CANCEL_DEBUFF, sp.GetRequiredService<CancelDebuffHandler>()},
                {(short)RecvOpcode.USE_SKILL_BOOK, sp.GetRequiredService<SkillBookHandler>()},
                {(short)RecvOpcode.SKILL_MACRO, sp.GetRequiredService<SkillMacroHandler>()},
                {(short)RecvOpcode.NOTE_ACTION, sp.GetRequiredService<NoteActionHandler>()},
                {(short)RecvOpcode.CLOSE_CHALKBOARD, sp.GetRequiredService<CloseChalkboardHandler>()},
                {(short)RecvOpcode.USE_MOUNT_FOOD, sp.GetRequiredService<UseMountFoodHandler>()},

                {(short)RecvOpcode.PET_AUTO_POT, sp.GetRequiredService<PetAutoPotHandler>()},
                {(short)RecvOpcode.PET_EXCLUDE_ITEMS, sp.GetRequiredService<PetExcludeItemsHandler>()},
                {(short)RecvOpcode.OWL_ACTION, sp.GetRequiredService<UseOwlOfMinervaHandler>()},
                {(short)RecvOpcode.OWL_WARP, sp.GetRequiredService<OwlWarpHandler>()},
                {(short)RecvOpcode.TOUCH_MONSTER_ATTACK, sp.GetRequiredService<TouchMonsterDamageHandler>()},
                {(short)RecvOpcode.TROCK_ADD_MAP, sp.GetRequiredService<TrockAddMapHandler>()},
                {(short)RecvOpcode.HIRED_MERCHANT_REQUEST, sp.GetRequiredService<HiredMerchantRequest>()},
                {(short)RecvOpcode.MOB_BANISH_PLAYER, sp.GetRequiredService<MobBanishPlayerHandler>()},
                {(short)RecvOpcode.MOB_DAMAGE_MOB, sp.GetRequiredService<MobDamageMobHandler>()},
                {(short)RecvOpcode.REPORT, sp.GetRequiredService<ReportHandler>()},
                {(short)RecvOpcode.MONSTER_BOOK_COVER, sp.GetRequiredService<MonsterBookCoverHandler>()},
                {(short)RecvOpcode.AUTO_DISTRIBUTE_AP, sp.GetRequiredService<AutoAssignHandler>()},

                {(short)RecvOpcode.USE_HAMMER, sp.GetRequiredService<UseHammerHandler>()},
                {(short)RecvOpcode.SCRIPTED_ITEM, sp.GetRequiredService<ScriptedItemHandler>()},
                {(short)RecvOpcode.TOUCHING_REACTOR, sp.GetRequiredService<TouchReactorHandler>()},
                {(short)RecvOpcode.BEHOLDER, sp.GetRequiredService<BeholderHandler>()},
                {(short)RecvOpcode.ADMIN_COMMAND, sp.GetRequiredService<AdminCommandHandler>()},
                {(short)RecvOpcode.ADMIN_LOG, sp.GetRequiredService<AdminLogHandler>()},
                {(short)RecvOpcode.ALLIANCE_OPERATION, sp.GetRequiredService<AllianceOperationHandler>()},
                {(short)RecvOpcode.DENY_ALLIANCE_REQUEST, sp.GetRequiredService<DenyAllianceRequestHandler>()},
                {(short)RecvOpcode.USE_SOLOMON_ITEM, sp.GetRequiredService<UseSolomonHandler>()},
                {(short)RecvOpcode.USE_GACHA_EXP, sp.GetRequiredService<UseGachaExpHandler>()},
                {(short)RecvOpcode.NEW_YEAR_CARD_REQUEST, sp.GetRequiredService<NewYearCardHandler>()},
                {(short)RecvOpcode.CASHSHOP_SURPRISE, sp.GetRequiredService<CashShopSurpriseHandler>()},
                {(short)RecvOpcode.USE_ITEM_REWARD, sp.GetRequiredService<ItemRewardHandler>()},
                {(short)RecvOpcode.USE_REMOTE, sp.GetRequiredService<RemoteGachaponHandler>()},

                {(short)RecvOpcode.USE_DEATHITEM, sp.GetRequiredService<UseDeathItemHandler>()},

                {(short)RecvOpcode.PLAYER_MAP_TRANSFER, sp.GetRequiredService<PlayerMapTransitionHandler>()},
                {(short)RecvOpcode.USE_MAPLELIFE, sp.GetRequiredService<UseMapleLifeHandler>()},
                {(short)RecvOpcode.USE_CATCH_ITEM, sp.GetRequiredService<UseCatchItemHandler>()},
                {(short)RecvOpcode.FIELD_DAMAGE_MOB, sp.GetRequiredService<FieldDamageMobHandler>()},
                {(short)RecvOpcode.MOB_DAMAGE_MOB_FRIENDLY, sp.GetRequiredService<MobDamageMobFriendlyHandler>()},
                {(short)RecvOpcode.PARTY_SEARCH_REGISTER, sp.GetRequiredService<PartySearchRegisterHandler>()},
                {(short)RecvOpcode.PARTY_SEARCH_START, sp.GetRequiredService<PartySearchStartHandler>()},
                {(short)RecvOpcode.PARTY_SEARCH_UPDATE, sp.GetRequiredService<PartySearchUpdateHandler>()},
                {(short)RecvOpcode.ITEM_SORT2, sp.GetRequiredService<InventorySortHandler>()},
                {(short)RecvOpcode.LEFT_KNOCKBACK, sp.GetRequiredService<LeftKnockbackHandler>()},
                {(short)RecvOpcode.SNOWBALL, sp.GetRequiredService<SnowballHandler>()},
                {(short)RecvOpcode.COCONUT, sp.GetRequiredService<CoconutHandler>()},
                {(short)RecvOpcode.ARAN_COMBO_COUNTER, sp.GetRequiredService<AranComboHandler>()},
                {(short)RecvOpcode.CLICK_GUIDE, sp.GetRequiredService<ClickGuideHandler>()},
                {(short)RecvOpcode.FREDRICK_ACTION, sp.GetRequiredService<FredrickHandler>()},
                {(short)RecvOpcode.MONSTER_CARNIVAL, sp.GetRequiredService<MonsterCarnivalHandler>()},
                {(short)RecvOpcode.REMOTE_STORE, sp.GetRequiredService<RemoteStoreHandler>()},
                {(short)RecvOpcode.WATER_OF_LIFE, sp.GetRequiredService<UseWaterOfLifeHandler>()},
                {(short)RecvOpcode.ADMIN_CHAT, sp.GetRequiredService<AdminChatHandler>()},
                {(short)RecvOpcode.MOVE_DRAGON, sp.GetRequiredService<MoveDragonHandler>()},
                {(short)RecvOpcode.OPEN_ITEMUI, sp.GetRequiredService<RaiseUIStateHandler>()},
                {(short)RecvOpcode.USE_ITEMUI, sp.GetRequiredService<RaiseIncExpHandler>()},
                {(short)RecvOpcode.CHANGE_QUICKSLOT, sp.GetRequiredService<QuickslotKeyMappedModifiedHandler>()},
            };
        }

        public IPacketHandlerBase<IChannelClient>? GetPacketHandler(short code)
        {
            return _dataSource.GetValueOrDefault(code);
        }

        public virtual void TryAddHandler(short code, IPacketHandlerBase<IChannelClient> handler)
        {
            _dataSource.TryAdd(code, handler);
        }
    }
}
