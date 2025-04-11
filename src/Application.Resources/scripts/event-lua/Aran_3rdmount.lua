-- This file is part of the HeavenMS MapleStory Server
-- Copyleft (L) 2016 - 2019 RonanLana
--
-- This program is free software: you can redistribute it and/or modify
-- it under the terms of the GNU Affero General Public License as
-- published by the Free Software Foundation version 3 as published by
-- the Free Software Foundation. You may not use, modify or distribute
-- this program under any other version of the GNU Affero General Public
-- License.
--
-- This program is distributed in the hope that it will be useful,
-- but WITHOUT ANY WARRANTY; without even the implied warranty of
-- MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
-- GNU Affero General Public License for more details.
--
-- You should have received a copy of the GNU Affero General Public License
-- along with this program.  If not, see <http://www.gnu.org/licenses/>.

-- Author: Ronan
-- Event - Wolves' Mount Quest

local entryMap = 914030000
local exitMap = 140010210

local minMapId = 914030000
local maxMapId = 914030000

local eventTime = 3 -- 3 minutes

local maxLobbies = 7

function getMaxLobbies()
    return maxLobbies
end

function init()
    em:setProperty("noEntry", "false")
end

function setup(level, lobbyId)
    local eventInstance = em:newInstance("Aran_3rdmount_" .. lobbyId)
    eventInstance:setProperty("level", level)
    eventInstance:setProperty("boss", "0")

    return eventInstance
end

function respawnStages(eventInstance) end

function playerEntry(eventInstance, player)
    local mapObject = eventInstance:getInstanceMap(entryMap)

    mapObject:resetPQ(1)
    mapObject:instanceMapForceRespawn()
    mapObject:closeMapSpawnPoints()
    respawnStages(eventInstance)

    player:changeMap(entryMap, 1)
    em:setProperty("noEntry", "true")
    player:sendPacket(PacketCreator:getClock(eventTime * 60))
    eventInstance:startEventTimer(eventTime * 60000)
end

function playerUnregistered(eventInstance, player) end

function playerExit(eventInstance, player)
    eventInstance:unregisterPlayer(player)
    eventInstance:dispose()
    em:setProperty("noEntry", "false")
end

function scheduledTimeout(eventInstance)
    local player = eventInstance:getPlayers():get(1)
    playerExit(eventInstance, player)
    player:changeMap(exitMap)
end

function playerDisconnected(eventInstance, player)
    playerExit(eventInstance, player)
end

function changedMap(eventInstance, character, mapId)
    if mapId < minMapId or mapId > maxMapId then
        playerExit(eventInstance, character)
    end
end

function clearPQ(eventInstance)
    eventInstance:stopEventTimer()
    eventInstance:setEventCleared()

    local player = eventInstance:getPlayers():get(1)
    eventInstance:unregisterPlayer(player)
    player:changeMap(exitMap)

    eventInstance:dispose()
    em:setProperty("noEntry", "false")
end

function monsterKilled(mob, eventInstance)
    if eventInstance:getInstanceMap(entryMap):countMonsters() == 0 then
        eventInstance:showClearEffect()
    end
end

function monsterValue(eventInstance, mobId)
    return 1
end

function friendlyKilled(mob, eventInstance)
    if em:getProperty("noEntry") ~= "false" then
        local player = eventInstance:getPlayers():get(1)
        playerExit(eventInstance, player)
        player:changeMap(exitMap)
    end
end

function allMonstersDead(eventInstance) end

function cancelSchedule() end

function dispose() end

-- ---------- FILLER FUNCTIONS ----------

function disbandParty(eventInstance, player) end

function afterSetup(eventInstance) end

function changedLeader(eventInstance, leader) end

function leftParty(eventInstance, player) end