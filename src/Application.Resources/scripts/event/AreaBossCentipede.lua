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
    local herbGarden = em:GetMap(251010102)

    if herbGarden:getMonsterById(5220004) ~= nil then
        em:schedule("start", 3 * 60 * 60 * 1000)
        return
    end

    local giantCentipede = LifeFactory.getMonster(5220004)
    herbGarden:spawnMonsterOnGroundBelow(giantCentipede, Point(560, 50))
    herbGarden:broadcastMessage(PacketCreator.serverNotice(6, "From the mists surrounding the herb garden, the gargantuous Giant Centipede appears."))
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