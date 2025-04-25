local BasePQ = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    name = "BossRushPQ",
    instanceName = "BossRush",
    minPlayers = 1,
    maxPlayers = 6,
    minLevel = 1,
    maxLevel = 255,
    entryMap = 970030100,
    exitMap = 970030000,
    recruitMap = 970030000,
    clearMap = 970030000,
    minMapId = 970030001,
    maxMapId = 970042711,
    eventTime = 5,    -- 5分钟
    maxLobbies = 7,
    
    -- 奖励配置
    rewardConfig = {
        finalItem = {
            level = 6,
            list = {
                3010061, 1122018, 1122005,  -- 6级奖励
                3010063, 1122018, 1122005,  -- 5级奖励
                1122001, 1122006, 1022103,  -- 4级奖励
                1122002, 1022088, 1012076,  -- 3级奖励
                1122003, 1012077, 1012079,  -- 2级奖励
                1122004, 1012078, 1432008   -- 1级奖励
            },
            quantity = {
                1, 1, 1,  -- 6级奖励数量
                1, 1, 1,  -- 5级奖励数量
                1, 1, 1,  -- 4级奖励数量
                1, 1, 1,  -- 3级奖励数量
                1, 1, 1,  -- 2级奖励数量
                1, 1, 1   -- 1级奖励数量
            }
        }
    }
}

-- 创建自定义PQ
local BossRushPQ = BasePQ:extend()

function BossRushPQ:playerEntry(eim, player)
    -- 根据实例ID分配不同的入口地图
    local map = eim:getMapInstance(self.entryMap + eim:getIntProperty("lobby"))
    player:changeMap(map, map:getPortal(0))
end

BossRushPQ:new(config)
