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

function Elevator:InitProperty()
    -- 初始在2层
    em:setProperty("current", self.stationA)

    -- 初始状态设置
    self.stationAMap:resetReactors()
    self.stationBMap:resetReactors()
    return eventName
end

function Elevator:scheduleNew()
    local elevatorCurrent = em:getIntProperty("current")
    if (elevatorCurrent == self.stationB) then
        -- 在99层，准备下降
        -- 锁定2层电梯
        self.stationAMap:setReactorState()
        -- 99层电梯开放
        self.stationBMap:resetReactors()
    else
        -- 准备上升
        self.stationAMap:resetReactors()
        self.stationBMap:setReactorState()
    end
    
    -- 准备出发
    em:schedule("takeoff", self.beginTime)
end

-- 出发
function Elevator:takeoff()
    if em:getIntProperty("current") == self.stationB then
        -- 下行
        -- 等待室传送到运行中的电梯
        self.waitingRoomBMap:warpEveryone(self.transportationB)
        -- 锁定99层的电梯
        self.stationBMap:setReactorState()
        -- 设置当前电梯处于运行中
        em:setProperty("current", self.transportationB)
    else
        -- 上行
        self.waitingRoomAMap:warpEveryone(self.transportationA)
        self.stationAMap:setReactorState()
        em:setProperty("current", self.transportationA)
    end

    -- 安排到达时间
    em:schedule("arrived", self.rideTime)
end

function Elevator:arrived()
    if em:getIntProperty("current") == self.stationB then
        -- 下行到达
        self.transportationBMap:warpEveryone(self.stationA, 4)
        em:setProperty("current", self.stationA)
    else
        -- 上行到达
        self.transportationAMap:warpEveryone(self.stationB, 0)
        em:setProperty("current", self.stationB)
    end

    -- 安排下一次运行
    self:scheduleNew()
end

Elevator:new(config)