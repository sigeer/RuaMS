using Application.Core.Client;
using Application.Core.Game.Maps;
using Application.Utility;
using scripting.map;

namespace Application.Plugin.Script
{
    // Extra:
    // 108010600, 108010610, 108010620, 108010630, 108010640,
    // 130030000, 130030001,
    // 677000001, 677000003, 677000005, 677000007, 677000009, 677000012,
    // 910510000, 912000000, 925040100, 926000000, 926000010, 926120300,
    // babyPigMap, cannon_tuto_01, cannon_tuto_direction, cannon_tuto_direction1, cannon_tuto_direction2, crash_Dragon, evanleaveD, getDragonEgg, meetWithDragon,
    // PromiseDragon, Resi_tutor10, Resi_tutor20, Resi_tutor30, Resi_tutor40, Resi_tutor50, Resi_tutor60, Resi_tutor70, Resi_tutor80
    internal class MapEnterScript : MapScriptMethods
    {
        public MapEnterScript(IChannelClient c, IMap m) : base(c, m)
        {
        }

        // Map: 0 
        [ScriptName("goAdventure")]
        public Task s_goAdventure()
        {
            // TODO
            goAdventure();
            return Task.CompletedTask;
        }


        // Map: 10000 
        public Task go10000()
        {
            // TODO
            unlockUI();
            mapEffect("maplemap/enter/10000");
            return Task.CompletedTask;
        }


        // Map: 20000 
        public Task go20000()
        {
            // TODO
            unlockUI();
            mapEffect("maplemap/enter/20000");
            return Task.CompletedTask;
        }


        // Map: 30000 
        public Task go30000()
        {
            // TODO
            mapEffect("maplemap/enter/30000");
            return Task.CompletedTask;
        }


        // Map: 40000 
        public Task go40000()
        {
            // TODO
            mapEffect("maplemap/enter/40000");
            return Task.CompletedTask;
        }


        // Map: 50000 
        public Task go50000()
        {
            // TODO
            mapEffect("maplemap/enter/50000");
            return Task.CompletedTask;
        }


        // Map: 1000000 
        public Task go1000000()
        {
            // TODO
            mapEffect("maplemap/enter/1000000");
            return Task.CompletedTask;
        }


        // Map: 1010000 
        public Task go1010000()
        {
            // TODO
            mapEffect("maplemap/enter/1010000");
            return Task.CompletedTask;
        }


        // Map: 1010100 
        public Task go1010100()
        {
            // TODO
            mapEffect("maplemap/enter/1010100");
            return Task.CompletedTask;
        }


        // Map: 1010200 
        public Task go1010200()
        {
            // TODO
            mapEffect("maplemap/enter/1010200");
            return Task.CompletedTask;
        }


        // Map: 1010300 
        public Task go1010300()
        {
            // TODO
            mapEffect("maplemap/enter/1010300");
            return Task.CompletedTask;
        }


        // Map: 1010400 
        public Task go1010400()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 1020000 
        public Task go1020000()
        {
            // TODO
            unlockUI();
            mapEffect("maplemap/enter/1020000");
            return Task.CompletedTask;
        }


        // Map: 1020100 
        public Task goSwordman()
        {
            // TODO
            startExplorerExperience();
            return Task.CompletedTask;
        }


        // Map: 1020200 
        public Task goMagician()
        {
            // TODO
            startExplorerExperience();
            return Task.CompletedTask;
        }


        // Map: 1020300 
        public Task goArcher()
        {
            // TODO
            startExplorerExperience();
            return Task.CompletedTask;
        }


        // Map: 1020400 
        public Task goRogue()
        {
            // TODO
            startExplorerExperience();
            return Task.CompletedTask;
        }


        // Map: 1020500 
        public Task goPirate()
        {
            // TODO
            startExplorerExperience();
            return Task.CompletedTask;
        }


        // Map: 2000000 
        public Task go2000000()
        {
            // TODO
            mapEffect("maplemap/enter/2000000");
            return Task.CompletedTask;
        }


        // Map: 2010000 
        [ScriptName("goLith")]
        public Task s_goLith()
        {
            // TODO
            goLith();
            return Task.CompletedTask;
        }


