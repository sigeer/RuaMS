/**
 * 机器人升级！
 */


function end(mode, type, selection) {
    if (!qm.CanCompleteQuest()) {
        qm.sendOkLevel("需要的材料都拿来了吗？需要1个#t5380000#和50个#t4000111#。");
        return;
    }

    var petSlot = qm.getPlayer().getPetIndex(5000048);
    if (petSlot === -1) {
        qm.getPlayer().message("Pet could not be evolved.");
        qm.dispose();
        return;
    }

    var after = qm.evolvePet(petSlot);
    if (after != null) {
        qm.gainItem(5380000, -1);
        qm.gainItem(4000111, -50);

        qm.completeQuest();
    }
    qm.dispose();
}
