local BaseTransport = require("scripts/event-lua/__BaseTransport")

-- 配置事件参数
local config = {
    name = "AirPlane",
    -- 时间设置（毫秒）
    closeTime = 4 * 60 * 1000,  -- 关闭登机时间
    beginTime = 5 * 60 * 1000,  -- 起飞前准备时间
    rideTime = 1 * 60 * 1000,   -- 飞行时间
    
    -- 地图配置
    stationA = 103000000,    -- 废弃都市
    stationAPortal = 7,
    stationB = 540010000,    -- CBD

    waitingRoomA = 540010100,  -- 废弃都市候机室
    waitingRoomB = 540010001,  -- CBD候机室
    transportationA = 540010101,  -- 飞往CBD的飞机
    transportationB = 540010002   -- 飞往废弃都市的飞机
}

-- 创建事件实例
local event = BaseTransport:new(config)

-- 导出所有方法到全局环境
local function exportMethods(obj)
    local exported = {}
    local current = obj
    while current do
        for k, v in pairs(current) do
            if type(v) == "function" and not exported[k] then
                _ENV[k] = function(...) return v(event, ...) end
                exported[k] = true
            end
        end
        current = getmetatable(current)
        if current then
            current = current.__index
        end
    end
end

exportMethods(event)