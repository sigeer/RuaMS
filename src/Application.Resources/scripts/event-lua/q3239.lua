local BaseChallenge = require("scripts/event-lua/__BaseChallenge")

local config = {
    name = "q32396",
    instanceName = "q3239_",
    entryMap = 922000000,
    entryPortal = 0,
    exitMap = 922000009,
    minMapId = 922000000,
    maxMapId = 922000000,
    eventTime = 20,
    maxLobbies = 7,

    eventItems = { 4031092 }
}

local Event = BaseChallenge:extend()

function Event:ResetMap(eim)
    local mapObject = eim:getInstanceMap(self.entryMap)
    mapObject:clearDrops()
    mapObject:resetReactors()
    mapObject:shuffleReactors()
end

-- 创建事件实例
local event = Event:new(config)

-- 导出所有方法到全局环境（包括继承的方法）
local function exportMethods(obj)
    local exported = {}
    local current = obj
    while current do
        for k, v in pairs(current) do
            if type(v) == "function" and not exported[k] then
                _ENV[k] = function(...)
                    return v(event, ...)
                end
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
