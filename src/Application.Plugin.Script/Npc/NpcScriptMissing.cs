namespace Application.Plugin.Script.Npc
{
    /// <summary>
    /// 补齐 WZ 中声明、但 JS 与 C# 两侧均无实现的NPC脚本。
    /// 占位方法抛出 <see cref="NotImplementedException"/>，由 PluginManager 统一处理。
    /// </summary>
    internal partial class NpcScript
    {
        private Task NotImplemented() => throw new NotImplementedException();

        // Npc: 9000054
        [ScriptName("BF_master")]
        public Task n_BF_master() => NotImplemented();

        // Npc: 9000052
        [ScriptName("BF_sheep")]
        public Task n_BF_sheep() => NotImplemented();

        // Npc: 9000053
        [ScriptName("BF_wolf")]
        public Task n_BF_wolf() => NotImplemented();

        // Npc: 9310102
        [ScriptName("CN_HontaleReward")]
        public Task n_CN_HontaleReward() => NotImplemented();

        // Npc: 9330079
        [ScriptName("CakeCandle_out")]
        public Task n_CakeCandle_out() => NotImplemented();

        // Npc: 9330078
        [ScriptName("CandleEvent")]
        public Task n_CandleEvent() => NotImplemented();

        // Npc: 9120055
        [ScriptName("CrimsonStoryH")]
        public Task n_CrimsonStoryH() => NotImplemented();

        // Npc: 9120054
        [ScriptName("CrimsonStoryL")]
        public Task n_CrimsonStoryL() => NotImplemented();

        // Npc: 9330076
        [ScriptName("Danwou")]
        public Task n_Danwou() => NotImplemented();

        // Npc: 1204031
        [ScriptName("Disguised")]
        public Task n_Disguised() => NotImplemented();

        // Npc: 9201000
        [ScriptName("EngageRing")]
        public Task n_EngageRing() => NotImplemented();

        // Npc: 9001004
        [ScriptName("Event10")]
        public Task n_Event10() => NotImplemented();

        // Npc: 4000000, 4000001, 4000002, 4000003
        [ScriptName("HLMS")]
        public Task n_HLMS() => NotImplemented();

        // Npc: 9201002
        [ScriptName("HighPriest")]
        public Task n_HighPriest() => NotImplemented();

        // Npc: 9201096
        [ScriptName("Jack_Additional")]
        public Task n_Jack_Additional() => NotImplemented();

        // Npc: 9310083
        [ScriptName("Jump_event_out")]
        public Task n_Jump_event_out() => NotImplemented();

        // Npc: 9310059
        [ScriptName("KARMA_List")]
        public Task n_KARMA_List() => NotImplemented();

        // Npc: 9000035
        [ScriptName("M_info")]
        public Task n_M_info() => NotImplemented();

        // Npc: 1022002
        [ScriptName("Manji")]
        public Task n_Manji() => NotImplemented();

        // Npc: 9201003
        [ScriptName("MeetPare")]
        public Task n_MeetPare() => NotImplemented();

        // Npc: 9310060
        [ScriptName("NewworldEvent")]
        public Task n_NewworldEvent() => NotImplemented();

        // Npc: 9000059, 9000060
        [ScriptName("PB_bossOut")]
        public Task n_PB_bossOut() => NotImplemented();

        // Npc: 2060103
        [ScriptName("PRaid_enter")]
        public Task n_PRaid_enter() => NotImplemented();

        // Npc: 9120106
        [ScriptName("Pachinko_dama_machine")]
        public Task n_Pachinko_dama_machine() => NotImplemented();

        // Npc: 9310085
        [ScriptName("Pachinko_into")]
        public Task n_Pachinko_into() => NotImplemented();

        // Npc: 9100200, 9100201, 9100202, 9100203, 9100204, 9100205
        [ScriptName("Pachinko_machine")]
        public Task n_Pachinko_machine() => NotImplemented();

        // Npc: 9310101
        [ScriptName("Pachinko_reward")]
        public Task n_Pachinko_reward() => NotImplemented();

        // Npc: 9201001
        [ScriptName("ProofHene")]
        public Task n_ProofHene() => NotImplemented();

        // Npc: 1204030
        [ScriptName("Warehouse")]
        public Task n_Warehouse() => NotImplemented();

        // Npc: 9201004
        [ScriptName("abouttheWedding")]
        public Task n_abouttheWedding() => NotImplemented();

        // Npc: 2060010
        [ScriptName("aqua_taxi3")]
        public Task n_aqua_taxi3() => NotImplemented();

        // Npc: 3003324
        [ScriptName("arcana_MC_In")]
        public Task n_arcana_MC_In() => NotImplemented();

        // Npc: 9000055
        [ScriptName("armi")]
        public Task n_armi() => NotImplemented();

        // Npc: 9000045
        [ScriptName("arrivalDoyo")]
        public Task n_arrivalDoyo() => NotImplemented();

        // Npc: 9000042
        [ScriptName("babyBird")]
        public Task n_babyBird() => NotImplemented();

        // Npc: 2081004
        [ScriptName("babyfood")]
        public Task n_babyfood() => NotImplemented();

        // Npc: 9900002
        [ScriptName("baishi")]
        public Task n_baishi() => NotImplemented();

        // Npc: 9201007
        [ScriptName("beginCeremony")]
        public Task n_beginCeremony() => NotImplemented();

        // Npc: 9201009
        [ScriptName("beginVagasCeremony")]
        public Task n_beginVagasCeremony() => NotImplemented();

        // Npc: 9010010
        [ScriptName("bingoBoard")]
        public Task n_bingoBoard() => NotImplemented();

        // Npc: 9001008
        [ScriptName("bohun")]
        public Task n_bohun() => NotImplemented();

        // Npc: 9330092
        [ScriptName("bottleChange")]
        public Task n_bottleChange() => NotImplemented();

        // Npc: 9201005
        [ScriptName("cathedral")]
        public Task n_cathedral() => NotImplemented();

        // Npc: 9201013
        [ScriptName("cathedralCoordinator")]
        public Task n_cathedralCoordinator() => NotImplemented();

        // Npc: 9310056
        [ScriptName("check_EnSchool")]
        public Task n_check_EnSchool() => NotImplemented();

        // Npc: 9010015
        [ScriptName("cheeseEnter")]
        public Task n_cheeseEnter() => NotImplemented();

        // Npc: 1012110, 9010005
        [ScriptName("child00")]
        public Task n_child00() => NotImplemented();

        // Npc: 1032105, 9010006
        [ScriptName("child01")]
        public Task n_child01() => NotImplemented();

        // Npc: 9010007
        [ScriptName("child02")]
        public Task n_child02() => NotImplemented();

        // Npc: 9900003
        [ScriptName("china001")]
        public Task n_china001() => NotImplemented();

        // Npc: 9001000
        [ScriptName("cokeTown")]
        public Task n_cokeTown() => NotImplemented();

        // Npc: 9010019
        [ScriptName("counsel")]
        public Task n_counsel() => NotImplemented();

        // Npc: 1204002
        [ScriptName("dollMaster01")]
        public Task n_dollMaster01() => NotImplemented();

        // Npc: 1204003
        [ScriptName("dollMaster02")]
        public Task n_dollMaster02() => NotImplemented();

        // Npc: 1204032
        [ScriptName("downHelena")]
        public Task n_downHelena() => NotImplemented();

        // Npc: 9200001
        [ScriptName("easter")]
        public Task n_easter() => NotImplemented();

        // Npc: 9001102
        [ScriptName("enterRabbitGround")]
        public Task n_enterRabbitGround() => NotImplemented();

        // Npc: 9310057
        [ScriptName("enter_EnSchool")]
        public Task n_enter_EnSchool() => NotImplemented();

        // Npc: 9000047
        [ScriptName("escapeFromDungeon")]
        public Task n_escapeFromDungeon() => NotImplemented();

        // Npc: 9270051
        [ScriptName("face_my1")]
        public Task n_face_my1() => NotImplemented();

        // Npc: 9270052
        [ScriptName("face_my2")]
        public Task n_face_my2() => NotImplemented();

        // Npc: 9310036
        [ScriptName("face_shaolin1")]
        public Task n_face_shaolin1() => NotImplemented();

        // Npc: 9310037
        [ScriptName("face_shaolin2")]
        public Task n_face_shaolin2() => NotImplemented();

        // Npc: 9330023
        [ScriptName("face_taiwan1")]
        public Task n_face_taiwan1() => NotImplemented();

        // Npc: 9330024
        [ScriptName("face_taiwan2")]
        public Task n_face_taiwan2() => NotImplemented();

        // Npc: 9310014
        [ScriptName("face_yuyuan1")]
        public Task n_face_yuyuan1() => NotImplemented();

        // Npc: 9310015
        [ScriptName("face_yuyuan2")]
        public Task n_face_yuyuan2() => NotImplemented();

        // Npc: 9010014
        [ScriptName("firework")]
        public Task n_firework() => NotImplemented();

        // Npc: 9330045, 9330046, 9330047
        [ScriptName("fishing")]
        public Task n_fishing() => NotImplemented();

        // Npc: 9330042
        [ScriptName("fortuneteller")]
        public Task n_fortuneteller() => NotImplemented();

        // Npc: 9000070
        [ScriptName("fullmoon")]
        public Task n_fullmoon() => NotImplemented();

        // Npc: 9050010
        [ScriptName("gachapon16")]
        public Task n_gachapon16() => NotImplemented();

        // Npc: 9310091
        [ScriptName("gachapon17")]
        public Task n_gachapon17() => NotImplemented();

        // Npc: 9310062, 9310093
        [ScriptName("gachapon20")]
        public Task n_gachapon20() => NotImplemented();

        // Npc: 9310067
        [ScriptName("gachapon21")]
        public Task n_gachapon21() => NotImplemented();

        // Npc: 9310086, 9310096
        [ScriptName("gachapon23")]
        public Task n_gachapon23() => NotImplemented();

        // Npc: 9310089, 9310097
        [ScriptName("gachapon24")]
        public Task n_gachapon24() => NotImplemented();

        // Npc: 9310090
        [ScriptName("gachapon25")]
        public Task n_gachapon25() => NotImplemented();

        // Npc: 9201117
        [ScriptName("gachaponbox1")]
        public Task n_gachaponbox1() => NotImplemented();

        // Npc: 9310088
        [ScriptName("gaga_Braid")]
        public Task n_gaga_Braid() => NotImplemented();

        // Npc: 9330084
        [ScriptName("gameLogin")]
        public Task n_gameLogin() => NotImplemented();

        // Npc: 9330083
        [ScriptName("gameLogout")]
        public Task n_gameLogout() => NotImplemented();

        // Npc: 2120001
        [ScriptName("gateKeeper")]
        public Task n_gateKeeper() => NotImplemented();

        // Npc: 2120004
        [ScriptName("giveUpDoll")]
        public Task n_giveUpDoll() => NotImplemented();

        // Npc: 9110107
        [ScriptName("goNinja")]
        public Task n_goNinja() => NotImplemented();

        // Npc: 2084000
        [ScriptName("goldCompass")]
        public Task n_goldCompass() => NotImplemented();

        // Npc: 2084001, 2084002
        [ScriptName("goldrich")]
        public Task n_goldrich() => NotImplemented();

        // Npc: 9201092
        [ScriptName("grubber_present")]
        public Task n_grubber_present() => NotImplemented();

        // Npc: 9900004
        [ScriptName("guoqing")]
        public Task n_guoqing() => NotImplemented();

        // Npc: 9900005
        [ScriptName("guoqing1")]
        public Task n_guoqing1() => NotImplemented();

        // Npc: 9330087
        [ScriptName("guyfawkes_tch")]
        public Task n_guyfawkes_tch() => NotImplemented();

        // Npc: 9270048
        [ScriptName("hair_my1")]
        public Task n_hair_my1() => NotImplemented();

        // Npc: 9270049
        [ScriptName("hair_my2")]
        public Task n_hair_my2() => NotImplemented();

        // Npc: 9105006
        [ScriptName("hair_royal2")]
        public Task n_hair_royal2() => NotImplemented();

        // Npc: 9310032
        [ScriptName("hair_shaolin1")]
        public Task n_hair_shaolin1() => NotImplemented();

        // Npc: 9310031
        [ScriptName("hair_shaolin2")]
        public Task n_hair_shaolin2() => NotImplemented();

        // Npc: 9310035
        [ScriptName("hair_shaolin3")]
        public Task n_hair_shaolin3() => NotImplemented();

        // Npc: 9330021
        [ScriptName("hair_taiwan1")]
        public Task n_hair_taiwan1() => NotImplemented();

        // Npc: 9330022
        [ScriptName("hair_taiwan2")]
        public Task n_hair_taiwan2() => NotImplemented();

        // Npc: 9310016
        [ScriptName("hair_yuyuan1")]
        public Task n_hair_yuyuan1() => NotImplemented();

        // Npc: 9310017
        [ScriptName("hair_yuyuan2")]
        public Task n_hair_yuyuan2() => NotImplemented();

        // Npc: 9201028
        [ScriptName("halloweenTrick")]
        public Task n_halloweenTrick() => NotImplemented();

        // Npc: 9201076
        [ScriptName("halloween_Lud")]
        public Task n_halloween_Lud() => NotImplemented();

        // Npc: 2120002
        [ScriptName("halloweenpq")]
        public Task n_halloweenpq() => NotImplemented();

        // Npc: 9310084
        [ScriptName("hearttoheart")]
        public Task n_hearttoheart() => NotImplemented();

        // Npc: 9120022
        [ScriptName("hina_out")]
        public Task n_hina_out() => NotImplemented();

        // Npc: 2083003
        [ScriptName("hontale_Bdoor")]
        public Task n_hontale_Bdoor() => NotImplemented();

        // Npc: 9310087
        [ScriptName("hontale_enter_ticket")]
        public Task n_hontale_enter_ticket() => NotImplemented();

        // Npc: 2121011
        [ScriptName("hwpicture")]
        public Task n_hwpicture() => NotImplemented();

        // Npc: 2120009
        [ScriptName("hwreward")]
        public Task n_hwreward() => NotImplemented();

        // Npc: 9310075, 9310079
        [ScriptName("img_quzi1")]
        public Task n_img_quzi1() => NotImplemented();

        // Npc: 9310076, 9310080
        [ScriptName("img_quzi2")]
        public Task n_img_quzi2() => NotImplemented();

        // Npc: 9310077, 9310081
        [ScriptName("img_quzi3")]
        public Task n_img_quzi3() => NotImplemented();

        // Npc: 9310078
        [ScriptName("img_quzi_test")]
        public Task n_img_quzi_test() => NotImplemented();

        // Npc: 9000044
        [ScriptName("itemDoyo")]
        public Task n_itemDoyo() => NotImplemented();

        // Npc: 9000048
        [ScriptName("jackynSkytree")]
        public Task n_jackynSkytree() => NotImplemented();

        // Npc: 1002103
        [ScriptName("leaderAl")]
        public Task n_leaderAl() => NotImplemented();

        // Npc: 9270053
        [ScriptName("lens_kuala")]
        public Task n_lens_kuala() => NotImplemented();

        // Npc: 9000043
        [ScriptName("lostDoyo")]
        public Task n_lostDoyo() => NotImplemented();

        // Npc: 9010016
        [ScriptName("mapleAni")]
        public Task n_mapleAni() => NotImplemented();

        // Npc: 9010018
        [ScriptName("mapleTCG")]
        public Task n_mapleTCG() => NotImplemented();

        // Npc: 1040001
        [ScriptName("mike")]
        public Task n_mike() => NotImplemented();

        // Npc: 2084009
        [ScriptName("miro_OX")]
        public Task n_miro_OX() => NotImplemented();

        // Npc: 2084010
        [ScriptName("miro_keyBox")]
        public Task n_miro_keyBox() => NotImplemented();

        // Npc: 2084003
        [ScriptName("miro_music2")]
        public Task n_miro_music2() => NotImplemented();

        // Npc: 2084004
        [ScriptName("miro_music3")]
        public Task n_miro_music3() => NotImplemented();

        // Npc: 2084005
        [ScriptName("miro_music4")]
        public Task n_miro_music4() => NotImplemented();

        // Npc: 2084006
        [ScriptName("miro_music5")]
        public Task n_miro_music5() => NotImplemented();

        // Npc: 2084007
        [ScriptName("miro_music6")]
        public Task n_miro_music6() => NotImplemented();

        // Npc: 2084008
        [ScriptName("miro_music7")]
        public Task n_miro_music7() => NotImplemented();

        // Npc: 9110109
        [ScriptName("mission_9110109")]
        public Task n_mission_9110109() => NotImplemented();

        // Npc: 9001100
        [ScriptName("moonFlower")]
        public Task n_moonFlower() => NotImplemented();

        // Npc: 1092097
        [ScriptName("nautil_pearl")]
        public Task n_nautil_pearl() => NotImplemented();

        // Npc: 9300010
        [ScriptName("necklaceGL")]
        public Task n_necklaceGL() => NotImplemented();

        // Npc: 9100000
        [ScriptName("neko1")]
        public Task n_neko1() => NotImplemented();

        // Npc: 9100001
        [ScriptName("neko2")]
        public Task n_neko2() => NotImplemented();

        // Npc: 9100002
        [ScriptName("neko3")]
        public Task n_neko3() => NotImplemented();

        // Npc: 9100003
        [ScriptName("neko4")]
        public Task n_neko4() => NotImplemented();

        // Npc: 9100004
        [ScriptName("neko5")]
        public Task n_neko5() => NotImplemented();

        // Npc: 9330028
        [ScriptName("nightmarket01")]
        public Task n_nightmarket01() => NotImplemented();

        // Npc: 9330032
        [ScriptName("nightmarket02")]
        public Task n_nightmarket02() => NotImplemented();

        // Npc: 9110105
        [ScriptName("ninja_maze")]
        public Task n_ninja_maze() => NotImplemented();

        // Npc: 9001009
        [ScriptName("nongsim")]
        public Task n_nongsim() => NotImplemented();

        // Npc: 9201139
        [ScriptName("oliviaMirror1")]
        public Task n_oliviaMirror1() => NotImplemented();

        // Npc: 9201140
        [ScriptName("oliviaMirror2")]
        public Task n_oliviaMirror2() => NotImplemented();

        // Npc: 9201141
        [ScriptName("oliviaMirror3")]
        public Task n_oliviaMirror3() => NotImplemented();

        // Npc: 9001107
        [ScriptName("outRabbitJump")]
        public Task n_outRabbitJump() => NotImplemented();

        // Npc: 9001108
        [ScriptName("outmoonFlower")]
        public Task n_outmoonFlower() => NotImplemented();

        // Npc: 9000018
        [ScriptName("pc_weapon")]
        public Task n_pc_weapon() => NotImplemented();

        // Npc: 2121006
        [ScriptName("picture1")]
        public Task n_picture1() => NotImplemented();

        // Npc: 2121010
        [ScriptName("picture2")]
        public Task n_picture2() => NotImplemented();

        // Npc: 2121009
        [ScriptName("picture3")]
        public Task n_picture3() => NotImplemented();

        // Npc: 2121007
        [ScriptName("picture4")]
        public Task n_picture4() => NotImplemented();

        // Npc: 2121008
        [ScriptName("picture5")]
        public Task n_picture5() => NotImplemented();

        // Npc: 9310094
        [ScriptName("pigmy12")]
        public Task n_pigmy12() => NotImplemented();

        // Npc: 9310095
        [ScriptName("pigmy13")]
        public Task n_pigmy13() => NotImplemented();

        // Npc: 9310098
        [ScriptName("pigmy16")]
        public Task n_pigmy16() => NotImplemented();

        // Npc: 9310099
        [ScriptName("pigmy8")]
        public Task n_pigmy8() => NotImplemented();

        // Npc: 9050009
        [ScriptName("pigmy_guide")]
        public Task n_pigmy_guide() => NotImplemented();

        // Npc: 9000050
        [ScriptName("pinokio_npc")]
        public Task n_pinokio_npc() => NotImplemented();

        // Npc: 9000051
        [ScriptName("pinokio_npc2")]
        public Task n_pinokio_npc2() => NotImplemented();

        // Npc: 9201091
        [ScriptName("pongo_present")]
        public Task n_pongo_present() => NotImplemented();

        // Npc: 9310100
        [ScriptName("potionExchange")]
        public Task n_potionExchange() => NotImplemented();

        // Npc: 9201014
        [ScriptName("presentExchange")]
        public Task n_presentExchange() => NotImplemented();

        // Npc: 9330065
        [ScriptName("q8694s")]
        public Task n_q8694s() => NotImplemented();

        // Npc: 9330067
        [ScriptName("q8700s")]
        public Task n_q8700s() => NotImplemented();

        // Npc: 9310063
        [ScriptName("sb_enter")]
        public Task n_sb_enter() => NotImplemented();

        // Npc: 9310066
        [ScriptName("sb_enter1")]
        public Task n_sb_enter1() => NotImplemented();

        // Npc: 9310065
        [ScriptName("sb_move")]
        public Task n_sb_move() => NotImplemented();

        // Npc: 9310068
        [ScriptName("sb_out")]
        public Task n_sb_out() => NotImplemented();

        // Npc: 9310064
        [ScriptName("sb_roomout")]
        public Task n_sb_roomout() => NotImplemented();

        // Npc: 9270050
        [ScriptName("skin_my1")]
        public Task n_skin_my1() => NotImplemented();

        // Npc: 9310043
        [ScriptName("skin_shaolin1")]
        public Task n_skin_shaolin1() => NotImplemented();

        // Npc: 9310034
        [ScriptName("skin_shaolin2")]
        public Task n_skin_shaolin2() => NotImplemented();

        // Npc: 9330025
        [ScriptName("skin_taiwan1")]
        public Task n_skin_taiwan1() => NotImplemented();

        // Npc: 9310018
        [ScriptName("skin_yuyuan1")]
        public Task n_skin_yuyuan1() => NotImplemented();

        // Npc: 9310069
        [ScriptName("slogan_Event")]
        public Task n_slogan_Event() => NotImplemented();

        // Npc: 9001102
        [ScriptName("spaceGaGa_ent")]
        public Task n_spaceGaGa_ent() => NotImplemented();

        // Npc: 9310070
        [ScriptName("star_Event1_01")]
        public Task n_star_Event1_01() => NotImplemented();

        // Npc: 9310071
        [ScriptName("star_Event1_02")]
        public Task n_star_Event1_02() => NotImplemented();

        // Npc: 9310072
        [ScriptName("star_Event1_03")]
        public Task n_star_Event1_03() => NotImplemented();

        // Npc: 9310074
        [ScriptName("star_Event1_04")]
        public Task n_star_Event1_04() => NotImplemented();

        // Npc: 9310073
        [ScriptName("star_Event1_05")]
        public Task n_star_Event1_05() => NotImplemented();

        // Npc: 9110015
        [ScriptName("surfing")]
        public Task n_surfing() => NotImplemented();

        // Npc: 2121001
        [ScriptName("tablet01")]
        public Task n_tablet01() => NotImplemented();

        // Npc: 2121002
        [ScriptName("tablet02")]
        public Task n_tablet02() => NotImplemented();

        // Npc: 2121003
        [ScriptName("tablet03")]
        public Task n_tablet03() => NotImplemented();

        // Npc: 2121004
        [ScriptName("tablet04")]
        public Task n_tablet04() => NotImplemented();

        // Npc: 9330044
        [ScriptName("taiwan_valentine")]
        public Task n_taiwan_valentine() => NotImplemented();

        // Npc: 9201041
        [ScriptName("targetsay")]
        public Task n_targetsay() => NotImplemented();

        // Npc: 2121012, 9010017
        [ScriptName("test")]
        public Task n_test() => NotImplemented();

        // Npc: 9270045
        [ScriptName("treeboss01")]
        public Task n_treeboss01() => NotImplemented();

        // Npc: 1102009
        [ScriptName("tutorNineheart")]
        public Task n_tutorNineheart() => NotImplemented();

        // Npc: 9201008
        [ScriptName("vegas")]
        public Task n_vegas() => NotImplemented();

        // Npc: 9201012
        [ScriptName("vegasCoordinator")]
        public Task n_vegasCoordinator() => NotImplemented();

        // Npc: 9201010
        [ScriptName("waitingChapel")]
        public Task n_waitingChapel() => NotImplemented();

        // Npc: 2073000, 9000039
        [ScriptName("watermelon_out")]
        public Task n_watermelon_out() => NotImplemented();

        // Npc: 9201006
        [ScriptName("watingCathedral")]
        public Task n_watingCathedral() => NotImplemented();

        // Npc: 9000058
        [ScriptName("witchRewordH")]
        public Task n_witchRewordH() => NotImplemented();

        // Npc: 9000057
        [ScriptName("witchRewordM")]
        public Task n_witchRewordM() => NotImplemented();

        // Npc: 9000056
        [ScriptName("witchRewordS")]
        public Task n_witchRewordS() => NotImplemented();

        // Npc: 9209101
        [ScriptName("xmas_sale")]
        public Task n_xmas_sale() => NotImplemented();

        // Npc: 2112012
        [ScriptName("yurete2_wise")]
        public Task n_yurete2_wise() => NotImplemented();

        // Npc: 2112002
        [ScriptName("yurete_wise")]
        public Task n_yurete_wise() => NotImplemented();

        // Npc: 9120020
        [ScriptName("zcap_out")]
        public Task n_zcap_out() => NotImplemented();
    }
}
