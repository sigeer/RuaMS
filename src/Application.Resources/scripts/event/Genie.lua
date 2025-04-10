
-- 天空之城 - 阿里安特
local Orbis_btf;
local Genie_to_Orbis;
local Orbis_docked;
local Ariant_btf;
local Genie_to_Ariant;
local Ariant_docked;

--Time Setting is in millisecond
local closeTime = 4 * 60 * 1000; --The time to close the gate
local beginTime = 5 * 60 * 1000; --The time to begin the ride
local rideTime = 5 * 60 * 1000; --The time that require move to destination

function init()
    closeTime = em:getTransportationTime(closeTime);
    beginTime = em:getTransportationTime(beginTime);
    rideTime = em:getTransportationTime(rideTime);

    Orbis_btf = em:GetMap(200000152);
    Ariant_btf = em:GetMap(260000110);
    Genie_to_Orbis = em:GetMap(200090410);
    Genie_to_Ariant = em:GetMap(200090400);
    Orbis_docked = em:GetMap(200000151);
    Ariant_docked = em:GetMap(260000100);
    Orbis_Station = em:GetMap(200000100);

    scheduleNew();
end

function scheduleNew()
    em:setProperty("docked", "true");
    Orbis_docked:setDocked(true);
    Ariant_docked:setDocked(true);

    em:setProperty("entry", "true");
    em:schedule("stopEntry", closeTime); --The time to close the gate
    em:schedule("takeoff", beginTime); --The time to begin the ride
end

function stopEntry()
    em:setProperty("entry", "false");
    em:setProperty("next", os.time() * 1000 + em:getTransportationTime(beginTime - closeTime + rideTime));
end

function takeoff()
    Orbis_btf:warpEveryone(Genie_to_Ariant:getId());
    Ariant_btf:warpEveryone(Genie_to_Orbis:getId());
    Orbis_docked:broadcastShip(false);
    Ariant_docked:broadcastShip(false);

    em:setProperty("docked", "false");
    Orbis_docked:setDocked(false);
    Ariant_docked:setDocked(false);

    em:schedule("arrived", rideTime); --The time that require move to destination
end

function arrived()
    Genie_to_Orbis:warpEveryone(Orbis_Station:getId(), 0);
    Genie_to_Ariant:warpEveryone(Ariant_docked:getId(), 1);
    Orbis_docked:broadcastShip(true);
    Ariant_docked:broadcastShip(true);

end

function cancelSchedule()
end

-- ---------- 辅助函数 ----------
function dispose()
end

function setup(eim, leaderid)
end

function monsterValue(eim, mobid)
    return 0
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