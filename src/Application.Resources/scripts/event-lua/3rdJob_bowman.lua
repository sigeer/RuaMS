local entryMap = 108010100
local exitMap = 105040305

local minMapId = 108010100
local maxMapId = 108010101

local eventTime = 20 -- 20 minutes

local maxLobbies = 7

function getMaxLobbies()
    return maxLobbies
end

function init()
    em:setProperty("noEntry", "false")
end

function setup(level, lobbyid)
    local eim = em:newInstance("3rdJob_bowman_" .. lobbyid)
    eim:setProperty("level", level)
    eim:setProperty("boss", "0")

    return eim
end

function playerEntry(eim, player)
    eim:getInstanceMap(maxMapId):resetPQ(1)

    player:changeMap(entryMap, 0)
    em:setProperty("noEntry", "true")
    player:sendPacket(PacketCreator.getClock(eventTime * 60))
    eim:startEventTimer(eventTime * 60000)
end

function playerUnregistered(eim, player)
end

function playerExit(eim, player)
    eim:unregisterPlayer(player)
    eim:dispose()
    em:setProperty("noEntry", "false")
end

function scheduledTimeout(eim)
    local player = eim:getPlayers():get(1)
    playerExit(eim, eim:getPlayers():get(1))
    player:changeMap(exitMap)
end

function playerDisconnected(eim, player)
    playerExit(eim, player)
end

function clear(eim)
    local player = eim:getPlayers():get(1)
    eim:unregisterPlayer(player)
    player:changeMap(exitMap)

    eim:dispose()
    em:setProperty("noEntry", "false")
end

function changedMap(eim, chr, mapid)
    if mapid < minMapId or mapid > maxMapId then
        playerExit(eim, chr)
    end
end

function monsterKilled(mob, eim)
end

function monsterValue(eim, mobId)
    return 1
end

function allMonstersDead(eim)
end

function cancelSchedule()
end

function dispose()
end

-- ---------- FILLER FUNCTIONS ----------

function disbandParty(eim, player)
end

function afterSetup(eim)
end

function changedLeader(eim, leader)
end

function leftParty(eim, player)
end

function clearPQ(eim)
end
