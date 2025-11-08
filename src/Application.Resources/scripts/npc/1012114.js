function start() {
    if (cm.isEventLeader()) {
        cm.sendSelectLevel(cm.GetTalkMessage("HenesysPQ_TutorialTalk0"));
    } else {
        cm.sendSelectLevel(cm.GetTalkMessage("HenesysPQ_TutorialTalk1"));
    }
}

function level0() {
    cm.sendNextLevel("Tutorial1", cm.GetTalkMessage("HenesysPQ_Tutorial0"));
}

function levelTutorial1() {
    cm.sendNextPrevLevel("0", "Tutorial2", cm.GetTalkMessage("HenesysPQ_Tutorial1"))
}
function levelTutorial2() {
    cm.sendNextPrevLevel("Tutorial2", "Tutorial3", cm.GetTalkMessage("HenesysPQ_Tutorial2"))
}
function levelTutorial3() {
    cm.sendNextPrevLevel("Tutorial2", "dispose", cm.GetTalkMessage("HenesysPQ_Tutorial3"))
}

function level1() {
    if (cm.haveItem(4001101, 10)) {
        cm.sendNextLevel("ExitComplete", cm.GetTalkMessage("HenesysPQ_CommitTask_Success"));
    } else {
        cm.sendOkLevel(cm.GetTalkMessage("HenesysPQ_CommitTask_Fail"));
    }
}

function levelExitComplete() {
    cm.gainItem(4001101, -10);

    var eim = cm.getEventInstance();
    clearStage(1, eim);

    var map = eim.getMapInstance(cm.getPlayer().getMapId());
    map.killAllMonstersNotFriendly();

    eim.clearPQ();
    cm.dispose();
}

function level2() {
    cm.sendYesNoLevel("ExitNo", "ExitYes", cm.GetTalkMessage("AreYouReturning"));
}

function levelExitNo() {
    cm.sendOkLevel(cm.GetTalkMessage("HenesysPQ_TutorialTalk2"));
}

function levelExitYes() {
    cm.warp(910010300);
}

function clearStage(stage, eim) {
    eim.setProperty(stage + "stageclear", "true");
    eim.showClearEffect(true);

    eim.giveEventPlayersStageReward(stage);
}