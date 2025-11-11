function enter(pi) {
    var eim = pi.getEventInstance();
    if (eim == null)
        return false;

    if (eim.isEventCleared()) {
        if (pi.isEventLeader()) {
            pi.playPortalSound();
            eim.warpEventTeam(930000800);
            return true;
        } else {
            pi.Pink("Tip_WaitForLeaderEnterPortal");
            return false;
        }
    } else {
        pi.Pink("EllinPQ_NeedDefeatBossFirst");
        return false;
    }
}