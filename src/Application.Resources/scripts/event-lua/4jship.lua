local entryMap = 912010000
local exitMap = 120000101

local minMapId = 912010000
local maxMapId = 912010200

local eventTime = 4 -- 4 minutes

local maxLobbies = 7

function getMaxLobbies()
    return maxLobbies
end

function init()
    em:setProperty("noEntry", "false")
end

function setup(level, lobbyId)
    local eventInstance = em:newInstance("4jship_" .. lobbyId)
    eventInstance:setProperty("level", level)
    eventInstance:setProperty("boss", "0")
    eventInstance:setProperty("canLeave", "0")

    eventInstance:getInstanceMap(entryMap):resetPQ(level)

    respawnStages(eventInstance)
    eventInstance:startEventTimer(eventTime * 60000)
    eventInstance:schedule("playerCanLeave", 1 * 60000)
    eventInstance:schedule("playerSurvived", 2 * 60000)
    return eventInstance
end

function afterSetup(eventInstance) end

function respawnStages(eventInstance) end

function playerCanLeave(eventInstance)
    eventInstance:setIntProperty("canLeave", 1)
end

function playerSurvived(eventInstance)
    if eventInstance:getLeader():isAlive() then
        eventInstance:setIntProperty("canLeave", 2)
        eventInstance:dropMessage(5, "Kyrin: You have passed the test. Now for the closing part... Are you able reach the exit over there?")
    else
        eventInstance:dropMessage(5, "Kyrin: You have failed the test. Aww, don't have such a sad face, just try it again later, ok?")
    end
end

function playerEntry(eventInstance, player)
    local map = eventInstance:getMapInstance(entryMap)
    player:changeMap(map, map:getPortal(0))
end

function playerUnregistered(eventInstance, player) end

function playerExit(eventInstance, player)
    eventInstance:unregisterPlayer(player)
    eventInstance:dispose()
    em:setProperty("noEntry", "false")
end

function playerLeft(eventInstance, player) end

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

function monsterKilled(mob, eventInstance) end

function leftParty(eventInstance, player) end

function disbandParty(eventInstance) end

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

function changedLeader(eventInstance, leader) end