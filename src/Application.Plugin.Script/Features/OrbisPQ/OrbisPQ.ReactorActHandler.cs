using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;
using System.Drawing;

namespace Application.Plugin.Script.Features.OrbisPQ
{
    internal sealed class OrbisPQReactorActHandler : ReactorActScriptBase
    {
        public OrbisPQReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
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

            await Pink("As the light flickers, someone appears out of the light.");
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
    }
}
