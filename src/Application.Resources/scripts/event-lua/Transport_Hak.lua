local BasePrivateTransport = require("scripts/event-lua/__BasePrivateTransport")

-- 配置事件参数
local config = {
    name = "Hak",
    -- 时间设置（毫秒）
    rideTime = 60 * 1000,   -- 运行时间
    
    -- 地图配置
    stationA = 200000141,    -- 天空之城
    stationB = 250000100,    -- 武陵
    transportationA = 200090300,  -- 天空之城->武陵的鸟
    transportationB = 200090310   -- 武陵->天空之城的鸟
}

-- 创建事件实例
local event = BasePrivateTransport:new(config)

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