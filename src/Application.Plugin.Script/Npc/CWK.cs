using Application.Core.Gameplay.Plugins;
using server.life;
using System.Drawing;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 9201107 
        public async Task glpqstatue0()
        {
            if (getMapId() == 610030500)
            {
                await SayOK("难以置信的力量和能力，任何人都可以实现。但是让一个战士特别的是他们的铁一般意志。无论面对多大的困难，真正的战士都会坚持到胜利确保为止。因此，战士之厅是一条残酷的道路，房间本身和其中的超强怪物都在与你对抗。利用你的技能摆脱影响，打败其中的怪物，到达战士雕像并夺取圣剑。祝你好运！");
            }
            else if (getMapId() == 610030000)
            {
                await SayOK("一支传奇英雄家族，德弗里西恩家族是风暴法师的创始人。这个家族很特别，因为每个儿子或女儿都继承了他们祖先的全部战斗技巧。这种能力被证明非常有用；因为它几乎可以无限制地运用战略、即兴和战术来打败所有的敌人。一个真正的代代相传的家族。");
            }
            else if (getMapId() == 610030510)
            {
                if (getMap().countMonsters() == 0)
                {
                    var eim = GetEventInstanceTrust();
                    var stgStatus = eim.getIntProperty("glpq5_room");
                    var jobNiche = getPlayer().getJob().GetJobNiche();

                    if ((stgStatus >> jobNiche) % 2 == 0)
                    {
                        if (canHold(4001259, 1))
                        {
                            await gainItem(4001259, 1);
                            await SayOK("干得好。");

                            stgStatus += (1 << jobNiche);
                            eim.setIntProperty("glpq5_room", stgStatus);
                        }
                        else
                        {
                            await SayOK("先在你的杂项物品栏腾出空间。");
                        }
                    }
                    else
                    {
                        await SayOK("这个房间里的武器已经被取走了。");
                    }
                }
                else
                {
                    await SayOK("消灭所有深红守护者。");
                }
            }
        }


        // Npc: 9201108 
        public async Task glpqstatue1()
        {
            if (getMapId() == 610030500)
            {
                await SayOK("一位被称为大师守护者的传奇生物在等待着你。它曾是一只深红守护者，里德利曾对其进行实验，导致它对魔法攻击、长矛、狼牙棒等几乎所有攻击都具有高度抵抗力，唯独不能抵挡威力异常强大的箭矢。弓箭手们！作为弓箭的无可争议的大师，你们必须使用最强大的攻击——从扫射到飓风到穿刺箭——来摧毁这个强大的生物，达到弓箭手雕像，夺取祖传弓！祝你好运！");
            }
            else if (getMapId() == 610030000)
            {
                await SayOK("Lockewood是为数不多的已知的神圣弓箭手之一，他是要塞中最著名的英雄之一。特别值得一提的是他定制的白色和金色战斗箭矢，据说是由一位强大的女神祝福过。他的瞄准能力在长距离上非常准确。因为他的创世箭和末日凤凰，他备受敬畏和尊重，曾经一次从英雄谷击倒了六只泰坦。");
            }
            else if (getMapId() == 610030540)
            {
                if (getMap().countMonsters() == 0)
                {
                    var eim = GetEventInstanceTrust();
                    var stgStatus = eim.getIntProperty("glpq5_room");
                    var jobNiche = getPlayer().getJob().GetJobNiche();

                    if ((stgStatus >> jobNiche) % 2 == 0)
                    {
                        if (canHold(4001258, 1))
                        {
                            await gainItem(4001258, 1);
                            await SayOK("干得好。");

                            stgStatus += (1 << jobNiche);
                            eim.setIntProperty("glpq5_room", stgStatus);
                        }
                        else
                        {
                            await SayOK("先在你的杂项物品栏腾出空间。");
                        }
                    }
                    else
                    {
                        await SayOK("这个房间里的武器已经被取走了。");
                    }
                }
                else
                {
                    await SayOK("消灭所有大师守护者。");
                }
            }
        }


        // Npc: 9201109 
        public async Task glpqstatue2()
        {
            if (getMapId() == 610030500)
            {
                await SayOK("作为一名强大的精英法师，里德利知道智慧的价值，这是巫师的标志品质。因此，法师之间是一个扭曲的阴谋迷宫——传送技能是你唯一能在里面使用的技能，魔法爪是唯一能打破雕像的技能。你还必须在其中杀死许多怪物。解决迷宫并打败其中所有的敌人后，推断出哪个法师雕像隐藏了第一魔法之杖，然后打破它来获取！祝你好运！");
            }
            else if (getMapId() == 610030000)
            {
                await SayOK("一个永远被铭记的名字，拉斐尔是一个非常有技巧的巫师，也是心灵魔法、念力和心灵感应的首席大师。除此之外，他还是掌握所有元素的精英法师之一。他最后被看到是在寻找元素神殿，以扭转克拉克军队的入侵潮流……");
            }
            else if (getMapId() == 610030521)
            {
                if (getMap().countMonsters() == 0)
                {
                    var eim = GetEventInstanceTrust();
                    var stgStatus = eim.getIntProperty("glpq5_room");
                    var jobNiche = getPlayer().getJob().GetJobNiche();

                    if ((stgStatus >> jobNiche) % 2 == 0)
                    {
                        if (canHold(4001257, 1))
                        {
                            await gainItem(4001257, 1);
                            await SayOK("干得好。");

                            stgStatus += (1 << jobNiche);
                            eim.setIntProperty("glpq5_room", stgStatus);
                        }
                        else
                        {
                            await SayOK("先在你的杂项物品栏腾出空间。");
                        }
                    }
                    else
                    {
                        await SayOK("这个房间里的武器已经被取走了。");
                    }
                }
                else
                {
                    await SayOK("消灭所有怪物。");
                }
            }
        }


        // Npc: 9201110 
        public async Task glpqstatue3()
        {
            switch (getMapId())
            {
                case 610030500:
                    await SayOK("作为每个盗贼都知道的，最好的攻击是你永远不会看到的那种。因此，为了最好地说明这一点，你将置身于一个只能通过加速术到达的平台和壁架的房间，还有你的匕首或爪子必须永久关闭的全视之眼。在所有全视之眼被消灭后，前往盗贼雕像并夺取原始之爪！祝你好运！");
                    break;
                case 610030000:
                    await SayOK("曾被誉为暗影王子的大师李龙拥有无与伦比的速度和力量，使用短距离匕首和长链爪。作为Bosshunters的兼职成员，他以无与伦比的能力融入夜晚本身而闻名。在与Crimson Balrog的战斗中，他的传奇故事逐渐传开，他移动如此迅速，以至于Balrog的攻击只能击中空气。李龙还为那些不如他自己幸运的人偶尔进行回收行动。");
                    break;
                case 610030530:
                    if (isAllReactorState(6108004, 1))
                    {
                        var eim = GetEventInstanceTrust();
                        var stgStatus = eim.getIntProperty("glpq5_room");
                        var jobNiche = getPlayer().getJob().GetJobNiche();

                        if ((stgStatus >> jobNiche) % 2 == 0)
                        {
                            if (canHold(4001256, 1))
                            {
                                await gainItem(4001256, 1);
                                await SayOK("干得好。");

                                stgStatus += (1 << jobNiche);
                                eim.setIntProperty("glpq5_room", stgStatus);
                            }
                            else
                            {
                                await SayOK("先在你的杂项物品栏腾出空间。");
                            }
                        }
                        else
                        {
                            await SayOK("这个房间里的武器已经被取走了。");
                        }
                    }
                    else
                    {
                        await SayOK("现在去吧，用你的移动技能摧毁所有警惕的眼睛，同伴盗贼。完成后向我报告。");
                    }
                    break;
            }
        }


        // Npc: 9201111 
        public async Task glpqstatue4()
        {
            if (getMapId() == 610030500)
            {
                await SayOK("你即将要淋湿并做海盗最擅长的事情——挖宝藏！小心——水下的这片水被称为重水，它非常密集，我怀疑你能否游过去！你必须绕道而行……你要寻找的遗物被称为禁忌之枪，这是曾经登上马斯泰利亚海岸的最优秀海盗——钢铁拳杰克的古老武器！它被埋在你在海底找到的众多宝箱之一中。这并不容易……海盗以在最不可能的地方埋藏东西而闻名，所以要深挖并保持警惕。那片水域中有鲨鱼和更糟糕的东西！");
            }
            else if (getMapId() == 610030000)
            {
                await SayOK("很久以前，一位奇怪的战士被冲上了玛斯特利亚的海岸。这个生物声称自己是一个神秘的战士团队的成员，他们使用类似爪子的武器和投射式火炮来打败敌人。被称为钢铁拳杰克，他在战斗中的狡诈和诡计非常有效。最终，他建造了一艘船，离开了要塞，去寻找他的前船员和船长。");
            }
        }


        // Npc: 9201112 
        public async Task glpqStory()
        {
            switch (getMapId())
            {
                case 610030500:
                    await SayNext("惊讶你能走到这一步！你在这里看到的是红木城堡的雕像，但是没有任何武器。");
                    await SayNext("有五个房间，每个房间附近都有一个雕像标记。");
                    await SayNext("我怀疑这些房间中每一个都有雕像的五把武器之一。");
                    await SayOK("把武器带回来，把它们恢复到掌握之遗物中！");
                    break;
                case 610030700:
                    await SayOK("那真是一次出色的表现！这条路通向扭曲大师的军械库。");
                    break;
            }
        }


        // Npc: 9201113 
        public Task glpqStart()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201114 
        public async Task glpqEnter()
        {
            if (haveItem(3992041, 1))
            {
                await warp(610030020, "out00");
            }
            else
            {
                await Pink("The giant gate of iron will not budge no matter what, however there is a visible key-shaped socket.");
            }
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
                await SayNext("我希望你们的队长和我谈谈。");
                return;
            }

            if (eim.getIntProperty("glpq6") == 0)
            {
                await SayNext("欢迎来到扭曲大师的堡垒。我将是今晚的主持人…");
                await SayNext("今晚，我们有一群冒险岛玩家的盛宴.. 哈哈哈...");
                await SayNext("让我们经过特别训练的守护大师护送你！");
                await mapMessage(6, "Engarde! Master Guardians approach!");
                for (var i = 0; i < 10; i++)
                {
                    var mob = LifeFactory.Instance.GetMonsterTrust(9400594);
                    var xPos = -1337 + Random.Shared.Next(1337);
                    await getMap().spawnMonsterOnGroundBelow(mob, new Point(xPos, 276));
                }
                for (var i = 0; i < 20; i++)
                {
                    var mob = LifeFactory.Instance.GetMonsterTrust(9400582);
                    var xPos = -1337 + Random.Shared.Next(1337);
                    await getMap().spawnMonsterOnGroundBelow(mob, new Point(xPos, 276));
                }
                eim.setIntProperty("glpq6", 1);
            }
            else if (eim.getIntProperty("glpq6") == 1)
            {
                if (getMap().countMonsters() == 0)
                {
                    await SayOK("嗯，这是什么？你打败了它们？");
                    await SayNext("好吧，无论如何！扭曲之主将很高兴欢迎你。");
                    await mapMessage(6, "Twisted Masters approach!");

                    //Margana
                    var mob = LifeFactory.Instance.GetMonsterTrust(9400590);
                    await getMap().spawnMonsterOnGroundBelow(mob, new Point(-22, 1));

                    //Red Nirg
                    var mob2 = LifeFactory.Instance.GetMonsterTrust(9400591);
                    await getMap().spawnMonsterOnGroundBelow(mob2, new Point(-22, 276));

                    //Hsalf
                    var mob4 = LifeFactory.Instance.GetMonsterTrust(9400593);
                    await getMap().spawnMonsterOnGroundBelow(mob4, new Point(496, 276));

                    //Rellik
                    var mob3 = LifeFactory.Instance.GetMonsterTrust(9400592);
                    await getMap().spawnMonsterOnGroundBelow(mob3, new Point(-496, 276));

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
                    await mapMessage(5, "The portal to the next stage has opened!");
                    eim.setIntProperty("glpq6", 3);

                    await eim.showClearEffect(true);
                    await eim.GiveStageClearRewardAll(6);

                    await eim.clearPQ();
                }
                else
                {
                    await SayOK("不要理会我。扭曲之主会护送你！");
                }
            }
        }
    }
}
