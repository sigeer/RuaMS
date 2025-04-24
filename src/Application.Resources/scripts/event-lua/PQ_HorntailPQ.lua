local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名,
    name = "HorntailPQ",
    instanceName = "PreHorntail",
    minPlayers = 6,
    maxPlayers = 6,
    minLevel = 120,
    maxLevel = 255,
    entryMap = 240050100,
    exitMap = 240050000,
    recruitMap = 240050000,
    clearMap = 240050400,
    warpTeamWhenClear = true,
    minMapId = 240050100,
    maxMapId = 240050310,
    eventTime = 25,
    maxLobbies = 1,

    -- base.setup.resetMap 中调用
    resetPQMaps = {240050000, 240050100, 240050101, 240050102, 240050103, 240050104, 240050105, 240050200,
    240050300, 240050310},
    -- base.setup.setEventExclusives 任务特有的道具，需要被清理
    eventItems = {4001087, 4001088, 4001089, 4001090, 4001091, 4001092, 4001093}
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

function Sample:BeforeStartEvent(eim, level, lobbyid)
    eim:getInstanceMap(240050101):getReactorByName("passKey1"):setEventState(0);
    eim:getInstanceMap(240050102):getReactorByName("passKey2"):setEventState(1);
    eim:getInstanceMap(240050103):getReactorByName("passKey3"):setEventState(2);
    eim:getInstanceMap(240050104):getReactorByName("passKey4"):setEventState(3);
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
