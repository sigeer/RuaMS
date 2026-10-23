using Application.Core.Client;
using Application.Core.Game.ContiMove;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.Server.maps;
using Application.Shared.Constants.Job;
using Application.Shared.GameProps;
using scripting.portal;
using System.Drawing;

namespace Application.Plugin.Script
{
    internal partial class PortalScript : PortalPlayerInteraction
    {
        public PortalScript(IChannelClient c, IPortal p) : base(c, p)
        {
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

        // Map: 40001, 40002, 50001, 60000 | pt: 9
        public async Task<bool> adviceMap()
        {

            await showInstruction("按#e#b[Up]#k箭头#n进入光圈移动到下一个地图。", 230, 5);
            return true;
        }

        // Map: 980010000 | pt: 7
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

        // Map: 670010300, 670010301, 670010302, 670010400, 670010500 | pt: 5
        public async Task<bool> apqClosed()
        {

            await Pink("大门还没有打开。");
            return false;
        }

        // Map: 670010600 | pt: 8
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

        // Map: 250010504 | pt: 8
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

        // Map: 100040105 | pt: 11
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

        // Map: 240000110 | pt: 7
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

        // Map: 222020100, 222020200 | pt: 8
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

        // Map: 200010300 | pt: 8
        public async Task<bool> eliza_Garden()
        {

            await playPortalSound();
            await warp(920020000, 2);
            return true;
        }

        // Map: 912000000 | pt: 7
        public async Task<bool> end_black()
        {

            await playPortalSound();
            await warp(120000200, 0);
            return true;
        }

        // Map: 912000100 | pt: 7
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

        // Map: 130000200 | pt: 7
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

        // Map: 130010000 | pt: 7
        public Task<bool> enterDisguise1() => enterDisguiseMaster(0, 130010010);

        // Map: 130010000 | pt: 7
        public Task<bool> enterDisguise2() => enterDisguiseMaster(1, 130010020);

        // Map: 130010100 | pt: 7
        public Task<bool> enterDisguise3() => enterDisguiseMaster(2, 130010110);

        // Map: 130010100 | pt: 7
        public Task<bool> enterDisguise4() => enterDisguiseMaster(3, 130010120);

        // Map: 130010200 | pt: 7
        public Task<bool> enterDisguise5() => enterDisguiseMaster(4, 130020000, "east00");

        /// <summary>
        /// 变身任务（20301~20305）进行中时进入对应职业的密林，否则退回上一层入口。
        /// private：脚本注册表收集的是 public 方法，辅助方法必须私有，否则会被当成脚本键。
        /// </summary>
        /// <param name="jobtype">职业序号，决定目标地图 108010600 + 10 * jobtype 与凭证 4032101 + jobtype</param>
        /// <param name="retreatMap">未接任务时退回到的地图</param>
        /// <param name="portal">目标地图与退回地图的落点（两者一致）</param>
        private async Task<bool> enterDisguiseMaster(int jobtype, int retreatMap, string portal = "out00")
        {
            bool inQuest = isQuestStarted(20301) || isQuestStarted(20302) || isQuestStarted(20303) || isQuestStarted(20304) || isQuestStarted(20305);
            if (!inQuest)
            {
                await playPortalSound();
                await warp(retreatMap, portal);
                return true;
            }

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

            switch (map.Id)
            {
                case 108010600:
                    await SpawnTutorNpc(map, 1104100, new Point(2830, 78));
                    break;
                case 108010610:
                    await SpawnTutorNpc(map, 1104101, new Point(3395, -322));
                    break;
                case 108010620:
                    await SpawnTutorNpc(map, 1104102, new Point(500, -522));
                    break;
                case 108010630:
                    await SpawnTutorNpc(map, 1104103, new Point(-2263, -582));
                    break;
                case 108010640:
                    await SpawnTutorNpc(map, 1104104, new Point(372, 70));
                    break;
                default:
                    break;
            }

            await playPortalSound();
            await warp(map.Id, portal);
            return true;
        }

        private async Task SpawnTutorNpc(IMap map, int npcId, Point pos)
        {
            if (map.getNPCById(npcId) != null)
                return;

            await spawnNpc(npcId, pos, map);
        }

        public async Task<bool> enterEvanRoom()
        {

            await playPortalSound();
            await warp(100030100, 0);
            return true;
        }

        // Map: 130020000 | pt: 7
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

        // Map: 140010100 | pt: 7
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

        // Map: 140030000 | pt: 8
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

        // Map: 140010000 | pt: 7
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

        // Map: 130020000 | pt: 7
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
                await TypedMessage(0, "重新进入第2演武场时将会清空背包里所有考试的证物，请务必注意。");
                return true;
            }
            else
            {
                await Pink("只有参加骑士指定的等级考试方可进入第2演武场。");
                return false;
            }
        }

