local BaseEvent = require("scripts/event-lua/__BaseEvent")

local config = {
    name = "Cygnus_Magic_Library",
    instanceName = "Cygnus_Magic_Library_",
    entryMap = 910110000,
    exitMap = 101000000,
    eventMap = 101000000,

    eventTimer = 10,
}

local Event = BaseEvent:extend()

-- 同时最多可执行的事件
function Event:setup(level, lobbyId)
    local eim = em:newInstance(self.name .. lobbyId)
    local eventMapObj = eim:getInstanceMap(self.eventMap)
    eventMapObj:resetFully()
    eventMapObj:allowSummonState(false)

    self:respawn(eim)
    eim:startEventTimer(self.eventTimer * 60 * 1000)
    return eim
end

function Event:respawn(eim)
    local map = eim:getMapInstance(self.entryMap)
    map:allowSummonState(true)
    map:instanceMapRespawn()
    eim:schedule("respawn", 10000)
end

-- 玩家进入事件
function Event:playerEntry(eim, player)
    local magicLibrary = eim:getMapInstance(self.eventMap)
    player:changeMap(magicLibrary, magicLibrary:getPortal(1))
end

-- 玩家退出事件
function Event:playerExit(eim, player)
    eim:unregisterPlayer(player)
    player:changeMap(self.exitMap, 2)
end

-- 玩家更换地图
function Event:changedMap(eim, player, mapId)
    if mapId == self.exitMap then
        self:playerExit(eim, player)
        eim:dispose()
    end
end

function Event:monsterValue(eim, mobId)
    return -1
end

-- 玩家复活
function Event:playerRevive(eim, player)
    player:respawn(eim, self.entryMap)
    -- 如果是true 这里的respawn没有一样
    return false
end

function Event:playerDisconnected(eim, player)
    for _, p in ipairs(eim:getPlayers()) do
        if p == player then
            self:removePlayer(eim, p)
        else
            self:playerExit(eim, p)
        end
    end
    eim:dispose()
end

function Event:removePlayer(eim, player)
    eim:unregisterPlayer(player)
    player:getMap():removePlayer(player)
    player:setMap(self.exitMap)
    -- player:changeMap(self.exitMap)
end

Event:new(config)