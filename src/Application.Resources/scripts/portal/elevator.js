function enter(pi) {
    try {
        var elevator = pi.getEventManager("Elevator");
        if (elevator == null) {
            pi.getPlayer().dropMessage(5, "The elevator is under maintenance.");
            return false
        }

        const currentMapId = pi.getMapId();
        if (currentMapId == elevator.getIntProperty("current") && elevator.getProperty("isMoving") === "false") {
            pi.playPortalSound();
            pi.warp(currentMapId == 222020100 ? 222020110 : 222020210, 0);
            return true;
        }

        pi.getPlayer().dropMessage(5, "The elevator is currently moving.");
        return false;
    } catch (e) {
        pi.getPlayer().dropMessage(5, "Error: " + e);
    }
    return false;
}