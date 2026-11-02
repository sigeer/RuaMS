using Application.Core.Channel.Net.Packets;
using Application.Core.Client;
using Application.Core.Server.life;
using Application.Core.Server.maps;
using Application.Shared.Constants.Item;
using Application.Shared.Constants.Mob;
using scripting.reactor;
using System.Drawing;

namespace Application.Plugin.Script
{
    // Extra: 1002008, 1002009, 1021002, 1209001, 200000, 200001, 200002, 200003, 200004, 200005, 200006, 200007, 200008, 200009,
    // 2008006, 2008007, 2052001, 2408002, 2408003, 2408004, 2508000, 2618000, 2618001, 2618002, 3009000, 3102000, 8098000,
    // 9018000, 9018001, 9018002, 9018003, 9018004, 9018005
    // 9108000, 9108001, 9108002, 9108003, 9108004, 9108005,
    // 9208000, 9208001, 9208002, 9208004, 9208007, 9208009, 9400300, 9400301
    internal partial class ReactorActScript : ReactorActScriptBase
    {
        public ReactorActScript(IChannelClient c, Reactor r) : base(c, r)
        {
        }

        // Reactor: 2000, 2001 
        public async Task mBoxItem0()
        {
            await dropItems(true, 2, 8, 15, 1);

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
            await dropItems(true, 2, 20, 40);


        }


       


        // Reactor: 1022001 
        public async Task periItem0()
        {
            await dropItems();


        }


        // Reactor: 1022002, 1032000, 1202000, 1202004 
        public async Task EpisodeQuest0()
        {
            await dropItems();


        }


        // Reactor: 1029000 
        public async Task scaScript0()
        {
            if (isAllReactorState(1029000, 0x04))
            { // 0x04 appears to be the destroyed state
                await killMonster(3230300);
                await killMonster(3230301);
                await LightBlue("Once the rock crumbled, Jr. Boogie was in great pain and disappeared.");
            }


        }


        // Reactor: 1050000 
        public async Task s4berserkMap0()
        {
            if (Random.Shared.NextDouble() > 0.7)
            {
                await dropItems();
            }
            else
            {
                await warp(105090200, 0);
            }


        }


        // Reactor: 1052000 
        public async Task s4berserkItem0()
        {
            await dropItems();


        }


        


        // Reactor: 1058015 已在 Features/Exped_Balrog 中实现


        // Reactor: 1072000 
        public async Task vFlowerItem1()
        {
            await dropItems();


        }


        // Reactor: 1102000 
        public async Task coconut0()
        {
            await dropItems();


        }


        // Reactor: 1102001 
        public async Task coconut1()
        {
            await dropItems();


        }


        // Reactor: 1102002 
        public async Task coconut2()
        {
            await dropItems();


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
            // string visibility thanks to ProXAIMeRx & Glvelturall
            await Pink("Failed to find Bart. Returning to the original location.");
            await warp(120000102);


        }


        // Reactor: 1202002 
        public async Task ntItem03()
        {
            await dropItems();


        }


        // Reactor: 1202003 
        public async Task EpisodeQuest1()
        {
            await dropItems();


        }





        // Reactor: 1302000 
        public async Task erebItem0()
        {
            await dropItems(true, 2, 8, 12, 2);


        }


