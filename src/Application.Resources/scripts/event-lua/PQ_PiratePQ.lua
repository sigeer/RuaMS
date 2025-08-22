local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    name = "PiratePQ",
    instanceName = "Pirate",
    minPlayers = 3,
    maxPlayers = 6,
    minLevel = 55,
    maxLevel = 100,
    entryMap = 925100000,
    exitMap = 925100700,
    recruitMap = 251010404,
    clearMap = 925100600,
    warpTeamWhenClear = true,
    minMapId = 925100000,
    maxMapId = 925100500,
    eventTime = 4,
    maxLobbies = 1,

    -- base.setup.resetMap 中调用
    -- 重置地图
    resetPQMaps = { 925100000, 925100100, 925100200, 925100201, 925100202, 925100300, 925100301, 925100302, 925100400, 925100500 },
    -- 打乱地图reactor顺序
    resetReactorMaps = { 925100000, 925100200, 925100300 },
    -- base.setup.setEventExclusives 任务特有的道具，需要被清理
    eventItems = { 4001117, 4001120, 4001121, 4001122 },

    -- base.setup.respawnStages调用 地图怪物重生设置
    respawnConfig = {
        maps = {},
        duration = 15000
    },

    -- 复合型怪物（一个用于显示的fake，加上多个真实的躯体）
    compositBoss = {
        -- 真实的mobid
        main = 8830010,
        -- 仅用于贴图显示整体的虚id
        fake = 8830007,
        -- 躯干mob
        part = { 8830009, 8830013 },
        minMobId = 8830007,
        maxMobId = 8830013,
        posX = 412,
        posY = 258
    }
}

-- 创建自定义事件
local PiratePQ = BaseEvent:extend()


function PiratePQ:BeforeStartEvent(eim, level, lobbyId)
    eim:setProperty("stage2", "0")
    eim:setProperty("stage2a", "0")
    eim:setProperty("stage3a", "0")
    eim:setProperty("stage2b", "0")
    eim:setProperty("stage3b", "0")
    eim:setProperty("stage4", "0")
    eim:setProperty("stage5", "0")

    eim:setProperty("curStage", "1")
    eim:setProperty("grindMode", "0")

    eim:setProperty("openedChests", "0")
    eim:setProperty("openedBoxes", "0")

    local map = eim.getInstanceMap(925100200)

    for i = 1, 5 do
        local mob = em:getMonster(9300124)
        local mob2 = em:getMonster(9300125)
        local mob3 = em:getMonster(9300124)
        local mob4 = em:getMonster(9300125)
        eim:registerMonster(mob)
        eim:registerMonster(mob2)
        eim:registerMonster(mob3)
        eim:registerMonster(mob4)
        mob:changeDifficulty(level, true)
        mob2:changeDifficulty(level, true)
        mob3:changeDifficulty(level, true)
        mob4:changeDifficulty(level, true)
        map:spawnMonsterOnGroundBelow(mob, Point(430, 75))
        map:spawnMonsterOnGroundBelow(mob2, Point(1600, 75))
        map:spawnMonsterOnGroundBelow(mob3, Point(430, 238))
        map:spawnMonsterOnGroundBelow(mob4, Point(1600, 238))
    end

    map = eim.getInstanceMap(925100201)
    for i = 1, 10 do
        local mob = em:getMonster(9300112)
        local mob2 = em:getMonster(9300113)
        eim:registerMonster(mob)
        eim:registerMonster(mob2)
        mob:changeDifficulty(level, true)
        mob2:changeDifficulty(level, true)
        map:spawnMonsterOnGroundBelow(mob, Point(0, 238))
        map:spawnMonsterOnGroundBelow(mob2, Point(1700, 238))
    end

    map = eim.getInstanceMap(925100300)
    for i = 1, 5 do
        local mob = em:getMonster(9300124)
        local mob2 = em:getMonster(9300125)
        local mob3 = em:getMonster(9300124)
        local mob4 = em:getMonster(9300125)
        eim:registerMonster(mob)
        eim:registerMonster(mob2)
        eim:registerMonster(mob3)
        eim:registerMonster(mob4)
        mob:changeDifficulty(level, true)
        mob2:changeDifficulty(level, true)
        mob3:changeDifficulty(level, true)
        mob4:changeDifficulty(level, true)
        map:spawnMonsterOnGroundBelow(mob, Point(430, 75))
        map:spawnMonsterOnGroundBelow(mob2, Point(1600, 75))
        map:spawnMonsterOnGroundBelow(mob3, Point(430, 238))
        map:spawnMonsterOnGroundBelow(mob4, Point(1600, 238))
    end
    map = eim.getInstanceMap(925100301)

    for i = 1, 10 do
        local mob = em:getMonster(9300112)
        local mob2 = em:getMonster(9300113)
        eim:registerMonster(mob)
        eim:registerMonster(mob2)
        mob:changeDifficulty(level, true)
        mob2:changeDifficulty(level, true)
        map:spawnMonsterOnGroundBelow(mob, Point(0, 238))
        map:spawnMonsterOnGroundBelow(mob2, Point(1700, 238))
    end
