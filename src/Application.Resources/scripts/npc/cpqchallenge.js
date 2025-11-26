/**
 -- Version Info -----------------------------------------------------------------------------------
 1.0 - First Version by Drago (MapleStorySA)
 2.0 - Second Version by Jayd - translated CPQ contents to English
 ---------------------------------------------------------------------------------------------------
 **/

function start() {
    const eim = cm.getEventInstance();
    var teamMembers = eim.Team1.getEligibleMembers();
    var snd = "";
    for (var i = 0; i < teamMembers.Count; i++) {
        snd += `#b${cm.GetClientMessage("Name")}: ${teamMembers[i].Name} / (${cm.GetClientMessage("Level")}: ${teamMembers[i].Level}) / ${cm.GetJobName(teamMembers[i].JobModel)}#k\r\n\r\n`;
    }
    cm.sendAcceptDeclineLevel("Dispose", "Accept", snd + "你想在怪物嘉年华上和这个队伍战斗吗？");
}
function levelDispose() {
    cm.getEventInstance().AcceptChallenge(false);
}

function levelAccept() {
    cm.getEventInstance().AcceptChallenge(true);
}