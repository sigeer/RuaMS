using Application.Core.Channel.Net.Packets;
using Application.Core.Client;
using Application.Shared.Constants.Item;
using Application.Shared.Constants.Mob;
using scripting.reactor;
using server.life;
using server.maps;
using System.Drawing;

namespace Application.Plugin.Script
{
    // Extra: 1002008, 1002009, 1021002, 1209001, 200000, 200001, 200002, 200003, 200004, 200005, 200006, 200007, 200008, 200009,
    // 2008006, 2008007, 2052001, 2408002, 2408003, 2408004, 2508000, 2618000, 2618001, 2618002, 3009000, 3102000, 8098000,
    // 9018000, 9018001, 9018002, 9018003, 9018004, 9018005
    // 9108000, 9108001, 9108002, 9108003, 9108004, 9108005,
    // 9208000, 9208001, 9208002, 9208004, 9208007, 9208009, 9400300, 9400301
    internal class ReactorActScript : ReactorActionManager
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


        // Reactor: 1020000 
        public async Task s4hitmanMap0()
        {
            await warp(910200000, "pt00");
        }


        // Reactor: 1020001 
        public async Task s4hitmanMap1()
        {
            await warp(910200000, "pt01");
        }


        // Reactor: 1020002 
        public async Task s4hitmanMap2()
        {
            await warp(910200000, "pt02");
        }


        // Reactor: 1020003 
        public async Task s4hitmanMap3()
        {
            // TODO
            // 1020003



        }


        // Reactor: 1020004 
        public async Task s4hitmanMap4()
        {
            // TODO
            // 1020004



        }


        // Reactor: 1020005 
        public async Task s4hitmanMap5()
        {
            // TODO
            // 1020005



        }


        // Reactor: 1020006 
        public async Task s4hitmanMap6()
        {
            // TODO
            // 1020006



        }


        // Reactor: 1020007 
        public async Task s4hitmanMap7()
        {
            // TODO
            // 1020007



        }


        // Reactor: 1020008 
        public async Task s4hitmanMap8()
        {
            // TODO
            // 1020008



        }


        // Reactor: 1021000 
        public async Task s4hitmanMob0()
        {
            await spawnMonster(9300091);


        }


        // Reactor: 1021001 
        public async Task s4hitmanMob1()
        {
            await spawnMonster(9300091);


        }


        // Reactor: 1022000 
        public async Task s4hitmanItem0()
        {
            await dropItems();


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


        // Reactor: 1052001, 1052002, 1052003 
        public async Task balogItem0()
        {
            await dropItems(true, 1, 500, 1000, 15);



        }


        // Reactor: 1058001, 1058003, 1058004, 1058011, 1058013, 1058014 
        public async Task minibalogSummon()
        {
            // TODO
            // 1058001

            // 1058003

            // 1058004

            // 1058011

            // 1058013

            // 1058014



        }


        // Reactor: 1058005 
        public async Task balogReactor()
        {
            // TODO
            // 1058005



        }


        // Reactor: 1058015 
        public async Task Easy_balogReactor()
        {
            // TODO
            // 1058015



        }


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


        // Reactor: 1209000 
        public async Task ntQuest01()
        {
            // string visibility thanks to ProXAIMeRx & Glvelturall
            if (isQuestStarted(6400))
            {
                await setQuestProgress(6400, 1, 2);
                await setQuestProgress(6400, 6401, "q3");
            }

            await Pink("Real Bart has been found. Return to Jonathan through the portal.");


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


        // Reactor: 2001000 
        public async Task fgodMob0()
        {
            if (getMap().getSummonState())
            {
                var count = GetEventInstanceTrust().getIntProperty("statusStg7_c");

                if (count < 7)
                {
                    var nextCount = (count + 1);
                    // 修复召唤出黑花后，禁止再召唤黑花，防止精灵爸爸重复出现
                    var monsterId = Random.Shared.NextDouble() >= .6 ? 9300049 : 9300048;
                    if (monsterId == 9300049)
                    {
                        getMap().allowSummonState(false);
                    }
                    await spawnMonster(monsterId);
                    GetEventInstanceTrust().setProperty("statusStg7_c", nextCount);
                }
                else
                {
                    await spawnMonster(9300049);
                    getMap().allowSummonState(false);
                }
            }


        }


        // Reactor: 2001001 
        public async Task fgodMob1()
        {
            if (getMap().getSummonState())
            {
                var count = GetEventInstanceTrust().getIntProperty("statusStg7_c");

                if (count < 7)
                {
                    var nextCount = (count + 1);
                    // 修复召唤出黑花后，禁止再召唤黑花，防止精灵爸爸重复出现
                    var monsterId = Random.Shared.NextDouble() >= .6 ? 9300049 : 9300048;
                    if (monsterId == 9300049)
                    {
                        getMap().allowSummonState(false);
                    }
                    await spawnMonster(monsterId);
                    GetEventInstanceTrust().setProperty("statusStg7_c", nextCount);
                }
                else
                {
                    await spawnMonster(9300049);
                    getMap().allowSummonState(false);
                }
            }


        }


        // Reactor: 2001002 
        public async Task fgodMob2()
        {
            var eim = GetEventInstanceTrust();
            if (eim.getIntProperty("statusStg2") == -1)
            {
                var rnd = Math.Max(Math.Floor(Random.Shared.NextDouble() * 14), 4);

                eim.setProperty("statusStg2", "" + rnd);
                eim.setProperty("statusStg2_c", "0");
            }

            var limit = eim.getIntProperty("statusStg2");
            var count = eim.getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                await dropItems();

                await eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                await eim.showClearEffect(true);
            }
            else
            {
                count++;
                eim.setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                await spawnMonster(9300040, 1, nextPos);
            }


        }


        // Reactor: 2001003 
        public async Task fgodMob3()
        {
            if (GetEventInstanceTrust().getIntProperty("statusStg2") == -1)
            {
                var rnd = (int)Math.Floor(Random.Shared.NextDouble() * 14);

                GetEventInstanceTrust().setProperty("statusStg2", "" + rnd);
                GetEventInstanceTrust().setProperty("statusStg2_c", "0");
            }

            var limit = GetEventInstanceTrust().getIntProperty("statusStg2");
            var count = GetEventInstanceTrust().getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                await dropItems();

                var eim = GetEventInstanceTrust();
                await eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                await eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                await spawnMonster(9300040, 1, nextPos);
            }


        }


