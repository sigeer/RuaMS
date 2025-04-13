local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    name = "",
    minPlayers = 5,
    maxPlayers = 6,
    minLevel = 51,
    maxLevel = 70,
    entryMap = 920010000,
    exitMap = 920011200,
    recruitMap = 200080101,
    clearMap = 920011300,
    minMapId = 920010000,
    maxMapId = 920011300,
    eventTime = 45,
    maxLobbies = 1,

    -- base.setup.resetMap 中调用
    resetConfig = {
        -- 重置地图
        resetPQMaps = {920010000, 920010100},
        -- 打乱地图reactor顺序
        resetReactorMaps = { }
    },
    -- base.setup.setEventExclusives 任务特有的道具，需要被清理
    eventItems = { 4001044, 4001045, 4001046, 4001047, 4001048, 4001049, 4001050, 4001051, 4001052, 4001053, 4001054, 4001055, 4001056, 4001057, 4001058, 4001059, 4001060, 4001061, 4001062, 4001063 },
    -- base.setup.setEventRewards 奖励设置
    rewardConfig = {
        -- 每一关的经验奖励
        expStages = {},
        -- 每一关的金钱奖励
        mesoStages = {},
        -- 最终关卡的物品奖励
        finalItem = {
            level = 1,
            list = {2040602, 2040802, 2040002, 2040402, 2040505, 2040502, 2040601, 2044501, 2044701, 2044601, 2041019, 2041016, 2041022, 2041013, 2041007, 2043301, 2040301, 2040801, 2040001, 2040004, 2040504, 2040501, 2040513, 2043101, 2044201, 2044401, 2040701, 2044301, 2043801, 2040401, 2043701, 2040803, 2000003, 2000002, 2000004, 2000006, 2000005, 2022000, 2001001, 2001002, 2022003, 2001000, 2020014, 2020015, 4003000, 1102015, 1102016, 1102017, 1102018, 1102021, 1102022, 1102023, 1102024, 1102084, 1102085, 1102086, 1032019, 1032020, 1032021, 1032014, 2070011, 4010003, 4010000, 4010006, 4010002, 4010005, 4010004, 4010001, 4020001, 4020002, 4020008, 4020007, 4020003, 4020000, 4020004, 4020005, 4020006, 2210000, 2210001, 2210002, 2070006, 2070005, 2070007, 2070004, 2061003, 2060003, 2060004, 2061004, 2100000, 2100001, 2100002, 2100003, 2100004, 2100005},
            quantity = {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 100, 100, 15, 80, 5, 25, 20, 20, 25, 20, 15, 10, 45, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 15, 15, 10, 15, 15, 15, 15, 15, 15, 10, 10, 15, 15, 15, 15, 15, 5, 5, 5, 1, 1, 1, 1, 2000, 2000, 2000, 2000, 1, 1, 1, 1, 1, 1}
        }
    },
    -- base.setup.respawnStages调用 地图怪物重生设置
    respawnConfig = {
        maps = {},
        duration = 15000
    },

    -- 复合型怪物（一个用于显示的fake，加上多个真实的躯体）
    compositBoss = {
        -- 真实的mobid
        main = 8830010,
        -- 仅用于贴图显示整体的虚id
        fake = 8830007,
        -- 躯干mob
        part = { 8830009, 8830013 },
        minMobId = 8830007,
        maxMobId = 8830013,
        posX = 412,
        posY = 258
    }
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 没办法通过设置处理的就在这里进行重载
function Sample:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    -- xxx
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