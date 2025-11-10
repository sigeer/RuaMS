function enter(pi) {
    const eim = pi.getEventInstance();
    if (eim != null) {
        eim.dropExclusiveItems(pi.getPlayer());

        var spring = pi.getMap().getReactorById(3008000);  // thanks Chloek3, seth1 for noticing fragments not being awarded properly
        if (spring != null && spring.getState() > 0) {
            if (!pi.canHold(4001198, 1)) {
                pi.Pink("Tip_CheckEtcSizeBeforeEnterPortal");
                return false;
            }

            pi.gainItem(4001198, 1);
        }
    }

    pi.playPortalSound();
    pi.warp(300030100, 0);
    return true;
}