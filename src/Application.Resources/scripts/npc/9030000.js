/**
 * 弗兰德里 雇佣商店物品取回
 */

function start() {
    var remoteData = cm.LoadFredrickRegistry();
    if (remoteData.MapId > 0) {
        cm.sendOkLevel("你有一个正在营业的商店，位于频道：" + remoteData.Channel + "，地图：" + remoteData.MapName);
    } else {
        if (remoteData.HasItem) {
            cm.ShowFredrick(remoteData);
        } else {
            cm.sendOkLevel("你没有任何物品或金币可以取回。");
        }
    }
}