-- High Priest John
-- Marriage NPC

local eim
local weddingEventName = "WeddingCathedral"
local cathedralWedding = true
local weddingIndoors = false
local weddingBlessingExp = WeddingManager:GetBlessExp()

function isWeddingIndoors(mapid)
    return mapid >= 680000100 and mapid <= 680000500
end

function detectPlayerItemid(player)
    for x = 4031357, 4031364 do
        if player:haveItem(x) then
            return x
        end
    end
    return -1
end

function getRingId(boxItemId)
    if boxItemId == 4031357 then return 1112803
    elseif boxItemId == 4031359 then return 1112806
    elseif boxItemId == 4031361 then return 1112807
    elseif boxItemId == 4031363 then return 1112809
    else return -1 end
end

local function isSuitedForWedding(player, equipped)
    local baseid = (player:getGender() == 0) and 1050131 or 1051150
    for i = 0, 3 do
        local id = baseid + i
        if equipped then
            if player:haveItemEquipped(id) then return true end
        else
            if player:haveItemWithId(id, true) then return true end
        end
    end
    return false
end

function getWeddingPreparationStatus(player, partner)
    if not player:haveItem(4000313) then return -3 end
    if not partner:haveItem(4000313) then return 3 end

    if not isSuitedForWedding(player, true) then return -4 end
    if not isSuitedForWedding(partner, true) then return 4 end

    local hasEngagement = false
    for x = 4031357, 4031364 do
        if player:haveItem(x) then
            hasEngagement = true
            break
        end
    end
    if not hasEngagement then return -1 end

    hasEngagement = false
    for x = 4031357, 4031364 do
        if partner:haveItem(x) then
            hasEngagement = true
            break
        end
    end
    if not hasEngagement then return -2 end

    if not player:canHold(1112803) then return 1 end
    if not partner:canHold(1112803) then return 2 end

    return 0
end

function giveCoupleBlessings(eim, player, partner)
    local blessCount = eim:gridSize()
    player:gainExp(blessCount * weddingBlessingExp)
    partner:gainExp(blessCount * weddingBlessingExp)
end

function start()
    weddingIndoors = isWeddingIndoors(cm:getMapId())
    if weddingIndoors then
        eim = cm:getEventInstance()
    end
    if not weddingIndoors then
        levelNotWeddingInDoorsMain()
    else
        levelMain()
    end
end

function levelNotWeddingInDoorsMain()
    local hasEngagement = false
    for x = 4031357, 4031364 do
        if cm:haveItem(x) then
            hasEngagement = true
            break
        end
    end

    if hasEngagement then
        local text = "你好。我能帮你什么吗？"
        local choice = { "我们准备好结婚了。" }
        for i, v in ipairs(choice) do
            text = text .. "\r\n#L" .. (i - 1) .. "##b" .. v .. "#l"
        end
        cm:sendSelectLevel("NotWeddingInDoors", text)
    else
        cm:sendOkLevel("Dispose", "嗯，今天两颗飘动的心将在爱的祝福下联合在一起！")
    end
end

function levelNotWeddingInDoors0()
    local wid = cm:getClient():getWorldServer():getRelationshipId(cm:getPlayer():getId())


    local weddingInfo = WeddingManager:GetPlayerWeddingInfoFromAll(cm:getPlayer())
    if weddingInfo == nil then
        cm:sendOkLevel("Dispose", "嗯，很抱歉，目前在这个频道没有为您预订的记录。")
        return
    end

    if cm:getClient():getChannelServer():getId() ~= weddingInfo.Channel then
        cm:sendOkLevel("Dispose", "您的婚礼将在频道" .. weddingInfo.Channel .. "举办。")
        return
    end

    local cserv = cm:getClient():getChannelServer()
    local partner = cserv.Players:getCharacterById(cm:getPlayer():getPartnerId())

    if partner == nil or cm:getMap() ~= partner:getMap() then
        cm:sendOkLevel("Dispose", "嗯，看来你的伴侣在别处……请让TA在开始仪式之前来这里。")
        return
    end

    if not cm:canHold(4000313) then
        cm:sendOkLevel("Dispose", "请确保有一个空闲的其他栏位以获取#b#t4000313##k。")
        return
    elseif not partner:canHold(4000313) then
        cm:sendOkLevel("Dispose", "请告知你的伴侣，他的其他栏必须有一个空闲的槽位才能获得#b#t4000313##k。")
        return
    elseif not isSuitedForWedding(cm:getPlayer(), false) then
        cm:sendOkLevel("Dispose", "请快速购买一件#婚礼服装#，以便参加仪式！没有它，我就不能为你举办结婚。")
        return
    elseif not isSuitedForWedding(partner, false) then
        cm:sendOkLevel("Dispose", "请让你的伴侣知道，他必须准备好一件#婚礼服装#，以便参加仪式。")
        return
    else
        cm:sendOkLevel("StartWedding", "很好，这里的准备工作也已经完成了。今天确实是个美好的日子，你们两个真的很幸运能在这样的日子结婚。让我们开始婚礼吧！")
        return
    end
