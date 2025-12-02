function getInactiveReactors(map) {
    var reactors = [];

    for (var mo of map.getReactors()) {
        if (mo.getState() >= 7) {
            reactors.push(mo);
        }
    }

    return reactors;
}

function start(ms) {
    var map = ms.getMap();
    map.resetReactors(getInactiveReactors(map));

    return (true);
}