using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.reactor;

namespace Application.Plugin.Script.Features.CWKPQ
{
    internal sealed class CWKPQReactorActHandler : ReactorActScriptBase
    {
        public CWKPQReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
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
                    await getMap().moveEnvironment("menhir0", 1);
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
                    await getMap().moveEnvironment("menhir1", 1);
                    await getMap().moveEnvironment("menhir2", 1);
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
                    await getMap().moveEnvironment("menhir3", 1);
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
                    await getMap().moveEnvironment("menhir4", 1);
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
                    await getMap().moveEnvironment("menhir5", 1);
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
            // Intentionally empty — touch/untouch handler does the work

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
    }
}