end

function levelStartWedding()
    local cserv = cm:getClient():getChannelServer()

    local partner = cserv.Players:getCharacterById(cm:getPlayer():getPartnerId())
    if partner == nil or cm:getMap() ~= partner:getMap() then
        cm:sendOkLevel("Dispose", "嗯，看来你的伴侣在别处……请让TA在开始仪式之前来这里。")
        return
    end

    local wedding = WeddingManager:GetPlayerWeddingInfoFromAll(cm:getPlayer())
    if wedding == nil
        cm:sendOkLevel("Dispose", "嗯，很抱歉，没有找到您的预订记录。")
        return
    end

    if wedding.Channel ~= cserv:getId() then
        cm:sendOkLevel("Dispose", "嗯，您得去频道" .. wedding.Channel .. " 。")
        return
    end

    local em = cm:getEventManager(weddingEventName)
    if em:startInstance(cm:getPlayer().EffectMarriageId, cm:getPlayer()) then
        eim = WeddingManager:GetMarriageInstance(cm:getPlayer())
        if eim then
            eim:setIntProperty("weddingId", wedding.MarriageId)
            eim:setIntProperty("groomId", wedding.GroomId)
            eim:setIntProperty("brideId", wedding.BrideId)
            eim:setIntProperty("isPremium", wedding.IsPremium)
            eim:registerPlayer(partner)
        else
            cm:sendOkLevel("Dispose","定位婚礼活动时发生了意外错误。请稍后再试。")
            return
        end
    else
        cm:sendOkLevel("Dispose","婚礼准备之前发生了意外错误。请稍后再试。")
        return
    end
end


function levelMain()
    if not eim then
        cm:warp(680000000, 0)
        cm:dispose()
        return
    end

    local playerId = cm:getPlayer():getId()
    local wstg = eim:getIntProperty("weddingStage")

    if playerId == eim:getIntProperty("groomId") or playerId == eim:getIntProperty("brideId") then
        if wstg == 2 then
            cm:sendYesNoLevel("Dispose", "ConfirmMarry", "非常好，客人们现在已经把所有的祝福都赐予了你。时机已经成熟，#r我应该让你们成为夫妻了吗#k？")
            return
        elseif wstg == 1 then
            cm:sendOkLevel("Dispose", "当你们两个向彼此宣誓结婚的时候，你们的客人正在向你们祝福。这是你们两个的幸福时刻，请享受这个仪式。")
            return
        else
            cm:sendOkLevel("Dispose", "恭喜你的婚礼！我们的仪式已经结束，你现在可以去找 #b#p9201007##k，她会带领你和你的客人去参加婚礼后的派对。为你们的爱干杯！")
            return
        end
    else
        if wstg == 1 then
            if eim:gridCheck(cm:getPlayer()) ~= -1 then
                cm:sendOkLevel("Dispose", "大家给这对可爱的夫妇送上祝福吧！")
                return
            elseif eim:getIntProperty("guestBlessings") == 1 then
                cm:sendYesNoLevel("Dispose", "ConfirmBless", "你想为这对夫妇祝福吗？")
                return
            else
                cm:sendOkLevel("Dispose", "今天我们在这里聚集，为了让这对活泼的夫妇重新团聚在婚姻的殿堂上！")
                return
            end
        elseif wstg == 3 then
            cm:sendOkLevel("Dispose", "这对恩爱的鸟儿现在已经结婚了。多么热闹的一天！请做好准备参加婚后派对，它应该很快就会开始。跟着新婚夫妇的步伐。")
            return
        else
            cm:sendOkLevel("Dispose", "客人的祝福时间已经结束。稍等，这对夫妇很快就会重新宣誓他们的誓言。真是一道美丽的风景！")
            return
        end
    end
end

