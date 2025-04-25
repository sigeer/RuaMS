-- 基础交通工具类
-- closeTime后，停止检票（无法上船）；beginTime后，开始启航；rideTime后，到达目的地；
-- 上船后，先传送到 waitingRoom，等待beginTime后，传送到 transportation，到站后，传送到station
local BaseTransport = {}
BaseTransport.__index = BaseTransport

-- 构造函数
function BaseTransport:new(config)
    config = config or {}

    -- 基础配置
    config.name = config.name
    -- 时间设置（以毫秒为单位）
    config.closeTime = config.closeTime -- 关闭登船/登车入口的时间
    config.beginTime = config.beginTime -- 启航/发车前的准备时间
    config.rideTime = config.rideTime -- 到达目的地所需的时间

    -- 地图配置
    config.stationA = config.stationA -- 起点站 / 售票处
    config.stationAPortal = config.stationAPortal or 0
    config.stationB = config.stationB -- 终点站 / 售票处
    config.stationBPortal = config.stationBPortal or 0

    config.waitingRoomA = config.waitingRoomA -- 起点候车室
    config.waitingRoomB = config.waitingRoomB -- 终点候车室

    config.dockA = config.dockA -- 起点码头/站台 有些交通工具没有码头/站台
    config.dockB = config.dockB -- 终点码头/站台

    config.transportationA = config.transportationA -- 交通工具本身
    config.transportationB = config.transportationB -- 交通工具本身

    config.cabinA = config.cabinA -- 船舱A
    config.cabinB = config.cabinB -- 船舱B

    config.invasionConfig = config.invasionConfig
    -- {
    --     mobA = 8150000,                         -- 蝙蝠魔
    --     mobB = 8150000,                         -- 蝙蝠魔
    --     posAX = 339,                            -- A 船生成怪物坐标
    --     posAY = 148,
    --     posBX = -538,                           -- B 船生成怪物坐标
    --     posBY = 143,
    --     rateA = 0,                               -- 生成怪物概率
    --     rateB = 0,
    --     countA = 2,                              -- 生成数量
    --     countB = 2,
    --     startTime = 3 * 60 * 1000,              -- 蝙蝠魔船只接近时间
    --     delayTime = 1 * 60 * 1000,              -- 蝙蝠魔船只接近时间随机
    --     delay = 5 * 1000                        -- 生成怪物的时间延迟
    -- }

    setmetatable(config, self)
    config:exportMethods()
    return config
end

-- 创建子类的辅助函数
function BaseTransport:extend(subclass)
    subclass = subclass or {}
    setmetatable(subclass, self)
    subclass.__index = subclass
    return subclass
end

-- 初始化
function BaseTransport:init()
    -- 初始化时间，根据航行速率重置时间
    self.closeTime = em:getTransportationTime(self.closeTime)
    self.beginTime = em:getTransportationTime(self.beginTime)
    self.rideTime = em:getTransportationTime(self.rideTime)

    -- 获取地图实例
    self.stationAMap = em:GetMap(self.stationA)
    self.stationBMap = em:GetMap(self.stationB)
    self.waitingRoomAMap = em:GetMap(self.waitingRoomA)
    self.waitingRoomBMap = em:GetMap(self.waitingRoomB)

    self.transportationMapA = em:GetMap(self.transportationA)
    self.transportationMapB = em:GetMap(self.transportationB)

    if (self.dockA) then
        self.dockAMap = em:GetMap(self.dockA)
    end

    if (self.dockB) then
        self.dockBMap = em:GetMap(self.dockB)
    end

    if self.cabinA then
        self.cabinAMap = em:GetMap(self.cabinA)
    end

    if self.cabinB then
        self.cabinBMap = em:GetMap(self.cabinB)
    end

    -- 安排新的周期性任务
    self:scheduleNew()
    return self.name
end

function BaseTransport:scheduleNew()
    -- 设置属性
    em:setProperty("docked", "true")
    em:setProperty("entry", "true")

    em:setProperty("haveBalrogA", "false")
    em:setProperty("haveBalrogB", "false")

    -- 设置码头/站台状态为已停靠
    if self.dockAMap then
        self.dockAMap:setDocked(true)
    end
    if self.dockBMap then
        self.dockBMap:setDocked(true)
    end

    -- 安排关闭入口和启程的时间点
    em:schedule("stopentry", self.closeTime)
    em:schedule("takeoff", self.beginTime)
end

