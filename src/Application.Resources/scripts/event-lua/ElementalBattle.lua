local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    name = "Elemental",
    minPlayers = 2,
    maxPlayers = 2,
    minLevel = 100,
    maxLevel = 255,
    entryMap = 922020100,
    exitMap = 220050300,
    recruitMap = 220050300,
    clearMap = 220050300,
    minMapId = 922020100,
    maxMapId = 922020100,
    eventTime = 20,
    maxLobbies = 7,

    -- base.setup.resetMap 中调用
    resetConfig = {
        -- 重置地图
        resetPQMaps = { 922020100 },
        -- 打乱地图reactor顺序
        resetReactorMaps = { }
    },
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 没办法通过设置处理的就在这里进行重载
function Sample:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    eim:setProperty("boss", 0)
    return eim
end

function Sample:isElemental(mob)
    local mobid = mob:getId()
    return mobid == 9300086 or mobid == 9300100
end

function Sample:monsterKilled(mob, eim)
    if (self:isElemental(mob)) then
        local killed = eim:getIntProperty("boss")
        if (killed == 1) then
            eim:showClearEffect()
            eim:clearPQ()
        else
            eim:setIntProperty("boss", killed + 1)
        end
    end
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