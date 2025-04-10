local KC_bfd
local Plane_to_CBD
local CBD_docked
local CBD_bfd
local Plane_to_KC
local KC_docked

-- Time Setting is in milliseconds
local closeTime = 4 * 60 * 1000 -- The time to close the gate
local beginTime = 5 * 60 * 1000 -- The time to begin the ride
local rideTime = 1 * 60 * 1000 -- The time that requires moving to destination

function init()
    closeTime = em:getTransportationTime(closeTime)
    beginTime = em:getTransportationTime(beginTime)
    rideTime = em:getTransportationTime(rideTime)

    KC_bfd = em:GetMap(540010100)
    CBD_bfd = em:GetMap(540010001)
    Plane_to_CBD = em:GetMap(540010101)
    Plane_to_KC = em:GetMap(540010002)
    CBD_docked = em:GetMap(540010000)
    KC_docked = em:GetMap(103000000)
    scheduleNew()
end

function scheduleNew()
    em:setProperty("docked", "true")
    em:setProperty("entry", "true")
    em:schedule("stopEntry", closeTime)
    em:schedule("takeoff", beginTime)
end

function stopEntry()
    em:setProperty("entry", "false")
end

function takeoff()
    em:setProperty("docked", "false")
    KC_bfd:warpEveryone(Plane_to_CBD:getId())
    CBD_bfd:warpEveryone(Plane_to_KC:getId())
    em:schedule("arrived", rideTime) -- The time that requires moving to destination
end

function arrived()
    Plane_to_CBD:warpEveryone(CBD_docked:getId(), 0)
    Plane_to_KC:warpEveryone(KC_docked:getId(), 7)

    scheduleNew()
end

function cancelSchedule() end

-- ---------- FILLER FUNCTIONS ----------

function dispose() end

function setup(eim, leaderid) end

function monsterValue(eim, mobid) return 0 end

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