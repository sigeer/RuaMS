using Application.Shared.Events;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        // Npc: 2101014 
        public async Task aMatchEnt()
        {
            if (getPlayer().getMapId() == 980010000)
            {
                if (getLevel() > 30)
                {
                    await SayOK("你已经超过了#r等级30#k，因此你不能再参与这个副本了。");
                    return;
                }

                ExpeditionType[] allAriants = [ExpeditionType.ARIANT, ExpeditionType.ARIANT1, ExpeditionType.ARIANT2];
                var exped = ExpeditionType.ARIANT;
                var exped1 = ExpeditionType.ARIANT1;
                var exped2 = ExpeditionType.ARIANT2;
                var expedicao = getExpedition(exped);
                var expedicao1 = getExpedition(exped1);
                var expedicao2 = getExpedition(exped2);

                var channelMaps = getClient().getChannelServer().getMapFactory();
                var startSnd = "What would you like to do? \r\n\r\n\t#e#r(Choose a Battle Arena)#n#k\r\n#b";
                var toSnd = startSnd;

                if (expedicao == null)
                {
                    toSnd += "#L0#Battle Arena (1) (Empty)#l\r\n";
                }
                else if (channelMaps.getMap(980010101).getAllPlayers().Count == 0)
                {
                    toSnd += "#L0#Join Battle Arena (1)  Owner (" + expedicao.getLeader().getName() + ")" + " Current Member: " + getExpeditionMemberNames(exped) + "\r\n";
                }
                if (expedicao1 == null)
                {
                    toSnd += "#L1#Battle Arena (2) (Empty)#l\r\n";
                }
                else if (channelMaps.getMap(980010201).getAllPlayers().Count == 0)
                {
                    toSnd += "#L1#Join Battle Arena (2)  Owner (" + expedicao1.getLeader().getName() + ")" + " Current Member: " + getExpeditionMemberNames(exped1) + "\r\n";
                }
                if (expedicao2 == null)
                {
                    toSnd += "#L2#Battle Arena (3) (Empty)#l\r\n";
                }
                else if (channelMaps.getMap(980010301).getAllPlayers().Count == 0)
                {
                    toSnd += "#L2#Join Battle Arena (3)  Owner (" + expedicao2.getLeader().getName() + ")" + " Current Member: " + getExpeditionMemberNames(exped2) + "\r\n";
                }

                if (toSnd == startSnd)
                {
                    await SayOK("所有的战斗竞技场都已经被占用。我建议你稍后再回来，或者换个频道。");
                    return;
                }
                else
                {
                    var option = await SayOption(startSnd);
                    var selectedExped = getExpedition(allAriants[option]);

                    if (selectedExped != null)
                    {
                        if (selectedExped.contains(getPlayer()))
                        {
                            await SayOK("抱歉，你已经在大厅里了。");
                            return;
                        }

                        var playerAdd = selectedExped.addMemberInt(getPlayer());
                        if (playerAdd == 3)
                        {
                            await SayOK("抱歉，大厅现在已经满了。");
                            return;
                        }
                        else
                        {
                            
                            if (playerAdd == 0)
                            {
                                warp(980010000 + (option + 1) * 100, 0);
                                return;
                            }
                            else if (playerAdd == 2)
                            {
                                await SayOK("抱歉，领袖不允许你进入。");
                                return;
                            }
                            else
                            {
                                await SayOK("错误。");
                                return;
                            }
                        }
                    }
                    else
                    {
                        var inputNumber = await SayInputNumber("Up to how many partipants can join in this match? (2~5 people)", 2, 2, 5);
                        var res = createExpedition(exped, true, 0, inputNumber);
                        if (res == 0)
                        {
                            warp(980010000 + (option + 1) * 100, 0);
                            getPlayer().dropMessage("Your arena was created successfully. Wait for people to join the battle.");
                        }
                        else if (res > 0)
                        {
                            await SayOK("抱歉，您已经达到了此次远征的尝试配额！请另选他日再试……");
                        }
                        else
                        {
                            await SayOK("在启动远征时发生了意外错误，请稍后重试。");
                        }
                    }

                }

            }
        }



        // Npc: 2101015 
        public Task aMatchScore()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2101016 
        public Task aMatchRwd()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2101017 
        public Task aMatchPlay()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2101018 
        public Task aMatchMove()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2103000 
        public Task ariant_oasis()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 2103001 
        public Task secret_wall()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2103002 
        public Task ariant_ring()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2103003 
        public Task ariant_house1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2103004 
        public Task ariant_house2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2103005 
        public Task ariant_house3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2103006 
        public Task ariant_house4()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2103009 
        public Task ariant_gold1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2103010 
        public Task ariant_gold2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2103011 
        public Task ariant_gold3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2103012 
        public Task ariant_gold4()
        {
            // TODO
            return Task.CompletedTask;
        }

    }
}
