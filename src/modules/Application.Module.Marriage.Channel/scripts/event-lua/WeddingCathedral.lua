local entryMap = 680000200
local exitMap = 680000500
local recruitMap = 680000000
local clearMap = 680000500

local minMapId = 680000100
local maxMapId = 680000401

local startMsgTime = 4
local blessMsgTime = 5

local eventTime = 10    -- 10 minutes gathering
local ceremonyTime = 20 -- 20 minutes ceremony
local blessingsTime = 15-- blessings are held until the 15th minute from the ceremony start
local partyTime = 45    -- 45 minutes party

local forceHideMsgTime = 10  -- unfortunately, EIM weddings don't send wedding talk packets to the server... this will need to suffice

local eventBoss = true   -- spawns a Cake boss at the hunting ground
local isCathedral = true

local maxLobbies = 99

function init()
end

function getMaxLobbies()
    return maxLobbies
end

function setEventExclusives(eim) 
    var itemSet = [4031217, 4000313]    -- golden key, golden maple leaf
    eim:setExclusiveItems(itemSet)
end

function setEventRewards(eim) 
    var itemSet, itemQty, evLevel, expStages

    evLevel = 1    --Rewards at clear PQ
    itemSet = []
    itemQty = []
    eim:setEventRewards(evLevel, itemSet, itemQty)

    expStages = []    --bonus exp given on CLEAR stage signal
    eim:setEventClearStageExp(expStages)
end

function spawnCakeBoss(eim) 
    local mapObj = eim:getMapInstance(680000400)
    local mobObj = LifeFactory.getMonster(9400606)
    mapObj.spawnMonsterOnGroundBelow(mobObj, Point(777, -177))
end

function setup(level, lobbyid) 
    local eim = WeddingManager:CreateMarriageInstance(em, "Wedding" + lobbyid)
    eim:setProperty("weddingId", "0")
    -- 0: gathering time, 1: wedding time, 2: ready to fulfill the wedding, 3: just married
    eim:setProperty("weddingStage", "0")   
    eim:setProperty("guestBlessings", "0")
    eim:setProperty("isPremium", "1")
    eim:setProperty("canJoin", "1")
    eim:setProperty("groomId", "0")
    eim:setProperty("brideId", "0")
    eim:setProperty("confirmedVows", "-1")
    eim:setProperty("groomWishlist", "")
    eim:setProperty("brideWishlist", "")
    eim:initializeGiftItems()

    eim:getInstanceMap(680000400):resetPQ(level)
    if (eventBoss) then
        spawnCakeBoss(eim)
    end

    respawnStages(eim)
    eim:startEventTimer(eventTime * 60000)
    setEventRewards(eim)
    setEventExclusives(eim)
    return eim
end

function afterSetup(eim)
end

function respawnStages(eim) 
    eim:getMapInstance(680000400).instanceMapRespawn()
    eim:schedule("respawnStages", 15 * 1000)
end

function playerEntry(eim, player) 
    eim:setProperty("giftedItemG" + player.getId(), "0")
    eim:setProperty("giftedItemB" + player.getId(), "0")
    player.getAbstractPlayerInteraction().gainItem(4000313, 1)

    local map = eim:getMapInstance(entryMap)
    player.changeMap(map, map.getPortal(0))
end

function stopBlessings(eim) 
    local mapobj = eim:getMapInstance(entryMap + 10)
    mapobj.dropMessage(6, "Wedding Assistant: Alright people, our couple are preparing their vows to each other right now.")

    eim:setIntProperty("weddingStage", 2)
end

function sendWeddingAction(eim, type) 
    local chr = eim:getLeader()
    if (chr.getGender() == 0) then
        chr.getMap().broadcastMessage(Wedding.OnWeddingProgress(type == 2, eim:getIntProperty("groomId"), eim:getIntProperty("brideId"), type + 1))
    else
        chr.getMap().broadcastMessage(Wedding.OnWeddingProgress(type == 2, eim:getIntProperty("brideId"), eim:getIntProperty("groomId"), type + 1))
    end
end

