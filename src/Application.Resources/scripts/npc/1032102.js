/*
        @author RMZero213 (RaGEZONE)
	Just keep this header here and don't claim that you made it.
*/

/*
	1032102.js
	Mar the Fairy
	Dragon Evolver
*/

var status;

function start() {
    status = -1;
    action(1, 0, 0);
}

function action(mode, type, selection) {
    if (mode == -1) {
        cm.dispose();
    } else {
        if (mode == 0 && type > 0) {
            cm.sendOk("好的，下次见。");
            cm.dispose();
            return;
        }
        if (mode == 1) {
            status++;
        } else {
            status--;
        }

        if (status == 0) {
            cm.sendYesNo("我是仙女玛尔。如果你有一只等级达到15级或更高的龙和一块进化之石，我可以帮你进化你的龙。如果你幸运的话，甚至可能会得到一只黑色的龙！你想让我这样做吗？");
        } else if (status == 1) {
            if (cm.haveItem(5000028, 1)) {
                cm.gainItem(5000028, -1);
                cm.gainItem(5000029, 1);
                cm.sendOk("我不知道你是怎么得到那个蛋的，但显然它已经孵化了！");
                cm.dispose();
            } else if (cm.getPlayer().getPet(0) == null) {
                cm.sendOk("确保你的宠物装备在第一个槽位上。");
                cm.dispose();
            } else if (cm.getPlayer().getPet(0).getItemId() < 5000029 || cm.getPlayer().getPet(0).getItemId() > 5000033 || !cm.haveItem(5380000, 1)) {
                cm.sendOk("你不符合要求。你需要 #i5380000##t5380000#，以及 #d#i5000029##t5000029##k, #g#i5000030##t5000030##k, #r#i5000031##t5000031##k, #b#i5000032##t5000032##k, 或者 #e#i5000033##t5000033##n 中的任意一个装备在槽位1上。请在符合条件后再来。");
                cm.dispose();
            } else if (cm.getPlayer().getPet(0).getLevel() < 15) {
                cm.sendOk("你的宠物必须达到15级或以上才能进化。");
                cm.dispose();
            } else if (cm.haveItem(5000029, 2) || cm.haveItem(5000030, 2) || cm.haveItem(5000031, 2) || cm.haveItem(5000032, 2) || cm.haveItem(5000033, 2)) {
                cm.sendSimple("你有一只还没有出来的龙，还有一只已经出来的龙。我可以帮你移除其中一只。请记住，我移除的龙的数据将会丢失。\r\n#r#L0#移除我的现金第一个槽位。#l#k\r\n#b#L1#移除我背包中的第一只龙。#l#k\r\n#g#L2#不用了，谢谢。#l#k");
            } else {
                var id = 5000029;
                var petSlot = qm.getPlayer().getPetIndexByItemId(id);
                if (petSlot == -1) {
                    cm.sendOk("你要么没有准备好进化的宠物龙，要么缺少#b#t5380000##k。");
                    cm.dispose();
                    return;
                }

                var after = cm.evolvePet(petSlot);
                if (after != null) {
                    cm.gainItem(5380000, -1);

                    cm.sendOk("你的龙现在进化了！它以前是一个 #i" + id + "# #t" + id + "#，现在它是一个 #i" + after.getItemId() + "# #t" + after.getItemId() + "#！");
                }
                cm.dispose();
            }
        } else if (status == 2) {
            if (selection == 0) {
                cm.RemoveFirstSlot(InventoryType.CASH);
                cm.sendOk("您的现金第一个槽位已被移除。");
            } else if (selection == 1) {
                if (cm.haveItem(5000029, 2)) {
                    cm.gainItem(5000029, -1);
                } else if (cm.haveItem(5000030, 2)) {
                    cm.gainItem(5000030, -1);
                } else if (cm.haveItem(5000031, 2)) {
                    cm.gainItem(5000031, -1);
                } else if (cm.haveItem(5000032, 2)) {
                    cm.gainItem(5000032, -1);
                } else if (cm.haveItem(5000033, 2)) {
                    cm.gainItem(5000033, -1);
                }
                cm.sendOk("你的库存中的第一只龙被移除。");
            } else if (selection == 2) {
                cm.sendOk("好的，下次再来。");
            }
            cm.dispose();
        }
    }
}