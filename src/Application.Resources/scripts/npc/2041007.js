﻿/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc> 
                       Matthias Butz <matze@odinms.de>
                       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation. You may not use, modify
    or distribute this program under any other version of the
    GNU Affero General Public License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

/* Miyu
	Ludibrium VIP Hair/Hair Color Change.

        GMS-like revised by Ronan -- contents found thanks to Mitsune (GamerBewbs), Waltzing, AyumiLove
*/
var status = 0;
var beauty = 0;
var hairprice = 1000000;
var haircolorprice = 1000000;
var mhair_v = Array(30160, 30190, 30250, 30640, 30660, 30840, 30870, 30990);
var fhair_v = Array(31270, 31290, 31550, 31680, 31810, 31830, 31840, 31870);
var hairnew = Array();

function pushIfItemExists(array, itemid) {
    if ((itemid = cm.getCosmeticItem(itemid)) != -1 && !cm.isCosmeticEquipped(itemid)) {
        array.push(itemid);
    }
}

function start() {
    status = -1;
    action(1, 0, 0);
}

function action(mode, type, selection) {
    if (mode < 1) {  // disposing issue with stylishs found thanks to Vcoc
        cm.dispose();
    } else {
        if (mode == 1) {
            status++;
        } else {
            status--;
        }
        if (status == 0) {
            cm.sendSimple("欢迎来到鲁塔比姆美发沙龙！你有#b#t5150007##k或者#b#t5151007##k吗？如果有的话，让我来为你打理一下头发吧？请选择你想要做的事情...\r\n#L1#理发：#i5150007##t5150007##l\r\n#L2#染发：#i5151007##t5151007##l");
        } else if (status == 1) {
            if (selection == 1) {
                beauty = 1;
                hairnew = Array();
                if (cm.getPlayer().getGender() == 0) {
                    for (var i = 0; i < mhair_v.length; i++) {
                        pushIfItemExists(hairnew, mhair_v[i] + parseInt(cm.getPlayer().getHair() % 10));
                    }
                }
                if (cm.getPlayer().getGender() == 1) {
                    for (var i = 0; i < fhair_v.length; i++) {
                        pushIfItemExists(hairnew, fhair_v[i] + parseInt(cm.getPlayer().getHair() % 10));
                    }
                }
                cm.sendStyle("我可以完全改变你的发型，你还没准备好接受改变吗？给我 #b#t5150007##k，剩下的事我来帮你处理，选你喜欢的风格吧！", hairnew);
            } else if (selection == 2) {
                beauty = 2;
                haircolor = Array();
                var current = parseInt(cm.getPlayer().getHair() / 10) * 10;
                for (var i = 0; i < 8; i++) {
                    pushIfItemExists(haircolor, current + i);
                }
                cm.sendStyle("我可以完全改变你头发的颜色，你还没准备好接受改变吗？给我 #b#t5151007##k，剩下的我来负责，选你喜欢的颜色吧！", haircolor);
            }
        } else if (status == 2) {
            cm.dispose();
            if (beauty == 1) {
                if (cm.haveItem(5420005)) {
                    cm.setHair(hairnew[selection]);
                    cm.sendOk("享受你的新发型吧！");
                } else if (cm.haveItem(5150007) == true) {
                    cm.gainItem(5150007, -1);
                    cm.setHair(hairnew[selection]);
                    cm.sendOk("享受你的新发型吧！");
                } else {
                    cm.sendOk("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你理发。对不起...");
                }
            }
            if (beauty == 2) {
                if (cm.haveItem(5151007) == true) {
                    cm.gainItem(5151007, -1);
                    cm.setHair(haircolor[selection]);
                    cm.sendOk("享受你的新发色！");
                } else {
                    cm.sendOk("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你染发。很抱歉...");
                }
            }
            if (beauty == 0) {
                if (selection == 0 && cm.getMeso() >= hairprice) {
                    cm.gainMeso(-hairprice);
                    cm.gainItem(5150007, 1);
                    cm.sendOk("享受！");
                } else if (selection == 1 && cm.getMeso() >= haircolorprice) {
                    cm.gainMeso(-haircolorprice);
                    cm.gainItem(5151007, 1);
                    cm.sendOk("享受！");
                } else {
                    cm.sendOk("你没有足够的金币来购买优惠券！");
                }
            }
        }
    }
}