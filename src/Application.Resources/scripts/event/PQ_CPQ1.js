// 事件实例化变量
var name = "PQ_CPQ1";              // 非必须，事件名，否则使用文件名作为事件名
var eventType = "CPQ";          // 非必须，仅支持 "PartyQuest": 组队任务，"Solo": 单人任务, "GuildQuest": 家族任务，"Expedition": 远征队
var minPlayers, maxPlayers;     // 该事件实例允许的队伍成员数量范围。
var minLevel = 30;
var maxLevel = 50;          // 合格队伍成员的等级范围。
var entryMap;               // 事件启动时玩家进入的初始地图。
var exitMap;                // 玩家未能完成事件时被传送至此地图。
var recruitMap;             // 玩家必须在此地图上才能开始此事件。
var clearMap;               // 玩家成功完成事件后被传送至此地图。

var minMapId = 980000100;               // 事件发生在此地图ID区间内。若玩家超出此范围则立即从事件中移除。
var maxMapId = 980000604;

var eventTime = 3;              // 事件的最大允许时间，以分钟计。

const maxLobbies = 1;       // 并发活跃大厅的最大数量。

function init() {
    // 在ChannelServer加载后执行初始化操作。
    em.InsertRoom("cpq1_room1", 2, 980000100);
    em.InsertRoom("cpq1_room2", 2, 980000200);
    em.InsertRoom("cpq1_room3", 3, 980000300);
}

function getMaxLobbies() {
    return maxLobbies;
}

function setEventRequirements() {
    // 设置在招募区域显示的关于事件的要求信息。
}

function setEventExclusives(eim) {
    // 设置仅在事件实例中存在的物品，并在事件结束时从库存中移除这些物品。
}

function setEventRewards(eim) {
    // 设置所有可能的奖励，随机给予玩家作为事件结束时的奖品。
    expStages = [50000, 25500, 21000, 19505, 17500, 12000, 5000, 2500];    //bonus exp given on CLEAR stage signal
    eim.setEventClearStageExp(expStages);
}

/**
 * 符合条件的队伍
 * @param {any} party
 * @param {any} room
 * @returns
 */
function getEligibleParty(party, room, stage) {      //selects, from the given party, the team that is allowed to attempt this event
    var eligible = [];
    var hasLeader = false;

    var validMap = stage == 0 ? room.RecruitMap : room.Map;
    if (party.Count > 0) {
        for (var i = 0; i < party.Count; i++) {
            var ch = party[i];

            if (ch.getMapId() == validMap && ch.getLevel() >= minLevel && ch.getLevel() <= maxLevel) {
                if (ch.isLeader()) {
                    hasLeader = true;
                }
                eligible.push(ch);
            }
        }
    }

    if (!(hasLeader && eligible.length >= room.MinCount)) {
        eligible = [];
    }
    return eligible
}


function setup(roomIndex) {
    // 当调用时设置事件实例，例如：开始PQ。
    const room = em.Rooms[roomIndex];
    const eim = em.newInstance(room.InstanceName);

    setStage(eim, 0);
    setEventRewards(eim);
    return eim;
}

function setStage(eim, stage) {
    eim.CurrentStage = stage;
    if (stage == 0) {
        // 准备室等待挑战
        eim.startEventTimer(eventTime * 60000);
    } else if (stage == 1) {
        // 准备室等待开始战斗
        eim.restartEventTimer(10000);
    } else if (stage == 2) {
        // 战斗中
        eim.restartEventTimer(eim.EventMap.TimeDefault * 1000 - 10000);
        eim.StartEvent();
        respawnStages(eim);
    } else if (stage > 2) {
        // 平分，加时
        eim.Pink("CPQ_ExtendTime");
        eim.restartEventTimer(eim.EventMap.TimeExtend * 1000);
    } else {
        // -1 结算
        eim.schedule("clearPQ", 10000);
    }
}

function afterSetup(eim) {
    // 事件实例初始化完毕且所有玩家分配完成后，但在玩家进入之前触发。
}

function respawnStages(eim) {
    // 定义事件内部允许重生的地图。此函数应在末尾创建一个新的任务，在指定的重生率后再次调用自身。
    eim.EventMap.instanceMapRespawn();
    if (eim.CurrentStage != -1) {
        eim.schedule("respawnStages", 10 * 1000);
    }
}

