local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    instanceName = "Delli",
    minPlayers = 1,
    maxPlayers = 2,
    minLevel = 120,
    maxLevel = 255,
    entryMap = 925010300,
    exitMap = 925010200,
    recruitMap = 925010200,
    clearMap = 0,
    minMapId = 925010300,
    maxMapId = 925010300,
    eventTime = 6,
    maxLobbies = 7,

    -- base.setup.respawnStages调用 地图怪物重生设置
    respawnConfig = {
        maps = { 925010300 },
        duration = 15000
    },
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 没办法通过设置处理的就在这里进行重载
function Sample:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    -- toggle？
    eim:getMapInstance(self.entryMap):toggleDrops();
    return eim
end

function Sample:scheduledTimeout(eim)
    eim:getMapInstance(self.entryMap):killAllMonstersNotFriendly();
    eim:showClearEffect();
    BaseEvent.clearPQ(self, eim);
end

function Sample:friendlyKilled(mob, eim)
    if (mob:getId() == 9300162) then
        BaseEvent.endEvent(self, eim);
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