﻿/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2016 - 2019 RonanLana

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

/**
 * @author: Ronan
 * @npc: Hidden Documents
 * @func: Yulete lab 2 quest
 */

function start() {
    if (cm.isQuestStarted(3367)) {
        var c = cm.getQuestProgressInt(3367, 30);
        if (c >= 30) {
            cm.sendNext("（所有文件已整理好。向尤莱特报告找到的文件。）");
            cm.dispose();
            return;
        }

        var book = (cm.getNpcObjectId() % 30);
        var prog = cm.getQuestProgressInt(3367, book);
        if (prog == 0) {
            c++;

            if (book < 20) {
                if (!cm.canHold(4031797, 1)) {
                    cm.sendNext("（你找到了一份报告文件，但由于你的杂项栏已满，你选择将文件放在你找到的地方。）");
                    cm.dispose();
                    return;
                } else {
                    cm.gainItem(4031797, 1);
                    cm.setQuestProgress(3367, 31, cm.getQuestProgressInt(3367, 31) + 1);
                }
            }

            cm.sendNext("（整理文件。#r" + (30 - c) + "#k 剩余。）");

            cm.setQuestProgress(3367, book, 1);
            cm.setQuestProgress(3367, 30, c);
        }
    }

    cm.dispose();
}