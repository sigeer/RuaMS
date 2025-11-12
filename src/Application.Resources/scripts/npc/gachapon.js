var status;
const ticketId = 5220000;
const mapNpcMapping = {
    9100100: "Map_Henesys",
    9100101: "Map_Ellinia",
    9100102: "Map_Perion",
    9100103: "Map_KerningCity",
    9100104: "Map_Sleepywood",
    9100105: "Map_MushroomShrine",
    9100106: "Map_ShowaSpaMale",
    9100107: "Map_ShowaSpaFemale",
    9100108: "Map_Ludibrium",
    9100109: "Map_NewLeafCity",
    9100110: "Map_ElNath",
    9100117: "Map_NautilusHarbor",
}
var curMapName = "";

function start() {
    curMapName = cm.GetClientMessage(mapNpcMapping[cm.getNpc()]);

    cm.sendSelectLevel(`欢迎来到${curMapName}扭蛋机。我可以为您做些什么呢？\r\n\r\n
#L0#什么是扭蛋机？#l\r\n
#L1#在哪里可以购买#t${ticketId}#？#l\r\n
#L2#使用#t${ticketId}#。#l\r\n
#L3#查看我的#r奖品仓库#k。#l`);
}

function level0() {
    cm.sendNextLevel("level0More", `玩转扭蛋机，赢得稀有卷轴、装备、椅子、熟练书和其他酷炫物品！你只需要一张 #i${ticketId}##b#t${ticketId}##k 就有机会成为随机物品的幸运获得者。`);
}

function level0More() {
    cm.sendLastNextLevel("level0", "dispose", "你会在" + curMapName + "的扭蛋机中找到各种物品，但最有可能找到与" + curMapName + "相关的物品和卷轴。");
}

function level1() {
    cm.sendNextLevel(`#i${ticketId}##b#t${ticketId}##k 可以在#r现金商店#k使用NX或枫叶点购买。点击屏幕右下角的红色商店图标访问#r现金商店#k，您可以购买门票。`);
}

function level2() {
    if (!cm.haveItem(ticketId)) {
        cm.sendOkLevel(`请确保你有一张 #i${ticketId}##b#t${ticketId}##k`);
        return;
    }
    if (!cm.CheckGachaponStorage(1)) {
        cm.sendOkLevel("请确保你的#r奖品仓库#k中至少有一个空位。");
        return;
    }

    cm.gainItem(ticketId, -1);
    const itemObj = cm.doGachapon();
    if (itemObj == null) {
        return;
    }
    cm.sendNextLevel(cm.GetTalkMessage("Tip_ObtainItem", itemObj.ItemId));
}

function level3() {
    cm.OpenGachaponStorage();
}