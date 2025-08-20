﻿var status = -1;
var exchangeItem = 4000438;

function start() {
    action(1, 0, 0);
}

function action(mode, type, selection) {
    if (mode == 1) {
        status++;
    } else {
        cm.dispose();
        return;
    }
    if (status == 0) {
        cm.sendSimple("这些怪物太简单了！用我的剑一击就能杀死它们……最好先弄把剑。#b\r\n#L0#嘿，拿着这些树干。你可以用它们来打造一把更好的剑。#l");
    } else if (status == 1) {
        if (!cm.haveItem(exchangeItem, 100)) {
            cm.sendNext("你没有足够的... 我至少需要100个。");
            cm.dispose();
        } else {
            cm.sendGetNumber("Hey, that's a good idea! I can give you #i4310000#Perfect Pitch for each 100 #i" + exchangeItem + "##t" + exchangeItem + "# you give me. How many do you want? (Current Items: " + cm.itemQuantity(exchangeItem) + ")", Math.min(300, cm.itemQuantity(exchangeItem) / 100), 1, Math.min(300, cm.itemQuantity(exchangeItem) / 100));
        }
    } else if (status == 2) {
        if (selection >= 1 && selection <= cm.itemQuantity(exchangeItem) / 100) {
            if (!cm.canHold(4310000, selection)) {
                cm.sendOk("请在杂项标签页中腾出一些空间。");
            } else {
                cm.gainItem(4310000, selection);
                cm.gainItem(exchangeItem, -(selection * 100));
                cm.sendOk("谢谢！");
            }
        }
        cm.dispose();
    }
}