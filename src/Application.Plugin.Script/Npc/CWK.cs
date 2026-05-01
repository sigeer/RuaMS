using System.Drawing;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        // Npc: 9201107 
        public Task glpqstatue0()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201108 
        public Task glpqstatue1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201109 
        public Task glpqstatue2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201110 
        public Task glpqstatue3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201111 
        public Task glpqstatue4()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201112 
        public Task glpqStory()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201113 
        public Task glpqStart()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201114 
        public Task glpqEnter()
        {
            if (haveItem(3992041, 1))
            {
                warp(610030020, "out00");
            }
            else
            {
                playerMessage(5, "The giant gate of iron will not budge no matter what, however there is a visible key-shaped socket.");
            }
            return Task.CompletedTask;
        }


        // Npc: 9201115 
        [ScriptTag(["CWK"])]
        public async Task glpqStory2()
        {
            var eim = GetEventInstanceTrust();
            if (eim.getIntProperty("glpq6") == 3)
            {
                await SayNext("干得漂亮。你超越了扭曲大师。通过那扇门领取你的奖品。");
                return;
            }

            if (!isEventLeader())
            {
                await SayNext("我希望你们的领导和我谈谈。");
                return;
            }

            if (eim.getIntProperty("glpq6") == 0)
            {
                await SayNext("欢迎来到扭曲大师的堡垒。我将是今晚的主持人…");
                await SayNext("今晚，我们有一群冒险岛玩家的盛宴.. 哈哈哈...");
                await SayNext("让我们经过特别训练的守护大师护送你！");
                mapMessage(6, "Engarde! Master Guardians approach!");
                for (var i = 0; i < 10; i++)
                {
                    var mob = eim.getMonster(9400594);
                    var xPos = -1337 + Random.Shared.Next(1337);
                    getMap().spawnMonsterOnGroundBelow(mob, new Point(xPos, 276));
                }
                for (var i = 0; i < 20; i++)
                {
                    var mob = eim.getMonster(9400582);
                    var xPos = -1337 + Random.Shared.Next(1337);
                    getMap().spawnMonsterOnGroundBelow(mob, new Point(xPos, 276));
                }
                eim.setIntProperty("glpq6", 1);
            }
            else if (eim.getIntProperty("glpq6") == 1)
            {
                if (getMap().countMonsters() == 0)
                {
                    await SayOK("嗯，这是什么？你打败了它们？");
                    await SayNext("好吧，无论如何！扭曲之主将很高兴欢迎你。");
                    mapMessage(6, "Twisted Masters approach!");

                    //Margana
                    var mob = eim.getMonster(9400590);
                    getMap().spawnMonsterOnGroundBelow(mob, new Point(-22, 1));

                    //Red Nirg
                    var mob2 = eim.getMonster(9400591);
                    getMap().spawnMonsterOnGroundBelow(mob2, new Point(-22, 276));

                    //Hsalf
                    var mob4 = eim.getMonster(9400593);
                    getMap().spawnMonsterOnGroundBelow(mob4, new Point(496, 276));

                    //Rellik
                    var mob3 = eim.getMonster(9400592);
                    getMap().spawnMonsterOnGroundBelow(mob3, new Point(-496, 276));

                    eim.setIntProperty("glpq6", 2);
                }
                else
                {
                    await SayOK("不要理我。主守护者会护送你！");
                }
            }
            else if (eim.getIntProperty("glpq6") == 2)
            {
                if (getMap().countMonsters() == 0)
                {
                    await SayOK("什么？呃...这不可能发生。");
                    mapMessage(5, "The portal to the next stage has opened!");
                    eim.setIntProperty("glpq6", 3);

                    eim.showClearEffect(true);
                    eim.giveEventPlayersStageReward(6);

                    eim.clearPQ();
                }
                else
                {
                    await SayOK("不要理会我。扭曲之主会护送你！");
                }
            }
        }
    }
}
