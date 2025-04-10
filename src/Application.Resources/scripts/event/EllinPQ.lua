local isPq = true;
local minPlayers, maxPlayers = 1, 6
local minLevel, maxLevel = 44, 55
local entryMap = 930000000;
local exitMap = 930000800;
local recruitMap = 300030100;
local clearMap = 930000800;

local minMapId = 930000000;
local maxMapId = 930000800;

local eventTime = 30;

local maxLobbies = 1;

function init()
    setEventRequirements();
end

function getMaxLobbies()
    return maxLobbies;
end

function setEventRequirements()
    local reqStr = "\n   组队人数: "
    if maxPlayers - minPlayers >= 1 then
        reqStr = reqStr .. minPlayers .. " ~ " .. maxPlayers
    else
        reqStr = reqStr .. minPlayers
    end

    reqStr = reqStr .. "\n   等级要求: "
    if maxLevel - minLevel >= 1 then
        reqStr = reqStr .. minLevel .. " ~ " .. maxLevel
    else
        reqStr = reqStr .. minLevel
    end

    reqStr = reqStr .. "\n   时间限制: " .. (eventTime / 60000) .. " 分钟"

    em:setProperty("party", reqStr)
end

function setEventExclusives(eim)
    local itemSet = { 4001162, 4001163, 4001169, 2270004 };
    eim:setExclusiveItems(itemSet);
end

function setEventRewards(eim)
end

function getEligibleParty(party)
    local eligible = {}
    local hasLeader = false

    if #party > 0 then
        for _, player in ipairs(party) do
            if player:getMapId() == recruitMap and player:getLevel() >= minLevel and player:getLevel() <= maxLevel then
                if player:isLeader() then
                    hasLeader = true
                end
                table.insert(eligible, player)
            end
        end
    end

    if not (hasLeader and #eligible >= minPlayers and #eligible <= maxPlayers) then
        eligible = {}
    end

    return eligible
end

function setup(level, lobbyid)
    local eim = em:newInstance("Ellin" + lobbyid);
    eim:setProperty("level", level);

    eim:setProperty("statusStg4", 0);

    eim:getInstanceMap(930000000):resetPQ(level);
    eim:getInstanceMap(930000100):resetPQ(level);
    eim:getInstanceMap(930000200):resetPQ(level);
    eim:getInstanceMap(930000300):resetPQ(level);
    eim:getInstanceMap(930000400):resetPQ(level);
    local map = eim:getInstanceMap(930000500);
    map:resetPQ(level);
    map:shuffleReactors();
    eim:getInstanceMap(930000600):resetPQ(level);
    eim:getInstanceMap(930000700):resetPQ(level);

    respawnStg2(eim);

    eim:startEventTimer(eventTime * 60000);
    setEventRewards(eim);
    setEventExclusives(eim);
    return eim;
end

function afterSetup(eim)
end

function respawnStg2(eim)
    if (#eim:getMapInstance(930000200):getPlayers() == 0) then
        eim:getMapInstance(930000200):instanceMapRespawn();
    end

    eim:schedule("respawnStg2", 4 * 1000);
end

function changedMap(eim, player, mapid)
    if (mapid < minMapId or mapid > maxMapId) then
        if (eim:isEventTeamLackingNow(true, minPlayers, player)) then
            eim:unregisterPlayer(player);
            finish(eim);
        else
            eim:unregisterPlayer(player);
        end
    end
end

function changedLeader(eim, leader)
    local mapid = leader:getMapId();
    if (not eim:isEventCleared() and (mapid < minMapId or mapid > maxMapId)) then
        finish(eim);
    end
end

function playerEntry(eim, player)
    local map = eim:getMapInstance(entryMap);
    player:changeMap(map, map:getPortal(0));
end

function scheduledTimeout(eim)
    finish(eim);
end

function playerUnregistered(eim, player)
end

function playerExit(eim, player)
    eim:unregisterPlayer(player);
    player:changeMap(exitMap, 0);
end

function playerLeft(eim, player)
    if (not eim:isEventCleared()) then
        playerExit(eim, player);
    end
end

function playerDead(eim, player) 
end

function playerRevive(eim, player)
    if (eim:isEventTeamLackingNow(true, minPlayers, player)) then
        eim:unregisterPlayer(player);
        finish(eim);
    else
        eim:unregisterPlayer(player);
    end
end


function playerDisconnected(eim, player)
    if (eim:isEventTeamLackingNow(true, minPlayers, player)) then
        finish(eim);
    else
        playerExit(eim, player);
    end
end

function leftParty(eim, player)
    if (eim:isEventTeamLackingNow(false, minPlayers, player)) then
        finish(eim);
    else
        playerLeft(eim, player);
    end
end

function disbandParty(eim)
    if (not eim:isEventCleared()) then
        finish(eim);
    end
end

function monsterValue(eim, mobId)
    return 1;
end

function finish(eim)
    local party = eim:getPlayers();

    for _, player in ipairs(party) do
        playerExit(eim, player);
    end
    eim:dispose();
end

function clearPQ(eim)
    eim:stopEventTimer();
    eim:setEventCleared();
end

function isPoisonGolem(mob)
    return  mob:getId() == 9300182
end

function monsterKilled(mob, eim, hasKiller)
    local map = mob:getMap();

    if (isPoisonGolem(mob)) then
        eim:showClearEffect(map:getId());
        eim:clearPQ();
    elseif (map:countMonsters() == 0) then
        local stage = ((map:getId() % 1000) / 100);

        if (stage == 1 or (stage == 4 and not hasKiller)) then
            eim:showClearEffect(map:getId());
        end
    end
end

function allMonstersDead(eim)
end

function cancelSchedule()
end

function dispose(eim)
end
