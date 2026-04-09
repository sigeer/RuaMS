using Application.Core.Client;
using scripting.reactor;
using server.life;
using server.maps;
using System.Drawing;
using tools;

namespace Application.Plugin.Script
{
    // Extra: 1002008, 1002009, 1021002, 1209001, 200000, 200001, 200002, 200003, 200004, 200005, 200006, 200007, 200008, 200009,
    // 2008006, 2008007, 2052001, 2408002, 2408003, 2408004, 2508000, 2618000, 2618001, 2618002, 3009000, 3102000, 8098000,
    // 9018000, 9018001, 9018002, 9018003, 9018004, 9018005
    // 9108000, 9108001, 9108002, 9108003, 9108004, 9108005,
    // 9208000, 9208001, 9208002, 9208004, 9208007, 9208009, 9400300, 9400301
    internal class ReactorActScript : ReactorActionManager
    {
        public ReactorActScript(IChannelClient c, Reactor r) : base(c, r, null)
        {
        }

        // Reactor: 2000, 2001 
        public Task mBoxItem0()
        {
            dropItems(true, 2, 8, 15, 1);
            return Task.CompletedTask;
        }


        // Reactor: 1002000 
        public Task babyBirdItem0()
        {
            // TODO
            // 1002000

            return Task.CompletedTask;
        }


        // Reactor: 1002001 
        public Task BF_item0()
        {
            // TODO
            // 1002001


            return Task.CompletedTask;
        }


        // Reactor: 1002002 
        public Task BF_item1()
        {
            // TODO
            // 1002002


            return Task.CompletedTask;
        }


        // Reactor: 1002003 
        public Task BF_item2()
        {
            // TODO
            // 1002003


            return Task.CompletedTask;
        }


        // Reactor: 1002006 
        public Task BF_item5()
        {
            // TODO
            // 1002006


            return Task.CompletedTask;
        }


        // Reactor: 1006000 
        public Task lostDoyoNPC0()
        {
            // TODO
            // 1006000


            return Task.CompletedTask;
        }


        // Reactor: 1009000 
        public Task BF_scr0()
        {
            // TODO
            // 1009000


            return Task.CompletedTask;
        }


        // Reactor: 1012000 
        public Task vFlowerItem0()
        {
            // TODO
            // 1012000
            dropItems(true, 2, 20, 40);

            return Task.CompletedTask;
        }


        // Reactor: 1020000 
        public Task s4hitmanMap0()
        {
            // TODO
            // 1020000
            warp(910200000, "pt00");

            return Task.CompletedTask;
        }


        // Reactor: 1020001 
        public Task s4hitmanMap1()
        {
            // TODO
            // 1020001
            warp(910200000, "pt01");

            return Task.CompletedTask;
        }


        // Reactor: 1020002 
        public Task s4hitmanMap2()
        {
            // TODO
            // 1020002
            warp(910200000, "pt02");

            return Task.CompletedTask;
        }


        // Reactor: 1020003 
        public Task s4hitmanMap3()
        {
            // TODO
            // 1020003


            return Task.CompletedTask;
        }


        // Reactor: 1020004 
        public Task s4hitmanMap4()
        {
            // TODO
            // 1020004


            return Task.CompletedTask;
        }


        // Reactor: 1020005 
        public Task s4hitmanMap5()
        {
            // TODO
            // 1020005


            return Task.CompletedTask;
        }


        // Reactor: 1020006 
        public Task s4hitmanMap6()
        {
            // TODO
            // 1020006


            return Task.CompletedTask;
        }


        // Reactor: 1020007 
        public Task s4hitmanMap7()
        {
            // TODO
            // 1020007


            return Task.CompletedTask;
        }


        // Reactor: 1020008 
        public Task s4hitmanMap8()
        {
            // TODO
            // 1020008


            return Task.CompletedTask;
        }


        // Reactor: 1021000 
        public Task s4hitmanMob0()
        {
            // TODO
            // 1021000
            spawnMonster(9300091);

            return Task.CompletedTask;
        }


        // Reactor: 1021001 
        public Task s4hitmanMob1()
        {
            // TODO
            // 1021001
            spawnMonster(9300091);

            return Task.CompletedTask;
        }