        // Map: 130020000 | pt: 7
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

        // Map: 1010000 | pt: 7
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

        // Map: 300000010 | pt: 7
        public async Task<bool> enterWarehouse()
        {

            await playPortalSound();
            await warp(300000011, 0);
            return true;
        }

        // Map: 120000101 | pt: 8
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

        // Map: 221000300 | pt: 8
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

        // Map: 674030100 | pt: 8
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
            await ShowEffect("Effect/OnUserEff.img/guideEffect/evanTutorial/evanBalloon30");
            return true;
        }

        // Map: 670010200

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

        // Map: 670010200 | pt: 8
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

        // Map: 610030010 | pt: 8
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

        // glpq 传送门已在 Features/CWKPQ 中实现

        // Map: 10000, 20000, 30000 | pt: 9
        public async Task<bool> glTutoMsg0()
        {

            await showInstruction("离开这个区域，将无法再回来。", 150, 5);
            return true;
        }

        // Map: 240020100 | pt: 7
        public async Task<bool> gryphius()
        {

            await playPortalSound();
            await warp(240020101, "out00");
            return true;
        }

        // 公会任务传送门（guild1F00~guildwaitingexit）已在 Features/GuildQuest 中实现

        // Map: 674030000 | pt: 9
        public async Task<bool> guyfawkes0_floor()
        {

            await warp(674030000, 0);
            return true;
        }

        // Map: 682000000 | pt: 8
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

        // Map: 101010103, 102000000, 103000000, 120000000, 200080100 | pt: 9
        public async Task<bool> highposition()
        {

            // thanks kvmba for noticing some issues running this script
            await touchTheSky();
            return false;
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

        // Map: 101000000 | pt: 7
        public async Task<bool> inERShip()
        {

            await playPortalSound();
            await warp(101000400, 2);
            return true;
        }

        // Map: 40000 | pt: 9
        public async Task<bool> infoAttack()
        {

            if (isQuestStarted(1035))
            {
                await ShowEffect("UI/tutorial.img/20");
            }

            await blockPortal();
            return true;
        }

        // Map: 10000 | pt: 9
        public async Task<bool> infoMinimap()
        {

            if (isQuestStarted(1031))
            {
                await ShowEffect("UI/tutorial.img/25");
            }

            await blockPortal();
            return true;
        }

        // Map: 40000 | pt: 9
        public async Task<bool> infoPickup()
        {

            if (isQuestStarted(1035))
            {
                await ShowEffect("UI/tutorial.img/21");
            }

            await blockPortal();
            return true;
        }

        // Map: 1000000 | pt: 9
        public async Task<bool> infoReactor()
        {

            if (isQuestCompleted(1008))
            {
                await ShowEffect("UI/tutorial.img/22");
            }
            else if (isQuestCompleted(1020))
            {
                await ShowEffect("UI/tutorial.img/27");
            }

            await blockPortal();
            return true;
        }

        // Map: 40000 | pt: 9
        public async Task<bool> infoSkill()
        {

            if (isQuestCompleted(1035))
            {
                await ShowEffect("UI/tutorial.img/23");
            }

            await blockPortal();
            return true;
        }

        // Map: 50000 | pt: 9
        public async Task<bool> infoWorldmap()
        {

            await ShowEffect("UI/tutorial.img/26");
            await blockPortal();
            return true;
        }

        // Map: 240020101, 240020102 | pt: 7
        public async Task<bool> inNix1()
        {

            await playPortalSound();
            await warp(240020600, "out00");
            return true;
        }

        // Map: 240020401, 240020402 | pt: 7
        public async Task<bool> inNix2()
        {

            await playPortalSound();
            await warp(240020600, "out01");
            return true;
        }

        // Map: 300000010 | pt: 8
        public async Task<bool> jail_in()
        {

            await playPortalSound();
            await warp(300000012, "portal");
            return true;
        }

        // Map: 300000012 | pt: 8
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

        // kerning PQ 传送门（kpq0~kpq4）已在 Features/KerningPQ 中实现

        public async Task<bool> lionCastle_enter()
        {

            await playPortalSound();
            await warp(211060010, "west00");
            return true;
        }

        // ludi PQ 传送门（lpq0~lpq7）已在 Features/LudiPQ 中实现

        public async Task<bool> ludi021()
        {

            await removeAll(4031092);
            await warp(220020600);
            return true;
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

        // Map: 680100000, 680100001, 680100002, 680100003 | pt: 7
        public async Task<bool> mapleMarket7_out()
        {

            await playPortalSound();
            await warp(getPlayer().getSavedLocation("EVENT"));
            return true;
        }

        // Map: 910000000 | pt: 7
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

        // Map: 100000100 | pt: 7
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

        // Map: 220000000 | pt: 7
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

        // Map: 211000100 | pt: 7
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

        // Map: 102000000 | pt: 7
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

        // Map: 230000000 | pt: 7
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

        // Map: 221000000 | pt: 7
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

        // Map: 200000000 | pt: 7
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

        // Map: 801000300 | pt: 7
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

        // Map: 240000000, 701000200 | pt: 7, 8
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

        // Map: 250000000 | pt: 7
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

        // Map: 251000000 | pt: 7
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

        // Map: 600000000 | pt: 7
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

        // Map: 260000000, 702000000 | pt: 7
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

        // Map: 222000000, 701000100 | pt: 7
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

        // Map: 540000000 | pt: 7
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

        // Map: 541000000 | pt: 7
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

        // Map: 120000200 | pt: 7
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

        // Map: 261000000 | pt: 7
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

        // Map: 130000200 | pt: 7
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

        // Map: 550000000 | pt: 7
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

        // Map: 551000000 | pt: 7
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

        // Map: 140000000 | pt: 7
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

        // Map: 610010200 | pt: 8
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

        // Map: 610010201 | pt: 8
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

        // Map: 610010202 | pt: 8
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

        // Map: 610020010 | pt: 8
        public async Task<bool> Masteria_CC1_A()
        {

            await playPortalSound();
            await warp(610020015, "CC6_A");
            return true;
        }

        // Map: 610020015 | pt: 8
        public async Task<bool> Masteria_CC6_A()
        {

            await playPortalSound();
            await warp(610020010, "CC1_A");
            return true;
        }

        // Map: 610020000 | pt: 8
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

        // Map: 610020000 | pt: 8
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

        // Map: 610020000 | pt: 8
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

        // Map: 610020000 | pt: 8
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

        // Map: 610020001 | pt: 8
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

        // Map: 610020001 | pt: 8
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

        // Map: 610020001 | pt: 8
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

        // Map: 610020001 | pt: 8
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

        // Map: 610010001 | pt: 8
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

        // Map: 610010002 | pt: 8
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

        // Map: 610010004 | pt: 8
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

        // Map: 610010004 | pt: 8
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

        // Map: 610010005 | pt: 8
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

        // Map: 240020400 | pt: 7
        public async Task<bool> mayong()
        {

            await playPortalSound();
            await warp(240020401, "out00");
            return true;
        }

        // Map: 600000000 | pt: 7
        public async Task<bool> MD_cakeEnter()
        {

            await playPortalSound();
            await warp(674030100, "in00");
            return true;
        }

        // Map: 105090311, 105090320 | pt: 7
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

        // Map: 261020300, 261020301 | pt: 7
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

        // Map: 105040304, 105040320 | pt: 7
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

        // Map: 551030000, 551030001 | pt: 7
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

        // Map: 105050100, 105050101 | pt: 7
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

        // Map: 100020000, 100020100 | pt: 7
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

        // Map: 240040520, 240040900 | pt: 7, 8
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

        // Map: 221023400, 221023401 | pt: 7
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

        // Map: 240040511, 240040800 | pt: 7, 8
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

        // Map: 240020500, 240020501 | pt: 7, 8
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

        // Map: 260020600, 260020630 | pt: 7
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

        // Map: 251010402, 251010410 | pt: 7
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

        // Map: 910320000 | pt: 9
        public async Task<bool> metro_Chat00()
        {

            await ShowDirectionEffect("Effect/Direction2.img/metro/Im");
            //showWZEffect("Effect/Direction2.img/metro/Im");
            return true;
        }

        // Map: 910320000 | pt: 7
        public async Task<bool> metro_in00()
        {

            await openNpc(1052115);
            return true;
        }

        // Map: 102040000 | pt: 7
        public async Task<bool> met_in()
        {

            //warp(910320000, 2); event not implemented

            await playPortalSound();
            await warp(103000103, 1);
            return true;
        }

        // Map: 910320000 | pt: 7
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

        // Map: 101010000, 240010100 | pt: 8
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

        // Map: 240010500 | pt: 8
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

        // Map: 108000710 | pt: 7
        public async Task<bool> moveBefore()
        {

            await playPortalSound();
            await warp(getMapId() - 10, "west00");
            return true;
        }

        // Map: 108000700 | pt: 7
        public async Task<bool> moveNext()
        {

            await playPortalSound();
            await warp(getMapId() + 10, "east00");
            return true;
        }

        // Map: 222020400 | pt: 7
        public async Task<bool> move_elin()
        {

            await playPortalSound();
            await warp(300000100, "out00");
            await Pink("移动到时间之门的另一端。");
            return true;
        }

        // Map: 200090070, 200090071, 200090072, 200090073, 200090074, 200090075 … (共 10 张) | pt: 9
        public async Task<bool> move_RieRit()
        {

            return true;
        }

        // Map: 200090060, 200090061, 200090062, 200090063, 200090064, 200090065 … (共 10 张) | pt: 9
        public async Task<bool> move_RitRie()
        {

            return true;
        }

        // Map: 260020500 | pt: 7
        public async Task<bool> nets_in()
        {

            getPlayer().saveLocation("MIRROR");
            await playPortalSound();
            await warp(926010000, 4);
            return true;
        }

        // Map: 926010000 | pt: 7
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

        // Map: 980041000, 980041100, 980042000, 980042100, 980043000, 980043100 … (共 7 张) | pt: 9
        public async Task<bool> NextMap()
        {

            await playPortalSound();
            await warp(getMapId() + 100, 0);
            return true;
        }

        public async Task<bool> outArchterMap()
        {

            await playPortalSound();
            await warp(100000000, "Achter00");
            await playPortalSound();
            return true;
        }

        // Map: 914000300 | pt: 7
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

        // Map: 914020000 | pt: 7
        public async Task<bool> outMaha()
        {
            await playPortalSound();
            await WarpReturn(0);
            return true;
        }

        // Map: 240020600 | pt: 7
        public async Task<bool> outNix1()
        {

            await playPortalSound();
            await warp(240020101, "in00");
            return true;
        }

        // Map: 240020600 | pt: 7
        public async Task<bool> outNix2()
        {

            await playPortalSound();
            await warp(240020401, "in00");
            return true;
        }

        // Map: 105100100 | pt: 7
        public async Task<bool> outPerrion_1()
        {

            await Pink("You found a shortcut to the start of the underground temple.");
            await playPortalSound();
            await warp(105100000, 2);
            return true;
        }

        // Map: 105100100 | pt: 9
        public async Task<bool> outPerrion_2()
        {

            await playPortalSound();
            await warp(105100000, 0);
            return true;
        }

        // Map: 921110000 | pt: 7
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

        // Map: 925040001 | pt: 7
        public async Task<bool> outSpecialSchool()
        {

            await playPortalSound();
            await warp(925040000, 1);
            return true;
        }

        // Map: 270000100 | pt: 7
        public async Task<bool> outTemple()
        {

            await useItem(2210016);
            await playPortalSound();
            await warp(200090510, 0);
            return true;
        }

        // Map: 914030000 | pt: 7
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


        // Map: 230040410 | pt: 7
        public async Task<bool> Pianus()
        {

            await playPortalSound();
            await warp(230040420, "out00");
            return true;
        }

        // Map: 100030000 | pt: 7
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

        // Map: 926130100 | pt: 8
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

        // Map: 926130200 | pt: 7
        public async Task<bool> q3366out()
        {

            await playPortalSound();
            await warp(926130100, "in00");
            return true;
        }

        // Map: 926130201 | pt: 7
        public async Task<bool> q3367out()
        {

            await playPortalSound();
            await warp(926130100, "in01");
            return true;
        }

        // Map: 926130100 | pt: 8
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

        // Map: 926130203 | pt: 7
        public async Task<bool> q3368out()
        {

            await playPortalSound();
            await warp(926130100, "in02");
            return true;
        }

        // Map: 100000204, 101000004, 102000004, 103000008, 120000105, 130000120 … (共 8 张) | pt: 8
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

        // Map: 100000201, 101000003, 102000003, 103000003, 120000101, 130000000 … (共 8 张) | pt: 7
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

        // Map: 240000110 | pt: 9
        public async Task<bool> reundodraco()
        {

            await blockPortal();
            return true;
        }

        // Map: 140010200 | pt: 7
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

        // Map: 140090100 | pt: 7
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

        // Map: 140090200 | pt: 7
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

        // Map: 140090300 | pt: 7
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

        // Map: 140090400 | pt: 7
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

        // Map: 140090500 | pt: 9
        public async Task<bool> rienTutor5()
        {

            await talkGuide("前面不远就是村子了。我先走一步了，还有一些需要整理的东西。战神，慢慢过来就行。");
            await blockPortal();
            return false;
        }

        // Map: 140090500 | pt: 9
        public async Task<bool> rienTutor6()
        {

            await removeGuide();
            await blockPortal();
            return true;
        }

        // Map: 140010000 | pt: 7
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

        // Map: 140000000 | pt: 7
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

        // Map: 670010750 | pt: 7
        public async Task<bool> stageBogo()
        {

            await playPortalSound();
            await warp(670010800, 0);
            return true;
        }

        // Map: 103000100 | pt: 7
        public async Task<bool> subway_in2()
        {

            await playPortalSound();
            await warp(103000101, 3);
            return true;
        }

        // Map: 200090510 | pt: 7
        public async Task<bool> templeenter()
        {

            await cancelItem(2210016);
            await playPortalSound();
            await warp(270000100, "out00");
            return true;
        }

        // Map: 260010401 | pt: 8
        public async Task<bool> thief_in1()
        {

            // unexpected warp condition noticed thanks to IxianMace

            await openNpc(2103008);
            return false;
        }

        // Map: 105100100 | pt: 7
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

        // Map: 10000 | pt: 9
        public async Task<bool> tutoChatNPC()
        {

            if (hasLevel30Character())
            {
                await openNpc(2007);
            }
            await blockPortal();
            return true;
        }

        // Map: 130030000 | pt: 9
        public async Task<bool> tutorHelper()
        {
            await spawnGuide();
            await talkGuide("Welcome to Maple World! I'm Mimo. I'm in charge of guiding you until you reach Lv. 10 and become a Knight-In-Training. Double-click for further information!");
            await blockPortal();
            return true;
        }

        // Map: 100000201, 101000003, 102000003, 103000003, 120000101 | pt: 9
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

        // Map: 130030001 | pt: 9
        public async Task<bool> tutorMinimap()
        {
            await setQuestProgress(20010, 20022, 1);
            await guideHint(1);
            await blockPortal();
            return true;
        }

        // Map: 130030001, 130030002, 130030003, 130030004 | pt: 7
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

        // Map: 200090500, 200090510 | pt: 7, 9
        public async Task<bool> undodraco()
        {

            await cancelItem(2210016);
            await playPortalSound();
            await warp(240000110, 2);
            return true;
        }

        // Map: 130030006
        [ScriptName("tutorWorldmap")]
        public async Task<bool> p_tutorWorldmap()
        {
            await guideHint(11);
            await blockPortal();
            return true;
        }
    }
}