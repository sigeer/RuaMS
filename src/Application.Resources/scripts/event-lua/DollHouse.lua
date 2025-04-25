local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    name = "DollHouse",
    instanceName = "DollHouse_",
    minPlayers = 1,
    maxPlayers = 1,
    minLevel = 1,
    maxLevel = 200,
    entryMap = 922000010,
    exitMap = 221024400,
    exitPortal = 4,
    recruitMap = 221024400,
    clearMap = 221024400,
    minMapId = 922000010,
    maxMapId = 922000010,
    eventTime = 10,
    maxLobbies = 1,

    -- 重置地图配置
    resetReactorMaps = { 922000010 },

    -- 任务特有道具
    eventItems = { 4031094 }
}

-- 创建自定义事件
local DollHouse = BaseEvent:extend()

function DollHouse:init()
    em:setProperty("noEntry", "false")
end

function DollHouse:playerEntry(eim, player)
    BaseEvent.playerEntry(self, eim, player)
    em:setProperty("noEntry", "true")
    player:sendPacket(PacketCreator.getClock(self.eventTime * 60));
end

function DollHouse:playerExit(eim, player)
    eim:unregisterPlayer(player)
    eim:dispose()
    em:setProperty("noEntry", "false")
end

function DollHouse:scheduledTimeout(eim)
    local player = eim:getPlayers()[0]
    if player then
        self:playerExit(eim, player)
        player:changeMap(self.exitMap, self.exitPortal)
    end
end

function DollHouse:playerDisconnected(eim, player)
    self:playerExit(eim, player)
end

function DollHouse:changedMap(eim, player, mapId)
    if mapId ~= self.entryMap then
        self:playerExit(eim, player)
    end
end

DollHouse:new(config)