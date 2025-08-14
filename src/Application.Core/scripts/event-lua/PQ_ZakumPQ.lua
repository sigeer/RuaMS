local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    name = "ZakumPQ",
    instanceName = "PreZakum",
    minPlayers = 6,
    maxPlayers = 6,
    minLevel = 50,
    maxLevel = 255,
    entryMap = 280010000,
    exitMap = 211042300,
    recruitMap = 211042300,
    clearMap = 211042300,
    minMapId = 280010000,
    maxMapId = 280011006,
    eventTime = 30,
    maxLobbies = 1,

    -- base.setup.resetMap 中调用
    -- 重置地图
    resetPQMaps = { 280010000, 280010010, 280010011, 280010020, 280010030, 280010031,
        280010040, 280010041, 280010050, 280010060, 280010070, 280010071,
        280010080, 280010081, 280010090, 280010091, 280010100, 280010101,
        280010110, 280010120, 280010130, 280010140, 280010150, 280011000,
        280011001, 280011002, 280011003, 280011004, 280011005, 280011006 },
    -- 打乱地图reactor顺序
    resetReactorMaps = {},
    -- 任务特有的道具，需要被清理
    eventItems = { 4001015, 4001016, 4001018 },
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

function Sample:SetupProperty(eim, level, lobbyid)
    eim:setProperty("gotDocuments", 0);
end

Sample:new(config)
