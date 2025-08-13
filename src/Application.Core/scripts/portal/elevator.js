function enter(pi) {
    try {
        var elevator = pi.getEventManager("Elevator");
        if (elevator == null) {
            pi.getPlayer().dropMessage(5, "电梯正在维护。");
            return false
        }

        const elevatorMap = elevator.getIntProperty("current")
        const currentMapId = pi.getMapId();
        if (currentMapId == elevatorMap) {
            pi.playPortalSound();
            pi.warp(elevatorMap + 10, 0);
            return true;
        }

        pi.getPlayer().dropMessage(5, "电梯已经启动。");
        return false;
    } catch (e) {
        pi.getPlayer().dropMessage(5, "电梯故障：" + e);
    }
    return false;
}