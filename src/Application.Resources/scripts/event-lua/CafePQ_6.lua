local CafePQBase = require("scripts/event-lua/__CafePQBase")

-- 配置事件参数
local config = {
    minPlayers = 3,
    maxPlayers = 6,
    minLevel = 21,
    maxLevel = 120,
    exitMap = 193000000,
    recruitMap = 193000000,

    eventTime = 45,
    maxLobbies = 1,
    -- base.setup.setEventExclusives 任务特有的道具，需要被清理
    eventItems = {4001007},

    name = "CafePQ_6",
    instanceName = "Lan6_",
    entryMap = 197000000,
    minMapId = 197000000,
    maxMapId = 197010000,
    couponsNeeded = 300,
    respawnConfig = {
        maps = {197000000, 197010000},
        duration = 15000
    },

    -- base.setup.setEventRewards 奖励设置
    rewardConfig = {
        -- 每一关的经验奖励
        expStages = {10000},
        -- 每一关的金钱奖励
        mesoStages = {},
        -- 最终关卡的物品奖励
        finalItem = {
            level = 1,
            list = {4001010},
            quantity = {1}
        }
    }
}

-- 创建事件实例
local event = CafePQBase:new(config)

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
