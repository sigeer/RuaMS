local BasePrivateTransport = require("scripts/event-lua/__BasePrivateTransport")

-- 配置事件参数
local config = {
    name = "Hak",
    instanceName = "Hak_",
    -- 时间设置（毫秒）
    rideTime = 60 * 1000,   -- 运行时间
    
    -- 地图配置
    stationA = 200000141,    -- 天空之城
    stationB = 250000100,    -- 武陵
    transportationA = 200090300,  -- 天空之城->武陵的鸟
    transportationB = 200090310   -- 武陵->天空之城的鸟
}

BasePrivateTransport:new(config)
