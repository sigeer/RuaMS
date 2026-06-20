using Application.Core.Client;
using Application.Core.Game.ContiMove;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;
using Application.Resources.Messages;
using Application.Shared.Constants.Job;
using Application.Shared.Constants.Map;
using Application.Shared.GameProps;
using Application.Templates.Mob;
using Application.Utility.Exceptions;
using scripting.portal;
using server.life;
using server.maps;
using System.Drawing;

namespace Application.Plugin.Script
{
    internal class PortalScript : PortalPlayerInteraction
    {
        public PortalScript(IChannelClient c, Portal p) : base(c, p)
        {
        }

        [ScriptName("08_xmas_out")]
        public async Task<bool> _08_xmas_out()
        {

            await playPortalSound();
            await warp(getMapId() - 2, 0);
            return true;
        }


        public async Task<bool> advice00()
        {

            await showInstruction("您可以使用箭头键移动。", 250, 5);
            return true;
        }


        public async Task<bool> advice01()
        {

            await showInstruction("单击 \r\\#b<Heena>#k", 100, 5);
            return true;
        }


        public async Task<bool> advice02()
        {

            await showInstruction("按下 #e#b[Alt]#k#n to\r\\ 跳跃。", 100, 5);
            return true;
        }


        public async Task<bool> advice03()
        {

            await showInstruction("按方向键 #e#b[Up]#k#n 爬上梯子或绳索", 350, 5);
            return true;
        }


        public async Task<bool> advice04()
        {

            await showInstruction("单击 \r\\#b<Sera>", 100, 5);
            return true;
        }


        public async Task<bool> advice05()
        {

            await showInstruction("按 #e#b[Q]#k#n 打开任务窗口。", 250, 5);
            return true;
        }


        public async Task<bool> advice06()
        {

            await showInstruction("按 #e#b[Up]#k 进入光圈\r\\移动到下一张地图。", 230, 5);
            return true;
        }


        public async Task<bool> advice07()
        {

            await showInstruction("您可以按#e#b[W]#k#n键查看世界地图。", 350, 5);
            return true;
        }


        public async Task<bool> advice08()
        {

            await showInstruction("您可以通过按#e#b[S]#k#n键来查看角色的属性面板。", 350, 5);
            return true;
        }


        public async Task<bool> advice09()
        {

            await showInstruction("同时按下方向键的#n#e#b[Alt]#k和#e#b[Down]#k#n向下跳跃。", 450, 6);
            return true;
        }


        public async Task<bool> adviceMap()
        {

            await showInstruction("按#e#b[Up]#k箭头#n进入光圈移动到下一个地图。", 230, 5);
            return true;
        }


        public async Task<bool> aMatchMove2()
        {

            await playPortalSound();
            await warp(getPlayer().getSavedLocation("MIRROR"));
            return true;
        }


        public async Task<bool> apq00()
        {

            await playPortalSound();
            await warp(670010300, 0);
            return true;
        }


        public async Task<bool> apq01()
        {

            await playPortalSound();
            await warp(670010301, 0);
            return true;
        }


        public async Task<bool> apq02()
        {

            await playPortalSound();
            await warp(670010302, 0);
            return true;
        }


        public async Task<bool> apq1()
        {

            await playPortalSound();
            await warp(670010400, 0);
            return true;
        }


        public async Task<bool> apq2()
        {

            await playPortalSound();
            await warp(670010500, 0);
            return true;
        }


        public async Task<bool> apq3()
        {

            await playPortalSound();
            await warp(670010600, 0);
            return true;
        }


        public async Task<bool> apqClosed()
        {

            await Pink("大门还没有打开。");
            return false;
        }


        public async Task<bool> apqDoor()
        {

            var name = getPortal().getName().Substring(2, 4);
            var gate = getPlayer().getMap().getReactorByName("gate" + name);
            if (gate != null && gate.getState() == 4)
            {
                await playPortalSound();
                await warp(670010600, "gt" + name + "PIB");
                return true;
            }
            else
            {
                await Pink("大门还没有打开。");
                return false;
            }
        }


        public async Task<bool> aqua_pq_boss_0()
        {

            await playPortalSound();
            await warp(230040420, 0);
            return true;
        }


        public async Task<bool> aranTutorAloneX()
        {

            await playPortalSound();
            await warp(914000100, 1);
            return true;
        }


        public async Task<bool> aranTutorArrow0()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "arr0=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "arr0=o;mo1=o;mo2=o;mo3=o");
            await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow3");
            return true;
        }