function playerEntry(eim, player) {
    // 将玩家传送到事件地图等操作。
    player.changeMap(eim.LobbyMap, eim.LobbyMap.getPortal(0));
}

function playerUnregistered(eim, player) {
    // 在玩家即将注销前对其进行某些操作。
}

function playerExit(eim, player) {
    // 在解散事件实例前对玩家进行某些操作。
    eim.unregisterPlayer(player);
    player.ForcedWarpOut();
}

function changedMap(eim, player, mapid) {
    // 当玩家更换地图时根据mapid执行的操作。
    if (mapid < minMapId || mapid > maxMapId) {
        if (!eim.isEventCleared()) {
            eim.Pink("CPQ_PlayerExit", player.TeamModel.MCTeam.TeamFlag == 0 ? "TeamRed" : "TeamBlue");
            end(eim);
        }
    }
}

function changedLeader(eim, leader) {
    // 如果队伍领袖变更时执行的操作。
}

function scheduledTimeout(eim) {
    // 当事件超时而未完成时触发。
    const stage = eim.CurrentStage;
    if (stage == 0) {
        end(eim);
    } else if (stage == 1) {
        var t1 = em.GetRoomEligibleParty(eim, eim.Team0);
        var t2 = em.GetRoomEligibleParty(eim, eim.Team1);
        if (t1.Count == t2.Count && t1.Count >= eim.Room.MinCount) {
            setStage(eim, 2);
        } else {
            eim.Pink("CPQ_Error");
            end(eim);
        }
    } else {
        if (!eim.Complete()) {
            setStage(eim, ++stage);
        } else {
            setStage(eim, -1);
        }
    }
}

function monsterKilled(mob, eim) {
    // 当敌对怪物死亡时触发。
}

function monsterValue(eim, mobid) {
    // 当注册的怪物被击杀时调用。
    // 返回此玩家获得的积分 - “保存点数”
}

function friendlyKilled(mob, eim) {
    // 当友好怪物死亡时触发。
}

function allMonstersDead(eim) {
    // 当调用unregisterMonster(Monster mob)或怪物被击杀后触发。
    // 只有当剩余怪物数量为0时触发。
}

function playerDead(eim, player) {
    // 当玩家死亡时触发。
}

function monsterRevive(mob, eim) {
    // 当敌对怪物复活时触发。
}

function playerRevive(eim, player) {
    // 当玩家复活时触发。
    // 参数返回true/false。
}

function playerDisconnected(eim, player) {
    // 返回0 - 正常注销玩家并在玩家数量为零时解散实例。
    // 返回大于0的值 - 正常注销玩家并在玩家数量等于或低于该值时解散实例。
    // 返回小于0的值 - 正常注销玩家并在玩家数量等于或低于该值时解散实例，如果是队长则踢出所有人。
    if (!eim.isEventCleared()) {
        eim.Pink("CPQ_PlayerExit", player.TeamModel.MCTeam.TeamFlag == 0 ? "TeamRed" : "TeamBlue");
        end(eim);
    }
}

function end(eim) {
    // 当队伍未能完成事件实例时触发。
    var party = eim.getPlayers();
    for (var i = 0; i < party.Count; i++) {
        playerExit(eim, party[i]);
    }
    eim.dispose();
}

function giveRandomEventReward(eim, player) {
    // 从奖励池中随机选择一个奖励给予玩家。
}

function clearPQ(eim) {
    // 当队伍成功完成事件实例时触发。
    eim.setEventCleared();
}

function leftParty(eim, player) {
    // 当玩家离开队伍时触发。
    if (!eim.isEventCleared()) {
        eim.Pink("CPQ_PlayerExit", player.TeamModel.MCTeam.TeamFlag == 0 ? "TeamRed" : "TeamBlue");
        end(eim);
    }
}

function disbandParty(eim, player) {
    // 当队伍解散时触发。
    if (!eim.isEventCleared()) {
        end(eim);
    }
}

function cancelSchedule() {
    // 结束正在进行的任务调度。
}

function dispose() {
    // 结束事件实例。
}