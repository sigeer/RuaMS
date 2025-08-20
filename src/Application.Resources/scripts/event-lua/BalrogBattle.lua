-- 基础配置
local isPq = true
local minPlayers = 6
local maxPlayers = 30
local minLevel = 50
local maxLevel = 255
local entryMap = 105100300
local exitMap = 105100100
local recruitMap = 105100100
local clearMap = 105100301

local minMapId = 105100300
local maxMapId = 105100301

local minMobId = 8830000
local maxMobId = 8830006
local bossMobId = 8830003

local eventTime = 60         -- 60 minutes
local releaseClawTime = 1

local maxLobbies = 1
minPlayers = YamlConfig.config.server.USE_ENABLE_SOLO_EXPEDITIONS and 1 or minPlayers  --如果解除远征队人数限制，则最低人数改为1人

function init()
    setEventRequirements()
end

function getMaxLobbies()
    return maxLobbies
end

function setEventRequirements()
    local reqStr = ""

    reqStr = reqStr .. "\r\n   组队人数: "
    if maxPlayers - minPlayers >= 1 then
        reqStr = reqStr .. minPlayers .. " ~ " .. maxPlayers
    else
        reqStr = reqStr .. minPlayers
    end

    reqStr = reqStr .. "\r\n   等级要求: "
    if maxLevel - minLevel >= 1 then
        reqStr = reqStr .. minLevel .. " ~ " .. maxLevel
    else
        reqStr = reqStr .. minLevel
    end

    reqStr = reqStr .. "\r\n   时间限制: "
    reqStr = reqStr .. eventTime .. " 分钟"

    em:setProperty("party", reqStr)
end

function setEventExclusives(eim)
    local itemSet = {}
    eim:setExclusiveItems(itemSet)
end

function setEventRewards(eim)
    local evLevel = 1    --Rewards at clear PQ
    local itemSet = {}
    local itemQty = {}
    eim:setEventRewards(evLevel, itemSet, itemQty)

    local expStages = {}    --bonus exp given on CLEAR stage signal
    eim:setEventClearStageExp(expStages)
end

function getEligibleParty(party)      --selects, from the given party, the team that is allowed to attempt this event
    local eligible = {}
    local hasLeader = false

    if party:size() > 0 then
        local partyList = party:toArray()

        for i = 1, party:size() do
            local ch = partyList[i]

            if ch:getMapId() == recruitMap and ch:getLevel() >= minLevel and ch:getLevel() <= maxLevel then
                if ch:isLeader() then
                    hasLeader = true
                end
                table.insert(eligible, ch)
            end
        end
    end

    if not (hasLeader and #eligible >= minPlayers and #eligible <= maxPlayers) then
        eligible = {}
    end
    return eligible
end

function setup(level, lobbyid)
    local eim = em:newInstance("Balrog" .. lobbyid)
    eim:setProperty("level", level)
    eim:setProperty("boss", "0")

    eim:getInstanceMap(105100300):resetPQ(level)
    eim:getInstanceMap(105100301):resetPQ(level)
    eim:schedule("releaseLeftClaw", releaseClawTime * 60000)

    respawnStages(eim)
    eim:startEventTimer(eventTime * 60000)
    setEventRewards(eim)
    setEventExclusives(eim)
    return eim
end

function afterSetup(eim)
    spawnBalrog(eim)
end

function respawnStages(eim) end

function releaseLeftClaw(eim)
    eim:getInstanceMap(entryMap):killMonster(8830006)
end

function spawnBalrog(eim)
    local mapObj = eim:getInstanceMap(entryMap)
    mapObj:spawnFakeMonsterOnGroundBelow(LifeFactory:getMonster(8830000), Point(412, 258))
    mapObj:spawnMonsterOnGroundBelow(LifeFactory:getMonster(8830002), Point(412, 258))
    mapObj:spawnMonsterOnGroundBelow(LifeFactory:getMonster(8830006), Point(412, 258))
end

function spawnSealedBalrog(eim)
    eim:getInstanceMap(entryMap):spawnMonsterOnGroundBelow(LifeFactory:getMonster(bossMobId), Point.new(412, 258))
end

function playerEntry(eim, player)
    local map = eim:getMapInstance(entryMap)
    player:changeMap(map, map:getPortal(0))
end

function scheduledTimeout(eim)
    end_(eim)
end

function playerUnregistered(eim, player) end

function playerExit(eim, player)
    eim:unregisterPlayer(player)
    player:changeMap(exitMap, 0)
end

function playerLeft(eim, player)
    if not eim:isEventCleared() then
        playerExit(eim, player)
    end
end

function changedMap(eim, player, mapid)
    if mapid < minMapId or mapid > maxMapId then
        if eim:isExpeditionTeamLackingNow(true, minPlayers, player) then
            eim:unregisterPlayer(player)
            end_(eim)
        else
            eim:unregisterPlayer(player)
        end
    end
end

function changedLeader(eim, leader) end

function playerDead(eim, player) end

function playerRevive(eim, player) -- player presses ok on the death pop up.
    if eim:isExpeditionTeamLackingNow(true, minPlayers, player) then
        eim:unregisterPlayer(player)
        end_(eim)
    else
        eim:unregisterPlayer(player)
    end
end

function playerDisconnected(eim, player)
    if eim:isExpeditionTeamLackingNow(true, minPlayers, player) then
        eim:unregisterPlayer(player)
        end_(eim)
    else
        eim:unregisterPlayer(player)
    end
end

function leftParty(eim, player) end

function disbandParty(eim) end

function monsterValue(eim, mobId)
    return 1
end

function end_(eim)
    local party = eim:getPlayers()
    for i = 1, party:size() do
        playerExit(eim, party:get(i))
    end
    eim:dispose()
end

function giveRandomEventReward(eim, player)
    eim:giveEventReward(player)
end

function clearPQ(eim)
    eim:stopEventTimer()
    eim:setEventCleared()
end

function isUnsealedBalrog(mob)
    local balrogid = mob:getId() - 8830000
    return balrogid >= 0 and balrogid <= 2
end

function isBalrogBody(mob)
    return mob:getId() == minMobId
end

function monsterKilled(mob, eim)
    if isUnsealedBalrog(mob) then
        local count = eim:getIntProperty("boss")

        if count == 2 then
            eim:showClearEffect()
            eim:clearPQ()

            eim:dispatchRaiseQuestMobCount(bossMobId, entryMap)
            eim:dispatchRaiseQuestMobCount(9101003, entryMap) -- thanks Atoot for noticing quest not getting updated after boss kill
            mob:getMap():broadcastBalrogVictory(eim:getLeader():getName())
        else
            if count == 1 then
                local mapobj = eim:getInstanceMap(entryMap)
                mapobj:makeMonsterReal(mapobj:getMonsterById(8830000))
            end

            eim:setIntProperty("boss", count + 1)
        end

        if isBalrogBody(mob) then
            eim:schedule("spawnSealedBalrog", 10 * 1000)
        end
    end
end

function allMonstersDead(eim) end

function cancelSchedule() end

function dispose(eim) end