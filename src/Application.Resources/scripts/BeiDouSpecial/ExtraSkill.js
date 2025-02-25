const skillList = [
    { name: "轻功", id: 4101004 },
    { name: "二段跳", id: 4111006 },
    { name: "瞬间移动", id: 2201002 },
]
function start() {
    levelStart();
}


function levelStart() {
    let text = "额外技能选择\r\n\r\n";
    for (var i = 0; i < skillList.length; i++) {
        text += `#L0# ${skillList[i].name} #l\r\n`;
    }
    cm.sendNextSelectLevel("SelectSkill", text);
}

function levelSelectSkill(idx) {
    cm.LearnExtraSkill(skillList[idx].id);
}
