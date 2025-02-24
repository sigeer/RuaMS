// 购票：阿里安特 -> 天空之城
const cost = 6000;
const ticket = 4031045;

function start() {
    cm.sendYesNoLevel("No", "Yes", `你好，我负责出售前往天空之城的船票。前往天空之城的船每10分钟出发一次，从整点开始，票价为#b${cost}金币#k。你确定要购买#b#t${ticket}##k吗？`);
}

function levelNo() {
    cm.sendNextLevel("你一定是有一些事情要在这里处理，对吧？");
}

function levelYes() {
    if (cm.getMeso() >= cost && cm.canHold(ticket)) {
        cm.gainItem(ticket, 1);
        cm.gainMeso(-cost);
        cm.dispose();
    } else {
        cm.sendOkLevel("你确定你有 #b" + cost + " 冒险币#k 吗？如果是的话，请检查你的杂项物品栏，看看是否已经满了。");
    }
}
