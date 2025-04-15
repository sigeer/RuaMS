local BaseChallenge = require("scripts/event-lua/__BaseChallenge")

local config = {
    instanceName = "3rdJob_pirate_",
    entryMap = 108010500,
    entryPortal = 0,
    exitMap = 105070200,
    minMapId = 108010500,
    maxMapId = 108010501,
    eventTime = 20,
    maxLobbies = 7
}

local Event = BaseChallenge:extend()

function Event:InitializeMap(eim)
    local mapObject = eim:getInstanceMap(self.maxMapId)
    mapObject:resetPQ(1)
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
