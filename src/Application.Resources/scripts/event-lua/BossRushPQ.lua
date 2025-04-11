local isPq = true
local minPlayers, maxPlayers = 1, 6
local minLevel, maxLevel = 1, 255
local entryMap = 970030100
local exitMap = 970030000
local recruitMap = 970030000
local clearMap = 970030000

local minMapId = 970030001
local maxMapId = 970042711

local eventTime = 5 * 60 * 1000 -- 5分钟，单位为毫秒

local maxLobbies = 7

function init()
    setEventRequirements()
end

function getMaxLobbies()
    return maxLobbies
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
    -- 可根据需要设置独占道具或规则
end

function setEventRewards(eim)
    local rewardList = {
        [6] = {{3010061, 1}, {1122018, 1}, {1122005, 1}},
        [5] = {{3010063, 1}, {1122018, 1}, {1122005, 1}},
        [4] = {{1122001, 1}, {1122006, 1}, {1022103, 1}},
        [3] = {{1122002, 1}, {1022088, 1}, {1012076, 1}},
        [2] = {{1122003, 1}, {1012077, 1}, {1012079, 1}},
        [1] = {{1122004, 1}, {1012078, 1}, {1432008, 1}}
    }

    for level, rewards in pairs(rewardList) do
        for _, reward in pairs(rewards) do
            local itemId, qty = reward[1], reward[2]
            eim:setEventReward(level, itemId, qty)
        end
    end
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
    local eim = em:newInstance("BossRush" .. lobbyid)
    eim:setProperty("level", level)
    eim:setProperty("lobby", lobbyid)
    eim:startEventTimer(eventTime)
    setEventRewards(eim)
    setEventExclusives(eim)
    return eim
end

function afterSetup(eim)
    -- 可在设置完成后执行
end

function playerEntry(eim, player)
    local map = eim:getMapInstance(entryMap + eim:getIntProperty("lobby"))
    player:changeMap(map, map:getPortal(0))
end

function scheduledTimeout(eim)
    endEvent(eim)
end

function playerUnregistered(eim, player)
    -- 处理玩家取消注册
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
            endEvent(eim)
        else
            eim:unregisterPlayer(player)
        end
    end
end

function changedLeader(eim, leader)
    local mapid = leader:getMapId()
    if not eim:isEventCleared() and (mapid < minMapId or mapid > maxMapId) then
        endEvent(eim)
    end
end

function playerDead(eim, player)
    -- 处理玩家死亡
end

function playerRevive(eim, player)
    if eim:isEventTeamLackingNow(true, minPlayers, player) then
        eim:unregisterPlayer(player)
        endEvent(eim)
    else
        eim:unregisterPlayer(player)
    end
end

function playerDisconnected(eim, player)
    if eim:isEventTeamLackingNow(true, minPlayers, player) then
        endEvent(eim)
    end
end

function endEvent(eim)
    eim:disposeIfPlayerBelow(minPlayers)
end
