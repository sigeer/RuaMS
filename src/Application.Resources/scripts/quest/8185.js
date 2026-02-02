/*
    This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
               Matthias Butz <matze@odinms.de>
               Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
/* 	Author: 		Blue
    Name:	 		Garnox
    Map(s): 		New Leaf City : Town Center
    Description: 		Quest - Pet Evolution2
*/

var status = -1;

function end(mode, type, selection) {
    if (mode == -1) {
        qm.dispose();
    } else {
        if (mode == 1) {
            status++;
        } else {
            status--;
        }
        if (status == 0) {
            if (qm.getMeso() < 10000) {
                qm.sendOk("喂！我需要 #b10,000 金币#k 来进行宠物进化！");
                qm.dispose();
                return;
            }

            if (!qm.haveItem(5380000)) {
                qm.sendOk("若没有进化之石是无法让宠物进化的！难道你没听懂吗？快去拿回来吧！");
                qm.dispose();
                return;
            }

            qm.sendNext("#e#b嘿，你成功了！#n#k \r\n#r哇！#k 现在我可以完成对你的宠物的研究了！");
        } else if (status == 1) {
            if (mode == 0) {
                qm.sendOk("我明白了...当你愿意的时候再来吧。我真的很期待这个。");
                qm.dispose();
            } else {
                qm.sendNextPrev("顺便说一下，你的新龙的颜色会是 #e#r随机#k#n！可能是 #g绿色，#b蓝色，#r红色，#d或者非常罕见的#k，黑色。\r\n\r\n#fUI/UIWindow.img/QuestIcon/5/0#\r\n\r 如果你不喜欢你宠物的新颜色，或者如果你想再次改变宠物的颜色，#e你可以改变它！#n 只需 #d购买另一个进化之石，10,000 金币，#k然后 #d卸下你的新宠物#k 再与我对话，但是当然，我不能将你的宠物变回小龙，只能变成另一只成年龙。");
            }
        } else if (status == 2) {
            qm.sendYesNo("现在让我试着进化你的宠物。你准备好了吗？想看你可爱的小龙变成成熟的深黑、蓝色、宁静的绿色或火红的成年龙吗？它的亲密度、等级、名字、饱食度、饥饿度和装备都会保持不变，以防你担心。\r\n\r #b#e你想继续还是有一些最后的事情要做？#k#n");
        } else if (status == 3) {
            qm.sendNextPrev("好的，我们开始吧...！ #rHYAHH!#k");
        } else if (status == 4) {
            var petSlot = qm.getPlayer().getPetIndexByItemId(5000029);
            if (petSlot === -1) {
                qm.getPlayer().message("宠物无法进化。");
                qm.dispose();
                return;
            }
            //qm.unequipPet(qm.getClient());

            var after = qm.evolvePet(petSlot);
            if (after != null) {
                qm.gainItem(5380000, -1);
                qm.gainMeso(-10000);
                qm.completeQuest();

                qm.sendOk("#b太棒了！#k 你的龙变得更加美丽！ #r你可以在 '现金' 背包下找到你的新宠物。\r 它曾经是 #b #i5000029##t5000029##k，现在是 \r 一个 #b#i" + after.getItemId() + "##t" + after.getItemId() + "##k！\r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0#\r\n#v" + after.getItemId() + "# #t" + after.getItemId() + "#");
            } else {
                qm.dispose();
            }

        } else if (status == 5) {
            qm.dispose();
        }
    }
}