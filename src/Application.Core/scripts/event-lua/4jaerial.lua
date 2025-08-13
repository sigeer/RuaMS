local entryMap = 912020000
local exitMap = 120000102

local minMapId = 912020000
local maxMapId = 912020000

local eventDurationInMinutes = 2 -- 2 minutes

local maxLobbiesCount = 7

function getMaxLobbies()
    return maxLobbiesCount
end

function init()
    em:setProperty("noEntry", "false")
end

function setup(level, lobbyId)
    local eventInstance = em:newInstance("4jaerial_" .. lobbyId)
    eventInstance:setProperty("level", level)
    eventInstance:setProperty("boss", "0")
    eventInstance:setProperty("canLeave", "0")

    eventInstance:getInstanceMap(entryMap):resetPQ(level)
    eventInstance:getInstanceMap(entryMap):shuffleReactors()

    respawnStages(eventInstance)
    eventInstance:startEventTimer(eventDurationInMinutes * 60000)
    return eventInstance
end

function afterSetup(eventInstance) end

function respawnStages(eventInstance) end

function playerEntry(eventInstance, player)
    local mapInstance = eventInstance:getMapInstance(entryMap)
    player:changeMap(mapInstance, mapInstance:getPortal(0))
end

function playerUnregistered(eventInstance, player) end

function playerExit(eventInstance, player)
    eventInstance:unregisterPlayer(player)
    eventInstance:dispose()
    em:setProperty("noEntry", "false")
end

function playerLeft(eventInstance, player) end

function scheduledTimeout(eventInstance)
    local player = eventInstance:getPlayers()[1]
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
end

function monsterKilled(mob, eventInstance) end

function leftParty(eventInstance, player) end

function disbandParty(eventInstance) end

function monsterValue(eventInstance, mobId)
    return 1
end

function allMonstersDead(eventInstance) end

function cancelSchedule() end

function dispose() end

-- ---------- FILLER FUNCTIONS ----------

function changedLeader(eventInstance, leader) end