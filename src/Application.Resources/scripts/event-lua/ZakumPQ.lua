local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
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
    resetConfig = {
        -- 重置地图
        resetPQMaps = {280010000, 280010010, 280010011, 280010020, 280010030, 280010031,
        280010040, 280010041, 280010050, 280010060, 280010070, 280010071,
        280010080, 280010081, 280010090, 280010091, 280010100, 280010101,
        280010110, 280010120, 280010130, 280010140, 280010150, 280011000,
        280011001, 280011002, 280011003, 280011004, 280011005, 280011006},
        -- 打乱地图reactor顺序
        resetReactorMaps = { }
    },
    -- 任务特有的道具，需要被清理
    eventItems = { 4001015, 4001016, 4001018 },
    -- 奖励设置
    rewardConfig = {
        -- 每一关的经验奖励
        expStages = {},
        -- 每一关的金钱奖励
        mesoStages = {},
        -- 最终关卡的物品奖励
        finalItem = {
            level = 1,
            list = {},
            quantity = {}
        }
    },
    -- base.setup.respawnStages调用 地图怪物重生设置
    respawnConfig = {
        maps = {},
        duration = 15000
    }
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 没办法通过设置处理的就在这里进行重载
function Sample:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    eim:setProperty("gotDocuments", 0);
    return eim
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