function init()
    scheduleNew()
end

function scheduleNew()
    setupTask = em:schedule("start", 0) -- spawns upon server start. Each 3 hours an server event checks if boss exists, if not spawns it instantly.
end

function cancelSchedule()
    if setupTask ~= nil then
        setupTask:cancel(true)
    end
end

function start()
    local dangerousCroko1 = em:GetMap(107000300)
    if dangerousCroko1:getMonsterById(6220000) ~= nil then
        setupTask = em:schedule("start", 3 * 60 * 60 * 1000)
        return
    end
    local spawnPoint = Point(90, 119)
    dangerousCroko1:spawnMonsterOnGroundBelow(LifeFactory.getMonster(6220000), spawnPoint)
    dangerousCroko1:broadcastMessage(PacketCreator.serverNotice(6, "The huge crocodile Dyle has come out from the swamp."))
    setupTask = em:schedule("start", 3 * 60 * 60 * 1000)
end

-- ---------- FILLER FUNCTIONS ----------

function dispose() end

function setup(eim, leaderId) end

function monsterValue(eim, mobId) return 0 end

function disbandParty(eim, player) end

function playerDisconnected(eim, player) end

function playerEntry(eim, player) end

function monsterKilled(mob, eim) end

function scheduledTimeout(eim) end

function afterSetup(eim) end

function changedLeader(eim, leader) end

function playerExit(eim, player) end

function leftParty(eim, player) end

function clearPQ(eim) end

function allMonstersDead(eim) end

function playerUnregistered(eim, player) end