        public async Task<bool> aranTutorArrow1()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "arr1=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "normal=o;arr0=o;arr1=o;mo1=o;mo2=o;mo3=o;mo4=o");
            await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow1");
            return true;
        }


        public async Task<bool> aranTutorArrow2()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "arr2=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "normal=o;arr0=o;arr1=o;arr2=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow1");
            return true;
        }


        public async Task<bool> aranTutorArrow3()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "arr3=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "cmd=o;normal=o;arr0=o;arr1=o;arr2=o;arr3=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow1");
            return true;
        }


        public async Task<bool> aranTutorGuide0()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "normal=o"))
            {
                return false;
            }
            await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialGuide1");
            await Pink("按一下Ctrl键，能够对怪兽进行一般攻击。");
            await updateAreaInfo(21002, "normal=o;arr0=o;mo1=o;mo2=o;mo3=o");
            return true;
        }


        public async Task<bool> aranTutorGuide1()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "chain=o"))
            {
                return false;
            }
            await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialGuide2");
            await Pink("按住Ctrl键，能够进行连续攻击。");
            await updateAreaInfo(21002, "normal=o;arr0=o;arr1=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            return true;
        }


        public async Task<bool> aranTutorGuide2()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "cmd=o"))
            {
                return false;
            }
            await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialGuide3");
            await Pink("连续攻击后，通过方向键和攻击键可以实现命令攻击。");
            await updateAreaInfo(21002, "cmd=o;normal=o;arr0=o;arr1=o;arr2=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            return true;
        }


        public async Task<bool> aranTutorLost()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "fin=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "cmd=o;normal=o;arr0=o;arr1=o;arr2=o;arr3=o;fin=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            await showIntro("Effect/Direction1.img/aranTutorial/ClickChild");
            return true;
        }


        public async Task<bool> aranTutorMono0()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "mo1=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "mo1=o");
            await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/legendBalloon1");
            return true;
        }


        public async Task<bool> aranTutorMono1()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "mo2=o"))
            {
                return false;
            }
            await playSound("Aran/balloon");
            await updateAreaInfo(21002, "mo1=o;mo2=o");
            await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/legendBalloon2");
            return true;
        }


        public async Task<bool> aranTutorMono2()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "mo3=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "mo1=o;mo2=o;mo3=o");
            await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/legendBalloon3");
            return true;
        }


        public async Task<bool> aranTutorMono3()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "mo4=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "normal=o;arr0=o;mo1=o;mo2=o;mo3=o;mo4=o");
            await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/legendBalloon6");
            return true;
        }


        public async Task<bool> aranTutorOut1()
        {

            if (isQuestStarted(21000))
            {
                //lol nexon does this xD
                await teachSkill(20000017, 0, -1, -1);
                await teachSkill(20000018, 0, -1, -1);
                //nexon sends updatePlayerStats Stat.AVAILABLESP 0
                await teachSkill(20000017, 1, 0, -1);
                await teachSkill(20000018, 1, 0, -1);
                //actually nexon does enableActions here :P
                await playPortalSound();
                await warp(914000200, 1);
                return true;
            }
            else
            {
                await Pink("你只有在接受你右边的雅典娜·皮尔斯的任务后才能退出。");
                return false;
            }
        }


        public async Task<bool> aranTutorOut2()
        {

            //lol nexon does this xD
            await teachSkill(20000014, 0, -1, -1);
            await teachSkill(20000015, 0, -1, -1);
            //nexon sends updatePlayerStats Stat.AVAILABLESP 0
            await teachSkill(20000014, 1, 0, -1);
            await teachSkill(20000015, 1, 0, -1);
            //actually nexon does enableActions here :P
            await playPortalSound();
            await warp(914000210, 1);
            return true;
        }


        public async Task<bool> aranTutorOut3()
        {

            //lol nexon does this xD
            await teachSkill(20000016, 0, -1, -1);
            //nexon sends updatePlayerStats Stat.AVAILABLESP 0
            await teachSkill(20000016, 1, 0, -1);
            //actually nexon does enableActions here :P
            await playPortalSound();
            await warp(914000220, 1);
            return true;
        }


        public async Task<bool> ariantMout()
        {

            await playPortalSound();
            await warp(980010020, 0);
            return true;
        }


        public async Task<bool> ariantMout2()
        {

            await playPortalSound();
            await warp(980010000, 0);
            return true;
        }


        public async Task<bool> ariant_Agit()
        {

            if (isQuestCompleted(3928) && isQuestCompleted(3931) && isQuestCompleted(3934))
            {
                await playPortalSound();
                await warp(260000201, 1);
                return true;
            }
            else
            {
                await Pink("仅限沙匪队成员进入。");
                return false;
            }
        }


        public async Task<bool> ariant_castle()
        {

            if (getPlayer().haveItem(4031582) == true)
            {
                await playPortalSound();
                await warp(260000301, 5);
                return true;
            }
            else
            {
                await Pink("只有持有宫殿通行证才能进入。");
                return false;
            }
        }


        static bool isTigunMorphed(Player chr)
        {
            return chr.getBuffSource(BuffStat.MORPH) == 2210005;
        }
        public async Task<bool> ariant_queens()
        {

            if (isTigunMorphed(getPlayer()))
            {
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(260000300, 7);
                await Pink("你，入侵者！你没有权限在宫殿中随意走动！滚出去！！");
                return true;
            }
        }


        public async Task<bool> babyPigOut()
        {

            if (isQuestCompleted(22015))
            {
                await playPortalSound();
                await warp(100030300, 2);
            }
            else
            {
                await Pink("请救救小猪！");//not gms like
            }
            return true;
        }


        public async Task<bool> balogTemple()
        {

            await playPortalSound();
            await warp(105100000, 2);
            return true;
        }


        public async Task<bool> balog_end()
        {
            if (!canHold(4001261, 1))
            {
                await Pink("请给装备栏腾出至少1个空格子。");
                return false;
            }
            await gainItem(4001261, 1);
            await playPortalSound();
            await warp(105100100, 0);
            return true;
        }


        public async Task<bool> bedroom_out()
        {

            if (isQuestStarted(2570))
            {
                await playPortalSound();
                await warp(120000101, 0);
                return true;
            }
            await EarnTitle("你似乎还有一些未完成的事情，我从你的眼神中可以看出来。等等……不，那些只是眼屎。");
            return false;
        }

        public async Task<bool> captinsg00()
        {

            if (!haveItem(4000381))
            {
                await Pink("You do not have White Essence.");
                return false;
            }
            else
            {
                var em = getEventManager("LatanicaBattle") as PartyQuestEventManager;
                if (em == null)
                {
                    throw new BusinessNotsupportException($"Event: LatanicaBattle");
                }

                var r = await em.StartInstance(getPlayer());
                switch (r)
                {
                    case CreateInstanceResult.Success:
                        await playPortalSound();
                        return true;
                    case CreateInstanceResult.RequiredParty:
                        await Pink("You are currently not in a party, create one to attempt the boss.");
                        return false;
                    case CreateInstanceResult.RequiredLeader:
                        await Pink("Your party leader must enter the portal to start the battle.");
                        return false;
                    case CreateInstanceResult.Requirement:
                        await Pink("Your party must consist of at least 2 players to attempt the boss.");
                        return false;
                    case CreateInstanceResult.LobbyLimited:
                        await Pink("The battle against the boss has already begun, so you may not enter this place yet.");
                        return false;
                    default:
                        return false;
                }
            }
        }


        public async Task<bool> catPriest_map()
        {
            await playPortalSound();
            await warp(925000000, 2);
            return true;
        }


        public async Task<bool> contactDragon()
        {

            await playPortalSound();
            await warp(900090100, 0);
            return true;
        }


        public async Task<bool> curseforest()
        {

            if (isQuestStarted(2224) || isQuestStarted(2226) || isQuestCompleted(2227))
            {
                var hourDay = getHourOfDay();
                if (!((hourDay >= 0 && hourDay < 7) || hourDay >= 17))
                {
                    await Pink("You cannot access this area right now.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    await warp(isQuestCompleted(2227) ? 910100001 : 910100000, "out00");
                    return true;
                }
            }

            await Pink("You cannot access this area.");
            return false;
        }


        public async Task<bool> davy2_hd1()
        {

            var eim = GetEventInstanceTrust();
            var level = eim.getIntProperty("level");
            if (eim.getProperty("stage2b") == "0")
            {
                await (await getMap(925100202)).spawnAllMonstersFromMapSpawnList(level, true);
                eim.setProperty("stage2b", "1");
            }

            await playPortalSound();
            await warp(925100202, 0);
            return true;
        }


        public async Task<bool> davy3_hd1()
        {

            var eim = GetEventInstanceTrust();
            var level = eim.getIntProperty("level");
            if (eim.getProperty("stage3b") == "0")
            {
                await (await getMap(925100302)).spawnAllMonstersFromMapSpawnList(level, true);
                eim.setProperty("stage3b", "1");
            }

            await playPortalSound();
            await warp(925100302, 0);
            return true;
        }


        public async Task<bool> davy_next0()
        {

            var eim = GetEventInstanceTrust();
            if (getMap().countMonsters() == 0 && passedGrindMode(getMap(), eim))
            {
                await playPortalSound();
                await warp(925100100, 0); //next
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }

        static bool passedGrindMode(IMap map, AbstractEventInstanceManager eim)
        {
            if (eim.getIntProperty("grindMode") == 0)
            {
                return true;
            }
            return eim.activatedAllReactorsOnMap(map, 2511000, 2517999);
        }

        public async Task<bool> davy_next1()
        {

            try
            {
                var eim = GetEventInstanceTrust();
                if (eim != null && eim.getProperty("stage2") == "3")
                {
                    await playPortalSound();
                    await warp(925100200, 0); //next
                    return true;
                }
                else
                {
                    await Pink("The portal is not opened yet.");
                    return false;
                }
            }
            catch (Exception e)
            {
                await Pink("Error: " + e.Message);
            }

            return false;
        }


        public async Task<bool> davy_next2()
        {

            if (getMap().countMonsters() == 0 && passedGrindMode(getMap(), GetEventInstanceTrust()))
            {
                await playPortalSound();
                await warp(925100300, 0); //next
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> davy_next3()
        {

            if (getMap().countMonsters() == 0 && passedGrindMode(getMap(), GetEventInstanceTrust()))
            {
                await playPortalSound();
                await warp(925100400, 0); //next
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> davy_next4()
        {

            if (getMap().getReactorByName("sMob1")?.getState() >= 1
                && getMap().getReactorByName("sMob2")?.getState() >= 1
                && getMap().getReactorByName("sMob3")?.getState() >= 1
                && getMap().getReactorByName("sMob4")?.getState() >= 1
                && getMap().countMonsters() == 0)
            {
                var eim = GetEventInstanceTrust();

                if (eim.getProperty("spawnedBoss") == null)
                {
                    var level = int.Parse(eim.getProperty("level"));
                    var chests = int.Parse(eim.getProperty("openedChests"));
                    MobTemplate boss;
                    if (chests == 0)
                    {
                        boss = LifeFactory.Instance.GetMonsterTrust(9300119);
                    }//lord pirate
                    else if (chests == 1)
                    {
                        boss = LifeFactory.Instance.GetMonsterTrust(9300105);
                    }//angry lord pirate
                    else
                    {
                        boss = LifeFactory.Instance.GetMonsterTrust(9300106);
                    }                   //enraged lord pirate

                    await (await getMap(925100500)).spawnMonsterOnGroundBelow(boss, new Point(777, 140), mob =>
                    {
                        mob.changeDifficulty(level, true);
                    });
                    eim.setProperty("spawnedBoss", "true");
                }

                await playPortalSound();
                await warp(925100500, 0);
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> Depart_goBack00()
        {

            await playPortalSound();
            await warp(getPlayer().getMap().getId() - 10, "left00");
            return true;
        }


        public async Task<bool> Depart_goBack01()
        {

            await playPortalSound();
            await warp(getPlayer().getMap().getId() - 10, "left01");
            return true;
        }


        public async Task<bool> Depart_goFoward0()
        {

            var mapid = getPlayer().getMap().getId();

            if (mapid == 103040410 && isQuestCompleted(2287))
            {
                await playPortalSound();
                await warp(103040420, "right00");
                return true;
            }
            else if (mapid == 103040420 && isQuestCompleted(2288))
            {
                await playPortalSound();
                await warp(103040430, "right00");
                return true;
            }
            else if (mapid == 103040410 && isQuestStarted(2287))
            {
                await playPortalSound();
                await warp(103040420, "right00");
                return true;
            }
            else if (mapid == 103040420 && isQuestStarted(2288))
            {
                await playPortalSound();
                await warp(103040430, "right00");
                return true;
            }
            else
            {
                if (mapid == 103040440 || mapid == 103040450)
                {
                    await playPortalSound();
                    await warp(mapid + 10, "right00");
                    return true;
                }
                await Pink("You cannot access this area.");
                return false;
            }
        }


        public async Task<bool> Depart_goFoward1()
        {

            var mapid = getPlayer().getMap().getId();

            if (mapid == 103040410 && isQuestCompleted(2287))
            {
                await playPortalSound();
                await warp(103040420, "right01");
                return true;
            }
            else if (mapid == 103040420 && isQuestCompleted(2288))
            {
                await playPortalSound();
                await warp(103040430, "right01");
                return true;
            }
            else if (mapid == 103040410 && isQuestStarted(2287))
            {
                await playPortalSound();
                await warp(103040420, "right01");
                return true;
            }
            else if (mapid == 103040420 && isQuestStarted(2288))
            {
                await playPortalSound();
                await warp(103040430, "right01");
                return true;
            }
            else
            {
                if (mapid == 103040440 || mapid == 103040450)
                {
                    await playPortalSound();
                    await warp(mapid + 10, "right01");
                    return true;
                }
                await Pink("You cannot access this area.");
                return false;
            }
        }


        public async Task<bool> Depart_ToKerning()
        {
            var em = GetEventManager("KerningTrain");
            if (await em.StartInstance(getPlayer()) != CreateInstanceResult.Success)
            {
                await Pink("The passenger wagon is already full. Try again a bit later.");
                return false;
            }

            await playPortalSound();
            return true;
        }


        public async Task<bool> Depart_TopFloor()
        {

            await openNpc(1052125); //It is actually suppose to open the npc, because it leads to a boss map
            return true;
        }


        public async Task<bool> Depart_topOut()
        {

            await playPortalSound();
            await warp(103040300, 1);
            return true;
        }


        public async Task<bool> dojang_exit()
        {

            var map = getPlayer().getSavedLocation("MIRROR");
            if (map == -1)
            {
                map = 100000000;
            }

            await playPortalSound();
            await warp(map);
            return true;
        }


        public async Task<bool> dojang_next()
        {

            var currwarp = c.CurrentServer.Node.getCurrentTime();

            if (currwarp - getPlayer().getNpcCooldown() < 3000)
            {
                return false;
            } // this script can be ran twice when passing the dojo portal... strange.
            getPlayer().setNpcCooldown(currwarp);

            var gate = getPlayer().getMap().getReactorByName("door");
            if (gate != null)
            {
                if (gate.getState() == 1 || getMap().countMonsters() == 0)
                {
                    if (Math.Floor(getPlayer().getMapId() / 100.0) % 100 < 38)
                    {
                        if (((Math.Floor((getPlayer().getMap().getId() + 100) / 100.0)) % 100) % 6 == 0)
                        {
                            if (Math.Floor(getPlayer().getMapId() / 10000.0) == 92503)
                            {
                                var restMapId = getPlayer().getMap().getId() + 100;
                                var mapId = getPlayer().getMap().getId();

                                for (var i = 0; i < 5; i++)
                                {
                                    var chrlist = (await getMap(mapId - 100 * i)).getAllPlayers();

                                    foreach (var chr in chrlist)
                                    {
                                        for (var j = i; j >= 0; j--)
                                        {
                                            await chr.Pink("You received " + chr.addDojoPointsByMap(mapId - 100 * j) + " training points. Your total training points score is now " + chr.getDojoPoints() + ".");
                                        }

                                        await chr.changeMap(restMapId, 0);
                                    }
                                }
                            }
                            else
                            {
                                await Pink("You received " + getPlayer().addDojoPointsByMap(getMapId()) + " training points. Your total training points score is now " + getPlayer().getDojoPoints() + ".");
                                await playPortalSound();
                                await warp(getPlayer().getMap().getId() + 100, 0);
                            }
                        }
                        else
                        {
                            await Pink("You received " + getPlayer().addDojoPointsByMap(getMapId()) + " training points. Your total training points score is now " + getPlayer().getDojoPoints() + ".");
                            await playPortalSound();
                            await warp(getPlayer().getMap().getId() + 100, 0);
                        }
                    }
                    else
                    {
                        await playPortalSound();
                        await warp(925020003, 0);
                        await getPlayer().gainExp(2000 * getPlayer().getDojoPoints(), true, true, true);
                    }
                    return true;
                }
                else
                {
                    await Pink("The door is not open yet.");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        public async Task<bool> dojang_tuto()
        {

            if (getPlayer().getMap().getMonsterById(9300216) != null)
            {
                getPlayer().FinishedDojoTutorial = true;
                getClient().getChannelServer().resetDojo(getPlayer().getMap().getId());
                getClient().getChannelServer().dismissDojoSchedule(getPlayer().getMap().getId(), getParty());
                await playPortalSound();
                await warp(925020001, 0);
                return true;
            }
            else
            {
                await Pink("So Gong: Haha! You're going to run away like a coward? I won't let you get away that easily!");
                return false;
            }
        }


        public async Task<bool> dojang_up()
        {

            try
            {
                if (getPlayer().getMap().getMonsterById(9300216) != null)
                {
                    await goDojoUp();
                    await getPlayer().getMap().setReactorState();
                    var stage = (int)Math.Floor(getPlayer().getMapId() / 100.0) % 100;
                    if ((stage - (stage / 6) | 0) == getPlayer().getVanquisherStage() && !MapId.isPartyDojo(getPlayer().getMapId())) // we can also try 5 * stage / 6 | 0 + 1
                    {
                        getPlayer().setVanquisherKills(getPlayer().getVanquisherKills() + 1);
                    }
                }
                else
                {
                    await Pink("There are still some monsters remaining.");
                }
                await enableActions();
                return true;
            }
            catch (Exception err)
            {
                await Notice(err.Message);
                return false;
            }
        }


        public async Task<bool> dracoout()
        {

            await playPortalSound();
            await warp(240000100, "east00");
            return true;
        }



        public async Task<bool> dragoneyes()
        {

            if (isQuestCompleted(22012))
            {
                return false;
            }
            else
            {
                await forceCompleteQuest(22012);
            }
            await blockPortal();
            return true;
        }


        public async Task<bool> dragonNest()
        {

            if (isQuestCompleted(3706))
            {
                await playPortalSound();
                await warp(240040612, "out00");
                return true;
            }
            else if (isQuestStarted(100203) || getPlayer().haveItem(4001094))
            {
                var em = GetEventManager<SoloEventManager>("NineSpirit");
                if (await em.StartInstance(getPlayer()) != Core.scripting.Events.Abstraction.CreateInstanceResult.Success)
                {
                    await Pink("There is currently someone in this map, come back later.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    return true;
                }
            }
            else
            {
                await Pink("A strange force is blocking you from entering.");
                return false;
            }
        }

        public async Task<bool> elevator()
        {
            var elevator = GetContiMove();
            if (elevator is not Elevator)
            {
                await Pink("电梯正在维护。");
                return false;
            }

            if (await elevator.Enter(getPlayer()))
            {
                await playPortalSound();
                return true;
            }

            await Pink("电梯已经启动。");
            return false;
        }

        public async Task<bool> eliza_Garden()
        {

            await playPortalSound();
            await warp(920020000, 2);
            return true;
        }


        public async Task<bool> end_black()
        {

            await playPortalSound();
            await warp(120000200, 0);
            return true;
        }


        public async Task<bool> end_cow()
        {

            if (isQuestStarted(2180) && (hasItem(4031847) || hasItem(4031848) || hasItem(4031849) || hasItem(4031850)))
            {
                if (hasItem(4031850))
                {
                    await playPortalSound();
                    await warp(120000103);
                    return true;
                }
                else
                {
                    await Pink("Your milk jug is not full...");
                    return false;
                }
            }
            else
            {
                await playPortalSound();
                await warp(120000103);
                return true;
            }
        }


        public async Task<bool> enterAchter()
        {

            await playPortalSound();
            await warp(100000201, "out02");
            await playPortalSound();
            return true;
        }


        public async Task<bool> enterBackStreet()
        {

            if (isQuestActive(21747) || isQuestActive(21744) && isQuestCompleted(21745))
            {
                await playPortalSound();
                await warp(925040000, 0);
                return true;
            }
            else
            {
                await Pink("You don't have permission to access this area.");
                return false;
            }
        }


        public async Task<bool> enterDisguise0()
        {

            if (isQuestStarted(20301) || isQuestStarted(20302) || isQuestStarted(20303) || isQuestStarted(20304) || isQuestStarted(20305))
            {
                if (hasItem(4032179))
                {
                    await playPortalSound();
                    await warp(130010000, "east00");
                }
                else
                {
                    await Pink("Due to the lock down you can not enter without a permit.");
                    return false;
                }
            }
            else
            {
                await playPortalSound();
                await warp(130010000, "east00");
            }
            return true;
        }


        public async Task<bool> enterDisguise1()
        {
            var jobtype = 0;
            if (isQuestStarted(20301) || isQuestStarted(20302) || isQuestStarted(20303) || isQuestStarted(20304) || isQuestStarted(20305))
            {
                var map = await getClient().getChannelServer().getMapFactory().getMap(108010600 + (10 * jobtype));
                if (map.countPlayers() > 0)
                {
                    await Pink("Someone else is already searching the area.");
                    return false;
                }

                if (haveItem(4032101 + jobtype, 1))
                {
                    await Pink("You have already challenged the Master of Disguise, report your success to the Chief Knight.");
                    return false;
                }

                await playPortalSound();
                await warp(map.Id, "out00");
            }
            else
            {
                await playPortalSound();
                await warp(130010010, "out00");
            }
            return true;
        }


        public async Task<bool> enterDisguise2()
        {

            var jobtype = 0;
            if (isQuestStarted(20301) || isQuestStarted(20302) || isQuestStarted(20303) || isQuestStarted(20304) || isQuestStarted(20305))
            {
                var map = await getClient().getChannelServer().getMapFactory().getMap(108010600 + (10 * jobtype));
                if (map.countPlayers() > 0)
                {
                    await Pink("Someone else is already searching the area.");
                    return false;
                }

                if (haveItem(4032101 + jobtype, 1))
                {
                    await Pink("You have already challenged the Master of Disguise, report your success to the Chief Knight.");
                    return false;
                }

                await playPortalSound();
                await warp(map.Id, "out00");
            }
            else
            {
                await playPortalSound();
                await warp(130010020, "out00");
            }
            return true;
        }


        public async Task<bool> enterDisguise3()
        {

            var jobtype = 0;
            if (isQuestStarted(20301) || isQuestStarted(20302) || isQuestStarted(20303) || isQuestStarted(20304) || isQuestStarted(20305))
            {
                var map = await getClient().getChannelServer().getMapFactory().getMap(108010600 + (10 * jobtype));
                if (map.countPlayers() > 0)
                {
                    await Pink("Someone else is already searching the area.");
                    return false;
                }

                if (haveItem(4032101 + jobtype, 1))
                {
                    await Pink("You have already challenged the Master of Disguise, report your success to the Chief Knight.");
                    return false;
                }

                await playPortalSound();
                await warp(map.Id, "out00");
            }
            else
            {
                await playPortalSound();
                await warp(130010110, "out00");
            }
            return true;
        }


        public async Task<bool> enterDisguise4()
        {

            var jobtype = 0;
            if (isQuestStarted(20301) || isQuestStarted(20302) || isQuestStarted(20303) || isQuestStarted(20304) || isQuestStarted(20305))
            {
                var map = await getClient().getChannelServer().getMapFactory().getMap(108010600 + (10 * jobtype));
                if (map.countPlayers() > 0)
                {
                    await Pink("Someone else is already searching the area.");
                    return false;
                }

                if (haveItem(4032101 + jobtype, 1))
                {
                    await Pink("You have already challenged the Master of Disguise, report your success to the Chief Knight.");
                    return false;
                }

                await playPortalSound();
                await warp(map.Id, "out00");
            }
            else
            {
                await playPortalSound();
                await warp(130010120, "out00");
            }
            return true;
        }


        public async Task<bool> enterDisguise5()
        {

            var jobtype = 0;
            if (isQuestStarted(20301) || isQuestStarted(20302) || isQuestStarted(20303) || isQuestStarted(20304) || isQuestStarted(20305))
            {
                var map = await getClient().getChannelServer().getMapFactory().getMap(108010600 + (10 * jobtype));
                if (map.countPlayers() > 0)
                {
                    await Pink("Someone else is already searching the area.");
                    return false;
                }

                if (haveItem(4032101 + jobtype, 1))
                {
                    await Pink("You have already challenged the Master of Disguise, report your success to the Chief Knight.");
                    return false;
                }

                await playPortalSound();
                await warp(map.Id, "east00");
            }
            else
            {
                await playPortalSound();
                await warp(130020000, "east00");
            }
            return true;
        }

        // Map: 105070300
        public async Task<bool> enterDollcave()
        {

            if (isQuestCompleted(20730) || isQuestCompleted(21734))
            {
                // puppeteer defeated, newfound secret path
                await playPortalSound();
                await warp(105040201, 2);
                return true;
            }

            await openNpc(1063011);
            return false;
        }


        public async Task<bool> enterDollWay()
        {

            if (isQuestCompleted(20730) || isQuestCompleted(21734))
            {
                // puppeteer defeated, newfound secret path
                await playPortalSound();
                await warp(105070300, 3);
                return true;
            }
            else if (isQuestStarted(21734))
            {
                await playPortalSound();
                await warp(910510100, 0);
                return true;
            }
            else
            {
                await Pink("一股不祥的力量阻止你通过此处。");
                return false;
            }
        }


        public async Task<bool> enterEvanRoom()
        {

            await playPortalSound();
            await warp(100030100, 0);
            return true;
        }


        public async Task<bool> enterFirstDH()
        {
            var map = 0;

            if (getQuestStatus(20701) == 1)
            {
                map = 913000000;
            }
            else if (getQuestStatus(20702) == 1)
            {
                map = 913000100;
            }
            else if (getQuestStatus(20703) == 1)
            {
                map = 913000200;
            }
            if (map > 0)
            {
                var mapp = await getMap(map);
                if (mapp.getAllPlayers().Count == 0)
                {
                    await mapp.resetPQ();

                    await playPortalSound();
                    await warp(map, 0);
                    return true;
                }
                else
                {
                    await Pink("已经有其他人比你先使用了演武场，请稍后再试！");
                    return false;
                }
            }
            else
            {
                await Pink("只有参加Kiku的适应训练，才能进入1号演武场。");
                return false;
            }
        }


        public async Task<bool> enterfourthDH()
        {

            if (hasItem(4032125) || hasItem(4032126) || hasItem(4032127) || hasItem(4032128) || hasItem(4032129))
            {
                await Pink("你已经有了能力的证物");
                return false;
            }

            if (isQuestStarted(20611) || isQuestStarted(20612) || isQuestStarted(20613) || isQuestStarted(20614) || isQuestStarted(20615))
            {
                var map = await getMap(913020300);
                if (map.getAllPlayers().Count == 0)
                {
                    await map.killAllMonsters();

                    await playPortalSound();
                    await warp(913020300, 0);
                    await spawnMonster(9300294, 87, 88);
                    return true;
                }
                else
                {
                    await Pink("已经有人在尝试击败Boss，请你稍后再来。");
                    return false;
                }
            }
            else
            {
                await Pink("只有参加骑士指定的等级考试方可进入");
                return false;
            }
        }


        public async Task<bool> enterGym()
        {

            if (isQuestStarted(21701))
            {
                await playPortalSound();
                await warp(914010000, 1);
                return true;
            }
            else if (isQuestStarted(21702))
            {
                await playPortalSound();
                await warp(914010100, 1);
                return true;
            }
            else if (isQuestStarted(21703))
            {
                await playPortalSound();
                await warp(914010200, 1);
                return true;
            }
            else
            {
                await Pink("You will be allowed to enter the Penguin Training Ground only if you are receiving a lesson from Puo.");
                return false;
            }
        }

        // Map: 910400000
        public async Task<bool> enterInfo()
        {
            if (isQuestStarted(21733) && getQuestProgressInt(21733, 21762) != 2)
            {
                var em = GetSoloQuestEventManager(21733);
                var r = await em.StartInstance(getPlayer());
                if (r == CreateInstanceResult.Success)
                {
                    return true;
                }
                else
                {
                    await Pink("目标地图有人，请稍后再尝试进入。");
                    return false;
                }
            }
            else
            {
                await playPortalSound();
                await warp(104000004);
                return true;
            }
        }





        public async Task<bool> enterMCave()
        {
            if (isQuestStarted(21201))
            {
                // Second Job
                for (var i = 108000700; i < 108000709; i++)
                {
                    if (await getPlayerCount(i) > 0 && await getPlayerCount(i + 10) > 0)
                    {
                        continue;
                    }

                    await playPortalSound();
                    await warp(i, "out00");
                    await setQuestProgress(21202, 21203, 0);
                    return true;
                }
                await Pink("The mirror is blank due to many players recalling their memories. Please wait and try again.");
                return false;
            }
            else if (isQuestStarted(21302) && !isQuestCompleted(21303))
            {
                // Third Job
                if (await getPlayerCount(108010701) > 0 || await getPlayerCount(108010702) > 0)
                {
                    await Pink("The mirror is blank due to many players recalling their memories. Please wait and try again.");
                    return false;
                }
                else
                {
                    var map = await getWarpMap(108010702);
                    await map.spawnMonsterOnGroundBelow(9001013, -210, 454);

                    await playPortalSound();
                    await setQuestProgress(21303, 21203, 1);
                    await warp(108010701, "out00");
                    return true;
                }
            }
            else
            {
                await Pink("你已经通过了测试，不需要再来了。");
                return false;
            }
        }

        // Map 200060000
        public async Task<bool> enterNepenthes()
        {
            if (isQuestActive(21739))
            {
                var em = GetSoloQuestEventManager(21739);
                var r = await em.StartInstance(getPlayer());
                if (r != CreateInstanceResult.Success)
                {
                    await Pink(em.HandleCreateInstanceResult(r, c) ?? "");
                    return false;
                }
                return true;
            }
            else
            {
                await playPortalSound();
                await warp(200060001, 2);
                return true;
            }
        }


        public async Task<bool> enterPort()
        {
            if (isQuestStarted(21301) && getQuestProgressInt(21301, 9001013) == 0)
            {
                var em = GetSoloQuestEventManager(21301);
                var r = await em.StartInstance(getPlayer());
                if (r != CreateInstanceResult.Success)
                {
                    await Pink("门从另一边被堵住了。有人已经和小偷乌鸦打起来了？");
                    return false;
                }

                await playPortalSound();
                return true;
            }
            else
            {
                await playPortalSound();
                await warp(140020300, 1);
            }
            return true;
        }


        public async Task<bool> enterRider()
        {
            if (isQuestStarted(21610) && !haveItem(4001193, 1))
            {
                var em = GetSoloQuestEventManager(21610);
                if (await em.StartInstance(getPlayer()) != CreateInstanceResult.Success)
                {
                    await Pink("There is currently someone in this map, come back later.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    return true;
                }
            }
            else
            {
                await Pink("领取了任务《斯卡德的建议》才可以进入这里");
                return false;
            }
        }


        public async Task<bool> enterRienFirst()
        {

            if (getPlayer().getJob().getId() == 2000 && !isQuestCompleted(21014))
            {
                await playPortalSound();
                await warp(140000000, "st00");
            }
            else
            {
                await playPortalSound();
                await warp(140000000, "west00");
            }

            return true;
        }


        public async Task<bool> enterSecondDH()
        {

            int[] maps = [108000600, 108000601, 108000602];
            if (isQuestStarted(20201) || isQuestStarted(20202) || isQuestStarted(20203) || isQuestStarted(20204) || isQuestStarted(20205))
            {
                await removeAll(4032096);
                await removeAll(4032097);
                await removeAll(4032098);
                await removeAll(4032099);
                await removeAll(4032100);

                var rand = Random.Shared.Next(maps.Length);
                await playPortalSound();
                await warp(maps[rand], 0);
                await playerMessage(0, "重新进入第2演武场时将会清空背包里所有考试的证物，请务必注意。");
                return true;
            }
            else
            {
                await Pink("只有参加骑士指定的等级考试方可进入第2演武场。");
                return false;
            }
        }


        public async Task<bool> enterthirdDH()
        {

            if (hasItem(4032120) || hasItem(4032121) || hasItem(4032122) || hasItem(4032123) || hasItem(4032124))
            {
                await Pink("你已经有了资格的证物。");
                return false;
            }
            if (isQuestStarted(20601) || isQuestStarted(20602) || isQuestStarted(20603) || isQuestStarted(20604) || isQuestStarted(20605))
            {
                var map = await getMap(913010200);
                if (map.getAllPlayers().Count == 0)
                {
                    await map.killAllMonsters();
                    await playPortalSound();
                    await warp(913010200, 0);
                    await spawnMonster(9300289, 0, 0);
                    return true;
                }
                else
                {
                    await Pink("已经有人在尝试击败Boss，请你稍后再来。");
                    return false;
                }
            }
            else
            {
                await Pink("你必须达到100级且正在进行技能训练方可进入第3演武场。");
                return false;
            }
        }


        public async Task<bool> entertraining()
        {

            if (isQuestStarted(1041))
            {
                await playPortalSound();
                await warp(1010100, 4);
            }
            else if (isQuestStarted(1042))
            {
                await playPortalSound();
                await warp(1010200, 4);
            }
            else if (isQuestStarted(1043))
            {
                await playPortalSound();
                await warp(1010300, 4);
            }
            else if (isQuestStarted(1044))
            {
                await playPortalSound();
                await warp(1010400, 4);
            }
            else
            {
                await Pink("只有接受了麦加训练的人才可以进入训练场");
                return false;
            }
            return true;
        }


        public async Task<bool> enterWarehouse()
        {

            await playPortalSound();
            await warp(300000011, 0);
            return true;
        }


        public async Task<bool> enterWitch()
        {

            if (isQuestCompleted(20404))
            {
                int warpMap;

                if (isQuestCompleted(20407))
                {
                    warpMap = 924010200;
                }
                else if (isQuestCompleted(20406))
                {
                    warpMap = 924010100;
                }
                else
                {
                    warpMap = 924010000;
                }

                await playPortalSound();
                await warp(warpMap, 1);
                return true;


            }
            else
            {
                await Pink("I shouldn't go here.. it's creepy!");
                return false;
            }
        }


        public async Task<bool> enter_earth00()
        {

            if (!haveItem(4031890))
            {
                await LightBlue("You need a warp card to activate this portal.");
                return false;
            }

            await playPortalSound();
            await warp(221000300, "earth00");
            return true;
        }


        public async Task<bool> enter_earth01()
        {

            if (!haveItem(4031890))
            {
                await LightBlue("You need a warp card to activate this portal.");
                return false;
            }

            await playPortalSound();
            await warp(120000101, "earth01");
            return true;
        }


        public async Task<bool> enter_nautil()
        {

            await playPortalSound();
            await warp(120010000, "nt01");
            return true;
        }


        public async Task<bool> enter_td()
        {

            await playPortalSound();
            await warp(600000000, "yn00");
            return true;
        }


        public async Task<bool> evanEntrance()
        {

            await playPortalSound();
            await warp(100030400, "east00");
            return true;
        }


        public async Task<bool> evanFall()
        {

            await playPortalSound();
            await warp(900090102, 0);
            return true;
        }


        public async Task<bool> evanFarmCT()
        {

            if (isQuestStarted(22010) || getPlayer().getJob().getId() != 2001)
            {
                await playPortalSound();
                await warp(100030310, 0);
            }
            else
            {
                await Pink("Cannot enter the Lush Forest without a reason.");
            }
            return true;
        }


        public async Task<bool> evanGarden0()
        {

            await playPortalSound();
            await warp(100030200, "east00");
            return true;
        }


        public async Task<bool> evanGarden1()
        {

            if (isQuestStarted(22008))
            {
                await playPortalSound();
                await warp(100030103, "west00");
            }
            else
            {
                await Pink("You cannot go to the Back Yard without a reason");
            }
            return true;
        }


        public async Task<bool> evanlivingRoom()
        {

            await playPortalSound();
            await warp(100030102, "in00");
            return true;
        }


        public async Task<bool> evanRoom0()
        {

            await blockPortal();
            if (containsAreaInfo(22014, "mo30=o"))
            {
                return false;
            }
            await updateAreaInfo(22014, "mo30=o");
            await showInfo("Effect/OnUserEff.img/guideEffect/evanTutorial/evanBalloon30");
            return true;
        }

        // Map: 910510000
        public async Task<bool> exit_puppeteer()
        {
            var eim = GetEventInstanceTrust();
            if (!eim.isEventCleared())
            {
                await Pink("Defeat the Puppeteer before leaving.");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(105070300, 3);
                return true;
            }
        }


        public async Task<bool> female00()
        {

            /**
     *female00.js
     */
            var gender = getPlayer().getGender();
            if (gender == 1)
            {
                await playPortalSound();
                await warp(670010200, 4);
                return true;
            }
            else
            {
                await Pink("You cannot proceed past here.");
                return false;
            }
        }


        public async Task<bool> foxLaidy_map()
        {

            if (!(isQuestStarted(3647) && haveItem(4031793, 1)))
            {
                await playPortalSound();
                await warp(222010200, "east00");
            }
            else
            {
                if (!isQuestStarted(23647))
                {
                    await forceStartQuest(23647);
                }
                await playPortalSound();
                await warp(922220000, "east00");
            }

            return true;
        }


        public async Task<bool> gaga_success()
        {

            await playPortalSound();
            await warp(922240100 + (getPlayer().getMapId() - 922240000), 0);
            return true;
        }


        public async Task<bool> gendergo()
        {

            var map = getPlayer().getMap();
            if (getPortal().getName() == "female00")
            {
                if (getPlayer().getGender() == 1)
                {
                    await playPortalSound();
                    await warp(map.getId(), "female01");
                    return true;
                }
                else
                {
                    await Pink("This portal leads to the girls' area, try the portal at the other side.");
                    return false;
                }
            }
            else
            {
                if (getPlayer().getGender() == 0)
                {
                    await playPortalSound();
                    await warp(map.getId(), "male01");
                    return true;
                }
                else
                {
                    await Pink("This portal leads to the boys' area, try the portal at the other side.");
                    return false;
                }
            }
        }


        public async Task<bool> ghostgate_open()
        {

            if (getPlayer().getMap().getReactorByName("ghostgate")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000800, 0);
                return true;
            }
            else
            {
                await Pink("This way forward is not open yet.");
                return false;
            }
        }


        public async Task<bool> glpqEnter()
        {

            if (haveItem(3992041, 1))
            {
                await playPortalSound();
                await warp(610030020, "out00");
                return true;
            }
            else
            {
                await Pink("The giant gate of iron will not budge no matter what, however there is a visible key-shaped socket.");
                return false;
            }
        }


        public async Task<bool> glpqPortal0()
        {

            var eim = GetEventInstanceTrust();
            if (eim.getIntProperty("glpq1") == 0)
            {
                await eim.Pink("This path is currently blocked.");
                return false;

            }
            else
            {
                await playPortalSound();
                await warp(610030200, 0);
                return true;
            }
        }


        public async Task<bool> glpqPortal00()
        {

            if (getPlayer().getJob().GetJobNiche() == 1)
            {
                await playPortalSound();
                await warp(610030510, 0);
                return true;
            }
            else
            {
                await Pink("Only warriors may enter this portal.");
                return false;
            }
        }


        public async Task<bool> glpqPortal01()
        {

            if (getPlayer().getJob().GetJobNiche() == 3)
            {
                await playPortalSound();
                await warp(610030540, 0);
                return true;
            }
            else
            {
                await Pink("Only bowmen may enter this portal.");
                return false;
            }
        }


        public async Task<bool> glpqPortal02()
        {

            if (getPlayer().getJob().GetJobNiche() == 2)
            {
                await playPortalSound();
                await warp(610030521, 0);
                return true;
            }
            else
            {
                await Pink("Only mages may enter this portal.");
                return false;
            }
        }


        public async Task<bool> glpqPortal03()
        {

            if (getPlayer().getJob().GetJobNiche() == 4)
            {
                await playPortalSound();
                await warp(610030530, 0);
                return true;
            }
            else
            {
                await Pink("Only thieves may enter this portal.");
                return false;
            }
        }


        public async Task<bool> glpqPortal04()
        {

            if (getPlayer().getJob().GetJobNiche() == 5)
            {
                await playPortalSound();
                await warp(610030550, 0);
                return true;
            }
            else
            {
                await Pink("Only pirates may enter this portal.");
                return false;
            }
        }


        public async Task<bool> glpqPortal1()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                if (eim.getIntProperty("glpq2") == 5)
                {
                    await playPortalSound();
                    await warp(610030300, 0);
                    return true;
                }
                else
                {
                    await Pink("The portal has not been activated yet!");
                    return false;
                }
            }

            return false;
        }


        public async Task<bool> glpqPortal2()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                await playPortalSound();
                await warp(610030300, 0);

                if (eim.getIntProperty("glpq3") < 5 || eim.getIntProperty("glpq3_p") < 5)
                {
                    if (eim.getIntProperty("glpq3_p") == 5)
                    {
                        await mapMessage(6, "Not all Sigils have been activated yet. Make sure they have all been activated to proceed to the next stage.");
                    }
                    else
                    {
                        eim.setIntProperty("glpq3_p", eim.getIntProperty("glpq3_p") + 1);

                        if (eim.getIntProperty("glpq3") == 5 && eim.getIntProperty("glpq3_p") == 5)
                        {
                            await mapMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                            await eim.showClearEffect(610030300, "3pt", 2);
                            await eim.GiveStageClearRewardAll(3);
                        }
                        else
                        {
                            await mapMessage(6, "An adventurer has passed through! " + (5 - eim.getIntProperty("glpq3_p")) + " to go.");
                        }
                    }
                }
                else
                {
                    await LightBlue("The portal at the bottom has already been opened! Proceed there!");
                }

                return true;
            }

            return false;
        }


        public async Task<bool> glpqPortal3()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                if (eim.getIntProperty("glpq3") < 5 || eim.getIntProperty("glpq3_p") < 5)
                {
                    await Pink("The portal is not opened yet.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    await warp(610030400, 0);
                    return true;
                }
            }

            return false;
        }


        public async Task<bool> glpqPortal4()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                if (eim.getIntProperty("glpq4") < 5)
                {
                    await Pink("The portal is not opened yet.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    await warp(610030500, 0);
                    return true;
                }
            }

            return false;
        }


        public async Task<bool> glpqPortal5()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                if (eim.getIntProperty("glpq5") < 5)
                {
                    await Pink("The portal is not opened yet.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    await warp(610030600, 0);
                    return true;
                }
            }

            return false;
        }


        public async Task<bool> glpqPortal6()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                if (eim.getIntProperty("glpq6") < 3)
                {
                    await Pink("The portal is not opened yet.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    await warp(610030700, 0);
                    return true;
                }
            }

            return false;
        }


        public async Task<bool> glpqPortal7()
        {

            await playPortalSound();
            await warp(610030800, 0);
            return true;
        }


        public async Task<bool> glpqPortalDummy()
        {

            var react = getMap().getReactorByName("mob0");
            if (react == null)
            {
                return false;
            }

            var eim = GetEventInstanceTrust();
            if (eim == null)
            {
                return false;
            }

            if (react.getState() < 1)
            {
                await react.forceHitReactor(1);

                eim.setIntProperty("glpq1", 1);

                await eim.dropMessage(5, "A strange force starts being emitted from the portal apparatus, showing a hidden path once blocked now open.");
                await playPortalSound();
                await warp(610030100, 0);

                await eim.showClearEffect();
                await eim.GiveStageClearRewardAll(1);
                return true;
            }

            await eim.dropMessage(5, "The portal apparatus is malfunctional, due to the last transportation. The finding another way through.");
            return false;
        }


        public async Task<bool> glTutoMsg0()
        {

            await showInstruction("离开这个区域，将无法再回来。", 150, 5);
            return true;
        }


        public async Task<bool> gotocastle()
        {

            if (isQuestCompleted(2321))
            {
                await playPortalSound();
                await warp(isQuestCompleted(2324) ? 106020501 : 106020500, 0);
                return true;
            }
            else
            {
                await Pink("前路布满荆棘，无法通过！");
                return false;
            }
        }


        public async Task<bool> go_secretroom()
        {

            if (!isQuestCompleted(2335) && !(isQuestStarted(2335) && hasItem(4032405)))
            {
                await Pink("门锁了，需要钥匙才能进去。");
                return false;
            }

            if (isQuestStarted(2335))
            {
                await forceCompleteQuest(2335, 1300002);
                await giveCharacterExp(5000, getPlayer());
                await gainItem(4032405, -1);
            }
            await playPortalSound();
            await warp(106021001, 1);
            return true;
        }


        public async Task<bool> gryphius()
        {

            await playPortalSound();
            await warp(240020101, "out00");
            return true;
        }


        public async Task<bool> guild1F00()
        {

            int[] backPortals = [6, 8, 9, 11];
            var idx = GetEventInstanceTrust().gridCheck(getPlayer());

            await playPortalSound();
            await warp(990000600, backPortals[idx]);
            return true;
        }


        public async Task<bool> guild1F01()
        {

            GetEventInstanceTrust().gridInsert(getPlayer(), 0);
            await playPortalSound();
            await warp(990000700, "st00");
            return true;
        }


        public async Task<bool> guild1F02()
        {

            GetEventInstanceTrust().gridInsert(getPlayer(), 1);
            await playPortalSound();
            await warp(990000700, "st00");
            return true;
        }


        public async Task<bool> guild1F03()
        {

            GetEventInstanceTrust().gridInsert(getPlayer(), 3);
            await playPortalSound();
            await warp(990000700, "st00");
            return true;
        }


        public async Task<bool> guild1F04()
        {

            GetEventInstanceTrust().gridInsert(getPlayer(), 2);
            await playPortalSound();
            await warp(990000700, "st00");
            return true;
        }

        [ScriptTag(["GuildQuest"])]
        public async Task<bool> guildwaitingenter()
        {

            var entryTime = long.Parse(GetEventInstanceTrust().getProperty("entryTimestamp"));
            var timeNow = c.CurrentServer.Node.getCurrentTime();

            var timeLeft = Math.Ceiling((entryTime - timeNow) / 1000.0);

            if (timeLeft <= 0)
            {
                await playPortalSound();
                await warp(990000100, 0);
                return true;
            }
            else
            { //cannot proceed while allies can still enter
                await Pink("The portal will open in about " + timeLeft + " seconds.");
                return false;
            }
        }


        public async Task<bool> guildwaitingexit()
        {

            await playPortalSound();
            await warp(101030104);
            return true;
        }


        public async Task<bool> guyfawkes0_esc()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg1") == 1)
            {
                await playPortalSound();
                await warp(674030200, 0);
                return true;
            }
            else
            {
                await Pink("The tunnel is currently blocked.");
                return false;
            }
        }


        public async Task<bool> guyfawkes0_floor()
        {

            await warp(674030000, 0);
            return true;
        }


        public async Task<bool> halloween_enter()
        {

            await playPortalSound();
            await warp(682000100, "st00");
            return true;
        }


        public async Task<bool> halloween_Omni1()
        {

            await Pink("It seems to be locked.");
            return true;
        }


        public async Task<bool> highposition()
        {

            // thanks kvmba for noticing some issues running this script
            await touchTheSky();
            return false;
        }


        public async Task<bool> hontale_Bopen()
        {

            int nextMap;
            AbstractEventInstanceManager eim;
            IMap target;
            Portal? targetPortal;
            string? avail;

            if (getPlayer().getMapId() == 240050101)
            {
                nextMap = 240050102;
                eim = GetEventInstanceTrust();
                target = await eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("sp");
                // only let people through if the eim is ready
                avail = eim.getProperty("1stageclear");
                if (avail == null)
                {
                    // do nothing; send message to player
                    await LightBlue("Horntail\'s Seal is Blocking this Door.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    await getPlayer().changeMap(target, targetPortal);
                    return true;
                }
            }
            else if (getPlayer().getMapId() == 240050102)
            {
                nextMap = 240050103;
                eim = GetEventInstanceTrust();
                target = await eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("sp");
                // only let people through if the eim is ready
                avail = eim.getProperty("2stageclear");
                if (avail == null)
                {
                    // do nothing; send message to player
                    await LightBlue("Horntail\'s Seal is Blocking this Door.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    await getPlayer().changeMap(target, targetPortal);
                    return true;
                }
            }
            else if (getPlayer().getMapId() == 240050103)
            {
                nextMap = 240050104;
                eim = GetEventInstanceTrust();
                target = await eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("sp");
                // only let people through if the eim is ready
                avail = eim.getProperty("3stageclear");
                if (avail == null)
                {
                    // do nothing; send message to player
                    await LightBlue("Horntail\'s Seal is Blocking this Door.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    await getPlayer().changeMap(target, targetPortal);
                    return true;
                }
            }
            else if (getPlayer().getMapId() == 240050104)
            {
                nextMap = 240050105;
                eim = GetEventInstanceTrust();
                target = await eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("sp");
                // only let people through if the eim is ready
                avail = eim.getProperty("4stageclear");
                if (avail == null)
                {
                    // do nothing; send message to player
                    await LightBlue("Horntail\'s Seal is Blocking this Door.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    await getPlayer().changeMap(target, targetPortal);
                    return true;
                }
            }
            else if (getPlayer().getMapId() == 240050105)
            {
                nextMap = 240050100;
                eim = GetEventInstanceTrust();
                target = await eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("st00");

                avail = eim.getProperty("5stageclear");
                if (avail == null)
                {
                    if (haveItem(4001092) && isEventLeader())
                    {
                        await eim.showClearEffect();
                        await LightBlue("The leader's key break the seal for a flash...");
                        await playPortalSound();
                        await getPlayer().changeMap(target, targetPortal);
                        eim.setIntProperty("5stageclear", 1);
                        return true;
                    }
                    else
                    {
                        await LightBlue("Horntail\'s Seal is blocking this door. Only the leader with the key can lift this seal.");
                        return false;
                    }
                }
                else
                {
                    await playPortalSound();
                    await getPlayer().changeMap(target, targetPortal);
                    return true;
                }
            }
            return true;
        }


        public async Task<bool> hontale_BR()
        {

            if (getPlayer().getMapId() == 240060000)
            {
                if (GetEventInstanceTrust().getIntProperty("defeatedHead") >= 1)
                {
                    await playPortalSound();
                    await warp(240060100, 0);
                    return true;
                }
                else
                {
                    await LightBlue("Horntail\'s Seal is Blocking this Door.");
                    return false;
                }
            }
            else if (getPlayer().getMapId() == 240060100)
            {
                if (GetEventInstanceTrust().getIntProperty("defeatedHead") >= 2)
                {
                    await playPortalSound();
                    await warp(240060200, 0);
                    return true;
                }
                else
                {
                    await LightBlue("Horntail\'s Seal is Blocking this Door.");
                    return false;
                }
            }
            return false;
        }


        public async Task<bool> hontale_BtoB1()
        {

            if (getMap().countPlayers() == 1)
            {
                await LightBlue("As the last player on this map, you are compelled to wait for the incoming keys.");
                return false;
            }
            else
            {
                if (haveItem(4001087))
                {
                    await LightBlue("You cannot pass to the next map holding the 1st Crystal Key in your inventory.");
                    return false;
                }
                await playPortalSound();
                await warp(240050101, 0);
                return true;
            }
        }


        public async Task<bool> hontale_C()
        {

            var eim = GetEventInstanceTrust();

            if (isEventLeader() == true)
            {
                int target;
                var theWay = getMap().getReactorByName("light")!.getState();
                if (theWay == 1)
                {
                    target = 240050300; //light
                }
                else if (theWay == 3)
                {
                    target = 240050310; //dark
                }
                else
                {
                    await Pink("Hit the Lightbulb to determine your fate!");
                    return false;
                }

                await playPortalSound();
                await eim.warpEventTeam(target);
                return true;
            }
            else
            {
                await Pink("You are not the party leader. Only the party leader may proceed through this portal.");
                return false;
            }
        }


        public async Task<bool> hontale_morph2()
        {

            await playPortalSound();
            await warp(240040600, 4);
            return true;
        }


        public async Task<bool> hontale_out1()
        {

            await playPortalSound();
            await warp(240050400, "sp");
            return true;
        }


        public async Task<bool> in2159011()
        {

            await openNpc(2159011);
            return true;
        }


        public async Task<bool> inDragonEgg()
        {

            await playPortalSound();
            if (isQuestStarted(22005))
            {
                await playPortalSound();
                await warp(900020100, 0);
            }
            else
            {
                await playPortalSound();
                await warp(100030301, 0);
            }
            return true;
        }


        public async Task<bool> inERShip()
        {

            await playPortalSound();
            await warp(101000400, 2);
            return true;
        }


        public async Task<bool> infoAttack()
        {

            if (isQuestStarted(1035))
            {
                await showInfo("UI/tutorial.img/20");
            }

            await blockPortal();
            return true;
        }


        public async Task<bool> infoMinimap()
        {

            if (isQuestStarted(1031))
            {
                await showInfo("UI/tutorial.img/25");
            }

            await blockPortal();
            return true;
        }


        public async Task<bool> infoPickup()
        {

            if (isQuestStarted(1035))
            {
                await showInfo("UI/tutorial.img/21");
            }

            await blockPortal();
            return true;
        }


        public async Task<bool> infoReactor()
        {

            if (isQuestCompleted(1008))
            {
                await showInfo("UI/tutorial.img/22");
            }
            else if (isQuestCompleted(1020))
            {
                await showInfo("UI/tutorial.img/27");
            }

            await blockPortal();
            return true;
        }


        public async Task<bool> infoSkill()
        {

            if (isQuestCompleted(1035))
            {
                await showInfo("UI/tutorial.img/23");
            }

            await blockPortal();
            return true;
        }


        public async Task<bool> infoWorldmap()
        {

            await showInfo("UI/tutorial.img/26");
            await blockPortal();
            return true;
        }


        public async Task<bool> inNix1()
        {

            await playPortalSound();
            await warp(240020600, "out00");
            return true;
        }


        public async Task<bool> inNix2()
        {

            await playPortalSound();
            await warp(240020600, "out01");
            return true;
        }


        public async Task<bool> investigate1()
        {

            if (isQuestActive(2314) || isQuestCompleted(2314))
            {
                await openNpc(1300014);
                return true;
            }
            return false;
        }


        public async Task<bool> investigate2()
        {

            if (isQuestActive(2322) || isQuestCompleted(2322))
            {
                await openNpc(1300014);
                return true;
            }
            return false;
        }


        public async Task<bool> in_xmas_party()
        {

            await openNpc(9209100);
            return false;
        }


        public async Task<bool> jail_in()
        {

            await playPortalSound();
            await warp(300000012, "portal");
            return true;
        }


        public async Task<bool> jail_out()
        {

            var jailedTime = getJailTimeLeft();

            if (jailedTime <= 0)
            {
                await playPortalSound();
                // warp(300000010, "in01");
                await warp(getPlayer().getSavedLocation("JAIL"));
                return true;
            }
            else
            {
                var seconds = Math.Floor(jailedTime / 1000.0) % 60;
                var minutes = (Math.Floor(jailedTime / (1000.0 * 60)) % 60);
                var hours = (Math.Floor(jailedTime / (1000.0 * 60 * 60)) % 24);

                await Pink("You have been caught in bad behaviour by the Maple POLICE. You've got to stay here for " + hours + " hours " + minutes + " minutes " + seconds + " seconds yet.");
                return false;
            }
        }


        public async Task<bool> jnr12_in()
        {

            await playPortalSound();
            await warp(926110401, 0); //next
            return true;
        }


        public async Task<bool> jnr1_out()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg2") == 1)
            {
                await playPortalSound();
                await warp(926110100, 0); //next
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> jnr1_pt00()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg1") == 1)
            {
                await playPortalSound();
                await warp(926110001, 0); //next
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> jnr2_out()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg3") == 3)
            {
                await playPortalSound();
                await warp(926110200, 0); //next
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> jnr3_in0()
        {

            if (getMap().getReactorByName("jnr3_out1")?.getState() == 1)
            {
                await playPortalSound();
                await warp(926110201, 0);
                return true;
            }
            else
            {
                await Pink("The door is not opened yet.");
                return false;
            }
        }


        public async Task<bool> jnr3_in1()
        {

            if (getMap().getReactorByName("jnr3_out2")?.getState() == 1)
            {
                await playPortalSound();
                await warp(926110202, 0);
                return true;
            }
            else
            {
                await Pink("The door is not opened yet.");
                return false;
            }
        }


        public async Task<bool> jnr3_out()
        {

            if (getMap().getReactorByName("jnr3_out3")?.getState() == 1)
            {
                await playPortalSound();
                await warp(926110203, 0); //next
                return true;
            }
            else
            {
                await Pink("The door is not opened yet.");
                return false;
            }
        }


        public async Task<bool> jnr4_r1()
        {

            var eim = GetEventInstanceTrust();
            var area = eim.getIntProperty("statusStg5");
            var reg = 0;

            if ((area >> reg) % 2 == 0)
            {
                area |= (1 << reg);
                eim.setIntProperty("statusStg5", area);

                await playPortalSound();
                await warp(926110301 + reg, 0); //next
                return true;
            }
            else
            {
                await Pink("This room is already being explored.");
                return false;
            }
        }


        public async Task<bool> jnr4_r2()
        {

            var eim = GetEventInstanceTrust();
            var area = eim.getIntProperty("statusStg5");
            var reg = 1;

            if ((area >> reg) % 2 == 0)
            {
                area |= (1 << reg);
                eim.setIntProperty("statusStg5", area);

                await playPortalSound();
                await warp(926110301 + reg, 0); //next
                return true;
            }
            else
            {
                await Pink("This room is already being explored.");
                return false;
            }
        }


        public async Task<bool> jnr4_r3()
        {

            var eim = GetEventInstanceTrust();
            var area = eim.getIntProperty("statusStg5");
            var reg = 2;

            if ((area >> reg) % 2 == 0)
            {
                area |= (1 << reg);
                eim.setIntProperty("statusStg5", area);

                await playPortalSound();
                await warp(926110301 + reg, 0); //next
                return true;
            }
            else
            {
                await Pink("This room is already being explored.");
                return false;
            }
        }


        public async Task<bool> jnr4_r4()
        {

            var eim = GetEventInstanceTrust();
            var area = eim.getIntProperty("statusStg5");
            var reg = 3;

            if ((area >> reg) % 2 == 0)
            {
                area |= (1 << reg);
                eim.setIntProperty("statusStg5", area);

                await playPortalSound();
                await warp(926110301 + reg, 0); //next
                return true;
            }
            else
            {
                await Pink("This room is already being explored.");
                return false;
            }
        }


        public async Task<bool> jnr5_rp()
        {

            var mapplayer = "stage6_comb" + (getMapId() % 10);
            var eim = GetEventInstanceTrust();

            var comb = eim.getProperty(mapplayer);
            if (string.IsNullOrEmpty(comb))
            {
                comb = "";
                for (var i = 0; i < 10; i++)
                {
                    var r = Random.Shared.Next(4);
                    comb += r.ToString();
                }

                eim.setProperty(mapplayer, comb);
            }

            var name = getPortal().getName().Substring(2, 4);
            var portalId = int.Parse(name);


            var pRow = (int)Math.Floor(portalId / 10.0);
            var pCol = (portalId % 10);

            if (pCol == int.Parse(comb.Substring(pRow, pRow + 1)))
            {    //climb
                if (pRow < 9)
                {
                    await playPortalSound();
                    await warp(getMapId(), getPortal().getId() + 4);
                }
                else
                {
                    if (eim.getIntProperty("statusStg6") == 0)
                    {
                        eim.setIntProperty("statusStg6", 1);
                        await eim.GiveStageClearRewardAll(6);
                    }

                    await playPortalSound();
                    await warp(getMapId(), 1);
                }

            }
            else
            {    //fail
                await playPortalSound();
                await warp(getMapId(), 2);
            }

            return true;
        }


        public async Task<bool> jnr6_out()
        {

            if (getMap().getReactorByName("jnr6_out")?.getState() == 1)
            {
                await playPortalSound();
                await warp(926110300, 0);
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> jnr_201_0()
        {

            if (getMap().getReactorByName("jnr31_out")?.getState() == 1)
            {
                await playPortalSound();
                await warp(926110200, 1);
                return true;
            }
            else
            {
                await Pink("The door is not opened yet.");
                return false;
            }
        }


        public async Task<bool> jnr_202()
        {

            if (getMap().getReactorByName("jnr32_out")?.getState() == 1)
            {
                await playPortalSound();
                await warp(926110200, 2);
                return true;
            }
            else
            {
                await Pink("The door is not opened yet.");
                return false;
            }
        }


        public async Task<bool> jnr_exit()
        {

            await playPortalSound();
            await warp(261000021, 0);
            return true;
        }


        public async Task<bool> kinggate2_open()
        {

            var eim = GetEventInstanceTrust();

            if (getPlayer().getMap().getReactorByName("kinggate")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000900, 2);
                if (eim.getProperty("boss") == "true")
                {
                    await changeMusic("Bgm10/Eregos");
                }
                return true;
            }
            else
            {
                await Pink("This crack appears to be blocked off by the door nearby.");
                return false;
            }
        }


        public async Task<bool> kinggate_open()
        {
            var eim = GetEventInstanceTrust();

            if (getPlayer().getMap().getReactorByName("kinggate")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000900, 1);
                if (eim.getProperty("boss") == "true")
                {
                    await changeMusic("Bgm10/Eregos");
                }
                return true;
            }
            else
            {
                await Pink("This door is closed.");
                return false;
            }
        }


        public async Task<bool> kpq0()
        {
            var eim = GetEventInstanceTrust();

            if (eim.ClearedMaps.ContainsKey(getMapId()))
            {
                await playPortalSound();
                var target = await eim.getMapInstance(103000801);
                await getPlayer().changeMap(target, target.getPortal("st00"));
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> kpq1()
        {
            var eim = GetEventInstanceTrust();

            if (eim.ClearedMaps.ContainsKey(getMapId()))
            {
                await playPortalSound();
                var target = await eim.getMapInstance(103000802);
                await getPlayer().changeMap(target, target.getPortal("st00"));
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> kpq2()
        {

            var eim = GetEventInstanceTrust();

            if (eim.ClearedMaps.ContainsKey(getMapId()))
            {
                await playPortalSound();
                var target = await eim.getMapInstance(103000803);
                await getPlayer().changeMap(target, target.getPortal("st00"));
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> kpq3()
        {

            var eim = GetEventInstanceTrust();

            if (eim.ClearedMaps.ContainsKey(getMapId()))
            {
                await playPortalSound();
                var target = await eim.getMapInstance(103000804);
                await getPlayer().changeMap(target, target.getPortal("st00"));
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> kpq4()
        {

            var eim = GetEventInstanceTrust();

            if (eim.ClearedMaps.ContainsKey(getMapId()))
            {
                await playPortalSound();
                var target = await eim.getMapInstance(103000805);
                await getPlayer().changeMap(target, target.getPortal("st00"));
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> lionCastle_enter()
        {

            await playPortalSound();
            await warp(211060010, "west00");
            return true;
        }


        public async Task<bool> lpq0()
        {
            var eim = GetEventInstanceTrust();
            if (!eim.ClearedMaps.ContainsKey(getMapId()))
            {
                await Pink("Some seal is blocking this door.");
                return false;
            }
            else
            {
                await playPortalSound();
                var nextMap = 922010200;
                var target = await eim.getMapInstance(nextMap);
                var targetPortal = target.getPortal("st00");
                await getPlayer().changeMap(target, targetPortal);
                return true;
            }
        }


        public async Task<bool> lpq1()
        {
            var eim = GetEventInstanceTrust();

            if (!eim.ClearedMaps.ContainsKey(getMapId()))
            {
                await Pink("Some seal is blocking this door.");
                return false;
            }
            else
            {
                await playPortalSound();
                var nextMap = 922010300;
                var target = await eim.getMapInstance(nextMap);
                var targetPortal = target.getPortal("st00");
                await getPlayer().changeMap(target, targetPortal);
                return true;
            }
        }


        public async Task<bool> lpq2()
        {
            var eim = GetEventInstanceTrust();

            // only let people through if the eim is ready
            if (!eim.ClearedMaps.ContainsKey(getMapId()))
            {
                // can't go thru eh?
                await Pink("Some seal is blocking this door.");
                return false;
            }
            else
            {
                await playPortalSound();
                var nextMap = 922010400;
                var target = await eim.getMapInstance(nextMap);
                var targetPortal = target.getPortal("st00");
                await getPlayer().changeMap(target, targetPortal);
                return true;
            }
        }


        public async Task<bool> lpq3()
        {
            var eim = GetEventInstanceTrust();

            // only let people through if the eim is ready
            if (!eim.ClearedMaps.ContainsKey(getMapId()))
            {
                // can't go thru eh?
                await Pink("Some seal is blocking this door.");
                return false;
            }
            else
            {
                await playPortalSound();
                var nextMap = 922010500;
                var target = await eim.getMapInstance(nextMap);
                var targetPortal = target.getPortal("st00");
                await getPlayer().changeMap(target, targetPortal);
                return true;
            }
        }


        public async Task<bool> lpq4()
        {
            var eim = GetEventInstanceTrust();

            // only let people through if the eim is ready
            if (!eim.ClearedMaps.ContainsKey(getMapId()))
            {
                // can't go thru eh?
                await Pink("Some seal is blocking this door.");
                return false;
            }
            else
            {
                await playPortalSound();
                var nextMap = 922010600;
                var target = await eim.getMapInstance(nextMap);
                var targetPortal = target.getPortal("st00");
                await getPlayer().changeMap(target, targetPortal);
                return true;
            }
        }


        public async Task<bool> lpq5()
        {
            var eim = GetEventInstanceTrust();
            // only let people through if the eim is ready
            if (!eim.ClearedMaps.ContainsKey(getMapId()))
            {
                // can't go thru eh?
                await Pink("Some seal is blocking this door.");
                return false;
            }
            else
            {
                await playPortalSound();
                var nextMap = 922010700;
                var target = await eim.getMapInstance(nextMap);
                var targetPortal = target.getPortal("st00");
                await getPlayer().changeMap(target, targetPortal);
                return true;
            }
        }


        public async Task<bool> lpq6()
        {
            var eim = GetEventInstanceTrust();

            if (eim.ClearedMaps.ContainsKey(getMapId()))
            {
                await playPortalSound();
                var target = await eim.getMapInstance(922010800);
                await getPlayer().changeMap(target, target.getPortal("st00"));
                return true;
            }
            else
            {
                return false;
            }
        }


        public async Task<bool> lpq7()
        {
            var eim = GetEventInstanceTrust();

            // only let people through if the eim is ready
            if (!eim.ClearedMaps.ContainsKey(getMapId()))
            {
                // can't go thru eh?
                await Pink("Some seal is blocking this door.");
                return false;
            }
            else
            {
                await playPortalSound();
                var nextMap = 922010900;
                var target = await eim.getMapInstance(nextMap);
                var targetPortal = target.getPortal("st00");
                await getPlayer().changeMap(target, targetPortal);
                return true;
            }
        }


        public async Task<bool> ludi021()
        {

            await removeAll(4031092);
            await warp(220020600);
            return true;
        }


        public async Task<bool> magatia_alc0()
        {

            if (!isQuestStarted(3309) || haveItem(4031708, 1))
            {
                await playPortalSound();
                await warp(261020700, "down00");
            }
            else
            {
                await playPortalSound();
                await warp(926120000, "out00");
            }

            return true;
        }


        public async Task<bool> magatia_dark0()
        {

            if (isQuestCompleted(7770))
            {
                await playPortalSound();
                await warp(926130000, "out00");
                return true;
            }
            else
            {
                await Pink("This pipe seems too dark to venture inside.");
                return false;
            }
        }


        public async Task<bool> male00()
        {

            /**
     *Male00.js
     */
            var gender = getPlayer().getGender();
            if (gender == 0)
            {
                await playPortalSound();
                await warp(670010200, 3);
                return true;
            }
            else
            {
                await Pink("You cannot proceed past here.");
                return false;
            }
        }


        public async Task<bool> mapleMarket7_out()
        {

            await playPortalSound();
            await warp(getPlayer().getSavedLocation("EVENT"));
            return true;
        }


        public async Task<bool> market00()
        {

            try
            {
                var toMap = getPlayer().getSavedLocation("FREE_MARKET");
                await playPortalSound();
                await warp(toMap, await getMarketPortalId(toMap));
            }
            catch (Exception)
            {
                await playPortalSound();
                await warp(100000000, 0);
            }
            return true;
        }


        public async Task<bool> market01()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market02()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market03()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market04()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market05()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market06()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market07()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market08()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market09()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market10()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market11()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market12()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market13()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market14()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market15()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market16()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market17()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market18()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market19()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market20()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market21()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market22()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market23()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market24()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market26()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market52()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market53()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market54()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market55()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> market56()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                await playPortalSound();
                await warp(910000000, "out00");
                return true;
            }
            return false;
        }


        public async Task<bool> Masteria_B1_1()
        {

            if (hasItem(3992040))
            {
                await playPortalSound();
                await warp(610010005, "sU6_1");
                return true;
            }
            return false;
        }


        public async Task<bool> Masteria_B2_1()
        {

            if (hasItem(3992040))
            {
                await playPortalSound();
                await warp(610010005, "sU6_1");
                return true;
            }
            return false;
        }


        public async Task<bool> Masteria_B3_1()
        {

            if (hasItem(3992040))
            {
                await playPortalSound();
                await warp(610010005, "sU6_1");
                return true;
            }
            return false;
        }


        public async Task<bool> Masteria_CC1_A()
        {

            await playPortalSound();
            await warp(610020015, "CC6_A");
            return true;
        }


        public async Task<bool> Masteria_CC6_A()
        {

            await playPortalSound();
            await warp(610020010, "CC1_A");
            return true;
        }


        public async Task<bool> Masteria_CM1_A()
        {

            if (hasItem(3992039))
            {
                await playPortalSound();
                await warp(610020000, "CM1_B");
                return false;
            }
            return true;
        }


        public async Task<bool> Masteria_CM1_B()
        {

            if (hasItem(3992039))
            {
                await playPortalSound();
                await warp(610020000, "CM1_C");
                return false;
            }
            return true;
        }


        public async Task<bool> Masteria_CM1_C()
        {

            if (hasItem(3992039))
            {
                await playPortalSound();
                await warp(610020000, "CM1_D");
                return false;
            }
            return true;
        }


        public async Task<bool> Masteria_CM1_D()
        {

            if (hasItem(3992039))
            {
                await playPortalSound();
                await warp(610020000, "CM1_E");
                return false;
            }
            return true;
        }


        public async Task<bool> Masteria_CM2_B()
        {

            if (hasItem(3992039))
            {
                await playPortalSound();
                await warp(610020001, "CM2_C");
                return false;
            }
            return true;
        }


        public async Task<bool> Masteria_CM2_C()
        {

            if (hasItem(3992039))
            {
                await playPortalSound();
                await warp(610020001, "CM2_D");
                return false;
            }
            return true;
        }


        public async Task<bool> Masteria_CM2_D()
        {

            if (hasItem(3992039))
            {
                await playPortalSound();
                await warp(610020001, "CM2_E");
                return false;
            }
            return true;
        }


        public async Task<bool> Masteria_CM2_E()
        {

            if (hasItem(3992039))
            {
                await playPortalSound();
                await warp(610020001, "CM2_F");
                return false;
            }
            return true;
        }


        public async Task<bool> Masteria_U2_1()
        {

            if (hasItem(3992040))
            {
                await playPortalSound();
                await warp(610010004, "U5_1");
                return false;
            }
            return true;
        }


        public async Task<bool> Masteria_U3_1()
        {

            if (hasItem(3992040))
            {
                await playPortalSound();
                await warp(610010201, "sB2_1");
                return false;
            }
            return true;
        }


        public async Task<bool> Masteria_U5_1()
        {

            if (hasItem(3992040))
            {
                await playPortalSound();
                await warp(610010001, "sU2_1");
                return false;
            }
            return true;
        }


        public async Task<bool> Masteria_U5_2()
        {

            if (hasItem(3992040))
            {
                await playPortalSound();
                await warp(610010004, "U5_1");
                return false;
            }
            return true;
        }


        public async Task<bool> Masteria_U6_1()
        {

            if (hasItem(3992040))
            {
                await playPortalSound();
                await warp(610010002, "sU3_1");
                return false;
            }
            return true;
        }


        public async Task<bool> mayong()
        {

            await playPortalSound();
            await warp(240020401, "out00");
            return true;
        }


        public async Task<bool> MC2revive()
        {

            await playPortalSound();
            await WarpReturn();
            return true;
        }


        public async Task<bool> MCrevive1()
        {

            await WarpReturn();
            await playPortalSound();
            return true;
        }


        public async Task<bool> MCrevive2()
        {

            await WarpReturn();
            await playPortalSound();
            return true;
        }


        public async Task<bool> MCrevive3()
        {

            var portal = 0;
            switch (getPlayer().getTeam())
            {
                case 0:
                    portal = 4;
                    break;
                case 1:
                    portal = 3;
                    break;
            }
            await WarpReturn(portal);
            await playPortalSound();
            return true;
        }


        public async Task<bool> MCrevive4()
        {

            var portal = 0;
            switch (getPlayer().getTeam())
            {
                case 0:
                    portal = 4;
                    break;
                case 1:
                    portal = 3;
                    break;
            }
            await WarpReturn(portal);
            await playPortalSound();
            return true;
        }


        public async Task<bool> MCrevive5()
        {

            await WarpReturn();
            await playPortalSound();
            return true;
        }


        public async Task<bool> MCrevive6()
        {

            await WarpReturn();
            await playPortalSound();
            return true;
        }


        public async Task<bool> mc_out()
        {

            var returnMap = getPlayer().getSavedLocation("MONSTER_CARNIVAL");
            if (returnMap < 0)
            {
                returnMap = 102000000; // Just Incase there is no saved location.
            }
            var target = await getPlayer().getClient().getChannelServer().getMapFactory().getMap(returnMap);
            await getPlayer().changeMap(target);
            await playPortalSound();
            return true;
        }


        public async Task<bool> MD_cakeEnter()
        {

            await playPortalSound();
            await warp(674030100, "in00");
            return true;
        }


        public async Task<bool> MD_drakeroom()
        {
            var baseid = 105090311;
            var dungeonid = 105090320;
            var dungeons = 30;

            if (getMapId() == baseid)
            {
                if (getParty() != null)
                {
                    if (isLeader())
                    {
                        for (var i = 0; i < dungeons; i++)
                        {
                            if (await startDungeonInstance(dungeonid + i))
                            {
                                await playPortalSound();
                                await warpParty(dungeonid + i, "out00");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        await Pink("Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return false;
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (await startDungeonInstance(dungeonid + i))
                        {
                            await playPortalSound();
                            await warp(dungeonid + i, "out00");
                            return true;
                        }
                    }
                }
                await Pink("All of the Mini-Dungeons are in use right now, please try again later.");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(baseid, "MD00");
                return true;
            }
        }


        public async Task<bool> MD_error()
        {
            var baseid = 261020300;
            var dungeonid = 261020301;
            var dungeons = 30;
            if (getMapId() == baseid)
            {
                if (getParty() != null)
                {
                    if (isLeader())
                    {
                        for (var i = 0; i < dungeons; i++)
                        {
                            if (await startDungeonInstance(dungeonid + i))
                            {
                                await playPortalSound();
                                await warpParty(dungeonid + i, "out00");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        await Pink("Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return false;
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (await startDungeonInstance(dungeonid + i))
                        {
                            await playPortalSound();
                            await warp(dungeonid + i, "out00");
                            return true;
                        }
                    }
                }
                await Pink("All of the Mini-Dungeons are in use right now, please try again later.");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(baseid, "MD00");
                return true;
            }
        }


        public async Task<bool> MD_golem()
        {
            var baseid = 105040304;
            var dungeonid = 105040320;
            var dungeons = 30;
            if (getMapId() == baseid)
            {
                if (getParty() != null)
                {
                    if (isLeader())
                    {
                        for (var i = 0; i < dungeons; i++)
                        {
                            if (await startDungeonInstance(dungeonid + i))
                            {
                                await playPortalSound();
                                await warpParty(dungeonid + i, "out00");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        await Pink("Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return false;
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (await startDungeonInstance(dungeonid + i))
                        {
                            await playPortalSound();
                            await warp(dungeonid + i, "out00");
                            return true;
                        }
                    }
                }
                await Pink("All of the Mini-Dungeons are in use right now, please try again later.");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(baseid, "MD00");
                return true;
            }
        }


        public async Task<bool> MD_high()
        {
            var baseid = 551030000;
            var dungeonid = 551030001;
            var dungeons = 19;
            if (getMapId() == baseid)
            {
                if (getParty() != null)
                {
                    if (isLeader())
                    {
                        for (var i = 0; i < dungeons; i++)
                        {
                            if (await startDungeonInstance(dungeonid + i))
                            {
                                await playPortalSound();
                                await warpParty(dungeonid + i, "out00");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        await Pink("Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return false;
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (await startDungeonInstance(dungeonid + i))
                        {
                            await playPortalSound();
                            await warp(dungeonid + i, "out00");
                            return true;
                        }
                    }
                }
                await Pink("All of the Mini-Dungeons are in use right now, please try again later.");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(baseid, "MD00");
                return true;
            }
        }


        public async Task<bool> MD_mushroom()
        {
            var baseid = 105050100;
            var dungeonid = 105050101;
            var dungeons = 30;
            if (getMapId() == baseid)
            {
                if (getParty() != null)
                {
                    if (isLeader())
                    {
                        for (var i = 0; i < dungeons; i++)
                        {
                            if (await startDungeonInstance(dungeonid + i))
                            {
                                await playPortalSound();
                                await warpParty(dungeonid + i, "out00");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        await Pink("Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return false;
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (await startDungeonInstance(dungeonid + i))
                        {
                            await playPortalSound();
                            await warp(dungeonid + i, "out00");
                            return true;
                        }
                    }
                }
                await Pink("All of the Mini-Dungeons are in use right now, please try again later.");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(baseid, "MD00");
                return true;
            }
        }


        public async Task<bool> MD_pig()
        {
            var baseid = 100020000;
            var dungeonid = 100020100;
            var dungeons = 30;
            if (getMapId() == baseid)
            {
                if (getParty() != null)
                {
                    if (isLeader())
                    {
                        for (var i = 0; i < dungeons; i++)
                        {
                            if (await startDungeonInstance(dungeonid + i))
                            {
                                await playPortalSound();
                                await warpParty(dungeonid + i, "out00");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        await Pink("Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return false;
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (await startDungeonInstance(dungeonid + i))
                        {
                            await playPortalSound();
                            await warp(dungeonid + i, "out00");
                            return true;
                        }
                    }
                }
                await Pink("All of the Mini-Dungeons are in use right now, please try again later.");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(baseid, "MD00");
                return true;
            }
        }


        public async Task<bool> MD_protect()
        {
            var baseid = 240040520;
            var dungeonid = 240040900;
            var dungeons = 19;
            if (getMapId() == baseid)
            {
                if (getParty() != null)
                {
                    if (isLeader())
                    {
                        for (var i = 0; i < dungeons; i++)
                        {
                            if (await startDungeonInstance(dungeonid + i))
                            {
                                await playPortalSound();
                                await warpParty(dungeonid + i, "out00");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        await Pink("Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return false;
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (await startDungeonInstance(dungeonid + i))
                        {
                            await playPortalSound();
                            await warp(dungeonid + i, "out00");
                            return true;
                        }
                    }
                }
                await Pink("All of the Mini-Dungeons are in use right now, please try again later.");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(baseid, "MD00");
                return true;
            }
        }


        public async Task<bool> MD_rabbit()
        {
            var baseid = 240040520;
            var dungeonid = 240040900;
            var dungeons = 19;
            if (getMapId() == baseid)
            {
                if (getParty() != null)
                {
                    if (isLeader())
                    {
                        for (var i = 0; i < dungeons; i++)
                        {
                            if (await startDungeonInstance(dungeonid + i))
                            {
                                await playPortalSound();
                                await warpParty(dungeonid + i, "out00");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        await Pink("Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return false;
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (await startDungeonInstance(dungeonid + i))
                        {
                            await playPortalSound();
                            await warp(dungeonid + i, "out00");
                            return true;
                        }
                    }
                }
                await Pink("All of the Mini-Dungeons are in use right now, please try again later.");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(baseid, "MD00");
                return true;
            }
        }


        public async Task<bool> MD_remember()
        {
            var baseid = 240040511;
            var dungeonid = 240040800;
            var dungeons = 19;
            if (getMapId() == baseid)
            {
                if (getParty() != null)
                {
                    if (isLeader())
                    {
                        for (var i = 0; i < dungeons; i++)
                        {
                            if (await startDungeonInstance(dungeonid + i))
                            {
                                await playPortalSound();
                                await warpParty(dungeonid + i, "out00");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        await Pink("Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return false;
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (await startDungeonInstance(dungeonid + i))
                        {
                            await playPortalSound();
                            await warp(dungeonid + i, "out00");
                            return true;
                        }
                    }
                }
                await Pink("All of the Mini-Dungeons are in use right now, please try again later.");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(baseid, "MD00");
                return true;
            }
        }


        public async Task<bool> MD_roundTable()
        {
            var baseid = 240020500;
            var dungeonid = 240020512;
            var dungeons = 30;
            if (getMapId() == baseid)
            {
                if (getParty() != null)
                {
                    if (isLeader())
                    {
                        for (var i = 0; i < dungeons; i++)
                        {
                            if (await startDungeonInstance(dungeonid + i))
                            {
                                await playPortalSound();
                                await warpParty(dungeonid + i, "out00");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        await Pink("Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return false;
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (await startDungeonInstance(dungeonid + i))
                        {
                            await playPortalSound();
                            await warp(dungeonid + i, "out00");
                            return true;
                        }
                    }
                }
                await Pink("All of the Mini-Dungeons are in use right now, please try again later.");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(baseid, "MD00");
                return true;
            }
        }


        public async Task<bool> MD_sand()
        {
            var baseid = 260020600;
            var dungeonid = 260020630;
            var dungeons = 34;

            if (getMapId() == baseid)
            {
                if (getParty() != null)
                {
                    if (isLeader())
                    {
                        for (var i = 0; i < dungeons; i++)
                        {
                            if (await startDungeonInstance(dungeonid + i))
                            {
                                await playPortalSound();
                                await warpParty(dungeonid + i, "out00");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        await Pink("Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return false;
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (await startDungeonInstance(dungeonid + i))
                        {
                            await playPortalSound();
                            await warp(dungeonid + i, "out00");
                            return true;
                        }
                    }
                }
                await Pink("All of the Mini-Dungeons are in use right now, please try again later.");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(baseid, "MD00");
                return true;
            }
        }


        public async Task<bool> MD_treasure()
        {
            var baseid = 251010402;
            var dungeonid = 251010410;
            var dungeons = 30;
            if (getMapId() == baseid)
            {
                if (getParty() != null)
                {
                    if (isLeader())
                    {
                        for (var i = 0; i < dungeons; i++)
                        {
                            if (await startDungeonInstance(dungeonid + i))
                            {
                                await playPortalSound();
                                await warpParty(dungeonid + i, "out00");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        await Pink("Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return false;
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (await startDungeonInstance(dungeonid + i))
                        {
                            await playPortalSound();
                            await warp(dungeonid + i, "out00");
                            return true;
                        }
                    }
                }
                await Pink("All of the Mini-Dungeons are in use right now, please try again later.");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(baseid, "MD00");
                return true;
            }
        }


        public async Task<bool> metalgate_open()
        {

            if (getPlayer().getMap().getReactorByName("metalgate")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000431, 0);
                return true;
            }
            await Pink("This way forward is not open yet.");
            return false;
        }


        public async Task<bool> metro_Chat00()
        {

            await showIntro("Effect/Direction2.img/metro/Im");
            //showWZEffect("Effect/Direction2.img/metro/Im");
            return true;
        }


        public async Task<bool> metro_in00()
        {

            await openNpc(1052115);
            return true;
        }


        public async Task<bool> met_in()
        {

            //warp(910320000, 2); event not implemented

            await playPortalSound();
            await warp(103000103, 1);
            return true;
        }


        public async Task<bool> met_out()
        {

            var mapId = getPlayer().getSavedLocation("MIRROR");

            await playPortalSound();
            if (mapId == -1)
            {
                await warp(102040000, 12);
            }
            else
            {
                await warp(mapId);
            }

            //warp(102040000, 12);
            return true;
        }


        public async Task<bool> minar_elli()
        {

            if (!haveItem(4031346))
            {
                await LightBlue("You need a magic seed to use this portal.");
                return false;
            }
            if (getPlayer().getMapId() == 240010100)
            {
                await gainItem(4031346, -1);
                await playPortalSound();
                await warp(101010000, "minar00");
                return true;
            }
            else if (getPlayer().getMapId() == 101010000)
            {
                await gainItem(4031346, -1);
                await playPortalSound();
                await warp(240010100, "elli00");
                return true;
            }
            return true;
        }


        public async Task<bool> minar_job4()
        {

            await playPortalSound();
            await warp(240010501, "out00");
            return true;
        }


        public async Task<bool> mirtalk00()
        {

            await blockPortal();
            if (containsAreaInfo(22013, "dt00=o"))
            {
                return false;
            }
            await mapEffect("evan/dragonTalk00");
            await updateAreaInfo(22013, "dt00=o;mo00=o");
            return true;
        }


        public async Task<bool> mirtalk01()
        {

            await blockPortal();
            if (containsAreaInfo(22013, "dt01=o"))
            {
                return false;
            }
            await mapEffect("evan/dragonTalk01");
            await updateAreaInfo(22013, "dt00=o;dt01=o;mo00=o;mo01=o;mo10=o;mo02=o");
            return true;
        }


        public async Task<bool> moveBefore()
        {

            await playPortalSound();
            await warp(getMapId() - 10, "west00");
            return true;
        }


        public async Task<bool> moveNext()
        {

            await playPortalSound();
            await warp(getMapId() + 10, "east00");
            return true;
        }


        public async Task<bool> move_elin()
        {

            await playPortalSound();
            await warp(300000100, "out00");
            await Pink("移动到时间之门的另一端。");
            return true;
        }


        public async Task<bool> move_RieRit()
        {

            return true;
        }


        public async Task<bool> move_RitRie()
        {

            return true;
        }


        public async Task<bool> nets_in()
        {

            getPlayer().saveLocation("MIRROR");
            await playPortalSound();
            await warp(926010000, 4);
            return true;
        }


        public async Task<bool> nets_out()
        {

            var mapid = getPlayer().getSavedLocation("MIRROR");

            await playPortalSound();
            if (mapid == 260020500)
            {
                await warp(mapid, 3);
            }
            else
            {
                await warp(mapid);
            }
            return true;
        }


        public async Task<bool> NextMap()
        {

            await playPortalSound();
            await warp(getMapId() + 100, 0);
            return true;
        }


        public async Task<bool> obstacle()
        {

            if (isQuestStarted(100202))
            {    //使用过奇拉蘑菇孢子后允许直接通过
                await playPortalSound();
                await warp(106020400, 2);
                return true;
            }
            else if (hasItem(4000507))
            {
                await gainItem(4000507, -1);
                await playPortalSound();
                await warp(106020400, 2);
                await Pink("消耗一个 蘑菇的毒孢子 通过了结界。");
                return true;
            }
            else
            {
                await showInfo("Effect/OnUserEff/normalEffect/mushroomcastle/chatBalloon1");
                await Pink("似乎有一个魔力强大的结界阻止你进入。");
            }
            return false;
        }


        public async Task<bool> outArchterMap()
        {

            await playPortalSound();
            await warp(100000000, "Achter00");
            await playPortalSound();
            return true;
        }


        public async Task<bool> outChild()
        {

            if (!isQuestStarted(21001))
            {
                await playPortalSound();
                await warp(914000220, 2);
                return true;
            }
            else
            {
                await playPortalSound();
                await warp(914000400, 2);
                return true;
            }
        }


        public async Task<bool> outDarkEreb()
        {
            var warpMap = isQuestCompleted(20407) ? 924010200 : 924010100;

            await playPortalSound();
            await warp(warpMap, 0);
            return true;
        }


        public async Task<bool> enterMagiclibrar()
        {
            if (isQuestStarted(20718))
            {
                var cml = GetSoloQuestEventManager(20718);
                await cml.StartInstance(getPlayer());
                await playPortalSound();
            }
            else
            {
                await playPortalSound();
                await warp(101000003, 8);
            }
            return true;
        }

        // Map: 910110000
        public async Task<bool> outMagiclib()
        {
            if (getMap().countMonster(2220100) > 0)
            {
                await Pink("Cannot leave until all Blue Mushrooms have been defeated.");
                return false;
            }
            else
            {
                await playPortalSound();
                // 离开任务副本时会自行关闭副本
                await warp(101000000, 26);

                if (isQuestCompleted(20718))
                {
                    await openNpc(1103003, "MaybeItsGrendel_end");
                }

                return true;
            }
        }


        public async Task<bool> outMaha()
        {
            await playPortalSound();
            await WarpReturn(0);
            return true;
        }


        public async Task<bool> outNix1()
        {

            await playPortalSound();
            await warp(240020101, "in00");
            return true;
        }


        public async Task<bool> outNix2()
        {

            await playPortalSound();
            await warp(240020401, "in00");
            return true;
        }


        public async Task<bool> outPerrion_1()
        {

            await Pink("You found a shortcut to the start of the underground temple.");
            await playPortalSound();
            await warp(105100000, 2);
            return true;
        }


        public async Task<bool> outPerrion_2()
        {

            await playPortalSound();
            await warp(105100000, 0);
            return true;
        }


        public async Task<bool> outRider()
        {

            if (canHold(4001193, 1))
            {
                await gainItem(4001193, 1);
                await playPortalSound();
                await warp(211050000, 4);
                return true;
            }
            else
            {
                await Pink("Free a slot on your inventory before receiving the couse clear's token.");
                return false;
            }
        }


        public async Task<bool> outSpecialSchool()
        {

            await playPortalSound();
            await warp(925040000, 1);
            return true;
        }


        public async Task<bool> outTemple()
        {

            await useItem(2210016);
            await playPortalSound();
            await warp(200090510, 0);
            return true;
        }


        public async Task<bool> outtestWolf()
        {

            if (getMap().countMonsters() == 0)
            {
                if (canHold(4001193, 1))
                {
                    await gainItem(4001193, 1);
                    await playPortalSound();
                    await warp(140010210, 0);
                    return true;
                }
                else
                {
                    await Pink("Free a slot on your inventory before receiving the couse clear's token.");
                    return false;
                }
            }
            else
            {
                await Pink("Defeat all wolves before exiting the stage.");
                return false;
            }
        }


        public async Task<bool> out_pepeking()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                await eim.stopEventTimer();
                await eim.DisposeAsync();
            }

            var questProgress = getQuestProgressInt(2330, 3300005) + getQuestProgressInt(2330, 3300006) + getQuestProgressInt(2330, 3300007); //3 Yetis
            if (questProgress == 3 && !hasItem(4032388))
            {
                if (canHold(4032388))
                {
                    await Pink("你已经拿到了结婚礼堂的钥匙。企鹅国王肯定是把它丢掉了。");
                    await gainItem(4032388, 1);

                    await playPortalSound();
                    await warp(106021400, 2);
                    return true;
                }
                else
                {
                    await Pink("请确保背包其它物品栏还有可用空间。");
                    return false;
                }
            }
            else
            {
                await playPortalSound();
                await warp(106021400, 2);
                return true;
            }
        }


        public async Task<bool> party3_gardenin()
        {

            if (getPlayer().getParty() != null && isEventLeader() && haveItem(4001055, 1))
            {
                await playPortalSound();
                await GetEventInstanceTrust().warpEventTeam(920010100);
                return true;
            }
            else
            {
                await Pink("Please get the leader in this portal, make sure you have the Root of Life.");
                return false;
            }
        }


        public async Task<bool> party3_jail1()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg8") == 1)
            {
                await playPortalSound();
                await warp(920010910, 0);
                return true;
            }
            else
            {
                await Pink("The storage is currently inaccessible, as the powers of the Pixies remains active within the tower.");
                return false;
            }
        }


        public async Task<bool> party3_jail2()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg8") == 1)
            {
                await playPortalSound();
                await warp(920010920, 0);
                return true;
            }
            else
            {
                await Pink("The storage is currently inaccessible, as the powers of the Pixies remains active within the tower.");
                return false;
            }
        }


        public async Task<bool> party3_jail3()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg8") == 1)
            {
                await playPortalSound();
                await warp(920010930, 0);
                return true;
            }
            else
            {
                await Pink("The storage is currently inaccessible, as the powers of the Pixies remains active within the tower.");
                return false;
            }
        }


        public async Task<bool> party3_jailin()
        {
            var map = getMap();
            var mobcount = map.countMonster(9300044);

            if (mobcount > 0)
            {
                await Pink("请先使用控制杆清除所有威胁再继续前进");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(getMapId() + 2, 0);
                return true;
            }
        }


        public async Task<bool> party3_r4pt()
        {

            var eim = GetEventInstanceTrust();
            if (eim.getProperty("stage4_comb") == null)
            {
                var r = Math.Floor((Random.Shared.NextDouble() * 3)) + 1;
                var s = Math.Floor((Random.Shared.NextDouble() * 3)) + 1;

                eim.setProperty("stage4_comb", "" + r + s);
            }

            var pname = int.Parse(getPortal().getName().Substring(4, 6));
            var cname = int.Parse(eim.getProperty("stage4_comb"));

            var secondPt = true;
            if (getPortal().getId() < 14)
            {
                cname = (int)Math.Floor(cname / 10.0);
                secondPt = false;
            }

            if ((pname % 10) == (cname % 10))
            {    //climb
                int nextPortal;
                if (secondPt)
                {
                    nextPortal = 1;
                }
                else
                {
                    nextPortal = getPortal().getId() + 3;
                }

                await playPortalSound();
                await warp(getMapId(), nextPortal);
            }
            else
            {    //fail
                await playPortalSound();
                await warp(getMapId(), 2);
            }

            return true;
        }


        public async Task<bool> party3_r4pt1()
        {

            await playPortalSound();
            await warp(920010600, Random.Shared.NextDouble() * 3 > 1 ? 1 : 2);
            return true;
        }


        public async Task<bool> party3_r6pt()
        {

            var eim = GetEventInstanceTrust();
            var comb = eim.getProperty("stage6_comb");
            if (string.IsNullOrEmpty(comb))
            {
                comb = "0";

                for (var i = 0; i < 16; i++)
                {
                    var r = Math.Floor((Random.Shared.NextDouble() * 4)) + 1;
                    comb += r.ToString();
                }

                eim.setProperty("stage6_comb", comb);
            }

            var name = getPortal().getName().Substring(2, 5);
            var portalId = int.Parse(name);


            var pRow = (int)Math.Floor(portalId / 10.0);
            var pCol = portalId % 10;

            if (pCol == int.Parse(comb.Substring(pRow, pRow + 1)))
            {    //climb
                await playPortalSound();
                await warp(getMapId(), (pRow % 4 != 0) ? getPortal().getId() + 4 : (pRow / 4));
            }
            else
            {    //fail
                pRow--;
                await playPortalSound();
                await warp(getMapId(), (pRow / 4.0) > 1 ? (int)(pRow / 4.0) : 5);  // thanks Chloek3, seth1 for noticing next plaform issues
            }

            return true;
        }


        public async Task<bool> party3_room1()
        {

            await playPortalSound();
            await warp(920010200, 13);
            return true;
        }


        public async Task<bool> party3_room2()
        {

            await playPortalSound();
            await warp(920010300, 1);
            return true;
        }


        public async Task<bool> party3_room3()
        {

            await playPortalSound();
            await warp(920010400, 8);
            return true;
        }


        public async Task<bool> party3_room4()
        {

            await playPortalSound();
            await warp(920010500, 3);
            return true;
        }


        public async Task<bool> party3_room5()
        {

            await playPortalSound();
            await warp(920010600, 17);
            return true;
        }


        public async Task<bool> party3_room6()
        {

            await playPortalSound();
            await warp(920010700, 23);
            return true;
        }


        public async Task<bool> party3_room8()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg8") == 1)
            {
                await playPortalSound();
                await warp(920011000, 0);
                return true;
            }
            else
            {
                await Pink("The storage is currently inaccessible, as the powers of the Pixies remains active within the tower.");
                return false;
            }
        }


        public async Task<bool> party3_roomout()
        {

            var exitPortal = 0;

            switch (getMapId())
            {
                case 920010200:
                    exitPortal = 4;
                    break;

                case 920010300:
                    exitPortal = 12;
                    break;

                case 920010400:
                    exitPortal = 5;
                    break;

                case 920010500:
                    exitPortal = 13;
                    break;

                case 920010600:
                    exitPortal = 15;
                    break;

                case 920010700:
                    exitPortal = 14;
                    break;

                case 920011000:
                    exitPortal = 16;
                    break;
            }

            await playPortalSound();
            await warp(920010100, exitPortal);
            return true;
        }


        public async Task<bool> party6_out()
        {

            var eim = GetEventInstanceTrust();
            if (eim == null)
                return false;

            if (eim.isEventCleared())
            {
                if (isEventLeader())
                {
                    await playPortalSound();
                    await eim.warpEventTeam(930000800);
                    return true;
                }
                else
                {
                    await Pink(nameof(ClientMessage.Tip_WaitForLeaderEnterPortal));
                    return false;
                }
            }
            else
            {
                await Pink(nameof(ClientMessage.EllinPQ_NeedDefeatBossFirst));
                return false;
            }
        }

        [ScriptTag(["EllinPQ"])]
        public async Task<bool> party6_stage()
        {

            switch (getMapId())
            {
                case 930000000:
                    await playPortalSound();
                    await warp(930000100, 0);
                    return true;
                case 930000100:
                    if (getMap().countMonsters() == 0)
                    {
                        await playPortalSound();
                        await warp(930000200, 0);
                        return true;
                    }
                    else
                    {
                        await Pink(nameof(ClientMessage.Tip_EliminateAllMonster));
                        return false;
                    }
                case 930000200:
                    if (getMap().getReactorByName("spine") != null && getMap().getReactorByName("spine")?.getState() < 4)
                    {
                        await Pink(nameof(ClientMessage.EllinPQ_SpineBlockWay));
                        return false;
                    }
                    else
                    {
                        await playPortalSound();
                        await warp(930000300, 0); //assuming they cant get past reactor without it being gone
                        return true;
                    }
                default:
                    await Pink(nameof(ClientMessage.Tip_UnknownPortal));
                    return false;
            }
        }


        public async Task<bool> party6_stage501()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "02st");
            }

            return true;
        }


        public async Task<bool> party6_stage502()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "03st");
            }

            return true;
        }


        public async Task<bool> party6_stage503()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "04st");
            }

            return true;
        }


        public async Task<bool> party6_stage504()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "05st");
            }

            return true;
        }


        public async Task<bool> party6_stage505()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "06st");
            }

            return true;
        }


        public async Task<bool> party6_stage506()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "07st");
            }

            return true;
        }


        public async Task<bool> party6_stage507()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "08st");
            }

            return true;
        }


        public async Task<bool> party6_stage508()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "09st");
            }

            return true;
        }


        public async Task<bool> party6_stage509()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "10st");
            }

            return true;
        }


        public async Task<bool> party6_stage510()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "11st");
            }

            return true;
        }


        public async Task<bool> party6_stage511()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "12st");
            }

            return true;
        }


        public async Task<bool> party6_stage512()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "13st");
            }

            return true;
        }


        public async Task<bool> party6_stage513()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "14st");
            }

            return true;
        }


        public async Task<bool> party6_stage514()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "15st");
            }

            return true;
        }


        public async Task<bool> party6_stage515()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "01st");
            }

            return true;
        }


        public async Task<bool> party6_stage800()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                await eim.dropExclusiveItems(getPlayer());

                var spring = getMap().getReactorById(3008000);  // thanks Chloek3, seth1 for noticing fragments not being awarded properly
                if (spring != null && spring.getState() > 0)
                {
                    if (!canHold(4001198, 1))
                    {
                        await Pink("Tip_CheckEtcSizeBeforeEnterPortal");
                        return false;
                    }

                    await gainItem(4001198, 1);
                }
            }

            await playPortalSound();
            await warp(300030100, 0);
            return true;
        }


        public async Task<bool> Pianus()
        {

            await playPortalSound();
            await warp(230040420, "out00");
            return true;
        }


        public async Task<bool> Pinkin()
        {

            await playPortalSound();
            await warp(270050100);
            return true;
        }


        public async Task<bool> Populatus00()
        {

            if (!((isQuestStarted(6361) && haveItem(4031870, 1)) || (isQuestCompleted(6361) && !isQuestCompleted(6363))))
            {
                var em = GetEventManager<PartyQuestEventManager>("PapulatusBattle");

                var r = await em.StartInstance(getPlayer());
                if (r == CreateInstanceResult.Success)
                {
                    await playPortalSound();
                    return true;
                }
                else if (r == CreateInstanceResult.RequiredParty)
                {
                    await Pink("You are currently not in a party, create one to attempt the boss.");
                    return false;
                }
                else if (r == CreateInstanceResult.RequiredLeader)
                {
                    await Pink("Your party leader must enter the portal to start the battle.");
                    return false;
                }
                else if (r == CreateInstanceResult.Requirement)
                {
                    await Pink("You cannot start this battle yet, because either your party is not in the range size, some of your party members are not eligible to attempt it or they are not in this map. If you're having trouble finding party members, try Party Search.");
                    return false;
                }
                else
                {
                    await Pink("The battle against the boss has already begun, so you may not enter this place yet.");
                    return false;
                }
            }
            else
            {
                await playPortalSound();
                await warp(922020300, 0);
                return true;
            }
        }


        public async Task<bool> PPinkOut()
        {

            await playPortalSound();
            await warp(270050000);
            return true;
        }


        public async Task<bool> q2073()
        {

            if (isQuestStarted(2073))
            {
                await playPortalSound();
                await warp(900000000, 0);
                return true;
            }
            else
            {
                await Pink("Private property. This place can only be entered when running an errand from Camila.");
                return false;
            }
        }


        public async Task<bool> q3366in()
        {

            if (isQuestStarted(3366))
            {
                await playPortalSound();
                await warp(926130101, 0);
                return true;
            }
            else
            {
                await Pink("You don't have permission to access this room.");
                return false;
            }
        }


        public async Task<bool> q3366out()
        {

            await playPortalSound();
            await warp(926130100, "in00");
            return true;
        }


        public async Task<bool> q3367in()
        {

            if (isQuestStarted(3367))
            {
                var booksDone = getQuestProgressInt(3367, 31);
                var booksInv = getItemQuantity(4031797);

                if (booksInv < booksDone)
                {
                    await gainItem(4031797, booksDone - booksInv);
                }

                await playPortalSound();
                await warp(926130102, 0);
                return true;
            }
            else
            {
                await Pink("You don't have permission to access this room.");
                return false;
            }
        }


        public async Task<bool> q3367out()
        {

            await playPortalSound();
            await warp(926130100, "in01");
            return true;
        }


        public async Task<bool> q3368in()
        {

            if (isQuestStarted(3368))
            {
                await playPortalSound();
                await warp(926130103, 0);
                return true;
            }
            else
            {
                await Pink("You don't have permission to access this room.");
                return false;
            }
        }


        public async Task<bool> q3368out()
        {

            await playPortalSound();
            await warp(926130100, "in02");
            return true;
        }


        public async Task<bool> raidout()
        {

            var map = getPlayer().getSavedLocation("BOSSPQ");
            if (map == -1)
            {
                map = 100000000;
            }

            await playPortalSound();
            await warp(map, 0);
            return true;
        }


        public async Task<bool> raid_rest()
        {

            var evLevel = ((getMapId() - 1) % 5) + 1;

            var eim = GetEventInstanceTrust();
            if (eim.isEventLeader(getPlayer()) && eim.getPlayerCount() > 1)
            {
                await Pink("Being the party leader, you cannot leave before your teammates leave first or you pass leadership.");
                return false;
            }

            if (await eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
            {
                await playPortalSound();
                await warp(970030000);
                return true;
            }
            else
            {
                await Pink(c.CurrentCulture.GetScriptTalkByKey(nameof(ScriptTalk.Redeem_InventoryFull)));
                return false;
            }
        }


        public async Task<bool> raid_stage()
        {

            if (getMap().countMonsters() == 0)
            {
                int nextStage;

                if (getMapId() % 500 >= 100)
                {
                    nextStage = getMapId() + 100;
                }
                else
                {
                    nextStage = 970030001 + (int)(Math.Floor((getMapId() - 970030100) / 500.0));
                }

                await playPortalSound();
                await warp(nextStage);
                return true;
            }
            else
            {
                await LightBlue("Defeat all monsters before proceeding to the next stage.");
                return false;
            }
        }


        public async Task<bool> rankDeveloperRoom()
        {

            if (getPlayer().getMapId() != 777777777)
            {
                if (!CanEnterDeveloperRoom())
                {
                    await Pink("The next room is currently unavailable.");
                    return false;
                }

                getPlayer().saveLocation("DEVELOPER");
                await playPortalSound();
                await warp(777777777, "out00");
            }
            else
            {
                try
                {
                    var toMap = getPlayer().getSavedLocation("DEVELOPER");
                    await playPortalSound();
                    await warp(toMap, "in00");
                }
                catch (Exception)
                {
                    await playPortalSound();
                    await warp(100000000, 0);
                }
            }

            return true;
        }


        public async Task<bool> rankRoom()
        {

            await playPortalSound();

            switch (getPlayer().getMapId())
            {
                case 130000000:
                    await warp(130000100, 5); //or 130000101
                    break;
                case 130000200:
                    await warp(130000100, 4); //or 130000101
                    break;
                case 140010100:
                    await warp(140010110, 1); //or 140010111
                    break;
                case 120000101:
                    await warp(120000105, 1);
                    break;
                case 103000003:
                    await warp(103000008, 1); //or 103000009
                    break;
                case 100000201:
                    await warp(100000204, 2); //or 100000205
                    break;
                case 101000003: // portal warp fix thanks to Vcoc
                    await warp(101000004, 2); //or 101000005
                    break;
                default:
                    await warp(getMapId() + 1, 1); //or + 2
                    break;
            }

            return true;
        }


        public async Task<bool> reundodraco()
        {

            await blockPortal();
            return true;
        }


        public async Task<bool> rienCaveEnter()
        {

            if (isQuestStarted(21201) || isQuestStarted(21302))
            { //aran first job
                await playPortalSound();
                await warp(140030000, 1);
                return true;
            }
            else
            {
                await Pink("Something seems to be blocking this portal!");
                return false;
            }
        }


        public async Task<bool> rienTutor1()
        {

            if (!isQuestCompleted(21010))
            {
                await Pink("You must complete the quest before proceeding to the next map.");
                return false;
            }
            await playPortalSound();
            await warp(140090200, 1);
            return true;
        }


        public async Task<bool> rienTutor2()
        {

            if (!isQuestCompleted(21011))
            {
                await Pink("You must complete the quest before proceeding to the next map..");
                return false;
            }
            await playPortalSound();
            await warp(140090300, 1);
            return true;
        }


        public async Task<bool> rienTutor3()
        {

            if (!isQuestCompleted(21012))
            {
                await Pink("You must complete the quest before proceeding to the next map..");
                return false;
            }
            await playPortalSound();
            await warp(140090400, 1);
            return true;
        }


        public async Task<bool> rienTutor4()
        {

            if (!isQuestCompleted(21013))
            {
                await Pink("You must complete the quest before proceeding to the next map..");
                return false;
            }
            await playPortalSound();
            await warp(140090500, 1);
            return true;
        }


        public async Task<bool> rienTutor5()
        {

            await talkGuide("前面不远就是村子了。我先走一步了，还有一些需要整理的东西。战神，慢慢过来就行。");
            await blockPortal();
            return false;
        }


        public async Task<bool> rienTutor6()
        {

            await removeGuide();
            await blockPortal();
            return true;
        }


        public async Task<bool> rienTutor7()
        {

            if (getJob() == Job.LEGEND && !isQuestCompleted(21014))
            {
                await showInfoText("The town of Rien is to the right. Take the portal on the right and go into town to meet Lilin.");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(140010100, 2);
                return true;
            }
        }


        public async Task<bool> rienTutor8()
        {

            if (getJob() == Job.LEGEND)
            {
                if (isQuestStarted(21015))
                {
                    await showInfoText("You must exit to the right in order to find Murupas.");
                    return false;
                }
                else if (isQuestStarted(21016))
                {
                    await showInfoText("You must exit to the right in order to find Murupias.");
                    return false;
                }
                else if (isQuestStarted(21017))
                {
                    await showInfoText("You must exit to the right in order to find MuruMurus.");
                    return false;
                }
            }
            await playPortalSound();
            await warp(140010000, 2);
            return true;
        }


        public async Task<bool> rnj12_in()
        {

            await playPortalSound();
            await warp(926100401, 0); //next
            return true;
        }

        [ScriptTag(["PQ_Magatia"])]
        public async Task<bool> rnj1_out()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg2") == 1)
            {
                await playPortalSound();
                await warp(926100100, 0); //next
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> rnj1_pt00()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg1") == 1)
            {
                await playPortalSound();
                await warp(926100001, 0); //next
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> rnj2_out()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg3") == 3)
            {
                await playPortalSound();
                await warp(926100200, 0); //next
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> rnj3_in0()
        {

            if (getMap().getReactorByName("rnj3_out1")?.getState() == 1)
            {
                await playPortalSound();
                await warp(926100201, 0);
                return true;
            }
            else
            {
                await Pink("The door is not opened yet.");
                return false;
            }
        }


        public async Task<bool> rnj3_in1()
        {

            if (getMap().getReactorByName("rnj3_out2")?.getState() == 1)
            {
                await playPortalSound();
                await warp(926100202, 0);
                return true;
            }
            else
            {
                await Pink("The door is not opened yet.");
                return false;
            }
        }


        public async Task<bool> rnj3_out()
        {

            if (getMap().getReactorByName("rnj3_out3")?.getState() == 1)
            {
                await playPortalSound();
                await warp(926100203, 0); //next
                return true;
            }
            else
            {
                await Pink("The door is not opened yet.");
                return false;
            }
        }

        async Task<bool> Rnj4_RAsync(int reg)
        {
            var eim = GetEventInstanceTrust();
            var area = eim.getIntProperty("statusStg5");

            if ((area >> reg) % 2 == 0)
            {
                area |= (1 << reg);
                eim.setIntProperty("statusStg5", area);

                await playPortalSound();
                await warp(926100301 + reg, 0); //next
                return true;
            }
            else
            {
                await Pink("This room is already being explored.");
                return false;
            }
        }

        public Task<bool> rnj4_r1() => Rnj4_RAsync(0);


        public Task<bool> rnj4_r2() => Rnj4_RAsync(1);


        public Task<bool> rnj4_r3() => Rnj4_RAsync(2);


        public Task<bool> rnj4_r4() => Rnj4_RAsync(3);


        public async Task<bool> rnj5_rp()
        {

            var mapplayer = "stage6_comb" + (getMapId() % 10);
            var eim = GetEventInstanceTrust();

            var comb = eim.getProperty(mapplayer) ?? "";

            var name = getPortal().getName().Substring(2, 4);
            var portalId = int.Parse(name);


            var pRow = (int)Math.Floor(portalId / 10.0);
            var pCol = (portalId % 10);

            if (pCol == int.Parse(comb.Substring(pRow, pRow + 1)))
            {    //climb
                if (pRow < 9)
                {
                    await playPortalSound();
                    await warp(getMapId(), getPortal().getId() + 4);
                }
                else
                {
                    if (eim.getIntProperty("statusStg6") == 0)
                    {
                        eim.setIntProperty("statusStg6", 1);
                        await eim.GiveStageClearRewardAll(6);
                    }

                    await playPortalSound();
                    await warp(getMapId(), 1);
                }

            }
            else
            {    //fail
                await playPortalSound();
                await warp(getMapId(), 2);
            }

            return true;
        }


        public async Task<bool> rnj6_out()
        {

            if (getMap().getReactorByName("rnj6_out")?.getState() == 1)
            {
                await playPortalSound();
                await warp(926100300, 0);
                return true;
            }
            else
            {
                await Pink("The portal is not opened yet.");
                return false;
            }
        }


        public async Task<bool> rnj_201_0()
        {

            if (getMap().getReactorByName("rnj31_out")?.getState() == 1)
            {
                await playPortalSound();
                await warp(926100200, 1);
                return true;
            }
            else
            {
                await Pink("The door is not opened yet.");
                return false;
            }
        }


        public async Task<bool> rnj_202()
        {

            if (getMap().getReactorByName("rnj32_out")?.getState() == 1)
            {
                await playPortalSound();
                await warp(926100200, 2);
                return true;
            }
            else
            {
                await Pink("The door is not opened yet.");
                return false;
            }
        }


        public async Task<bool> rnj_exit()
        {

            await playPortalSound();
            await warp(261000011, 0);
            return true;
        }


        public async Task<bool> s4berserk()
        {

            if (isQuestStarted(6153) && haveItem(4031475))
            {
                var mapobj = await getWarpMap(910500200);
                if (mapobj.countPlayers() == 0)
                {
                    await resetMapObjects(910500200);
                    await playPortalSound();
                    await warp(910500200, "out01");

                    return true;
                }
                else
                {
                    await Pink("Some other player is currently inside.");
                    return false;
                }
            }
            else
            {
                await Pink("A mysterious force won't let you in.");
                return false;
            }
        }


        public async Task<bool> s4berserk_move()
        {

            if (getPlayer().getMap().countMonsters() == 0)
            {
                await playPortalSound();
                await warp(910500200, "out00");
                return true;
            }
            await Pink("You must defeat all the monsters first.");
            return true;
        }


        public async Task<bool> s4common1_exit()
        {

            if (hasItem(4031495))
            {
                await playPortalSound();
                await warp(921100301);
            }
            else
            {
                await playPortalSound();
                await warp(211040100);
            }

            return true;
        }


        public async Task<bool> s4firehawk()
        {

            if (isQuestStarted(6240))
            {
                if ((await getWarpMap(921100200)).countPlayers() == 0)
                {
                    await resetMapObjects(921100200);
                    await playPortalSound();
                    await warp(921100200, 0);

                    return true;
                }
                else
                {
                    await Pink("Some other player is currently inside.");
                    return false;
                }
            }
            else
            {
                await Pink("A mysterious force won't let you in.");
                return false;
            }
        }


        public async Task<bool> s4hitman()
        {

            if (isQuestStarted(6201))
            {
                if ((await getWarpMap(910200000)).countPlayers() == 0)
                {
                    await resetMapObjects(910200000);
                    await playPortalSound();
                    await warp(910200000, 0);

                    return true;
                }
                else
                {
                    await Pink("Some other player is currently inside.");
                    return false;
                }
            }

            await Pink("A mysterious force won't let you in.");
            return false;
        }


        public async Task<bool> s4iceeagle()
        {

            if (isQuestStarted(6242))
            {
                if ((await getWarpMap(921100210)).countPlayers() == 0)
                {
                    await resetMapObjects(921100210);
                    await playPortalSound();
                    await warp(921100210, 0);

                    return true;
                }
                else
                {
                    await Pink("Some other player is currently inside.");
                    return false;
                }
            }
            else
            {
                await Pink("A mysterious force won't let you in.");
                return false;
            }
        }


        public async Task<bool> s4mind_end()
        {

            if (!GetEventInstanceTrust().isEventCleared())
            {
                await Pink("You have to clear this mission before entering this portal.");
                return false;
            }
            else
            {
                if (isQuestStarted(6410))
                {
                    await setQuestProgress(6410, 6411, "p2");
                }

                await playPortalSound();
                await warp(925010400);
                return true;
            }
        }


        public async Task<bool> s4nest()
        {

            if (isQuestStarted(6241) || isQuestStarted(6243))
            {
                if ((await getWarpMap(924000100)).countPlayers() == 0)
                {
                    await resetMapObjects(924000100);
                    await playPortalSound();
                    await warp(924000100, 0);

                    return true;
                }
                else
                {
                    await Pink("Some other player is currently inside.");
                    return false;
                }
            }

            await Pink("A mysterious force won't let you in.");
            return false;
        }


        public async Task<bool> s4resurrection()
        {

            if (haveItem(4001108))
            {
                var map = await getWarpMap(923000100);
                if (map.getAllPlayers().Count == 0)
                {
                    await map.resetMapObjects ();
                    await playPortalSound();
                    await warp(map.Id, 0);

                    return true;
                }
                else
                {
                    await Pink("Some other player is currently inside.");
                    return false;
                }
            }

            await Pink("A mysterious force won't let you in.");
            return false;
        }


        public async Task<bool> s4resur_enter()
        {

            if (isQuestStarted(6134))
            {
                await playPortalSound();
                await warp(922020000, 0);
                return true;
            }

            await Pink("A mysterious force won't let you in.");
            return false;
        }


        public async Task<bool> s4resur_out()
        {

            if (isQuestStarted(6134))
            {
                if (canHold(4031448))
                {
                    await gainItem(4031448, 1);
                    await playPortalSound();
                    await warp(220070400, 3);

                    return true;
                }
                else
                {
                    await Pink("Make room on your ETC to receive the quest item.");
                    return false;
                }
            }
            else
            {
                await playPortalSound();
                await warp(220070400, 3);
                return true;
            }
        }


        public async Task<bool> s4rush()
        {

            if (isQuestStarted(6110))
            {
                await playPortalSound();
                await warp(910500100, 0);
                return true;
            }
            else
            {
                await Pink("A mysterious force won't let you in.");
                return false;
            }
        }


        public async Task<bool> s4ship_out()
        {

            var exit = GetEventInstanceTrust().getIntProperty("canLeave");
            if (exit == 0)
            {
                await Pink("You have to wait one minute before you can leave this place.");
                return false;
            }
            else if (exit == 2)
            {
                await playPortalSound();
                await warp(912010200);
                return true;
            }
            else
            {
                await playPortalSound();
                await warp(120000101);
                return true;
            }
        }


        public async Task<bool> s4super_out()
        {

            var exit = GetEventInstanceTrust().getIntProperty("canLeave");
            if (exit == 0)
            {
                await Pink("You have to wait one minute before you can leave this place.");
                return false;
            }
            else if (exit == 2)
            {
                await playPortalSound();
                await warp(912010200);
                return true;
            }
            else
            {
                await playPortalSound();
                await warp(120000101);
                return true;
            }
        }


        public async Task<bool> s4tornado_enter()
        {

            if (isQuestStarted(6230) || isQuestStarted(6231) || haveItem(4001110))
            {
                if ((await getWarpMap(922020200)).countPlayers() == 0)
                {
                    await resetMapObjects(922020200);
                    await playPortalSound();
                    await warp(922020200, 0);

                    return true;
                }
                else
                {
                    await Pink("Some other player is currently inside.");
                    return false;
                }
            }

            return false;
        }


        public async Task<bool> secretDoor()
        {

            if (isQuestCompleted(3360))
            {
                await playPortalSound();
                await warp(261030000, "sp_" + ((getMapId() == 261010000) ? "jenu" : "alca"));
                return true;
            }
            else if (isQuestStarted(3360))
            {
                await openNpc(2111024);
                return false;
            }
            else
            {
                await Pink("门锁住了.");
                return false;
            }
        }


        public async Task<bool> secretgate1_open()
        {

            if (getPlayer().getMap().getReactorByName("secretgate1")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000611, 1);
                return true;
            }
            else
            {
                await Pink("This door is closed.");
                return false;
            }
        }


        public async Task<bool> secretgate2_open()
        {

            if (getPlayer().getMap().getReactorByName("secretgate2")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000631, 1);
                return true;
            }
            else
            {
                await Pink("This door is closed.");
                return false;
            }
        }


        public async Task<bool> secretgate3_open()
        {

            if (getPlayer().getMap().getReactorByName("secretgate3")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000641, 1);
                return true;
            }
            else
            {
                await Pink("This door is closed.");
                return false;
            }
        }


        public async Task<bool> skyrom()
        {

            if (isQuestStarted(3935) && !haveItem(4031574, 1))
            {
                if ((await getWarpMap(926000010)).countPlayers() == 0)
                {
                    await playPortalSound();
                    await warp(926000010, 0);
                    return true;
                }
                else
                {
                    await Pink("Someone is already trying this map.");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        public async Task<bool> Spacegaga_out0()
        {

            var eim = GetEventInstanceTrust();
            var fc = eim.getIntProperty("falls");

            if (fc >= 3)
            {
                await playPortalSound();
                await warp(922240200, 0);
            }
            else
            {
                eim.setIntProperty("falls", fc + 1);
                await playPortalSound();
                await warp(getPlayer().getMapId(), 0);
            }

            return true;
        }


        public async Task<bool> Spacegaga_out1()
        {

            var eim = GetEventInstanceTrust();
            var fc = eim.getIntProperty("falls");

            if (fc >= 3)
            {
                await playPortalSound();
                await warp(922240200, 0);
            }
            else
            {
                eim.setIntProperty("falls", fc + 1);
                await playPortalSound();
                await warp(getPlayer().getMapId(), 0);
            }

            return true;
        }


        public async Task<bool> Spacegaga_out2()
        {

            var eim = GetEventInstanceTrust();
            var fc = eim.getIntProperty("falls");

            if (fc >= 3)
            {
                await playPortalSound();
                await warp(922240200, 0);
            }
            else
            {
                eim.setIntProperty("falls", fc + 1);
                await playPortalSound();
                await warp(getPlayer().getMapId(), 0);
            }

            return true;
        }


        public async Task<bool> Spacegaga_out3()
        {

            var eim = GetEventInstanceTrust();
            var fc = eim.getIntProperty("falls");

            if (fc >= 3)
            {
                await playPortalSound();
                await warp(922240200, 0);
            }
            else
            {
                eim.setIntProperty("falls", fc + 1);
                await playPortalSound();
                await warp(getPlayer().getMapId(), 0);
            }

            return true;
        }


        public async Task<bool> space_return()
        {

            await playPortalSound();
            await warp(getPlayer().getSavedLocation("EVENT"));
            return true;
        }


        public async Task<bool> speargate_open()
        {

            if (getPlayer().getMap().getReactorByName("speargate")?.getState() == 4)
            {
                await playPortalSound();
                await warp(990000401, 0);
                return true;
            }
            else
            {
                await Pink("This way forward is not open yet.");
                return false;
            }
        }


        public async Task<bool> stageBogo()
        {

            await playPortalSound();
            await warp(670010800, 0);
            return true;
        }


        public async Task<bool> statuegate_open()
        {

            if (getPlayer().getMap().getReactorByName("statuegate")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000301, 0);
                return true;
            }
            else
            {
                await Pink("The gate is closed.");
                return false;
            }
        }


        public async Task<bool> stonegate_open()
        {

            if (getPlayer().getMap().getReactorByName("stonegate")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000430, 0);
                return true;
            }
            else
            {
                await Pink("The door is still blocked.");
                return false;
            }
        }


        public async Task<bool> subway_in2()
        {

            await playPortalSound();
            await warp(103000101, 3);
            return true;
        }


        public async Task<bool> tamepig_out2()
        {

            if (!(haveItem(4031507, 5) && haveItem(4031508, 5) && isQuestStarted(6002)))
            {
                await removeAll(4031507);
                await removeAll(4031508);
            }

            var pCount = getPlayer().countItem(4031507);
            var rCount = getPlayer().countItem(4031508);

            if (pCount > 5)
            {
                await gainItem(4031507, -1 * (pCount - 5));
            }
            if (rCount > 5)
            {
                await gainItem(4031508, -1 * (rCount - 5));
            }

            await playPortalSound();
            await warp(230000003, "out00");
            return true;
        }


        public async Task<bool> TD_Boss_enter()
        {

            var stage = (int)((Math.Floor(getMapId() / 100.0)) % 10) - 1;
            var em = GetEventManager<PartyQuestEventManager>("TD_Battle" + stage);
            if (em == null)
            {
                await Pink("TD Battle " + stage + " encountered an unexpected error and is currently unavailable.");
                return false;
            }

            var r = await em.StartInstance(getPlayer());
            switch (r)
            {
                case CreateInstanceResult.Success:
                    await playPortalSound();
                    return true;
                case CreateInstanceResult.RequiredParty:
                    await Pink("You are currently not in a party, create one to attempt the boss.");
                    return false;
                case CreateInstanceResult.RequiredLeader:
                    await Pink("Your party leader must enter the portal to start the battle.");
                    return false;
                case CreateInstanceResult.Requirement:
                    await Pink("Your party must consist of at least 2 players to attempt the boss.");
                    return false;
                case CreateInstanceResult.LobbyLimited:
                    await Pink("The battle against the boss has already begun, so you may not enter this place yet.");
                    return false;
                default:
                    return false;
            }
        }


        public async Task<bool> TD_chat_enter()
        {

            await openNpc(2083006);
            return false;
        }


        public async Task<bool> TD_MC_Egate()
        {

            await playPortalSound();
            await warp(106021300, 1);
            return true;
        }


        public async Task<bool> TD_MC_enterboss1()
        {

            var questProgress = getQuestProgressInt(2330, 3300005) + getQuestProgressInt(2330, 3300006) + getQuestProgressInt(2330, 3300007); //3 Yetis

            if (isQuestStarted(2330) && questProgress < 3)
            {
                await openNpc(1300013);
            }
            else
            {
                await playPortalSound();
                await warp(106021401, 1);
            }

            return true;
        }


        public async Task<bool> TD_MC_enterboss2()
        {

            if (isQuestCompleted(2331))
            {
                await openNpc(1300013);
                return false;
            }

            if (isQuestCompleted(2333) && isQuestStarted(2331) && !hasItem(4001318))
            {
                await Pink("玉玺丢失了？嗯，不用担心！凯文会帮您保密。");
                if (canHold(4001318))
                {
                    await gainItem(4001318, 1);
                }
                else
                {
                    await Pink("嘿，你背包空间已经满了，如何拿取蘑菇王国玉玺？");
                }
            }

            if (isQuestCompleted(2333))
            {
                await playPortalSound();
                await warp(106021600, 1);
                return true;
            }
            else if (isQuestStarted(2332) && hasItem(4032388))
            {
                await forceCompleteQuest(2332, 1300002);
                await Pink("找到了公主！");
                await giveCharacterExp(4400, getPlayer());

                var em = GetEventManager(nameof(MK_PrimeMinister));
                var r = await em.StartInstance(getPlayer());
                if (r == CreateInstanceResult.Success)
                {
                    await playPortalSound();
                    return true;
                }
                else
                {
                    await Pink(em.HandleCreateInstanceResult(r, c) ?? "");
                    return false;
                }
            }
            else if (isQuestStarted(2333) || (isQuestCompleted(2332) && !isQuestStarted(2333)))
            {
                var em = GetEventManager(nameof(MK_PrimeMinister));

                var r = await em.StartInstance(getPlayer());
                if (r == CreateInstanceResult.Success)
                {
                    await playPortalSound();
                    return true;
                }
                else
                {
                    await Pink(em.HandleCreateInstanceResult(r, c) ?? "");
                    return false;
                }
            }
            else
            {
                await Pink("门似乎已经被锁住了，需要找到开启门的钥匙……");
                return false;
            }
        }


        public async Task<bool> TD_MC_first()
        {

            if (isQuestCompleted(2260) ||
        isQuestStarted(2300) || isQuestCompleted(2300) ||
        isQuestStarted(2301) || isQuestCompleted(2301) ||
        isQuestStarted(2302) || isQuestCompleted(2302) ||
        isQuestStarted(2303) || isQuestCompleted(2303) ||
        isQuestStarted(2304) || isQuestCompleted(2304) ||
        isQuestStarted(2305) || isQuestCompleted(2305) ||
        isQuestStarted(2306) || isQuestCompleted(2306) ||
        isQuestStarted(2307) || isQuestCompleted(2307) ||
        isQuestStarted(2308) || isQuestCompleted(2308) ||
        isQuestStarted(2309) || isQuestCompleted(2309) ||
        isQuestStarted(2310) || isQuestCompleted(2310))
            {
                await playPortalSound();
                await warp(106020000, 0);
                return true;
            }
            await Pink("A strange force is blocking you from entering.");
            return false;
        }


        public async Task<bool> TD_MC_jump()
        {

            await playPortalSound();
            await warp(106020501, 0);
            return true;
        }


        public async Task<bool> TD_neo_inTree()
        {

            var nex = getEventManager("GuardianNex") as SoloEventManager;
            if (nex == null)
            {
                await Pink("Guardian Nex challenge encountered an error and is unavailable.");
                return false;
            }

            int[] quests = [3719, 3724, 3730, 3736, 3742, 3748];
            int[] mobs = [7120100, 7120101, 7120102, 8120100, 8120101, 8140510];

            for (var i = 0; i < quests.Length; i++)
            {
                if (isQuestActive(quests[i]))
                {
                    if (getQuestProgressInt(quests[i], mobs[i]) != 0)
                    {
                        await Pink("You already faced Nex. Complete your mission.");
                        return false;
                    }

                    if (await nex.StartInstance(getPlayer(), lobbyId: i) != CreateInstanceResult.Success)
                    {
                        await Pink("Someone is already challenging Nex. Wait for them to finish before you enter.");
                        return false;
                    }
                    else
                    {
                        await playPortalSound();
                        return true;
                    }
                }
            }

            await Pink("A mysterious force won't let you in.");
            return false;
        }


        public async Task<bool> templeenter()
        {

            await cancelItem(2210016);
            await playPortalSound();
            await warp(270000100, "out00");
            return true;
        }

        // Map: 260010401
        public async Task<bool> thief_in1()
        {

            // unexpected warp condition noticed thanks to IxianMace

            await openNpc(2103008);
            return false;
        }


        public async Task<bool> timeQuest()
        {

            var mapid = getPlayer().getMapId();
            await playPortalSound();
            var map = (mapid - 270010000) / 100;
            //await Pink(map + " " + isQuestCompleted(3534));
            if (map < 5 && isQuestCompleted(3500 + map))
            {
                await warp(mapid + 10, "out00");
            }
            else if (map == 5 && isQuestCompleted(3502 + map))
            {
                await warp(270020000, "out00");
            }
            else if (map > 100 && map < 105 && isQuestCompleted(3407 + map))
            {
                await warp(mapid + 10, "out00");
            }
            else if (map == 105 && isQuestCompleted(3514))
            {
                await warp(270030000, "out00");
            }
            else if (map > 200 && map < 205 && isQuestCompleted(3314 + map))
            {
                await warp(mapid + 10, "out00");
            }
            else if (map == 205 && isQuestCompleted(3519))
            {
                await warp(270040000, "out00");
            }
            else if (map == 300 && (haveItem(4032002) || isQuestCompleted(3522)))
            {
                await warp(270040100, "out00");
            }
            else
            {
                if (map > 200)
                {
                    await Pink("随着时间开始变得异常流动，你被传送回到了一个安全的空间。");
                    await warp(270030000, "in00");
                }
                else if (map > 100)
                {
                    await Pink("随着时间开始变得异常流动，你被传送回到了一个安全的空间。");
                    await warp(270020000, "in00");
                }
                else
                {
                    await Pink("随着时间开始变得异常流动，你被传送回到了一个安全的空间。");
                    await warp(270010000, "in00");
                }
            }
            return true;
        }


        public async Task<bool> tristanEnter()
        {

            if (isQuestCompleted(2238))
            {
                await playPortalSound();
                await warp(105100101, "in00");
                return true;
            }
            else
            {
                await Pink("A mysterious force won't let you in.");
                return false;
            }
        }


        public async Task<bool> tutoChatNPC()
        {

            if (hasLevel30Character())
            {
                await openNpc(2007);
            }
            await blockPortal();
            return true;
        }


        public async Task<bool> tutorHelper()
        {

            await spawnGuide();
            await talkGuide("Welcome to Maple World! I'm Mimo. I'm in charge of guiding you until you reach Lv. 10 and become a Knight-In-Training. Double-click for further information!");
            await blockPortal();
            return true;
        }


        public async Task<bool> tutorialNPC()
        {

            if (getPlayer().getLevel() <= 10 && getPlayer().getJob().getId() == 0)
            {
                var m = getPlayer().getMap().getId();
                var npcid = 0;

                if (m == 120000101)
                { // Navigation Room, The Nautilus
                    npcid = 1090000; // Maybe 1090000?
                }
                else if (m == 102000003)
                { // Warrior's Sanctuary
                    npcid = 1022000;
                }
                else if (m == 103000003)
                { // Thieves' Hideout
                    npcid = 1052001;
                }
                else if (m == 100000201)
                { // Bowman Instructional School
                    npcid = 1012100;
                }
                else if (m == 101000003)
                { // Magic Library
                    npcid = 1032001;
                }

                if (npcid != 0)
                {
                    await openNpc(npcid);
                    return true;
                }
            }
            return false;
        }


        public async Task<bool> tutorMinimap()
        {

            await guideHint(1);
            await blockPortal();
            return true;
        }


        public async Task<bool> tutorquest()
        {

            if (getPlayer().getMapId() == 130030001)
            {
                if (isQuestStarted(20010))
                {
                    await playPortalSound();
                    await warp(130030002, 0);
                    return true;
                }
                else
                {
                    await Pink("Please click on the NPC first to receive a quest.");
                }
            }
            else if (getPlayer().getMapId() == 130030002)
            {
                if (isQuestCompleted(20011))
                {
                    await playPortalSound();
                    await warp(130030003, 0);
                    return true;
                }
                else
                {
                    await Pink("Please complete the required quest before proceeding.");
                }
            }
            else if (getPlayer().getMapId() == 130030003)
            {
                if (isQuestCompleted(20012))
                {
                    await playPortalSound();
                    await warp(130030004, 0);
                    return true;
                }
                else
                {
                    await Pink("Please complete the required quest before proceeding.");
                }
            }
            else if (getPlayer().getMapId() == 130030004)
            {
                if (isQuestCompleted(20013))
                {
                    await playPortalSound();
                    await warp(130030005, 0);
                    return true;
                }
                else
                {
                    await Pink("Please complete the required quest before proceeding.");
                }
            }

            return false;
        }


        public async Task<bool> under30gate()
        {

            if (getPlayer().getLevel() <= 30)
            {
                await playPortalSound();
                await warp(990000640, 1);
                return true;
            }
            else
            {
                await Pink("You cannot proceed past this point.");
                return false;
            }
        }


        public async Task<bool> undodraco()
        {

            await cancelItem(2210016);
            await playPortalSound();
            await warp(240000110, 2);
            return true;
        }


        public async Task<bool> watergate_open()
        {

            if (getPlayer().getMap().getReactorByName("watergate")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000600, 1);
                return true;
            }
            else
            {
                await Pink("This way forward is not open yet.");
            }
            return false;
        }


        public async Task<bool> Zakum03()
        {

            var eim = GetEventInstanceTrust();
            if (!eim.isEventCleared())
            {
                await Pink("你的队伍尚未完成试炼，请先完成奥拉的需求。");
                return false;
            }

            if (eim.gridCheck(getPlayer()) == -1)
            {
                await Pink("你还没有领取战利品，请先与Aura交谈。");
                return false;
            }

            await playPortalSound();
            await warp(211042300);
            return true;
        }


        public async Task<bool> Zakum05()
        {

            if (!(isQuestStarted(100200) || isQuestCompleted(100200)))
            {
                await Pink("你需要得到大师们的准许才能挑战扎昆BOSS,你现在没有资格进入。");
                return false;
            }

            if (!isQuestCompleted(100201))
            {
                await Pink("你必须完成所有试炼任务才有资格进入。");
                return false;
            }

            if (!haveItem(4001017))
            {
                // thanks Conrad for pointing out missing checks for token item and unused reactor
                await Pink("扎昆祭台需要 火焰之眼 ，否则无法召唤扎昆BOSS，请准备好所需物品再来挑战。");
                return false;
            }

            var react = getMap().getReactorById(2118002);
            if (react != null && react.getState() > 0)
            {
                await Pink("入口目前已被封锁，无法进入。");
                return false;
            }

            await playPortalSound();
            await warp(211042400, "west00");
            return true;
        }



    }
}