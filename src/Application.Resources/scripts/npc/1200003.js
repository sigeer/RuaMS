/* Dawnveil
    To Rien
    Puro
    Made by Daenerys
*/
var status = 0;

function start() {
    status = -1;
    action(1, 0, 0);
}

function action(mode, type, selection) {
    if (mode === -1) {
        cm.dispose();
        return;
    }
    if (status >= 0 && mode == 0) {
        cm.sendOk("恩... 我猜你还有想在这做的事？");
        cm.dispose();
        return;
    }
    if (mode == 1)
        status++;
    else
        status--;

    if (status == 0) {
        cm.sendYesNo("搭上了这艘船，你可以前往更大的大陆冒险。 只要給我 #e80 金币#n，我会帶你去 #b金银岛#k 你想要去金银岛吗？");
    } else if (status == 1) {
        if (cm.haveItem(4032338)) {
            cm.sendNextPrev("既然你有推荐信，我不会收你任何的费用。收起來，我们前往金银岛，坐好，旅途中可能会有点动荡！");
        } else {
            if (cm.getLevel() >= 8) {
                if (cm.getMeso() < 80) {
                    cm.sendOk("什么？你说你想搭免费的船？ 你真是个怪人！");
                    cm.dispose();
                } else {
                    cm.sendNext("哇! #e80#n 金币我收到了！ 好，准备触发去明珠港喽！");
                }
            } else {
                cm.sendOk("让我看看... 我觉得你还不够强。 你至少要达到8级我才能让你到明珠港哦。");
                cm.dispose();
            }
        }
    } else if (status == 2) {
        var em = cm.getEventManager("Whale");
        if (!em.startInstance(cm.getPlayer())) {
            cm.sendOk("呃...我们目前接受的冒险岛玩家请求太多了...请稍后再试。");
            cm.dispose();
            return;
        }
        if (cm.haveItem(4032338)) {
            cm.dispose();
        } else {
            cm.gainMeso(-80);
            cm.dispose();
        }
    }
}