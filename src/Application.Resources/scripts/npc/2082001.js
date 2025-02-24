// 神木林 -> 天空之城
const ticket = 4031045;
const targetMapId = 240000111;

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
        cm.sendOkLevel(`确保你有一张飞往天空之城的机票。检查你的物品栏。`);
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