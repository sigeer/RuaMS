local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    instanceName = "Showa",
    minPlayers = 3,
    maxPlayers = 30,
    minLevel = 100,
    maxLevel = 255,
    entryMap = 801040100,
    entryPortal = 0,
    exitMap = 801040004,
    recruitMap = 801040004,
    clearMap = 801040101,
    warpTeamWhenClear = false,
    minMapId = 801040100,
    maxMapId = 801040101,
    eventTime = 60,
    maxLobbies = 1,

    resetPQMaps = { 801040100 },
    -- base.setup.setEventExclusives 任务特有的道具，需要被清理
    eventItems = { },
    -- base.setup.setEventRewards 奖励设置
    rewardConfig = {
        -- 每一关的经验奖励
        expStages = {},
        -- 每一关的金钱奖励
        mesoStages = {},
        -- 最终关卡的物品奖励
        finalItem = {
            level = 1,
            list = {1102145, 1102084, 1102085, 1102086, 1102087, 1052165, 1052166, 1052167, 1402013, 1332030, 1032030, 1032070, 4003000, 4000030, 4006000, 4006001, 4005000, 4005001, 4005002, 4005003, 4005004, 2022016, 2022263, 2022264, 2022015, 2022306, 2022307, 2022306, 2022113},
            quantity = {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 40, 40, 100, 100, 2, 2, 2, 2, 1, 100, 100, 100, 40, 40, 40, 40, 40}
        }
    },
    -- base.setup.respawnStages调用 地图怪物重生设置
    respawnConfig = {
        maps = { 801040100 },
        duration = 15000
    },
}

-- 创建自定义事件
local Sample = BaseEvent:extend()


function Sample:SetupProperty(eim, level, lobbyid)
    eim:setProperty("canJoin", 1);
    eim:setProperty("playerDied", 0);
end

function Sample:noticePlayerEnter(eim, player)
    eim:dropMessage(5, "[Expedition] " .. player.Name .. " has entered the map.")
end

function Sample:noticePlayerLeft(eim, player)
    eim:dropMessage(5, "[Expedition] " .. player.Name .. " has left the instance.")
end

function Sample:noticeMemberCount(eim, player)
    eim:dropMessage(5,
        "[Expedition] Either the leader has quit the expedition or there is no longer the minimum number of members required to continue it.")
end

function Sample:playerDead(eim, player)
    eim:setIntProperty("playerDied", 1);
end

function Sample:clearPQ(eim)
    eim:getInstanceMap(801040100):killAllMonsters();

    BaseEvent.clearPQ(self, eim)

    if (eim:getIntProperty("playerDied") == 0) then
        local mob = eim:getMonster(9400114);
        eim:getMapInstance(801040101):spawnMonsterOnGroundBelow(mob, Point(500, -50));
        eim:dropMessage(5, "Konpei: The Boss has been defeated with no casualties, well done! We found a suspicious machine inside, we're moving it out.");
    end
end

function Sample:monsterKilled(mob, eim) 
    if (mob:getId() == 9400300) then
        eim:showClearEffect();
        eim:clearPQ();
    end
end

Sample:new(config)