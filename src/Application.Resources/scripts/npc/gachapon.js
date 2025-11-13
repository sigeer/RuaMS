const ticketId = 5220000;
var curMapName = "";

function start() {
    curMapName = cm.GetGachaponMapName();

    cm.sendSelectLevel(`欢迎来到${curMapName}扭蛋机。我可以为您做些什么呢？\r\n\r\n
#L0#什么是扭蛋机？#l\r\n
#L1#在哪里可以购买#t${ticketId}#？#l\r\n
#L2#使用1张#t${ticketId}#。#l\r\n
#L3#使用10张#t${ticketId}#。#l\r\n
#L4#查看我的#r奖品仓库#k。#l`);
}

function level0() {
    cm.sendNextLevel("0More", `玩转扭蛋机，赢得稀有卷轴、装备、椅子、熟练书和其他酷炫物品！你只需要一张 #i${ticketId}##b#t${ticketId}##k 就有机会成为随机物品的幸运获得者。`);
}

function level0More() {
    cm.sendLastNextLevel("0", "dispose", "你会在" + curMapName + "的扭蛋机中找到各种物品，但最有可能找到与" + curMapName + "相关的物品和卷轴。");
}

function level1() {
    cm.sendNextLevel(`#i${ticketId}##b#t${ticketId}##k 可以在#r现金商店#k使用NX或枫叶点购买。点击屏幕右下角的红色商店图标访问#r现金商店#k。`);
}

function level2() {
    doGachapon(1);
}

function level3() {
    doGachapon(10);
}

function doGachapon(count) {
    if (!cm.haveItem(ticketId, count)) {
        cm.sendOkLevel(cm.GetTalkMessage("Tip_CheckItemWithId", ticketId, count));
        return;
    }
    if (!cm.CheckGachaponStorage(count)) {
        cm.sendOkLevel(cm.GetTalkMessage("Storage_CheckGachaponStorage", count));
        return;
    }

    cm.gainItem(ticketId, -count);
    for (var i = 0; i < count; i++) {
        const itemObj = cm.doGachapon();
        if (itemObj == null) {
            cm.sendOkLevel(cm.GetTalkMessage("Tip_ThankPatronage"));
            return;
        }
        cm.sendOkLevel(cm.GetTalkMessage("Tip_ObtainItem", itemObj.ItemId));
    }
}

function level4() {
    cm.dispose();
    cm.OpenGachaponStorage();
}