        // Reactor: 2001004 
        public async Task fgodMob4()
        {
            if (GetEventInstanceTrust().getIntProperty("statusStg2") == -1)
            {
                var rnd = (int)Math.Floor(Random.Shared.NextDouble() * 14);

                GetEventInstanceTrust().setProperty("statusStg2", "" + rnd);
                GetEventInstanceTrust().setProperty("statusStg2_c", "0");
            }

            var limit = GetEventInstanceTrust().getIntProperty("statusStg2");
            var count = GetEventInstanceTrust().getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                await dropItems();

                var eim = GetEventInstanceTrust();
                await eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                await eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                await spawnMonster(9300040, 1, nextPos);
            }


        }


        // Reactor: 2001005 
        public async Task fgodMob5()
        {
            if (GetEventInstanceTrust().getIntProperty("statusStg2") == -1)
            {
                var rnd = (int)Math.Floor(Random.Shared.NextDouble() * 14);

                GetEventInstanceTrust().setProperty("statusStg2", "" + rnd);
                GetEventInstanceTrust().setProperty("statusStg2_c", "0");
            }

            var limit = GetEventInstanceTrust().getIntProperty("statusStg2");
            var count = GetEventInstanceTrust().getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                await dropItems();

                var eim = GetEventInstanceTrust();
                await eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                await eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                await spawnMonster(9300040, 1, nextPos);
            }


        }


        // Reactor: 2001006 
        public async Task fgodMob6()
        {
            if (GetEventInstanceTrust().getIntProperty("statusStg2") == -1)
            {
                var rnd = (int)Math.Floor(Random.Shared.NextDouble() * 14);

                GetEventInstanceTrust().setProperty("statusStg2", "" + rnd);
                GetEventInstanceTrust().setProperty("statusStg2_c", "0");
            }

            var limit = GetEventInstanceTrust().getIntProperty("statusStg2");
            var count = GetEventInstanceTrust().getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                await dropItems();

                var eim = GetEventInstanceTrust();
                await eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                await eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                await spawnMonster(9300040, 1, nextPos);
            }


        }


        // Reactor: 2001007 
        public async Task fgodMob7()
        {
            if (GetEventInstanceTrust().getIntProperty("statusStg2") == -1)
            {
                var rnd = (int)Math.Floor(Random.Shared.NextDouble() * 14);

                GetEventInstanceTrust().setProperty("statusStg2", "" + rnd);
                GetEventInstanceTrust().setProperty("statusStg2_c", "0");
            }

            var limit = GetEventInstanceTrust().getIntProperty("statusStg2");
            var count = GetEventInstanceTrust().getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                await dropItems();

                var eim = GetEventInstanceTrust();
                await eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                await eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                await spawnMonster(9300040, 1, nextPos);
            }


        }


        // Reactor: 2001008 
        public async Task fgodMob8()
        {
            if (GetEventInstanceTrust().getIntProperty("statusStg2") == -1)
            {
                var rnd = (int)Math.Floor(Random.Shared.NextDouble() * 14);

                GetEventInstanceTrust().setProperty("statusStg2", "" + rnd);
                GetEventInstanceTrust().setProperty("statusStg2_c", "0");
            }

            var limit = GetEventInstanceTrust().getIntProperty("statusStg2");
            var count = GetEventInstanceTrust().getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                await dropItems();

                var eim = GetEventInstanceTrust();
                await eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                await eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                await spawnMonster(9300040, 1, nextPos);
            }


        }


        // Reactor: 2001009 
        public async Task fgodMob9()
        {
            if (GetEventInstanceTrust().getIntProperty("statusStg2") == -1)
            {
                var rnd = (int)Math.Floor(Random.Shared.NextDouble() * 14);

                GetEventInstanceTrust().setProperty("statusStg2", "" + rnd);
                GetEventInstanceTrust().setProperty("statusStg2_c", "0");
            }

            var limit = GetEventInstanceTrust().getIntProperty("statusStg2");
            var count = GetEventInstanceTrust().getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                await dropItems();

                var eim = GetEventInstanceTrust();
                await eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                await eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                await spawnMonster(9300040, 1, nextPos);
            }


        }


        // Reactor: 2001010 
        public async Task fgodMob10()
        {
            if (GetEventInstanceTrust().getIntProperty("statusStg2") == -1)
            {
                var rnd = (int)Math.Floor(Random.Shared.NextDouble() * 14);

                GetEventInstanceTrust().setProperty("statusStg2", "" + rnd);
                GetEventInstanceTrust().setProperty("statusStg2_c", "0");
            }

            var limit = GetEventInstanceTrust().getIntProperty("statusStg2");
            var count = GetEventInstanceTrust().getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                await dropItems();

                var eim = GetEventInstanceTrust();
                await eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                await eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                await spawnMonster(9300040, 1, nextPos);
            }


        }


        // Reactor: 2001011 
        public async Task fgodMob11()
        {
            if (GetEventInstanceTrust().getIntProperty("statusStg2") == -1)
            {
                var rnd = (int)Math.Floor(Random.Shared.NextDouble() * 14);

                GetEventInstanceTrust().setProperty("statusStg2", "" + rnd);
                GetEventInstanceTrust().setProperty("statusStg2_c", "0");
            }

            var limit = GetEventInstanceTrust().getIntProperty("statusStg2");
            var count = GetEventInstanceTrust().getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                await dropItems();

                var eim = GetEventInstanceTrust();
                await eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                await eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                await spawnMonster(9300040, 1, nextPos);
            }


        }


        // Reactor: 2001012 
        public async Task fgodMob12()
        {
            if (GetEventInstanceTrust().getIntProperty("statusStg2") == -1)
            {
                var rnd = (int)Math.Floor(Random.Shared.NextDouble() * 14);

                GetEventInstanceTrust().setProperty("statusStg2", "" + rnd);
                GetEventInstanceTrust().setProperty("statusStg2_c", "0");
            }

            var limit = GetEventInstanceTrust().getIntProperty("statusStg2");
            var count = GetEventInstanceTrust().getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                await dropItems();

                var eim = GetEventInstanceTrust();
                await eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                await eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                await spawnMonster(9300040, 1, nextPos);
            }


        }


        // Reactor: 2001013 
        public async Task fgodMob13()
        {
            if (GetEventInstanceTrust().getIntProperty("statusStg2") == -1)
            {
                var rnd = (int)Math.Floor(Random.Shared.NextDouble() * 14);

                GetEventInstanceTrust().setProperty("statusStg2", "" + rnd);
                GetEventInstanceTrust().setProperty("statusStg2_c", "0");
            }

            var limit = GetEventInstanceTrust().getIntProperty("statusStg2");
            var count = GetEventInstanceTrust().getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                await dropItems();

                var eim = GetEventInstanceTrust();
                await eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                await eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                await spawnMonster(9300040, 1, nextPos);
            }


        }


        // Reactor: 2001014 
        public async Task fgodMob14()
        {
            if (GetEventInstanceTrust().getIntProperty("statusStg2") == -1)
            {
                var rnd = (int)Math.Floor(Random.Shared.NextDouble() * 14);

                GetEventInstanceTrust().setProperty("statusStg2", "" + rnd);
                GetEventInstanceTrust().setProperty("statusStg2_c", "0");
            }

            var limit = GetEventInstanceTrust().getIntProperty("statusStg2");
            var count = GetEventInstanceTrust().getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                await dropItems();

                var eim = GetEventInstanceTrust();
                await eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                await eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                await spawnMonster(9300040, 1, nextPos);
            }


        }


        // Reactor: 2001015 
        public async Task fgodMob15()
        {
            if (GetEventInstanceTrust().getIntProperty("statusStg2") == -1)
            {
                var rnd = (int)Math.Floor(Random.Shared.NextDouble() * 14);

                GetEventInstanceTrust().setProperty("statusStg2", "" + rnd);
                GetEventInstanceTrust().setProperty("statusStg2_c", "0");
            }

            var limit = GetEventInstanceTrust().getIntProperty("statusStg2");
            var count = GetEventInstanceTrust().getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                await dropItems();

                var eim = GetEventInstanceTrust();
                await eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                await eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                await spawnMonster(9300040, 1, nextPos);
            }


        }


        // Reactor: 2001016 
        public async Task fgodBoss()
        {
            await getMap().killAllMonsters();
            getMap().allowSummonState(false);
            await spawnMonster(9300039, 260, 490);
            await mapMessage(5, "As the air on the tower outskirts starts to become more dense, Papa Pixie appears.");


        }


        // Reactor: 2002000 
        public async Task oBoxItem0()
        {
            await dropItems(true, 2, 60, 80);


        }


        // Reactor: 2002001 
        public async Task fgodItem0()
        {
            await dropItems();


        }


        // Reactor: 2002002 
        public async Task fgodItem1()
        {
            // TODO
            // 2002002
            await dropItems();


        }


        // Reactor: 2002003 
        public async Task fgodItem2()
        {
            await dropItems();

            var eim = GetEventInstanceTrust();
            eim.setProperty("statusStg7", "1");


        }


        // Reactor: 2002004 
        public async Task fgodItem3()
        {
            await dropItems();


        }


        // Reactor: 2002005 
        public async Task fgodItem4()
        {
            await dropItems();


        }


        // Reactor: 2002006 
        public async Task fgodItem5()
        {
            await dropItems();


        }


        // Reactor: 2002007 
        public async Task fgodItem6()
        {
            await dropItems();


        }


        // Reactor: 2002008 
        public async Task fgodItem7()
        {
            await dropItems();


        }


        // Reactor: 2002009 
        public async Task fgodItem8()
        {
            await dropItems();


        }


        // Reactor: 2002010 
        public async Task fgodItem9()
        {
            await dropItems();


        }


        // Reactor: 2002011 
        public async Task fgodItem10()
        {
            await dropItems();


        }


        // Reactor: 2002012 
        public async Task fgodItem11()
        {
            await dropItems();


        }


        // Reactor: 2002013 
        public async Task fgodItem12()
        {
            await dropItems();


        }


        // Reactor: 2002014 
        public async Task fgodItem13()
        {
            await dropItems(true, 1, 100, 400, 15);

            var eim = GetEventInstanceTrust();
            if (eim.getProperty("statusStgBonus") != "1")
            {
                await spawnNpc(2013002, new Point(46, 840));
                eim.setProperty("statusStgBonus", "1");
            }


        }


        // Reactor: 2002015 
        public async Task fgodItem14()
        {
            // TODO
            // 2002015



        }


        // Reactor: 2002016 
        public async Task fgodItem15()
        {
            // TODO
            // 2002016



        }


        // Reactor: 2002017 
        public async Task fgodItem16()
        {
            await dropItems(true, 1, 100, 400, 15);


        }


        // Reactor: 2002018 
        public async Task fgodItem17()
        {
            await dropItems(true, 1, 100, 400, 15);


        }


        // Reactor: 2006000 
        public async Task fgodNPC0()
        {

            await mapMessage(5, "As the light flickers, someone appears out of the light.");
            await spawnNpc(2013001);


        }


        // Reactor: 2006001 
        public async Task fgodNPC1()
        {
            await spawnNpc(2013002);
            var eim = GetEventInstanceTrust();
            await eim.clearPQ();

            eim.setProperty("statusStg8", "1");
            await eim.giveEventPlayersExp(3500);
            await eim.showClearEffect(true);

            await eim.startEventTimer(5 * 60000); //bonus time


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
            await Pink("An unknown force has moved you to the starting point.");
            await warp(280010000, 0);


        }


        // Reactor: 2111000 
        public async Task boxMob0()
        {
            await Pink("Oh noes! Monsters in the chest!");
            await spawnMonster(9300004, 3);


        }


        // Reactor: 2111001 
        public async Task boss()
        {
            var eim = getEventInstance();
            if (eim != null)
            {
                eim.setProperty("summoned", "true");
                eim.setProperty("canEnter", "false");
            }
            await changeMusic("Bgm06/FinalFight");
            await SpawnZakum();

            await createMapMonitor(280030000, "ps00");
            await mapMessage(5, "Zakum is summoned by the force of Eye of Fire.");


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
            // TODO
            // 2119000
            // If the chest is destroyed before Riche, killing him should yield no exp


        }


        // Reactor: 2119001 
        public async Task scaScript2()
        {
            // TODO
            // 2119001
            // If the chest is destroyed before Riche, killing him should yield no exp


        }


        // Reactor: 2119002 
        public async Task scaScript3()
        {
            // TODO
            // 2119002
            // If the chest is destroyed before Riche, killing him should yield no exp


        }


        // Reactor: 2119003 
        public async Task scaScript4()
        {
            // TODO
            // 2119003
            // If the chest is destroyed before Riche, killing him should yield no exp


        }


        [ScriptName("snowscaScript0", "snowscaScript1", "snowscaScript2")]
        public async Task SnowscaScript()
        {
            await weakenAreaBoss(6090001, "The light at the altar appeases the hatred of the Snow Witch. The force of the Witch has weakened.");


        }


        // Reactor: 2200000 
        public async Task go221024400()
        {
            await Pink("差一点就成功了！下次再挑战吧！");
            await warp(221024400, 1);


        }


        // Reactor: 2200001 
        public async Task ludiPotal0()
        {
            await Pink("You have found a secret factory!");
            await warp(Random.Shared.NextDouble() < .5 ? 922000020 : 922000021, 0);


        }


        // Reactor: 2200002 
        public async Task go922010201()
        {
            await mapMessage(5, "An unknown force has warped you into a trap.");
            warpMap(922010201);


        }


        // Reactor: 2201000 
        public async Task ludiMob0()
        {
            await spawnMonster(9300011, 10);


        }


        // Reactor: 2201001 
        public async Task ludiMob1()
        {
            for (var i = 0; i < 3; i++)
            {
                await spawnMonster(9300007);
            }


        }


        // Reactor: 2201002 
        public async Task ludiMob2()
        {
            await mapMessage(5, "Rombard has been summoned somewhere in the map.");
            await spawnMonster(9300010, 1, -211);


        }


        // Reactor: 2201003 
        public async Task ludiBoss0()
        {
            if (getPlayer().getMapId() == 922010900)
            {
                await mapMessage(5, "Alishar has been summoned.");
                await spawnMonster(9300012, 941, 184);
            }
            else if (getPlayer().getMapId() == 922010700)
            {
                await mapMessage(5, "Rombard has been summoned somewhere in the map.");
                await spawnMonster(9300010, 1, -211);
            }


        }


        // Reactor: 2201004 
        public async Task boss2()
        {
            await mapMessage(5, "The dimensional hole has been filled by the <Piece of Cracked Dimension>.");
            await changeMusic("Bgm09/TimeAttack");
            await spawnMonster(8500000, -410, -400);
            await createMapMonitor(220080001, "in00");


        }


        // Reactor: 2202000 
        public async Task ludiquest0()
        {
            await dropItems();


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


        // Reactor: 2292001 
        public async Task amberItem0()
        {
            // TODO
            // 2292001



        }


        // Reactor: 2292002 
        public async Task amberItem1()
        {
            // TODO
            // 2292002



        }


        // Reactor: 2292003 
        public async Task amberItem2()
        {
            // TODO
            // 2292003



        }


        // Reactor: 2292004 
        public async Task amberItem3()
        {
            // TODO
            // 2292004



        }


        // Reactor: 2292005 
        public async Task amberItem4()
        {
            // TODO
            // 2292005



        }


        // Reactor: 2292006 
        public async Task amberItem5()
        {
            // TODO
            // 2292006



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


        // Reactor: 2401000 
        public async Task hontaleBoss()
        {
            await changeMusic("Bgm14/HonTale");
            if (getReactor().getMap().getMonsterById(8810026) == null)
            {
                await getReactor().getMap().spawnHorntailOnGroundBelow(new Point(71, 260));

                var eim = GetEventInstanceTrust();
                await eim.restartEventTimer(60 * 60000);
            }
            await mapMessage(6, "From the depths of his cave, here comes Horntail!");


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


        // Reactor: 2402002 
        public async Task hontaleItem0()
        {
            // TODO
            // 2402002



        }


        // Reactor: 2402003 
        public async Task hontaleItem1()
        {
            // TODO
            // 2402003



        }


        // Reactor: 2402004 
        public async Task hontaleItem2()
        {
            // TODO
            // 2402004



        }


        // Reactor: 2402005 
        public async Task hontaleItem3()
        {
            // TODO
            // 2402005



        }


        // Reactor: 2402006 
        public async Task hontaleItem4()
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


        // Reactor: 2511000 
        public async Task davyMob0()
        {
            var eim = GetEventInstanceTrust();
            var now = eim.getIntProperty("openedBoxes");
            var nextNum = now + 1;
            eim.setIntProperty("openedBoxes", nextNum);

            await spawnMonster(9300109, 3);
            await spawnMonster(9300110, 5);


        }


        // Reactor: 2511001 
        public async Task davyMob1()
        {
            for (var i = 0; i < 6; i++)
            {
                await spawnMonster(9300124);
                await spawnMonster(9300125);
            }


        }


        // Reactor: 2512000 
        public async Task davyItem0()
        {
            var eim = GetEventInstanceTrust();
            var now = eim.getIntProperty("openedBoxes");
            var nextNum = now + 1;
            eim.setIntProperty("openedBoxes", nextNum);

            await dropItems(true, 1, 30, 60, 15);

            var map = getMap();
            if (map.countMonsters() == 0 && (eim.getIntProperty("grindMode") == 0 || eim.activatedAllReactorsOnMap(map, 2511000, 2517999)))
            {
                await eim.showClearEffect(map.getId());
            }


        }


        // Reactor: 2512001 
        public async Task davyItem1()
        {
            var eim = GetEventInstanceTrust();
            var now = eim.getIntProperty("openedChests");
            var nextNum = now + 1;
            eim.setIntProperty("openedChests", nextNum);
            await dropItems(true, 1, 50, 100, 15);


        }


        // Reactor: 2516000 
        public async Task davyNPC0()
        {
            await Pink("老海盗已经被打败, 无恙被释放了！");
            await spawnNpc(2094001);


        }


        // Reactor: 2519000 
        public Task davyScript0() => DavyScript();

        // Reactor: 2519001 
        public Task davyScript1() => DavyScript();

        // Reactor: 2519002 
        public Task davyScript2() => DavyScript();


        // Reactor: 2519003 
        public Task davyScript3() => DavyScript();

        // Map 925100400
        async Task DavyScript()
        {
            var denyWidth = 320;
            var denyHeight = 150;
            var denyPos = getReactor().getPosition();
            var denyArea = new Rectangle(denyPos.X - denyWidth / 2, denyPos.Y - denyHeight / 2, denyWidth, denyHeight);

            var map = getReactor().getMap();
            map.setAllowSpawnPointInBox(false, denyArea);

            if (map.GetRequiredMapObjects<Reactor>(Shared.MapObjects.MapObjectType.REACTOR, r => r.getName().StartsWith("sMob") && r.getState() < 1).Count == 0
                && map.countMonsters() == 0)
            {
                await GetEventInstanceTrust().showClearEffect(map.getId());
            }


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


        // Reactor: 2612001 
        public async Task rnjItem0()
        {
            await dropItems();
        }


        // Reactor: 2612002 
        public async Task rnjItem1()
        {
            await dropItems();


        }


        // Reactor: 2612003 
        public async Task rnjItem2()
        {
            await dropItems();
        }


        // Reactor: 2612004 
        public async Task magatiaItem1()
        {
            // TODO
            // 2612004



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
            // TODO
            // 2619000
            // There's a timeout of 3 seconds to revert back from state 1 to 0.
            // Reactor is destroyed (state 2) and triggers this if dropping two Magic Devices at once, which shouldn't really happen.


        }


        // Reactor: 2619001 
        public async Task rnjScript0()
        {
            // TODO
            // 2619001



        }


        // Reactor: 2619002 
        public async Task rnjScript1()
        {
            // TODO
            // 2619002



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
            // TODO
            // 2708000



        }


        // Reactor: 2709000 
        public async Task PinkBeenScript0()
        {
            // TODO
            // 2709000



        }


        // Reactor: 3001000 
        public async Task pFBoss()
        {
            await playerMessage(5, "Poison Golem has been spawned.");
            await spawnMonster(9300180, 1);


        }


        // Reactor: 3002000 
        public async Task pFItem0()
        {
            await dropItems();


        }


        // Reactor: 3002001 
        public async Task pFItem1()
        {
            await GetEventInstanceTrust().showClearEffect(getMap().getId());
            await dropItems();


        }


        // Reactor: 3008000 
        public async Task pFBack0()
        {
            var eim = getEventInstance();
            if (eim != null)
                await eim.giveEventPlayersExp(52000, getMapId());


        }


        // Reactor: 5411000 
        public async Task sgboss0()
        {
            await changeMusic("Bgm09/TimeAttack");
            await spawnMonster(9420513, -146, 225);
            GetEventInstanceTrust().setIntProperty("boss", 1);
            await mapMessage(5, "As you wish, here comes Capt Latanica.");


        }


        // Reactor: 5511000 
        public async Task myboss0()
        {
            var targaMobId = 9420542;
            if (getReactor().getMap().getMonsterById(targaMobId) == null)
            {
                await summonBossDelayed(targaMobId, 3200, -527, 637, "Bgm09/TimeAttack", "Beware! The furious Targa has shown himself!");
            }


        }


        // Reactor: 5511001 
        public async Task myboss1()
        {
            var scarlionMobId = 9420547;
            if (getReactor().getMap().getMonsterById(scarlionMobId) == null)
            {
                await summonBossDelayed(scarlionMobId, 3200, -238, 636, "Bgm09/TimeAttack", "Beware! The furious Scarlion has shown himself!");
            }


        }


        // Reactor: 6102001 
        public async Task glpqitem2()
        {
            await dropItems();


        }


        // Reactor: 6102002 
        public async Task glpqreward1()
        {
            await dropItems(true, 1, 90, 360, 15);


        }


        // Reactor: 6102003 
        public async Task glpqreward2()
        {
            await dropItems(true, 1, 90, 360, 15);


        }


        // Reactor: 6102004 
        public async Task glpqreward3()
        {
            await dropItems(true, 1, 90, 360, 15);


        }


        // Reactor: 6102005 
        public async Task glpqreward4()
        {
            await dropItems(true, 1, 90, 360, 15);


        }


        // Reactor: 6109000 
        public async Task glpqskill0()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                var mapId = getMap().getId();

                if (mapId == 610030200)
                {
                    await eim.dropMessage(6, "The Warrior Sigil has been activated!");
                    eim.setIntProperty("glpq2", eim.getIntProperty("glpq2") + 1);
                    if (eim.getIntProperty("glpq2") == 5)
                    { //all 5 done
                        await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                        await eim.showClearEffect(mapId, "2pt", 2);
                        await eim.GiveStageClearRewardAll(2);
                    }
                }
                else if (mapId == 610030300)
                {
                    await eim.LightBlue("The Warrior Sigil has been activated! You hear gears turning! The Menhir Defense System is active! Run!");
                    eim.setIntProperty("glpq3", eim.getIntProperty("glpq3") + 1);
                    getMap().moveEnvironment("menhir0", 1);
                    if (eim.getIntProperty("glpq3") == 5 && eim.getIntProperty("glpq3_p") == 5)
                    {
                        await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                        await eim.showClearEffect(mapId, "3pt", 2);
                        await eim.GiveStageClearRewardAll(3);
                    }
                }
            }


        }


        // Reactor: 6109001 
        public async Task glpqskill1()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                var mapId = getMap().getId();

                if (mapId == 610030200)
                {
                    await eim.LightBlue("The Archer Sigil has been activated!");
                    eim.setIntProperty("glpq2", eim.getIntProperty("glpq2") + 1);
                    if (eim.getIntProperty("glpq2") == 5)
                    { //all 5 done
                        await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                        await eim.showClearEffect(mapId, "2pt", 2);
                        await eim.GiveStageClearRewardAll(2);
                    }
                }
                else if (mapId == 610030300)
                {
                    await eim.LightBlue("The Archer Sigil has been activated! You hear gears turning! The Menhir Defense System is active! Run!");
                    eim.setIntProperty("glpq3", eim.getIntProperty("glpq3") + 1);
                    getMap().moveEnvironment("menhir1", 1);
                    getMap().moveEnvironment("menhir2", 1);
                    if (eim.getIntProperty("glpq3") == 5 && eim.getIntProperty("glpq3_p") == 5)
                    {
                        await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                        await eim.showClearEffect(mapId, "3pt", 2);
                        await eim.GiveStageClearRewardAll(3);
                    }
                }
            }


        }


        // Reactor: 6109002 
        public async Task glpqskill2()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                var mapId = getMap().getId();

                if (mapId == 610030200)
                {
                    await eim.LightBlue("The Mage Sigil has been activated!");
                    eim.setIntProperty("glpq2", eim.getIntProperty("glpq2") + 1);
                    if (eim.getIntProperty("glpq2") == 5)
                    { //all 5 done
                        await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                        await eim.showClearEffect(mapId, "2pt", 2);
                        await eim.GiveStageClearRewardAll(2);
                    }
                }
                else if (mapId == 610030300)
                {
                    await eim.LightBlue("The Mage Sigil has been activated! You hear gears turning! The Menhir Defense System is active! Run!");
                    eim.setIntProperty("glpq3", eim.getIntProperty("glpq3") + 1);
                    getMap().moveEnvironment("menhir3", 1);
                    if (eim.getIntProperty("glpq3") == 5 && eim.getIntProperty("glpq3_p") == 5)
                    {
                        await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                        await eim.showClearEffect(mapId, "3pt", 2);
                        await eim.GiveStageClearRewardAll(3);
                    }
                }
            }


        }


        // Reactor: 6109003 
        public async Task glpqskill3()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                var mapId = getMap().getId();

                if (mapId == 610030200)
                {
                    await eim.LightBlue("The Thief Sigil has been activated!");
                    eim.setIntProperty("glpq2", eim.getIntProperty("glpq2") + 1);
                    if (eim.getIntProperty("glpq2") == 5)
                    { //all 5 done
                        await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                        await eim.showClearEffect(mapId, "2pt", 2);
                        await eim.GiveStageClearRewardAll(2);
                    }
                }
                else if (mapId == 610030300)
                {
                    await eim.LightBlue("The Thief Sigil has been activated! You hear gears turning! The Menhir Defense System is active! Run!");
                    eim.setIntProperty("glpq3", eim.getIntProperty("glpq3") + 1);
                    getMap().moveEnvironment("menhir4", 1);
                    if (eim.getIntProperty("glpq3") == 5 && eim.getIntProperty("glpq3_p") == 5)
                    {
                        await mapMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                        await eim.showClearEffect(mapId, "3pt", 2);
                        await eim.GiveStageClearRewardAll(3);
                    }
                }
            }


        }


        // Reactor: 6109004 
        public async Task glpqskill4()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                var mapId = getMap().getId();

                if (mapId == 610030200)
                {
                    await eim.LightBlue("The Pirate Sigil has been activated!");
                    eim.setIntProperty("glpq2", eim.getIntProperty("glpq2") + 1);
                    if (eim.getIntProperty("glpq2") == 5)
                    { //all 5 done
                        await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                        await eim.showClearEffect(mapId, "2pt", 2);
                        await eim.GiveStageClearRewardAll(2);
                    }
                }
                else if (mapId == 610030300)
                {
                    await eim.LightBlue("The Pirate Sigil has been activated! You hear gears turning! The Menhir Defense System is active! Run!");
                    eim.setIntProperty("glpq3", eim.getIntProperty("glpq3") + 1);
                    getMap().moveEnvironment("menhir5", 1);
                    if (eim.getIntProperty("glpq3") == 5 && eim.getIntProperty("glpq3_p") == 5)
                    {
                        await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                        await eim.showClearEffect(mapId, "3pt", 2);
                        await eim.GiveStageClearRewardAll(3);
                    }
                }
            }


        }


        // Reactor: 6109005 
        public async Task glpqweapon0()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                await eim.LightBlue("A weapon has been restored to the Relic of Mastery!");
                eim.setIntProperty("glpq5", eim.getIntProperty("glpq5") + 1);
                if (eim.getIntProperty("glpq5") == 5)
                { //all 5 done
                    await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                    await eim.showClearEffect(610030500, "5pt", 2);
                    await eim.GiveStageClearRewardAll(5);
                }
            }


        }


        // Reactor: 6109006 
        public async Task glpqweapon1()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                await eim.LightBlue("A weapon has been restored to the Relic of Mastery!");
                eim.setIntProperty("glpq5", eim.getIntProperty("glpq5") + 1);
                if (eim.getIntProperty("glpq5") == 5)
                { //all 5 done
                    await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                    await eim.showClearEffect(610030500, "5pt", 2);
                    await eim.GiveStageClearRewardAll(5);
                }
            }


        }


        // Reactor: 6109007 
        public async Task glpqweapon2()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                await eim.LightBlue("A weapon has been restored to the Relic of Mastery!");
                eim.setIntProperty("glpq5", eim.getIntProperty("glpq5") + 1);
                if (eim.getIntProperty("glpq5") == 5)
                { //all 5 done
                    await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                    await eim.showClearEffect(610030500, "5pt", 2);
                    await eim.GiveStageClearRewardAll(5);
                }
            }


        }


        // Reactor: 6109008 
        public async Task glpqweapon3()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                await eim.LightBlue("A weapon has been restored to the Relic of Mastery!");
                eim.setIntProperty("glpq5", eim.getIntProperty("glpq5") + 1);
                if (eim.getIntProperty("glpq5") == 5)
                { //all 5 done
                    await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                    await eim.showClearEffect(610030500, "5pt", 2);
                    await eim.GiveStageClearRewardAll(5);
                }
            }


        }


        // Reactor: 6109009 
        public async Task glpqweapon4()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                await eim.LightBlue("A weapon has been restored to the Relic of Mastery!");
                eim.setIntProperty("glpq5", eim.getIntProperty("glpq5") + 1);
                if (eim.getIntProperty("glpq5") == 5)
                { //all 5 done
                    await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                    await eim.showClearEffect(610030500, "5pt", 2);
                    await eim.GiveStageClearRewardAll(5);
                }
            }


        }


        // Reactor: 6109010 
        public async Task glpqmob0()
        {
            // TODO
            // 6109010



        }


        // Reactor: 6109011 
        public async Task glpqmob1()
        {
            // TODO
            // 6109011



        }


        // Reactor: 6109013 
        public async Task glpqstrge()
        {
            // TODO
            // 6109013



        }





        // Reactor: 6109016 
        public async Task glpqskill5()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                await eim.LightBlue("The Warrior Sigil has been activated!");
                eim.setIntProperty("glpq4", eim.getIntProperty("glpq4") + 1);
                if (eim.getIntProperty("glpq4") == 5)
                { //all 5 done
                    await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                    await eim.showClearEffect(610030400, "4pt", 2);
                    await eim.GiveStageClearRewardAll(4);
                }
            }


        }


        // Reactor: 6109017 
        public async Task glpqskill6()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                await eim.LightBlue("The Archer Sigil has been activated!");
                eim.setIntProperty("glpq4", eim.getIntProperty("glpq4") + 1);
                if (eim.getIntProperty("glpq4") == 5)
                { //all 5 done
                    await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                    await eim.showClearEffect(610030400, "4pt", 2);
                    await eim.GiveStageClearRewardAll(4);
                }
            }


        }


        // Reactor: 6109018 
        public async Task glpqskill7()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                await eim.LightBlue("The Mage Sigil has been activated!");
                eim.setIntProperty("glpq4", eim.getIntProperty("glpq4") + 1);
                if (eim.getIntProperty("glpq4") == 5)
                { //all 5 done
                    await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                    await eim.showClearEffect(610030400, "4pt", 2);
                    await eim.GiveStageClearRewardAll(4);
                }
            }


        }


        // Reactor: 6109019 
        public async Task glpqskill8()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                await eim.LightBlue("The Thief Sigil has been activated!");
                eim.setIntProperty("glpq4", eim.getIntProperty("glpq4") + 1);
                if (eim.getIntProperty("glpq4") == 5)
                { //all 5 done
                    await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                    await eim.showClearEffect(610030400, "4pt", 2);
                    await eim.GiveStageClearRewardAll(4);
                }
            }


        }


        // Reactor: 6109020 
        public async Task glpqskill9()
        {
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                await eim.LightBlue("The Pirate Sigil has been activated!");
                eim.setIntProperty("glpq4", eim.getIntProperty("glpq4") + 1);
                if (eim.getIntProperty("glpq4") == 5)
                { //all 5 done
                    await eim.LightBlue("The Antellion grants you access to the next portal! Proceed!");

                    await eim.showClearEffect(610030400, "4pt", 2);
                    await eim.GiveStageClearRewardAll(4);
                }
            }


        }


        // Reactor: 6109021 
        public async Task glpqflame1()
        {
            // TODO
            // 6109021



        }


        // Reactor: 6109022 
        public async Task glpqflame2()
        {
            // TODO
            // 6109022



        }


        // Reactor: 6109023 
        public async Task glpqflame3()
        {
            // TODO
            // 6109023



        }


        // Reactor: 6109024 
        public async Task glpqflame4()
        {
            // TODO
            // 6109024



        }


        // Reactor: 6109025 
        public async Task glpqflame5()
        {
            // TODO
            // 6109025



        }


        // Reactor: 6109026 
        public async Task glpqflame6()
        {
            // TODO
            // 6109026



        }


        // Reactor: 6109027 
        public async Task glpqflame7()
        {
            // TODO
            // 6109027



        }


        [ScriptName("amoriaboxMob0", "amoriaboxMob1", "amoriaboxMob2")]
        public async Task AmoriaboxMob()
        {
            var startId = 9400523;
            var mapObj = getMap();
            for (var i = 0; i < 7; i++)
            {
                var mobObj = LifeFactory.Instance.getMonster(startId + Random.Shared.Next(3));
                await mapObj.spawnMonsterOnGroundBelow(mobObj, getReactor().getPosition());
            }


        }


        // Reactor: 6702000 
        public async Task amoriaItem0()
        {
            await dropItems();


        }


        // Reactor: 6702001 
        public async Task amoriaItem1()
        {
            // TODO
            // 6702001



        }


        // Reactor: 6702002 
        public async Task amoriaItem2()
        {
            // TODO
            // 6702002



        }

        [ScriptName("amoriaItem3", "amoriaItem4", "amoriaItem5", "amoriaItem6", "amoriaItem7", "amoriaItem8",
            "amoriaItem9", "amoriaItem10", "amoriaItem11", "amoriaItem12")]
        public async Task AmoriaItem()
        {
            var count = Math.Max(1, Random.Shared.Next(4));
            //We'll make it drop a lot of crap :D
            for (var i = 0; i < count; i++)
            {
                await dropItems(true, 1, 30, 60, 15);
            }


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
            await playerMessage(5, "Enjoy Halloween!");
            await spawnMonster(9400202, 10);


        }


        // Reactor: 8001000 
        public async Task shouwaBoss()
        {
            await spawnMonster(9400112, 1, 420, 160);


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


        // Reactor: 8892000 
        [ScriptName("08_Snowman0")]
        public async Task s_08_Snowman0()
        {
            // TODO
            // 8892000



        }


        // Reactor: 8892001 
        [ScriptName("08_Cross0")]
        public async Task s_08_Cross0()
        {
            // TODO
            // 8892001



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


        // Reactor: 9101000 
        public async Task moonMob0()
        {
            var eim = getEventInstance();
            if (eim != null)
            {
                eim.ClearedMaps[getMapId()] = Core.scripting.Events.Abstraction.StageStatus.Started;
                await spawnMonster(MobId.MOON_BUNNY, 1, -183, -433); // (0, 0) is temp position
                await getMap().startMapEffect("月妙开始制作美味的年糕，其香味会吸引各种怪物，请务必守护好月妙！！！", ItemId.Weather_HenesysPQ, 7000);
                await getMap().broadcastMessage(MessagePacket.SystemMessage("保护月妙！")); // Protect the Moon Bunny!
                getMap().allowSummonState(true);
            }



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

        [ScriptName("moonItem0", "moonItem1", "moonItem2", "moonItem3", "moonItem4", "moonItem5")]
        public async Task MoonItem0()
        {
            await dropItems();
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


        // Reactor: 9802000 
        public async Task goldRichbox0()
        {
            // TODO
            // 9802000



        }


        // Reactor: 9802002, 9802003, 9802004, 9802008 
        public async Task goldRichItem0()
        {
            // TODO
            // 9802002

            // 9802003

            // 9802004

            // 9802008



        }


        // Reactor: 9802005 
        public async Task goldRichItem1()
        {
            // TODO
            // 9802005



        }


        // Reactor: 9802006 
        public async Task goldRichItem2()
        {
            // TODO
            // 9802006



        }


        // Reactor: 9902000 
        public async Task PB_boxCount()
        {
            // TODO
            // 9902000



        }


        // Reactor: 9980000, 9980001 
        public async Task mcGuardian0()
        {
            await dispelAllMonsters(int.Parse(getReactor().getName().Substring(1, 2)), int.Parse(getReactor().getName().Substring(0, 1)));
        }
    }
}