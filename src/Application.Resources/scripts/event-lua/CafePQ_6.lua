local BaseEvent = require("scripts/event/__BasePQ")

-- 配置事件参数
local config = {
    name = "Lan6_",
    minPlayers = 3,
    maxPlayers = 6,
    minLevel = 21,
    maxLevel = 120,
    entryMap = 197000000,
    exitMap = 193000000,
    recruitMap = 193000000,

    minMapId = 197000000,
    maxMapId = 197010000,
    eventTime = 45,
    maxLobbies = 1,

    respawnConfig = {
        maps = {197000000, 197010000},
        duration = 15000
    }
}

-- 创建自定义事件
local CafePQ6 = BaseEvent:extend()

function CafePQ6:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    eim:setProperty("stage", 0)
    eim:setIntProperty("couponsNeeded", 300)
    return eim
end

function CafePQ6:clearPQ(eim)
    BaseEvent.clearPQ(self, eim)
    eim:giveEventPlayersStageReward(1);

    for _, mapId in ipairs(self.respawnConfig.maps) do
        eim:getInstanceMap(mapId):killAllMonstersNotFriendly()
        eim:showClearEffect(mapId)
    end
end

function CafePQ6:setEventExclusives(eim)
    eim:setExclusiveItems(LuaTableUtils.ToList({ 4001007 }))
end

function CafePQ6:setEventRewards(eim)
    local evLevel = 1
    local itemSet =  { 4001010 }
    local itemQty = { 1}
    eim:setEventRewards(evLevel, LuaTableUtils.ToList(itemSet), LuaTableUtils.ToList(itemQty))

    local expStages = { 10000 }
    eim:setEventClearStageExp(LuaTableUtils.ToList(expStages))
end

function CafePQ6:monsterKilled(mob, eim)
    if (eim:isEventCleared()) then
        return ;
    end

    local mapObj = mob.getMap();
    local itemObj = Item(4001007, 0, getDroppedQuantity(mob));
    local dropper = eim:getPlayers()[0];

    mapObj:spawnItemDrop(mob, dropper, itemObj, mob:getPosition(), true, false);
end

-- 创建事件实例
local event = CafePQ6:new(config)

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