local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    name = "",
    minPlayers = 1,
    maxPlayers = 6,
    minLevel = 120,
    maxLevel = 255,
    entryMap = 702060000,
    exitMap = 702070400,
    recruitMap = 702070400,
    clearMap = 702060000,
    minMapId = 702060000,
    maxMapId = 702060000,
    eventTime = 30,
    maxLobbies = 1,

    -- base.setup.resetMap 中调用
    resetConfig = {
        -- 重置地图
        resetPQMaps = {},
        -- 打乱地图reactor顺序
        resetReactorMaps = { }
    },

    -- base.setup.respawnStages调用 地图怪物重生设置
    respawnConfig = {
        maps = {},
        duration = 15000
    },

    -- 复合型怪物（一个用于显示的fake，加上多个真实的躯体）
    bossConfig = {
        id = 9600025,
        posX = 351,
        posY = 580,
        difficulty = true,
        drops = {
            items = {2000005},
            counts = {5},
            chances = {0.4}
        }
    }
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 没办法通过设置处理的就在这里进行重载
function Sample:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    -- xxx
    return eim
end

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