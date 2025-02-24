// 天空之城 -> 玩具城
const ticket = 4031074;
const target = "玩具城";
const targetMapId = 200000122;

function start() {
    if (cm.haveItem(ticket)) {
        const em = cm.getEventManager("Trains");
        if (em.getProperty("entry") == "true") {
            cm.sendYesNoLevel("No", "Yes", `你想去${target}吗？`);
        } else {
            const next = new Date(+em.getProperty("next")).formatTime();
            cm.sendOkLevel(`前往${target}的火车已经启程，请耐心等待下一班列车。下一班将在 ${next} 抵达。`);
        }
    } else {
        cm.sendOkLevel(`确保你有一张前往${target}的车票才能乘坐这趟火车。检查你的背包。`);
    }
}

function levelNo() {
    cm.sendOkLevel("好的，如果你改变主意，就跟我说话！");
}

function levelYes() {
    const em = cm.getEventManager("Trains");
    if (em.getProperty("entry") == "true") {
        cm.warp(targetMapId);
        cm.gainItem(ticket, -1);
        cm.dispose();
    } else {
        const next = new Date(+em.getProperty("next")).formatTime();
        cm.sendOkLevel(`前往${target}的火车已经启程，请耐心等候下一班列车。下一班将在 ${next} 抵达。`);
    }
}