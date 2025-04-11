local entryMap = 923010000
local exitMap = 923010100

local minMapId = 923010000
local maxMapId = 923010000

local eventMaps = {923010000}

local eventTime = 5 -- 5 minutes

local maxLobbies = 7

function getMaxLobbies()
    return maxLobbies
end

function init()
    em:setProperty("noEntry", "false")
end

function checkHogHealth(eim)
    local watchHog = eim:getInstanceMap(923010000):getMonsterById(9300102)
    if watchHog ~= nil then
        local hp = watchHog:getHp()
        local oldHp = eim:getIntProperty("whog_hp")

        if oldHp - hp > 1000 then -- or 800, if using mobHP / eventTime
            eim:dropMessage(6, "Please protect the pig from the aliens!") -- thanks Vcoc
        end
        eim:setIntProperty("whog_hp", hp)
    end
end

function respawnStages(eim)
    for i = 1, #eventMaps do
        eim:getInstanceMap(eventMaps[i]):instanceMapRespawn()
    end
    checkHogHealth(eim)

    eim:schedule("respawnStages", 10 * 1000)
end

function setup(level, lobbyid)
    local eim = em:newInstance("3rdJob_mount_" .. lobbyid)
    eim:setProperty("level", level)
    eim:setProperty("boss", "0")
    eim:setProperty("whog_hp", "0")

    return eim
end

function playerEntry(eim, player)
    local mapObj = eim:getInstanceMap(entryMap)

    mapObj:resetPQ(1)
    mapObj:instanceMapForceRespawn()
    respawnStages(eim)

    player:changeMap(entryMap, 0)
    em:setProperty("noEntry", "true")
    player:sendPacket(PacketCreator.getClock(eventTime * 60))
    eim:startEventTimer(eventTime * 60000)
end

function playerUnregistered(eim, player)
end

function playerExit(eim, player)
    local api = player:getAbstractPlayerInteraction()
    api:removeAll(4031507)
    api:removeAll(4031508)

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

function changedMap(eim, chr, mapid)
    if mapid < minMapId or mapid > maxMapId then
        playerExit(eim, chr)
    end
end

function clearPQ(eim)
    eim:stopEventTimer()
    eim:setEventCleared()

    local player = eim:getPlayers():get(1)
    eim:unregisterPlayer(player)
    player:changeMap(exitMap)

    eim:dispose()
    em:setProperty("noEntry", "false")
end

function monsterKilled(mob, eim)
end

function monsterValue(eim, mobId)
    return 1
end

function friendlyKilled(mob, eim)
    if em:getProperty("noEntry") ~= "false" then
        local player = eim:getPlayers()[1]
        playerExit(eim, player)
        player:changeMap(exitMap)
    end
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
