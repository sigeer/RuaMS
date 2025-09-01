local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    name = "KingPepeAndYetis",
    instanceName = "KingPepe_",
    minPlayers = 1,
    maxPlayers = 3,
    minLevel = 30,
    maxLevel = 255,
    entryMap = 106021500,
    entryPortal = 1,
    exitMap = 106021400,
    exitPortal = 2,
    recruitMap = 106021400,
    clearMap = 0,
    warpTeamWhenClear = false,
    minMapId = 106021500,
    maxMapId = 106021500,
    eventTime = 20,
    maxLobbies = 1,
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

function Sample:BeforeStartEvent(eim, level, lobbyid)
    eim:getInstanceMap(eventMap):resetFully()
    eim:getInstanceMap(eventMap):allowSummonState(false)
end

-- 创建事件实例
Sample:new(config)