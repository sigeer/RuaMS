local entryMap;
local exitMap;
local eventLength = 20;

function init()
    em:setProperty("noEntry", "false");
    entryMap = em:GetMap(922000000);
    exitMap = em:GetMap(922000009);
end

function setup(level, lobbyid)
    local eim = em:newInstance("q3239_" + lobbyid);
    eim:setExclusiveItems({ 4031092 });
    return eim;
end

function playerEntry(eim, player)
    local im = eim:getInstanceMap(entryMap:getId());

    -- Reset instance
    im:clearDrops();
    im:resetReactors();
    im:shuffleReactors();

    -- Start timer
    eim:startEventTimer(eventLength * 60 * 1000);

    -- Warp player and mark event as occupied
    player:changeMap(entryMap, 0);
    em:setProperty("noEntry", "true");
end

function changedMap(eim, player, mapid)
    if (mapid ~= entryMap:getId()) then
        playerExit(eim, player);
    end
end

function playerExit(eim, player)
    finish(eim);
end

function playerDisconnected(eim, player)
    finish(eim);
end

function scheduledTimeout(eim)
    finish(eim);
end

function finish(eim)
    local party = eim:getPlayers(); -- should only ever be one player
    for _, player in ipairs(party) do
        eim:unregisterPlayer(player);
        player:changeMap(exitMap);
    end

    eim:dispose();
    em:setProperty("noEntry", "false");
end

-- Stub/filler functions

function disbandParty(eim, player)
end

function afterSetup(eim) 
end

function playerUnregistered(eim, player)
end

function changedLeader(eim, leader) 
end

function leftParty(eim, player) 
end

function clearPQ(eim) 
end

function dispose() 
end

function cancelSchedule() 
end

function allMonstersDead(eim) 
end

function monsterValue(eim, mobId) 
end

function monsterKilled(mob, eim) 
end