        // Map: 100000000, 100000003, 100010000, 100040000, 100040100, 101000000, 101010103, 101020000, 101030104, 101030406, 102000000, 102020300, 102050000, 103000000, 103010001, 103030200, 104000000, 104010001, 104020000, 105040300, 105040305, 105070001, 105080000, 105090200, 105090300, 105090301, 105090312, 105090500, 105090900, 110000000, 200000000, 200010100, 200010300, 200080000, 200080100, 211000000, 211030000, 211040300, 211041200, 211041800, 220000000, 220020300, 220040200, 221000000, 221020701, 221030600, 221040400, 222000000, 222010400, 222020000, 230000000, 230010200, 230010201, 230010400, 230020000, 230020201, 230030100, 230040000, 230040200, 230040400, 240000000, 240010200, 240010800, 240020101, 240020102, 240020401, 240020402, 240030000, 240040400, 240040511, 240040521, 240050000, 250000000, 250010300, 250010304, 250010500, 250010504, 250020300, 251000000, 251010200, 251010402, 251010500, 260000000, 260010300, 260010600, 260020300, 260020700, 261000000, 261010100, 261020000, 261020401, 261030000, 910310000, 910310001, 910310002, 910310003, 910310004 
        public Task explorationPoint()
        {
            // TODO
            if (getPlayer().getMapId() == 110000000 || (getPlayer().getMapId() >= 100000000 && getPlayer().getMapId() < 105040300))
            {
                explorerQuest(29005, "新手冒险家");//Beginner Explorer
            }
            else if (getPlayer().getMapId() >= 105040300 && getPlayer().getMapId() <= 105090900)
            {
                explorerQuest(29014, "林中之城探险家");//Sleepywood Explorer
            }
            else if (getPlayer().getMapId() >= 200000000 && getPlayer().getMapId() <= 211041800)
            {
                explorerQuest(29006, "冰峰雪域山脉探险家");//El Nath Mts. Explorer
            }
            else if (getPlayer().getMapId() >= 220000000 && getPlayer().getMapId() <= 222020000)
            {
                explorerQuest(29007, "时间静止之湖探险家");//Ludus Lake Explorer
            }
            else if (getPlayer().getMapId() >= 230000000 && getPlayer().getMapId() <= 230040401)
            {
                explorerQuest(29008, "海底探险家");//Undersea Explorer
            }
            else if (getPlayer().getMapId() >= 250000000 && getPlayer().getMapId() <= 251010500)
            {
                explorerQuest(29009, "武陵探险家");//Mu Lung Explorer
            }
            else if (getPlayer().getMapId() >= 260000000 && getPlayer().getMapId() <= 261030000)
            {
                explorerQuest(29010, "尼哈沙漠探险家");//Nihal Desert Explorer
            }
            else if (getPlayer().getMapId() >= 240000000 && getPlayer().getMapId() <= 240050000)
            {
                explorerQuest(29011, "米纳尔森林探险家");//Minar Forest Explorer
            }
            if (getPlayer().getMapId() == 104000000)
            {
                mapEffect("maplemap/enter/104000000");
            }
            return Task.CompletedTask;
        }


        // Map: 100000006 
        [ScriptName("100000006")]
        public Task s_100000006()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 103000300, 103000301, 103000302 
        public Task Depart_inSubway()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 103040410, 103040411, 103040412, 103040413, 103040414, 103040415, 103040416, 103040417, 103040418, 103040419 
        public Task Depart_topFloorEnter()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 103040430, 103040431, 103040432, 103040433, 103040434, 103040435, 103040436, 103040437, 103040438, 103040439 
        public Task Depart_BossEnter()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 105100100 
        public Task outCase()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 105100300, 105100400 
        public Task balog_buff()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 105100301, 105100401 
        public Task balog_dateSet()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 106020000 
        public Task TD_MC_title()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 106020001 
        public Task TD_MC_Openning()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 106020501 
        public Task TD_MC_gasi2()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 106020502 
        public Task TD_MC_gasi()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 106021001, 106021002, 106021003, 106021004, 106021005, 106021006, 106021007, 106021008, 106021009, 106021010 
        public Task in_secretroom()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 106021400 
        public Task TD_MC_keycheck()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 106021500, 106021501, 106021502, 106021503, 106021504, 106021505, 106021506, 106021507, 106021508, 106021509 
        public Task pepeking_effect()
        {
            // TODO
            var mobId = 3300000 + (Random.Shared.Next(3) + 5);
            var player = getPlayer();
            var map = player.getMap();
            map.spawnMonsterOnGroundBelow(mobId, -28, -67);
            return Task.CompletedTask;
        }


        // Map: 106021600, 106021601, 106021602, 106021603, 106021604, 106021605, 106021606, 106021607, 106021608, 106021609 
        public Task findvioleta()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 109090100, 109090101, 109090102, 109090103, 109090104, 910040110, 910040210, 910040310, 910041110, 910041210, 910041310 
        public Task BF_sheepChat()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 109090200, 109090201, 109090202, 109090203, 109090204, 910040120, 910040220, 910040320, 910041120, 910041220, 910041320 
        public Task BF_wolfChat()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 130000000 
        public Task startEreb()
        {
            // TODO
            if (getJobId() == 1000 && getLevel() >= 10)
            {
                unlockUI();
            }
            return Task.CompletedTask;
        }


