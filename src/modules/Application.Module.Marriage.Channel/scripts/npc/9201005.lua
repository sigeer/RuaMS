local cathedralWedding = true;
local weddingEventName = "WeddingCathedral";
local weddingEntryTicketCommon = 5251000;
local weddingEntryTicketPremium = 5251003;
local weddingSendTicket = 4031395;
local weddingGuestTicket = 4031407;
local weddingAltarMapid = 680000210;

function start( ... )
	-- body
    levelMain()
end

function levelMain()
	-- body
	local text = "欢迎来到 #b大教堂#k！ 有什么我可以帮到你？"
	local choice = {
		"如何举办婚礼？",
		"我有了订婚戒指，需要举办一场婚礼",
		"我受邀来参加一场婚礼"
	}

	for i = 1, #choice do
		text = text .. "\r\n#L" .. (i - 1) .. "##b" .. choice[i] .. "#l"
	end

	if cm:haveItem(5251100) then
        text = text .. "\r\n#L3##b制作更多的邀请函#l";
    end

	cm:sendSelectLevel(text);
end

function level0()
	cm:sendOkLevel("首先，你需要和某人订婚：找 #p9201000# 制作订婚戒指。一旦获得订婚状态，购买一张#b#t" .. weddingEntryTicketCommon .. "##k。\r\n给我看你的订婚戒指和婚礼门票，我会为你预订并提供#r15张婚礼门票#k。使用它们邀请你的婚礼客人。他们每人需要一张门票才能进入。");
end

function level1()
    local hasCommon = cm:haveItem(weddingEntryTicketCommon)
    local hasPremium = cm:haveItem(weddingEntryTicketPremium)
	if not hasPremium and not hasCommon then
		cm:sendOkLevel("Dispose", "预订失败，请确保您的背包里有一张#b#t" + weddingEntryTicketCommon + "##k。")
		return
	end

	local weddingInfo = WeddingManager:GetPlayerWeddingInfoFromAll(cm:getPlayer())
    if weddingInfo ~= nil then
        if cm.getClient():getChannelServer():getId() ~= weddingInfo.Channel then
            cm:sendOkLevel("Dispose", "你的婚礼将在 #r频道" + weddingInfo.Channel + "#k 举办。记得穿正装，不要迟到！")
            return
        end

        cm:sendOkLevel("Dispose", "你的婚礼将在 #r" + formatTimeFromMillis(weddingInfo.StartTime) + "#k 举办。记得穿正装，不要迟到！")
        return
    end

    local partnerId = cm:getPlayer().Id == wedding.GroomId and wedding.BrideId or wedding.GroomId
	local cserv = cm.getClient():getChannelServer()
	local partner = cserv.Players:getCharacterById(partnerId)
	if partner == nil then
		cm:sendOkLevel("你的伴侣似乎不在这里... 确保在时机成熟时把两个人都召集到这里！")
		return
	end

	if (partner == nil or cm.getMap() ~= partner:getMap()) then
        cm:sendOkLevel("请让您的伴侣也来这里。")
        return
    end

    if (WeddingManager:HasWeddingRing(cm:getPlayer()) or WeddingManager:HasWeddingRing(partner)) then
        cm:sendOkLevel("你或者你的伴侣已经有了结婚戒指。")
        return
    end

    if (not cm:canHold(weddingSendTicket, 15) or not partner:canHold(weddingSendTicket, 15)) then
        cm:sendOkLevel("你或者你伴侣的其他栏没有足够的空间来放置婚礼邀请函！")
		return
    end

    if (WeddingManager:GetUnclaimedMarriageGifts(cm:getPlayer()).Count > 0 or WeddingManager:GetUnclaimedMarriageGifts(partner).Count > 0)) then
        cm:sendOkLevel("呃...抱歉，根据阿莫利亚婚礼礼品登记簿的记录，似乎有些不对劲。请向 #b#p9201014##k 了解情况。")
        return
    end

    if not WeddingManager:HasEngagement(cm:getPlayer()) then
        cm:sendOkLevel("你没有订婚戒指。")
        return
    end

	local res = WeddingManager:ReserveWedding(cm:getPlayer(), cathedralWedding, hasPremium)
	if res.Code == 0 then
		cm:gainItem(hasPremium and weddingEntryTicketPremium or weddingEntryTicketCommon, -1);
		cm:GainItem(weddingSendTicket, 15, false, true, res.StartTime)
		partner:GainItem(weddingSendTicket, 15, false, true, res.StartTime)

		local wedType = weddingType and "特别" or ""
		local noticeMsg = "你们的#b" .. wedType .. " 婚礼#k 登记完成。你们有 #r30分钟#k 的时间邀请你们的好友，或者立即开始婚礼。婚礼开始后将无法邀请，记得穿得正式一点，不要迟到！"
		cm:sendOkLevel(noticeMsg)

        player:dropMessage(6, "Wedding Assistant: " .. noticeMsg)
        partner:dropMessage(6, "Wedding Assistant: " .. noticeMsg)

        if (not hasSuitForWedding(player)) then
            player:dropMessage(5, "Wedding Assistant: Please purchase a wedding garment before showing up for the ceremony. One can be bought at the Wedding Shop left-most Amoria.")
        end

        if (not hasSuitForWedding(partner)) then
            partner:dropMessage(5, "Wedding Assistant: Please purchase a wedding garment before showing up for the ceremony. One can be bought at the Wedding Shop left-most Amoria.")
        end
	end
