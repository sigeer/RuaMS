local BaseEvent = require("scripts/event-lua/__BasePQ")
-- 未完成
-- 配置事件参数
local config = {
    name = "CWKPQ",
    instanceName = "CWKPQ",
    minPlayers = 6,
    maxPlayers = 30,
    minLevel = 90,
    maxLevel = 255,
    entryMap = 610030100,
    exitMap = 610030020,
    recruitMap = 610030020,
    clearMap = 610030020,
    minMapId = 610030100,
    maxMapId = 610030800,
    eventTime = 2,
    maxLobbies = 1,

    eventItems = { 4001256, 4001257, 4001258, 4001259, 4001260 },
    rewardConfig = {
        finalItem = {
            level = 1,
            list = { },
            quantity = { }
        },
        expStages = { 2500, 8000, 18000, 25000, 30000, 40000 } ,
        mesoStages = { 500, 1000, 2000, 5000, 8000, 20000 }
    },

    resetConfig = {
        resetPQMaps = {610030100, 610030200, 610030300, 610030400, 
        610030500, 610030510, 610030520, 610030521,
        610030522, 610030530, 610030540, 610030550,
        610030600, 610030700, 610030800},
        resetReactorMaps = { 610030550 }
    }
}

-- 创建自定义事件
local EllinPQ = BaseEvent:extend()

function EllinPQ:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    eim:setProperty("statusStg4", 0)
    return eim
end


function EllinPQ:respawnStages(eim)
    local stg2Map = eim:getMapInstance(930000200)
    if stg2Map:getPlayers().Count > 0 then
        stg2Map:instanceMapRespawn()
    end
    eim:schedule("respawnStages", 4 * 1000)
    
end

-- 创建事件实例
local event = EllinPQ:new(config)

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