function levelConfirmMarry( ... )
    local player = cm:getPlayer()
    local partner = cm:getMap():getCharacterById(cm:getPlayer().PartnerId)
    if partner == nil then
        cm:sendOkLevel("Dispose", "嗯，看来你的伴侣不在这里……很遗憾，如果你的伴侣不在这里，我无法完成婚礼。");
        return
    end

    local checkResult = getWeddingPreparationStatus(player, partner)

    if checkResult == -1 then
        cm:sendOkLevel("Dispose", "看来你不再拥有你和你的伴侣在订婚时共享的戒指/戒指盒了。抱歉，但这是婚礼所需要的...");
        return
    end

    if checkResult == -2 then
        cm:sendOkLevel("Dispose", "似乎你的伴侣不再拥有你们订婚时共同分享的戒指/戒指盒了。抱歉，但这是婚礼所需要的…");
        return
    end

    if checkResult == -3 then
        cm:sendOkLevel("Dispose", "看起来你没有入口处给的 #r#t4000313##k... 请找到它，没有这个物品我不能嫁给你。");
        return
    end

    if checkResult == -4 then
        cm:sendOkLevel("Dispose", "请原谅我的无礼，但服装是仪式的重要组成部分。请为婚礼适当地打扮。");
        return
    end

    if checkResult == 1 then
        cm:sendOkLevel("Dispose", "请空出一个装备栏位以获取结婚戒指，可以吗？");
        return
    end

    if checkResult == 2 then
        cm:sendOkLevel("Dispose", "请让你的伴侣知道要留出一个装备栏位来获取婚戒，好吗？");
        return
    end

    if checkResult == 3 then
        cm:sendOkLevel("Dispose", "看起来你的伴侣没有入口处给的#r#t4000313##k... 请找到它，没有这个物品我不能和你结婚。");
        return
    end

    if checkResult == 4 then
        cm:sendOkLevel("Dispose", "看来你的伴侣没有适当地穿着参加婚礼……请原谅我的无礼，但服装是仪式的重要部分。");
        return
    end

	local pid = eim:getIntProperty("confirmedVows")
    if pid == -1 then
       eim:setIntProperty("confirmedVows", cm:getPlayer().Id)
       cm:getMap():dropMessage(6, "Wedding Assistant: " + cm:getPlayer().Name + " has confirmed vows! Alright, one step away to make it official. Tighten your seatbelts!")
       return
    end

    if pid == cm:getPlayer().Id then
       cm:sendOkLevel("Dispose", "你已经确认了你的誓言。现在只剩下你的伴侣需要确认了。")
       return
    end

    eim:setIntProperty("weddingStage", 3)
    local cmPartner = partner:getAbstractPlayerInteraction()

    local playerItemId = detectPlayerItemid(player)
    local partnerItemId = (playerItemId % 2 == 1) and playerItemId + 1 or playerItemId - 1

    local marriageRingId = getRingId((playerItemId % 2 == 1) and playerItemId or partnerItemId)

    cm:gainItem(playerItemId, -1)
    cmPartner:gainItem(partnerItemId, -1)
    WeddingManager:CompleteWedding(player, partner, marriageRingId)
    player:setMarriageItemId(marriageRingId)
    partner:setMarriageItemId(marriageRingId)

    --var marriageId = eim.getIntProperty("weddingId")
    --player.sendPacket(Wedding.OnMarriageResult(marriageId, player, true))
    --partner.sendPacket(Wedding.OnMarriageResult(marriageId, player, true))

    giveCoupleBlessings(eim, player, partner)

    cm:getMap():dropMessage(6, "High Priest John: By the power vested in me through the mighty Maple tree, I now pronounce you  Husband and Wife. You may kiss the bride!")
    eim:schedule("showMarriedMsg", 2 * 1000)
end


function levelConfirmBless( ... )
    eim:gridInsert(cm:getPlayer(), 1)
    if WeddingManager:IsBlesserShowFX() then
        local target = cm:getPlayer()
        target:sendPacket(PacketCreator.showSpecialEffect(9))
        target:getMap():broadcastMessage(target, PacketCreator:showForeignEffect(target.Id, 9), false)
    else
        local groom = eim:getPlayerById(eim:getIntProperty("groomId"))
        groom:sendPacket(PacketCreator.showSpecialEffect(9))
        groom:getMap():broadcastMessage(target, PacketCreator:showForeignEffect(groom.Id, 9), false)

        local bride = eim:getPlayerById(eim:getIntProperty("brideId"))
        bride:sendPacket(PacketCreator.showSpecialEffect(9))
        bride:getMap():broadcastMessage(bride, PacketCreator:showForeignEffect(bride.Id, 9), false)
    end

    cm:sendOkLevel("Dispose", "你的祝福已经融入了他们的爱。对于这对可爱的夫妇来说，这是多么高尚的行为！")
end
