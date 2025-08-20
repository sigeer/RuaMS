/**
 *9201101 - T-1337
 *@author Ronan
 */

function start() {
    if (YamlConfig.config.server.USE_ENABLE_CUSTOM_NPC_SCRIPT) {
        cm.openShopNPC(9201101);
    } else {
        //cm.sendOk("新叶城的巡逻队随时待命。没有任何生物能够突破到城市里。");
        cm.sendDefault();
    }

    cm.dispose();
}