var ticketId = 5451000;
function start() {
    doGachapon(1);
}
function doGachapon(count) {
    if (!cm.haveItem(ticketId, count)) {
        cm.sendOkLevel(cm.GetTalkMessage("Tip_CheckItemWithId", ticketId, count));
        return;
    }
    if (!cm.CheckGachaponStorage(count)) {
        cm.sendOkLevel(cm.GetTalkMessage("Storage_CheckGachaponStorage", count));
        return;
    }

    cm.gainItem(ticketId, -count);
    for (var i = 0; i < count; i++) {
        const itemObj = cm.doGachapon();
        if (itemObj == null) {
            cm.sendOkLevel(cm.GetTalkMessage("Tip_ThankPatronage"));
            return;
        }
        cm.sendOkLevel("showResult", cm.GetTalkMessage("Tip_ObtainItem", itemObj.ItemId));
    }
}
