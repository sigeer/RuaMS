local BossBattle = require("scripts/event-lua/__BaseBossBattle1")

-- 配置事件参数
local config = {
    name = "TD_Battle4",
    instanceName = "TDBoss",
    minPlayers = 2,
    maxPlayers = 6,
    minLevel = 70,
    maxLevel = 255,
    entryMap = 240070503,
    entryPortal = 0,
    exitMap = 240070502,
    recruitMap = 240070502,
    clearMap = 240070502,
    warpTeamWhenClear = false,
    minMapId = 240070503,
    maxMapId = 240070503,
    eventTime = 15,
    maxLobbies = 1,

    resetPQMaps = { 240070203 },
    bossId = 8220012
}

BossBattle:new(config)