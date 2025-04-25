local BaseTransport = require("scripts/event-lua/__BaseTransport")

-- 电梯，赫丽奥斯塔2层A - 赫丽奥斯塔99层B
-- 配置事件参数
local config = {
    name = "Elevator",
    -- 时间设置（毫秒）
    beginTime = 60 * 1000,  -- 开始运行时间
    closeTime = 60 * 1000,
    rideTime = 60 * 1000,   -- 运行时间
    
    -- 地图配置（电梯系统）
    stationA = 222020100,     -- 2层
    stationB = 222020200,     -- 99层
    waitingRoomA = 222020110, -- 2层电梯-准备上升
    waitingRoomB = 222020210, -- 99层电梯-准备下降
    transportationA = 222020111, -- 上升中
    transportationB = 222020211  -- 下降中
}

-- 创建自定义交通工具
local Elevator = BaseTransport:extend()

-- 不同于其他，每次都是单向的
function Elevator:init()
    local eventName = BaseTransport.init(self)
    -- 初始状态设置
    self.stationAMap:resetReactors()
    self.stationBMap:resetReactors()
    return eventName
end

function Elevator:scheduleNew()
    local elevatorCurrent = em:getProperty("direction")
    if (elevatorCurrent == "up") then
        -- 准备下降
        self.stationAMap:setReactorState()
        self.stationBMap:resetReactors()
        em:setProperty("direction", "down")
    else
        -- 准备上升
        self.stationAMap:resetReactors()
        self.stationBMap:setReactorState()
        em:setProperty("direction", "up")
    end
    
    -- 安排下一次运行
    em:schedule("takeoff", self.beginTime)
end

function Elevator:takeoff()
    if em:getProperty("direction") == "down" then
        -- 下行
        self.waitingRoomBMap:warpEveryone(self.transportationB)
        self.stationBMap:setReactorState()
    else
        -- 上行
        self.waitingRoomAMap:warpEveryone(self.transportationA)
        self.stationAMap:setReactorState()
    end
    
    -- 安排到达时间
    em:schedule("arrived", self.rideTime)
end

function Elevator:arrived()
    if em:getProperty("direction") == "down" then
        -- 下行到达
        self.transportationBMap:warpEveryone(self.stationA, 4)
    else
        -- 上行到达
        self.transportationAMap:warpEveryone(self.stationB, 0)
    end
    
    -- 安排下一次运行
    self:scheduleNew()
end

Elevator:new(config)