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
	Description: 		Quest - Pet Re-Evolution
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
                qm.sendOk("嗨！我需要 #b10,000 金币#k 来进行您的宠物再进化！");
                qm.dispose();
                return;
            }

            if (!qm.haveItem(5380000)) {
                qm.sendOk("若没有进化之石是无法让宠物进化的！难道你没听懂吗？快去拿回来吧！");
                qm.dispose();
                return;
            }

            qm.sendYesNo("好的，那么，让我们再来一次吧，好吗？和往常一样，这将是随机的，我将拿走你的一个进化之石。\r\n\r #r#e准备好了吗？#n#k");
        } else if (status == 1) {
            qm.sendNextPrev("好的，我们开始吧...！ #rHYAHH!#k");
        } else if (status == 2) {
            var petidx = -1;
            var id = -1;
            for (var i = 0; i < 3; i++) {
                var pet = qm.getPlayer().getPet(i);
                if (pet != null) {
                    id = pet.getItemId();
                    if (id >= 5000029 && id <= 5000033) {
                        petidx = i;
                        break;
                    } else if (id >= 5000048 && id <= 5000053) {    // thanks Conrad for noticing Robo pets not being able to re-evolve
                        petidx = i;
                        break;
                    }
                }
            }

            if (petidx == -1) {
                qm.sendOk("出现了一些问题 请重试.");
                qm.dispose();
                return;
            }

            var after = qm.evolvePet(petidx);
            if (after != null) {
                qm.gainMeso(-10000);
                qm.gainItem(5380000, -1);
                qm.completeQuest();

                qm.sendOk("哇！又成功了！#r你可以在'现金'物品栏下找到你的新宠物。\r #k它曾经是一个#b#i" + id + "##t" + id + "##k，现在它是一个#b#i" + after.getItemId() + "##t" + after.getItemId() + "##k！\r\n 如果你不喜欢，带着1万枚金币和另一个进化之石回来吧！\r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0#\r\n#v" + after.getItemId() + "# #t" + after.getItemId() + "#");
            } else {
                qm.dispose();
            }


        } else if (status == 3) {
            qm.dispose();
        }
    }
}
