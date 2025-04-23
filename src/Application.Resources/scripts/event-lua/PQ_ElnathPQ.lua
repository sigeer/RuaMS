local BaseEvent = require("scripts/event-lua/__BasePQ")

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

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 创建事件实例
local event = Sample:new(config)

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