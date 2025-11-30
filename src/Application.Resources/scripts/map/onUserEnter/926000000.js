function start(ms) {
    var map = ms.getMap();
    map.resetPQ(1);

    if (map.countMonster(9100013) == 0) {
        map.spawnMonsterOnGroundBelow(LifeFactory.getMonster(9100013), new Point(82, 200));
    }

    return (true);
}
