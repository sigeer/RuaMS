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
    local bossMobId = 9400633
    local bossMapId = 677000012
    local bossMessage = "Astaroth has appeared!"

    local map = em:GetMap(bossMapId)
    if map:getMonsterById(bossMobId) ~= nil then
        em:schedule("start", 3 * 60 * 60 * 1000)
        return
    end

    local boss = LifeFactory.getMonster(bossMobId)
    local bossPosition = Point(842, 0)
    map:spawnMonsterOnGroundBelow(boss, bossPosition)
    map:broadcastMessage(PacketCreator.serverNotice(6, bossMessage))

    em:schedule("start", 3 * 60 * 60 * 1000)
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