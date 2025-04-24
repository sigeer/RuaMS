local BossBattle = require("scripts/event-lua/__BaseBossBattle1")

-- 配置事件参数
local config = {
    name = "TD_Battle2",
    instanceName = "TDBoss",
    minPlayers = 2,
    maxPlayers = 6,
    minLevel = 70,
    maxLevel = 255,
    entryMap = 240070303,
    entryPortal = 0,
    exitMap = 240070302,
    recruitMap = 240070302,
    clearMap = 240070302,
    warpTeamWhenClear = false,
    minMapId = 240070303,
    maxMapId = 240070303,
    eventTime = 10,
    maxLobbies = 1,

    resetPQMaps = { 240070203 },

    bossId = 8220010
}

-- 创建事件实例
local event = BossBattle:new(config)

-- 导出所有方法到全局环境（包括继承的方法）
local function exportMethods(obj)
    local exported = {}
    local current = obj
    while current do
        for k, v in pairs(current) do
            if type(v) == "function" and not exported[k] then
                _ENV[k] = function(...) return v(event, ...) end
                exported[k] = true
            end
        end
        current = getmetatable(current)
        if current then
            current = current.__index
        end
    end
end

exportMethods(event)