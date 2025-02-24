// 魔法密林 -> 天空之城
const ticket = 4031045;
const target = "天空之城";
const targetMapId = 101000301;

function start() {
    if (cm.haveItem(ticket)) {
        const em = cm.getEventManager("Boats");
        if (em.getProperty("entry") == "true") {
            cm.sendYesNoLevel("No", "Yes", `你想去${target}吗？`);
        } else {
            const next = new Date(+em.getProperty("next")).formatTime();
            cm.sendOkLevel(`飞往${target}的船只已经启程，请耐心等待下一班。下一班将在 ${next} 抵达。`);
        }
    } else {
        cm.sendOkLevel(`确保你有一张飞往${target}的船票才能乘坐这艘船。检查你的物品栏。`);
    }
}

function levelNo() {
    cm.sendOkLevel("好的，如果你改变主意，就跟我说话！");
}

function levelYes() {
    const em = cm.getEventManager("Boats");
    if (em.getProperty("entry") == "true") {
        cm.warp(targetMapId);
        cm.gainItem(ticket, -1);
        cm.dispose();
    } else {
        const next = new Date(+em.getProperty("next")).formatTime();
        cm.sendOkLevel(`飞往${target}的船只已经启程，请耐心等待下一班。下一班将在 ${next} 抵达。`);
    }
}