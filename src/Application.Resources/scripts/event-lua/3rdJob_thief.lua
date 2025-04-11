local entryMap = 108010400
local exitMap = 107000402

local minMapId = 108010400
local maxMapId = 108010401

local eventTime = 20 -- 20 minutes

local maxLobbies = 7

function getMaxLobbies()
    return maxLobbies
end

function init()
    em:setProperty("noEntry", "false")
end

function setup(level, lobbyId)
    local eventInstance = em:newInstance("3rdJob_thief_" .. lobbyId)
    eventInstance:setProperty("level", level)
    eventInstance:setProperty("boss", "0")

    return eventInstance
end

function playerEntry(eventInstance, player)
    eventInstance:getInstanceMap(maxMapId):resetPQ(1)

    player:changeMap(entryMap, 0)
    em:setProperty("noEntry", "true")
    player:sendPacket(PacketCreator.getClock(eventTime * 60))
    eventInstance:startEventTimer(eventTime * 60000)
end

function playerUnregistered(eventInstance, player)
end

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

function clear(eventInstance)
    local player = eventInstance:getPlayers()[1]
    eventInstance:unregisterPlayer(player)
    player:changeMap(exitMap)

    eventInstance:dispose()
    em:setProperty("noEntry", "false")
end

function changedMap(eventInstance, character, mapId)
    if mapId < minMapId or mapId > maxMapId then
        playerExit(eventInstance, character)
    end
end

function monsterKilled(mob, eventInstance)
end

function monsterValue(eventInstance, mobId)
    return 1
end

function allMonstersDead(eventInstance)
end

function cancelSchedule()
end

function dispose()
end

-- ---------- FILLER FUNCTIONS ----------

function disbandParty(eventInstance, player)
end

function afterSetup(eventInstance)
end

function changedLeader(eventInstance, leader)
end

function leftParty(eventInstance, player)
end

function clearPQ(eventInstance)
end