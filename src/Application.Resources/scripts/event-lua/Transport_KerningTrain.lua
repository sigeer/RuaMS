local BasePrivateTransport = require("scripts/event-lua/__BasePrivateTransport")

-- 配置事件参数
local config = {
    name = "KerningTrain",
    instanceName = "KerningTrain_",
    -- 时间设置（毫秒）
    rideTime = 10 * 1000,   -- 运行时间
    
    -- 地图配置
    stationA = 103000100,    -- 废弃都市
    stationB = 103000310,    -- 废都广场
    transportationA = 103000301,  -- 废弃都市->废都广场
    transportationB = 103000302   -- 废都广场->废弃都市
}

-- 创建自定义交通工具
local KerningTrain = BasePrivateTransport:extend()

function KerningTrain:playerEntry(eim, player)
    -- 调用父类方法
    BasePrivateTransport.playerEntry(self, eim, player)
    
    -- 添加站名提示
    local currentMap = player:getMapId()
    local nextStation = currentMap == self.transportationA and "废都广场" or "废弃都市"
    player:sendPacket(PacketCreator.earnTitleMessage("下一站停靠 " .. nextStation .. " 站。请走左侧门。"))
end

KerningTrain:new(config)