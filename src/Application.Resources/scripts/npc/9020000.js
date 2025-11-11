/**
 * 拉克里斯 废弃都市组队任务入口NPC、放弃任务
 */

var em = null;

function start() {
    if (cm.getMapId() >= 103000800 && cm.getMapId() <= 103000805) {
        cm.sendYesNoLevel("dispose", "ExitYes", cm.GetTalkMessage("KerningPQ_Abandon"));
    } else {
        levelMain();
    }
}

function levelExitYes() {
    cm.warp(103000000);
}

function levelMain() {
    em = cm.getEventManager("KerningPQ");
    if (em == null) {
        cm.sendOkLevel(cm.GetTalkMessage("KerningPQ_Error"));
        return;
    }
    if (cm.isUsingOldPqNpcStyle()) {
        level0();
        return;
    }

    cm.sendSelectLevel(
        cm.GetTalkMessage("KerningPQ_Description", em.GetRequirementDescription(cm.getClient())) +
        "\r\n#L0#" + cm.GetTalkMessage("PartyQuest_Participate") + "#l" +
        "\r\n#L1#" + cm.GetTalkMessage("PartyQuest_Intro") + "#l");
}

function level0() {
    if (cm.getParty() == null)
        cm.sendOkLevel(cm.GetTalkMessage("PartyQuest_CannotStart_Party"));
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
    cm.sendOkLevel(cm.GetTalkMessage("KerningPQ_Intro"));
}