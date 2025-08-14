const ticket = 4031331;
const targetMapId = 200000132;

function start() {
    if (cm.haveItem(ticket)) {
        const em = cm.getEventManager("Cabin");
        if (em.getProperty("entry") == "true") {
            cm.sendYesNoLevel("No", "Yes", `你希望登上这班航班吗？`);
        } else {
            const next = new Date(+em.getProperty("next")).formatTime();
            cm.sendOkLevel(`飞机还没有到达。请尽快回来。下一班将在 ${next} 抵达。`);
        }
    } else {
        cm.sendOkLevel(`确保你有一张飞往神木村的机票。检查你的背包。`);
    }
}

function levelNo() {
    cm.sendOkLevel("好的，如果你改变主意，就跟我说话！");
}

function levelYes() {
    const em = cm.getEventManager("Cabin");
    if (em.getProperty("entry") == "true") {
        cm.warp(targetMapId);
        cm.gainItem(ticket, -1);
        cm.dispose();
    } else {
        const next = new Date(+em.getProperty("next")).formatTime();
        cm.sendOkLevel(`飞机还没有到达。请尽快回来。下一班将在 ${next} 抵达。`);
    }
}
