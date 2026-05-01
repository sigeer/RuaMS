function enter(pi) {
    try {
        var elevator = pi.GetElevator();
        if (elevator == null) {
            pi.getPlayer().dropMessage(5, "电梯正在维护。");
            return false
        }

        if (elevator.Enter(pi.getPlayer())) {
            pi.playPortalSound();
            return true;
        }

        pi.getPlayer().dropMessage(5, "电梯已经启动。");
        return false;
    } catch (e) {
        pi.getPlayer().dropMessage(5, "电梯故障：" + e);
    }
    return false;
}