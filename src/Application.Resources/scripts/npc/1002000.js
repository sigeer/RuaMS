var status = 0;
var imaps = [104000000, 102000000, 101000000, 100000000, 103000000, 120000000, 105040300];
var maps = [102000000, 100000000, 101000000, 103000000, 120000000];
var cost = [1000, 1000, 800, 1000, 800];


var 明珠港_1 = "Npc1002000_LithHarbor1";
var 明珠港_2 = "Npc1002000_LithHarbor2";
var 明珠港_3 = "Npc1002000_LithHarbor3";
var 勇士部落_1 = "Npc1002000_Perion1";
var 勇士部落_2 = "Npc1002000_Perion2";
var 勇士部落_3 = "Npc1002000_Perion3"
var 魔法密林_1 = "Npc1002000_Ellinia1"; 
var 魔法密林_2 = "Npc1002000_Ellinia2"
var 魔法密林_3 = "Npc1002000_Ellinia3";
var 射手村_1 = "Npc1002000_Henesys1";
var 射手村_2 = "Npc1002000_Henesys2";
var 射手村_3 = "Npc1002000_Henesys3";
var 废弃都市_1 = "Npc1002000_KerningCity1";
var 废弃都市_2 = "Npc1002000_KerningCity2";
var 废弃都市_3 = "Npc1002000_KerningCity3";
var 诺特勒斯_1 = "Npc1002000_Nautilus1";
var 诺特勒斯_2 = "Npc1002000_Nautilus2";
var 诺特勒斯_3 = "Npc1002000_Nautilus3";
var 林中之城_1 = "Npc1002000_Sleepywood1";
var 林中之城_2 = "Npc1002000_Sleepywood2";
var 林中之城_3 = "Npc1002000_Sleepywood3";
var 林中之城_4 = "Npc1002000_Sleepywood4";


var townText = [[明珠港_1, 明珠港_2, 明珠港_3], [勇士部落_1, 勇士部落_2, 勇士部落_3], [魔法密林_1, 魔法密林_2, 魔法密林_3], [射手村_1, 射手村_2, 射手村_3], [废弃都市_1, 废弃都市_2, 废弃都市_3], [诺特勒斯_1,诺特勒斯_2,诺特勒斯_3],[林中之城_1,林中之城_2,林中之城_3,林中之城_4]];
var selectedMap = -1;
var town = false;

function start() {
    cm.sendNext(cm.GetTalkMessage("Npc1002000_Message0"));
}

function action(mode, type, selection) {
    status++;
    if (mode != 1) {
        if ((mode == 0 && !town) || mode == -1) {
            if (type == 1 && mode != -1) {
                cm.sendNext("Npc1002000_Message1");
            }
            cm.dispose();
            return;
        } else {
            status -= 2;

            if (status < 1) {
                cm.dispose();
                return;
            }
        }

    }
    if (status == 1) {
        cm.sendSimple(cm.GetTalkMessage("Npc1002000_Message2"));
    } else if (status == 2) {
        if (selection == 0) {
            town = true;
            var text = cm.GetTalkMessage("Npc1002000_Message3");
            for (var i = 0; i < imaps.length; i++) {
                text += "\r\n#L" + i + "##m" + imaps[i] + "##l";
            }
            cm.sendSimple(text);
        } else if (selection == 1) {
            var selStr = cm.GetTalkMessage(cm.getJobId() == 0 ? "Npc1002000_CoditionNoviceTrue" : "Npc1002000_CoditionNoviceFalse");
            for (var i = 0; i < maps.length; i++) {
                selStr += "\r\n#L" + i + "##m" + maps[i] + "# (" + (cost[i] / (cm.getJobId() == 0 ? 10 : 1)) + "  " + cm.GetTalkMessage("Meso") + ")#l";
            }
            cm.sendSimple(selStr);
        }
    } else if (town) {
        if (selectedMap == -1) {
            selectedMap = selection;
        }
        if (status == 3) {
            cm.sendNext(cm.GetTalkMessage(townText[selectedMap][status - 3]));
        } else {
            townText[selectedMap][status - 3] == undefined ? cm.dispose() : cm.sendNextPrev(cm.GetTalkMessage(townText[selectedMap][status - 3]));
        }
    } else if (status == 3) {
        selectedMap = selection;
        cm.sendYesNo(cm.GetTalkMessage("Npc1002000_ConfirmMove", maps[selection], (cost[selection] / (cm.getJobId() == 0 ? 10 : 1)).toString()));
    } else if (status == 4) {
        if (cm.getMeso() < (cost[selectedMap] / (cm.getJobId() == 0 ? 10 : 1))) {
            cm.sendNext(cm.GetTalkMessage("Npc1002000_MesoNotEnough"));
        } else {
            cm.gainMeso(-(cost[selectedMap] / (cm.getJobId() == 0 ? 10 : 1)));
            cm.warp(maps[selectedMap]);
        }
        cm.dispose();
    }
}