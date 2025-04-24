local BaseChallenge = require("scripts/event-lua/__BaseChallenge")

local config = {
    instanceName = "3rdJob_thief_",
    entryMap = 108010400,
    entryPortal = 0,
    exitMap = 107000402,
    minMapId = 108010400,
    maxMapId = 108010401,
    eventTime = 20,
    maxLobbies = 7,

    resetPQMaps = {108010401},
}

local Event = BaseChallenge:extend()

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
