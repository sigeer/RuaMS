local BossBattle = require("scripts/event-lua/__BaseBossBattle1")
-- 配置事件参数
local config = {
    name = "TD_Battle3",
    instanceName = "TDBoss",
    minPlayers = 2,
    maxPlayers = 6,
    minLevel = 70,
    maxLevel = 255,
    entryMap = 240070403,
    entryPortal = 0,
    exitMap = 240070402,
    recruitMap = 240070402,
    clearMap = 240070402,
    warpTeamWhenClear = false,
    minMapId = 240070403,
    maxMapId = 240070403,
    eventTime = 15,
    maxLobbies = 1,

    -- base.setup.resetMap 中调用
    resetPQMaps = { 240070203 },

    bossId = 8220011
}

BossBattle:new(config)