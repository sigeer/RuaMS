// 天空之城 -> 阿里安特
const ticket = 4031576;
const targetMapId = 200000152;

function start() {
    if (cm.haveItem(ticket)) {
        const em = cm.getEventManager("Genie");
        if (em.getProperty("entry") == "true") {
            cm.sendYesNoLevel("No", "Yes", `这将不是一次短途飞行，所以你需要先处理一些事情，我建议你在登机前先做好这些。你还想要登上魔灯吗？`);
        } else {
            const next = new Date(+em.getProperty("next")).formatTime();
            cm.sendOkLevel(`这个精灵正在准备起飞。很抱歉，你将不得不等下一班车。下一班将在 ${next} 抵达。`);
        }
    } else {
        cm.sendOkLevel(`确保你有阿里安特的船票才能在这个魔灯上旅行。检查你的背包。`);
    }
}

function levelNo() {
    cm.sendOkLevel("好的，如果你改变主意，就跟我说话！");
}

function levelYes() {
    const em = cm.getEventManager("Genie");
    if (em.getProperty("entry") == "true") {
        cm.warp(targetMapId);
        cm.gainItem(ticket, -1);
        cm.dispose();
    } else {
        const next = new Date(+em.getProperty("next")).formatTime();
        cm.sendOkLevel(`这个精灵正在准备起飞。很抱歉，你得等下一班车。下一班将在 ${next} 抵达。`);
    }
}