/**
 -- Version Info -----------------------------------------------------------------------------------
 1.0 - First Version by Drago (MapleStorySA)
 2.0 - Second Version by Jayd - translated CPQ contents to English
 ---------------------------------------------------------------------------------------------------
 **/

function start() {
    showChallengerInfo();
}
function levelDispose() {
    cm.getEventInstance().AcceptChanllege(false);
}

function showChallengerInfo() {
    const eim = cm.getEventInstance();
    var teamMembers = eim.Room.Team1.getEligibleMembers();
    var snd = "";
    for (var i = 0; i < teamMembers.Count; i++) {
        snd += "#b名称：" + teamMembers[i].Name + " / (等级：" + teamMembers[i].Level + ") / " + teamMembers[i].JobModel.Name + "#k\r\n\r\n";
    }
    cm.sendAcceptDeclineLevel("Dispose", "Accept", snd + "你想在怪物嘉年华上和这个队伍战斗吗？");
}

function levelAccept() {
    cm.getEventInstance().AcceptChanllege(true);
}