        // Reactor: 1402000 
        public async Task rienItem0()
        {
            await dropItems(true, 2, 8, 15);


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
            await dropItems();


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


        // Reactor: 2110000 
        public async Task go280010000()
        {
            await Pink("未知力量将你传送至起始点！");
            await warp(280010000, 0);
        }


        // Reactor: 2111000 
        public async Task boxMob0()
        {
            await Pink("Oh noes! Monsters in the chest!");
            await spawnMonster(9300004, 3);


        }





        [ScriptName("boxItem0", "boxItem1", "boxItem2", "boxItem3")]
        public async Task BoxItem()
        {
            await dropItems();


        }


        // Reactor: 2112004, 2112011 
        public async Task boxKey0()
        {
            await dropItems();


        }


        // Reactor: 2112005, 2112012 
        public async Task boxPaper0()
        {
            await dropItems();


        }


        // Reactor: 2112006 
        public async Task money10000()
        {
            await dropItems(true, 1, 500, 800);


        }


        // Reactor: 2112013 
        public async Task money100()
        {
            await dropItems(true, 1, 125, 175);


        }


        // Reactor: 2112014 
        public async Task boxBItem0()
        {
            await dropItems();


        }


        // Reactor: 2112015 
        public async Task s4frameItem0()
        {
            await dropItems();


        }


        // Reactor: 2112016 
        public async Task s4fireHawkItem0()
        {
            await dropItems();


        }


        // Reactor: 2112017 
        public async Task s4iceEagleItem0()
        {
            await dropItems();


        }


        // Reactor: 2119000 
        public async Task scaScript1()
        {
            // JS 的 act() 为空函数：若在 Riche 之前摧毁宝箱，击杀他将不会获得经验
            return;
        }


        // Reactor: 2119001 
        public async Task scaScript2()
        {
            // JS 的 act() 为空函数：若在 Riche 之前摧毁宝箱，击杀他将不会获得经验
            return;
        }


        // Reactor: 2119002 
        public async Task scaScript3()
        {
            // JS 的 act() 为空函数
            return;
            // If the chest is destroyed before Riche, killing him should yield no exp


        }


        // Reactor: 2119003 
        public async Task scaScript4()
        {
            // JS 的 act() 为空函数
            return;
            // If the chest is destroyed before Riche, killing him should yield no exp


        }


        [ScriptName("snowscaScript0", "snowscaScript1", "snowscaScript2")]
        public async Task SnowscaScript()
        {
            await weakenAreaBoss(6090001, "The light at the altar appeases the hatred of the Snow Witch. The force of the Witch has weakened.");
        }


        // Reactor: 2200001 
        public async Task ludiPotal0()
        {
            await Pink("已发现隐藏的机械工厂！");
            await warp(Random.Shared.NextDouble() < .5 ? 922000020 : 922000021, 0);
        }


        // Reactor: 2200002 
        public async Task go922010201()
        {
            await mapMessage(5, "An unknown force has warped you into a trap.");
            warpMap(922010201);


        }


        // Reactor: 2202001 
        public async Task ludiquest1()
        {
            await dropItems();


        }


        // Reactor: 2202002 
        public async Task ludiquest2()
        {
            if (isQuestActive(3238))
            {
                await warp(922000020, 0);
            }
            else
            {
                await warp(922000009, 0);
            }


        }


        // Reactor: 2202003 
        public async Task ludiquest3()
        {
            await dropItems();


        }


        // Reactor: 2202004 
        public async Task ludiquest4()
        {
            await dropItems(true, 1, 30, 60, 15);


        }


        // Reactor: 2212000 
        public async Task osquest0()
        {
            await dropItems(true, 2, 80, 100);


        }


        // Reactor: 2212001 
        public async Task osquest1()
        {
            await dropItems(true, 2, 80, 100);


        }


        // Reactor: 2212002 
        public async Task osquest2()
        {
            await dropItems(true, 2, 80, 100);


        }


        // Reactor: 2212003 
        public async Task osquest3()
        {
            await dropItems(true, 2, 80, 100);


        }


        // Reactor: 2212004 
        public async Task osquest4()
        {
            await dropItems(true, 2, 80, 100);


        }


        // Reactor: 2212005 
        public async Task osquest5()
        {
            await dropItems();


        }


        // Reactor: 2221000 
        public async Task fvMob0()
        {
            await spawnMonster(7130400);
            await mapMessage(5, "Here comes Yellow King Goblin!");


        }


        // Reactor: 2221001 
        public async Task fvMob1()
        {
            await spawnMonster(7130401);
            await mapMessage(5, "Here comes Blue King Goblin!");


        }


        // Reactor: 2221002 
        public async Task fvMob2()
        {
            await spawnMonster(7130402, -340, 100);
            await mapMessage(5, "Here comes Green King Goblin!");


        }


        // Reactor: 2221003 
        public async Task fvquest0()
        {
            await spawnMonster(9500400);


        }


        // Reactor: 2221004 
        public async Task fvquest1()
        {
            await spawnMonster(9500400);


        }


        // Reactor: 2222000 
        public async Task fvquest2()
        {
            await dropItems(true, 2, 80, 120);


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

            await weakenAreaBoss(6090003, "The grieving Scholar Ghost has been slightly appeased. You may be able to defeat the Scholar Ghost.");


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
            await dropItems(true, 2, 75, 90);


        }


        // Reactor: 2302001 
        public async Task aquaItem1()
        {
            //dropItems(true, 2, 105, 140);

            await dropItems();


        }


        // Reactor: 2302002 
        public async Task aquaItem2()
        {
            await dropItems(true, 2, 55, 70);


        }


        // Reactor: 2302003 
        public async Task s4resurItem0()
        {
            await dropItems();


        }


        // Reactor: 2302005 
        public async Task tameItem0()
        {
            await dropItems();


        }


        // Reactor: 2401001 
        public async Task s4fireHawkMob0()
        {
            await spawnMonster(9300089);


        }


        // Reactor: 2401002 
        public async Task s4iceEagleMob0()
        {
            await spawnMonster(9300090);


        }


        // Reactor: 2402000 
        public async Task leafItem0()
        {
            await dropItems();


        }


        // Reactor: 2402001 
        public async Task leafItem1()
        {
            await dropItems();


        }


        


        // Reactor: 2402007, 2402008 
        public async Task neoCityItem0()
        {
            await dropItems(true, 2, 5, 10, 1);


        }


        // Reactor: 2406000 
        public async Task hontaleNPC0()
        {
            await spawnNpc(2081008);
            await startQuest(100203);
            await mapMessage(6, "光芒闪烁间，龙蛋破壳而出，一只璀璨的幼龙降临世间！");
        }


        // Reactor: 2502000 
        public async Task muruengItem0()
        {
            await dropItems();


        }


        // Reactor: 2502001 
        public async Task muruengItem1()
        {
            await dropItems();


        }


        // Reactor: 2502002 
        public async Task muruengItem2()
        {
            await dropItems();


        }



        // Reactor: 2602000 
        public async Task ariantItem0()
        {
            await dropItems();


        }


        // Reactor: 2612000 
        public async Task magatiaItem0()
        {
            await dropItems();


        }


        


        // Reactor: 2612004 
        public async Task magatiaItem1()
        {
            // JS 的 act() 为空函数：掉落逻辑在 hit() 中处理
            return;
        }


        // Reactor: 2612005 
        public async Task magatiaItem2()
        {
            await dropItems();


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
            // JS 的 act() 为空函数：状态 1 会在 3 秒后自动回落到 0；
            // 反应堆被摧毁（状态 2）时触发此处，仅在一次性掉落两个魔法装置时才会发生，正常情况下不会出现
            return;
        }



        // Reactor: 2619003 
        public async Task magascaScript0()
        {
            await weakenAreaBoss(6090004, "Rurumo has been poisoned. It may finally be defeatable!");


        }


        // Reactor: 2619004 
        public async Task magascaScript1()
        {
            await weakenAreaBoss(6090004, "Rurumo has been poisoned. It may finally be defeatable!");


        }


        // Reactor: 2619005 
        public async Task magascaScript2()
        {
            await weakenAreaBoss(6090004, "Rurumo has been poisoned. It may finally be defeatable!");


        }


        // Reactor: 2708000 
        public async Task PinkBeenBack0()
        {
            // JS 的 act() 为空函数：击杀逻辑在 hit() 中处理
            return;
        }


        // Reactor: 2709000 — 粉豆召唤：移除占位怪物，由事件模板生成真正的粉豆
        public async Task PinkBeenScript0()
        {
            await getMap().killMonster(8820000);
        }



        // Reactor: 5411000 
        public async Task sgboss0()
        {
            await changeMusic("Bgm09/TimeAttack");
            await spawnMonster(9420513, -146, 225);
            GetEventInstanceTrust().setIntProperty("boss", 1);
            await mapMessage(5, "As you wish, here comes Capt Latanica.");


        }


        // Reactor: 6741001 
        public async Task guyfawkesmob0()
        {
            await spawnMonster(9400589);


        }


        // Reactor: 6741015 
        public async Task guyfawkesmob1()
        {
            await dropItems();


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
            await dropItems(true, 1, 5, 25, 15);


        }


        // Reactor: 6802000 
        public async Task weddingItem0()
        {
            await dropItems(true, 1, 100, 400, 15);


        }


        // Reactor: 6802001 
        public async Task weddingItem1()
        {
            await dropItems(true, 1, 100, 400, 15);


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
            //wtf is this?
            await Pink("Enjoy Halloween!");
            await spawnMonster(9400202, 10);


        }




        // Reactor: 8091000 
        public async Task JPludiMob0()
        {
            await spawnMonster(9400210, 2);
            await spawnMonster(9400209, 2);
            await mapMessage(5, "Some monsters are summoned.");


        }


        // Reactor: 8091001 
        public async Task JPludiMob1()
        {
            await spawnMonster(9400211, 2);
            await spawnMonster(9400212, 2);
            await mapMessage(5, "Some monsters are summoned.");


        }


        // Reactor: 8091002 
        public async Task JPludiMob2()
        {
            await spawnMonster(9400213, 2);
            await spawnMonster(9400214, 2);
            await mapMessage(5, "Some monsters are summoned.");


        }


        // Reactor: 8091003 
        public async Task JPludiMob3()
        {
            await spawnMonster(9400215, 2);
            await spawnMonster(9400216, 2);
            await mapMessage(5, "Some monsters are summoned.");


        }


        // Reactor: 8091004 
        public async Task JPludiMob4()
        {
            await spawnMonster(9400217, 2);
            await spawnMonster(9400218, 2);
            await mapMessage(5, "Some monsters are summoned.");


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
            await dropItems(true, 2, 25, 100);
        }


        // Reactor: 9102001 
        public async Task sBoxItem1()
        {
            await dropItems(true, 2, 25, 100);
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
            await spawnMonster(9300033, 8, -100, 50);
        }


        // Reactor: 9201001 
        public async Task syarenNPC0()
        {
            await mapMessage(5, "A bright flash of light, then someone familiar appears in front of the blocked gate.");
            await spawnNpc(9040003);
        }


        // Reactor: 9201002 
        public async Task syarenMob1()
        {
            await changeMusic("Bgm10/Eregos");
            await spawnMonster(9300028);
            await spawnMonster(9300031, 130, 90);
            await spawnMonster(9300032, 540, 90);
            await spawnMonster(9300029, 130, 150);
            await spawnMonster(9300030, 540, 150);
        }


        // Reactor: 9202000 
        public async Task syarenItem0()
        {
            await dropItems();


        }


        // Reactor: 9202001 
        public async Task syarenItem1()
        {
            await dropItems();


        }


        // Reactor: 9202002 
        public async Task syarenItem2()
        {
            await dropItems();


        }


        // Reactor: 9202003 
        public async Task syarenItem3()
        {
            await dropItems();


        }


        // Reactor: 9202004 
        public async Task syarenItem4()
        {
            await dropItems();


        }


        // Reactor: 9202005 
        public async Task syarenItem5()
        {
            await dropItems();


        }


        // Reactor: 9202006 
        public async Task syarenItem6()
        {
            await dropItems();


        }


        // Reactor: 9202007 
        public async Task syarenItem7()
        {
            await dropItems();


        }


        // Reactor: 9202008 
        public async Task syarenItem8()
        {
            await dropItems();


        }


        // Reactor: 9202009 
        public async Task syarenItem9()
        {
            await dropItems();


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
            await dropItems(true, 1, 30, 60, 10);


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