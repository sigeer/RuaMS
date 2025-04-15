local BaseEvent = require("scripts/event-lua/__BaseEvent")

-- 无需等待的交通工具
local BasePrivateTransport = BaseEvent:extend()

function BasePrivateTransport:new(config)
    local instance = {}
    
    -- 基础配置
    instance.name = config.name
    -- 时间设置（以毫秒为单位）
    instance.rideTime = config.rideTime   -- 到达目的地所需的时间
    
    -- 地图配置
    instance.stationA = config.stationA       -- A站
    instance.stationB = config.stationB       -- B站
    instance.transportationA = config.transportationA   -- A->B路线
    instance.transportationB = config.transportationB   -- B->A路线
    
    return BaseEvent:new(instance)
end

function BasePrivateTransport:init()
    -- 初始化时间
    self.rideTime = em:getTransportationTime(self.rideTime)
    
    -- 获取地图实例
    self.stationAMap = em:GetMap(self.stationA)
    self.stationBMap = em:GetMap(self.stationB)
    self.transportationMapA = em:GetMap(self.transportationA)
    self.transportationMapB = em:GetMap(self.transportationB)
    return self.name
end

function BasePrivateTransport:setup(level, lobbyid)
    return em:newInstance(self.name .. "_" .. lobbyid)
end

function BasePrivateTransport:playerEntry(eim, player)
    -- 根据玩家当前位置决定乘坐方向
    local currentMap = player:getMapId()
    local transportMap
    
    if currentMap == self.stationA then
        transportMap = self.transportationMapA
    else
        transportMap = self.transportationMapB
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
    
    if success then
        -- 成功到达目的地
        if currentMap == self.transportationA then
            destMap = self.stationBMap
        else
            destMap = self.stationAMap
        end
    else
        -- 中途退出，返回出发点
        if currentMap == self.transportationA then
            destMap = self.stationAMap
        else
            destMap = self.stationBMap
        end
    end
    
    player:changeMap(destMap, destMap:getPortal(0))
end

function BasePrivateTransport:timeOut(eim)
    -- 时间到，结束行程
    local players = eim:getPlayers()
    for i = 1, #players do
        self:playerExit(eim, players[i], true)
    end
    eim:dispose()
end

function BasePrivateTransport:playerDisconnected(eim, player)
    self:playerExit(eim, player, false)
end

return BasePrivateTransport