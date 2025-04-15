-- 单人挑战事件
local BasePQ = require("scripts/event-lua/__BasePQ")

local BaseChallenge = BasePQ:extend()

function BaseChallenge:new(config)
    return BasePQ.new(self, config)
end

-- 在ChannelServer加载后执行初始化操作
function BaseChallenge:init()
    em:setProperty("noEntry", "false")
    return self.name
end


function BaseChallenge:respawnStages(eim) end

-- 不同于__BasePQ，地图初始化和计时在进入时触发而不是setup
function BaseChallenge:playerEntry(eim, player)
    self:InitializeMap(eim)
    BasePQ.playerEntry(self, eim, player)
end

function BaseChallenge:InitializeMap(eim)

end

function BaseChallenge:playerUnregistered(eim, player) end

function BaseChallenge:playerExit(eim, player)
    self:clearPQ(eim)
end

function BaseChallenge:scheduledTimeout(eim)
    self:clearPQ(eim)
end

function BaseChallenge:playerDisconnected(eim, player)
    self:clearPQ(eim)
end

function BaseChallenge:changedMap(eim, player, mapId)
    if mapId < self.minMapId or mapId > self.maxMapId then
        self:clearPQ(eim)
    end
end


function BaseChallenge:clearPQ(eim)
    eim:stopEventTimer()
    eim:setEventCleared()

    local player = eim:getPlayers()[0]
    eim:unregisterPlayer(player)
    player:changeMap(self.exitMap)

    eim:dispose()
    em:setProperty("noEntry", "false")
end

return BaseChallenge