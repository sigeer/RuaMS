local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    name = "",
    minPlayers = 1,
    maxPlayers = 6,
    minLevel = 1,
    maxLevel = 255,
    entryMap = 0,
    entryPortal = 0,
    exitMap = 0,
    recruitMap = 0,
    clearMap = 0,
    warpTeamWhenClear = false,
    minMapId = 0,
    maxMapId = 0,
    eventTime = 30,
    maxLobbies = 1,

    -- base.setup.resetMap 中调用
    resetPQMaps = {},
    resetReactorMaps = { },

    -- base.setup.setEventExclusives 任务特有的道具，需要被清理
    eventItems = { },
    -- base.setup.setEventRewards 奖励设置
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