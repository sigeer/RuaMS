using Application.Core.Client;
using Application.Core.Game.ContiMove;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using Application.Shared.Constants.Map;
using Application.Shared.GameProps;
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
        public Task<bool> _08_xmas_out()
        {

            playPortalSound();
            warp(getMapId() - 2, 0);
            return Task.FromResult(true);
        }


        public Task<bool> advice00()
        {

            showInstruction("您可以使用箭头键移动。", 250, 5);
            return Task.FromResult(true);
        }


        public Task<bool> advice01()
        {

            showInstruction("单击 \r\\#b<Heena>#k", 100, 5);
            return Task.FromResult(true);
        }


        public Task<bool> advice02()
        {

            showInstruction("按下 #e#b[Alt]#k#n to\r\\ 跳跃。", 100, 5);
            return Task.FromResult(true);
        }


        public Task<bool> advice03()
        {

            showInstruction("按方向键 #e#b[Up]#k#n 爬上梯子或绳索", 350, 5);
            return Task.FromResult(true);
        }


        public Task<bool> advice04()
        {

            showInstruction("单击 \r\\#b<Sera>", 100, 5);
            return Task.FromResult(true);
        }


        public Task<bool> advice05()
        {

            showInstruction("按 #e#b[Q]#k#n 打开任务窗口。", 250, 5);
            return Task.FromResult(true);
        }


        public Task<bool> advice06()
        {

            showInstruction("按 #e#b[Up]#k 进入光圈\r\\移动到下一张地图。", 230, 5);
            return Task.FromResult(true);
        }


        public Task<bool> advice07()
        {

            showInstruction("您可以按#e#b[W]#k#n键查看世界地图。", 350, 5);
            return Task.FromResult(true);
        }


        public Task<bool> advice08()
        {

            showInstruction("您可以通过按#e#b[S]#k#n键来查看角色的属性面板。", 350, 5);
            return Task.FromResult(true);
        }


        public Task<bool> advice09()
        {

            showInstruction("同时按下方向键的#n#e#b[Alt]#k和#e#b[Down]#k#n向下跳跃。", 450, 6);
            return Task.FromResult(true);
        }


        public Task<bool> adviceMap()
        {

            showInstruction("按#e#b[Up]#k箭头#n进入光圈移动到下一个地图。", 230, 5);
            return Task.FromResult(true);
        }


        public Task<bool> aMatchMove2()
        {

            playPortalSound();
            warp(getPlayer().getSavedLocation("MIRROR"));
            return Task.FromResult(true);
        }


        public Task<bool> apq00()
        {

            playPortalSound();
            warp(670010300, 0);
            return Task.FromResult(true);
        }


        public Task<bool> apq01()
        {

            playPortalSound();
            warp(670010301, 0);
            return Task.FromResult(true);
        }


        public Task<bool> apq02()
        {

            playPortalSound();
            warp(670010302, 0);
            return Task.FromResult(true);
        }


        public Task<bool> apq1()
        {

            playPortalSound();
            warp(670010400, 0);
            return Task.FromResult(true);
        }


        public Task<bool> apq2()
        {

            playPortalSound();
            warp(670010500, 0);
            return Task.FromResult(true);
        }


        public Task<bool> apq3()
        {

            playPortalSound();
            warp(670010600, 0);
            return Task.FromResult(true);
        }


        public Task<bool> apqClosed()
        {

            message("大门还没有打开。");
            return Task.FromResult(false);
        }


        public Task<bool> apqDoor()
        {

            var name = getPortal().getName().Substring(2, 4);
            var gate = getPlayer().getMap().getReactorByName("gate" + name);
            if (gate != null && gate.getState() == 4)
            {
                playPortalSound();
                warp(670010600, "gt" + name + "PIB");
                return Task.FromResult(true);
            }
            else
            {
                message("大门还没有打开。");
                return Task.FromResult(false);
            }
        }


        public Task<bool> aqua_pq_boss_0()
        {

            playPortalSound();
            warp(230040420, 0);
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorAloneX()
        {

            playPortalSound();
            warp(914000100, 1);
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorArrow0()
        {

            blockPortal();
            if (containsAreaInfo(21002, "arr0=o"))
            {
                return Task.FromResult(false);
            }
            updateAreaInfo(21002, "arr0=o;mo1=o;mo2=o;mo3=o");
            showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow3");
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorArrow1()
        {

            blockPortal();
            if (containsAreaInfo(21002, "arr1=o"))
            {
                return Task.FromResult(false);
            }
            updateAreaInfo(21002, "normal=o;arr0=o;arr1=o;mo1=o;mo2=o;mo3=o;mo4=o");
            showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow1");
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorArrow2()
        {

            blockPortal();
            if (containsAreaInfo(21002, "arr2=o"))
            {
                return Task.FromResult(false);
            }
            updateAreaInfo(21002, "normal=o;arr0=o;arr1=o;arr2=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow1");
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorArrow3()
        {

            blockPortal();
            if (containsAreaInfo(21002, "arr3=o"))
            {
                return Task.FromResult(false);
            }
            updateAreaInfo(21002, "cmd=o;normal=o;arr0=o;arr1=o;arr2=o;arr3=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow1");
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorGuide0()
        {

            blockPortal();
            if (containsAreaInfo(21002, "normal=o"))
            {
                return Task.FromResult(false);
            }
            showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialGuide1");
            message("要对怪物使用普通攻击，请按Ctrl键。");
            updateAreaInfo(21002, "normal=o;arr0=o;mo1=o;mo2=o;mo3=o");
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorGuide1()
        {

            blockPortal();
            if (containsAreaInfo(21002, "chain=o"))
            {
                return Task.FromResult(false);
            }
            showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialGuide2");
            message("您可以通过多次按下Ctrl键来使用连续攻击。");
            updateAreaInfo(21002, "normal=o;arr0=o;arr1=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorGuide2()
        {

            blockPortal();
            if (containsAreaInfo(21002, "cmd=o"))
            {
                return Task.FromResult(false);
            }
            showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialGuide3");
            message("在连续攻击后，您可以通过同时按下箭头键和攻击键来使用命令攻击。");
            updateAreaInfo(21002, "cmd=o;normal=o;arr0=o;arr1=o;arr2=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorLost()
        {

            blockPortal();
            if (containsAreaInfo(21002, "fin=o"))
            {
                return Task.FromResult(false);
            }
            updateAreaInfo(21002, "cmd=o;normal=o;arr0=o;arr1=o;arr2=o;arr3=o;fin=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            showIntro("Effect/Direction1.img/aranTutorial/ClickChild");
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorMono0()
        {

            blockPortal();
            if (containsAreaInfo(21002, "mo1=o"))
            {
                return Task.FromResult(false);
            }
            updateAreaInfo(21002, "mo1=o");
            showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/legendBalloon1");
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorMono1()
        {

            blockPortal();
            if (containsAreaInfo(21002, "mo2=o"))
            {
                return Task.FromResult(false);
            }
            playSound("Aran/balloon");
            updateAreaInfo(21002, "mo1=o;mo2=o");
            showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/legendBalloon2");
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorMono2()
        {

            blockPortal();
            if (containsAreaInfo(21002, "mo3=o"))
            {
                return Task.FromResult(false);
            }
            updateAreaInfo(21002, "mo1=o;mo2=o;mo3=o");
            showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/legendBalloon3");
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorMono3()
        {

            blockPortal();
            if (containsAreaInfo(21002, "mo4=o"))
            {
                return Task.FromResult(false);
            }
            updateAreaInfo(21002, "normal=o;arr0=o;mo1=o;mo2=o;mo3=o;mo4=o");
            showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/legendBalloon6");
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorOut1()
        {

            if (isQuestStarted(21000))
            {
                //lol nexon does this xD
                teachSkill(20000017, 0, -1, -1);
                teachSkill(20000018, 0, -1, -1);
                //nexon sends updatePlayerStats Stat.AVAILABLESP 0
                teachSkill(20000017, 1, 0, -1);
                teachSkill(20000018, 1, 0, -1);
                //actually nexon does enableActions here :P
                playPortalSound();
                warp(914000200, 1);
                return Task.FromResult(true);
            }
            else
            {
                message("你只有在接受你右边的雅典娜·皮尔斯的任务后才能退出。");
                return Task.FromResult(false);
            }
        }


        public Task<bool> aranTutorOut2()
        {

            //lol nexon does this xD
            teachSkill(20000014, 0, -1, -1);
            teachSkill(20000015, 0, -1, -1);
            //nexon sends updatePlayerStats Stat.AVAILABLESP 0
            teachSkill(20000014, 1, 0, -1);
            teachSkill(20000015, 1, 0, -1);
            //actually nexon does enableActions here :P
            playPortalSound();
            warp(914000210, 1);
            return Task.FromResult(true);
        }


        public Task<bool> aranTutorOut3()
        {

            //lol nexon does this xD
            teachSkill(20000016, 0, -1, -1);
            //nexon sends updatePlayerStats Stat.AVAILABLESP 0
            teachSkill(20000016, 1, 0, -1);
            //actually nexon does enableActions here :P
            playPortalSound();
            warp(914000220, 1);
            return Task.FromResult(true);
        }


        public Task<bool> ariantMout()
        {

            playPortalSound();
            warp(980010020, 0);
            return Task.FromResult(true);
        }


        public Task<bool> ariantMout2()
        {

            playPortalSound();
            warp(980010000, 0);
            return Task.FromResult(true);
        }


        public Task<bool> ariant_Agit()
        {

            if (isQuestCompleted(3928) && isQuestCompleted(3931) && isQuestCompleted(3934))
            {
                playPortalSound();
                warp(260000201, 1);
                return Task.FromResult(true);
            }
            else
            {
                message("仅限沙匪队成员进入。");
                return Task.FromResult(false);
            }
        }


        public Task<bool> ariant_castle()
        {

            if (getPlayer().haveItem(4031582) == true)
            {
                playPortalSound();
                warp(260000301, 5);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "只有持有宫殿通行证才能进入。");
                return Task.FromResult(false);
            }
        }


        static bool isTigunMorphed(Player chr)
        {
            return chr.getBuffSource(BuffStat.MORPH) == 2210005;
        }
        public Task<bool> ariant_queens()
        {

            if (isTigunMorphed(getPlayer()))
            {
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(260000300, 7);
                message("你，入侵者！你没有权限在宫殿中随意走动！滚出去！！");
                return Task.FromResult(true);
            }
        }


        public Task<bool> babyPigOut()
        {

            if (isQuestCompleted(22015))
            {
                playPortalSound();
                warp(100030300, 2);
            }
            else
            {
                playerMessage(5, "请救救小猪！");//not gms like
            }
            return Task.FromResult(true);
        }


        public Task<bool> balogTemple()
        {

            playPortalSound();
            warp(105100000, 2);
            return Task.FromResult(true);
        }


        public Task<bool> balog_end()
        {

            if (!canHold(4001261, 1))
            {
                playerMessage(5, "请给装备栏腾出至少1个空格子。");
                return Task.FromResult(false);
            }
            gainItem(4001261, 1);
            playPortalSound();
            warp(105100100, 0);
            return Task.FromResult(true);
        }


        public Task<bool> bedroom_out()
        {

            if (isQuestStarted(2570))
            {
                playPortalSound();
                warp(120000101, 0);
                return Task.FromResult(true);
            }
            earnTitle("你似乎还有一些未完成的事情，我从你的眼神中可以看出来。等等……不，那些只是眼屎。");
            return Task.FromResult(false);
        }

        public Task<bool> captinsg00()
        {

            if (!haveItem(4000381))
            {
                playerMessage(5, "You do not have White Essence.");
                return Task.FromResult(false);
            }
            else
            {
                var em = getEventManager("LatanicaBattle") as PartyQuestEventManager;
                if (em == null)
                {
                    throw new BusinessNotsupportException($"Event: LatanicaBattle");
                }

                var chrParty = getParty();
                if (chrParty == null)
                {
                    playerMessage(5, "You are currently not in a party, create one to attempt the boss.");
                    return Task.FromResult(false);
                }
                else if (!isLeader())
                {
                    playerMessage(5, "Your party leader must enter the portal to start the battle.");
                    return Task.FromResult(false);
                }
                else
                {
                    var eli = em.getEligibleParty(chrParty);
                    if (eli.Count > 0)
                    {
                        if (!em.StartPQInstance(getPlayer(), eli))
                        {
                            playerMessage(5, "The battle against the boss has already begun, so you may not enter this place yet.");
                            return Task.FromResult(false);
                        }
                    }
                    else
                    {  //this should never appear
                        playerMessage(5, "You cannot start this battle yet, because either your party is not in the range size, some of your party members are not eligible to attempt it or they are not in this map. If you're having trouble finding party members, try Party Search.");
                        return Task.FromResult(false);
                    }

                    playPortalSound();
                    return Task.FromResult(true);
                }
            }
        }


        public Task<bool> catPriest_map()
        {

            playPortalSound();
            warp(925000000, 2);
            return Task.FromResult(true);
        }


        public Task<bool> contactDragon()
        {

            playPortalSound();
            warp(900090100, 0);
            return Task.FromResult(true);
        }


        public Task<bool> curseforest()
        {

            if (isQuestStarted(2224) || isQuestStarted(2226) || isQuestCompleted(2227))
            {
                var hourDay = getHourOfDay();
                if (!((hourDay >= 0 && hourDay < 7) || hourDay >= 17))
                {
                    getPlayer().dropMessage(5, "You cannot access this area right now.");
                    return Task.FromResult(false);
                }
                else
                {
                    playPortalSound();
                    warp(isQuestCompleted(2227) ? 910100001 : 910100000, "out00");
                    return Task.FromResult(true);
                }
            }

            getPlayer().dropMessage(5, "You cannot access this area.");
            return Task.FromResult(false);
        }


        public Task<bool> davy2_hd1()
        {

            var eim = GetEventInstanceTrust() ?? throw new BusinessOutOfInstance();
            var level = eim.getIntProperty("level");
            if (eim.getProperty("stage2b") == "0")
            {
                getMap(925100202).spawnAllMonstersFromMapSpawnList(level, true);
                eim.setProperty("stage2b", "1");
            }

            playPortalSound();
            warp(925100202, 0);
            return Task.FromResult(true);
        }


        public Task<bool> davy3_hd1()
        {

            var eim = GetEventInstanceTrust() ?? throw new BusinessOutOfInstance();
            var level = eim.getIntProperty("level");
            if (eim.getProperty("stage3b") == "0")
            {
                getMap(925100302).spawnAllMonstersFromMapSpawnList(level, true);
                eim.setProperty("stage3b", "1");
            }

            playPortalSound();
            warp(925100302, 0);
            return Task.FromResult(true);
        }


        public Task<bool> davy_next0()
        {

            var eim = GetEventInstanceTrust() ?? throw new BusinessOutOfInstance();
            if (getMap().countMonsters() == 0 && passedGrindMode(getMap(), eim))
            {
                playPortalSound();
                warp(925100100, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
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

        public Task<bool> davy_next1()
        {

            try
            {
                var eim = GetEventInstanceTrust();
                if (eim != null && eim.getProperty("stage2") == "3")
                {
                    playPortalSound();
                    warp(925100200, 0); //next
                    return Task.FromResult(true);
                }
                else
                {
                    playerMessage(5, "The portal is not opened yet.");
                    return Task.FromResult(false);
                }
            }
            catch (Exception e)
            {
                playerMessage(5, "Error: " + e.Message);
            }

            return Task.FromResult(false);
        }


        public Task<bool> davy_next2()
        {

            if (getMap().countMonsters() == 0 && passedGrindMode(getMap(), GetEventInstanceTrust() ?? throw new BusinessOutOfInstance()))
            {
                playPortalSound();
                warp(925100300, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> davy_next3()
        {

            if (getMap().countMonsters() == 0 && passedGrindMode(getMap(), GetEventInstanceTrust() ?? throw new BusinessOutOfInstance()))
            {
                playPortalSound();
                warp(925100400, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> davy_next4()
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
                    Monster boss;
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

                    boss.changeDifficulty(level, true);
                    getMap(925100500).spawnMonsterOnGroundBelow(boss, new Point(777, 140));
                    eim.setProperty("spawnedBoss", "true");
                }

                playPortalSound();
                warp(925100500, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> Depart_goBack00()
        {

            playPortalSound();
            warp(getPlayer().getMap().getId() - 10, "left00");
            return Task.FromResult(true);
        }


        public Task<bool> Depart_goBack01()
        {

            playPortalSound();
            warp(getPlayer().getMap().getId() - 10, "left01");
            return Task.FromResult(true);
        }


        public Task<bool> Depart_goFoward0()
        {

            var mapid = getPlayer().getMap().getId();

            if (mapid == 103040410 && isQuestCompleted(2287))
            {
                playPortalSound();
                warp(103040420, "right00");
                return Task.FromResult(true);
            }
            else if (mapid == 103040420 && isQuestCompleted(2288))
            {
                playPortalSound();
                warp(103040430, "right00");
                return Task.FromResult(true);
            }
            else if (mapid == 103040410 && isQuestStarted(2287))
            {
                playPortalSound();
                warp(103040420, "right00");
                return Task.FromResult(true);
            }
            else if (mapid == 103040420 && isQuestStarted(2288))
            {
                playPortalSound();
                warp(103040430, "right00");
                return Task.FromResult(true);
            }
            else
            {
                if (mapid == 103040440 || mapid == 103040450)
                {
                    playPortalSound();
                    warp(mapid + 10, "right00");
                    return Task.FromResult(true);
                }
                getPlayer().dropMessage(5, "You cannot access this area.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> Depart_goFoward1()
        {

            var mapid = getPlayer().getMap().getId();

            if (mapid == 103040410 && isQuestCompleted(2287))
            {
                playPortalSound();
                warp(103040420, "right01");
                return Task.FromResult(true);
            }
            else if (mapid == 103040420 && isQuestCompleted(2288))
            {
                playPortalSound();
                warp(103040430, "right01");
                return Task.FromResult(true);
            }
            else if (mapid == 103040410 && isQuestStarted(2287))
            {
                playPortalSound();
                warp(103040420, "right01");
                return Task.FromResult(true);
            }
            else if (mapid == 103040420 && isQuestStarted(2288))
            {
                playPortalSound();
                warp(103040430, "right01");
                return Task.FromResult(true);
            }
            else
            {
                if (mapid == 103040440 || mapid == 103040450)
                {
                    playPortalSound();
                    warp(mapid + 10, "right01");
                    return Task.FromResult(true);
                }
                getPlayer().dropMessage(5, "You cannot access this area.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> Depart_ToKerning()
        {

            var em = GetEventManager<SoloEventManager>("KerningTrain");
            if (!em.startInstance(getPlayer()))
            {
                message("The passenger wagon is already full. Try again a bit later.");
                return Task.FromResult(false);
            }

            playPortalSound();
            return Task.FromResult(true);
        }


        public Task<bool> Depart_TopFloor()
        {

            openNpc(1052125); //It is actually suppose to open the npc, because it leads to a boss map
            return Task.FromResult(true);
        }


        public Task<bool> Depart_topOut()
        {

            playPortalSound();
            warp(103040300, 1);
            return Task.FromResult(true);
        }


        public Task<bool> dojang_exit()
        {

            var map = getPlayer().getSavedLocation("MIRROR");
            if (map == -1)
            {
                map = 100000000;
            }

            playPortalSound();
            warp(map);
            return Task.FromResult(true);
        }


        public Task<bool> dojang_next()
        {

            var currwarp = c.CurrentServer.Node.getCurrentTime();

            if (currwarp - getPlayer().getNpcCooldown() < 3000)
            {
                return Task.FromResult(false);
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
                                    var chrlist = getMap(mapId - 100 * i).getAllPlayers();

                                    foreach (var chr in chrlist)
                                    {
                                        for (var j = i; j >= 0; j--)
                                        {
                                            chr.message("You received " + chr.addDojoPointsByMap(mapId - 100 * j) + " training points. Your total training points score is now " + chr.getDojoPoints() + ".");
                                        }

                                        chr.changeMap(restMapId, 0);
                                    }
                                }
                            }
                            else
                            {
                                getPlayer().message("You received " + getPlayer().addDojoPointsByMap(getMapId()) + " training points. Your total training points score is now " + getPlayer().getDojoPoints() + ".");
                                playPortalSound();
                                warp(getPlayer().getMap().getId() + 100, 0);
                            }
                        }
                        else
                        {
                            getPlayer().message("You received " + getPlayer().addDojoPointsByMap(getMapId()) + " training points. Your total training points score is now " + getPlayer().getDojoPoints() + ".");
                            playPortalSound();
                            warp(getPlayer().getMap().getId() + 100, 0);
                        }
                    }
                    else
                    {
                        playPortalSound();
                        warp(925020003, 0);
                        getPlayer().gainExp(2000 * getPlayer().getDojoPoints(), true, true, true);
                    }
                    return Task.FromResult(true);
                }
                else
                {
                    getPlayer().message("The door is not open yet.");
                    return Task.FromResult(false);
                }
            }
            else
            {
                return Task.FromResult(false);
            }
        }


        public Task<bool> dojang_tuto()
        {

            if (getPlayer().getMap().getMonsterById(9300216) != null)
            {
                getPlayer().FinishedDojoTutorial = true;
                getClient().getChannelServer().resetDojo(getPlayer().getMap().getId());
                getClient().getChannelServer().dismissDojoSchedule(getPlayer().getMap().getId(), getParty());
                playPortalSound();
                warp(925020001, 0);
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().message("So Gong: Haha! You're going to run away like a coward? I won't let you get away that easily!");
                return Task.FromResult(false);
            }
        }


        public Task<bool> dojang_up()
        {

            try
            {
                if (getPlayer().getMap().getMonsterById(9300216) != null)
                {
                    goDojoUp();
                    getPlayer().getMap().setReactorState();
                    var stage = (int)Math.Floor(getPlayer().getMapId() / 100.0) % 100;
                    if ((stage - (stage / 6) | 0) == getPlayer().getVanquisherStage() && !MapId.isPartyDojo(getPlayer().getMapId())) // we can also try 5 * stage / 6 | 0 + 1
                    {
                        getPlayer().setVanquisherKills(getPlayer().getVanquisherKills() + 1);
                    }
                }
                else
                {
                    getPlayer().message("There are still some monsters remaining.");
                }
                enableActions();
                return Task.FromResult(true);
            }
            catch (Exception err)
            {
                getPlayer().dropMessage(err.Message);
                return Task.FromResult(false);
            }
        }


        public Task<bool> dracoout()
        {

            playPortalSound();
            warp(240000100, "east00");
            return Task.FromResult(true);
        }



        public Task<bool> dragoneyes()
        {

            if (isQuestCompleted(22012))
            {
                return Task.FromResult(false);
            }
            else
            {
                forceCompleteQuest(22012);
            }
            blockPortal();
            return Task.FromResult(true);
        }


        public Task<bool> dragonNest()
        {

            if (isQuestCompleted(3706))
            {
                playPortalSound();
                warp(240040612, "out00");
                return Task.FromResult(true);
            }
            else if (isQuestStarted(100203) || getPlayer().haveItem(4001094))
            {
                var em = GetEventManager<SoloEventManager>("NineSpirit");
                if (!em.startInstance(getPlayer()))
                {
                    message("There is currently someone in this map, come back later.");
                    return Task.FromResult(false);
                }
                else
                {
                    playPortalSound();
                    return Task.FromResult(true);
                }
            }
            else
            {
                message("A strange force is blocking you from entering.");
                return Task.FromResult(false);
            }
        }

        public Task<bool> elevator()
        {
            var elevator = GetContiMove();
            if (elevator is not Elevator)
            {
                getPlayer().Pink("电梯正在维护。");
                return Task.FromResult(false);
            }

            if (elevator.Enter(getPlayer()))
            {
                playPortalSound();
                return Task.FromResult(true);
            }

            getPlayer().Pink("电梯已经启动。");
            return Task.FromResult(false);
        }

        public Task<bool> eliza_Garden()
        {

            playPortalSound();
            warp(920020000, 2);
            return Task.FromResult(true);
        }


        public Task<bool> end_black()
        {

            playPortalSound();
            warp(120000200, 0);
            return Task.FromResult(true);
        }


        public Task<bool> end_cow()
        {

            if (isQuestStarted(2180) && (hasItem(4031847) || hasItem(4031848) || hasItem(4031849) || hasItem(4031850)))
            {
                if (hasItem(4031850))
                {
                    playPortalSound();
                    warp(120000103);
                    return Task.FromResult(true);
                }
                else
                {
                    getPlayer().dropMessage(5, "Your milk jug is not full...");
                    return Task.FromResult(false);
                }
            }
            else
            {
                playPortalSound();
                warp(120000103);
                return Task.FromResult(true);
            }
        }


        public Task<bool> enterAchter()
        {

            playPortalSound();
            warp(100000201, "out02");
            playPortalSound();
            return Task.FromResult(true);
        }


        public Task<bool> enterBackStreet()
        {

            if (isQuestActive(21747) || isQuestActive(21744) && isQuestCompleted(21745))
            {
                playPortalSound();
                warp(925040000, 0);
                return Task.FromResult(true);
            }
            else
            {
                message("You don't have permission to access this area.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> enterDisguise0()
        {

            if (isQuestStarted(20301) || isQuestStarted(20302) || isQuestStarted(20303) || isQuestStarted(20304) || isQuestStarted(20305))
            {
                if (hasItem(4032179))
                {
                    playPortalSound();
                    warp(130010000, "east00");
                }
                else
                {
                    getPlayer().dropMessage(5, "Due to the lock down you can not enter without a permit.");
                    return Task.FromResult(false);
                }
            }
            else
            {
                playPortalSound();
                warp(130010000, "east00");
            }
            return Task.FromResult(true);
        }


        public Task<bool> enterDisguise1()
        {
            var jobtype = 0;
            if (isQuestStarted(20301) || isQuestStarted(20302) || isQuestStarted(20303) || isQuestStarted(20304) || isQuestStarted(20305))
            {
                var map = getClient().getChannelServer().getMapFactory().getMap(108010600 + (10 * jobtype));
                if (map.countPlayers() > 0)
                {
                    message("Someone else is already searching the area.");
                    return Task.FromResult(false);
                }

                if (haveItem(4032101 + jobtype, 1))
                {
                    message("You have already challenged the Master of Disguise, report your success to the Chief Knight.");
                    return Task.FromResult(false);
                }

                playPortalSound();
                warp(108010600 + (10 * jobtype), "out00");
            }
            else
            {
                playPortalSound();
                warp(130010010, "out00");
            }
            return Task.FromResult(true);
        }


        public Task<bool> enterDisguise2()
        {

            var jobtype = 0;
            if (isQuestStarted(20301) || isQuestStarted(20302) || isQuestStarted(20303) || isQuestStarted(20304) || isQuestStarted(20305))
            {
                var map = getClient().getChannelServer().getMapFactory().getMap(108010600 + (10 * jobtype));
                if (map.countPlayers() > 0)
                {
                    message("Someone else is already searching the area.");
                    return Task.FromResult(false);
                }

                if (haveItem(4032101 + jobtype, 1))
                {
                    message("You have already challenged the Master of Disguise, report your success to the Chief Knight.");
                    return Task.FromResult(false);
                }

                playPortalSound();
                warp(108010600 + (10 * jobtype), "out00");
            }
            else
            {
                playPortalSound();
                warp(130010020, "out00");
            }
            return Task.FromResult(true);
        }


        public Task<bool> enterDisguise3()
        {

            var jobtype = 0;
            if (isQuestStarted(20301) || isQuestStarted(20302) || isQuestStarted(20303) || isQuestStarted(20304) || isQuestStarted(20305))
            {
                var map = getClient().getChannelServer().getMapFactory().getMap(108010600 + (10 * jobtype));
                if (map.countPlayers() > 0)
                {
                    message("Someone else is already searching the area.");
                    return Task.FromResult(false);
                }

                if (haveItem(4032101 + jobtype, 1))
                {
                    message("You have already challenged the Master of Disguise, report your success to the Chief Knight.");
                    return Task.FromResult(false);
                }

                playPortalSound();
                warp(108010600 + (10 * jobtype), "out00");
            }
            else
            {
                playPortalSound();
                warp(130010110, "out00");
            }
            return Task.FromResult(true);
        }


        public Task<bool> enterDisguise4()
        {

            var jobtype = 0;
            if (isQuestStarted(20301) || isQuestStarted(20302) || isQuestStarted(20303) || isQuestStarted(20304) || isQuestStarted(20305))
            {
                var map = getClient().getChannelServer().getMapFactory().getMap(108010600 + (10 * jobtype));
                if (map.countPlayers() > 0)
                {
                    message("Someone else is already searching the area.");
                    return Task.FromResult(false);
                }

                if (haveItem(4032101 + jobtype, 1))
                {
                    message("You have already challenged the Master of Disguise, report your success to the Chief Knight.");
                    return Task.FromResult(false);
                }

                playPortalSound();
                warp(108010600 + (10 * jobtype), "out00");
            }
            else
            {
                playPortalSound();
                warp(130010120, "out00");
            }
            return Task.FromResult(true);
        }


        public Task<bool> enterDisguise5()
        {

            var jobtype = 0;
            if (isQuestStarted(20301) || isQuestStarted(20302) || isQuestStarted(20303) || isQuestStarted(20304) || isQuestStarted(20305))
            {
                var map = getClient().getChannelServer().getMapFactory().getMap(108010600 + (10 * jobtype));
                if (map.countPlayers() > 0)
                {
                    message("Someone else is already searching the area.");
                    return Task.FromResult(false);
                }

                if (haveItem(4032101 + jobtype, 1))
                {
                    message("You have already challenged the Master of Disguise, report your success to the Chief Knight.");
                    return Task.FromResult(false);
                }

                playPortalSound();
                warp(108010600 + (10 * jobtype), "east00");
            }
            else
            {
                playPortalSound();
                warp(130020000, "east00");
            }
            return Task.FromResult(true);
        }


        public Task<bool> enterDollcave()
        {

            if (isQuestCompleted(20730) || isQuestCompleted(21734))
            {  // puppeteer defeated, newfound secret path
                playPortalSound();
                warp(105040201, 2);
                return Task.FromResult(true);
            }

            openNpc(1063011, "PupeteerPassword");
            return Task.FromResult(false);
        }


        public Task<bool> enterDollWay()
        {

            if (isQuestCompleted(20730) || isQuestCompleted(21734))
            {  // puppeteer defeated, newfound secret path
                playPortalSound();
                warp(105070300, 3);
                return Task.FromResult(true);
            }
            else if (isQuestStarted(21734))
            {
                playPortalSound();
                warp(910510100, 0);
                return Task.FromResult(true);
            }
            else
            {
                message("An ominous power prevents you from passing here.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> enterEvanRoom()
        {

            playPortalSound();
            warp(100030100, 0);
            return Task.FromResult(true);
        }


        public Task<bool> enterFirstDH()
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
                if (getPlayerCount(map) == 0)
                {
                    var mapp = getMap(map);
                    mapp.resetPQ();

                    playPortalSound();
                    warp(map, 0);
                    return Task.FromResult(true);
                }
                else
                {
                    playerMessage(5, "已经有其他人比你先使用了演武场，请稍后再试！");
                    return Task.FromResult(false);
                }
            }
            else
            {
                playerMessage(5, "只有参加Kiku的适应训练，才能进入1号演武场。");
                return Task.FromResult(false);
            }
        }


        public Task<bool> enterfourthDH()
        {

            if (hasItem(4032125) || hasItem(4032126) || hasItem(4032127) || hasItem(4032128) || hasItem(4032129))
            {
                playerMessage(5, "你已经有了能力的证物");
                return Task.FromResult(false);
            }

            if (isQuestStarted(20611) || isQuestStarted(20612) || isQuestStarted(20613) || isQuestStarted(20614) || isQuestStarted(20615))
            {
                if (getPlayerCount(913020300) == 0)
                {
                    var map = getMap(913020300);
                    map.killAllMonsters();

                    playPortalSound();
                    warp(913020300, 0);
                    spawnMonster(9300294, 87, 88);
                    return Task.FromResult(true);
                }
                else
                {
                    playerMessage(5, "已经有人在尝试击败Boss，请你稍后再来。");
                    return Task.FromResult(false);
                }
            }
            else
            {
                playerMessage(5, "只有参加骑士指定的等级考试方可进入");
                return Task.FromResult(false);
            }
        }


        public Task<bool> enterGym()
        {

            if (isQuestStarted(21701))
            {
                playPortalSound();
                warp(914010000, 1);
                return Task.FromResult(true);
            }
            else if (isQuestStarted(21702))
            {
                playPortalSound();
                warp(914010100, 1);
                return Task.FromResult(true);
            }
            else if (isQuestStarted(21703))
            {
                playPortalSound();
                warp(914010200, 1);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "You will be allowed to enter the Penguin Training Ground only if you are receiving a lesson from Puo.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> enterInfo()
        {

            var mapobj = getWarpMap(104000004);
            if (isQuestActive(21733) && getQuestProgressInt(21733, 9300345) == 0 && mapobj.countMonsters() == 0)
            {
                mapobj.spawnMonsterOnGroundBelow(9300345, 0, 0);
                setQuestProgress(21733, 21762, 2);
            }

            playPortalSound();
            warp(104000004, 1);
            return Task.FromResult(true);
        }


        public Task<bool> enterMagiclibrar()
        {

            if (isQuestStarted(20718))
            {
                var cml = GetEventManager<SoloEventManager>("Cygnus_Magic_Library");
                cml.setProperty("player", getPlayer().getName());
                cml.startInstance(getPlayer());
                playPortalSound();
            }
            else
            {
                playPortalSound();
                warp(101000003, 8);
            }
            return Task.FromResult(true);
        }


        public Task<bool> enterMCave()
        {

            if (isQuestStarted(21201))
            { // Second Job
                for (var i = 108000700; i < 108000709; i++)
                {
                    if (getPlayerCount(i) > 0 && getPlayerCount(i + 10) > 0)
                    {
                        continue;
                    }

                    playPortalSound();
                    warp(i, "out00");
                    setQuestProgress(21202, 21203, 0);
                    return Task.FromResult(true);
                }
                message("The mirror is blank due to many players recalling their memories. Please wait and try again.");
                return Task.FromResult(false);
            }
            else if (isQuestStarted(21302) && !isQuestCompleted(21303))
            { // Third Job
                if (getPlayerCount(108010701) > 0 || getPlayerCount(108010702) > 0)
                {
                    message("The mirror is blank due to many players recalling their memories. Please wait and try again.");
                    return Task.FromResult(false);
                }
                else
                {
                    var map = getClient().getChannelServer().getMapFactory().getMap(108010702);
                    map.spawnMonsterOnGroundBelow(9001013, -210, 454);

                    playPortalSound();
                    setQuestProgress(21303, 21203, 1);
                    warp(108010701, "out00");
                    return Task.FromResult(true);
                }
            }
            else
            {
                message("You have already passed your test, there is no need to access the mirror again.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> enterNepenthes()
        {

            if (isQuestActive(21739))
            {
                var mapobj1 = getWarpMap(920030000);
                var mapobj2 = getWarpMap(920030001);

                if (mapobj1.countPlayers() == 0 && mapobj2.countPlayers() == 0)
                {
                    mapobj1.resetPQ(1);
                    mapobj2.resetPQ(1);
                    mapobj2.spawnMonsterOnGroundBelow(9300348, 591, -34);

                    playPortalSound();
                    warp(920030000, 2);
                    return Task.FromResult(true);
                }
                else
                {
                    message("Someone is already challenging the area.");
                    return Task.FromResult(false);
                }
            }
            else
            {
                playPortalSound();
                warp(200060001, 2);
                return Task.FromResult(true);
            }
        }


        public Task<bool> enterPort()
        {

            if (isQuestStarted(21301) && getQuestProgressInt(21301, 9001013) == 0)
            {
                if (getPlayerCount(108010700) != 0)
                {
                    message("The portal is blocked from the other side. I wonder if someone is already fighting the Thief Crow?");
                    return Task.FromResult(false);
                }
                else
                {
                    var map = getClient().getChannelServer().getMapFactory().getMap(108010700);
                    map.spawnMonsterOnGroundBelow(9001013, 2732, 3);

                    playPortalSound();
                    warp(108010700, "west00");
                }
            }
            else
            {
                playPortalSound();
                warp(140020300, 1);
            }
            return Task.FromResult(true);
        }


        public Task<bool> enterRider()
        {

            if (isQuestStarted(21610) && !haveItem(4001193, 1))
            {
                var em = getEventManager("Aran_2ndmount") as SoloEventManager;
                if (em == null)
                {
                    message("Sorry, but the 2nd mount quest (Scadur) is closed.");
                    return Task.FromResult(false);
                }
                else
                {
                    if (!em.startInstance(getPlayer()))
                    {
                        message("There is currently someone in this map, come back later.");
                        return Task.FromResult(false);
                    }
                    else
                    {
                        playPortalSound();
                        return Task.FromResult(true);
                    }
                }
            }
            else
            {
                playerMessage(5, "Only attendants of the 2nd Wolf Riding quest may enter this field.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> enterRienFirst()
        {

            if (getPlayer().getJob().getId() == 2000 && !isQuestCompleted(21014))
            {
                playPortalSound();
                warp(140000000, "st00");
            }
            else
            {
                playPortalSound();
                warp(140000000, "west00");
            }

            return Task.FromResult(true);
        }


        public Task<bool> enterSecondDH()
        {

            int[] maps = [108000600, 108000601, 108000602];
            if (isQuestStarted(20201) || isQuestStarted(20202) || isQuestStarted(20203) || isQuestStarted(20204) || isQuestStarted(20205))
            {
                removeAll(4032096);
                removeAll(4032097);
                removeAll(4032098);
                removeAll(4032099);
                removeAll(4032100);

                var rand = Random.Shared.Next(maps.Length);
                playPortalSound();
                warp(maps[rand], 0);
                playerMessage(0, "重新进入第2演武场时将会清空背包里所有考试的证物，请务必注意。");
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "只有参加骑士指定的等级考试方可进入第2演武场。");
                return Task.FromResult(false);
            }
        }


        public Task<bool> enterthirdDH()
        {

            if (hasItem(4032120) || hasItem(4032121) || hasItem(4032122) || hasItem(4032123) || hasItem(4032124))
            {
                playerMessage(5, "你已经有了资格的证物。");
                return Task.FromResult(false);
            }
            if (isQuestStarted(20601) || isQuestStarted(20602) || isQuestStarted(20603) || isQuestStarted(20604) || isQuestStarted(20605))
            {
                if (getPlayerCount(913010200) == 0)
                {
                    var map = getMap(913010200);
                    map.killAllMonsters();
                    playPortalSound();
                    warp(913010200, 0);
                    spawnMonster(9300289, 0, 0);
                    return Task.FromResult(true);
                }
                else
                {
                    playerMessage(5, "已经有人在尝试击败Boss，请你稍后再来。");
                    return Task.FromResult(false);
                }
            }
            else
            {
                playerMessage(5, "你必须达到100级且正在进行技能训练方可进入第3演武场。");
                return Task.FromResult(false);
            }
        }


        public Task<bool> entertraining()
        {

            if (isQuestStarted(1041))
            {
                playPortalSound();
                warp(1010100, 4);
            }
            else if (isQuestStarted(1042))
            {
                playPortalSound();
                warp(1010200, 4);
            }
            else if (isQuestStarted(1043))
            {
                playPortalSound();
                warp(1010300, 4);
            }
            else if (isQuestStarted(1044))
            {
                playPortalSound();
                warp(1010400, 4);
            }
            else
            {
                message("只有接受了麦加训练的人才可以进入训练场");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }


        public Task<bool> enterWarehouse()
        {

            playPortalSound();
            warp(300000011, 0);
            return Task.FromResult(true);
        }


        public Task<bool> enterWitch()
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

                playPortalSound();
                warp(warpMap, 1);
                return Task.FromResult(true);


            }
            else
            {
                playerMessage(5, "I shouldn't go here.. it's creepy!");
                return Task.FromResult(false);
            }
        }


        public Task<bool> enter_earth00()
        {

            if (!haveItem(4031890))
            {
                getPlayer().dropMessage(6, "You need a warp card to activate this portal.");
                return Task.FromResult(false);
            }

            playPortalSound();
            warp(221000300, "earth00");
            return Task.FromResult(true);
        }


        public Task<bool> enter_earth01()
        {

            if (!haveItem(4031890))
            {
                getPlayer().dropMessage(6, "You need a warp card to activate this portal.");
                return Task.FromResult(false);
            }

            playPortalSound();
            warp(120000101, "earth01");
            return Task.FromResult(true);
        }


        public Task<bool> enter_nautil()
        {

            playPortalSound();
            warp(120010000, "nt01");
            return Task.FromResult(true);
        }


        public Task<bool> enter_td()
        {

            playPortalSound();
            warp(600000000, "yn00");
            return Task.FromResult(true);
        }


        public Task<bool> evanEntrance()
        {

            playPortalSound();
            warp(100030400, "east00");
            return Task.FromResult(true);
        }


        public Task<bool> evanFall()
        {

            playPortalSound();
            warp(900090102, 0);
            return Task.FromResult(true);
        }


        public Task<bool> evanFarmCT()
        {

            if (isQuestStarted(22010) || getPlayer().getJob().getId() != 2001)
            {
                playPortalSound();
                warp(100030310, 0);
            }
            else
            {
                playerMessage(5, "Cannot enter the Lush Forest without a reason.");
            }
            return Task.FromResult(true);
        }


        public Task<bool> evanGarden0()
        {

            playPortalSound();
            warp(100030200, "east00");
            return Task.FromResult(true);
        }


        public Task<bool> evanGarden1()
        {

            if (isQuestStarted(22008))
            {
                playPortalSound();
                warp(100030103, "west00");
            }
            else
            {
                playerMessage(5, "You cannot go to the Back Yard without a reason");
            }
            return Task.FromResult(true);
        }


        public Task<bool> evanlivingRoom()
        {

            playPortalSound();
            warp(100030102, "in00");
            return Task.FromResult(true);
        }


        public Task<bool> evanRoom0()
        {

            blockPortal();
            if (containsAreaInfo(22014, "mo30=o"))
            {
                return Task.FromResult(false);
            }
            updateAreaInfo(22014, "mo30=o");
            showInfo("Effect/OnUserEff.img/guideEffect/evanTutorial/evanBalloon30");
            return Task.FromResult(true);
        }

        public Task<bool> exit_puppeteer()
        {

            if (getMap().countMonster(9300285) > 0)
            {
                getPlayer().message("Defeat the Puppeteer before leaving.");
                return Task.FromResult(false);
            }
            else
            {
                var eim = GetEventInstanceTrust();
                if (eim != null)
                {
                    eim.stopEventTimer();
                    eim.Dispose();
                }

                playPortalSound();
                warp(105070300, 3);
                return Task.FromResult(true);
            }
        }


        public Task<bool> female00()
        {

            /**
     *female00.js
     */
            var gender = getPlayer().getGender();
            if (gender == 1)
            {
                playPortalSound();
                warp(670010200, 4);
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(5, "You cannot proceed past here.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> foxLaidy_map()
        {

            if (!(isQuestStarted(3647) && haveItem(4031793, 1)))
            {
                playPortalSound();
                warp(222010200, "east00");
            }
            else
            {
                if (!isQuestStarted(23647))
                {
                    forceStartQuest(23647);
                }
                playPortalSound();
                warp(922220000, "east00");
            }

            return Task.FromResult(true);
        }


        public Task<bool> gaga_success()
        {

            playPortalSound();
            warp(922240100 + (getPlayer().getMapId() - 922240000), 0);
            return Task.FromResult(true);
        }


        public Task<bool> gendergo()
        {

            var map = getPlayer().getMap();
            if (getPortal().getName() == "female00")
            {
                if (getPlayer().getGender() == 1)
                {
                    playPortalSound();
                    warp(map.getId(), "female01");
                    return Task.FromResult(true);
                }
                else
                {
                    message("This portal leads to the girls' area, try the portal at the other side.");
                    return Task.FromResult(false);
                }
            }
            else
            {
                if (getPlayer().getGender() == 0)
                {
                    playPortalSound();
                    warp(map.getId(), "male01");
                    return Task.FromResult(true);
                }
                else
                {
                    message("This portal leads to the boys' area, try the portal at the other side.");
                    return Task.FromResult(false);
                }
            }
        }


        public Task<bool> ghostgate_open()
        {

            if (getPlayer().getMap().getReactorByName("ghostgate")?.getState() == 1)
            {
                playPortalSound();
                warp(990000800, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "This way forward is not open yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> glpqEnter()
        {

            if (haveItem(3992041, 1))
            {
                playPortalSound();
                warp(610030020, "out00");
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The giant gate of iron will not budge no matter what, however there is a visible key-shaped socket.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> glpqPortal0()
        {

            var eim = GetEventInstanceTrust();
            if (eim.getIntProperty("glpq1") == 0)
            {
                eim.Pink("This path is currently blocked.");
                return Task.FromResult(false);

            }
            else
            {
                playPortalSound();
                warp(610030200, 0);
                return Task.FromResult(true);
            }
        }


        public Task<bool> glpqPortal00()
        {

            if (getPlayer().getJob().GetJobNiche() == 1)
            {
                playPortalSound();
                warp(610030510, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "Only warriors may enter this portal.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> glpqPortal01()
        {

            if (getPlayer().getJob().GetJobNiche() == 3)
            {
                playPortalSound();
                warp(610030540, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "Only bowmen may enter this portal.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> glpqPortal02()
        {

            if (getPlayer().getJob().GetJobNiche() == 2)
            {
                playPortalSound();
                warp(610030521, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "Only mages may enter this portal.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> glpqPortal03()
        {

            if (getPlayer().getJob().GetJobNiche() == 4)
            {
                playPortalSound();
                warp(610030530, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "Only thieves may enter this portal.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> glpqPortal04()
        {

            if (getPlayer().getJob().GetJobNiche() == 5)
            {
                playPortalSound();
                warp(610030550, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "Only pirates may enter this portal.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> glpqPortal1()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                if (eim.getIntProperty("glpq2") == 5)
                {
                    playPortalSound();
                    warp(610030300, 0);
                    return Task.FromResult(true);
                }
                else
                {
                    playerMessage(5, "The portal has not been activated yet!");
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(false);
        }


        public Task<bool> glpqPortal2()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                playPortalSound();
                warp(610030300, 0);

                if (eim.getIntProperty("glpq3") < 5 || eim.getIntProperty("glpq3_p") < 5)
                {
                    if (eim.getIntProperty("glpq3_p") == 5)
                    {
                        mapMessage(6, "Not all Sigils have been activated yet. Make sure they have all been activated to proceed to the next stage.");
                    }
                    else
                    {
                        eim.setIntProperty("glpq3_p", eim.getIntProperty("glpq3_p") + 1);

                        if (eim.getIntProperty("glpq3") == 5 && eim.getIntProperty("glpq3_p") == 5)
                        {
                            mapMessage(6, "The Antellion grants you access to the next portal! Proceed!");

                            eim.showClearEffect(610030300, "3pt", 2);
                            eim.giveEventPlayersStageReward(3);
                        }
                        else
                        {
                            mapMessage(6, "An adventurer has passed through! " + (5 - eim.getIntProperty("glpq3_p")) + " to go.");
                        }
                    }
                }
                else
                {
                    getPlayer().dropMessage(6, "The portal at the bottom has already been opened! Proceed there!");
                }

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }


        public Task<bool> glpqPortal3()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                if (eim.getIntProperty("glpq3") < 5 || eim.getIntProperty("glpq3_p") < 5)
                {
                    playerMessage(5, "The portal is not opened yet.");
                    return Task.FromResult(false);
                }
                else
                {
                    playPortalSound();
                    warp(610030400, 0);
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }


        public Task<bool> glpqPortal4()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                if (eim.getIntProperty("glpq4") < 5)
                {
                    playerMessage(5, "The portal is not opened yet.");
                    return Task.FromResult(false);
                }
                else
                {
                    playPortalSound();
                    warp(610030500, 0);
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }


        public Task<bool> glpqPortal5()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                if (eim.getIntProperty("glpq5") < 5)
                {
                    playerMessage(5, "The portal is not opened yet.");
                    return Task.FromResult(false);
                }
                else
                {
                    playPortalSound();
                    warp(610030600, 0);
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }


        public Task<bool> glpqPortal6()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                if (eim.getIntProperty("glpq6") < 3)
                {
                    playerMessage(5, "The portal is not opened yet.");
                    return Task.FromResult(false);
                }
                else
                {
                    playPortalSound();
                    warp(610030700, 0);
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }


        public Task<bool> glpqPortal7()
        {

            playPortalSound();
            warp(610030800, 0);
            return Task.FromResult(true);
        }


        public Task<bool> glpqPortalDummy()
        {

            var react = getMap().getReactorByName("mob0");
            if (react == null)
            {
                return Task.FromResult(false);
            }

            var eim = GetEventInstanceTrust();
            if (eim == null)
            {
                return Task.FromResult(false);
            }

            if (react.getState() < 1)
            {
                react.forceHitReactor(1);

                eim.setIntProperty("glpq1", 1);

                eim.dropMessage(5, "A strange force starts being emitted from the portal apparatus, showing a hidden path once blocked now open.");
                playPortalSound();
                warp(610030100, 0);

                eim.showClearEffect();
                eim.giveEventPlayersStageReward(1);
                return Task.FromResult(true);
            }

            eim.dropMessage(5, "The portal apparatus is malfunctional, due to the last transportation. The finding another way through.");
            return Task.FromResult(false);
        }


        public Task<bool> glTutoMsg0()
        {

            showInstruction("离开这个区域，将无法再回来。", 150, 5);
            return Task.FromResult(true);
        }


        public Task<bool> gotocastle()
        {

            if (isQuestCompleted(2321))
            {
                playPortalSound();
                warp(isQuestCompleted(2324) ? 106020501 : 106020500, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "前路布满荆棘，无法通过！");
                return Task.FromResult(false);
            }
        }


        public Task<bool> go_secretroom()
        {

            if (!isQuestCompleted(2335) && !(isQuestStarted(2335) && hasItem(4032405)))
            {
                getPlayer().message("门锁了，需要钥匙才能进去。");
                return Task.FromResult(false);
            }

            if (isQuestStarted(2335))
            {
                forceCompleteQuest(2335, 1300002);
                giveCharacterExp(5000, getPlayer());
                gainItem(4032405, -1);
            }
            playPortalSound();
            warp(106021001, 1);
            return Task.FromResult(true);
        }


        public Task<bool> gryphius()
        {

            playPortalSound();
            warp(240020101, "out00");
            return Task.FromResult(true);
        }


        public Task<bool> guild1F00()
        {

            int[] backPortals = [6, 8, 9, 11];
            var idx = GetEventInstanceTrust().gridCheck(getPlayer());

            playPortalSound();
            warp(990000600, backPortals[idx]);
            return Task.FromResult(true);
        }


        public Task<bool> guild1F01()
        {

            GetEventInstanceTrust().gridInsert(getPlayer(), 0);
            playPortalSound();
            warp(990000700, "st00");
            return Task.FromResult(true);
        }


        public Task<bool> guild1F02()
        {

            GetEventInstanceTrust().gridInsert(getPlayer(), 1);
            playPortalSound();
            warp(990000700, "st00");
            return Task.FromResult(true);
        }


        public Task<bool> guild1F03()
        {

            GetEventInstanceTrust().gridInsert(getPlayer(), 3);
            playPortalSound();
            warp(990000700, "st00");
            return Task.FromResult(true);
        }


        public Task<bool> guild1F04()
        {

            GetEventInstanceTrust().gridInsert(getPlayer(), 2);
            playPortalSound();
            warp(990000700, "st00");
            return Task.FromResult(true);
        }

        [ScriptTag(["GuildQuest"])]
        public Task<bool> guildwaitingenter()
        {

            var entryTime = long.Parse(GetEventInstanceTrust().getProperty("entryTimestamp"));
            var timeNow = c.CurrentServer.Node.getCurrentTime();

            var timeLeft = Math.Ceiling((entryTime - timeNow) / 1000.0);

            if (timeLeft <= 0)
            {
                playPortalSound();
                warp(990000100, 0);
                return Task.FromResult(true);
            }
            else
            { //cannot proceed while allies can still enter
                playerMessage(5, "The portal will open in about " + timeLeft + " seconds.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> guildwaitingexit()
        {

            playPortalSound();
            warp(101030104);
            return Task.FromResult(true);
        }


        public Task<bool> guyfawkes0_esc()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg1") == 1)
            {
                playPortalSound();
                warp(674030200, 0);
                return Task.FromResult(true);
            }
            else
            {
                message("The tunnel is currently blocked.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> guyfawkes0_floor()
        {

            warp(674030000, 0);
            return Task.FromResult(true);
        }


        public Task<bool> halloween_enter()
        {

            playPortalSound();
            warp(682000100, "st00");
            return Task.FromResult(true);
        }


        public Task<bool> halloween_Omni1()
        {

            playerMessage(5, "It seems to be locked.");
            return Task.FromResult(true);
        }


        public Task<bool> highposition()
        {

            // thanks kvmba for noticing some issues running this script
            touchTheSky();
            return Task.FromResult(false);
        }


        public Task<bool> hontale_Bopen()
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
                target = eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("sp");
                // only let people through if the eim is ready
                avail = eim.getProperty("1stageclear");
                if (avail == null)
                {
                    // do nothing; send message to player
                    getPlayer().dropMessage(6, "Horntail\'s Seal is Blocking this Door.");
                    return Task.FromResult(false);
                }
                else
                {
                    playPortalSound();
                    getPlayer().changeMap(target, targetPortal);
                    return Task.FromResult(true);
                }
            }
            else if (getPlayer().getMapId() == 240050102)
            {
                nextMap = 240050103;
                eim = GetEventInstanceTrust();
                target = eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("sp");
                // only let people through if the eim is ready
                avail = eim.getProperty("2stageclear");
                if (avail == null)
                {
                    // do nothing; send message to player
                    getPlayer().dropMessage(6, "Horntail\'s Seal is Blocking this Door.");
                    return Task.FromResult(false);
                }
                else
                {
                    playPortalSound();
                    getPlayer().changeMap(target, targetPortal);
                    return Task.FromResult(true);
                }
            }
            else if (getPlayer().getMapId() == 240050103)
            {
                nextMap = 240050104;
                eim = GetEventInstanceTrust();
                target = eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("sp");
                // only let people through if the eim is ready
                avail = eim.getProperty("3stageclear");
                if (avail == null)
                {
                    // do nothing; send message to player
                    getPlayer().dropMessage(6, "Horntail\'s Seal is Blocking this Door.");
                    return Task.FromResult(false);
                }
                else
                {
                    playPortalSound();
                    getPlayer().changeMap(target, targetPortal);
                    return Task.FromResult(true);
                }
            }
            else if (getPlayer().getMapId() == 240050104)
            {
                nextMap = 240050105;
                eim = GetEventInstanceTrust();
                target = eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("sp");
                // only let people through if the eim is ready
                avail = eim.getProperty("4stageclear");
                if (avail == null)
                {
                    // do nothing; send message to player
                    getPlayer().dropMessage(6, "Horntail\'s Seal is Blocking this Door.");
                    return Task.FromResult(false);
                }
                else
                {
                    playPortalSound();
                    getPlayer().changeMap(target, targetPortal);
                    return Task.FromResult(true);
                }
            }
            else if (getPlayer().getMapId() == 240050105)
            {
                nextMap = 240050100;
                eim = GetEventInstanceTrust();
                target = eim.getMapInstance(nextMap);
                targetPortal = target.getPortal("st00");

                avail = eim.getProperty("5stageclear");
                if (avail == null)
                {
                    if (haveItem(4001092) && isEventLeader())
                    {
                        eim.showClearEffect();
                        getPlayer().dropMessage(6, "The leader's key break the seal for a flash...");
                        playPortalSound();
                        getPlayer().changeMap(target, targetPortal);
                        eim.setIntProperty("5stageclear", 1);
                        return Task.FromResult(true);
                    }
                    else
                    {
                        getPlayer().dropMessage(6, "Horntail\'s Seal is blocking this door. Only the leader with the key can lift this seal.");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    playPortalSound();
                    getPlayer().changeMap(target, targetPortal);
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(true);
        }


        public Task<bool> hontale_BR()
        {

            if (getPlayer().getMapId() == 240060000)
            {
                if (GetEventInstanceTrust().getIntProperty("defeatedHead") >= 1)
                {
                    playPortalSound();
                    warp(240060100, 0);
                    return Task.FromResult(true);
                }
                else
                {
                    getPlayer().dropMessage(6, "Horntail\'s Seal is Blocking this Door.");
                    return Task.FromResult(false);
                }
            }
            else if (getPlayer().getMapId() == 240060100)
            {
                if (GetEventInstanceTrust().getIntProperty("defeatedHead") >= 2)
                {
                    playPortalSound();
                    warp(240060200, 0);
                    return Task.FromResult(true);
                }
                else
                {
                    getPlayer().dropMessage(6, "Horntail\'s Seal is Blocking this Door.");
                    return Task.FromResult(false);
                }
            }
            return Task.FromResult(false);
        }


        public Task<bool> hontale_BtoB1()
        {

            if (getMap().countPlayers() == 1)
            {
                getPlayer().dropMessage(6, "As the last player on this map, you are compelled to wait for the incoming keys.");
                return Task.FromResult(false);
            }
            else
            {
                if (haveItem(4001087))
                {
                    getPlayer().dropMessage(6, "You cannot pass to the next map holding the 1st Crystal Key in your inventory.");
                    return Task.FromResult(false);
                }
                playPortalSound();
                warp(240050101, 0);
                return Task.FromResult(true);
            }
        }


        public Task<bool> hontale_C()
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
                    playerMessage(5, "Hit the Lightbulb to determine your fate!");
                    return Task.FromResult(false);
                }

                playPortalSound();
                eim.warpEventTeam(target);
                return Task.FromResult(true);
            }
            else
            {
                Pink("You are not the party leader. Only the party leader may proceed through this portal.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> hontale_morph2()
        {

            playPortalSound();
            warp(240040600, 4);
            return Task.FromResult(true);
        }


        public Task<bool> hontale_out1()
        {

            playPortalSound();
            warp(240050400, "sp");
            return Task.FromResult(true);
        }


        public Task<bool> in2159011()
        {

            openNpc(2159011);
            return Task.FromResult(true);
        }


        public Task<bool> inDragonEgg()
        {

            playPortalSound();
            if (isQuestStarted(22005))
            {
                playPortalSound();
                warp(900020100, 0);
            }
            else
            {
                playPortalSound();
                warp(100030301, 0);
            }
            return Task.FromResult(true);
        }


        public Task<bool> inERShip()
        {

            playPortalSound();
            warp(101000400, 2);
            return Task.FromResult(true);
        }


        public Task<bool> infoAttack()
        {

            if (isQuestStarted(1035))
            {
                showInfo("UI/tutorial.img/20");
            }

            blockPortal();
            return Task.FromResult(true);
        }


        public Task<bool> infoMinimap()
        {

            if (isQuestStarted(1031))
            {
                showInfo("UI/tutorial.img/25");
            }

            blockPortal();
            return Task.FromResult(true);
        }


        public Task<bool> infoPickup()
        {

            if (isQuestStarted(1035))
            {
                showInfo("UI/tutorial.img/21");
            }

            blockPortal();
            return Task.FromResult(true);
        }


        public Task<bool> infoReactor()
        {

            if (isQuestCompleted(1008))
            {
                showInfo("UI/tutorial.img/22");
            }
            else if (isQuestCompleted(1020))
            {
                showInfo("UI/tutorial.img/27");
            }

            blockPortal();
            return Task.FromResult(true);
        }


        public Task<bool> infoSkill()
        {

            if (isQuestCompleted(1035))
            {
                showInfo("UI/tutorial.img/23");
            }

            blockPortal();
            return Task.FromResult(true);
        }


        public Task<bool> infoWorldmap()
        {

            showInfo("UI/tutorial.img/26");
            blockPortal();
            return Task.FromResult(true);
        }


        public Task<bool> inNix1()
        {

            playPortalSound();
            warp(240020600, "out00");
            return Task.FromResult(true);
        }


        public Task<bool> inNix2()
        {

            playPortalSound();
            warp(240020600, "out01");
            return Task.FromResult(true);
        }


        public Task<bool> investigate1()
        {

            if (isQuestActive(2314) || isQuestCompleted(2314))
            {
                openNpc(1300014);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> investigate2()
        {

            if (isQuestActive(2322) || isQuestCompleted(2322))
            {
                openNpc(1300014);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> in_xmas_party()
        {

            openNpc(9209100);
            return Task.FromResult(false);
        }


        public Task<bool> jail_in()
        {

            playPortalSound();
            warp(300000012, "portal");
            return Task.FromResult(true);
        }


        public Task<bool> jail_out()
        {

            var jailedTime = getJailTimeLeft();

            if (jailedTime <= 0)
            {
                playPortalSound();
                // warp(300000010, "in01");
                warp(getPlayer().getSavedLocation("JAIL"));
                return Task.FromResult(true);
            }
            else
            {
                var seconds = Math.Floor(jailedTime / 1000.0) % 60;
                var minutes = (Math.Floor(jailedTime / (1000.0 * 60)) % 60);
                var hours = (Math.Floor(jailedTime / (1000.0 * 60 * 60)) % 24);

                playerMessage(5, "You have been caught in bad behaviour by the Maple POLICE. You've got to stay here for " + hours + " hours " + minutes + " minutes " + seconds + " seconds yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> jnr12_in()
        {

            playPortalSound();
            warp(926110401, 0); //next
            return Task.FromResult(true);
        }


        public Task<bool> jnr1_out()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg2") == 1)
            {
                playPortalSound();
                warp(926110100, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> jnr1_pt00()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg1") == 1)
            {
                playPortalSound();
                warp(926110001, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> jnr2_out()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg3") == 3)
            {
                playPortalSound();
                warp(926110200, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> jnr3_in0()
        {

            if (getMap().getReactorByName("jnr3_out1")?.getState() == 1)
            {
                playPortalSound();
                warp(926110201, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The door is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> jnr3_in1()
        {

            if (getMap().getReactorByName("jnr3_out2")?.getState() == 1)
            {
                playPortalSound();
                warp(926110202, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The door is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> jnr3_out()
        {

            if (getMap().getReactorByName("jnr3_out3")?.getState() == 1)
            {
                playPortalSound();
                warp(926110203, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The door is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> jnr4_r1()
        {

            var eim = GetEventInstanceTrust();
            var area = eim.getIntProperty("statusStg5");
            var reg = 0;

            if ((area >> reg) % 2 == 0)
            {
                area |= (1 << reg);
                eim.setIntProperty("statusStg5", area);

                playPortalSound();
                warp(926110301 + reg, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "This room is already being explored.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> jnr4_r2()
        {

            var eim = GetEventInstanceTrust();
            var area = eim.getIntProperty("statusStg5");
            var reg = 1;

            if ((area >> reg) % 2 == 0)
            {
                area |= (1 << reg);
                eim.setIntProperty("statusStg5", area);

                playPortalSound();
                warp(926110301 + reg, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "This room is already being explored.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> jnr4_r3()
        {

            var eim = GetEventInstanceTrust();
            var area = eim.getIntProperty("statusStg5");
            var reg = 2;

            if ((area >> reg) % 2 == 0)
            {
                area |= (1 << reg);
                eim.setIntProperty("statusStg5", area);

                playPortalSound();
                warp(926110301 + reg, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "This room is already being explored.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> jnr4_r4()
        {

            var eim = GetEventInstanceTrust();
            var area = eim.getIntProperty("statusStg5");
            var reg = 3;

            if ((area >> reg) % 2 == 0)
            {
                area |= (1 << reg);
                eim.setIntProperty("statusStg5", area);

                playPortalSound();
                warp(926110301 + reg, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "This room is already being explored.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> jnr5_rp()
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
                    playPortalSound();
                    warp(getMapId(), getPortal().getId() + 4);
                }
                else
                {
                    if (eim.getIntProperty("statusStg6") == 0)
                    {
                        eim.setIntProperty("statusStg6", 1);
                        eim.giveEventPlayersStageReward(6);
                    }

                    playPortalSound();
                    warp(getMapId(), 1);
                }

            }
            else
            {    //fail
                playPortalSound();
                warp(getMapId(), 2);
            }

            return Task.FromResult(true);
        }


        public Task<bool> jnr6_out()
        {

            if (getMap().getReactorByName("jnr6_out")?.getState() == 1)
            {
                playPortalSound();
                warp(926110300, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> jnr_201_0()
        {

            if (getMap().getReactorByName("jnr31_out")?.getState() == 1)
            {
                playPortalSound();
                warp(926110200, 1);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The door is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> jnr_202()
        {

            if (getMap().getReactorByName("jnr32_out")?.getState() == 1)
            {
                playPortalSound();
                warp(926110200, 2);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The door is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> jnr_exit()
        {

            playPortalSound();
            warp(261000021, 0);
            return Task.FromResult(true);
        }


        public Task<bool> kinggate2_open()
        {

            var eim = GetEventInstanceTrust();

            if (getPlayer().getMap().getReactorByName("kinggate")?.getState() == 1)
            {
                playPortalSound();
                warp(990000900, 2);
                if (eim.getProperty("boss") == "true")
                {
                    changeMusic("Bgm10/Eregos");
                }
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "This crack appears to be blocked off by the door nearby.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> kinggate_open()
        {
            var eim = GetEventInstanceTrust();

            if (getPlayer().getMap().getReactorByName("kinggate")?.getState() == 1)
            {
                playPortalSound();
                warp(990000900, 1);
                if (eim.getProperty("boss") == "true")
                {
                    changeMusic("Bgm10/Eregos");
                }
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "This door is closed.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> kpq0()
        {

            var eim = GetEventInstanceTrust();
            var target = eim.getMapInstance(103000801);

            if (eim.getProperty("1stageclear") != null)
            {
                playPortalSound();
                getPlayer().changeMap(target, target.getPortal("st00"));
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> kpq1()
        {

            var eim = GetEventInstanceTrust();
            var target = eim.getMapInstance(103000802);
            if (eim.getProperty("2stageclear") != null)
            {
                playPortalSound();
                getPlayer().changeMap(target, target.getPortal("st00"));
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> kpq2()
        {

            var eim = GetEventInstanceTrust();
            var target = eim.getMapInstance(103000803);
            if (eim.getProperty("3stageclear") != null)
            {
                playPortalSound();
                getPlayer().changeMap(target, target.getPortal("st00"));
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> kpq3()
        {

            var eim = GetEventInstanceTrust();
            var target = eim.getMapInstance(103000804);
            if (eim.getProperty("4stageclear") != null)
            {
                playPortalSound();
                getPlayer().changeMap(target, target.getPortal("st00"));
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> kpq4()
        {

            var eim = GetEventInstanceTrust();
            var target = eim.getMapInstance(103000805);
            if (eim.getProperty("5stageclear") != null)
            {
                playPortalSound();
                getPlayer().changeMap(target, target.getPortal("st00"));
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> lionCastle_enter()
        {

            playPortalSound();
            warp(211060010, "west00");
            return Task.FromResult(true);
        }


        public Task<bool> lpq0()
        {

            var nextMap = 922010200;
            var eim = GetEventInstanceTrust();
            var target = eim.getMapInstance(nextMap);
            var targetPortal = target.getPortal("st00");
            var avail = eim.getProperty("1stageclear");
            if (avail == null)
            {
                getPlayer().dropMessage(5, "Some seal is blocking this door.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                getPlayer().changeMap(target, targetPortal);
                return Task.FromResult(true);
            }
        }


        public Task<bool> lpq1()
        {

            var nextMap = 922010300;
            var eim = GetEventInstanceTrust();
            var target = eim.getMapInstance(nextMap);
            var targetPortal = target.getPortal("st00");
            var avail = eim.getProperty("2stageclear");
            if (avail == null)
            {
                getPlayer().dropMessage(5, "Some seal is blocking this door.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                getPlayer().changeMap(target, targetPortal);
                return Task.FromResult(true);
            }
        }


        public Task<bool> lpq2()
        {

            var nextMap = 922010400;
            var eim = GetEventInstanceTrust();
            var target = eim.getMapInstance(nextMap);
            var targetPortal = target.getPortal("st00");
            // only let people through if the eim is ready
            var avail = eim.getProperty("3stageclear");
            if (avail == null)
            {
                // can't go thru eh?
                getPlayer().dropMessage(5, "Some seal is blocking this door.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                getPlayer().changeMap(target, targetPortal);
                return Task.FromResult(true);
            }
        }


        public Task<bool> lpq3()
        {

            var nextMap = 922010500;
            var eim = GetEventInstanceTrust();
            var target = eim.getMapInstance(nextMap);
            var targetPortal = target.getPortal("st00");
            // only let people through if the eim is ready
            var avail = eim.getProperty("4stageclear");
            if (avail == null)
            {
                // can't go thru eh?
                getPlayer().dropMessage(5, "Some seal is blocking this door.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                getPlayer().changeMap(target, targetPortal);
                return Task.FromResult(true);
            }
        }


        public Task<bool> lpq4()
        {

            var nextMap = 922010600;
            var eim = GetEventInstanceTrust();
            var target = eim.getMapInstance(nextMap);
            var targetPortal = target.getPortal("st00");
            // only let people through if the eim is ready
            var avail = eim.getProperty("5stageclear");
            if (avail == null)
            {
                // can't go thru eh?
                getPlayer().dropMessage(5, "Some seal is blocking this door.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                getPlayer().changeMap(target, targetPortal);
                return Task.FromResult(true);
            }
        }


        public Task<bool> lpq5()
        {

            var nextMap = 922010700;
            var eim = GetEventInstanceTrust();
            var target = eim.getMapInstance(nextMap);
            var targetPortal = target.getPortal("st00");
            // only let people through if the eim is ready
            var avail = eim.getProperty("5stageclear");
            if (avail == null)
            {
                // can't go thru eh?
                getPlayer().dropMessage(5, "Some seal is blocking this door.");
                return Task.FromResult(false);
            }
            else
            {
                if (eim.getProperty("6stageclear") == null)
                {
                    eim.setProperty("6stageclear", "true");
                }
                playPortalSound();
                getPlayer().changeMap(target, targetPortal);
                return Task.FromResult(true);
            }
        }


        public Task<bool> lpq6()
        {

            var eim = GetEventInstanceTrust();
            var target = eim.getMapInstance(922010800);
            if (eim.getProperty("7stageclear") != null)
            {
                playPortalSound();
                getPlayer().changeMap(target, target.getPortal("st00"));
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }


        public Task<bool> lpq7()
        {

            var nextMap = 922010900;
            var eim = GetEventInstanceTrust();
            var target = eim.getMapInstance(nextMap);
            var targetPortal = target.getPortal("st00");
            // only let people through if the eim is ready
            var avail = eim.getProperty("8stageclear");
            if (avail == null)
            {
                // can't go thru eh?
                getPlayer().dropMessage(5, "Some seal is blocking this door.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                getPlayer().changeMap(target, targetPortal);
                return Task.FromResult(true);
            }
        }


        public Task<bool> ludi021()
        {

            removeAll(4031092);
            warp(220020600);
            return Task.FromResult(true);
        }


        public Task<bool> magatia_alc0()
        {

            if (!isQuestStarted(3309) || haveItem(4031708, 1))
            {
                playPortalSound();
                warp(261020700, "down00");
            }
            else
            {
                playPortalSound();
                warp(926120000, "out00");
            }

            return Task.FromResult(true);
        }


        public Task<bool> magatia_dark0()
        {

            if (isQuestCompleted(7770))
            {
                playPortalSound();
                warp(926130000, "out00");
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "This pipe seems too dark to venture inside.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> male00()
        {

            /**
     *Male00.js
     */
            var gender = getPlayer().getGender();
            if (gender == 0)
            {
                playPortalSound();
                warp(670010200, 3);
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(5, "You cannot proceed past here.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> mapleMarket7_out()
        {

            playPortalSound();
            warp(getPlayer().getSavedLocation("EVENT"));
            return Task.FromResult(true);
        }


        public Task<bool> market00()
        {

            try
            {
                var toMap = getPlayer().getSavedLocation("FREE_MARKET");
                playPortalSound();
                warp(toMap, getMarketPortalId(toMap));
            }
            catch (Exception)
            {
                playPortalSound();
                warp(100000000, 0);
            }
            return Task.FromResult(true);
        }


        public Task<bool> market01()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market02()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market03()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market04()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market05()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market06()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market07()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market08()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market09()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market10()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market11()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market12()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market13()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market14()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market15()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market16()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market17()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market18()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market19()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market20()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market21()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market22()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market23()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market24()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market26()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market52()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market53()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market54()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market55()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> market56()
        {

            if (getPlayer().getMapId() != 910000000)
            {
                getPlayer().saveLocation("FREE_MARKET");
                playPortalSound();
                warp(910000000, "out00");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> Masteria_B1_1()
        {

            if (hasItem(3992040))
            {
                playPortalSound();
                warp(610010005, "sU6_1");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> Masteria_B2_1()
        {

            if (hasItem(3992040))
            {
                playPortalSound();
                warp(610010005, "sU6_1");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> Masteria_B3_1()
        {

            if (hasItem(3992040))
            {
                playPortalSound();
                warp(610010005, "sU6_1");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }


        public Task<bool> Masteria_CC1_A()
        {

            playPortalSound();
            warp(610020015, "CC6_A");
            return Task.FromResult(true);
        }


        public Task<bool> Masteria_CC6_A()
        {

            playPortalSound();
            warp(610020010, "CC1_A");
            return Task.FromResult(true);
        }


        public Task<bool> Masteria_CM1_A()
        {

            if (hasItem(3992039))
            {
                playPortalSound();
                warp(610020000, "CM1_B");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }


        public Task<bool> Masteria_CM1_B()
        {

            if (hasItem(3992039))
            {
                playPortalSound();
                warp(610020000, "CM1_C");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }


        public Task<bool> Masteria_CM1_C()
        {

            if (hasItem(3992039))
            {
                playPortalSound();
                warp(610020000, "CM1_D");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }


        public Task<bool> Masteria_CM1_D()
        {

            if (hasItem(3992039))
            {
                playPortalSound();
                warp(610020000, "CM1_E");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }


        public Task<bool> Masteria_CM2_B()
        {

            if (hasItem(3992039))
            {
                playPortalSound();
                warp(610020001, "CM2_C");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }


        public Task<bool> Masteria_CM2_C()
        {

            if (hasItem(3992039))
            {
                playPortalSound();
                warp(610020001, "CM2_D");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }


        public Task<bool> Masteria_CM2_D()
        {

            if (hasItem(3992039))
            {
                playPortalSound();
                warp(610020001, "CM2_E");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }


        public Task<bool> Masteria_CM2_E()
        {

            if (hasItem(3992039))
            {
                playPortalSound();
                warp(610020001, "CM2_F");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }


        public Task<bool> Masteria_U2_1()
        {

            if (hasItem(3992040))
            {
                playPortalSound();
                warp(610010004, "U5_1");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }


        public Task<bool> Masteria_U3_1()
        {

            if (hasItem(3992040))
            {
                playPortalSound();
                warp(610010201, "sB2_1");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }


        public Task<bool> Masteria_U5_1()
        {

            if (hasItem(3992040))
            {
                playPortalSound();
                warp(610010001, "sU2_1");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }


        public Task<bool> Masteria_U5_2()
        {

            if (hasItem(3992040))
            {
                playPortalSound();
                warp(610010004, "U5_1");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }


        public Task<bool> Masteria_U6_1()
        {

            if (hasItem(3992040))
            {
                playPortalSound();
                warp(610010002, "sU3_1");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }


        public Task<bool> mayong()
        {

            playPortalSound();
            warp(240020401, "out00");
            return Task.FromResult(true);
        }


        public Task<bool> MC2revive()
        {

            playPortalSound();
            WarpReturn();
            return Task.FromResult(true);
        }


        public Task<bool> MCrevive1()
        {

            WarpReturn();
            playPortalSound();
            return Task.FromResult(true);
        }


        public Task<bool> MCrevive2()
        {

            WarpReturn();
            playPortalSound();
            return Task.FromResult(true);
        }


        public Task<bool> MCrevive3()
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
            WarpReturn(portal);
            playPortalSound();
            return Task.FromResult(true);
        }


        public Task<bool> MCrevive4()
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
            WarpReturn(portal);
            playPortalSound();
            return Task.FromResult(true);
        }


        public Task<bool> MCrevive5()
        {

            WarpReturn();
            playPortalSound();
            return Task.FromResult(true);
        }


        public Task<bool> MCrevive6()
        {

            WarpReturn();
            playPortalSound();
            return Task.FromResult(true);
        }


        public Task<bool> mc_out()
        {

            var returnMap = getPlayer().getSavedLocation("MONSTER_CARNIVAL");
            if (returnMap < 0)
            {
                returnMap = 102000000; // Just Incase there is no saved location.
            }
            var target = getPlayer().getClient().getChannelServer().getMapFactory().getMap(returnMap);
            getPlayer().changeMap(target);
            playPortalSound();
            return Task.FromResult(true);
        }


        public Task<bool> MD_cakeEnter()
        {

            playPortalSound();
            warp(674030100, "in00");
            return Task.FromResult(true);
        }


        public Task<bool> MD_drakeroom()
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
                            if (startDungeonInstance(dungeonid + i))
                            {
                                playPortalSound();
                                warpParty(dungeonid + i, "out00");
                                return Task.FromResult(true);
                            }
                        }
                    }
                    else
                    {
                        playerMessage(5, "Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (startDungeonInstance(dungeonid + i))
                        {
                            playPortalSound();
                            warp(dungeonid + i, "out00");
                            return Task.FromResult(true);
                        }
                    }
                }
                playerMessage(5, "All of the Mini-Dungeons are in use right now, please try again later.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(baseid, "MD00");
                return Task.FromResult(true);
            }
        }


        public Task<bool> MD_error()
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
                            if (startDungeonInstance(dungeonid + i))
                            {
                                playPortalSound();
                                warpParty(dungeonid + i, "out00");
                                return Task.FromResult(true);
                            }
                        }
                    }
                    else
                    {
                        playerMessage(5, "Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (startDungeonInstance(dungeonid + i))
                        {
                            playPortalSound();
                            warp(dungeonid + i, "out00");
                            return Task.FromResult(true);
                        }
                    }
                }
                playerMessage(5, "All of the Mini-Dungeons are in use right now, please try again later.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(baseid, "MD00");
                return Task.FromResult(true);
            }
        }


        public Task<bool> MD_golem()
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
                            if (startDungeonInstance(dungeonid + i))
                            {
                                playPortalSound();
                                warpParty(dungeonid + i, "out00");
                                return Task.FromResult(true);
                            }
                        }
                    }
                    else
                    {
                        playerMessage(5, "Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (startDungeonInstance(dungeonid + i))
                        {
                            playPortalSound();
                            warp(dungeonid + i, "out00");
                            return Task.FromResult(true);
                        }
                    }
                }
                playerMessage(5, "All of the Mini-Dungeons are in use right now, please try again later.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(baseid, "MD00");
                return Task.FromResult(true);
            }
        }


        public Task<bool> MD_high()
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
                            if (startDungeonInstance(dungeonid + i))
                            {
                                playPortalSound();
                                warpParty(dungeonid + i, "out00");
                                return Task.FromResult(true);
                            }
                        }
                    }
                    else
                    {
                        playerMessage(5, "Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (startDungeonInstance(dungeonid + i))
                        {
                            playPortalSound();
                            warp(dungeonid + i, "out00");
                            return Task.FromResult(true);
                        }
                    }
                }
                playerMessage(5, "All of the Mini-Dungeons are in use right now, please try again later.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(baseid, "MD00");
                return Task.FromResult(true);
            }
        }


        public Task<bool> MD_mushroom()
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
                            if (startDungeonInstance(dungeonid + i))
                            {
                                playPortalSound();
                                warpParty(dungeonid + i, "out00");
                                return Task.FromResult(true);
                            }
                        }
                    }
                    else
                    {
                        playerMessage(5, "Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (startDungeonInstance(dungeonid + i))
                        {
                            playPortalSound();
                            warp(dungeonid + i, "out00");
                            return Task.FromResult(true);
                        }
                    }
                }
                playerMessage(5, "All of the Mini-Dungeons are in use right now, please try again later.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(baseid, "MD00");
                return Task.FromResult(true);
            }
        }


        public Task<bool> MD_pig()
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
                            if (startDungeonInstance(dungeonid + i))
                            {
                                playPortalSound();
                                warpParty(dungeonid + i, "out00");
                                return Task.FromResult(true);
                            }
                        }
                    }
                    else
                    {
                        playerMessage(5, "Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (startDungeonInstance(dungeonid + i))
                        {
                            playPortalSound();
                            warp(dungeonid + i, "out00");
                            return Task.FromResult(true);
                        }
                    }
                }
                playerMessage(5, "All of the Mini-Dungeons are in use right now, please try again later.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(baseid, "MD00");
                return Task.FromResult(true);
            }
        }


        public Task<bool> MD_protect()
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
                            if (startDungeonInstance(dungeonid + i))
                            {
                                playPortalSound();
                                warpParty(dungeonid + i, "out00");
                                return Task.FromResult(true);
                            }
                        }
                    }
                    else
                    {
                        playerMessage(5, "Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (startDungeonInstance(dungeonid + i))
                        {
                            playPortalSound();
                            warp(dungeonid + i, "out00");
                            return Task.FromResult(true);
                        }
                    }
                }
                playerMessage(5, "All of the Mini-Dungeons are in use right now, please try again later.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(baseid, "MD00");
                return Task.FromResult(true);
            }
        }


        public Task<bool> MD_rabbit()
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
                            if (startDungeonInstance(dungeonid + i))
                            {
                                playPortalSound();
                                warpParty(dungeonid + i, "out00");
                                return Task.FromResult(true);
                            }
                        }
                    }
                    else
                    {
                        playerMessage(5, "Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (startDungeonInstance(dungeonid + i))
                        {
                            playPortalSound();
                            warp(dungeonid + i, "out00");
                            return Task.FromResult(true);
                        }
                    }
                }
                playerMessage(5, "All of the Mini-Dungeons are in use right now, please try again later.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(baseid, "MD00");
                return Task.FromResult(true);
            }
        }


        public Task<bool> MD_remember()
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
                            if (startDungeonInstance(dungeonid + i))
                            {
                                playPortalSound();
                                warpParty(dungeonid + i, "out00");
                                return Task.FromResult(true);
                            }
                        }
                    }
                    else
                    {
                        playerMessage(5, "Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (startDungeonInstance(dungeonid + i))
                        {
                            playPortalSound();
                            warp(dungeonid + i, "out00");
                            return Task.FromResult(true);
                        }
                    }
                }
                playerMessage(5, "All of the Mini-Dungeons are in use right now, please try again later.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(baseid, "MD00");
                return Task.FromResult(true);
            }
        }


        public Task<bool> MD_roundTable()
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
                            if (startDungeonInstance(dungeonid + i))
                            {
                                playPortalSound();
                                warpParty(dungeonid + i, "out00");
                                return Task.FromResult(true);
                            }
                        }
                    }
                    else
                    {
                        playerMessage(5, "Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (startDungeonInstance(dungeonid + i))
                        {
                            playPortalSound();
                            warp(dungeonid + i, "out00");
                            return Task.FromResult(true);
                        }
                    }
                }
                playerMessage(5, "All of the Mini-Dungeons are in use right now, please try again later.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(baseid, "MD00");
                return Task.FromResult(true);
            }
        }


        public Task<bool> MD_sand()
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
                            if (startDungeonInstance(dungeonid + i))
                            {
                                playPortalSound();
                                warpParty(dungeonid + i, "out00");
                                return Task.FromResult(true);
                            }
                        }
                    }
                    else
                    {
                        playerMessage(5, "Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (startDungeonInstance(dungeonid + i))
                        {
                            playPortalSound();
                            warp(dungeonid + i, "out00");
                            return Task.FromResult(true);
                        }
                    }
                }
                playerMessage(5, "All of the Mini-Dungeons are in use right now, please try again later.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(baseid, "MD00");
                return Task.FromResult(true);
            }
        }


        public Task<bool> MD_treasure()
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
                            if (startDungeonInstance(dungeonid + i))
                            {
                                playPortalSound();
                                warpParty(dungeonid + i, "out00");
                                return Task.FromResult(true);
                            }
                        }
                    }
                    else
                    {
                        playerMessage(5, "Only solo or party leaders are supposed to enter the Mini-Dungeon.");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    for (var i = 0; i < dungeons; i++)
                    {
                        if (startDungeonInstance(dungeonid + i))
                        {
                            playPortalSound();
                            warp(dungeonid + i, "out00");
                            return Task.FromResult(true);
                        }
                    }
                }
                playerMessage(5, "All of the Mini-Dungeons are in use right now, please try again later.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(baseid, "MD00");
                return Task.FromResult(true);
            }
        }


        public Task<bool> metalgate_open()
        {

            if (getPlayer().getMap().getReactorByName("metalgate")?.getState() == 1)
            {
                playPortalSound();
                warp(990000431, 0);
                return Task.FromResult(true);
            }
            Pink("This way forward is not open yet.");
            return Task.FromResult(false);
        }


        public Task<bool> metro_Chat00()
        {

            showIntro("Effect/Direction2.img/metro/Im");
            //showWZEffect("Effect/Direction2.img/metro/Im");
            return Task.FromResult(true);
        }


        public Task<bool> metro_in00()
        {

            openNpc(1052115);
            return Task.FromResult(true);
        }


        public Task<bool> met_in()
        {

            //warp(910320000, 2); event not implemented

            playPortalSound();
            warp(103000103, 1);
            return Task.FromResult(true);
        }


        public Task<bool> met_out()
        {

            var mapId = getPlayer().getSavedLocation("MIRROR");

            playPortalSound();
            if (mapId == -1)
            {
                warp(102040000, 12);
            }
            else
            {
                warp(mapId);
            }

            //warp(102040000, 12);
            return Task.FromResult(true);
        }


        public Task<bool> minar_elli()
        {

            if (!haveItem(4031346))
            {
                getPlayer().dropMessage(6, "You need a magic seed to use this portal.");
                return Task.FromResult(false);
            }
            if (getPlayer().getMapId() == 240010100)
            {
                gainItem(4031346, -1);
                playPortalSound();
                warp(101010000, "minar00");
                return Task.FromResult(true);
            }
            else if (getPlayer().getMapId() == 101010000)
            {
                gainItem(4031346, -1);
                playPortalSound();
                warp(240010100, "elli00");
                return Task.FromResult(true);
            }
            return Task.FromResult(true);
        }


        public Task<bool> minar_job4()
        {

            playPortalSound();
            warp(240010501, "out00");
            return Task.FromResult(true);
        }


        public Task<bool> mirtalk00()
        {

            blockPortal();
            if (containsAreaInfo(22013, "dt00=o"))
            {
                return Task.FromResult(false);
            }
            mapEffect("evan/dragonTalk00");
            updateAreaInfo(22013, "dt00=o;mo00=o");
            return Task.FromResult(true);
        }


        public Task<bool> mirtalk01()
        {

            blockPortal();
            if (containsAreaInfo(22013, "dt01=o"))
            {
                return Task.FromResult(false);
            }
            mapEffect("evan/dragonTalk01");
            updateAreaInfo(22013, "dt00=o;dt01=o;mo00=o;mo01=o;mo10=o;mo02=o");
            return Task.FromResult(true);
        }


        public Task<bool> moveBefore()
        {

            playPortalSound();
            warp(getMapId() - 10, "west00");
            return Task.FromResult(true);
        }


        public Task<bool> moveNext()
        {

            playPortalSound();
            warp(getMapId() + 10, "east00");
            return Task.FromResult(true);
        }


        public Task<bool> move_elin()
        {

            playPortalSound();
            warp(300000100, "out00");
            playerMessage(5, "Now passing the Time Gate.");
            return Task.FromResult(true);
        }


        public Task<bool> move_RieRit()
        {

            return Task.FromResult(true);
        }


        public Task<bool> move_RitRie()
        {

            return Task.FromResult(true);
        }


        public Task<bool> nets_in()
        {

            getPlayer().saveLocation("MIRROR");
            playPortalSound();
            warp(926010000, 4);
            return Task.FromResult(true);
        }


        public Task<bool> nets_out()
        {

            var mapid = getPlayer().getSavedLocation("MIRROR");

            playPortalSound();
            if (mapid == 260020500)
            {
                warp(mapid, 3);
            }
            else
            {
                warp(mapid);
            }
            return Task.FromResult(true);
        }


        public Task<bool> NextMap()
        {

            playPortalSound();
            warp(getMapId() + 100, 0);
            return Task.FromResult(true);
        }


        public Task<bool> obstacle()
        {

            if (isQuestStarted(100202))
            {    //使用过奇拉蘑菇孢子后允许直接通过
                playPortalSound();
                warp(106020400, 2);
                return Task.FromResult(true);
            }
            else if (hasItem(4000507))
            {
                gainItem(4000507, -1);
                playPortalSound();
                warp(106020400, 2);
                message("消耗一个 蘑菇的毒孢子 通过了结界。");
                return Task.FromResult(true);
            }
            else
            {
                showInfo("Effect/OnUserEff/normalEffect/mushroomcastle/chatBalloon1");
                message("似乎有一个魔力强大的结界阻止你进入。");
            }
            return Task.FromResult(false);
        }


        public Task<bool> outArchterMap()
        {

            playPortalSound();
            warp(100000000, "Achter00");
            playPortalSound();
            return Task.FromResult(true);
        }


        public Task<bool> outChild()
        {

            if (!isQuestStarted(21001))
            {
                playPortalSound();
                warp(914000220, 2);
                return Task.FromResult(true);
            }
            else
            {
                playPortalSound();
                warp(914000400, 2);
                return Task.FromResult(true);
            }
        }


        public Task<bool> outDarkEreb()
        {

            var warpMap = isQuestCompleted(20407) ? 924010200 : 924010100;

            playPortalSound();
            warp(warpMap, 0);
            return Task.FromResult(true);
        }


        public Task<bool> outMagiclib()
        {

            if (getMap().countMonster(2220100) > 0)
            {
                getPlayer().message("Cannot leave until all Blue Mushrooms have been defeated.");
                return Task.FromResult(false);
            }
            else
            {
                var eim = GetEventInstanceTrust();
                eim.stopEventTimer();
                eim.Dispose();

                playPortalSound();
                warp(101000000, 26);

                if (isQuestCompleted(20718))
                {
                    openNpc(1103003, "MaybeItsGrendel_end");
                }

                return Task.FromResult(true);
            }
        }


        public Task<bool> outMaha()
        {

            playPortalSound();
            warp(140000000, 0);
            return Task.FromResult(true);
        }


        public Task<bool> outNix1()
        {

            playPortalSound();
            warp(240020101, "in00");
            return Task.FromResult(true);
        }


        public Task<bool> outNix2()
        {

            playPortalSound();
            warp(240020401, "in00");
            return Task.FromResult(true);
        }


        public Task<bool> outPerrion_1()
        {

            message("You found a shortcut to the start of the underground temple.");
            playPortalSound();
            warp(105100000, 2);
            return Task.FromResult(true);
        }


        public Task<bool> outPerrion_2()
        {

            playPortalSound();
            warp(105100000, 0);
            return Task.FromResult(true);
        }


        public Task<bool> outRider()
        {

            if (canHold(4001193, 1))
            {
                gainItem(4001193, 1);
                playPortalSound();
                warp(211050000, 4);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "Free a slot on your inventory before receiving the couse clear's token.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> outSpecialSchool()
        {

            playPortalSound();
            warp(925040000, 1);
            return Task.FromResult(true);
        }


        public Task<bool> outTemple()
        {

            useItem(2210016);
            playPortalSound();
            warp(200090510, 0);
            return Task.FromResult(true);
        }


        public Task<bool> outtestWolf()
        {

            if (getMap().countMonsters() == 0)
            {
                if (canHold(4001193, 1))
                {
                    gainItem(4001193, 1);
                    playPortalSound();
                    warp(140010210, 0);
                    return Task.FromResult(true);
                }
                else
                {
                    playerMessage(5, "Free a slot on your inventory before receiving the couse clear's token.");
                    return Task.FromResult(false);
                }
            }
            else
            {
                playerMessage(5, "Defeat all wolves before exiting the stage.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> out_pepeking()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                eim.stopEventTimer();
                eim.Dispose();
            }

            var questProgress = getQuestProgressInt(2330, 3300005) + getQuestProgressInt(2330, 3300006) + getQuestProgressInt(2330, 3300007); //3 Yetis
            if (questProgress == 3 && !hasItem(4032388))
            {
                if (canHold(4032388))
                {
                    getPlayer().message("你已经拿到了结婚礼堂的钥匙。企鹅国王肯定是把它丢掉了。");
                    gainItem(4032388, 1);

                    playPortalSound();
                    warp(106021400, 2);
                    return Task.FromResult(true);
                }
                else
                {
                    getPlayer().message("请确保背包其它物品栏还有可用空间。");
                    return Task.FromResult(false);
                }
            }
            else
            {
                playPortalSound();
                warp(106021400, 2);
                return Task.FromResult(true);
            }
        }


        public Task<bool> party3_gardenin()
        {

            if (getPlayer().getParty() != null && isEventLeader() && haveItem(4001055, 1))
            {
                playPortalSound();
                GetEventInstanceTrust().warpEventTeam(920010100);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "Please get the leader in this portal, make sure you have the Root of Life.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> party3_jail1()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg8") == 1)
            {
                playPortalSound();
                warp(920010910, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The storage is currently inaccessible, as the powers of the Pixies remains active within the tower.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> party3_jail2()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg8") == 1)
            {
                playPortalSound();
                warp(920010920, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The storage is currently inaccessible, as the powers of the Pixies remains active within the tower.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> party3_jail3()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg8") == 1)
            {
                playPortalSound();
                warp(920010930, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The storage is currently inaccessible, as the powers of the Pixies remains active within the tower.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> party3_jailin()
        {
            var map = getMap();
            var mobcount = map.countMonster(9300044);

            if (mobcount > 0)
            {
                playerMessage(5, "请先使用控制杆清除所有威胁再继续前进");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(getMapId() + 2, 0);
                return Task.FromResult(true);
            }
        }


        public Task<bool> party3_r4pt()
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

                playPortalSound();
                warp(getMapId(), nextPortal);
            }
            else
            {    //fail
                playPortalSound();
                warp(getMapId(), 2);
            }

            return Task.FromResult(true);
        }


        public Task<bool> party3_r4pt1()
        {

            playPortalSound();
            warp(920010600, Random.Shared.NextDouble() * 3 > 1 ? 1 : 2);
            return Task.FromResult(true);
        }


        public Task<bool> party3_r6pt()
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
                playPortalSound();
                warp(getMapId(), (pRow % 4 != 0) ? getPortal().getId() + 4 : (pRow / 4));
            }
            else
            {    //fail
                pRow--;
                playPortalSound();
                warp(getMapId(), (pRow / 4.0) > 1 ? (int)(pRow / 4.0) : 5);  // thanks Chloek3, seth1 for noticing next plaform issues
            }

            return Task.FromResult(true);
        }


        public Task<bool> party3_room1()
        {

            playPortalSound();
            warp(920010200, 13);
            return Task.FromResult(true);
        }


        public Task<bool> party3_room2()
        {

            playPortalSound();
            warp(920010300, 1);
            return Task.FromResult(true);
        }


        public Task<bool> party3_room3()
        {

            playPortalSound();
            warp(920010400, 8);
            return Task.FromResult(true);
        }


        public Task<bool> party3_room4()
        {

            playPortalSound();
            warp(920010500, 3);
            return Task.FromResult(true);
        }


        public Task<bool> party3_room5()
        {

            playPortalSound();
            warp(920010600, 17);
            return Task.FromResult(true);
        }


        public Task<bool> party3_room6()
        {

            playPortalSound();
            warp(920010700, 23);
            return Task.FromResult(true);
        }


        public Task<bool> party3_room8()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg8") == 1)
            {
                playPortalSound();
                warp(920011000, 0);
                return Task.FromResult(true);
            }
            else
            {
                Pink("The storage is currently inaccessible, as the powers of the Pixies remains active within the tower.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> party3_roomout()
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

            playPortalSound();
            warp(920010100, exitPortal);
            return Task.FromResult(true);
        }


        public Task<bool> party6_out()
        {

            var eim = GetEventInstanceTrust();
            if (eim == null)
                return Task.FromResult(false);

            if (eim.isEventCleared())
            {
                if (isEventLeader())
                {
                    playPortalSound();
                    eim.warpEventTeam(930000800);
                    return Task.FromResult(true);
                }
                else
                {
                    Pink(nameof(ClientMessage.Tip_WaitForLeaderEnterPortal));
                    return Task.FromResult(false);
                }
            }
            else
            {
                Pink(nameof(ClientMessage.EllinPQ_NeedDefeatBossFirst));
                return Task.FromResult(false);
            }
        }

        [ScriptTag(["EllinPQ"])]
        public Task<bool> party6_stage()
        {

            switch (getMapId())
            {
                case 930000000:
                    playPortalSound();
                    warp(930000100, 0);
                    return Task.FromResult(true);
                case 930000100:
                    if (getMap().countMonsters() == 0)
                    {
                        playPortalSound();
                        warp(930000200, 0);
                        return Task.FromResult(true);
                    }
                    else
                    {
                        Pink(nameof(ClientMessage.Tip_EliminateAllMonster));
                        return Task.FromResult(false);
                    }
                case 930000200:
                    if (getMap().getReactorByName("spine") != null && getMap().getReactorByName("spine")?.getState() < 4)
                    {
                        Pink(nameof(ClientMessage.EllinPQ_SpineBlockWay));
                        return Task.FromResult(false);
                    }
                    else
                    {
                        playPortalSound();
                        warp(930000300, 0); //assuming they cant get past reactor without it being gone
                        return Task.FromResult(true);
                    }
                default:
                    Pink(nameof(ClientMessage.Tip_UnknownPortal));
                    return Task.FromResult(false);
            }
        }


        public Task<bool> party6_stage501()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "02st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage502()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "03st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage503()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "04st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage504()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "05st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage505()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "06st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage506()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "07st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage507()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "08st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage508()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "09st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage509()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "10st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage510()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "11st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage511()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "12st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage512()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "13st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage513()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "14st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage514()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "15st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage515()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                playPortalSound();
                warp(930000300, "16st");
            }
            else
            {
                playPortalSound();
                warp(930000300, "01st");
            }

            return Task.FromResult(true);
        }


        public Task<bool> party6_stage800()
        {

            var eim = GetEventInstanceTrust();
            if (eim != null)
            {
                eim.dropExclusiveItems(getPlayer());

                var spring = getMap().getReactorById(3008000);  // thanks Chloek3, seth1 for noticing fragments not being awarded properly
                if (spring != null && spring.getState() > 0)
                {
                    if (!canHold(4001198, 1))
                    {
                        Pink("Tip_CheckEtcSizeBeforeEnterPortal");
                        return Task.FromResult(false);
                    }

                    gainItem(4001198, 1);
                }
            }

            playPortalSound();
            warp(300030100, 0);
            return Task.FromResult(true);
        }


        public Task<bool> Pianus()
        {

            playPortalSound();
            warp(230040420, "out00");
            return Task.FromResult(true);
        }


        public Task<bool> Pinkin()
        {

            playPortalSound();
            warp(270050100);
            return Task.FromResult(true);
        }


        public Task<bool> Populatus00()
        {

            if (!((isQuestStarted(6361) && haveItem(4031870, 1)) || (isQuestCompleted(6361) && !isQuestCompleted(6363))))
            {
                var em = GetEventManager<PartyQuestEventManager>("PapulatusBattle");

                var party = getParty();
                if (party == null)
                {
                    playerMessage(5, "You are currently not in a party, create one to attempt the boss.");
                    return Task.FromResult(false);
                }
                else if (!isLeader())
                {
                    playerMessage(5, "Your party leader must enter the portal to start the battle.");
                    return Task.FromResult(false);
                }
                else
                {
                    var eli = em.getEligibleParty(party);
                    if (eli.Count > 0)
                    {
                        if (!em.StartPQInstance(getPlayer(), eli))
                        {
                            playerMessage(5, "The battle against the boss has already begun, so you may not enter this place yet.");
                            return Task.FromResult(false);
                        }
                    }
                    else
                    {  //this should never appear
                        playerMessage(5, "You cannot start this battle yet, because either your party is not in the range size, some of your party members are not eligible to attempt it or they are not in this map. If you're having trouble finding party members, try Party Search.");
                        return Task.FromResult(false);
                    }

                    playPortalSound();
                    return Task.FromResult(true);
                }
            }
            else
            {
                playPortalSound();
                warp(922020300, 0);
                return Task.FromResult(true);
            }
        }


        public Task<bool> PPinkOut()
        {

            playPortalSound();
            warp(270050000);
            return Task.FromResult(true);
        }


        public Task<bool> q2073()
        {

            if (isQuestStarted(2073))
            {
                playPortalSound();
                warp(900000000, 0);
                return Task.FromResult(true);
            }
            else
            {
                message("Private property. This place can only be entered when running an errand from Camila.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> q3366in()
        {

            if (isQuestStarted(3366))
            {
                playPortalSound();
                warp(926130101, 0);
                return Task.FromResult(true);
            }
            else
            {
                message("You don't have permission to access this room.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> q3366out()
        {

            playPortalSound();
            warp(926130100, "in00");
            return Task.FromResult(true);
        }


        public Task<bool> q3367in()
        {

            if (isQuestStarted(3367))
            {
                var booksDone = getQuestProgressInt(3367, 31);
                var booksInv = getItemQuantity(4031797);

                if (booksInv < booksDone)
                {
                    gainItem(4031797, booksDone - booksInv);
                }

                playPortalSound();
                warp(926130102, 0);
                return Task.FromResult(true);
            }
            else
            {
                message("You don't have permission to access this room.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> q3367out()
        {

            playPortalSound();
            warp(926130100, "in01");
            return Task.FromResult(true);
        }


        public Task<bool> q3368in()
        {

            if (isQuestStarted(3368))
            {
                playPortalSound();
                warp(926130103, 0);
                return Task.FromResult(true);
            }
            else
            {
                message("You don't have permission to access this room.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> q3368out()
        {

            playPortalSound();
            warp(926130100, "in02");
            return Task.FromResult(true);
        }


        public Task<bool> raidout()
        {

            var map = getPlayer().getSavedLocation("BOSSPQ");
            if (map == -1)
            {
                map = 100000000;
            }

            playPortalSound();
            warp(map, 0);
            return Task.FromResult(true);
        }


        public Task<bool> raid_rest()
        {

            var evLevel = ((getMapId() - 1) % 5) + 1;

            var eim = GetEventInstanceTrust();
            if (eim.isEventLeader(getPlayer()) && eim.getPlayerCount() > 1)
            {
                message("Being the party leader, you cannot leave before your teammates leave first or you pass leadership.");
                return Task.FromResult(false);
            }

            if (eim.giveEventReward(getPlayer(), evLevel))
            {
                playPortalSound();
                warp(970030000);
                return Task.FromResult(true);
            }
            else
            {
                message("Make a room available on all EQUIP, USE, SET-UP and ETC inventory to claim an instance prize.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> raid_stage()
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

                playPortalSound();
                warp(nextStage);
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(6, "Defeat all monsters before proceeding to the next stage.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rankDeveloperRoom()
        {

            if (getPlayer().getMapId() != 777777777)
            {
                if (!CanEnterDeveloperRoom())
                {
                    message("The next room is currently unavailable.");
                    return Task.FromResult(false);
                }

                getPlayer().saveLocation("DEVELOPER");
                playPortalSound();
                warp(777777777, "out00");
            }
            else
            {
                try
                {
                    var toMap = getPlayer().getSavedLocation("DEVELOPER");
                    playPortalSound();
                    warp(toMap, "in00");
                }
                catch (Exception)
                {
                    playPortalSound();
                    warp(100000000, 0);
                }
            }

            return Task.FromResult(true);
        }


        public Task<bool> rankRoom()
        {

            playPortalSound();

            switch (getPlayer().getMapId())
            {
                case 130000000:
                    warp(130000100, 5); //or 130000101
                    break;
                case 130000200:
                    warp(130000100, 4); //or 130000101
                    break;
                case 140010100:
                    warp(140010110, 1); //or 140010111
                    break;
                case 120000101:
                    warp(120000105, 1);
                    break;
                case 103000003:
                    warp(103000008, 1); //or 103000009
                    break;
                case 100000201:
                    warp(100000204, 2); //or 100000205
                    break;
                case 101000003: // portal warp fix thanks to Vcoc
                    warp(101000004, 2); //or 101000005
                    break;
                default:
                    warp(getMapId() + 1, 1); //or + 2
                    break;
            }

            return Task.FromResult(true);
        }


        public Task<bool> reundodraco()
        {

            blockPortal();
            return Task.FromResult(true);
        }


        public Task<bool> rienCaveEnter()
        {

            if (isQuestStarted(21201) || isQuestStarted(21302))
            { //aran first job
                playPortalSound();
                warp(140030000, 1);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "Something seems to be blocking this portal!");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rienTutor1()
        {

            if (!isQuestCompleted(21010))
            {
                message("You must complete the quest before proceeding to the next map.");
                return Task.FromResult(false);
            }
            playPortalSound();
            warp(140090200, 1);
            return Task.FromResult(true);
        }


        public Task<bool> rienTutor2()
        {

            if (!isQuestCompleted(21011))
            {
                message("You must complete the quest before proceeding to the next map..");
                return Task.FromResult(false);
            }
            playPortalSound();
            warp(140090300, 1);
            return Task.FromResult(true);
        }


        public Task<bool> rienTutor3()
        {

            if (!isQuestCompleted(21012))
            {
                message("You must complete the quest before proceeding to the next map..");
                return Task.FromResult(false);
            }
            playPortalSound();
            warp(140090400, 1);
            return Task.FromResult(true);
        }


        public Task<bool> rienTutor4()
        {

            if (!isQuestCompleted(21013))
            {
                message("You must complete the quest before proceeding to the next map..");
                return Task.FromResult(false);
            }
            playPortalSound();
            warp(140090500, 1);
            return Task.FromResult(true);
        }


        public Task<bool> rienTutor5()
        {

            talkGuide("你离城镇很近了。我先去那边处理一些事情，你慢慢来。");
            blockPortal();
            return Task.FromResult(false);
        }


        public Task<bool> rienTutor6()
        {

            removeGuide();
            blockPortal();
            return Task.FromResult(true);
        }


        public Task<bool> rienTutor7()
        {

            if (getPlayer().getJob().getId() == 2000 && !isQuestCompleted(21014))
            {
                showInfoText("The town of Rien is to the right. Take the portal on the right and go into town to meet Lilin.");
                return Task.FromResult(false);
            }
            else
            {
                playPortalSound();
                warp(140010100, 2);
                return Task.FromResult(true);
            }
        }


        public Task<bool> rienTutor8()
        {

            if (getPlayer().getJob().getId() == 2000)
            {
                if (isQuestStarted(21015))
                {
                    showInfoText("You must exit to the right in order to find Murupas.");
                    return Task.FromResult(false);
                }
                else if (isQuestStarted(21016))
                {
                    showInfoText("You must exit to the right in order to find Murupias.");
                    return Task.FromResult(false);
                }
                else if (isQuestStarted(21017))
                {
                    showInfoText("You must exit to the right in order to find MuruMurus.");
                    return Task.FromResult(false);
                }
            }
            playPortalSound();
            warp(140010000, 2);
            return Task.FromResult(true);
        }


        public Task<bool> rnj12_in()
        {

            playPortalSound();
            warp(926100401, 0); //next
            return Task.FromResult(true);
        }


        public Task<bool> rnj1_out()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg2") == 1)
            {
                playPortalSound();
                warp(926100100, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rnj1_pt00()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg1") == 1)
            {
                playPortalSound();
                warp(926100001, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rnj2_out()
        {

            if (GetEventInstanceTrust().getIntProperty("statusStg3") == 3)
            {
                playPortalSound();
                warp(926100200, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rnj3_in0()
        {

            if (getMap().getReactorByName("rnj3_out1")?.getState() == 1)
            {
                playPortalSound();
                warp(926100201, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The door is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rnj3_in1()
        {

            if (getMap().getReactorByName("rnj3_out2")?.getState() == 1)
            {
                playPortalSound();
                warp(926100202, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The door is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rnj3_out()
        {

            if (getMap().getReactorByName("rnj3_out3")?.getState() == 1)
            {
                playPortalSound();
                warp(926100203, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The door is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rnj4_r1()
        {

            var eim = GetEventInstanceTrust();
            var area = eim.getIntProperty("statusStg5");
            var reg = 0;

            if ((area >> reg) % 2 == 0)
            {
                area |= (1 << reg);
                eim.setIntProperty("statusStg5", area);

                playPortalSound();
                warp(926100301 + reg, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "This room is already being explored.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rnj4_r2()
        {

            var eim = GetEventInstanceTrust();
            var area = eim.getIntProperty("statusStg5");
            var reg = 1;

            if ((area >> reg) % 2 == 0)
            {
                area |= (1 << reg);
                eim.setIntProperty("statusStg5", area);

                playPortalSound();
                warp(926100301 + reg, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "This room is already being explored.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rnj4_r3()
        {

            var eim = GetEventInstanceTrust();
            var area = eim.getIntProperty("statusStg5");
            var reg = 2;

            if ((area >> reg) % 2 == 0)
            {
                area |= (1 << reg);
                eim.setIntProperty("statusStg5", area);

                playPortalSound();
                warp(926100301 + reg, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "This room is already being explored.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rnj4_r4()
        {

            var eim = GetEventInstanceTrust();
            var area = eim.getIntProperty("statusStg5");
            var reg = 3;

            if ((area >> reg) % 2 == 0)
            {
                area |= (1 << reg);
                eim.setIntProperty("statusStg5", area);

                playPortalSound();
                warp(926100301 + reg, 0); //next
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "This room is already being explored.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rnj5_rp()
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
                    playPortalSound();
                    warp(getMapId(), getPortal().getId() + 4);
                }
                else
                {
                    if (eim.getIntProperty("statusStg6") == 0)
                    {
                        eim.setIntProperty("statusStg6", 1);
                        eim.giveEventPlayersStageReward(6);
                    }

                    playPortalSound();
                    warp(getMapId(), 1);
                }

            }
            else
            {    //fail
                playPortalSound();
                warp(getMapId(), 2);
            }

            return Task.FromResult(true);
        }


        public Task<bool> rnj6_out()
        {

            if (getMap().getReactorByName("rnj6_out")?.getState() == 1)
            {
                playPortalSound();
                warp(926100300, 0);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The portal is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rnj_201_0()
        {

            if (getMap().getReactorByName("rnj31_out")?.getState() == 1)
            {
                playPortalSound();
                warp(926100200, 1);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The door is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rnj_202()
        {

            if (getMap().getReactorByName("rnj32_out")?.getState() == 1)
            {
                playPortalSound();
                warp(926100200, 2);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "The door is not opened yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> rnj_exit()
        {

            playPortalSound();
            warp(261000011, 0);
            return Task.FromResult(true);
        }


        public Task<bool> s4berserk()
        {

            if (isQuestStarted(6153) && haveItem(4031475))
            {
                var mapobj = getWarpMap(910500200);
                if (mapobj.countPlayers() == 0)
                {
                    resetMapObjects(910500200);
                    mapobj.shuffleReactors();
                    playPortalSound();
                    warp(910500200, "out01");

                    return Task.FromResult(true);
                }
                else
                {
                    getPlayer().message("Some other player is currently inside.");
                    return Task.FromResult(false);
                }
            }
            else
            {
                getPlayer().message("A mysterious force won't let you in.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> s4berserk_move()
        {

            if (getPlayer().getMap().countMonsters() == 0)
            {
                playPortalSound();
                warp(910500200, "out00");
                return Task.FromResult(true);
            }
            getPlayer().dropMessage(5, "You must defeat all the monsters first.");
            return Task.FromResult(true);
        }


        public Task<bool> s4common1_exit()
        {

            if (hasItem(4031495))
            {
                playPortalSound();
                warp(921100301);
            }
            else
            {
                playPortalSound();
                warp(211040100);
            }

            return Task.FromResult(true);
        }


        public Task<bool> s4firehawk()
        {

            if (isQuestStarted(6240))
            {
                if (getWarpMap(921100200).countPlayers() == 0)
                {
                    resetMapObjects(921100200);
                    playPortalSound();
                    warp(921100200, 0);

                    return Task.FromResult(true);
                }
                else
                {
                    getPlayer().message("Some other player is currently inside.");
                    return Task.FromResult(false);
                }
            }
            else
            {
                getPlayer().message("A mysterious force won't let you in.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> s4hitman()
        {

            if (isQuestStarted(6201))
            {
                if (getWarpMap(910200000).countPlayers() == 0)
                {
                    resetMapObjects(910200000);
                    playPortalSound();
                    warp(910200000, 0);

                    return Task.FromResult(true);
                }
                else
                {
                    getPlayer().message("Some other player is currently inside.");
                    return Task.FromResult(false);
                }
            }

            getPlayer().message("A mysterious force won't let you in.");
            return Task.FromResult(false);
        }


        public Task<bool> s4iceeagle()
        {

            if (isQuestStarted(6242))
            {
                if (getWarpMap(921100210).countPlayers() == 0)
                {
                    resetMapObjects(921100210);
                    playPortalSound();
                    warp(921100210, 0);

                    return Task.FromResult(true);
                }
                else
                {
                    getPlayer().message("Some other player is currently inside.");
                    return Task.FromResult(false);
                }
            }
            else
            {
                getPlayer().message("A mysterious force won't let you in.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> s4mind_end()
        {

            if (!GetEventInstanceTrust().isEventCleared())
            {
                message("You have to clear this mission before entering this portal.");
                return Task.FromResult(false);
            }
            else
            {
                if (isQuestStarted(6410))
                {
                    setQuestProgress(6410, 6411, "p2");
                }

                playPortalSound();
                warp(925010400);
                return Task.FromResult(true);
            }
        }


        public Task<bool> s4nest()
        {

            if (isQuestStarted(6241) || isQuestStarted(6243))
            {
                if (getWarpMap(924000100).countPlayers() == 0)
                {
                    resetMapObjects(924000100);
                    playPortalSound();
                    warp(924000100, 0);

                    return Task.FromResult(true);
                }
                else
                {
                    getPlayer().message("Some other player is currently inside.");
                    return Task.FromResult(false);
                }
            }

            getPlayer().message("A mysterious force won't let you in.");
            return Task.FromResult(false);
        }


        public Task<bool> s4resurrection()
        {

            if (haveItem(4001108))
            {
                if (getWarpMap(923000100).countPlayers() == 0)
                {
                    resetMapObjects(923000100);
                    playPortalSound();
                    warp(923000100, 0);

                    return Task.FromResult(true);
                }
                else
                {
                    getPlayer().message("Some other player is currently inside.");
                    return Task.FromResult(false);
                }
            }

            getPlayer().message("A mysterious force won't let you in.");
            return Task.FromResult(false);
        }


        public Task<bool> s4resur_enter()
        {

            if (isQuestStarted(6134))
            {
                playPortalSound();
                warp(922020000, 0);
                return Task.FromResult(true);
            }

            getPlayer().message("A mysterious force won't let you in.");
            return Task.FromResult(false);
        }


        public Task<bool> s4resur_out()
        {

            if (isQuestStarted(6134))
            {
                if (canHold(4031448))
                {
                    gainItem(4031448, 1);
                    playPortalSound();
                    warp(220070400, 3);

                    return Task.FromResult(true);
                }
                else
                {
                    getPlayer().message("Make room on your ETC to receive the quest item.");
                    return Task.FromResult(false);
                }
            }
            else
            {
                playPortalSound();
                warp(220070400, 3);
                return Task.FromResult(true);
            }
        }


        public Task<bool> s4rush()
        {

            if (isQuestStarted(6110))
            {
                playPortalSound();
                warp(910500100, 0);
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().message("A mysterious force won't let you in.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> s4ship_out()
        {

            var exit = GetEventInstanceTrust().getIntProperty("canLeave");
            if (exit == 0)
            {
                message("You have to wait one minute before you can leave this place.");
                return Task.FromResult(false);
            }
            else if (exit == 2)
            {
                playPortalSound();
                warp(912010200);
                return Task.FromResult(true);
            }
            else
            {
                playPortalSound();
                warp(120000101);
                return Task.FromResult(true);
            }
        }


        public Task<bool> s4super_out()
        {

            var exit = GetEventInstanceTrust().getIntProperty("canLeave");
            if (exit == 0)
            {
                message("You have to wait one minute before you can leave this place.");
                return Task.FromResult(false);
            }
            else if (exit == 2)
            {
                playPortalSound();
                warp(912010200);
                return Task.FromResult(true);
            }
            else
            {
                playPortalSound();
                warp(120000101);
                return Task.FromResult(true);
            }
        }


        public Task<bool> s4tornado_enter()
        {

            if (isQuestStarted(6230) || isQuestStarted(6231) || haveItem(4001110))
            {
                if (getWarpMap(922020200).countPlayers() == 0)
                {
                    resetMapObjects(922020200);
                    playPortalSound();
                    warp(922020200, 0);

                    return Task.FromResult(true);
                }
                else
                {
                    getPlayer().message("Some other player is currently inside.");
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(false);
        }


        public Task<bool> secretDoor()
        {

            if (isQuestCompleted(3360))
            {
                playPortalSound();
                warp(261030000, "sp_" + ((getMapId() == 261010000) ? "jenu" : "alca"));
                return Task.FromResult(true);
            }
            else if (isQuestStarted(3360))
            {
                openNpc(2111024, "MagatiaPassword");
                return Task.FromResult(false);
            }
            else
            {
                message("门锁住了.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> secretgate1_open()
        {

            if (getPlayer().getMap().getReactorByName("secretgate1")?.getState() == 1)
            {
                playPortalSound();
                warp(990000611, 1);
                return Task.FromResult(true);
            }
            else
            {
                playerMessage(5, "This door is closed.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> secretgate2_open()
        {

            if (getPlayer().getMap().getReactorByName("secretgate2")?.getState() == 1)
            {
                playPortalSound();
                warp(990000631, 1);
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(5, "This door is closed.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> secretgate3_open()
        {

            if (getPlayer().getMap().getReactorByName("secretgate3")?.getState() == 1)
            {
                playPortalSound();
                warp(990000641, 1);
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(5, "This door is closed.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> skyrom()
        {

            if (isQuestStarted(3935) && !haveItem(4031574, 1))
            {
                if (getWarpMap(926000010).countPlayers() == 0)
                {
                    playPortalSound();
                    warp(926000010, 0);
                    return Task.FromResult(true);
                }
                else
                {
                    message("Someone is already trying this map.");
                    return Task.FromResult(false);
                }
            }
            else
            {
                return Task.FromResult(false);
            }
        }


        public Task<bool> Spacegaga_out0()
        {

            var eim = GetEventInstanceTrust();
            var fc = eim.getIntProperty("falls");

            if (fc >= 3)
            {
                playPortalSound();
                warp(922240200, 0);
            }
            else
            {
                eim.setIntProperty("falls", fc + 1);
                playPortalSound();
                warp(getPlayer().getMapId(), 0);
            }

            return Task.FromResult(true);
        }


        public Task<bool> Spacegaga_out1()
        {

            var eim = GetEventInstanceTrust();
            var fc = eim.getIntProperty("falls");

            if (fc >= 3)
            {
                playPortalSound();
                warp(922240200, 0);
            }
            else
            {
                eim.setIntProperty("falls", fc + 1);
                playPortalSound();
                warp(getPlayer().getMapId(), 0);
            }

            return Task.FromResult(true);
        }


        public Task<bool> Spacegaga_out2()
        {

            var eim = GetEventInstanceTrust();
            var fc = eim.getIntProperty("falls");

            if (fc >= 3)
            {
                playPortalSound();
                warp(922240200, 0);
            }
            else
            {
                eim.setIntProperty("falls", fc + 1);
                playPortalSound();
                warp(getPlayer().getMapId(), 0);
            }

            return Task.FromResult(true);
        }


        public Task<bool> Spacegaga_out3()
        {

            var eim = GetEventInstanceTrust();
            var fc = eim.getIntProperty("falls");

            if (fc >= 3)
            {
                playPortalSound();
                warp(922240200, 0);
            }
            else
            {
                eim.setIntProperty("falls", fc + 1);
                playPortalSound();
                warp(getPlayer().getMapId(), 0);
            }

            return Task.FromResult(true);
        }


        public Task<bool> space_return()
        {

            playPortalSound();
            warp(getPlayer().getSavedLocation("EVENT"));
            return Task.FromResult(true);
        }


        public Task<bool> speargate_open()
        {

            if (getPlayer().getMap().getReactorByName("speargate")?.getState() == 4)
            {
                playPortalSound();
                warp(990000401, 0);
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(5, "This way forward is not open yet.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> stageBogo()
        {

            playPortalSound();
            warp(670010800, 0);
            return Task.FromResult(true);
        }


        public Task<bool> statuegate_open()
        {

            if (getPlayer().getMap().getReactorByName("statuegate")?.getState() == 1)
            {
                playPortalSound();
                warp(990000301, 0);
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(5, "The gate is closed.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> stonegate_open()
        {

            if (getPlayer().getMap().getReactorByName("stonegate")?.getState() == 1)
            {
                playPortalSound();
                warp(990000430, 0);
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(5, "The door is still blocked.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> subway_in2()
        {

            playPortalSound();
            warp(103000101, 3);
            return Task.FromResult(true);
        }


        public Task<bool> tamepig_out2()
        {

            if (!(haveItem(4031507, 5) && haveItem(4031508, 5) && isQuestStarted(6002)))
            {
                removeAll(4031507);
                removeAll(4031508);
            }

            var pCount = getPlayer().countItem(4031507);
            var rCount = getPlayer().countItem(4031508);

            if (pCount > 5)
            {
                gainItem(4031507, -1 * (pCount - 5));
            }
            if (rCount > 5)
            {
                gainItem(4031508, -1 * (rCount - 5));
            }

            playPortalSound();
            warp(230000003, "out00");
            return Task.FromResult(true);
        }


        public Task<bool> TD_Boss_enter()
        {

            var stage = (int)((Math.Floor(getMapId() / 100.0)) % 10) - 1;
            var em = GetEventManager<PartyQuestEventManager>("TD_Battle" + stage);
            if (em == null)
            {
                playerMessage(5, "TD Battle " + stage + " encountered an unexpected error and is currently unavailable.");
                return Task.FromResult(false);
            }

            var party = getParty();
            if (party == null)
            {
                playerMessage(5, "You are currently not in a party, create one to attempt the boss.");
                return Task.FromResult(false);
            }
            else if (!isLeader())
            {
                playerMessage(5, "Your party leader must enter the portal to start the battle.");
                return Task.FromResult(false);
            }
            else
            {
                var eli = em.getEligibleParty(party);
                if (eli.Count > 0)
                {
                    if (!em.StartPQInstance(getPlayer(), eli))
                    {
                        playerMessage(5, "The battle against the boss has already begun, so you may not enter this place yet.");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    playerMessage(5, "Your party must consist of at least 2 players to attempt the boss.");
                    return Task.FromResult(false);
                }

                playPortalSound();
                return Task.FromResult(true);
            }
        }


        public Task<bool> TD_chat_enter()
        {

            openNpc(2083006);
            return Task.FromResult(false);
        }


        public Task<bool> TD_MC_Egate()
        {

            playPortalSound();
            warp(106021300, 1);
            return Task.FromResult(true);
        }


        public Task<bool> TD_MC_enterboss1()
        {

            var questProgress = getQuestProgressInt(2330, 3300005) + getQuestProgressInt(2330, 3300006) + getQuestProgressInt(2330, 3300007); //3 Yetis

            if (isQuestStarted(2330) && questProgress < 3)
            {
                openNpc(1300013);
            }
            else
            {
                playPortalSound();
                warp(106021401, 1);
            }

            return Task.FromResult(true);
        }


        public Task<bool> TD_MC_enterboss2()
        {

            if (isQuestCompleted(2331))
            {
                openNpc(1300013);
                return Task.FromResult(false);
            }

            if (isQuestCompleted(2333) && isQuestStarted(2331) && !hasItem(4001318))
            {
                getPlayer().message("玉玺丢失了？嗯，不用担心！凯文会帮您保密。");
                if (canHold(4001318))
                {
                    gainItem(4001318, 1);
                }
                else
                {
                    getPlayer().message("嘿，你背包空间已经满了，如何拿取蘑菇王国玉玺？");
                }
            }

            if (isQuestCompleted(2333))
            {
                playPortalSound();
                warp(106021600, 1);
                return Task.FromResult(true);
            }
            else if (isQuestStarted(2332) && hasItem(4032388))
            {
                forceCompleteQuest(2332, 1300002);
                getPlayer().message("找到了公主！");
                giveCharacterExp(4400, getPlayer());

                var em = GetEventManager<PartyQuestEventManager>("MK_PrimeMinister");
                var party = getPlayer().getParty();
                if (party != null)
                {
                    var eli = em.getEligibleParty(party);   // thanks Conrad for pointing out missing eligible party declaration here
                    if (eli.Count > 0)
                    {
                        if (em.StartPQInstance(getPlayer(), eli))
                        {
                            playPortalSound();
                            return Task.FromResult(true);
                        }
                        else
                        {
                            message("有其它团队正在此频道挑战BOSS。");
                            return Task.FromResult(false);
                        }
                    }
                }
                else
                {
                    if (em.startInstance(getPlayer()))
                    { // thanks RedHat for noticing an issue here
                        playPortalSound();
                        return Task.FromResult(true);
                    }
                    else
                    {
                        message("有其它团队正在此频道挑战BOSS。");
                        return Task.FromResult(false);
                    }
                }

                return Task.FromResult(false);
            }
            else if (isQuestStarted(2333) || (isQuestCompleted(2332) && !isQuestStarted(2333)))
            {
                var em = GetEventManager<PartyQuestEventManager>("MK_PrimeMinister");

                var party = getPlayer().getParty();
                if (party != null)
                {
                    var eli = em.getEligibleParty(party);
                    if (eli.Count > 0)
                    {
                        if (em.StartPQInstance(getPlayer(), eli))
                        {
                            playPortalSound();
                            return Task.FromResult(true);
                        }
                        else
                        {
                            message("有其它团队正在此频道挑战BOSS。");
                            return Task.FromResult(false);
                        }
                    }
                }
                else
                {
                    if (em.startInstance(getPlayer()))
                    {
                        playPortalSound();
                        return Task.FromResult(true);
                    }
                    else
                    {
                        message("有其它团队正在此频道挑战BOSS。");
                        return Task.FromResult(false);
                    }
                }
            }
            else
            {
                getPlayer().message("门似乎已经被锁住了，需要找到开启门的钥匙……");
                return Task.FromResult(false);
            }

            return Task.FromResult(false);
        }


        public Task<bool> TD_MC_first()
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
                playPortalSound();
                warp(106020000, 0);
                return Task.FromResult(true);
            }
            playerMessage(5, "A strange force is blocking you from entering.");
            return Task.FromResult(false);
        }


        public Task<bool> TD_MC_jump()
        {

            playPortalSound();
            warp(106020501, 0);
            return Task.FromResult(true);
        }


        public Task<bool> TD_neo_inTree()
        {

            var nex = getEventManager("GuardianNex") as SoloEventManager;
            if (nex == null)
            {
                message("Guardian Nex challenge encountered an error and is unavailable.");
                return Task.FromResult(false);
            }

            int[] quests = [3719, 3724, 3730, 3736, 3742, 3748];
            int[] mobs = [7120100, 7120101, 7120102, 8120100, 8120101, 8140510];

            for (var i = 0; i < quests.Length; i++)
            {
                if (isQuestActive(quests[i]))
                {
                    if (getQuestProgressInt(quests[i], mobs[i]) != 0)
                    {
                        message("You already faced Nex. Complete your mission.");
                        return Task.FromResult(false);
                    }

                    if (!nex.startInstance(i, getPlayer()))
                    {
                        message("Someone is already challenging Nex. Wait for them to finish before you enter.");
                        return Task.FromResult(false);
                    }
                    else
                    {
                        playPortalSound();
                        return Task.FromResult(true);
                    }
                }
            }

            message("A mysterious force won't let you in.");
            return Task.FromResult(false);
        }


        public Task<bool> templeenter()
        {

            cancelItem(2210016);
            playPortalSound();
            warp(270000100, "out00");
            return Task.FromResult(true);
        }


        public Task<bool> thief_in1()
        {

            // unexpected warp condition noticed thanks to IxianMace

            openNpc(1063011, "ThiefPassword");
            return Task.FromResult(false);
        }


        public Task<bool> timeQuest()
        {

            var mapid = getPlayer().getMapId();
            playPortalSound();
            var map = (mapid - 270010000) / 100;
            //getPlayer().dropMessage(5, map + " " + isQuestCompleted(3534));
            if (map < 5 && isQuestCompleted(3500 + map))
            {
                warp(mapid + 10, "out00");
            }
            else if (map == 5 && isQuestCompleted(3502 + map))
            {
                warp(270020000, "out00");
            }
            else if (map > 100 && map < 105 && isQuestCompleted(3407 + map))
            {
                warp(mapid + 10, "out00");
            }
            else if (map == 105 && isQuestCompleted(3514))
            {
                warp(270030000, "out00");
            }
            else if (map > 200 && map < 205 && isQuestCompleted(3314 + map))
            {
                warp(mapid + 10, "out00");
            }
            else if (map == 205 && isQuestCompleted(3519))
            {
                warp(270040000, "out00");
            }
            else if (map == 300 && (haveItem(4032002) || isQuestCompleted(3522)))
            {
                warp(270040100, "out00");
            }
            else
            {
                if (map > 200)
                {
                    playerMessage(5, "随着时间开始变得异常流动，你被传送回到了一个安全的空间。");
                    warp(270030000, "in00");
                }
                else if (map > 100)
                {
                    playerMessage(5, "随着时间开始变得异常流动，你被传送回到了一个安全的空间。");
                    warp(270020000, "in00");
                }
                else
                {
                    playerMessage(5, "随着时间开始变得异常流动，你被传送回到了一个安全的空间。");
                    warp(270010000, "in00");
                }
            }
            return Task.FromResult(true);
        }


        public Task<bool> tristanEnter()
        {

            if (isQuestCompleted(2238))
            {
                playPortalSound();
                warp(105100101, "in00");
                return Task.FromResult(true);
            }
            else
            {
                message("A mysterious force won't let you in.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> tutoChatNPC()
        {

            if (hasLevel30Character())
            {
                openNpc(2007);
            }
            blockPortal();
            return Task.FromResult(true);
        }


        public Task<bool> tutorHelper()
        {

            spawnGuide();
            talkGuide("Welcome to Maple World! I'm Mimo. I'm in charge of guiding you until you reach Lv. 10 and become a Knight-In-Training. Double-click for further information!");
            blockPortal();
            return Task.FromResult(true);
        }


        public Task<bool> tutorialNPC()
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
                    openNpc(npcid);
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }


        public Task<bool> tutorMinimap()
        {

            guideHint(1);
            blockPortal();
            return Task.FromResult(true);
        }


        public Task<bool> tutorquest()
        {

            if (getPlayer().getMapId() == 130030001)
            {
                if (isQuestStarted(20010))
                {
                    playPortalSound();
                    warp(130030002, 0);
                    return Task.FromResult(true);
                }
                else
                {
                    message("Please click on the NPC first to receive a quest.");
                }
            }
            else if (getPlayer().getMapId() == 130030002)
            {
                if (isQuestCompleted(20011))
                {
                    playPortalSound();
                    warp(130030003, 0);
                    return Task.FromResult(true);
                }
                else
                {
                    message("Please complete the required quest before proceeding.");
                }
            }
            else if (getPlayer().getMapId() == 130030003)
            {
                if (isQuestCompleted(20012))
                {
                    playPortalSound();
                    warp(130030004, 0);
                    return Task.FromResult(true);
                }
                else
                {
                    message("Please complete the required quest before proceeding.");
                }
            }
            else if (getPlayer().getMapId() == 130030004)
            {
                if (isQuestCompleted(20013))
                {
                    playPortalSound();
                    warp(130030005, 0);
                    return Task.FromResult(true);
                }
                else
                {
                    message("Please complete the required quest before proceeding.");
                }
            }

            return Task.FromResult(false);
        }


        public Task<bool> under30gate()
        {

            if (getPlayer().getLevel() <= 30)
            {
                playPortalSound();
                warp(990000640, 1);
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(5, "You cannot proceed past this point.");
                return Task.FromResult(false);
            }
        }


        public Task<bool> undodraco()
        {

            cancelItem(2210016);
            playPortalSound();
            warp(240000110, 2);
            return Task.FromResult(true);
        }


        public Task<bool> watergate_open()
        {

            if (getPlayer().getMap().getReactorByName("watergate")?.getState() == 1)
            {
                playPortalSound();
                warp(990000600, 1);
                return Task.FromResult(true);
            }
            else
            {
                getPlayer().dropMessage(5, "This way forward is not open yet.");
            }
            return Task.FromResult(false);
        }


        public Task<bool> Zakum03()
        {

            var eim = GetEventInstanceTrust();
            if (!eim.isEventCleared())
            {
                getPlayer().dropMessage(5, "你的队伍尚未完成试炼，请先完成奥拉的需求。");
                return Task.FromResult(false);
            }

            if (eim.gridCheck(getPlayer()) == -1)
            {
                getPlayer().dropMessage(5, "你还没有领取战利品，请先与Aura交谈。");
                return Task.FromResult(false);
            }

            playPortalSound();
            warp(211042300);
            return Task.FromResult(true);
        }


        public Task<bool> Zakum05()
        {

            if (!(isQuestStarted(100200) || isQuestCompleted(100200)))
            {
                getPlayer().dropMessage(5, "你需要得到大师们的准许才能挑战扎昆BOSS,你现在没有资格进入。");
                return Task.FromResult(false);
            }

            if (!isQuestCompleted(100201))
            {
                getPlayer().dropMessage(5, "你必须完成所有试炼任务才有资格进入。");
                return Task.FromResult(false);
            }

            if (!haveItem(4001017))
            {    // thanks Conrad for pointing out missing checks for token item and unused reactor
                getPlayer().dropMessage(5, "扎昆祭台需要 火焰之眼 ，否则无法召唤扎昆BOSS，请准备好所需物品再来挑战。");
                return Task.FromResult(false);
            }

            var react = getMap().getReactorById(2118002);
            if (react != null && react.getState() > 0)
            {
                getPlayer().dropMessage(5, "入口目前已被封锁，无法进入。");
                return Task.FromResult(false);
            }

            playPortalSound();
            warp(211042400, "west00");
            return Task.FromResult(true);
        }



    }
}