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

    name = "CafePQ_2",
    instanceName = "Lan2_",
    entryMap = 191000000,
    minMapId = 191000000,
    maxMapId = 191000001,
    couponsNeeded = 350,
    respawnConfig = {
        maps = {191000000, 191000001},
        duration = 15000
    },

    -- base.setup.setEventRewards 奖励设置
    rewardConfig = {
        -- 每一关的经验奖励
        expStages = {15000},
        -- 每一关的金钱奖励
        mesoStages = {},
        -- 最终关卡的物品奖励
        finalItem = {
            level = 1,
            list = {4001009},
            quantity = {1}
        }
    }
}

CafePQBase:new(config)