        // Map: 140000000 
        public Task rien()
        {
            // TODO
            if (isQuestCompleted(21101) && containsAreaInfo(21019, "miss=o;arr=o;helper=clear"))
            {
                updateAreaInfo(21019, "miss=o;arr=o;ck=1;helper=clear");
            }
            unlockUI();
            return Task.CompletedTask;
        }


        // Map: 140010000 
        public Task rienArrow()
        {
            // TODO
            if (containsAreaInfo(21019, "miss=o;helper=clear"))
            {
                updateAreaInfo(21019, "miss=o;arr=o;helper=clear");
                showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow3");
            }
            return Task.CompletedTask;
        }


        // Map: 140030000 
        public Task mirrorCave()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 140090000 
        public Task iceCave()
        {
            // TODO
            teachSkill(20000014, -1, 0, -1);
            teachSkill(20000015, -1, 0, -1);
            teachSkill(20000016, -1, 0, -1);
            teachSkill(20000017, -1, 0, -1);
            teachSkill(20000018, -1, 0, -1);
            unlockUI();
            showIntro("Effect/Direction1.img/aranTutorial/ClickLilin");
            return Task.CompletedTask;
        }


        // Map: 240000110 
        public Task undomorphdarco()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 240070000, 240070100, 240070200, 240070300, 240070400, 240070500, 240070600 
        public Task TD_NC_title()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 240070203, 240070204, 240070205, 240070206, 240070207, 240070208, 240070209, 240070303, 240070304, 240070305, 240070306, 240070307, 240070308, 240070309, 240070403, 240070404, 240070405, 240070406, 240070407, 240070408, 240070409, 240070503, 240070504, 240070505, 240070506, 240070507, 240070508, 240070509, 240070603, 240070604, 240070605, 240070606, 240070607, 240070608, 240070609 
        public Task TD_neo_BossEnter()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 270000100 
        public Task reundodraco()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 910020100, 910020101, 910020102, 910020103, 910020104, 910020200, 910020201, 910020202, 910020203, 910020204, 910020300, 910020301, 910020302, 910020303, 910020304, 910021000 
        public Task delDoyo()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 910030001 
        public Task undoMorph_jepeto()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 910320001, 926010001, 926010002, 926010003, 926010004 
        public Task Massacre_result()
        {
            // TODO
            var py = getPyramid();
            if (py != null)
            {
                py.sendScore(getPlayer());
            }
            return Task.CompletedTask;
        }


        // Map: 910320100, 910320101, 910320102, 910320103, 910320104, 910330100, 910330101, 910330102, 910330103, 910330104, 910330105, 910330106, 910330107, 910330108, 910330109, 926010100, 926010101, 926010102, 926010103, 926010104, 926011100, 926011101, 926011102, 926011103, 926011104, 926012100, 926012101, 926012102, 926012103, 926012104, 926013100, 926013101, 926013102, 926013103, 926013104, 926020100, 926020101, 926020102, 926020103, 926020104, 926020105, 926020106, 926020107, 926020108, 926020109, 926021100, 926021101, 926021102, 926021103, 926021104, 926021105, 926021106, 926021107, 926021108, 926021109, 926022100, 926022101, 926022102, 926022103, 926022104, 926022105, 926022106, 926022107, 926022108, 926022109, 926023100, 926023101, 926023102, 926023103, 926023104, 926023105, 926023106, 926023107, 926023108, 926023109 
        public Task Massacre_first()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 910320200, 910320201, 910320202, 910320203, 910320204, 910320300, 910320301, 910320302, 910320303, 910320304 
        public Task metro_firstSetting()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 910400000 
        public Task dangerInfo()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 910510200 
        public Task dollCave00()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 910510201 
        public Task dollCave01()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 910510202 
        public Task dollCave02()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 913040000, 913040001, 913040002, 913040003, 913040004, 913040005, 913040006, 913040011 
        public Task cygnusTest()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 913040100, 913040101, 913040102, 913040103, 913040104, 913040105, 913040106 
        public Task cygnusJobTutorial()
        {
            // TODO
            displayCygnusIntro();
            return Task.CompletedTask;
        }


