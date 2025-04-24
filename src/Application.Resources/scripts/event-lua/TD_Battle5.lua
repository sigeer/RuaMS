local BossBattle = require("scripts/event-lua/__BaseBossBattle1")

-- 配置事件参数
local config = {
    name = "TD_Battle5",
    instanceName = "TDBoss",
    minPlayers = 2,
    maxPlayers = 6,
    minLevel = 70,
    maxLevel = 255,
    entryMap = 240070603,
    entryPortal = 0,
    exitMap = 240070602,
    recruitMap = 240070602,
    clearMap = 240070602,
    warpTeamWhenClear = false,
    minMapId = 240070603,
    maxMapId = 240070603,
    eventTime = 20,
    maxLobbies = 1,

    -- base.setup.resetMap 中调用
    -- 重置地图
    resetPQMaps = { 240070203 },
    -- 打乱地图reactor顺序
    resetReactorMaps = {},

    bossId = 8220015
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
