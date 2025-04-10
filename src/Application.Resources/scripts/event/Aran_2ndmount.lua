local entryMap = 921110000
local exitMap = 211050000

local minMapId = 921110000
local maxMapId = 921110000

local eventTime = 3 -- 3 minutes

local maxLobbies = 7

function getMaxLobbies()
    return maxLobbies
end

function init()
    em:setProperty("noEntry", "false")
end

function setup(level, lobbyId)
    local eventInstance = em:newInstance("Aran_2ndmount_" .. lobbyId)
    eventInstance:setProperty("level", level)
    eventInstance:setProperty("boss", "0")

    return eventInstance
end

function respawnStages(eventInstance) end

function playerEntry(eventInstance, player)
    local mapObject = eventInstance:getInstanceMap(entryMap)

    mapObject:resetPQ(1)
    mapObject:instanceMapForceRespawn()
    respawnStages(eventInstance)

    player:changeMap(entryMap, 2)
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

    local player = eventInstance:getPlayers()[1]
    eventInstance:unregisterPlayer(player)
    player:changeMap(exitMap)

    eventInstance:dispose()
    em:setProperty("noEntry", "false")
end

function monsterKilled(mob, eventInstance) end

function monsterValue(eventInstance, mobId)
    return 1
end

function friendlyKilled(mob, eventInstance)
    if em:getProperty("noEntry") ~= "false" then
        local player = eventInstance:getPlayers()[1]
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