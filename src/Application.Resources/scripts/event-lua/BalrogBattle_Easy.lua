local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    instanceName = "Balrog",
    minPlayers = 3,
    maxPlayers = 30,
    minLevel = 50,
    maxLevel = 255,
    entryMap = 105100400,
    exitMap = 105100100,
    recruitMap = 105100100,
    clearMap = 105100401,
    minMapId = 105100400,
    maxMapId = 105100401,
    eventTime = 60,
    maxLobbies = 1,

    -- 重置地图
    resetPQMaps = { 105100400, 105100401 },
    -- 打乱地图reactor顺序
    resetReactorMaps = {},
    -- 复合型怪物（一个用于显示的fake，加上多个真实的躯体）
    compositBoss = {
        main = 8830010,
        fake = 8830007,
        part = { 8830009, 8830013 },
        minMobId = 8830007,
        maxMobId = 8830013,
        posX = 412,
        posY = 258
    }
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 没办法通过设置处理的就在这里进行重载
function Sample:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    eim:setProperty("boss", 0)

    eim:schedule("releaseLeftClaw", 60000)
    return eim
end

function Sample:afterSetup(eim)
    self:spawnBalrog(eim)
end

function Sample:releaseLeftClaw(eim)
    eim:getInstanceMap(self.entryMap):killMonster(self.conpositBoss.maxMobId)
end

function Sample:spawnBalrog(eim)
    local mapObj = eim:getInstanceMap(self.entryMap)
    local spawnPoint = Point(self.compositBoss.posX, self.compositBoss.posY)
    mapObj:spawnFakeMonsterOnGroundBelow(LifeFactory:getMonster(self.compositBoss.fake), spawnPoint)

    for _, partMobId in ipairs(self.compositBoss.part) do
        mapObj:spawnMonsterOnGroundBelow(LifeFactory:getMonster(partMobId), spawnPoint)
    end
end

function Sample:spawnSealedBalrog(eim)
    local spawnPoint = Point(self.compositBoss.posX, self.compositBoss.posY)
    eim:getInstanceMap(self.entryMap):spawnMonsterOnGroundBelow(LifeFactory:getMonster(self.compositBoss.main),
        spawnPoint)
end

function Sample:isUnsealedBalrog(mob)
    local balrogid = mob:getId() - self.compositBoss.minMobId
    return balrogid >= 0 and balrogid <= 2
end

function Sample:isBalrogBody(mob)
    return mob:getId() == self.compositBoss.minMobId
end

function Sample:monsterKilled(mob, eim)
    if (self:isUnsealedBalrog(mob)) then
        local count = eim:getIntProperty("boss")

        if (count == 2) then
            eim:showClearEffect()
            eim:clearPQ()

            eim:dispatchRaiseQuestMobCount(self.compositBoss.main, self.entryMap)
            mob:getMap():broadcastBalrogVictory(eim:getLeader():getName())
        else
            if (count == 1) then
                local mapobj = eim:getInstanceMap(self.entryMap)
                mapobj:makeMonsterReal(mapobj:getMonsterById(self.compositBoss.fake))
            end
            eim:setIntProperty("boss", count + 1)
        end

        if (self:isBalrogBody(mob)) then
            eim:schedule("spawnSealedBalrog", 10 * 1000)
        end
    end
end

Sample:new(config)