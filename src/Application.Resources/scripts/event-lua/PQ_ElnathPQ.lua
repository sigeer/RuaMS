local BasePQ = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    name = "ElnathPQ",
    instanceName = "Tylus",
    minPlayers = 1,
    maxPlayers = 4,
    minLevel = 80,
    maxLevel = 255,
    entryMap = 921100300,
    exitMap = 211040100,
    recruitMap = 211000001,
    clearMap = 0,
    minMapId = 921100300,
    maxMapId = 921100300,
    eventTime = 10,
    maxLobbies = 1,

    -- base.setup.respawnStages调用 地图怪物重生设置
    respawnConfig = {
        maps = { 921100300 },
        duration = 15000
    },
}

BasePQ:new(config)