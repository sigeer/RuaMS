local BasePrivateTransport = require("scripts/event-lua/__BasePrivateTransport")

-- 里恩 - 明珠港
local config = {
    name = "Whale",
    instanceName = "Whale_",
    -- 时间设置（毫秒）
    rideTime = 60 * 1000,   -- 运行时间
    
    -- 地图配置
    stationA = 140020300,    -- 企鹅港口（里恩）
    stationB = 104000000,    -- 明珠港
    stationBPortal = 3,
    transportationA = 200090070,  -- 开往明珠港
    transportationB = 200090060   -- 开往里恩岛
}

BasePrivateTransport:new(config)