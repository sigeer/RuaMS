local KC_Waiting;
local Subway_to_KC;
local KC_docked;
local NLC_Waiting;
local Subway_to_NLC;
local NLC_docked;

--Time Setting is in millisecond
local closeTime = 50 * 1000; --The time to close the gate
local beginTime = 1 * 60 * 1000; --The time to begin the ride
local rideTime = 4 * 60 * 1000; --The time that require move to destination

function init()
    closeTime = em:getTransportationTime(closeTime);
    beginTime = em:getTransportationTime(beginTime);
    rideTime = em:getTransportationTime(rideTime);

    KC_Waiting = em:GetMap(600010004);
    NLC_Waiting = em:GetMap(600010002);
    Subway_to_KC = em:GetMap(600010003);
    Subway_to_NLC = em:GetMap(600010005);
    KC_docked = em:GetMap(103000100);
    NLC_docked = em:GetMap(600010001);
    scheduleNew();
end

function scheduleNew() 
    em:setProperty("docked", "true");
    em:setProperty("entry", "true");
    em:schedule("stopEntry", closeTime);
    em:schedule("takeoff", beginTime);
end

function stopEntry() 
    em:setProperty("entry", "false");
end

function takeoff() 
    em:setProperty("docked", "false");
    KC_Waiting:warpEveryone(Subway_to_NLC:getId());
    NLC_Waiting:warpEveryone(Subway_to_KC:getId());
    em:schedule("arrived", rideTime);
end

function arrived() 
    Subway_to_KC:warpEveryone(KC_docked:getId(), 0);
    Subway_to_NLC:warpEveryone(NLC_docked:getId(), 0);
    scheduleNew();
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