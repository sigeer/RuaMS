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

    name = "CafePQ_5",
    instanceName = "Lan5_",
    entryMap = 196000000,
    minMapId = 196000000,
    maxMapId = 196010000,
    couponsNeeded = 500,
    respawnConfig = {
        maps = {196000000, 196010000},
        duration = 15000
    },

    -- base.setup.setEventRewards 奖励设置
    rewardConfig = {
        -- 每一关的经验奖励
        expStages = {25000},
        -- 每一关的金钱奖励
        mesoStages = {},
        -- 最终关卡的物品奖励
        finalItem = {
            level = 1,
            list = {4001012},
            quantity = {1}
        }
    }
}

CafePQBase:new(config)