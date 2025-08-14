local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    instanceName = "BalrogQuest_",
    minPlayers = 1,
    maxPlayers = 1,
    minLevel = 1,
    maxLevel = 255,
    entryMap = 910520000,
    exitMap = 105100100,
    recruitMap = 0,
    clearMap = 0,
    warpTeamWhenClear = false,
    minMapId = 910520000,
    maxMapId = 910520000,
    eventTime = 10,
    maxLobbies = 1,

    -- base.setup.resetMap 中调用
    -- 重置地图
    resetPQMaps = {},
    -- 打乱地图reactor顺序
    resetReactorMaps = {},
    -- base.setup.respawnStages调用 地图怪物重生设置
    respawnConfig = {
        maps = {},
        duration = 15000
    },
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 没办法通过设置处理的就在这里进行重载
function Sample:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    -- xxx
    eim:setProperty("boss", "0")
    return eim
end

function Sample:playerEntry(eim, player)
    local mapObj = eim:getInstanceMap(self.entryMap);

    mapObj:resetPQ(1);
    mapObj:instanceMapForceRespawn();
    mapObj:closeMapSpawnPoints();

    player.changeMap(self.entryMap, 1);
    em:setProperty("noEntry", "true");
    player:sendPacket(PacketCreator.getClock(self.eventTime * 60));
    eim:startEventTimer(self.eventTime * 60000);
end

function Sample:monsterKilled(mob, eim)
    if mob:getId() == 9300326 then
        eim:spawnNpc(1061015, Point(0, 115), mob:getMap());
    end
end

Sample:new(config)