function init() {
    scheduleNew();
}

function scheduleNew() {
    setupTask = em.schedule("start", 0);    //spawns upon server start. Each 3 hours an server event checks if boss exists, if not spawns it instantly.
}

function cancelSchedule() {
    if (setupTask != null) {
        setupTask.cancel(true);
    }
}

function start() {
    var labSecretBasementPath = em.getChannelServer().getMapFactory().getMap(261030000);
    var chimera = LifeFactory.getMonster(8220002);

    if (labSecretBasementPath.getMonsterById(8220002) != null) {
        em.schedule("start", 3 * 60 * 60 * 1000);
        return;
    }

    var posX;
    var posY = 180;
    posX = (Math.floor(Math.random() * 900) - 900);
    const spawnpoint = new Point(posX, posY);
    labSecretBasementPath.spawnMonsterOnGroundBelow(chimera, spawnpoint);
    labSecretBasementPath.broadcastMessage(PacketCreator.serverNotice(6, "Kimera has appeared out of the darkness of the underground with a glitter in her eyes."));
    em.schedule("start", 3 * 60 * 60 * 1000);
}

// ---------- FILLER FUNCTIONS ----------

function dispose() {}

function setup(eim, leaderid) {}

function monsterValue(eim, mobid) {return 0;}

function disbandParty(eim, player) {}

function playerDisconnected(eim, player) {}

function playerEntry(eim, player) {}

function monsterKilled(mob, eim) {}

function scheduledTimeout(eim) {}

function afterSetup(eim) {}

function changedLeader(eim, leader) {}

function playerExit(eim, player) {}

function leftParty(eim, player) {}

function clearPQ(eim) {}

function allMonstersDead(eim) {}

function playerUnregistered(eim, player) {}

