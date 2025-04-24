local BaseTransport = require("scripts/event-lua/__BaseTransport")

-- 配置事件参数
local config = {
    name = "Cabin",
    -- 时间设置（毫秒）
    closeTime = 4 * 60 * 1000,  -- 关闭检票时间
    beginTime = 5 * 60 * 1000,  -- 出发前准备时间
    rideTime = 5 * 60 * 1000,   -- 运行时间
    
    -- 地图配置
    stationA = 200000100,    -- 天空之城站
    stationB = 240000100,    -- 神木村站
    waitingRoomA = 200000132,  -- 天空之城候车室
    waitingRoomB = 240000111,  -- 神木村候车室
    dockA = 200000131,       -- 天空之城站台
    dockB = 240000110,       -- 神木村站台
    transportationA = 200090200,  -- 开往神木村的缆车
    transportationB = 200090210   -- 开往天空之城的缆车
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

