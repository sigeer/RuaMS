-- 月妙任务入口

local em

function start( ... )
    levelStart()
end

function levelStart( ... )
    if cm:getMapId() == 100000200 then
        em = cm:getEventManager("HenesysPQ")
        if em == nil then
            cm:sendOkLevel(cm:GetTalkMessage("HenesysPQ_Error"))
            return
        end

        if  cm:isUsingOldPqNpcStyle() then
            level0()
            return
        end

        cm:sendSelectLevel(
            cm:GetTalkMessage("HenesysPQ_Description", em:GetRequirementDescription(cm:getClient())) .. 
            "#L0#" .. cm:GetTalkMessage("PartyQuest_Participate") .. "\r\n#l" .. 
            "#L1#" .. cm:GetTalkMessage("PartyQuest_Intro") .. "\r\n#l" .. 
            "#L2#" .. cm:GetTalkMessage("HenesysPQ_Redeem") .. "\r\n#l")

    elseif cm:getMapId() == 910010100 then
        cm:sendYesNoLevel("dispose", "ExitYes", cm:GetTalkMessage("HenesysPQ_Complete"))
    elseif cm:getMapId() == 910010400 then
        cm:sendYesNoLevel("dispose", "ExitYes", cm:GetTalkMessage("AreYouReturningMap", cm:GetTalkMessage("Henesys")))
    end
end

function level0( ... )
    if cm:getParty() == nil then
        cm:sendOkLevel(cm:GetTalkMessage("HenesysPQ_EnterTalk1"))
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


function level1(...)
    cm:sendOkLevel(cm:GetTalkMessage("HenesysPQ_Intro"))
end

function level2()
    if cm:hasItem(4001101, 20) then
        if cm:canHold(1002798) then
            cm:gainItem(4001101, -20)
            cm:gainItem(1002798, 1)
            cm:sendNextLevel(cm:GetTalkMessage("Redeem_Success"))
        end
    else
        cm:sendNextLevel(cm:GetTalkMessage("Redeem_NotEnough", "#t4001101#"))
    end
end

function levelExitYes() 
    if cm:getEventInstance() == nil then
        cm:warp(100000200)
        return
    end
    if (cm:getEventInstance():giveEventReward(cm:getPlayer())) then
        cm:warp(100000200)
    else
        cm:sendOkLevel(cm:GetTalkMessage("Redeem_InventoryFull"))
    end
end
