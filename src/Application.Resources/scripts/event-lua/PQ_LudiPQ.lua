local BaseEvent = require("scripts/event-lua/__BasePQ")
-- 101层
-- 配置事件参数
local config = {
    name = "LudiPQ",
    instanceName = "Ludi",
    minPlayers = 5,
    maxPlayers = 6,
    minLevel = 35,
    maxLevel = 255,
    entryMap = 922010100,
    exitMap = 922010000,
    recruitMap = 221024500,
    clearMap = 922011000,
    warpTeamWhenClear = true,
    minMapId = 922010100,
    maxMapId = 922011100,
    eventTime = 45,
    maxLobbies = 1,

    -- 重置地图配置,
    resetPQMaps = { 922010100, 922010200, 922010201, 922010300, 922010400, 922010401, 922010402, 922010403,
        922010404, 922010405, 922010500, 922010500, 922010501, 922010502, 922010503, 922010504,
        922010505, 922010506, 922010600, 922010700, 922010800, 922010900, 922011000, 922011100 },
    -- 打乱地图reactor顺序
    resetReactorMaps = {},
    -- base.setup.setEventExclusives 任务特有的道具，需要被清理
    eventItems = {},
    -- base.setup.setEventRewards 奖励设置
    rewardConfig = {
        -- 每一关的经验奖励
        expStages = { 210, 2520, 2940, 3360, 3770, 0, 4620, 5040, 5950 },
        -- 每一关的金钱奖励
        mesoStages = {},
        -- 最终关卡的物品奖励
        finalItem = {
            level = 1,
            list = { 2040602, 2040802, 2040002, 2040402, 2040505, 2040502, 2040601, 2044501, 2044701, 2044601, 2041019,
                2041016, 2041022, 2041013, 2041007, 2043301, 2040301, 2040801, 2040001, 2040004, 2040504, 2040501,
                2040513, 2043101, 2044201, 2044401, 2040701, 2044301, 2043801, 2040401, 2043701, 2040803, 2000003,
                2000002, 2000004, 2000006, 2000005, 2022000, 2001001, 2001002, 2022003, 2001000, 2020014, 2020015,
                4003000, 1102003, 1102004, 1102000, 1102002, 1102001, 1102011, 1102012, 1102013, 1102014, 1032011,
                1032012, 1032013, 1032002, 1032008, 1032011, 2070011, 4010003, 4010000, 4010006, 4010002, 4010005,
                4010004, 4010001, 4020001, 4020002, 4020008, 4020007, 4020003, 4020000, 4020004, 4020005, 4020006 },
            quantity = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                85, 85, 10, 60, 2, 20, 15, 15, 20, 15, 10, 5, 35, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 10, 10, 6, 10, 10, 10, 10, 10, 10, 4, 4, 10, 10, 10, 10, 10 }
        }
    }
}

-- 创建自定义事件
local LudiPQ = BaseEvent:extend()

function LudiPQ:BeforeStartEvent(eim, level, lobbyId)
    eim:setProperty("statusStg1", -1);
    eim:setProperty("statusStg2", -1);
    eim:setProperty("statusStg3", -1);
    eim:setProperty("statusStg4", -1);
    eim:setProperty("statusStg5", -1);
    eim:setProperty("statusStg6", -1);
    eim:setProperty("statusStg7", -1);
    eim:setProperty("statusStg8", -1);
    eim:setProperty("statusStg9", -1);
end

function LudiPQ:afterSetup(eim)
    eim:dropAllExclusiveItems();
end

function LudiPQ:scheduledTimeout(eim)
    if (eim:getProperty("9stageclear")) then
        local curStage = 922011000
        local toStage = 922011100
        eim:warpEventTeam(curStage, toStage)
    else
        BaseEvent.endEvent(self, eim)
    end
end

function LudiPQ:clearPQ(eim)
    BaseEvent.clearPQ(self, eim)
    -- 奖励地图的倒计时
    eim:startEventTimer(1 * 60000)
end

LudiPQ:new(config)
