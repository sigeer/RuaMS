var ticketId = 5451000;
function start() {
    if (!cm.haveItem(ticketId, 1)) {
        cm.sendOkLevel(cm.GetTalkMessage("Tip_CheckItemWithId", ticketId, 1));
        return;
    }
    if (!cm.CheckGachaponStorage(1)) {
        cm.sendOkLevel(cm.GetTalkMessage("Storage_CheckGachaponStorage", 1));
        return;
    }

    cm.gainItem(ticketId, -1);
    const itemObj = cm.doGachapon();
    if (itemObj == null) {
        cm.sendOkLevel(cm.GetTalkMessage("Tip_ThankPatronage"));
        return;
    }
    cm.sendOkLevel("showResult", cm.GetTalkMessage("Tip_ObtainItem", itemObj.ItemId));
}

