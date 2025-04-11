local isPartyQuest = true
local onlyMarriedPlayers = true
local minPlayers = 6
local maxPlayers = 6
local minLevel = 40
local maxLevel = 255
local entryMap = 670010200
local exitMap = 670011000
local recruitMap = 670010100
local clearMap = 670010800

local minMapId = 670010200
local maxMapId = 670010800

local eventTime = 75     -- 75 minutes

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

    reqStr = reqStr .. "\r\n    At least 1 of both genders"
    if onlyMarriedPlayers then
        reqStr = reqStr .. "\r\n    All married"
    end

    reqStr = reqStr .. "\r\n   时间限制: "
    reqStr = reqStr .. eventTime .. " 分钟"

    em:setProperty("party", reqStr)
end

function setEventExclusives(eim)
    local itemSet = {4031594, 4031595, 4031596, 4031597}
    eim:setExclusiveItems(itemSet)
end

function setEventRewards(eim)
    local evLevel = 1    --Rewards at clear PQ
    local itemSet = {}
    local itemQty = {}
    eim:setEventRewards(evLevel, itemSet, itemQty)

    local expStages = {2000, 4000, 6000, 8000, 9000, 11000}    --bonus exp given on CLEAR stage signal
    eim:setEventClearStageExp(expStages)
end

function getEligibleParty(party)      --selects, from the given party, the team that is allowed to attempt this event
    local eligible = {}
    local hasLeader = false
    local hasNotMarried = false
    local mask = 0

    if party:Size() > 0 then
        local partyList = party:ToArray()

        for i = 1, party:Size() do
            local ch = partyList[i]

            if ch:getMapId() == recruitMap and ch:getLevel() >= minLevel and ch:getLevel() <= maxLevel then
                if ch:isLeader() then
                    hasLeader = true
                end
                if not ch:getPlayer():isMarried() then
                    hasNotMarried = true
                end
                table.insert(eligible, ch)

                mask = bit.bor(mask, bit.lshift(1, ch:getPlayer():getGender()))
            end
        end
    end

    if not (hasLeader and #eligible >= minPlayers and #eligible <= maxPlayers and mask == 3) then
        eligible = {}
    end
    if onlyMarriedPlayers and hasNotMarried then
        eligible = {}
    end
    return eligible
end

function setup(level, lobbyid)
    local eim = em:newInstance("Amoria" .. lobbyid)
    eim:setProperty("level", level)

    eim:setProperty("marriedGroup", 0)
    eim:setProperty("missCount", 0)
    eim:setProperty("statusStg1", -1)
    eim:setProperty("statusStg2", -1)
    eim:setProperty("statusStg3", -1)
    eim:setProperty("statusStg4", -1)
    eim:setProperty("statusStg5", -1)
    eim:setProperty("statusStg6", -1)
    eim:setProperty("statusStgBonus", 0)

    -- 重置所有地图
    local maps = {
        670010200, 670010300, 670010301, 670010302,
        670010400, 670010500, 670010600, 670010700,
        670010750, 670010800
    }
    
    for _, mapId in ipairs(maps) do
        eim:getInstanceMap(mapId):resetPQ(level)
    end

    -- 切换掉落
    for _, mapId in ipairs({670010200, 670010300, 670010301, 670010302}) do
        eim:getInstanceMap(mapId):toggleDrops()
    end

    -- 强制刷新地图
    eim:getInstanceMap(670010200):instanceMapForceRespawn()
    eim:getInstanceMap(670010500):instanceMapForceRespawn()

    -- 打乱反应堆
    eim:getInstanceMap(670010750):shuffleReactors()
    eim:getInstanceMap(670010800):shuffleReactors()

    -- 生成怪物
    local mapObj = eim:getInstanceMap(670010700)
    local mobObj = LifeFactory:getMonster(9400536)
    mapObj:spawnMonsterOnGroundBelow(mobObj, Point(942, 478))

    respawnStages(eim)

    eim:startEventTimer(eventTime * 60000)
    setEventRewards(eim)
    setEventExclusives(eim)

    return eim
end

function isTeamAllCouple(eim)     -- everyone partner of someone on the team
    local eventPlayers = eim:getPlayers()
    
    for _, chr in ipairs(eventPlayers) do
        local pid = chr:getPartnerId()
        if pid <= 0 or eim:getPlayerById(pid) == nil then
            return false
        end
    end

    return true
end

function afterSetup(eim)
    if isTeamAllCouple(eim) then
        eim:setIntProperty("marriedGroup", 1)
    end
end

function respawnStages(eim)
end

function playerEntry(eim, player)
    local map = eim:getMapInstance(entryMap)
    player:changeMap(map, map:getPortal(0))
end

function scheduledTimeout(eim)
    if eim:getIntProperty("statusStg6") == 1 then
        eim:warpEventTeam(exitMap)
    else
        end_(eim)
    end
end

function playerUnregistered(eim, player)
end

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
        if eim:isEventTeamLackingNow(true, minPlayers, player) then
            eim:unregisterPlayer(player)
            end_(eim)
        else
            eim:unregisterPlayer(player)
        end
    end
end

function changedLeader(eim, leader)
    local mapid = leader:getMapId()
    if not eim:isEventCleared() and (mapid < minMapId or mapid > maxMapId) then
        end_(eim)
    end
end

function playerDead(eim, player)
end

function playerRevive(eim, player)
    if eim:isEventTeamLackingNow(true, minPlayers, player) then
        eim:unregisterPlayer(player)
        end_(eim)
    else
        eim:unregisterPlayer(player)
    end
end

function playerDisconnected(eim, player)
    if eim:isEventTeamLackingNow(true, minPlayers, player) then
        eim:unregisterPlayer(player)
        end_(eim)
    else
        eim:unregisterPlayer(player)
    end
end

function leftParty(eim, player)
    if eim:isEventTeamLackingNow(false, minPlayers, player) then
        end_(eim)
    else
        playerLeft(eim, player)
    end
end

function disbandParty(eim)
    if not eim:isEventCleared() then
        end_(eim)
    end
end

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

function monsterKilled(mob, eim)
end

function allMonstersDead(eim)
end

function cancelSchedule()
end

function dispose(eim)
end