        // Map: 914000000, 914000300, 914000400, 914000410, 914000420, 914000500 
        public Task aranTutorAlone()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 914090000, 914090001, 914090002, 914090003, 914090004, 914090005, 914090006, 914090007, 914090010, 914090011, 914090012, 914090013, 914090014, 914090015, 914090100, 914090200, 914090201 
        public Task aranDirection()
        {
            // TODO
            displayAranIntro();
            return Task.CompletedTask;
        }


        // Map: 920010000 
        public Task start_itemTake()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 920030001 
        public Task sealGarden()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 922240200 
        public Task space_first()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 925020000 
        public Task dojang_Msg()
        {
            // TODO
            if (getPlayer().getMap().getId() == 925020000)
            {
                if (getPlayer().getMap().findClosestPlayerSpawnpoint(getPlayer().getPosition())?.getId() == 0)
                {
                    string[] messages = [
                        "勇闯武陵道场，阁下好胆识！",
                        "想尝尝败北的滋味？尽管放马过来！",
                        "定让你后悔挑战武陵道场！速来受死！"
                    ];
                    getPlayer().startMapEffect(Randomizer.Select(messages), 5120024);
                }

                resetDojoEnergy();
            }
            else
            {
                getPlayer().resetEnteredScript(); //in case the person dcs in here we set it at dojang_tuto portal
                getPlayer().startMapEffect("Ha! Let's see what you got! I won't let you leave unless you defeat me first!", 5120024);
            }
            return Task.CompletedTask;
        }


        // Map: 925020002, 925020003, 925040000 
        public Task dojang_QcheckSet()
        {
            // TODO

            return Task.CompletedTask;
        }


