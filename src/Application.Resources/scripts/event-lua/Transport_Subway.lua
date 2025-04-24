local BaseTransport = require("scripts/event-lua/__BaseTransport")
-- 废弃都市A - 新叶城B
-- 配置参数
local config = {
    name = "Subway",
    -- 时间设置（毫秒）
    closeTime = 50 * 1000,
    beginTime = 1 * 60 * 1000,
    rideTime = 4 * 60 * 1000,

    -- 地图配置
    stationA = 103000100, -- 废弃都市地铁站
    stationB = 600010001, -- 新叶城 地铁站
    waitingRoomA = 600010004, -- 候车室(从废弃都市往新叶城)
    waitingRoomB = 600010002, -- 候车室(从新叶城往废弃都市)
    dockA = 103000100, -- 废弃都市地铁站
    dockB = 600010001, -- 新叶城 地铁站
    transportationA = 600010005, -- 行驶中 - 从废弃都市往新叶城
    transportationB = 600010003, -- 行驶中 - 从新叶城往废弃都市
}

-- 创建实例
local transport = BaseTransport:new(config)

-- 导出所有方法到全局环境
local function exportMethods(obj)
    local exported = {}
    local current = obj
    while current do
        for k, v in pairs(current) do
            if type(v) == "function" and not exported[k] then
                _ENV[k] = function(...)
                    return v(transport, ...)
                end
                exported[k] = true
            end
        end
        current = getmetatable(current)
        if current then
            current = current.__index
        end
    end
end

exportMethods(transport)
