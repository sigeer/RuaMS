-- 4th Job Snipe / Concentration

local minPlayers = 1

function init()
    em:setProperty("started", "false")
end

function monsterValue(eim, mobId)
    return 1
end

function getEligibleParty(party)
    local eligible = {}
    local hasLeader = false

    if #party > 0 then
        local partyList = party:toArray()

        for i = 1, #party do
            local ch = partyList[i]

            if ch:getMapId() == 105090200 and ch:getLevel() >= 120 then
                if ch:isLeader() then
                    hasLeader = true
                end
                table.insert(eligible, ch)
            end
        end
    end

    if not (hasLeader and #eligible >= minPlayers) then
        eligible = {}
    end
    return eligible
end

function setup(level, lobbyid)
    local eim = em:newInstance("s4aWorld_" .. lobbyid)
    eim:setProperty("level", level)

    eim:getInstanceMap(910500000):resetPQ(1)
    respawnStages(eim)
    eim:getMapInstance(910500000):instanceMapForceRespawn()
    eim:startEventTimer(1200000)

    em:setProperty("started", "true")

    return eim
end

function afterSetup(eim)
    -- Empty function
end

function respawnStages(eim)
    eim:getMapInstance(910500000):instanceMapRespawn()
    eim:schedule("respawnStages", 15 * 1000)
end

function playerEntry(eim, player)
    local map = eim:getMapFactory():getMap(910500000)
    player:changeMap(map, map:getPortal(0))
end

function playerDead(eim, player)
    -- Empty function
end

function playerRevive(eim, player)
    -- Empty function
end

function scheduledTimeout(eim)
    eim:disposeIfPlayerBelow(100, 105090200)

    em:setProperty("started", "false")
end

function changedMap(eim, player, mapid)
    if mapid ~= 910500000 then
        eim:unregisterPlayer(player)

        if eim:disposeIfPlayerBelow(minPlayers, 105090200) then
            em:setProperty("started", "false")
        end
    end
end

function playerDisconnected(eim, player)
    return 0
end

function leftParty(eim, player)
    -- If only 2 players are left, uncompletable:
    playerExit(eim, player)
end

function disbandParty(eim)
    -- Boot whole party and end
    eim:disposeIfPlayerBelow(100, 105090200)

    em:setProperty("started", "false")
end

function playerUnregistered(eim, player)
    -- Empty function
end

function playerExit(eim, player)
    eim:unregisterPlayer(player)
    local map = eim:getMapFactory():getMap(105090200)
    player:changeMap(map, map:getPortal(0))
end

function clearPQ(eim)
    eim:disposeIfPlayerBelow(100, 105090200)

    em:setProperty("started", "false")
end

function monsterKilled(mob, eim)
    -- Empty function
end

function allMonstersDead(eim)
    -- Empty function
end

function cancelSchedule()
    -- Empty function
end

function dispose()
    -- Empty function
end

-- ---------- FILLER FUNCTIONS ----------
function changedLeader(eim, leader)
    -- Empty function
end
