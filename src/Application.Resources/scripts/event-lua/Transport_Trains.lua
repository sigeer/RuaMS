local BaseTransport = require("scripts/event-lua/__BaseTransport")
-- 天空之城 - 玩具城
-- 配置参数
local config = {
    name = "Trains",
    -- 时间设置（毫秒）
    closeTime = 4 * 60 * 1000,
    beginTime = 5 * 60 * 1000,
    rideTime = 5 * 60 * 1000,

    -- 地图配置
    stationA = 200000100,        -- 天空之城售票处
    stationB = 220000100,        -- 玩具城售票处
    waitingRoomA = 200000122,    -- 候船室 - 开往玩具城
    waitingRoomB = 220000111,    -- 候船室 - 开往天空之城
    dockA = 200000121,           -- 天空之城码头 - 开往玩具城
    dockB = 220000110,           -- 玩具城码头 - 开往天空之城
    transportationA = 200090110, -- 航海中 - 开往天空之城
    transportationB = 200090100  -- 航海中 - 开往玩具城
}

BaseTransport:new(config)