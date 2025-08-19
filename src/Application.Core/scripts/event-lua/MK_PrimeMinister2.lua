local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    instanceName = "MK_PrimeMinister2_",
    minPlayers = 1,
    maxPlayers = 3,
    minLevel = 30,
    maxLevel = 255,
    entryMap = 106021601,
    entryPortal = 1,
    exitMap = 106021402,
    recruitMap = 106021402,
    clearMap = 0,
    warpTeamWhenClear = false,
    minMapId = 106021601,
    maxMapId = 106021601,
    eventTime = 10,
    maxLobbies = 1,
}

local mobId = 3300008

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 没办法通过设置处理的就在这里进行重载
function Sample:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    -- xxx
    self:respawn(eim)
    return eim
end

function Sample:primeMinisterCheck(eim)
    local map = eim.getMapInstance(self.entryMap);

    return map.getAllPlayers().Length > 0;
end

function Sample:respawn(eim)
    if (self:primeMinisterCheck(eim)) then
        eim:startEventTimer(self.eventTime);

        local weddinghall = eim:getMapInstance(self.entryMap);
        weddinghall:getPortal(self.entryPortal):setPortalState(false);
        weddinghall:spawnMonsterOnGroundBelow(LifeFactory:getMonster(mobId), Point(292, 143));
    else
        eim:schedule("respawn", 10000);
    end
end

function Sample:monsterKilled(mob, eim)
    if (mob:getId() == mobId) then
        eim:getMapInstance(self.entryMap):getPortal(1):setPortalState(true);

        eim:showClearEffect();
        eim:clearPQ();
    end
end

Sample:new(config)