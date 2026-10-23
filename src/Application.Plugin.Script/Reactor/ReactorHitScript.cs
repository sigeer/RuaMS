using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script
{
    // Extra: 1002008, 1002009, 1021002, 1209001, 200000, 200001, 200002, 200003, 200004, 200005, 200006, 200007, 200008, 200009, 2008006, 2008007, 2052001,
    // 2408002, 2408003, 2408004, 2508000, 2618000, 2618001, 2618002, 3009000, 3102000, 8098000,
    // 9018000, 9018001, 9018002, 9018003, 9018004, 9018005, 9108000, 9108001, 9108002, 9108003, 9108004, 9108005, 9208000, 9208001, 9208002, 9208004, 9208007, 9208009, 9400300, 9400301
    internal class ReactorHitScript : ReactorHitScriptBase
    {
        public ReactorHitScript(IChannelClient c, Reactor r)
            : base(c, r) { }

        // Reactor: 2000, 2001
        public async Task mBoxItem0()
        {
            // TODO
            // 2000

            // 2001
        }

        // Reactor: 1002000
        public async Task babyBirdItem0()
        {
            // TODO
            // 1002000
        }

        // Reactor: 1002001
        public async Task BF_item0()
        {
            // TODO
            // 1002001
        }

        // Reactor: 1002002
        public async Task BF_item1()
        {
            // TODO
            // 1002002
        }

        // Reactor: 1002003
        public async Task BF_item2()
        {
            // TODO
            // 1002003
        }

        // Reactor: 1002006
        public async Task BF_item5()
        {
            // TODO
            // 1002006
        }

        // Reactor: 1006000
        public async Task lostDoyoNPC0()
        {
            // TODO
            // 1006000
        }

        // Reactor: 1009000
        public async Task BF_scr0()
        {
            // TODO
            // 1009000
        }

        // Reactor: 1012000
        public async Task vFlowerItem0()
        {
            // TODO
            // 1012000
        }

        // Reactor: 1022001
        public async Task periItem0()
        {
            // TODO
            // 1022001
        }

        // Reactor: 1022002, 1032000, 1202000, 1202004
        public async Task EpisodeQuest0()
        {
            // TODO
            // 1022002

            // 1032000

            // 1202000

            // 1202004
        }

        // Reactor: 1029000
        public async Task scaScript0()
        {
            // TODO
            // 1029000
        }

        // Reactor: 1050000
        public async Task s4berserkMap0()
        {
            // TODO
            // 1050000
        }

        // Reactor: 1052000
        public async Task s4berserkItem0()
        {
            // TODO
            // 1052000
        }


        // Reactor: 1072000
        public async Task vFlowerItem1()
        {
            // TODO
            // 1072000
        }

        // Reactor: 1102000
        public async Task coconut0()
        {
            // TODO
            // 1102000
        }

        // Reactor: 1102001
        public async Task coconut1()
        {
            // TODO
            // 1102001
        }

        // Reactor: 1102002
        public async Task coconut2()
        {
            // TODO
            // 1102002
        }

        // Reactor: 1102003
        public async Task florinaBox0()
        {
            // TODO
            // 1102003
        }

        // Reactor: 1200000
        public async Task ntQuest02()
        {
            // TODO
            // 1200000
        }

        // Reactor: 1202002
        public async Task ntItem03()
        {
            // TODO
            // 1202002
        }

        // Reactor: 1202003
        public async Task EpisodeQuest1()
        {
            // TODO
            // 1202003
        }

        // Reactor: 1209000
        public async Task ntQuest01()
        {
            // TODO
            // 1209000
        }



        // Reactor: 2092000
        public async Task snowdrop()
        {
            // TODO
            // 2092000
        }

        // Reactor: 2092001
        public async Task snowdrop1()
        {
            // TODO
            // 2092001
        }

        // Reactor: 2092002
        public async Task SEAitem0()
        {
            // TODO
            // 2092002
        }

        // Reactor: 2092003
        public async Task canedrop()
        {
            // TODO
            // 2092003
        }

        // Reactor: 2099000
        public async Task snowScript0()
        {
            // TODO
            // 2099000
        }


        // Reactor: 2111000
        public async Task boxMob0()
        {
            // TODO
            // 2111000
        }


        // Reactor: 2112000, 2112007
        public async Task boxItem0()
        {
            // TODO
            // 2112000

            // 2112007
        }

        // Reactor: 2112001, 2112008
        public async Task boxItem1()
        {
            // TODO
            // 2112001

            // 2112008
        }

        // Reactor: 2112002, 2112009
        public async Task boxItem2()
        {
            // TODO
            // 2112002

            // 2112009
        }

        // Reactor: 2112003, 2112010
        public async Task boxItem3()
        {
            // TODO
            // 2112003

            // 2112010
        }

        // Reactor: 2112004, 2112011
        public async Task boxKey0()
        {
            // TODO
            // 2112004

            // 2112011
        }

        // Reactor: 2112005, 2112012
        public async Task boxPaper0()
        {
            // TODO
            // 2112005

            // 2112012
        }

        // Reactor: 2112006
        public async Task money10000()
        {
            // TODO
            // 2112006
        }

        // Reactor: 2112013
        public async Task money100()
        {
            // TODO
            // 2112013
        }

        // Reactor: 2112014
        public async Task boxBItem0()
        {
            // TODO
            // 2112014
        }

        // Reactor: 2112015
        public async Task s4frameItem0()
        {
            // TODO
            // 2112015
        }

        // Reactor: 2112016
        public async Task s4fireHawkItem0()
        {
            // TODO
            // 2112016
        }

        // Reactor: 2112017
        public async Task s4iceEagleItem0()
        {
            // TODO
            // 2112017
        }

        // Reactor: 2119000
        public async Task scaScript1()
        {
            if (getReactor().getState() != 0)
                return;

            await weakenAreaBoss(
                6090000,
                "As the tombstone lit up and vanished, Lich lost all his magic abilities."
            );
        }

        // Reactor: 2119001
        public async Task scaScript2()
        {
            if (getReactor().getState() != 0)
                return;

            await weakenAreaBoss(
                6090000,
                "As the tombstone lit up and vanished, Lich lost all his magic abilities."
            );
        }

        // Reactor: 2119002
        public async Task scaScript3()
        {
            if (getReactor().getState() != 0)
                return;

            await weakenAreaBoss(
                6090000,
                "As the tombstone lit up and vanished, Lich lost all his magic abilities."
            );
        }

        // Reactor: 2119003
        public async Task scaScript4()
        {
            if (getReactor().getState() != 0)
                return;

            await weakenAreaBoss(
                6090000,
                "As the tombstone lit up and vanished, Lich lost all his magic abilities."
            );
        }

        // Reactor: 2119004
        public async Task snowscaScript0()
        {
        }

        // Reactor: 2119005
        public async Task snowscaScript1()
        {
            // TODO
            // 2119005
        }

        // Reactor: 2119006
        public async Task snowscaScript2()
        {
            // TODO
            // 2119006
        }

        // Reactor: 2200000
        public async Task go221024400()
        {
            // TODO
            // 2200000
        }

        // Reactor: 2212000
        public async Task osquest0()
        {
            // TODO
            // 2212000
        }

        // Reactor: 2212001
        public async Task osquest1()
        {
            // TODO
            // 2212001
        }

        // Reactor: 2212002
        public async Task osquest2()
        {
            // TODO
            // 2212002
        }

        // Reactor: 2212003
        public async Task osquest3()
        {
            // TODO
            // 2212003
        }

        // Reactor: 2212004
        public async Task osquest4()
        {
            // TODO
            // 2212004
        }

        // Reactor: 2212005
        public async Task osquest5()
        {
            // TODO
            // 2212005
        }

        // Reactor: 2221000
        public async Task fvMob0()
        {
            // TODO
            // 2221000
        }

        // Reactor: 2221001
        public async Task fvMob1()
        {
            // TODO
            // 2221001
        }

        // Reactor: 2221002
        public async Task fvMob2()
        {
            // TODO
            // 2221002
        }

        // Reactor: 2221003
        public async Task fvquest0()
        {
            // TODO
            // 2221003
        }

        // Reactor: 2221004
        public async Task fvquest1()
        {
            // TODO
            // 2221004
        }

        // Reactor: 2222000
        public async Task fvquest2()
        {
            // TODO
            // 2222000
        }

        // Reactor: 2222001
        public async Task fvevent0()
        {
            // TODO
            // 2222001
        }

        // Reactor: 2222002
        public async Task fvevent1()
        {
            // TODO
            // 2222002
        }

        // Reactor: 2229009
        public async Task fvscaScript0()
        {
            // TODO
            // 2229009
        }


        // Reactor: 2298001
        public async Task hwMob0()
        {
            // TODO
            // 2298001
        }

        // Reactor: 2302000
        public async Task aquaItem0()
        {
            // TODO
            // 2302000
        }

        // Reactor: 2302001
        public async Task aquaItem1()
        {
            // TODO
            // 2302001
        }

        // Reactor: 2302002
        public async Task aquaItem2()
        {
            // TODO
            // 2302002
        }

        // Reactor: 2302003
        public async Task s4resurItem0()
        {
            // TODO
            // 2302003
        }

        // Reactor: 2302005
        public async Task tameItem0()
        {
            // TODO
            // 2302005
        }


        // Reactor: 2401001
        public async Task s4fireHawkMob0()
        {
            // TODO
            // 2401001
        }

        // Reactor: 2401002
        public async Task s4iceEagleMob0()
        {
            // TODO
            // 2401002
        }

        // Reactor: 2402000
        public async Task leafItem0()
        {
            // TODO
            // 2402000
        }

        // Reactor: 2402001
        public async Task leafItem1()
        {
            // TODO
            // 2402001
        }

        
        // Reactor: 2402007, 2402008
        public async Task neoCityItem0()
        {
            // TODO
            // 2402007

            // 2402008
        }

        // Reactor: 2406000
        public async Task hontaleNPC0()
        {
            // TODO
            // 2406000
        }

        // Reactor: 2502000
        public async Task muruengItem0()
        {
            // TODO
            // 2502000
        }

        // Reactor: 2502001
        public async Task muruengItem1()
        {
            // TODO
            // 2502001
        }

        // Reactor: 2502002
        public async Task muruengItem2()
        {
            // TODO
            // 2502002
        }

        // Reactor: 2511000 ~ 2519003 已在 Features/DavyJohn 中实现

        // Reactor: 2602000
        public async Task ariantItem0()
        {
            // TODO
            // 2602000
        }

        // Reactor: 2612000
        public async Task magatiaItem0()
        {
            // TODO
            // 2612000
        }


        // Reactor: 2612004
        public async Task magatiaItem1()
        {
            // JS 的 rm.sprayItems()：以反应堆为中心向四周喷出掉落物
            await dropItems();
        }

        // Reactor: 2612005
        public async Task magatiaItem2()
        {
            // TODO
            // 2612005
        }

        // Reactor: 2612006
        public async Task magatiaItem3()
        {
            // TODO
            // 2612006
        }

        // Reactor: 2612007
        public async Task magatiaItem4()
        {
            // TODO
            // 2612007
        }

        // Reactor: 2619000
        public async Task magaScript0()
        {
            await dropItems();
        }


        // Reactor: 2619003
        public async Task magascaScript0()
        {
            // TODO
            // 2619003
        }

        // Reactor: 2619004
        public async Task magascaScript1()
        {
            // TODO
            // 2619004
        }

        // Reactor: 2619005
        public async Task magascaScript2()
        {
            // TODO
            // 2619005
        }

        // Reactor: 2708000
        public async Task PinkBeenBack0()
        {
            var mapObj = getMap();

            //spawnJrBoss(mapObj.getMonsterById(8820019));
            //spawnJrBoss(mapObj.getMonsterById(8820020));
            //spawnJrBoss(mapObj.getMonsterById(8820021));
            //spawnJrBoss(mapObj.getMonsterById(8820022));
            //spawnJrBoss(mapObj.getMonsterById(8820023));

            await mapObj.killMonster(8820000);
        }

        // Reactor: 2709000
        public async Task PinkBeenScript0()
        {
            // TODO
            // 2709000
        }

        



        // Reactor: 6741001
        public async Task guyfawkesmob0()
        {
            // TODO
            // 6741001
        }

        // Reactor: 6741015
        public async Task guyfawkesmob1()
        {
            // TODO
            // 6741015
        }

        // Reactor: 6741016
        public async Task guyfawkesmob2()
        {
            // TODO
            // 6741016
        }

        // Reactor: 6742014
        public async Task guyfawkesbox0()
        {
            // TODO
            // 6742014
        }

        // Reactor: 6802000
        public async Task weddingItem0()
        {
            // TODO
            // 6802000
        }

        // Reactor: 6802001
        public async Task weddingItem1()
        {
            // TODO
            // 6802001
        }

        // Reactor: 6822000
        public async Task halloweenGLitem()
        {
            // TODO
            // 6822000
        }

        // Reactor: 6829000
        public async Task halloweenbox()
        {
            // TODO
            // 6829000
        }

        // Reactor: 8001000
        public async Task shouwaBoss()
        {
            // TODO
            // 8001000
        }

        // Reactor: 8091000
        public async Task JPludiMob0()
        {
            // TODO
            // 8091000
        }

        // Reactor: 8091001
        public async Task JPludiMob1()
        {
            // TODO
            // 8091001
        }

        // Reactor: 8091002
        public async Task JPludiMob2()
        {
            // TODO
            // 8091002
        }

        // Reactor: 8091003
        public async Task JPludiMob3()
        {
            // TODO
            // 8091003
        }

        // Reactor: 8091004
        public async Task JPludiMob4()
        {
            // TODO
            // 8091004
        }


        // Reactor: 9000000
        public async Task eventMap0()
        {
            // TODO
            // 9000000
        }

        // Reactor: 9000001
        public async Task eventMap1()
        {
            // TODO
            // 9000001
        }

        // Reactor: 9000002
        public async Task eventMap2()
        {
            // TODO
            // 9000002
        }

        // Reactor: 9001000
        public async Task eventMob0()
        {
            // TODO
            // 9001000
        }

        // Reactor: 9002000
        public async Task eventItem0()
        {
            // TODO
            // 9002000
        }

        // Reactor: 9002001
        public async Task eventItem1()
        {
            // TODO
            // 9002001
        }

        // Reactor: 9002002
        public async Task eventItem2()
        {
            // TODO
            // 9002002
        }

        // Reactor: 9102000
        public async Task sBoxItem0()
        {
            // TODO
            // 9102000
        }

        // Reactor: 9102001
        public async Task sBoxItem1()
        {
            // TODO
            // 9102001
        }


        // Reactor: 9102008
        public async Task dropTreasureBox()
        {
            // TODO
            // 9102008
        }

        // Reactor: 9201000
        public async Task syarenMob0()
        {
            // TODO
            // 9201000
        }

        // Reactor: 9201001
        public async Task syarenNPC0()
        {
            // TODO
            // 9201001
        }

        // Reactor: 9201002
        public async Task syarenMob1()
        {
            // TODO
            // 9201002
        }

        // Reactor: 9202000
        public async Task syarenItem0()
        {
            // TODO
            // 9202000
        }

        // Reactor: 9202001
        public async Task syarenItem1()
        {
            // TODO
            // 9202001
        }

        // Reactor: 9202002
        public async Task syarenItem2()
        {
            // TODO
            // 9202002
        }

        // Reactor: 9202003
        public async Task syarenItem3()
        {
            // TODO
            // 9202003
        }

        // Reactor: 9202004
        public async Task syarenItem4()
        {
            // TODO
            // 9202004
        }

        // Reactor: 9202005
        public async Task syarenItem5()
        {
            // TODO
            // 9202005
        }

        // Reactor: 9202006
        public async Task syarenItem6()
        {
            // TODO
            // 9202006
        }

        // Reactor: 9202007
        public async Task syarenItem7()
        {
            // TODO
            // 9202007
        }

        // Reactor: 9202008
        public async Task syarenItem8()
        {
            // TODO
            // 9202008
        }

        // Reactor: 9202009
        public async Task syarenItem9()
        {
            // TODO
            // 9202009
        }

        // Reactor: 9202010
        public async Task syarenItem10()
        {
            // TODO
            // 9202010
        }

        // Reactor: 9202011
        public async Task syarenItem11()
        {
            // TODO
            // 9202011
        }

        // Reactor: 9202012
        public async Task syarenItem12()
        {
            // TODO
            // 9202012
        }

        // Reactor: 9222000
        [ScriptName("6th_item0")]
        public async Task s_6th_item0()
        {
            // TODO
            // 9222000
        }

        // Reactor: 9702000
        [ScriptName("5thItem0")]
        public async Task s_5thItem0()
        {
            // TODO
            // 9702000
        }

        

        // Reactor: 9902000
        public async Task PB_boxCount()
        {
            // TODO
            // 9902000
        }
    }
}
