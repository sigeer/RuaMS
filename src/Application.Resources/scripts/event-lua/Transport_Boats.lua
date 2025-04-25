local BaseTransport = require("scripts/event-lua/__BaseTransport")
-- 天空之城A - 魔法密林B
-- 配置参数
local config = {
    name = "Boats",
    -- 时间设置（毫秒）
    closeTime = 4 * 60 * 1000,
    beginTime = 5 * 60 * 1000,
    rideTime = 10 * 60 * 1000,

    -- 地图配置
    stationA = 200000100, -- 天空之城售票处
    stationB = 101000300, -- 魔法密林码头 / 售票处
    waitingRoomA = 200000112, -- 候船室<开往魔法密林>
    waitingRoomB = 101000301, -- 候船室<开往天空之城>
    dockA = 200000111, -- 码头<开往魔法密林>
    dockB = 101000300, -- 魔法密林码头 / 售票处
    transportationA = 200090000, -- 航海中 - 开往魔法密林
    transportationB = 200090010, -- 航海中 - 开往天空之城

    cabinA = 200090001,
    cabinB = 200090011,

    invasionConfig = {
        mobA = 8150000,                         -- 蝙蝠魔
        mobB = 8150000,                         -- 蝙蝠魔
        posAX = 339,                            -- A 船生成怪物坐标
        posAY = 148,
        posBX = -538,                           -- B 船生成怪物坐标
        posBY = 143,
        rateA = 0.42,                               -- 生成怪物概率
        rateB = 0.42,
        countA = 2,                              -- 生成数量
        countB = 2,
        startTime = 3 * 60 * 1000,              -- 蝙蝠魔船只接近时间
        delayTime = 1 * 60 * 1000,              -- 蝙蝠魔船只接近时间随机
        delay = 5 * 1000                        -- 生成怪物的时间延迟
    }
}

BaseTransport:new(config)