        // Reactor: 1022000 
        public Task s4hitmanItem0()
        {
            // TODO
            // 1022000
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 1022001 
        public Task periItem0()
        {
            // TODO
            // 1022001
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 1022002, 1032000, 1202000, 1202004 
        public Task EpisodeQuest0()
        {
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 1029000 
        public Task scaScript0()
        {
            // TODO
            // 1029000
            if (isAllReactorState(1029000, 0x04))
            { // 0x04 appears to be the destroyed state
                killMonster(3230300);
                killMonster(3230301);
                playerMessage(6, "Once the rock crumbled, Jr. Boogie was in great pain and disappeared.");
            }

            return Task.CompletedTask;
        }


        // Reactor: 1050000 
        public Task s4berserkMap0()
        {
            // TODO
            // 1050000
            if (Random.Shared.NextDouble() > 0.7)
            {
                dropItems();
            }
            else
            {
                warp(105090200, 0);
            }

            return Task.CompletedTask;
        }


        // Reactor: 1052000 
        public Task s4berserkItem0()
        {
            // TODO
            // 1052000
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 1052001, 1052002, 1052003 
        public Task balogItem0()
        {
            // TODO
            // 1052001
            sprayItems(true, 1, 500, 1000, 15);
            // 1052002
            sprayItems(true, 1, 500, 1000, 15);
            // 1052003


            return Task.CompletedTask;
        }


        // Reactor: 1058001, 1058003, 1058004, 1058011, 1058013, 1058014 
        public Task minibalogSummon()
        {
            // TODO
            // 1058001

            // 1058003

            // 1058004

            // 1058011

            // 1058013

            // 1058014


            return Task.CompletedTask;
        }


        // Reactor: 1058005 
        public Task balogReactor()
        {
            // TODO
            // 1058005


            return Task.CompletedTask;
        }


        // Reactor: 1058015 
        public Task Easy_balogReactor()
        {
            // TODO
            // 1058015


            return Task.CompletedTask;
        }


        // Reactor: 1072000 
        public Task vFlowerItem1()
        {
            // TODO
            // 1072000
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 1102000 
        public Task coconut0()
        {
            // TODO
            // 1102000
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 1102001 
        public Task coconut1()
        {
            // TODO
            // 1102001
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 1102002 
        public Task coconut2()
        {
            // TODO
            // 1102002
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 1102003 
        public Task florinaBox0()
        {
            // TODO
            // 1102003


            return Task.CompletedTask;
        }


        // Reactor: 1200000 
        public Task ntQuest02()
        {
            // TODO
            // 1200000
            // string visibility thanks to ProXAIMeRx & Glvelturall
            message("Failed to find Bart. Returning to the original location.");
            warp(120000102);

            return Task.CompletedTask;
        }


        // Reactor: 1202002 
        public Task ntItem03()
        {
            // TODO
            // 1202002
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 1202003 
        public Task EpisodeQuest1()
        {
            // TODO
            // 1202003
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 1209000 
        public Task ntQuest01()
        {
            // TODO
            // 1209000
            // string visibility thanks to ProXAIMeRx & Glvelturall
            if (isQuestStarted(6400))
            {
                setQuestProgress(6400, 1, 2);
                setQuestProgress(6400, 6401, "q3");
            }

            message("Real Bart has been found. Return to Jonathan through the portal.");

            return Task.CompletedTask;
        }


        // Reactor: 1302000 
        public Task erebItem0()
        {
            // TODO
            // 1302000
            dropItems(true, 2, 8, 12, 2);

            return Task.CompletedTask;
        }


        // Reactor: 1402000 
        public Task rienItem0()
        {
            // TODO
            // 1402000
            dropItems(true, 2, 8, 15);

            return Task.CompletedTask;
        }


        // Reactor: 2001000 
        public Task fgodMob0()
        {
            // TODO
            // 2001000
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
                    spawnMonster(monsterId);
                    GetEventInstanceTrust().setProperty("statusStg7_c", nextCount);
                }
                else
                {
                    spawnMonster(9300049);
                    getMap().allowSummonState(false);
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001001 
        public Task fgodMob1()
        {
            // TODO
            // 2001001
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
                    spawnMonster(monsterId);
                    GetEventInstanceTrust().setProperty("statusStg7_c", nextCount);
                }
                else
                {
                    spawnMonster(9300049);
                    getMap().allowSummonState(false);
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001002 
        public Task fgodMob2()
        {
            // TODO
            // 2001002
            if (GetEventInstanceTrust().getIntProperty("statusStg2") == -1)
            {
                var rnd = Math.Max(Math.Floor(Random.Shared.NextDouble() * 14), 4);

                GetEventInstanceTrust().setProperty("statusStg2", "" + rnd);
                GetEventInstanceTrust().setProperty("statusStg2_c", "0");
            }

            var limit = GetEventInstanceTrust().getIntProperty("statusStg2");
            var count = GetEventInstanceTrust().getIntProperty("statusStg2_c");
            if (count >= limit)
            {
                dropItems();

                var eim = GetEventInstanceTrust();
                eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                spawnMonster(9300040, 1, nextPos);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001003 
        public Task fgodMob3()
        {
            // TODO
            // 2001003
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
                dropItems();

                var eim = GetEventInstanceTrust();
                eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                spawnMonster(9300040, 1, nextPos);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001004 
        public Task fgodMob4()
        {
            // TODO
            // 2001004
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
                dropItems();

                var eim = GetEventInstanceTrust();
                eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                spawnMonster(9300040, 1, nextPos);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001005 
        public Task fgodMob5()
        {
            // TODO
            // 2001005
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
                dropItems();

                var eim = GetEventInstanceTrust();
                eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                spawnMonster(9300040, 1, nextPos);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001006 
        public Task fgodMob6()
        {
            // TODO
            // 2001006
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
                dropItems();

                var eim = GetEventInstanceTrust();
                eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                spawnMonster(9300040, 1, nextPos);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001007 
        public Task fgodMob7()
        {
            // TODO
            // 2001007
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
                dropItems();

                var eim = GetEventInstanceTrust();
                eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                spawnMonster(9300040, 1, nextPos);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001008 
        public Task fgodMob8()
        {
            // TODO
            // 2001008
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
                dropItems();

                var eim = GetEventInstanceTrust();
                eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                spawnMonster(9300040, 1, nextPos);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001009 
        public Task fgodMob9()
        {
            // TODO
            // 2001009
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
                dropItems();

                var eim = GetEventInstanceTrust();
                eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                spawnMonster(9300040, 1, nextPos);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001010 
        public Task fgodMob10()
        {
            // TODO
            // 2001010
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
                dropItems();

                var eim = GetEventInstanceTrust();
                eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                spawnMonster(9300040, 1, nextPos);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001011 
        public Task fgodMob11()
        {
            // TODO
            // 2001011
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
                dropItems();

                var eim = GetEventInstanceTrust();
                eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                spawnMonster(9300040, 1, nextPos);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001012 
        public Task fgodMob12()
        {
            // TODO
            // 2001012
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
                dropItems();

                var eim = GetEventInstanceTrust();
                eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                spawnMonster(9300040, 1, nextPos);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001013 
        public Task fgodMob13()
        {
            // TODO
            // 2001013
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
                dropItems();

                var eim = GetEventInstanceTrust();
                eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                spawnMonster(9300040, 1, nextPos);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001014 
        public Task fgodMob14()
        {
            // TODO
            // 2001014
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
                dropItems();

                var eim = GetEventInstanceTrust();
                eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                spawnMonster(9300040, 1, nextPos);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001015 
        public Task fgodMob15()
        {
            // TODO
            // 2001015
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
                dropItems();

                var eim = GetEventInstanceTrust();
                eim.giveEventPlayersExp(3500);

                eim.setProperty("statusStg2", "1");
                eim.showClearEffect(true);
            }
            else
            {
                count++;
                GetEventInstanceTrust().setProperty("statusStg2_c", count);

                var nextHashed = (11 * (count)) % 14;

                var nextPos = getMap().getReactorById(2001002 + nextHashed).getPosition();
                spawnMonster(9300040, 1, nextPos);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2001016 
        public Task fgodBoss()
        {
            // TODO
            // 2001016
            getMap().killAllMonsters();
            getMap().allowSummonState(false);
            spawnMonster(9300039, 260, 490);
            mapMessage(5, "As the air on the tower outskirts starts to become more dense, Papa Pixie appears.");

            return Task.CompletedTask;
        }


        // Reactor: 2002000 
        public Task oBoxItem0()
        {
            // TODO
            // 2002000
            dropItems(true, 2, 60, 80);

            return Task.CompletedTask;
        }


        // Reactor: 2002001 
        public Task fgodItem0()
        {
            // TODO
            // 2002001
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2002002 
        public Task fgodItem1()
        {
            // TODO
            // 2002002
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2002003 
        public Task fgodItem2()
        {
            // TODO
            // 2002003
            dropItems();

            var eim = GetEventInstanceTrust();
            eim.setProperty("statusStg7", "1");

            return Task.CompletedTask;
        }


        // Reactor: 2002004 
        public Task fgodItem3()
        {
            // TODO
            // 2002004
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2002005 
        public Task fgodItem4()
        {
            // TODO
            // 2002005
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2002006 
        public Task fgodItem5()
        {
            // TODO
            // 2002006
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2002007 
        public Task fgodItem6()
        {
            // TODO
            // 2002007
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2002008 
        public Task fgodItem7()
        {
            // TODO
            // 2002008
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2002009 
        public Task fgodItem8()
        {
            // TODO
            // 2002009
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2002010 
        public Task fgodItem9()
        {
            // TODO
            // 2002010
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2002011 
        public Task fgodItem10()
        {
            // TODO
            // 2002011
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2002012 
        public Task fgodItem11()
        {
            // TODO
            // 2002012
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2002013 
        public Task fgodItem12()
        {
            // TODO
            // 2002013
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2002014 
        public Task fgodItem13()
        {
            // TODO
            // 2002014
            dropItems(true, 1, 100, 400, 15);

            var eim = GetEventInstanceTrust();
            if (eim.getProperty("statusStgBonus") != "1")
            {
                spawnNpc(2013002, new Point(46, 840));
                eim.setProperty("statusStgBonus", "1");
            }

            return Task.CompletedTask;
        }


        // Reactor: 2002015 
        public Task fgodItem14()
        {
            // TODO
            // 2002015


            return Task.CompletedTask;
        }


        // Reactor: 2002016 
        public Task fgodItem15()
        {
            // TODO
            // 2002016


            return Task.CompletedTask;
        }


        // Reactor: 2002017 
        public Task fgodItem16()
        {
            // TODO
            // 2002017
            sprayItems(true, 1, 100, 400, 15);

            return Task.CompletedTask;
        }


        // Reactor: 2002018 
        public Task fgodItem17()
        {
            // TODO
            // 2002018
            sprayItems(true, 1, 100, 400, 15);

            return Task.CompletedTask;
        }


        // Reactor: 2006000 
        public Task fgodNPC0()
        {
            // TODO
            // 2006000
            mapMessage(5, "As the light flickers, someone appears out of the light.");
            spawnNpc(2013001);

            return Task.CompletedTask;
        }


        // Reactor: 2006001 
        public Task fgodNPC1()
        {
            // TODO
            // 2006001
            spawnNpc(2013002);
            var eim = GetEventInstanceTrust();
            eim.clearPQ();

            eim.setProperty("statusStg8", "1");
            eim.giveEventPlayersExp(3500);
            eim.showClearEffect(true);

            eim.startEventTimer(5 * 60000); //bonus time

            return Task.CompletedTask;
        }


        // Reactor: 2092000 
        public Task snowdrop()
        {
            // TODO
            // 2092000


            return Task.CompletedTask;
        }


        // Reactor: 2092001 
        public Task snowdrop1()
        {
            // TODO
            // 2092001
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2092002 
        public Task SEAitem0()
        {
            // TODO
            // 2092002


            return Task.CompletedTask;
        }


        // Reactor: 2092003 
        public Task canedrop()
        {
            // TODO
            // 2092003


            return Task.CompletedTask;
        }


        // Reactor: 2099000 
        public Task snowScript0()
        {
            // TODO
            // 2099000


            return Task.CompletedTask;
        }


        // Reactor: 2110000 
        public Task go280010000()
        {
            // TODO
            // 2110000
            playerMessage(5, "An unknown force has moved you to the starting point.");
            warp(280010000, 0);

            return Task.CompletedTask;
        }


        // Reactor: 2111000 
        public Task boxMob0()
        {
            // TODO
            // 2111000
            playerMessage(5, "Oh noes! Monsters in the chest!");
            spawnMonster(9300004, 3);

            return Task.CompletedTask;
        }


        // Reactor: 2111001 
        public Task boss()
        {
            // TODO
            // 2111001
            var eim = getEventInstance();
            if (eim != null)
            {
                eim.setProperty("summoned", "true");
                eim.setProperty("canEnter", "false");
            }
            changeMusic("Bgm06/FinalFight");
            SpawnZakum();

            createMapMonitor(280030000, "ps00");
            mapMessage(5, "Zakum is summoned by the force of Eye of Fire.");

            return Task.CompletedTask;
        }


        [ScriptName("boxItem0", "boxItem1", "boxItem2", "boxItem3")]
        public Task BoxItem()
        {
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2112004, 2112011 
        public Task boxKey0()
        {
            // TODO
            // 2112004
            dropItems();
            // 2112011
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2112005, 2112012 
        public Task boxPaper0()
        {
            // TODO
            // 2112005
            dropItems();
            // 2112012
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2112006 
        public Task money10000()
        {
            // TODO
            // 2112006
            dropItems(true, 1, 500, 800);

            return Task.CompletedTask;
        }


        // Reactor: 2112013 
        public Task money100()
        {
            // TODO
            // 2112013
            dropItems(true, 1, 125, 175);

            return Task.CompletedTask;
        }


        // Reactor: 2112014 
        public Task boxBItem0()
        {
            // TODO
            // 2112014
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2112015 
        public Task s4frameItem0()
        {
            // TODO
            // 2112015
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2112016 
        public Task s4fireHawkItem0()
        {
            // TODO
            // 2112016
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2112017 
        public Task s4iceEagleItem0()
        {
            // TODO
            // 2112017
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2119000 
        public Task scaScript1()
        {
            // TODO
            // 2119000
            // If the chest is destroyed before Riche, killing him should yield no exp

            return Task.CompletedTask;
        }


        // Reactor: 2119001 
        public Task scaScript2()
        {
            // TODO
            // 2119001
            // If the chest is destroyed before Riche, killing him should yield no exp

            return Task.CompletedTask;
        }


        // Reactor: 2119002 
        public Task scaScript3()
        {
            // TODO
            // 2119002
            // If the chest is destroyed before Riche, killing him should yield no exp

            return Task.CompletedTask;
        }


        // Reactor: 2119003 
        public Task scaScript4()
        {
            // TODO
            // 2119003
            // If the chest is destroyed before Riche, killing him should yield no exp

            return Task.CompletedTask;
        }


        [ScriptName("snowscaScript0", "snowscaScript1", "snowscaScript2")]
        public Task SnowscaScript()
        {
            // TODO
            // 2119006
            weakenAreaBoss(6090001, "The light at the altar appeases the hatred of the Snow Witch. The force of the Witch has weakened.");

            return Task.CompletedTask;
        }


        // Reactor: 2200000 
        public Task go221024400()
        {
            // TODO
            // 2200000
            playerMessage(5, "Gotcha! Try again next time!");
            warp(221023200);

            return Task.CompletedTask;
        }


        // Reactor: 2200001 
        public Task ludiPotal0()
        {
            // TODO
            // 2200001
            playerMessage(5, "You have found a secret factory!");
            warp(Random.Shared.NextDouble() < .5 ? 922000020 : 922000021, 0);

            return Task.CompletedTask;
        }


        // Reactor: 2200002 
        public Task go922010201()
        {
            // TODO
            // 2200002
            mapMessage(5, "An unknown force has warped you into a trap.");
            warpMap(922010201);

            return Task.CompletedTask;
        }


        // Reactor: 2201000 
        public Task ludiMob0()
        {
            // TODO
            // 2201000
            spawnMonster(9300011, 10);

            return Task.CompletedTask;
        }


        // Reactor: 2201001 
        public Task ludiMob1()
        {
            // TODO
            // 2201001
            for (var i = 0; i < 3; i++)
            {
                spawnMonster(9300007);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2201002 
        public Task ludiMob2()
        {
            // TODO
            // 2201002
            mapMessage(5, "Rombard has been summoned somewhere in the map.");
            spawnMonster(9300010, 1, -211);

            return Task.CompletedTask;
        }


        // Reactor: 2201003 
        public Task ludiBoss0()
        {
            // TODO
            // 2201003
            if (getPlayer().getMapId() == 922010900)
            {
                mapMessage(5, "Alishar has been summoned.");
                spawnMonster(9300012, 941, 184);
            }
            else if (getPlayer().getMapId() == 922010700)
            {
                mapMessage(5, "Rombard has been summoned somewhere in the map.");
                spawnMonster(9300010, 1, -211);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2201004 
        public Task boss2()
        {
            // TODO
            // 2201004
            mapMessage(5, "The dimensional hole has been filled by the <Piece of Cracked Dimension>.");
            changeMusic("Bgm09/TimeAttack");
            spawnMonster(8500000, -410, -400);
            createMapMonitor(220080001, "in00");

            return Task.CompletedTask;
        }


        // Reactor: 2202000 
        public Task ludiquest0()
        {
            // TODO
            // 2202000
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2202001 
        public Task ludiquest1()
        {
            // TODO
            // 2202001
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2202002 
        public Task ludiquest2()
        {
            // TODO
            // 2202002
            if (isQuestActive(3238))
            {
                warp(922000020, 0);
            }
            else
            {
                warp(922000009, 0);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2202003 
        public Task ludiquest3()
        {
            // TODO
            // 2202003
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2202004 
        public Task ludiquest4()
        {
            // TODO
            // 2202004
            sprayItems(true, 1, 30, 60, 15);

            return Task.CompletedTask;
        }


        // Reactor: 2212000 
        public Task osquest0()
        {
            // TODO
            // 2212000
            dropItems(true, 2, 80, 100);

            return Task.CompletedTask;
        }


        // Reactor: 2212001 
        public Task osquest1()
        {
            // TODO
            // 2212001
            dropItems(true, 2, 80, 100);

            return Task.CompletedTask;
        }


        // Reactor: 2212002 
        public Task osquest2()
        {
            // TODO
            // 2212002
            dropItems(true, 2, 80, 100);

            return Task.CompletedTask;
        }


        // Reactor: 2212003 
        public Task osquest3()
        {
            // TODO
            // 2212003
            dropItems(true, 2, 80, 100);

            return Task.CompletedTask;
        }


        // Reactor: 2212004 
        public Task osquest4()
        {
            // TODO
            // 2212004
            dropItems(true, 2, 80, 100);

            return Task.CompletedTask;
        }


        // Reactor: 2212005 
        public Task osquest5()
        {
            // TODO
            // 2212005
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2221000 
        public Task fvMob0()
        {
            // TODO
            // 2221000
            spawnMonster(7130400);
            mapMessage(5, "Here comes Yellow King Goblin!");

            return Task.CompletedTask;
        }


        // Reactor: 2221001 
        public Task fvMob1()
        {
            // TODO
            // 2221001
            spawnMonster(7130401);
            mapMessage(5, "Here comes Blue King Goblin!");

            return Task.CompletedTask;
        }


        // Reactor: 2221002 
        public Task fvMob2()
        {
            // TODO
            // 2221002
            spawnMonster(7130402, -340, 100);
            mapMessage(5, "Here comes Green King Goblin!");

            return Task.CompletedTask;
        }


        // Reactor: 2221003 
        public Task fvquest0()
        {
            // TODO
            // 2221003
            spawnMonster(9500400);

            return Task.CompletedTask;
        }


        // Reactor: 2221004 
        public Task fvquest1()
        {
            // TODO
            // 2221004
            spawnMonster(9500400);

            return Task.CompletedTask;
        }


        // Reactor: 2222000 
        public Task fvquest2()
        {
            // TODO
            // 2222000
            dropItems(true, 2, 80, 120);

            return Task.CompletedTask;
        }


        // Reactor: 2222001 
        public Task fvevent0()
        {
            // TODO
            // 2222001


            return Task.CompletedTask;
        }


        // Reactor: 2222002 
        public Task fvevent1()
        {
            // TODO
            // 2222002


            return Task.CompletedTask;
        }


        // Reactor: 2229009 
        public Task fvscaScript0()
        {
            // TODO
            // 2229009
            weakenAreaBoss(6090003, "The grieving Scholar Ghost has been slightly appeased. You may be able to defeat the Scholar Ghost.");

            return Task.CompletedTask;
        }


        // Reactor: 2292001 
        public Task amberItem0()
        {
            // TODO
            // 2292001


            return Task.CompletedTask;
        }


        // Reactor: 2292002 
        public Task amberItem1()
        {
            // TODO
            // 2292002


            return Task.CompletedTask;
        }


        // Reactor: 2292003 
        public Task amberItem2()
        {
            // TODO
            // 2292003


            return Task.CompletedTask;
        }


        // Reactor: 2292004 
        public Task amberItem3()
        {
            // TODO
            // 2292004


            return Task.CompletedTask;
        }


        // Reactor: 2292005 
        public Task amberItem4()
        {
            // TODO
            // 2292005


            return Task.CompletedTask;
        }


        // Reactor: 2292006 
        public Task amberItem5()
        {
            // TODO
            // 2292006


            return Task.CompletedTask;
        }


        // Reactor: 2298001 
        public Task hwMob0()
        {
            // TODO
            // 2298001


            return Task.CompletedTask;
        }


        // Reactor: 2302000 
        public Task aquaItem0()
        {
            // TODO
            // 2302000
            dropItems(true, 2, 75, 90);

            return Task.CompletedTask;
        }


        // Reactor: 2302001 
        public Task aquaItem1()
        {
            // TODO
            // 2302001
            //dropItems(true, 2, 105, 140);

            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2302002 
        public Task aquaItem2()
        {
            // TODO
            // 2302002
            dropItems(true, 2, 55, 70);

            return Task.CompletedTask;
        }


        // Reactor: 2302003 
        public Task s4resurItem0()
        {
            // TODO
            // 2302003
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2302005 
        public Task tameItem0()
        {
            // TODO
            // 2302005
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2401000 
        public Task hontaleBoss()
        {
            // TODO
            // 2401000
            changeMusic("Bgm14/HonTale");
            if (getReactor().getMap().getMonsterById(8810026) == null)
            {
                getReactor().getMap().spawnHorntailOnGroundBelow(new Point(71, 260));

                var eim = GetEventInstanceTrust();
                eim.restartEventTimer(60 * 60000);
            }
            mapMessage(6, "From the depths of his cave, here comes Horntail!");

            return Task.CompletedTask;
        }


        // Reactor: 2401001 
        public Task s4fireHawkMob0()
        {
            // TODO
            // 2401001
            spawnMonster(9300089);

            return Task.CompletedTask;
        }


        // Reactor: 2401002 
        public Task s4iceEagleMob0()
        {
            // TODO
            // 2401002
            spawnMonster(9300090);

            return Task.CompletedTask;
        }


        // Reactor: 2402000 
        public Task leafItem0()
        {
            // TODO
            // 2402000
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2402001 
        public Task leafItem1()
        {
            // TODO
            // 2402001
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2402002 
        public Task hontaleItem0()
        {
            // TODO
            // 2402002


            return Task.CompletedTask;
        }


        // Reactor: 2402003 
        public Task hontaleItem1()
        {
            // TODO
            // 2402003


            return Task.CompletedTask;
        }


        // Reactor: 2402004 
        public Task hontaleItem2()
        {
            // TODO
            // 2402004


            return Task.CompletedTask;
        }


        // Reactor: 2402005 
        public Task hontaleItem3()
        {
            // TODO
            // 2402005


            return Task.CompletedTask;
        }


        // Reactor: 2402006 
        public Task hontaleItem4()
        {
            // TODO
            // 2402006
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2402007, 2402008 
        public Task neoCityItem0()
        {
            dropItems(true, 2, 5, 10, 1);

            return Task.CompletedTask;
        }


        // Reactor: 2406000 
        public Task hontaleNPC0()
        {
            // TODO
            // 2406000
            spawnNpc(2081008);
            startQuest(100203);
            mapMessage(6, "光芒闪烁间，龙蛋破壳而出，一只璀璨的幼龙降临世间！");

            return Task.CompletedTask;
        }


        // Reactor: 2502000 
        public Task muruengItem0()
        {
            // TODO
            // 2502000
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2502001 
        public Task muruengItem1()
        {
            // TODO
            // 2502001
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2502002 
        public Task muruengItem2()
        {
            // TODO
            // 2502002
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2511000 
        public Task davyMob0()
        {
            // TODO
            // 2511000
            var eim = GetEventInstanceTrust();
            var now = eim.getIntProperty("openedBoxes");
            var nextNum = now + 1;
            eim.setIntProperty("openedBoxes", nextNum);

            spawnMonster(9300109, 3);
            spawnMonster(9300110, 5);

            return Task.CompletedTask;
        }


        // Reactor: 2511001 
        public Task davyMob1()
        {
            // TODO
            // 2511001
            for (var i = 0; i < 6; i++)
            {
                spawnMonster(9300124);
                spawnMonster(9300125);
            }

            return Task.CompletedTask;
        }


        // Reactor: 2512000 
        public Task davyItem0()
        {
            // TODO
            // 2512000
            var eim = GetEventInstanceTrust();
            var now = eim.getIntProperty("openedBoxes");
            var nextNum = now + 1;
            eim.setIntProperty("openedBoxes", nextNum);

            dropItems(true, 1, 30, 60, 15);

            var map = getMap();
            if (map.countMonsters() == 0 && (eim.getIntProperty("grindMode") == 0 || eim.activatedAllReactorsOnMap(map, 2511000, 2517999)))
            {
                eim.showClearEffect(map.getId());
            }

            return Task.CompletedTask;
        }


        // Reactor: 2512001 
        public Task davyItem1()
        {
            // TODO
            // 2512001
            var eim = GetEventInstanceTrust();
            var now = eim.getIntProperty("openedChests");
            var nextNum = now + 1;
            eim.setIntProperty("openedChests", nextNum);
            sprayItems(true, 1, 50, 100, 15);

            return Task.CompletedTask;
        }


        // Reactor: 2516000 
        public Task davyNPC0()
        {
            // TODO
            // 2516000
            mapMessage(5, "As Lord Pirate dies, Wu Yang is released!");
            spawnNpc(2094001);

            return Task.CompletedTask;
        }


        // Reactor: 2519000 
        public Task davyScript0()
        {
            // TODO
            // 2519000
            var denyWidth = 320;
            var denyHeight = 150;
            var denyPos = getReactor().getPosition();
            var denyArea = new Rectangle(denyPos.X - denyWidth / 2, denyPos.Y - denyHeight / 2, denyWidth, denyHeight);

            getReactor().getMap().setAllowSpawnPointInBox(false, denyArea);

            var map = getReactor().getMap();
            if (map.getReactorByName("sMob2")?.getState() >= 1
                && map.getReactorByName("sMob3")?.getState() >= 1
                && map.getReactorByName("sMob4")?.getState() >= 1
                && map.countMonsters() == 0)
            {
                GetEventInstanceTrust().showClearEffect(map.getId());
            }

            return Task.CompletedTask;
        }


        // Reactor: 2519001 
        public Task davyScript1()
        {
            // TODO
            // 2519001
            var denyWidth = 320;
            var denyHeight = 150;
            var denyPos = getReactor().getPosition();
            var denyArea = new Rectangle(denyPos.X - denyWidth / 2, denyPos.Y - denyHeight / 2, denyWidth, denyHeight);

            getReactor().getMap().setAllowSpawnPointInBox(false, denyArea);

            var map = getReactor().getMap();
            if (map.getReactorByName("sMob1")?.getState() >= 1
                && map.getReactorByName("sMob3")?.getState() >= 1
                && map.getReactorByName("sMob4")?.getState() >= 1
                && map.countMonsters() == 0)
            {
                GetEventInstanceTrust().showClearEffect(map.getId());
            }

            return Task.CompletedTask;
        }


        // Reactor: 2519002 
        public Task davyScript2()
        {
            // TODO
            // 2519002
            var denyWidth = 320;
            var denyHeight = 150;
            var denyPos = getReactor().getPosition();
            var denyArea = new Rectangle(denyPos.X - denyWidth / 2, denyPos.Y - denyHeight / 2, denyWidth, denyHeight);

            getReactor().getMap().setAllowSpawnPointInBox(false, denyArea);

            var map = getReactor().getMap();
            if (map.getReactorByName("sMob1")?.getState() >= 1
                && map.getReactorByName("sMob2")?.getState() >= 1
                && map.getReactorByName("sMob4")?.getState() >= 1 && map.countMonsters() == 0)
            {
                GetEventInstanceTrust().showClearEffect(map.getId());
            }

            return Task.CompletedTask;
        }


        // Reactor: 2519003 
        public Task davyScript3()
        {
            // TODO
            // 2519003
            var denyWidth = 320;
            var denyHeight = 150;
            var denyPos = getReactor().getPosition();
            var denyArea = new Rectangle(denyPos.X - denyWidth / 2, denyPos.Y - denyHeight / 2, denyWidth, denyHeight);

            var map = getReactor().getMap();
            map.setAllowSpawnPointInBox(false, denyArea);

            if (map.getReactorByName("sMob1")?.getState() >= 1
                && map.getReactorByName("sMob2")?.getState() >= 1
                && map.getReactorByName("sMob3")?.getState() >= 1
                && map.countMonsters() == 0)
            {
                GetEventInstanceTrust().showClearEffect(map.getId());
            }

            return Task.CompletedTask;
        }


        // Reactor: 2602000 
        public Task ariantItem0()
        {
            // TODO
            // 2602000
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2612000 
        public Task magatiaItem0()
        {
            // TODO
            // 2612000
            sprayItems();

            return Task.CompletedTask;
        }


        // Reactor: 2612001 
        public Task rnjItem0()
        {
            // TODO
            // 2612001
            sprayItems();

            return Task.CompletedTask;
        }


        // Reactor: 2612002 
        public Task rnjItem1()
        {
            // TODO
            // 2612002
            sprayItems();

            return Task.CompletedTask;
        }


        // Reactor: 2612003 
        public Task rnjItem2()
        {
            // TODO
            // 2612003
            sprayItems();

            return Task.CompletedTask;
        }


        // Reactor: 2612004 
        public Task magatiaItem1()
        {
            // TODO
            // 2612004


            return Task.CompletedTask;
        }


        // Reactor: 2612005 
        public Task magatiaItem2()
        {
            // TODO
            // 2612005
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 2612006 
        public Task magatiaItem3()
        {
            // TODO
            // 2612006


            return Task.CompletedTask;
        }


        // Reactor: 2612007 
        public Task magatiaItem4()
        {
            // TODO
            // 2612007


            return Task.CompletedTask;
        }


        // Reactor: 2619000 
        public Task magaScript0()
        {
            // TODO
            // 2619000
            // There's a timeout of 3 seconds to revert back from state 1 to 0.
            // Reactor is destroyed (state 2) and triggers this if dropping two Magic Devices at once, which shouldn't really happen.

            return Task.CompletedTask;
        }


        // Reactor: 2619001 
        public Task rnjScript0()
        {
            // TODO
            // 2619001


            return Task.CompletedTask;
        }


        // Reactor: 2619002 
        public Task rnjScript1()
        {
            // TODO
            // 2619002


            return Task.CompletedTask;
        }


        // Reactor: 2619003 
        public Task magascaScript0()
        {
            // TODO
            // 2619003
            weakenAreaBoss(6090004, "Rurumo has been poisoned. It may finally be defeatable!");

            return Task.CompletedTask;
        }


        // Reactor: 2619004 
        public Task magascaScript1()
        {
            // TODO
            // 2619004
            weakenAreaBoss(6090004, "Rurumo has been poisoned. It may finally be defeatable!");

            return Task.CompletedTask;
        }


        // Reactor: 2619005 
        public Task magascaScript2()
        {
            // TODO
            // 2619005
            weakenAreaBoss(6090004, "Rurumo has been poisoned. It may finally be defeatable!");

            return Task.CompletedTask;
        }


        // Reactor: 2708000 
        public Task PinkBeenBack0()
        {
            // TODO
            // 2708000


            return Task.CompletedTask;
        }


        // Reactor: 2709000 
        public Task PinkBeenScript0()
        {
            // TODO
            // 2709000


            return Task.CompletedTask;
        }


        // Reactor: 3001000 
        public Task pFBoss()
        {
            // TODO
            // 3001000
            playerMessage(5, "Poison Golem has been spawned.");
            spawnMonster(9300180, 1);

            return Task.CompletedTask;
        }


        // Reactor: 3002000 
        public Task pFItem0()
        {
            // TODO
            // 3002000
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 3002001 
        public Task pFItem1()
        {
            // TODO
            // 3002001
            GetEventInstanceTrust().showClearEffect(getMap().getId());
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 3008000 
        public Task pFBack0()
        {
            // TODO
            // 3008000


            return Task.CompletedTask;
        }


        // Reactor: 5411000 
        public Task sgboss0()
        {
            // TODO
            // 5411000
            changeMusic("Bgm09/TimeAttack");
            spawnMonster(9420513, -146, 225);
            GetEventInstanceTrust().setIntProperty("boss", 1);
            mapMessage(5, "As you wish, here comes Capt Latanica.");

            return Task.CompletedTask;
        }


        // Reactor: 5511000 
        public Task myboss0()
        {
            // TODO
            // 5511000
            var targaMobId = 9420542;
            if (getReactor().getMap().getMonsterById(targaMobId) == null)
            {
                summonBossDelayed(targaMobId, 3200, -527, 637, "Bgm09/TimeAttack", "Beware! The furious Targa has shown himself!");
            }

            return Task.CompletedTask;
        }


        // Reactor: 5511001 
        public Task myboss1()
        {
            // TODO
            // 5511001
            var scarlionMobId = 9420547;
            if (getReactor().getMap().getMonsterById(scarlionMobId) == null)
            {
                summonBossDelayed(scarlionMobId, 3200, -238, 636, "Bgm09/TimeAttack", "Beware! The furious Scarlion has shown himself!");
            }

            return Task.CompletedTask;
        }


        // Reactor: 6102001 
        public Task glpqitem2()
        {
            // TODO
            // 6102001
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 6102002 
        public Task glpqreward1()
        {
            // TODO
            // 6102002
            sprayItems(true, 1, 90, 360, 15);

            return Task.CompletedTask;
        }


        // Reactor: 6102003 
        public Task glpqreward2()
        {
            // TODO
            // 6102003
            sprayItems(true, 1, 90, 360, 15);

            return Task.CompletedTask;
        }


        // Reactor: 6102004 
        public Task glpqreward3()
        {
            // TODO
            // 6102004
            sprayItems(true, 1, 90, 360, 15);

            return Task.CompletedTask;
        }


        // Reactor: 6102005 
        public Task glpqreward4()
        {
            // TODO
            // 6102005
            sprayItems(true, 1, 90, 360, 15);

            return Task.CompletedTask;
        }


        // Reactor: 6109000 
        public Task glpqskill0()
        {
            // TODO
            // 6109000
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                var mapId = getMap().getId();

                if (mapId == 610030200)
                {
                    eim.dropMessage(6, "The Warrior Sigil has been activated!");
                    eim.setIntProperty("glpq2", eim.getIntProperty("glpq2") + 1);
                    if (eim.getIntProperty("glpq2") == 5)
                    { //all 5 done
                        eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                        eim.showClearEffect(mapId, "2pt", 2);
                        eim.giveEventPlayersStageReward(2);
                    }
                }
                else if (mapId == 610030300)
                {
                    eim.dropMessage(6, "The Warrior Sigil has been activated! You hear gears turning! The Menhir Defense System is active! Run!");
                    eim.setIntProperty("glpq3", eim.getIntProperty("glpq3") + 1);
                    getMap().moveEnvironment("menhir0", 1);
                    if (eim.getIntProperty("glpq3") == 5 && eim.getIntProperty("glpq3_p") == 5)
                    {
                        eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                        eim.showClearEffect(mapId, "3pt", 2);
                        eim.giveEventPlayersStageReward(3);
                    }
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109001 
        public Task glpqskill1()
        {
            // TODO
            // 6109001
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                var mapId = getMap().getId();

                if (mapId == 610030200)
                {
                    eim.dropMessage(6, "The Archer Sigil has been activated!");
                    eim.setIntProperty("glpq2", eim.getIntProperty("glpq2") + 1);
                    if (eim.getIntProperty("glpq2") == 5)
                    { //all 5 done
                        eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                        eim.showClearEffect(mapId, "2pt", 2);
                        eim.giveEventPlayersStageReward(2);
                    }
                }
                else if (mapId == 610030300)
                {
                    eim.dropMessage(6, "The Archer Sigil has been activated! You hear gears turning! The Menhir Defense System is active! Run!");
                    eim.setIntProperty("glpq3", eim.getIntProperty("glpq3") + 1);
                    getMap().moveEnvironment("menhir1", 1);
                    getMap().moveEnvironment("menhir2", 1);
                    if (eim.getIntProperty("glpq3") == 5 && eim.getIntProperty("glpq3_p") == 5)
                    {
                        eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                        eim.showClearEffect(mapId, "3pt", 2);
                        eim.giveEventPlayersStageReward(3);
                    }
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109002 
        public Task glpqskill2()
        {
            // TODO
            // 6109002
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                var mapId = getMap().getId();

                if (mapId == 610030200)
                {
                    eim.dropMessage(6, "The Mage Sigil has been activated!");
                    eim.setIntProperty("glpq2", eim.getIntProperty("glpq2") + 1);
                    if (eim.getIntProperty("glpq2") == 5)
                    { //all 5 done
                        eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                        eim.showClearEffect(mapId, "2pt", 2);
                        eim.giveEventPlayersStageReward(2);
                    }
                }
                else if (mapId == 610030300)
                {
                    eim.dropMessage(6, "The Mage Sigil has been activated! You hear gears turning! The Menhir Defense System is active! Run!");
                    eim.setIntProperty("glpq3", eim.getIntProperty("glpq3") + 1);
                    getMap().moveEnvironment("menhir3", 1);
                    if (eim.getIntProperty("glpq3") == 5 && eim.getIntProperty("glpq3_p") == 5)
                    {
                        eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                        eim.showClearEffect(mapId, "3pt", 2);
                        eim.giveEventPlayersStageReward(3);
                    }
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109003 
        public Task glpqskill3()
        {
            // TODO
            // 6109003
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                var mapId = getMap().getId();

                if (mapId == 610030200)
                {
                    eim.dropMessage(6, "The Thief Sigil has been activated!");
                    eim.setIntProperty("glpq2", eim.getIntProperty("glpq2") + 1);
                    if (eim.getIntProperty("glpq2") == 5)
                    { //all 5 done
                        eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                        eim.showClearEffect(mapId, "2pt", 2);
                        eim.giveEventPlayersStageReward(2);
                    }
                }
                else if (mapId == 610030300)
                {
                    eim.dropMessage(6, "The Thief Sigil has been activated! You hear gears turning! The Menhir Defense System is active! Run!");
                    eim.setIntProperty("glpq3", eim.getIntProperty("glpq3") + 1);
                    getMap().moveEnvironment("menhir4", 1);
                    if (eim.getIntProperty("glpq3") == 5 && eim.getIntProperty("glpq3_p") == 5)
                    {
                        mapMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                        eim.showClearEffect(mapId, "3pt", 2);
                        eim.giveEventPlayersStageReward(3);
                    }
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109004 
        public Task glpqskill4()
        {
            // TODO
            // 6109004
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                var mapId = getMap().getId();

                if (mapId == 610030200)
                {
                    eim.dropMessage(6, "The Pirate Sigil has been activated!");
                    eim.setIntProperty("glpq2", eim.getIntProperty("glpq2") + 1);
                    if (eim.getIntProperty("glpq2") == 5)
                    { //all 5 done
                        eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                        eim.showClearEffect(mapId, "2pt", 2);
                        eim.giveEventPlayersStageReward(2);
                    }
                }
                else if (mapId == 610030300)
                {
                    eim.dropMessage(6, "The Pirate Sigil has been activated! You hear gears turning! The Menhir Defense System is active! Run!");
                    eim.setIntProperty("glpq3", eim.getIntProperty("glpq3") + 1);
                    getMap().moveEnvironment("menhir5", 1);
                    if (eim.getIntProperty("glpq3") == 5 && eim.getIntProperty("glpq3_p") == 5)
                    {
                        eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                        eim.showClearEffect(mapId, "3pt", 2);
                        eim.giveEventPlayersStageReward(3);
                    }
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109005 
        public Task glpqweapon0()
        {
            // TODO
            // 6109005
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                eim.dropMessage(6, "A weapon has been restored to the Relic of Mastery!");
                eim.setIntProperty("glpq5", eim.getIntProperty("glpq5") + 1);
                if (eim.getIntProperty("glpq5") == 5)
                { //all 5 done
                    eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                    eim.showClearEffect(610030500, "5pt", 2);
                    eim.giveEventPlayersStageReward(5);
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109006 
        public Task glpqweapon1()
        {
            // TODO
            // 6109006
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                eim.dropMessage(6, "A weapon has been restored to the Relic of Mastery!");
                eim.setIntProperty("glpq5", eim.getIntProperty("glpq5") + 1);
                if (eim.getIntProperty("glpq5") == 5)
                { //all 5 done
                    eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                    eim.showClearEffect(610030500, "5pt", 2);
                    eim.giveEventPlayersStageReward(5);
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109007 
        public Task glpqweapon2()
        {
            // TODO
            // 6109007
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                eim.dropMessage(6, "A weapon has been restored to the Relic of Mastery!");
                eim.setIntProperty("glpq5", eim.getIntProperty("glpq5") + 1);
                if (eim.getIntProperty("glpq5") == 5)
                { //all 5 done
                    eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                    eim.showClearEffect(610030500, "5pt", 2);
                    eim.giveEventPlayersStageReward(5);
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109008 
        public Task glpqweapon3()
        {
            // TODO
            // 6109008
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                eim.dropMessage(6, "A weapon has been restored to the Relic of Mastery!");
                eim.setIntProperty("glpq5", eim.getIntProperty("glpq5") + 1);
                if (eim.getIntProperty("glpq5") == 5)
                { //all 5 done
                    eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                    eim.showClearEffect(610030500, "5pt", 2);
                    eim.giveEventPlayersStageReward(5);
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109009 
        public Task glpqweapon4()
        {
            // TODO
            // 6109009
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                eim.dropMessage(6, "A weapon has been restored to the Relic of Mastery!");
                eim.setIntProperty("glpq5", eim.getIntProperty("glpq5") + 1);
                if (eim.getIntProperty("glpq5") == 5)
                { //all 5 done
                    eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                    eim.showClearEffect(610030500, "5pt", 2);
                    eim.giveEventPlayersStageReward(5);
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109010 
        public Task glpqmob0()
        {
            // TODO
            // 6109010


            return Task.CompletedTask;
        }


        // Reactor: 6109011 
        public Task glpqmob1()
        {
            // TODO
            // 6109011


            return Task.CompletedTask;
        }


        // Reactor: 6109013 
        public Task glpqstrge()
        {
            // TODO
            // 6109013


            return Task.CompletedTask;
        }


        // Reactor: 6109014 
        public Task glpqflame0()
        {
            // TODO
            // 6109014


            return Task.CompletedTask;
        }


        // Reactor: 6109016 
        public Task glpqskill5()
        {
            // TODO
            // 6109016
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                eim.dropMessage(6, "The Warrior Sigil has been activated!");
                eim.setIntProperty("glpq4", eim.getIntProperty("glpq4") + 1);
                if (eim.getIntProperty("glpq4") == 5)
                { //all 5 done
                    eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                    eim.showClearEffect(610030400, "4pt", 2);
                    eim.giveEventPlayersStageReward(4);
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109017 
        public Task glpqskill6()
        {
            // TODO
            // 6109017
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                eim.dropMessage(6, "The Archer Sigil has been activated!");
                eim.setIntProperty("glpq4", eim.getIntProperty("glpq4") + 1);
                if (eim.getIntProperty("glpq4") == 5)
                { //all 5 done
                    eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                    eim.showClearEffect(610030400, "4pt", 2);
                    eim.giveEventPlayersStageReward(4);
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109018 
        public Task glpqskill7()
        {
            // TODO
            // 6109018
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                eim.dropMessage(6, "The Mage Sigil has been activated!");
                eim.setIntProperty("glpq4", eim.getIntProperty("glpq4") + 1);
                if (eim.getIntProperty("glpq4") == 5)
                { //all 5 done
                    eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                    eim.showClearEffect(610030400, "4pt", 2);
                    eim.giveEventPlayersStageReward(4);
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109019 
        public Task glpqskill8()
        {
            // TODO
            // 6109019
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                eim.dropMessage(6, "The Thief Sigil has been activated!");
                eim.setIntProperty("glpq4", eim.getIntProperty("glpq4") + 1);
                if (eim.getIntProperty("glpq4") == 5)
                { //all 5 done
                    eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                    eim.showClearEffect(610030400, "4pt", 2);
                    eim.giveEventPlayersStageReward(4);
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109020 
        public Task glpqskill9()
        {
            // TODO
            // 6109020
            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                eim.dropMessage(6, "The Pirate Sigil has been activated!");
                eim.setIntProperty("glpq4", eim.getIntProperty("glpq4") + 1);
                if (eim.getIntProperty("glpq4") == 5)
                { //all 5 done
                    eim.dropMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                    eim.showClearEffect(610030400, "4pt", 2);
                    eim.giveEventPlayersStageReward(4);
                }
            }

            return Task.CompletedTask;
        }


        // Reactor: 6109021 
        public Task glpqflame1()
        {
            // TODO
            // 6109021


            return Task.CompletedTask;
        }


        // Reactor: 6109022 
        public Task glpqflame2()
        {
            // TODO
            // 6109022


            return Task.CompletedTask;
        }


        // Reactor: 6109023 
        public Task glpqflame3()
        {
            // TODO
            // 6109023


            return Task.CompletedTask;
        }


        // Reactor: 6109024 
        public Task glpqflame4()
        {
            // TODO
            // 6109024


            return Task.CompletedTask;
        }


        // Reactor: 6109025 
        public Task glpqflame5()
        {
            // TODO
            // 6109025


            return Task.CompletedTask;
        }


        // Reactor: 6109026 
        public Task glpqflame6()
        {
            // TODO
            // 6109026


            return Task.CompletedTask;
        }


        // Reactor: 6109027 
        public Task glpqflame7()
        {
            // TODO
            // 6109027


            return Task.CompletedTask;
        }


        [ScriptName("amoriaboxMob0", "amoriaboxMob1", "amoriaboxMob2")]
        public Task AmoriaboxMob()
        {
            var startId = 9400523;
            var mapObj = getMap();
            for (var i = 0; i < 7; i++)
            {
                var mobObj = LifeFactory.Instance.getMonster(startId + Random.Shared.Next(3));
                mapObj.spawnMonsterOnGroundBelow(mobObj, getReactor().getPosition());
            }

            return Task.CompletedTask;
        }


        // Reactor: 6702000 
        public Task amoriaItem0()
        {
            // TODO
            // 6702000
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 6702001 
        public Task amoriaItem1()
        {
            // TODO
            // 6702001


            return Task.CompletedTask;
        }


        // Reactor: 6702002 
        public Task amoriaItem2()
        {
            // TODO
            // 6702002


            return Task.CompletedTask;
        }

        [ScriptName("amoriaItem3", "amoriaItem4", "amoriaItem5", "amoriaItem6", "amoriaItem7", "amoriaItem8",
            "amoriaItem9", "amoriaItem10", "amoriaItem11", "amoriaItem12")]
        public Task AmoriaItem()
        {
            // TODO
            // 6702012
            var count = Math.Max(1, Random.Shared.Next(4));
            //We'll make it drop a lot of crap :D
            for (var i = 0; i < count; i++)
            {
                sprayItems(true, 1, 30, 60, 15);
            }

            return Task.CompletedTask;
        }


        // Reactor: 6741001 
        public Task guyfawkesmob0()
        {
            // TODO
            // 6741001
            spawnMonster(9400589);

            return Task.CompletedTask;
        }


        // Reactor: 6741015 
        public Task guyfawkesmob1()
        {
            // TODO
            // 6741015
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 6741016 
        public Task guyfawkesmob2()
        {
            // TODO
            // 6741016


            return Task.CompletedTask;
        }


        // Reactor: 6742014 
        public Task guyfawkesbox0()
        {
            // TODO
            // 6742014
            sprayItems(true, 1, 5, 25, 15);

            return Task.CompletedTask;
        }


        // Reactor: 6802000 
        public Task weddingItem0()
        {
            // TODO
            // 6802000
            sprayItems(true, 1, 100, 400, 15);

            return Task.CompletedTask;
        }


        // Reactor: 6802001 
        public Task weddingItem1()
        {
            // TODO
            // 6802001
            sprayItems(true, 1, 100, 400, 15);

            return Task.CompletedTask;
        }


        // Reactor: 6822000 
        public Task halloweenGLitem()
        {
            // TODO
            // 6822000


            return Task.CompletedTask;
        }


        // Reactor: 6829000 
        public Task halloweenbox()
        {
            // TODO
            // 6829000
            //wtf is this?
            playerMessage(5, "Enjoy Halloween!");
            spawnMonster(9400202, 10);

            return Task.CompletedTask;
        }


        // Reactor: 8001000 
        public Task shouwaBoss()
        {
            // TODO
            // 8001000
            spawnMonster(9400112, 1, 420, 160);

            return Task.CompletedTask;
        }


        // Reactor: 8091000 
        public Task JPludiMob0()
        {
            // TODO
            // 8091000
            spawnMonster(9400210, 2);
            spawnMonster(9400209, 2);
            mapMessage(5, "Some monsters are summoned.");

            return Task.CompletedTask;
        }


        // Reactor: 8091001 
        public Task JPludiMob1()
        {
            // TODO
            // 8091001
            spawnMonster(9400211, 2);
            spawnMonster(9400212, 2);
            mapMessage(5, "Some monsters are summoned.");

            return Task.CompletedTask;
        }


        // Reactor: 8091002 
        public Task JPludiMob2()
        {
            // TODO
            // 8091002
            spawnMonster(9400213, 2);
            spawnMonster(9400214, 2);
            mapMessage(5, "Some monsters are summoned.");

            return Task.CompletedTask;
        }


        // Reactor: 8091003 
        public Task JPludiMob3()
        {
            // TODO
            // 8091003
            spawnMonster(9400215, 2);
            spawnMonster(9400216, 2);
            mapMessage(5, "Some monsters are summoned.");

            return Task.CompletedTask;
        }


        // Reactor: 8091004 
        public Task JPludiMob4()
        {
            // TODO
            // 8091004
            spawnMonster(9400217, 2);
            spawnMonster(9400218, 2);
            mapMessage(5, "Some monsters are summoned.");

            return Task.CompletedTask;
        }


        // Reactor: 8892000 
        [ScriptName("08_Snowman0")]
        public Task s_08_Snowman0()
        {
            // TODO
            // 8892000


            return Task.CompletedTask;
        }


        // Reactor: 8892001 
        [ScriptName("08_Cross0")]
        public Task s_08_Cross0()
        {
            // TODO
            // 8892001


            return Task.CompletedTask;
        }



        // Reactor: 9000000 
        public Task eventMap0()
        {
            // TODO
            // 9000000


            return Task.CompletedTask;
        }


        // Reactor: 9000001 
        public Task eventMap1()
        {
            // TODO
            // 9000001


            return Task.CompletedTask;
        }


        // Reactor: 9000002 
        public Task eventMap2()
        {
            // TODO
            // 9000002


            return Task.CompletedTask;
        }


        // Reactor: 9001000 
        public Task eventMob0()
        {
            // TODO
            // 9001000


            return Task.CompletedTask;
        }


        // Reactor: 9002000 
        public Task eventItem0()
        {
            // TODO
            // 9002000


            return Task.CompletedTask;
        }


        // Reactor: 9002001 
        public Task eventItem1()
        {
            // TODO
            // 9002001


            return Task.CompletedTask;
        }


        // Reactor: 9002002 
        public Task eventItem2()
        {
            // TODO
            // 9002002


            return Task.CompletedTask;
        }


        // Reactor: 9101000 
        public Task moonMob0()
        {
            // TODO
            // 9101000
            spawnMonster(9300061, 1, 0, 0); // (0, 0) is temp position
            getMap().startMapEffect("Protect the Moon Bunny that's pounding the mill, and gather up 10 Moon Bunny's Rice Cakes!", 5120016, 7000);
            getMap().broadcastMessage(PacketCreator.bunnyPacket()); // Protect the Moon Bunny!
            getMap().broadcastMessage(PacketCreator.showHPQMoon());
            // getMap().showAllMonsters();

            return Task.CompletedTask;
        }


        // Reactor: 9102000 
        public Task sBoxItem0()
        {
            // TODO
            // 9102000
            dropItems(true, 2, 25, 100);

            return Task.CompletedTask;
        }


        // Reactor: 9102001 
        public Task sBoxItem1()
        {
            // TODO
            // 9102001
            dropItems(true, 2, 25, 100);

            return Task.CompletedTask;
        }


        // Reactor: 9102002 
        public Task moonItem0()
        {
            // TODO
            // 9102002
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9102003 
        public Task moonItem1()
        {
            // TODO
            // 9102003
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9102004 
        public Task moonItem2()
        {
            // TODO
            // 9102004
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9102005 
        public Task moonItem3()
        {
            // TODO
            // 9102005
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9102006 
        public Task moonItem4()
        {
            // TODO
            // 9102006
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9102007 
        public Task moonItem5()
        {
            // TODO
            // 9102007
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9102008 
        public Task dropTreasureBox()
        {
            // TODO
            // 9102008


            return Task.CompletedTask;
        }


        // Reactor: 9201000 
        public Task syarenMob0()
        {
            // TODO
            // 9201000
            spawnMonster(9300033, 8, -100, 50);

            return Task.CompletedTask;
        }


        // Reactor: 9201001 
        public Task syarenNPC0()
        {
            // TODO
            // 9201001
            mapMessage(5, "A bright flash of light, then someone familiar appears in front of the blocked gate.");
            spawnNpc(9040003);

            return Task.CompletedTask;
        }


        // Reactor: 9201002 
        public Task syarenMob1()
        {
            // TODO
            // 9201002
            changeMusic("Bgm10/Eregos");
            spawnMonster(9300028);
            spawnMonster(9300031, 130, 90);
            spawnMonster(9300032, 540, 90);
            spawnMonster(9300029, 130, 150);
            spawnMonster(9300030, 540, 150);

            return Task.CompletedTask;
        }


        // Reactor: 9202000 
        public Task syarenItem0()
        {
            // TODO
            // 9202000
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9202001 
        public Task syarenItem1()
        {
            // TODO
            // 9202001
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9202002 
        public Task syarenItem2()
        {
            // TODO
            // 9202002
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9202003 
        public Task syarenItem3()
        {
            // TODO
            // 9202003
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9202004 
        public Task syarenItem4()
        {
            // TODO
            // 9202004
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9202005 
        public Task syarenItem5()
        {
            // TODO
            // 9202005
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9202006 
        public Task syarenItem6()
        {
            // TODO
            // 9202006
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9202007 
        public Task syarenItem7()
        {
            // TODO
            // 9202007
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9202008 
        public Task syarenItem8()
        {
            // TODO
            // 9202008
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9202009 
        public Task syarenItem9()
        {
            // TODO
            // 9202009
            dropItems();

            return Task.CompletedTask;
        }


        // Reactor: 9202010 
        public Task syarenItem10()
        {
            // TODO
            // 9202010


            return Task.CompletedTask;
        }


        // Reactor: 9202011 
        public Task syarenItem11()
        {
            // TODO
            // 9202011


            return Task.CompletedTask;
        }


        // Reactor: 9202012 
        public Task syarenItem12()
        {
            // TODO
            // 9202012
            sprayItems(true, 1, 30, 60, 10);

            return Task.CompletedTask;
        }


        // Reactor: 9222000 
        [ScriptName("6th_item0")]
        public Task s_6th_item0()
        {
            // TODO
            // 9222000


            return Task.CompletedTask;
        }


        // Reactor: 9702000 
        [ScriptName("5thItem0")]
        public Task s_5thItem0()
        {
            // TODO
            // 9702000


            return Task.CompletedTask;
        }


        // Reactor: 9802000 
        public Task goldRichbox0()
        {
            // TODO
            // 9802000


            return Task.CompletedTask;
        }


        // Reactor: 9802002, 9802003, 9802004, 9802008 
        public Task goldRichItem0()
        {
            // TODO
            // 9802002

            // 9802003

            // 9802004

            // 9802008


            return Task.CompletedTask;
        }


        // Reactor: 9802005 
        public Task goldRichItem1()
        {
            // TODO
            // 9802005


            return Task.CompletedTask;
        }


        // Reactor: 9802006 
        public Task goldRichItem2()
        {
            // TODO
            // 9802006


            return Task.CompletedTask;
        }


        // Reactor: 9902000 
        public Task PB_boxCount()
        {
            // TODO
            // 9902000


            return Task.CompletedTask;
        }


        // Reactor: 9980000, 9980001 
        public Task mcGuardian0()
        {
            // TODO
            dispelAllMonsters(int.Parse(getReactor().getName().Substring(1, 2)), int.Parse(getReactor().getName().Substring(0, 1)));
            return Task.CompletedTask;
        }



    }
}