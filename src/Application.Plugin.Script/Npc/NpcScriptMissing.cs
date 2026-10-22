using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Npc
{
    /// <summary>
    /// 补齐 WZ 中声明、但 JS 与 C# 两侧均无实现的NPC脚本。
    /// 占位方法抛出 <see cref="NotImplementedException"/>，由 PluginManager 统一处理。
    /// </summary>
    internal class NpcScriptMissing : NpcScriptBase
{
        public NpcScriptMissing(IChannelClient c, int npcId, NPC? npcObj) : base(c, npcId, npcObj)
        {
        }

        private Task NotImplemented() => throw new NotImplementedException();

        [ScriptName("BF_master")]
        public Task n_BF_master() => NotImplemented();

        [ScriptName("BF_sheep")]
        public Task n_BF_sheep() => NotImplemented();

        [ScriptName("BF_wolf")]
        public Task n_BF_wolf() => NotImplemented();

        [ScriptName("CN_HontaleReward")]
        public Task n_CN_HontaleReward() => NotImplemented();

        [ScriptName("CakeCandle_out")]
        public Task n_CakeCandle_out() => NotImplemented();

        [ScriptName("CandleEvent")]
        public Task n_CandleEvent() => NotImplemented();

        [ScriptName("CrimsonStoryH")]
        public Task n_CrimsonStoryH() => NotImplemented();

        [ScriptName("CrimsonStoryL")]
        public Task n_CrimsonStoryL() => NotImplemented();

        [ScriptName("Danwou")]
        public Task n_Danwou() => NotImplemented();

        [ScriptName("Disguised")]
        public Task n_Disguised() => NotImplemented();

        [ScriptName("EngageRing")]
        public Task n_EngageRing() => NotImplemented();

        [ScriptName("Event10")]
        public Task n_Event10() => NotImplemented();

        [ScriptName("HLMS")]
        public Task n_HLMS() => NotImplemented();

        [ScriptName("HighPriest")]
        public Task n_HighPriest() => NotImplemented();

        [ScriptName("Jack_Additional")]
        public Task n_Jack_Additional() => NotImplemented();

        [ScriptName("Jump_event_out")]
        public Task n_Jump_event_out() => NotImplemented();

        [ScriptName("KARMA_List")]
        public Task n_KARMA_List() => NotImplemented();

        [ScriptName("M_info")]
        public Task n_M_info() => NotImplemented();

        [ScriptName("Manji")]
        public Task n_Manji() => NotImplemented();

        [ScriptName("MeetPare")]
        public Task n_MeetPare() => NotImplemented();

        [ScriptName("NewworldEvent")]
        public Task n_NewworldEvent() => NotImplemented();

        [ScriptName("PB_bossOut")]
        public Task n_PB_bossOut() => NotImplemented();

        [ScriptName("PRaid_enter")]
        public Task n_PRaid_enter() => NotImplemented();

        [ScriptName("Pachinko_dama_machine")]
        public Task n_Pachinko_dama_machine() => NotImplemented();

        [ScriptName("Pachinko_into")]
        public Task n_Pachinko_into() => NotImplemented();

        [ScriptName("Pachinko_machine")]
        public Task n_Pachinko_machine() => NotImplemented();

        [ScriptName("Pachinko_reward")]
        public Task n_Pachinko_reward() => NotImplemented();

        [ScriptName("ProofHene")]
        public Task n_ProofHene() => NotImplemented();

        [ScriptName("Warehouse")]
        public Task n_Warehouse() => NotImplemented();

        [ScriptName("abouttheWedding")]
        public Task n_abouttheWedding() => NotImplemented();

        [ScriptName("aqua_taxi3")]
        public Task n_aqua_taxi3() => NotImplemented();

        [ScriptName("arcana_MC_In")]
        public Task n_arcana_MC_In() => NotImplemented();

        [ScriptName("armi")]
        public Task n_armi() => NotImplemented();

        [ScriptName("arrivalDoyo")]
        public Task n_arrivalDoyo() => NotImplemented();

        [ScriptName("babyBird")]
        public Task n_babyBird() => NotImplemented();

        [ScriptName("babyfood")]
        public Task n_babyfood() => NotImplemented();

        [ScriptName("baishi")]
        public Task n_baishi() => NotImplemented();

        [ScriptName("beginCeremony")]
        public Task n_beginCeremony() => NotImplemented();

        [ScriptName("beginVagasCeremony")]
        public Task n_beginVagasCeremony() => NotImplemented();

        [ScriptName("bingoBoard")]
        public Task n_bingoBoard() => NotImplemented();

        [ScriptName("bohun")]
        public Task n_bohun() => NotImplemented();

        [ScriptName("bottleChange")]
        public Task n_bottleChange() => NotImplemented();

        [ScriptName("cathedral")]
        public Task n_cathedral() => NotImplemented();

        [ScriptName("cathedralCoordinator")]
        public Task n_cathedralCoordinator() => NotImplemented();

        [ScriptName("check_EnSchool")]
        public Task n_check_EnSchool() => NotImplemented();

        [ScriptName("cheeseEnter")]
        public Task n_cheeseEnter() => NotImplemented();

        [ScriptName("child00")]
        public Task n_child00() => NotImplemented();

        [ScriptName("child01")]
        public Task n_child01() => NotImplemented();

        [ScriptName("child02")]
        public Task n_child02() => NotImplemented();

        [ScriptName("china001")]
        public Task n_china001() => NotImplemented();

        [ScriptName("cokeTown")]
        public Task n_cokeTown() => NotImplemented();

        [ScriptName("counsel")]
        public Task n_counsel() => NotImplemented();

        [ScriptName("dollMaster01")]
        public Task n_dollMaster01() => NotImplemented();

        [ScriptName("dollMaster02")]
        public Task n_dollMaster02() => NotImplemented();

        [ScriptName("downHelena")]
        public Task n_downHelena() => NotImplemented();

        [ScriptName("easter")]
        public Task n_easter() => NotImplemented();

        [ScriptName("enterRabbitGround")]
        public Task n_enterRabbitGround() => NotImplemented();

        [ScriptName("enter_EnSchool")]
        public Task n_enter_EnSchool() => NotImplemented();

        [ScriptName("escapeFromDungeon")]
        public Task n_escapeFromDungeon() => NotImplemented();

        [ScriptName("face_my1")]
        public Task n_face_my1() => NotImplemented();

        [ScriptName("face_my2")]
        public Task n_face_my2() => NotImplemented();

        [ScriptName("face_shaolin1")]
        public Task n_face_shaolin1() => NotImplemented();

        [ScriptName("face_shaolin2")]
        public Task n_face_shaolin2() => NotImplemented();

        [ScriptName("face_taiwan1")]
        public Task n_face_taiwan1() => NotImplemented();

        [ScriptName("face_taiwan2")]
        public Task n_face_taiwan2() => NotImplemented();

        [ScriptName("face_yuyuan1")]
        public Task n_face_yuyuan1() => NotImplemented();

        [ScriptName("face_yuyuan2")]
        public Task n_face_yuyuan2() => NotImplemented();

        [ScriptName("firework")]
        public Task n_firework() => NotImplemented();

        [ScriptName("fishing")]
        public Task n_fishing() => NotImplemented();

        [ScriptName("fortuneteller")]
        public Task n_fortuneteller() => NotImplemented();

        [ScriptName("fullmoon")]
        public Task n_fullmoon() => NotImplemented();

        [ScriptName("gachapon16")]
        public Task n_gachapon16() => NotImplemented();

        [ScriptName("gachapon17")]
        public Task n_gachapon17() => NotImplemented();

        [ScriptName("gachapon20")]
        public Task n_gachapon20() => NotImplemented();

        [ScriptName("gachapon21")]
        public Task n_gachapon21() => NotImplemented();

        [ScriptName("gachapon23")]
        public Task n_gachapon23() => NotImplemented();

        [ScriptName("gachapon24")]
        public Task n_gachapon24() => NotImplemented();

        [ScriptName("gachapon25")]
        public Task n_gachapon25() => NotImplemented();

        [ScriptName("gachaponbox1")]
        public Task n_gachaponbox1() => NotImplemented();

        [ScriptName("gaga_Braid")]
        public Task n_gaga_Braid() => NotImplemented();

        [ScriptName("gameLogin")]
        public Task n_gameLogin() => NotImplemented();

        [ScriptName("gameLogout")]
        public Task n_gameLogout() => NotImplemented();

        [ScriptName("gateKeeper")]
        public Task n_gateKeeper() => NotImplemented();

        [ScriptName("giveUpDoll")]
        public Task n_giveUpDoll() => NotImplemented();

        [ScriptName("goNinja")]
        public Task n_goNinja() => NotImplemented();

        [ScriptName("goldCompass")]
        public Task n_goldCompass() => NotImplemented();

        [ScriptName("goldrich")]
        public Task n_goldrich() => NotImplemented();

        [ScriptName("grubber_present")]
        public Task n_grubber_present() => NotImplemented();

        [ScriptName("guoqing")]
        public Task n_guoqing() => NotImplemented();

        [ScriptName("guoqing1")]
        public Task n_guoqing1() => NotImplemented();

        [ScriptName("guyfawkes_tch")]
        public Task n_guyfawkes_tch() => NotImplemented();

        [ScriptName("hair_my1")]
        public Task n_hair_my1() => NotImplemented();

        [ScriptName("hair_my2")]
        public Task n_hair_my2() => NotImplemented();

        [ScriptName("hair_royal2")]
        public Task n_hair_royal2() => NotImplemented();

        [ScriptName("hair_shaolin1")]
        public Task n_hair_shaolin1() => NotImplemented();

        [ScriptName("hair_shaolin2")]
        public Task n_hair_shaolin2() => NotImplemented();

        [ScriptName("hair_shaolin3")]
        public Task n_hair_shaolin3() => NotImplemented();

        [ScriptName("hair_taiwan1")]
        public Task n_hair_taiwan1() => NotImplemented();

        [ScriptName("hair_taiwan2")]
        public Task n_hair_taiwan2() => NotImplemented();

        [ScriptName("hair_yuyuan1")]
        public Task n_hair_yuyuan1() => NotImplemented();

        [ScriptName("hair_yuyuan2")]
        public Task n_hair_yuyuan2() => NotImplemented();

        [ScriptName("halloweenTrick")]
        public Task n_halloweenTrick() => NotImplemented();

        [ScriptName("halloween_Lud")]
        public Task n_halloween_Lud() => NotImplemented();

        [ScriptName("halloweenpq")]
        public Task n_halloweenpq() => NotImplemented();

        [ScriptName("hearttoheart")]
        public Task n_hearttoheart() => NotImplemented();

        [ScriptName("hina_out")]
        public Task n_hina_out() => NotImplemented();

        [ScriptName("hontale_Bdoor")]
        public Task n_hontale_Bdoor() => NotImplemented();

        [ScriptName("hontale_enter_ticket")]
        public Task n_hontale_enter_ticket() => NotImplemented();

        [ScriptName("hwpicture")]
        public Task n_hwpicture() => NotImplemented();

        [ScriptName("hwreward")]
        public Task n_hwreward() => NotImplemented();

        [ScriptName("img_quzi1")]
        public Task n_img_quzi1() => NotImplemented();

        [ScriptName("img_quzi2")]
        public Task n_img_quzi2() => NotImplemented();

        [ScriptName("img_quzi3")]
        public Task n_img_quzi3() => NotImplemented();

        [ScriptName("img_quzi_test")]
        public Task n_img_quzi_test() => NotImplemented();

        [ScriptName("itemDoyo")]
        public Task n_itemDoyo() => NotImplemented();

        [ScriptName("jackynSkytree")]
        public Task n_jackynSkytree() => NotImplemented();

        [ScriptName("leaderAl")]
        public Task n_leaderAl() => NotImplemented();

        [ScriptName("lens_kuala")]
        public Task n_lens_kuala() => NotImplemented();

        [ScriptName("lostDoyo")]
        public Task n_lostDoyo() => NotImplemented();

        [ScriptName("mapleAni")]
        public Task n_mapleAni() => NotImplemented();

        [ScriptName("mapleTCG")]
        public Task n_mapleTCG() => NotImplemented();

        [ScriptName("mike")]
        public Task n_mike() => NotImplemented();

        [ScriptName("miro_OX")]
        public Task n_miro_OX() => NotImplemented();

        [ScriptName("miro_keyBox")]
        public Task n_miro_keyBox() => NotImplemented();

        [ScriptName("miro_music2")]
        public Task n_miro_music2() => NotImplemented();

        [ScriptName("miro_music3")]
        public Task n_miro_music3() => NotImplemented();

        [ScriptName("miro_music4")]
        public Task n_miro_music4() => NotImplemented();

        [ScriptName("miro_music5")]
        public Task n_miro_music5() => NotImplemented();

        [ScriptName("miro_music6")]
        public Task n_miro_music6() => NotImplemented();

        [ScriptName("miro_music7")]
        public Task n_miro_music7() => NotImplemented();

        [ScriptName("mission_9110109")]
        public Task n_mission_9110109() => NotImplemented();

        [ScriptName("moonFlower")]
        public Task n_moonFlower() => NotImplemented();

        [ScriptName("nautil_pearl")]
        public Task n_nautil_pearl() => NotImplemented();

        [ScriptName("necklaceGL")]
        public Task n_necklaceGL() => NotImplemented();

        [ScriptName("neko1")]
        public Task n_neko1() => NotImplemented();

        [ScriptName("neko2")]
        public Task n_neko2() => NotImplemented();

        [ScriptName("neko3")]
        public Task n_neko3() => NotImplemented();

        [ScriptName("neko4")]
        public Task n_neko4() => NotImplemented();

        [ScriptName("neko5")]
        public Task n_neko5() => NotImplemented();

        [ScriptName("nightmarket01")]
        public Task n_nightmarket01() => NotImplemented();

        [ScriptName("nightmarket02")]
        public Task n_nightmarket02() => NotImplemented();

        [ScriptName("ninja_maze")]
        public Task n_ninja_maze() => NotImplemented();

        [ScriptName("nongsim")]
        public Task n_nongsim() => NotImplemented();

        [ScriptName("oliviaMirror1")]
        public Task n_oliviaMirror1() => NotImplemented();

        [ScriptName("oliviaMirror2")]
        public Task n_oliviaMirror2() => NotImplemented();

        [ScriptName("oliviaMirror3")]
        public Task n_oliviaMirror3() => NotImplemented();

        [ScriptName("outRabbitJump")]
        public Task n_outRabbitJump() => NotImplemented();

        [ScriptName("outmoonFlower")]
        public Task n_outmoonFlower() => NotImplemented();

        [ScriptName("pc_weapon")]
        public Task n_pc_weapon() => NotImplemented();

        [ScriptName("picture1")]
        public Task n_picture1() => NotImplemented();

        [ScriptName("picture2")]
        public Task n_picture2() => NotImplemented();

        [ScriptName("picture3")]
        public Task n_picture3() => NotImplemented();

        [ScriptName("picture4")]
        public Task n_picture4() => NotImplemented();

        [ScriptName("picture5")]
        public Task n_picture5() => NotImplemented();

        [ScriptName("pigmy12")]
        public Task n_pigmy12() => NotImplemented();

        [ScriptName("pigmy13")]
        public Task n_pigmy13() => NotImplemented();

        [ScriptName("pigmy16")]
        public Task n_pigmy16() => NotImplemented();

        [ScriptName("pigmy8")]
        public Task n_pigmy8() => NotImplemented();

        [ScriptName("pigmy_guide")]
        public Task n_pigmy_guide() => NotImplemented();

        [ScriptName("pinokio_npc")]
        public Task n_pinokio_npc() => NotImplemented();

        [ScriptName("pinokio_npc2")]
        public Task n_pinokio_npc2() => NotImplemented();

        [ScriptName("pongo_present")]
        public Task n_pongo_present() => NotImplemented();

        [ScriptName("potionExchange")]
        public Task n_potionExchange() => NotImplemented();

        [ScriptName("presentExchange")]
        public Task n_presentExchange() => NotImplemented();

        [ScriptName("q8694s")]
        public Task n_q8694s() => NotImplemented();

        [ScriptName("q8700s")]
        public Task n_q8700s() => NotImplemented();

        [ScriptName("s4strike_statue")]
        public Task n_s4strike_statue() => NotImplemented();

        [ScriptName("sb_enter")]
        public Task n_sb_enter() => NotImplemented();

        [ScriptName("sb_enter1")]
        public Task n_sb_enter1() => NotImplemented();

        [ScriptName("sb_move")]
        public Task n_sb_move() => NotImplemented();

        [ScriptName("sb_out")]
        public Task n_sb_out() => NotImplemented();

        [ScriptName("sb_roomout")]
        public Task n_sb_roomout() => NotImplemented();

        [ScriptName("skin_my1")]
        public Task n_skin_my1() => NotImplemented();

        [ScriptName("skin_shaolin1")]
        public Task n_skin_shaolin1() => NotImplemented();

        [ScriptName("skin_shaolin2")]
        public Task n_skin_shaolin2() => NotImplemented();

        [ScriptName("skin_taiwan1")]
        public Task n_skin_taiwan1() => NotImplemented();

        [ScriptName("skin_yuyuan1")]
        public Task n_skin_yuyuan1() => NotImplemented();

        [ScriptName("slogan_Event")]
        public Task n_slogan_Event() => NotImplemented();

        [ScriptName("spaceGaGa_ent")]
        public Task n_spaceGaGa_ent() => NotImplemented();

        [ScriptName("star_Event1_01")]
        public Task n_star_Event1_01() => NotImplemented();

        [ScriptName("star_Event1_02")]
        public Task n_star_Event1_02() => NotImplemented();

        [ScriptName("star_Event1_03")]
        public Task n_star_Event1_03() => NotImplemented();

        [ScriptName("star_Event1_04")]
        public Task n_star_Event1_04() => NotImplemented();

        [ScriptName("star_Event1_05")]
        public Task n_star_Event1_05() => NotImplemented();

        [ScriptName("surfing")]
        public Task n_surfing() => NotImplemented();

        [ScriptName("tablet01")]
        public Task n_tablet01() => NotImplemented();

        [ScriptName("tablet02")]
        public Task n_tablet02() => NotImplemented();

        [ScriptName("tablet03")]
        public Task n_tablet03() => NotImplemented();

        [ScriptName("tablet04")]
        public Task n_tablet04() => NotImplemented();

        [ScriptName("taiwan_valentine")]
        public Task n_taiwan_valentine() => NotImplemented();

        [ScriptName("targetsay")]
        public Task n_targetsay() => NotImplemented();

        [ScriptName("test")]
        public Task n_test() => NotImplemented();

        [ScriptName("treeboss01")]
        public Task n_treeboss01() => NotImplemented();

        [ScriptName("tutorNineheart")]
        public Task n_tutorNineheart() => NotImplemented();

        [ScriptName("vegas")]
        public Task n_vegas() => NotImplemented();

        [ScriptName("vegasCoordinator")]
        public Task n_vegasCoordinator() => NotImplemented();

        [ScriptName("waitingChapel")]
        public Task n_waitingChapel() => NotImplemented();

        [ScriptName("watermelon_out")]
        public Task n_watermelon_out() => NotImplemented();

        [ScriptName("watingCathedral")]
        public Task n_watingCathedral() => NotImplemented();

        [ScriptName("witchRewordH")]
        public Task n_witchRewordH() => NotImplemented();

        [ScriptName("witchRewordM")]
        public Task n_witchRewordM() => NotImplemented();

        [ScriptName("witchRewordS")]
        public Task n_witchRewordS() => NotImplemented();

        [ScriptName("xmas_sale")]
        public Task n_xmas_sale() => NotImplemented();

        [ScriptName("yurete2_wise")]
        public Task n_yurete2_wise() => NotImplemented();

        [ScriptName("yurete_wise")]
        public Task n_yurete_wise() => NotImplemented();

        [ScriptName("zcap_out")]
        public Task n_zcap_out() => NotImplemented();
    }
}
