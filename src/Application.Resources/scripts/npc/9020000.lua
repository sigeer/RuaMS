-- 拉克里斯 废弃都市组队任务入口NPC、放弃任务

local em

function start( ... )
	if cm:getMapId() >= 103000800 and cm:getMapId() <= 103000805 then
        cm:sendYesNoLevel("dispose", "ExitYes", cm:GetTalkMessage("KerningPQ_Abandon"))
    else
        levelMain()
    end
end

function levelMain()
    em = cm:getEventManager("KerningPQ")
    if em == nil then
        cm:sendOkLevel(cm:GetTalkMessage("KerningPQ_Error"))
        return
    end

    if  cm:isUsingOldPqNpcStyle() then
        level0()
        return
    end

    cm:sendSelectLevel(
        cm:GetTalkMessage("KerningPQ_Description", em:GetRequirementDescription(cm:getClient())) .. 
        "#L0#" .. cm:GetTalkMessage("PartyQuest_Participate") .. "\r\n#l" .. 
        "#L1#" .. cm:GetTalkMessage("PartyQuest_Intro") .. "\r\n#l")
end

function level0( ... )
    if cm:getParty() == nil then
        cm:sendOkLevel(cm:GetTalkMessage("PartyQuest_CannotStart_Party"))
    elseif (not cm:isLeader()) then
        cm:sendOkLevel(cm:GetTalkMessage("PartyQuest_NeedLeaderTalk"))
    else
        local eli = em:getEligibleParty(cm:getParty())
        if (eli.Count > 0) then
            if (not em:startInstance(cm:getParty(), cm:getPlayer():getMap(), 1)) then
                cm:sendOkLevel(cm:GetTalkMessage("PartyQuest_CannotStart_ChannelFull"))
            end
        else
            cm:sendOkLevel(cm:GetTalkMessage("PartyQuest_CannotStart_Req"))
        end
    end
end

function level1( ... )
    cm:sendOkLevel(cm:GetTalkMessage("KerningPQ_Intro"))
end

function levelExitYes() 
    cm:warp(103000000)
end