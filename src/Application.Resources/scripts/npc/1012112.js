/**
 * 月妙任务入口，离开
 */

var em = null;

function start() {
    if (cm.getMapId() == 100000200) {
        em = cm.getEventManager("HenesysPQ");
        if (em == null) {
            cm.sendOkLevel(cm.GetTalkMessage("HenesysPQ_Error"));
            return;
        }

        if (cm.isUsingOldPqNpcStyle()) {
            level0();
            return;
        }

        cm.sendSelectLevel(
            cm.GetTalkMessage("HenesysPQ_Description", em.GetRequirementDescription(cm.getClient())) +
            "\r\n#L0#" + cm.GetTalkMessage("PartyQuest_Participate") + "#l" +
            "\r\n#L1#" + cm.GetTalkMessage("PartyQuest_Intro") + "#l" +
            "\r\n#L2#" + cm.GetTalkMessage("HenesysPQ_Redeem") + "#l");
    }
    else if (cm.getMapId() == 910010100)
        cm.sendYesNoLevel("dispose", "ExitYes", cm.GetTalkMessage("HenesysPQ_Complete"));
    else if (cm.getMapId() == 910010400)
        cm.sendYesNoLevel("dispose", "ExitYes", cm.GetTalkMessage("AreYouReturningMap", cm.GetTalkMessage("Henesys")));
}

function level0() {
    if (cm.getParty() == null)
        cm.sendOkLevel(cm.GetTalkMessage("HenesysPQ_EnterTalk1"));
    else if (!cm.isLeader())
        cm.sendOkLevel(cm.GetTalkMessage("PartyQuest_NeedLeaderTalk"));
    else {
        var eli = em.getEligibleParty(cm.getParty());
        if (eli.Count > 0) {
            if (!em.StartPQInstance(cm.getParty(), cm.getPlayer().getMap(), 1)) {
                cm.sendOkLevel(cm.GetTalkMessage("PartyQuest_CannotStart_ChannelFull"));
            }
        } else {
            cm.sendOkLevel(cm.GetTalkMessage("PartyQuest_CannotStart_Req"));
        }
    }
}


function level1() {
    cm.sendOkLevel(cm.GetTalkMessage("HenesysPQ_Intro"));
}

function level2() {
    if (cm.hasItem(4001101, 20)) {
        if (cm.canHold(1002798)) {
            cm.gainItem(4001101, -20);
            cm.gainItem(1002798, 1);
            cm.sendNextLevel(cm.GetTalkMessage("Redeem_Success"));
        }
    } else {
        cm.sendNextLevel(cm.GetTalkMessage("Redeem_NotEnough", "#t4001101#"));
    }
}

function levelExitYes() {
    if (cm.getEventInstance() == null) {
        cm.warp(100000200);
        return;
    }
    if (cm.getEventInstance().giveEventReward(cm.getPlayer())) {
        cm.warp(100000200);
        return;
    }
    cm.sendOkLevel(cm.GetTalkMessage("Redeem_InventoryFull"));
}