function BaseTransport:stopentry()
    em:setProperty("entry", "false")
    em:setProperty("next", os.time() * 1000 + self.beginTime - self.closeTime + self.rideTime)

    -- 如果有船舱/车厢，清除其中的对象
    if self.cabinAMap then
        self.cabinAMap:clearMapObjects()
    end
    if self.cabinBMap then
        self.cabinBMap:clearMapObjects()
    end
end

-- 出发
function BaseTransport:takeoff()
    -- 传送玩家
    self.waitingRoomAMap:warpEveryone(self.transportationA)
    self.waitingRoomBMap:warpEveryone(self.transportationB)

    -- 设置码头/站台状态
    em:setProperty("docked", "false")

    -- 广播离开消息
    if self.dockAMap then
        self.dockAMap:broadcastShip(false)
        self.dockAMap:setDocked(false)
    end
    if self.dockBMap then
        self.dockBMap:broadcastShip(false)
        self.dockBMap:setDocked(false)
    end

    if self.invasionConfig then
        -- 随机决定是否会有蝙蝠魔船只接近
        if (math.random() < self.invasionConfig.rateA) then
            em:schedule("invasionApproachA", self.invasionConfig.startTime + math.random(self.invasionConfig.delayTime));
        end
        -- 之前是2船一样，这里调整分开计算概率
        if (math.random() < self.invasionConfig.rateB) then
            em:schedule("invasionApproachB", self.invasionConfig.startTime + math.random(self.invasionConfig.delayTime));
        end
    end

    -- 安排到达时间
    em:schedule("arrived", self.rideTime)
end

-- 到达目的地
function BaseTransport:arrived()
    -- 传送玩家到目的地
    self.transportationMapA:warpEveryone(self.stationB, self.stationBPortal)
    self.transportationMapB:warpEveryone(self.stationA, self.stationAPortal)

    if self.cabinAMap then
        self.cabinAMap:warpEveryone(self.stationB, self.stationBPortal)
    end
    if self.cabinBMap then
        self.cabinBMap:warpEveryone(self.stationA, self.stationAPortal)
    end

    -- 广播到达消息
    if self.dockAMap then
        self.dockAMap:broadcastShip(true)
    end
    if self.dockBMap then
        self.dockBMap:broadcastShip(true)
    end

    if em:getProperty("haveBalrogA") == "true" then
        self.transportationMapA:broadcastEnemyShip(false)
        self.transportationMapA:killAllMonsters()
    end

    if em:getProperty("haveBalrogB") == "true" then
        self.transportationMapB:broadcastEnemyShip(false)
        self.transportationMapB:killAllMonsters()
    end

    -- 安排下一班次
    self:scheduleNew()
end

function BaseTransport:invasionApproachA()
    em:setProperty("haveBalrogA", "true");
    self.transportationA:broadcastEnemyShip(true);
    -- 更改背景音乐
    self.transportationA:broadcastMessage(PacketCreator.musicChange("Bgm04/ArabPirate"));
    -- 安排蝙蝠魔出现的时间点
    em:schedule("invasionSpawnMobA", self.invasionConfig.invasionDelay);
end

function BaseTransport:invasionApproachB()
    em:setProperty("haveBalrogB", "true");
    self.transportationB:broadcastEnemyShip(true);
    -- 更改背景音乐
    self.transportationB:broadcastMessage(PacketCreator.musicChange("Bgm04/ArabPirate"));
    -- 安排蝙蝠魔出现的时间点
    em:schedule("invasionSpawnMobB", self.invasionConfig.invasionDelay);
end

-- 生成蝙蝠魔
function BaseTransport:invasionSpawnMobA()
    -- 生成偶遇蝙蝠魔
    local pos = Point(self.invasionConfig.posAX, self.invasionConfig.posAY)
    for i = 1, self.invasionConfig.countA do
        self.transportationA:spawnMonsterOnGroundBelow(LifeFactory.getMonster(self.invasionConfig.mobA), pos)
    end
end

function BaseTransport:invasionSpawnMobB()
    -- 生成偶遇蝙蝠魔
    local pos = Point(self.invasionConfig.posBX, self.invasionConfig.posBY)
    for i = 1, self.invasionConfig.countB do
        self.transportationB:spawnMonsterOnGroundBelow(LifeFactory.getMonster(self.invasionConfig.mobB), pos)
    end
end

-- 取消调度
function BaseTransport:cancelSchedule()
end

function BaseTransport:exportMethods()
    local exported = {}
    local current = self
    while current do
        for k, v in pairs(current) do
            if type(v) == "function" and not exported[k] then
                _ENV[k] = function(...) return v(self, ...) end
                exported[k] = true
            end
        end
        local mt = getmetatable(current)
        current = mt and mt.__index or nil
    end
end

return BaseTransport
