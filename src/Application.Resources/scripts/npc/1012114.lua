function start( ... )
	-- body
    levelTalk()
end

function levelTalk( ... )
	if cm:isEventLeader() then
        cm:sendSelectLevel(cm:GetTalkMessage("HenesysPQ_TutorialTalk0"))
    else
        cm:sendSelectLevel(cm:GetTalkMessage("HenesysPQ_TutorialTalk1"))
    end

end

function level0( ... )
	cm:sendNextLevel("Tutorial1", cm:GetTalkMessage("HenesysPQ_Tutorial0"))
end

function level1( ... )
    if cm:haveItem(4001101, 10)) then
        cm:sendNextLevel("ExitComplete", cm:GetTalkMessage("HenesysPQ_CommitTask_Success"))
    else
        cm:sendOkLevel(cm:GetTalkMessage("HenesysPQ_CommitTask_Fail"))
    end
end

function level2( ... )
	 cm:sendYesNoLevel("ExitNo", "ExitYes", cm:GetTalkMessage("AreYouReturning"))
end

function levelExitYes( ... )
    cm:warp(910010300)
end

function levelExitNo( ... )
    cm:sendOkLevel(cm:GetTalkMessage("HenesysPQ_TutorialTalk2"))
end

function levelExitComplete( ... )
    cm:gainItem(4001101, -10)

    local eim = cm:getEventInstance()
    clearStage(1, eim)

    local map = eim:getMapInstance(cm:getPlayer():getMapId())
    map:killAllMonstersNotFriendly()

    eim:clearPQ()
    cm:dispose()
end

function levelTutorial1( ... )
	cm:sendNextPrevLevel("0", "Tutorial2", cm:GetTalkMessage("HenesysPQ_Tutorial1"))
end

function levelTutorial2( ... )
	cm:sendNextPrev("Tutorial1", "Tutorial3", cm:GetTalkMessage("HenesysPQ_Tutorial2"))
end

function levelTutorial3( ... )
	cm:sendNextPrev("Tutorial2", "dispose", cm:GetTalkMessage("HenesysPQ_Tutorial3"))
end



function clearStage(stage, eim)
    eim:setProperty(stage .. "stageclear", "true")
    eim:showClearEffect(true)

    eim:giveEventPlayersStageReward(stage)
end
