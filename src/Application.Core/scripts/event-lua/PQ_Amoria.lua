local BasePQ = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    name = "AmoriaPQ",
    instanceName = "Amoria",
    minPlayers = 6,
    maxPlayers = 6,
    minLevel = 40,
    maxLevel = 255,
    entryMap = 670010200,
    entryPortal = 0,
    exitMap = 670011000,
    recruitMap = 670010100,
    clearMap = 670010800,
    warpTeamWhenClear = false,
    minMapId = 670010200,
    maxMapId = 670010800,
    eventTime = 75,
    maxLobbies = 1,

    -- base.setup.resetMap 中调用
    -- 重置地图
    resetPQMaps = { 670010200, 670010300, 670010301, 670010302,
        670010400, 670010500, 670010600, 670010700,
        670010750, 670010800 },
    -- 打乱地图reactor顺序
    resetReactorMaps = { 670010750, 670010800 },
    -- base.setup.setEventExclusives 任务特有的道具，需要被清理
    eventItems = { 4031594, 4031595, 4031596, 4031597 },
    -- base.setup.setEventRewards 奖励设置
    rewardConfig = {
        -- 每一关的经验奖励
        expStages = { 2000, 4000, 6000, 8000, 9000, 11000 },
        -- 每一关的金钱奖励
        mesoStages = {},
        -- 最终关卡的物品奖励
        finalItem = {
            level = 1,
            list = {},
            quantity = {}
        }
    },
}

-- 创建自定义事件
local Sample = BasePQ:extend()

function Sample:BeforeStartEvent(eim)
    eim:setProperty("marriedGroup", 0)
    eim:setProperty("missCount", 0)
    eim:setProperty("statusStg1", -1)
    eim:setProperty("statusStg2", -1)
    eim:setProperty("statusStg3", -1)
    eim:setProperty("statusStg4", -1)
    eim:setProperty("statusStg5", -1)
    eim:setProperty("statusStg6", -1)
    eim:setProperty("statusStgBonus", 0)

    -- 切换掉落
    for _, mapId in ipairs({ 670010200, 670010300, 670010301, 670010302 }) do
        eim:getInstanceMap(mapId):toggleDrops()
    end

    -- 强制刷新地图
    eim:getInstanceMap(670010200):instanceMapForceRespawn()
    eim:getInstanceMap(670010500):instanceMapForceRespawn()

    -- 生成怪物
    local mapObj = eim:getInstanceMap(670010700)
    local mobObj = LifeFactory:getMonster(9400536)
    mapObj:spawnMonsterOnGroundBelow(mobObj, Point(942, 478))
end

function isTeamAllCouple(eim)     -- everyone partner of someone on the team
    local eventPlayers = eim:getPlayers()
    
    for _, chr in ipairs(eventPlayers) do
        local pid = chr:getPartnerId()
        if pid <= 0 or eim:getPlayerById(pid) == nil then
            return false
        end
    end

    return true
end

function Sample:afterSetup(eim)
    if self:isTeamAllCouple(eim) then
        eim:setIntProperty("marriedGroup", 1)
    end
end

function Sample:scheduledTimeout(eim)
    if eim:getIntProperty("statusStg6") == 1 then
        eim:warpEventTeam(self.exitMap)
    else
        BasePQ.scheduledTimeout(self, eim)
    end
end



Sample:new(config)
