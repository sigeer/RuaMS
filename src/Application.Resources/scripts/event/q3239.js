var name = "q3239";
var eventType = "Solo";
var entryMap = 922000000;
var exitMap = 922000009;
var eventLength = 20;
const maxLobbies = 7;

function init() {
}

function getMaxLobbies() {
    return maxLobbies;
}

function setup(level, lobbyid) {
    var eim = em.newInstance("q3239_" + lobbyid);
    eim.setExclusiveItems([4031092]);
    return eim;
}

function playerEntry(eim, player) {
    var im = eim.getInstanceMap(entryMap);

    // Reset instance
    im.clearDrops();
    im.resetReactors();
    im.shuffleReactors();

    // Start timer
    eim.startEventTimer(eventLength * 60 * 1000);

    // Warp player and mark event as occupied
    player.changeMap(im, 0);
}

function changedMap(eim, player, mapid) {
    if (mapid != entryMap)
        playerExit(eim, player);
}

function playerExit(eim, player) {
    end(eim);
}

function playerDisconnected(eim, player) {
    end(eim);
}

function scheduledTimeout(eim) {
    end(eim);
}

function end(eim) {
    var party = eim.getPlayers(); // should only ever be one player
    for (var i = 0; i < party.Count; i++) {
        var player = party[i];
        eim.unregisterPlayer(player);
        player.changeMap(exitMap);
    }

    eim.dispose();
}

// Stub/filler functions

function disbandParty(eim, player) {}
function afterSetup(eim) {}
function playerUnregistered(eim, player) {}
function changedLeader(eim, leader) {}
function leftParty(eim, player) {}
function clearPQ(eim) {}
function dispose() {}
function cancelSchedule() {}
function allMonstersDead(eim) {}
function monsterValue(eim, mobId) {}
function monsterKilled(mob, eim) {}
