local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    name = "PapulatusBattle",
    instanceName = "Papulatus",
    minPlayers = 1,
    maxPlayers = 6,
    minLevel = 1,
    maxLevel = 255,
    entryMap = 220080001,
    entryPortal = 0,
    exitMap = 220080000,
    recruitMap = 220080000,
    clearMap = 220080000,
    warpTeamWhenClear = false,
    minMapId = 220080001,
    maxMapId = 220080001,
    eventTime = 45,
    maxLobbies = 1,

    resetPQMaps = {220080001},
    -- base.setup.setEventExclusives 任务特有的道具，需要被清理
    eventItems = {},
    -- base.setup.setEventRewards 奖励设置
    rewardConfig = {
        -- 每一关的经验奖励
        expStages = {},
        -- 每一关的金钱奖励
        mesoStages = {},
        -- 最终关卡的物品奖励
        finalItem = {
            level = 1,
            list = {},
            quantity = {}
        }
    },
    -- base.setup.respawnStages调用 地图怪物重生设置
    respawnConfig = {
        maps = {},
        duration = 15000
    }
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

function Sample:BeforeStartEvent(eim, level, lobbyid)
    eim:setProperty("boss", 0)
end

function Sample:afterSetup()
    self:updateGateState(1)
end

function Sample:clearPQ(eim)
    BaseEvent.clearPQ(self, eim)
    self:updateGateState(0)
end

function Sample:dispose(eim)
    if (not eim.isEventCleared()) then
        self:updateGateState(0)
    end
end

function Sample:monsterKilled(mob, eim)
    if (mob:getId() == 8500002) then
        eim:showClearEffect()
        eim:clearPQ()
    end
end

function Sample:updateGateState(newState)
    em:GetMap(220080000):getReactorById(2208001):forceHitReactor(newState)
    em:GetMap(220080000):getReactorById(2208002):forceHitReactor(newState)
    em:GetMap(220080000):getReactorById(2208003):forceHitReactor(newState)
end

Sample:new(config)
