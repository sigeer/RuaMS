function start() {
    if (!cm.haveItem(4001163) || !cm.isEventLeader()) {
        cm.sendYesNoLevel("dispose", "Exit", "让你的队长在这里给我看#t4001163#。\r\n\r\n或者你想要#r离开这片森林#k吗？现在离开意味着抛弃你的伙伴，记住这一点。");
    } else {
        cm.sendNextLevel("Complete", "太好了，你有了#t4001163#。我会带你们去通往石头祭坛的路。跟我来吧。");
    }
}

function levelExit() {
    cm.warp(930000800, 0);
}

function levelComplete() {
    cm.getEventInstance().warpEventTeam(930000600);
}
