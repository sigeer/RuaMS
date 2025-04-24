local BasePQ = require("scripts/event-lua/__BasePQ")

-- 创建自定义事件
local CafePQ = BasePQ:extend()

function CafePQ:setup(level, lobbyid)
    local eim = BasePQ.setup(self, level, lobbyid)
    eim:setProperty("stage", 0)
    eim:setIntProperty("couponsNeeded", self.couponsNeeded)

    for _, mapId in ipairs(self.respawnConfig.maps) do
        local mapObj = eim:getInstanceMap(mapId)
        mapObj:resetPQ(level);
        mapObj:toggleDrops();
        mapObj:instanceMapForceRespawn();
    end

    return eim
end

function CafePQ:clearPQ(eim)
    BasePQ.clearPQ(self, eim)
    eim:giveEventPlayersStageReward(1);

    for _, mapId in ipairs(self.respawnConfig.maps) do
        eim:getInstanceMap(mapId):killAllMonstersNotFriendly()
        eim:showClearEffect(mapId)
    end
end

function CafePQ:monsterKilled(mob, eim)
    if (eim:isEventCleared()) then
        return;
    end

    local mapObj = mob:getMap();
    local itemObj = Item(4001007, 0, GetDroppedQuantity(mob));
    local dropper = eim:getPlayers()[0];

    mapObj:spawnItemDrop(mob, dropper, itemObj, mob:getPosition(), true, false);
end

function GetDroppedQuantity(mob)
    if (mob:getLevel() > 65) then
        return 5;
    elseif (mob:getLevel() > 40) then
        return 2;
    else
        return 1;
    end
end


return CafePQ