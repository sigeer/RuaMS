local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 创建武功PQ实例
local config = {
    name = "WuGongPQ",
    instanceName = "WuGongPQ",
    minPlayers = 1,
    maxPlayers = 6,
    minLevel = 25,
    maxLevel = 90,
    entryMap = 701010323,
    exitMap = 701010320,
    recruitMap = 701010322,
    clearMap = 701010323,
    minMapId = 701010323,
    maxMapId = 701010323,
    eventTime = 10,
    maxLobbies = 1,
    rewardConfig = {
        -- 每一关的经验奖励
        expStages = {},
        -- 每一关的金钱奖励
        mesoStages = {},
        -- 最终关卡的物品奖励
        finalItem = {
            level = 1,
            list = {2000005},
            quantity = {5}
        }
    },
    bossConfig = {
        id = 9600009,
        posX = 2261,
        posY = 823,
        difficulty = true,
        drops = {
            items = {2000005},
            counts = {5},
            chances = {0.4}
        }
    }
}

local WuGongPQ = BaseEvent:extend()

function WuGongPQ:monsterKilled(mob, eim)
    if eim:isEventCleared() then
        return
    end

    if mob:getId() == self.bossConfig.id then
        local mapObj = mob:getMap();
        local dropper = eim:getPlayers()[0];
        mapObj.spawnItemDropList({ 2000005 },mob,dropper,mob.getPosition());
    end
end


local event = WuGongPQ:new(config)

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