end

-- 参加婚礼
function level2()
    local allInvitation = WeddingManager:GetWeddingMasterByGuestTicket(cm:getPlayer(), cathedralWedding)
    if allInvitation.Count == 0 then
        cm:sendOkLevel("Dispose", "你没有#b#t婚礼宾客券##k。");
    else if allInvitation.Count == 1 then
        guestSelectWedding(cm:getPlayer(), allInvitation[0].Id)
    end
    else
        local msg = "你要参加谁的婚礼？"
	    for i = 0, allInvitation.Count - 1 do
		    text = text .. "\r\n#L" .. i .. "##b" ..  allInvitation[i].GroomName .. " 与 " .. allInvitation[i].BrideName .. "#l"
            if cm.getClient():getChannelServer():getId() ~= allInvitation[i].Channel then
                text = text .. "（频道" .. allInvitation[i].Channel .."）"
            end
	    end
        cm:SetContextData(allInvitation)
        cm:sendNextSelectLevel("SelectWedding", msg)
    end
end

-- 选择谁的婚礼
function levelSelectWedding(idx)
    local selected = cm:GetContextData()[idx]
    if (selected == nil) then
        cm:sendOkLevel("Dispose", "婚礼已经结束")
        return
    end

    guestSelectWedding(cm:getPlayer(), selected.Id)
end

function guestSelectWedding(guest, weddingId)
    local wedding = WeddingManager:FindChannelWedding(cm.getClient():getChannelServer(), selected.Id)
    if wedding == nil then
        cm:sendOkLevel("Dispose", "婚礼不在当前频道举办，或者已经结束")
        return
    end

    local eim = getMarriageInstance(wid);
    if (eim != nil) then
        cm:sendOkLevel("GuestJoinWedding", "享受婚礼。不要掉落你的金枫叶，否则你将无法完成整个婚礼。");
    else
        cm:sendOkLevel("Dispose", "请稍等片刻，当这对夫妇准备好进入大教堂时。");
    end
end

function levelGuestJoinWedding()
    if (eim != nil) then
        cm:gainItem(weddingGuestTicket, -1);
        eim.registerPlayer(cm.getPlayer());
    else
        cm.sendOkLevel("Dispose", "婚礼活动未找到。");
    end
end


function hasSuitForWedding(player)
    local baseid = player:getGender() == 0 and 1050131 or 1051150

    for i = 0, 3 do
        if player:haveItemWithId(baseid + i, true) then
            return true
        end
    end

    return false
end

function formatTimeFromMillis(millis)
    -- 将毫秒转换为秒（os.date 需要的是秒）
    local seconds = math.floor(millis / 1000)
    
    -- 使用 os.date 转换为表
    local timeTable = os.date("*t", seconds)

    -- 格式化为字符串：X年X月X日X时X分
    return string.format("%d年%d月%d日%d时%d分",
        timeTable.year, timeTable.month, timeTable.day,
        timeTable.hour, timeTable.min)
end
