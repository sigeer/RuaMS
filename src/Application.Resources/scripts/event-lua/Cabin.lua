local Orbis_btf;
local Leafre_btf;
local Cabin_to_Orbis;
local Cabin_to_Leafre;
local Orbis_docked;
local Leafre_docked;

--Time Setting is in millisecond
local closeTime = 4 * 60 * 1000; -- The time to close the gate
local beginTime = 5 * 60 * 1000; --The time to begin the ride
local rideTime = 5 * 60 * 1000; --The time that require move to destination

function init()
    closeTime = em:getTransportationTime(closeTime);
    beginTime = em:getTransportationTime(beginTime);
    rideTime = em:getTransportationTime(rideTime);

    Orbis_btf = em:GetMap(200000132);
    Leafre_btf = em:GetMap(240000111);
    Cabin_to_Orbis = em:GetMap(200090210);
    Cabin_to_Leafre = em:GetMap(200090200);
    Orbis_docked = em:GetMap(200000131);
    Leafre_docked = em:GetMap(240000110);
    Orbis_Station = em:GetMap(200000100);
    Leafre_Station = em:GetMap(240000100);

    scheduleNew();
end

function scheduleNew()
    em:setProperty("docked", "true");
    Orbis_docked:setDocked(true);
    Leafre_docked:setDocked(true);

    em:setProperty("entry", "true");
    em:schedule("stopEntry", closeTime); --The time to close the gate
    em:schedule("takeoff", beginTime); --The time to begin the ride
end

function stopEntry()
    em:setProperty("entry", "false");
    em:setProperty("next", os.time() * 1000 + em:getTransportationTime(beginTime - closeTime + rideTime));
end

function takeoff()
    Orbis_btf:warpEveryone(Cabin_to_Leafre:getId());
    Leafre_btf:warpEveryone(Cabin_to_Orbis:getId());

    Orbis_docked:broadcastShip(false);
    Leafre_docked:broadcastShip(false);

    em:setProperty("docked", "false");
    Orbis_docked:setDocked(false);
    Leafre_docked:setDocked(false);

    em:schedule("arrived", rideTime); --The time that require move to destination
end

function arrived()
    Cabin_to_Orbis:warpEveryone(Orbis_Station:getId(), 0);
    Cabin_to_Leafre:warpEveryone(Leafre_Station:getId(), 0);

    Orbis_docked:broadcastShip(true);
    Leafre_docked:broadcastShip(true);

    scheduleNew();
end

function cancelSchedule()
end


-- ---------- FILLER FUNCTIONS ----------

function dispose()
end

function setup(eim, leaderid)
end

function monsterValue(eim, mobid)
end

function disbandParty(eim, player)
end

function playerDisconnected(eim, player)
end

function playerEntry(eim, player)
end

function monsterKilled(mob, eim)
end

function scheduledTimeout(eim)
end

function afterSetup(eim)
end

function changedLeader(eim, leader)
end

function playerExit(eim, player)
end

function leftParty(eim, player)
end

function clearPQ(eim)
end

function allMonstersDead(eim)
end

function playerUnregistered(eim, player)
end

