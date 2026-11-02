using Application.Core.Client;
using Application.Core.Game.Maps;
using Application.Core.Server.life;
using Application.Shared.Constants.Map;
using Application.Utility;
using System.Drawing;
using scripting.map;

namespace Application.Plugin.Script
{

    // 地图脚本：Map.wz 的 onUserEnter 与 onFirstUserEnter 共用同一实现，
    // 两者只是触发时机不同，因此不再区分种类。
    //
    // Extra:
    // 108010600, 108010610, 108010620, 108010630, 108010640,
    // 130030000, 130030001,
    // 677000001, 677000003, 677000005, 677000007, 677000009, 677000012,
    // 910510000, 912000000, 925040100, 926000000, 926010000, 926120300,
    // babyPigMap, cannon_tuto_01, cannon_tuto_direction, cannon_tuto_direction1, cannon_tuto_direction2,
    // crash_Dragon, evanleaveD, getDragonEgg, meetWithDragon,
    // PromiseDragon, Resi_tutor10 ~ Resi_tutor80
    internal partial class MapScript : MapScriptMethods
    {
        public MapScript(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 0 
        [ScriptName("goAdventure")]
        public async Task s_goAdventure()
        {
            await goAdventure();
        }

        // Map: 10000
        public async Task go10000()
        {
            await unlockUI();
            await mapEffect("maplemap/enter/10000");

        }

        // Map: 20000
        public async Task go20000()
        {
            await unlockUI();
            await mapEffect("maplemap/enter/20000");

        }

        // Map: 30000
        public async Task go30000()
        {
            await mapEffect("maplemap/enter/30000");

        }

        // Map: 40000
        public async Task go40000()
        {
            await mapEffect("maplemap/enter/40000");

        }

        // Map: 50000
        public async Task go50000()
        {
            await mapEffect("maplemap/enter/50000");
        }

        // Map: 1000000
        public async Task go1000000()
        {
            await mapEffect("maplemap/enter/1000000");

        }

        // Map: 1010000
        public async Task go1010000()
        {
            await mapEffect("maplemap/enter/1010000");

        }

        // Map: 1010100
        public async Task go1010100()
        {
            await mapEffect("maplemap/enter/1010100");

        }

        // Map: 1010200
        public async Task go1010200()
        {
            await mapEffect("maplemap/enter/1010200");

        }

        // Map: 1010300
        public async Task go1010300()
        {
            await mapEffect("maplemap/enter/1010300");

        }

        // Map: 1010400
        public async Task go1010400()
        {
            // TODO

        }

        // Map: 1020000
        public async Task go1020000()
        {
            await unlockUI();
            await mapEffect("maplemap/enter/1020000");

        }

        // Map: 1020100
        public async Task goSwordman()
        {
            await startExplorerExperience();

        }

        // Map: 1020200
        public async Task goMagician()
        {
            await startExplorerExperience();

        }

        // Map: 1020300
        public async Task goArcher()
        {
            await startExplorerExperience();

        }

        // Map: 1020400
        public async Task goRogue()
        {
            await startExplorerExperience();

        }

        // Map: 1020500
        public async Task goPirate()
        {
            await startExplorerExperience();

        }

        // Map: 2000000
        public async Task go2000000()
        {
            await mapEffect("maplemap/enter/2000000");

        }

        // Map: 2010000 
        [ScriptName("goLith")]
        public async Task s_goLith()
        {
            await goLith();

        }

        // Map: 100000000, 100000003, 100010000, 100040000, 100040100, 101000000 … (共 97 张)
        public async Task explorationPoint()
        {
            if (getPlayer().getMapId() == 110000000 || (getPlayer().getMapId() >= 100000000 && getPlayer().getMapId() < 105040300))
            {
                await explorerQuest(29005);//Beginner Explorer
            }
            else if (getPlayer().getMapId() >= 105040300 && getPlayer().getMapId() <= 105090900)
            {
                await explorerQuest(29014);//Sleepywood Explorer
            }
            else if (getPlayer().getMapId() >= 200000000 && getPlayer().getMapId() <= 211041800)
            {
                await explorerQuest(29006);//El Nath Mts. Explorer
            }
            else if (getPlayer().getMapId() >= 220000000 && getPlayer().getMapId() <= 222020000)
            {
                await explorerQuest(29007);//Ludus Lake Explorer
            }
            else if (getPlayer().getMapId() >= 230000000 && getPlayer().getMapId() <= 230040401)
            {
                await explorerQuest(29008);//Undersea Explorer
            }
            else if (getPlayer().getMapId() >= 250000000 && getPlayer().getMapId() <= 251010500)
            {
                await explorerQuest(29009);//Mu Lung Explorer
            }
            else if (getPlayer().getMapId() >= 260000000 && getPlayer().getMapId() <= 261030000)
            {
                await explorerQuest(29010);//Nihal Desert Explorer
            }
            else if (getPlayer().getMapId() >= 240000000 && getPlayer().getMapId() <= 240050000)
            {
                await explorerQuest(29011);//Minar Forest Explorer
            }
            if (getPlayer().getMapId() == 104000000)
            {
                await mapEffect("maplemap/enter/104000000");
            }

        }

        // Map: 100000006 
        [ScriptName("100000006")]
        public async Task s_100000006()
        {
            // TODO

        }

        // Map: 103000300, 103000301, 103000302
        public async Task Depart_inSubway()
        {
            // TODO

        }



        // Map: 109090100, 109090101, 109090102, 109090103, 109090104, 910040110 … (共 11 张)
        public async Task BF_sheepChat()
        {
            // TODO

        }

        // Map: 109090200, 109090201, 109090202, 109090203, 109090204, 910040120 … (共 11 张)
        public async Task BF_wolfChat()
        {
            // TODO

        }

        // Map: 240000110
        public async Task undomorphdarco()
        {
            // TODO

        }

        // Map: 240070203, 240070204, 240070205, 240070206, 240070207, 240070208 … (共 35 张)
        public async Task TD_neo_BossEnter()
        {
            // TODO

        }

        // Map: 270000100
        public async Task reundodraco()
        {
            // TODO

        }

        // Map: 910020100, 910020101, 910020102, 910020103, 910020104, 910020200 … (共 16 张)
        public async Task delDoyo()
        {
            // TODO

        }

        // Map: 910030001
        public async Task undoMorph_jepeto()
        {
            // TODO

        }

        // Map: 910320001, 926010001, 926010002, 926010003, 926010004
        public async Task Massacre_result()
        {
            var py = getPyramid();
            if (py != null)
            {
                await py.sendScore(getPlayer());
            }

        }

        // Map: 910320100, 910320101, 910320102, 910320103, 910320104, 910330100 … (共 75 张)
        public async Task Massacre_first()
        {
            // TODO

        }

        // Map: 910320200, 910320201, 910320202, 910320203, 910320204, 910320300 … (共 10 张)
        public async Task metro_firstSetting()
        {
            // TODO

        }

        // Map: 913040000, 913040001, 913040002, 913040003, 913040004, 913040005 … (共 8 张)
        public async Task cygnusTest()
        {
            // TODO

        }

        // Map: 913040100, 913040101, 913040102, 913040103, 913040104, 913040105 … (共 7 张)
        public async Task cygnusJobTutorial()
        {
            await displayCygnusIntro();

        }

        // Map: 914000000, 914000300, 914000400, 914000410, 914000420, 914000500
        public async Task aranTutorAlone()
        {
            // TODO

        }

        // Map: 914090000, 914090001, 914090002, 914090003, 914090004, 914090005 … (共 17 张)
        public async Task aranDirection()
        {
            string intro = (c.OnlinedCharacter.getMapId()) switch
            {
                MapId.ARAN_TUTO_1 => "Effect/Direction1.img/aranTutorial/Scene0",
                MapId.ARAN_TUTO_2 => "Effect/Direction1.img/aranTutorial/Scene1" + (c.OnlinedCharacter.getGender() == 0 ? "0" : "1"),
                MapId.ARAN_TUTO_3 => "Effect/Direction1.img/aranTutorial/Scene2" + (c.OnlinedCharacter.getGender() == 0 ? "0" : "1"),
                MapId.ARAN_TUTO_4 => "Effect/Direction1.img/aranTutorial/Scene3",
                MapId.ARAN_POLEARM => "Effect/Direction1.img/aranTutorial/HandedPoleArm" + (c.OnlinedCharacter.getGender() == 0 ? "0" : "1"),
                MapId.ARAN_MAHA => "Effect/Direction1.img/aranTutorial/Maha",
                _ => ""
            };
            await lockUI();
            await ShowDirectionEffect(intro);
        }

    

        // Map: 109090100, 109090101, 109090102, 109090103, 109090104, 910040110 … (共 11 张)
        public async Task BF_sheepSet()
        {
            // TODO

        }

        // Map: 109090200, 109090201, 109090202, 109090203, 109090204, 910040120 … (共 11 张)
        public async Task BF_wolfSet()
        {
            // TODO

        }

        // Map: 677000012
        public async Task astaroth_summon()
        {
            var pos = new Point(842, 0);
            var mobId = 9400633;

            var mobobj = LifeFactory.Instance.GetMonsterTrust(mobId);
            var mobName = c.CurrentCulture.GetMobName(mobId);
            await getMap().spawnMonsterOnGroundBelow(mobobj, pos);
            await Pink(mobName + " 已现身！");
        }

        // Map: 910031000, 910031001, 910031002, 910031003, 910031004, 910031005 … (共 30 张)
        public async Task snack_intro()
        {
            // TODO

        }

        // Map: 910320010, 910320011, 910320012, 910320013, 910320014, 910320015 … (共 100 张)
        public async Task killing_BonusSetting()
        {
            // TODO

        }

        // Map: 910320100, 910320101, 910320102, 910320103, 910320104, 910320200 … (共 345 张)
        public async Task killing_MapSetting()
        {
            // TODO

        }

        // Map: 910520000, 910520001, 910520002, 910520003, 910520004
        public async Task tristan_questMap()
        {
            // TODO

        }

        // Map: 980041200, 980041201, 980041202, 980041203, 980041204, 980041205 … (共 35 张)
        public async Task onRewordMap()
        {
            // TODO

        }    
    }
}
