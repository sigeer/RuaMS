﻿/*
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
/* Aldol
 * 
 * @Author Ronan
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
        if (mode == 0 && status == 0) {
            cm.dispose();
            return;
        }
        if (mode == 1) {
            status++;
        } else {
            status--;
        }

        var eim = cm.getEventInstance();
        if (!eim.isEventCleared()) {
            if (status == 0) {
                cm.sendYesNo("如果你现在离开，就无法返回了。你确定要离开吗？");
            } else if (status == 1) {
                cm.warp(551030100, 2);
                cm.dispose();
            }
        } else {
            if (status == 0) {
                cm.sendNext("你们打败了斯卡利昂和塔加！太棒了！把这个纪念品当作你们勇敢的奖励。");
            } else if (status == 1) {
                if (!eim.giveEventReward(cm.getPlayer())) {
                    cm.sendNext("请先在你的背包里腾出空间！");
                } else {
                    cm.warp(551030100, 2);
                }

                cm.dispose();
            }
        }
    }
}