        // Map: 925020100, 925020101, 925020102, 925020103, 925020104, 925020105, 925020106, 925020107, 925020108, 925020109, 925020200, 925020201, 925020202, 925020203, 925020204, 925020205, 925020206, 925020207, 925020208, 925020209, 925020300, 925020301, 925020302, 925020303, 925020304, 925020305, 925020306, 925020307, 925020308, 925020309, 925020400, 925020401, 925020402, 925020403, 925020404, 925020405, 925020406, 925020407, 925020408, 925020409, 925020500, 925020501, 925020502, 925020503, 925020504, 925020505, 925020506, 925020507, 925020508, 925020509, 925020600, 925020601, 925020602, 925020603, 925020604, 925020605, 925020606, 925020607, 925020608, 925020609, 925020610, 925020700, 925020701, 925020702, 925020703, 925020704, 925020705, 925020706, 925020707, 925020708, 925020709, 925020800, 925020801, 925020802, 925020803, 925020804, 925020805, 925020806, 925020807, 925020808, 925020809, 925020900, 925020901, 925020902, 925020903, 925020904, 925020905, 925020906, 925020907, 925020908, 925020909, 925021000, 925021001, 925021002, 925021003, 925021004, 925021005, 925021006, 925021007, 925021008, 925021009, 925021100, 925021101, 925021102, 925021103, 925021104, 925021105, 925021106, 925021107, 925021108, 925021109, 925021200, 925021201, 925021202, 925021203, 925021204, 925021205, 925021206, 925021207, 925021208, 925021209, 925021300, 925021301, 925021302, 925021303, 925021304, 925021305, 925021306, 925021307, 925021308, 925021309, 925021400, 925021401, 925021402, 925021403, 925021404, 925021405, 925021406, 925021407, 925021408, 925021409, 925021500, 925021501, 925021502, 925021503, 925021504, 925021505, 925021506, 925021507, 925021508, 925021509, 925021600, 925021601, 925021602, 925021603, 925021604, 925021605, 925021606, 925021607, 925021608, 925021609, 925021700, 925021701, 925021702, 925021703, 925021704, 925021705, 925021706, 925021707, 925021708, 925021709, 925021800, 925021801, 925021802, 925021803, 925021804, 925021805, 925021806, 925021807, 925021808, 925021809, 925021900, 925021901, 925021902, 925021903, 925021904, 925021905, 925021906, 925021907, 925021908, 925021909, 925022000, 925022001, 925022002, 925022003, 925022004, 925022005, 925022006, 925022007, 925022008, 925022009, 925022100, 925022101, 925022102, 925022103, 925022104, 925022105, 925022106, 925022107, 925022108, 925022109, 925022200, 925022201, 925022202, 925022203, 925022204, 925022205, 925022206, 925022207, 925022208, 925022209, 925022300, 925022301, 925022302, 925022303, 925022304, 925022305, 925022306, 925022307, 925022308, 925022309, 925022400, 925022401, 925022402, 925022403, 925022404, 925022405, 925022406, 925022407, 925022408, 925022409, 925022500, 925022501, 925022502, 925022503, 925022504, 925022505, 925022506, 925022507, 925022508, 925022509, 925022600, 925022601, 925022602, 925022603, 925022604, 925022605, 925022606, 925022607, 925022608, 925022609, 925022700, 925022701, 925022702, 925022703, 925022704, 925022705, 925022706, 925022707, 925022708, 925022709, 925022800, 925022801, 925022802, 925022803, 925022804, 925022805, 925022806, 925022807, 925022808, 925022809, 925022900, 925022901, 925022902, 925022903, 925022904, 925022905, 925022906, 925022907, 925022908, 925022909, 925023000, 925023001, 925023002, 925023003, 925023004, 925023005, 925023006, 925023007, 925023008, 925023009, 925023100, 925023101, 925023102, 925023103, 925023104, 925023105, 925023106, 925023107, 925023108, 925023109, 925023200, 925023201, 925023202, 925023203, 925023204, 925023205, 925023206, 925023207, 925023208, 925023209, 925023300, 925023301, 925023302, 925023303, 925023304, 925023305, 925023306, 925023307, 925023308, 925023309, 925023400, 925023401, 925023402, 925023403, 925023404, 925023405, 925023406, 925023407, 925023408, 925023409, 925023500, 925023501, 925023502, 925023503, 925023504, 925023505, 925023506, 925023507, 925023508, 925023509, 925023600, 925023601, 925023602, 925023603, 925023604, 925023605, 925023606, 925023607, 925023608, 925023609, 925023700, 925023701, 925023702, 925023703, 925023704, 925023705, 925023706, 925023707, 925023708, 925023709, 925023800, 925023801, 925023802, 925023803, 925023804, 925023805, 925023806, 925023807, 925023808, 925023809, 925030100, 925030101, 925030102, 925030103, 925030104, 925030200, 925030201, 925030202, 925030203, 925030204, 925030300, 925030301, 925030302, 925030303, 925030304, 925030400, 925030401, 925030402, 925030403, 925030404, 925030500, 925030501, 925030502, 925030503, 925030504, 925030600, 925030601, 925030602, 925030603, 925030604, 925030700, 925030701, 925030702, 925030703, 925030704, 925030800, 925030801, 925030802, 925030803, 925030804, 925030900, 925030901, 925030902, 925030903, 925030904, 925031000, 925031001, 925031002, 925031003, 925031004, 925031100, 925031101, 925031102, 925031103, 925031104, 925031200, 925031201, 925031202, 925031203, 925031204, 925031300, 925031301, 925031302, 925031303, 925031304, 925031400, 925031401, 925031402, 925031403, 925031404, 925031500, 925031501, 925031502, 925031503, 925031504, 925031600, 925031601, 925031602, 925031603, 925031604, 925031700, 925031701, 925031702, 925031703, 925031704, 925031800, 925031801, 925031802, 925031803, 925031804, 925031900, 925031901, 925031902, 925031903, 925031904, 925032000, 925032001, 925032002, 925032003, 925032004, 925032100, 925032101, 925032102, 925032103, 925032104, 925032200, 925032201, 925032202, 925032203, 925032204, 925032300, 925032301, 925032302, 925032303, 925032304, 925032400, 925032401, 925032402, 925032403, 925032404, 925032500, 925032501, 925032502, 925032503, 925032504, 925032600, 925032601, 925032602, 925032603, 925032604, 925032700, 925032701, 925032702, 925032703, 925032704, 925032800, 925032801, 925032802, 925032803, 925032804, 925032900, 925032901, 925032902, 925032903, 925032904, 925033000, 925033001, 925033002, 925033003, 925033004, 925033100, 925033101, 925033102, 925033103, 925033104, 925033200, 925033201, 925033202, 925033203, 925033204, 925033300, 925033301, 925033302, 925033303, 925033304, 925033400, 925033401, 925033402, 925033403, 925033404, 925033500, 925033501, 925033502, 925033503, 925033504, 925033600, 925033601, 925033602, 925033603, 925033604, 925033700, 925033701, 925033702, 925033703, 925033704, 925033800, 925033801, 925033802, 925033803, 925033804 
        public Task dojang_Eff()
        {
            // TODO
            getPlayer().resetEnteredScript();
            var stage = (int)Math.Floor(getPlayer().getMap().getId() / 100.0) % 100;

            getPlayer().showDojoClock();
            if (stage % 6 > 0)
            {
                var realstage = stage - ((stage / 6) | 0);
                dojoEnergy();

                playSound("Dojang/start");
                showEffect("dojang/start/stage");
                showEffect("dojang/start/number/" + realstage);
            }
            return Task.CompletedTask;
        }



    }
}