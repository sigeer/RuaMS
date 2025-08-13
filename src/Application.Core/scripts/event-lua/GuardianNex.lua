local BaseEvent = require("scripts/event-lua/__BaseEvent")

local config = {
    name = "GuardianNex",
    instanceName = "Nex_",
    minPlayers = 1,

    exitMap = 240070000,
    eventMap = 240070010,

    eventTimer = 15,
    bossId = nil
}

local Event = BaseEvent:extend()

-- 同时最多可执行的事件
function Event:setup(level, lobbyId)
    local eim = em:newInstance(self.name .. lobbyId)

    self.eventMapObj = eim:getInstanceMap(self.eventMap + 10 * lobbyId)
    self.eventMapObj:resetFully()
    self.eventMapObj:allowSummonState(false)

    local bossIndex = tonumber(lobbyId) + 1
    local bossList =  {7120100, 7120101, 7120102, 8120100, 8120101, 8140510}
    self.bossId = bossList[bossIndex]

    self:respawn(eim)
    eim:startEventTimer(self.eventTimer * 60 * 1000)
    return eim
end

-- 玩家进入事件
function Event:playerEntry(eim, player)
    player:changeMap(self.eventMapObj, 1)
end

-- 玩家退出事件
function Event:playerExit(eim, player)
    eim:unregisterPlayer(player)
    player:changeMap(self.exitMap)
end

-- 玩家更换地图
function Event:changedMap(eim, player, mapId)
    if mapId ~= self.eventMapObj:getId() then
        self:playerDisconnected(eim, player)
    end
end

function Event:monsterValue(eim, mobId)
    return -1
end

-- 玩家复活
function Event:playerRevive(eim, player)
    player:respawn(eim, self.entryMap)
    return false
end

function Event:playerDisconnected(eim, player)
    if eim:isEventTeamLackingNow(true, self.minPlayers, player) then
        eim:unregisterPlayer(player)
        self:endEvent(eim)
    else
        eim:unregisterPlayer(player)
    end
end

function Event:clearPQ(eim)
    eim:stopEventTimer()
    eim:setEventCleared()
end

function Event:monsterKilled(mob, eim)
    if mob:getId() == self.bossId then
        eim:showClearEffect()
        eim:clearPQ()
    end
end

Event:new(config)