function hidePriestMsg(eim) 
    sendWeddingAction(eim, 2)
end

function showStartMsg(eim) 
    eim:getMapInstance(entryMap + 10).broadcastMessage(Wedding.OnWeddingProgress(false, 0, 0, 0))
    eim:schedule("hidePriestMsg", forceHideMsgTime * 1000)
end

function showBlessMsg(eim) 
    eim:getMapInstance(entryMap + 10).broadcastMessage(Wedding.OnWeddingProgress(false, 0, 0, 1))
    eim:setIntProperty("guestBlessings", 1)
    eim:schedule("hidePriestMsg", forceHideMsgTime * 1000)
end

function showMarriedMsg(eim) 
    sendWeddingAction(eim, 3)
    eim:schedule("hidePriestMsg", 10 * 1000)

    eim:restartEventTimer(partyTime * 60000)
end

function scheduledTimeout(eim) 
    if (eim:getIntProperty("canJoin") == 1) then
        em.getChannelServer().closeOngoingWedding(isCathedral)
        eim:setIntProperty("canJoin", 0)

        local mapobj = eim:getMapInstance(entryMap)
        local chr = mapobj:getCharacterById(eim:getIntProperty("groomId"))
        if (chr != null) then
            chr:changeMap(entryMap + 10, "we00")
        end

        chr = mapobj:getCharacterById(eim:getIntProperty("brideId"))
        if (chr != null) then
            chr:changeMap(entryMap + 10, "we00")
        end

        mapobj:dropMessage(6, "Wedding Assistant: The couple are heading to the altar, hurry hurry talk to me to arrange your seat.")

        eim:setIntProperty("weddingStage", 1)
        eim:schedule("showStartMsg", startMsgTime * 60 * 1000)
        eim:schedule("showBlessMsg", blessMsgTime * 60 * 1000)
        eim:schedule("stopBlessings", blessingsTime * 60 * 1000)
        eim:startEventTimer(ceremonyTime * 60000)
    else
        end(eim)
    end
end

function playerUnregistered(eim, player) 
end

function playerExit(eim, player) 
    eim:unregisterPlayer(player)
    player:changeMap(exitMap, 0)
end

function playerLeft(eim, player) 
    if (!eim:isEventCleared()) then
        playerExit(eim, player)
    end
end

function isMarrying(eim, player) 
    local playerid = player.getId()
    return playerid == eim:getIntProperty("groomId") or playerid == eim:getIntProperty("brideId")
end

function changedMap(eim, player, mapid) {
    if (mapid < minMapId or mapid > maxMapId) then
        if (isMarrying(eim, player)) then
            eim:unregisterPlayer(player)
            end(eim)
        else
            eim:unregisterPlayer(player)
        end
    end
}

function changedLeader(eim, leader) 
end

function playerDead(eim, player) 
end
-- player presses ok on the death pop up.
function playerRevive(eim, player) 
    if (isMarrying(eim, player)) then
        eim:unregisterPlayer(player)
        end(eim)
    else
        eim:unregisterPlayer(player)
    end
end

function playerDisconnected(eim, player) 
    if (isMarrying(eim, player)) then
        eim:unregisterPlayer(player)
        end(eim)
    else
        eim:unregisterPlayer(player)
    end
end

function leftParty(eim, player) 
end

function disbandParty(eim)
end

function monsterValue(eim, mobId) 
    return 1
end

function end(eim) 
    local party = eim:getPlayers()

    for (var i = 0 i < party.size() i++) {
        playerExit(eim, party.get(i))
    }
    eim:dispose()
end

function giveRandomEventReward(eim, player) 
    eim:giveEventReward(player)
end

function clearPQ(eim) 
    eim:stopEventTimer()
    eim:setEventCleared()
end

function isCakeBoss(mob) 
    return mob.getId() == 9400606
end

function monsterKilled(mob, eim)
    if (isCakeBoss(mob)) then
        eim:showClearEffect()
        eim:clearPQ()
    end
end

function allMonstersDead(eim) 
end

function cancelSchedule() 
end

function dispose(eim)
end
