local BaseEvent = require("scripts/event-lua/__BaseEvent")

-- 无需等待的交通工具
local BasePrivateTransport = BaseEvent:extend()

function BasePrivateTransport:new(config)
    config = config or {}
    -- 基础配置
    config.name = config.name
    config.instanceName = config.instanceName
    -- 时间设置（以毫秒为单位）
    config.rideTime = config.rideTime   -- 到达目的地所需的时间
    
    -- 地图配置
    config.stationA = config.stationA       -- A站
    config.stationAPortal = config.stationAPortal or 0
    config.stationB = config.stationB       -- B站
    config.stationBPortal = config.stationBPortal or 0
    config.transportationA = config.transportationA   -- A->B路线
    config.transportationB = config.transportationB   -- B->A路线
    config.maxLobbies = config.maxLobbies or 99
    
    config.loaded = false
    return BaseEvent.new(self, config)
end

function BasePrivateTransport:setup(level, lobbyid)
    if not self.loaded then
        -- 初始化时间
        self.rideTime = em:getTransportationTime(self.rideTime)
        -- 获取地图实例
        self.stationAMap = em:GetMap(self.stationA)
        self.stationBMap = em:GetMap(self.stationB)
        self.loaded = true
    end

    return em:newInstance(self.instanceName .. lobbyid)
end

function BasePrivateTransport:playerEntry(eim, player)
    -- 根据玩家当前位置决定乘坐方向
    local currentMap = player:getMapId()
    local transportMap
    
    if currentMap == self.stationA then
        transportMap = eim:getMapInstance(self.transportationA)
    else
        transportMap = eim:getMapInstance(self.transportationB)
    end
    
    -- 传送玩家到交通工具
    player:changeMap(transportMap, transportMap:getPortal(0))
    -- 显示倒计时
    player:sendPacket(PacketCreator.getClock(self.rideTime / 1000))
    -- 安排到达时间
    eim:schedule("timeOut", self.rideTime)
end

function BasePrivateTransport:playerExit(eim, player, success)
    eim:unregisterPlayer(player)
    -- 根据是否成功完成决定目的地
    local currentMap = player:getMapId()
    local destMap
    local destPortal
    
    if success then
        -- 成功到达目的地
        if currentMap == self.transportationA then
            destMap = self.stationBMap
            destPortal = self.stationBPortal
        else
            destMap = self.stationAMap
            destPortal = self.stationAPortal
        end
    else
        -- 中途退出，返回出发点
        if currentMap == self.transportationA then
            destMap = self.stationAMap
            destPortal = self.stationAPortal
        else
            destMap = self.stationBMap
            destPortal = self.stationBPortal
        end
    end
    
    player:changeMap(destMap, destMap:getPortal(destPortal))
    eim:dispose()
end

function BasePrivateTransport:timeOut(eim)
    -- 时间到，结束行程
    local players = eim:getPlayers()
    for i = 0, players.Count - 1 do
        self:playerExit(eim, players[i], true)
    end
end

function BasePrivateTransport:playerDisconnected(eim, player)
    self:playerExit(eim, player, false)
end

return BasePrivateTransport