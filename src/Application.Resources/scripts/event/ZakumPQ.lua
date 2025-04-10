local isPq = true
local minPlayers = 6
local maxPlayers = 6
local minLevel = 50
local maxLevel = 255
local entryMap = 280010000
local exitMap = 211042300
local recruitMap = 211042300
local clearMap = 211042300

local minMapId = 280010000
local maxMapId = 280011006

local eventTime = 30  -- 30 minutes

local maxLobbies = 1

function init()
setEventRequirements()
end

function getMaxLobbies()
return maxLobbies
end

function setEventRequirements()
    local reqStr = ""
reqStr = reqStr.. "\r\n   组队人数: "

if maxPlayers - minPlayers >= 1 then
reqStr = reqStr..minPlayers.. " ~ "..maxPlayers
    else
reqStr = reqStr..minPlayers
end

reqStr = reqStr.. "\r\n   等级要求: "

if maxLevel - minLevel >= 1 then
reqStr = reqStr..minLevel.. " ~ "..maxLevel
    else
reqStr = reqStr..minLevel
end

reqStr = reqStr.. "\r\n   时间限制: "..eventTime.. " 分钟"

em: setProperty("party", reqStr)
end

function setEventExclusives(eim)
    local itemSet = { 4001015, 4001016, 4001018}
eim: setExclusiveItems(itemSet)
end

function setEventRewards(eim)
    local itemSet, itemQty, evLevel, expStages

evLevel = 1  -- Rewards at clear PQ
itemSet = {}
itemQty = {}
eim: setEventRewards(evLevel, itemSet, itemQty)

expStages = {}  -- Bonus exp given on clear stage signal
eim: setEventClearStageExp(expStages)
end

function getEligibleParty(party)  --Selects from the given party, the team that is allowed to attempt this event
    local eligible = {}
    local hasLeader = false

if party: size() > 0 then
        local partyList = party: toArray()

for i = 0, party: size() - 1 do
            local ch = partyList[i]

            if ch: getMapId() == recruitMap and ch: getLevel() >= minLevel and ch: getLevel() <= maxLevel then
if ch: isLeader() then
hasLeader = true
end
table.insert(eligible, ch)
end
end
end

if not(hasLeader and #eligible >= minPlayers and #eligible <= maxPlayers) then
eligible = {}
end

return eligible
end

function setup(level, lobbyid)
    local eim = em: newInstance("PreZakum"..lobbyid)
eim: setProperty("level", level)
eim: setProperty("gotDocuments", 0)

for i = 280010000, 280011006 do
    eim: getInstanceMap(i): resetPQ(level)
end

respawnStages(eim)
eim: startEventTimer(eventTime * 60000)
setEventRewards(eim)
setEventExclusives(eim)

return eim
end

function afterSetup(eim) end

function respawnStages(eim) end

function playerEntry(eim, player)
    local map = eim: getMapInstance(entryMap)
player: changeMap(map, map: getPortal(0))
end

function scheduledTimeout(eim)
endEvent(eim)
end

function playerUnregistered(eim, player) end

function playerExit(eim, player)
eim: unregisterPlayer(player)
player: changeMap(exitMap, 0)
end

function playerLeft(eim, player)
if not eim: isEventCleared() then
playerExit(eim, player)
end
end

function changedMap(eim, player, mapid)
if mapid < minMapId or mapid > maxMapId then
if eim: isEventTeamLackingNow(true, minPlayers, player) then
eim: unregisterPlayer(player)
endEvent(eim)
        else
eim: unregisterPlayer(player)
end
end
end

function changedLeader(eim, leader)
    local mapid = leader: getMapId()
if not eim: isEventCleared() and(mapid < minMapId or mapid > maxMapId) then
endEvent(eim)
end
end

function playerDead(eim, player) end

function playerRevive(eim, player)
if eim: isEventTeamLackingNow(true, minPlayers, player) then
eim: unregisterPlayer(player)
endEvent(eim)
    else
eim: unregisterPlayer(player)
end
end

function playerDisconnected(eim, player)
if eim: isEventTeamLackingNow(true, minPlayers, player) then
eim: unregisterPlayer(player)
endEvent(eim)
    else
eim: unregisterPlayer(player)
end
end

function leftParty(eim, player)
if eim: isEventTeamLackingNow(false, minPlayers, player) then
endEvent(eim)
    else
playerLeft(eim, player)
end
end

function disbandParty(eim)
if not eim: isEventCleared() then
endEvent(eim)
end
end

function monsterValue(eim, mobId)
return 1
end

function endEvent(eim)
    local party = eim: getPlayers()
for i = 0, party: size() - 1 do
    playerExit(eim, party: get(i))
    end
    eim: dispose()
end

function giveRandomEventReward(eim, player)
eim: giveEventReward(player)
end

function clearPQ(eim)
eim: stopEventTimer()
eim: setEventCleared()
end

function monsterKilled(mob, eim) end

function allMonstersDead(eim) end

function cancelSchedule() end

function dispose(eim) end