end

function PiratePQ:respawnStages(eim)
    local stg = eim:getIntProperty("stage2")
    if (stg < 3) then
        eim:getMapInstance(925100100):spawnAllMonsterIdFromMapSpawnList("9300114" .. stg, eim:getIntProperty("level"),
            true)
    end

    eim:getMapInstance(925100400):instanceMapRespawn()
    eim:schedule("respawnStages", 10 * 1000)
end

function PiratePQ:changedMapInside(eim, mapid)
    local stage = eim.getIntProperty("curStage")

    if (stage == 1) then
        if (mapid == 925100100) then
            eim.restartEventTimer(6 * 60 * 1000)
            eim.setIntProperty("curStage", 2)
        end
    elseif (stage == 2) then
        if (mapid == 925100200) then
            eim.restartEventTimer(6 * 60 * 1000)
            eim.setIntProperty("curStage", 3)
        end
    elseif (stage == 3) then
        if (mapid == 925100300) then
            eim.restartEventTimer(6 * 60 * 1000)
            eim.setIntProperty("curStage", 4)
        end
    elseif (stage == 4) then
        if (mapid == 925100400) then
            eim.restartEventTimer(6 * 60 * 1000)
            eim.setIntProperty("curStage", 5)
        end
    elseif (stage == 5) then
        if (mapid == 925100500) then
            eim.restartEventTimer(8 * 60 * 1000)
            eim.setIntProperty("curStage", 6)
        end
    end
end

function PiratePQ:changedMap(eim, player, mapId)
    if mapId < self.minMapId or mapId > self.maxMapId then
        if (eim:isEventTeamLackingNow(true, self.minPlayers, player)) then
            eim:unregisterPlayer(player)
            self:noticePlayerLeft(eim, player)
            self:endEvent(eim)
        else
            self:noticeMemberCount(eim, player)
            eim:unregisterPlayer(player)
        end
    else
        self:changedMapInside(eim, mapId)
    end
end

function PiratePQ:clearPQ(eim)
    BaseEvent.clearPQ(self, eim)

    local chests = tonumber(eim.getProperty("openedChests"))
    local expSet = { 28000, 35000, 42000 }
    local expGain = expSet[chests + 1]
    eim:giveEventPlayersExp(expGain)
end

function PiratePQ:isLordPirate(mob)
    local mobid = mob.getId()
    return (mobid == 9300105) or (mobid == 9300106) or (mobid == 9300107) or (mobid == 9300119)
end

function PiratePQ:passedGrindMode(map, eim)
    if (eim:getIntProperty("grindMode") == 0) then
        return true
    end
    return eim:activatedAllReactorsOnMap(map, 2511000, 2517999)
end

function PiratePQ:monsterKilled(mob, eim)
    local map = mob:getMap()

    if (self:isLordPirate(mob)) then
        map:broadcastStringMessage(5, "As Lord Pirate dies, Wu Yang is released!")
        eim:spawnNpc(2094001, Point(777, 140), mob:getMap())
    end

    if (map:countMonsters() == 0) then
        local stage = ((map:getId() % 1000) / 100) + 1

        if ((stage == 1 or stage == 3 or stage == 4) and self:passedGrindMode(map, eim)) then
            eim:showClearEffect(map:getId())
        elseif (stage == 5) then
            if (map:getReactorByName("sMob1"):getState() >= 1 and map:getReactorByName("sMob2"):getState() >= 1 and map:getReactorByName("sMob3"):getState() >= 1 and map:getReactorByName("sMob4"):getState() >= 1) then
                eim:showClearEffect(map:getId())
            end
        end
    end
end

PiratePQ:new(config)
