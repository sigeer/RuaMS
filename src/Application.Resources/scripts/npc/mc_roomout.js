function start() {
    const eim = cm.getEventInstance();
    if (eim == null) {
        cm.WarpOut();
    } else {
        eim.Dispose();
        cm.dispose();
    }
}
