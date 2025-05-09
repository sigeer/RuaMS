local BaseTransport = require("scripts/event-lua/__BaseTransport")
-- 天空之城A - 阿里安特B
-- 配置参数
local config = {
    name = "Genie",
    -- 时间设置（毫秒）
    closeTime = 4 * 60 * 1000,
    beginTime = 5 * 60 * 1000,
    rideTime = 5 * 60 * 1000,

    -- 地图配置
    stationA = 200000100, -- 天空之城 - 天空之城售票处
    stationB = 260000100, -- 阿里安特升降场 - 前往天空之城
    waitingRoomA = 200000152, -- 前往＜阿里安特＞的船所在的机场
    waitingRoomB = 260000110, -- 出发前往天空之城
    dockA = 200000151, -- 码头 - 前往阿里安特
    dockB = 260000100, -- 阿里安特升降场 - 前往天空之城
    transportationA = 200090400, -- 行驶中 - 开往阿里安特
    transportationB = 200090410, -- 行驶中 - 开往天空之城
}

BaseTransport:new(config)