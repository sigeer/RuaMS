-- 渡轮相关地图变量
local Orbis_btf  -- 候船室<开往魔法密林>
local Boat_to_Orbis  -- 开往天空之城
local Orbis_Boat_Cabin  -- 船仓<开往天空之城>
local Orbis_docked  -- 码头<开往魔法密林>

local Ellinia_btf  -- 候船室<开往天空之城>
local Boat_to_Ellinia  -- 开往魔法密林
local Ellinia_Boat_Cabin  -- 船仓<开往魔法密林>
local Ellinia_docked  -- 魔法密林码头

local Orbis_Station  -- 天空之城售票处

-- 时间设置（以毫秒为单位）
local closeTime = 4 * 60 * 1000  -- 关闭登船入口的时间
local beginTime = 5 * 60 * 1000  -- 船只启航前的准备时间
local rideTime = 10 * 60 * 1000  -- 到达目的地所需的时间
local invasionStartTime = 3 * 60 * 1000  -- 蝙蝠魔船只接近的时间
local invasionDelayTime = 1 * 60 * 1000  -- 蝙蝠魔船只接近的时间延迟
local invasionDelay = 5 * 1000  -- 生成蝙蝠魔的时间延迟

-- 初始化函数
function init()
    -- 初始化时间
    closeTime = em:getTransportationTime(closeTime)
    beginTime = em:getTransportationTime(beginTime)
    rideTime = em:getTransportationTime(rideTime)
    invasionStartTime = em:getTransportationTime(invasionStartTime)
    invasionDelayTime = em:getTransportationTime(invasionDelayTime)

    -- 获取地图实例
    Orbis_btf = em:GetMap(200000112)  -- 候船室<开往魔法密林>
    Ellinia_btf = em:GetMap(101000301)  -- 候船室<开往天空之城>
    Boat_to_Orbis = em:GetMap(200090010)  -- 开往天空之城
    Boat_to_Ellinia = em:GetMap(200090000)  -- 开往魔法密林
    Orbis_Boat_Cabin = em:GetMap(200090011)  -- 船仓<开往天空之城>
    Ellinia_Boat_Cabin = em:GetMap(200090001)  -- 船仓<开往魔法密林>
    Ellinia_docked = em:GetMap(101000300)  -- 魔法密林码头
    Orbis_Station = em:GetMap(200000100)  -- 天空之城售票处
    Orbis_docked = em:GetMap(200000111)  -- 码头<开往魔法密林>

    -- 设置码头状态为已停靠
    Ellinia_docked:setDocked(true)
    Orbis_docked:setDocked(true)

    -- 安排新的周期性任务
    scheduleNew()
end

function scheduleNew()
    -- 设置属性，并安排关闭入口和起飞的任务
    em:setProperty("docked", "true")
    em:setProperty("entry", "true")
    em:setProperty("haveBalrog", "false")

    -- 安排关闭入口和起飞的时间点
    em:schedule("stopentry", closeTime)
    em:schedule("takeoff", beginTime)
end

function stopentry()
    -- 关闭入口后清除船舱内的对象（例如箱子）
    em:setProperty("entry", "false")
    em:setProperty("next", os.time() * 1000 + em:getTransportationTime(beginTime - closeTime + rideTime))
    Orbis_Boat_Cabin:clearMapObjects()
    Ellinia_Boat_Cabin:clearMapObjects()
end

function takeoff()
    -- 玩家被传送至船上，广播船只离开的消息
    Orbis_btf:warpEveryone(Boat_to_Ellinia:getId())
    Ellinia_btf:warpEveryone(Boat_to_Orbis:getId())
    Ellinia_docked:broadcastShip(false)  -- 广播魔法密林船只离开消息
    Orbis_docked:broadcastShip(false)   -- 广播天空之城船只离开消息

    -- 设置码头状态为未停靠
    em:setProperty("docked", "false")

    -- 随机决定是否会有蝙蝠魔船只接近
    if math.random() < 0.42 then
        em:schedule("approach", invasionStartTime + math.floor(math.random() * invasionDelayTime))
    end

    -- 安排到达目的地的时间点
    em:schedule("arrived", rideTime)
end

function arrived()
    -- 玩家到达目的地后被传送至对应站点或码头
    Boat_to_Orbis:warpEveryone(Orbis_Station:getId(), 0)
    Orbis_Boat_Cabin:warpEveryone(Orbis_Station:getId(), 0)
    Boat_to_Ellinia:warpEveryone(Ellinia_docked:getId(), 1)
    Ellinia_Boat_Cabin:warpEveryone(Ellinia_docked:getId(), 1)

    -- 播放船只到达的消息并重置蝙蝠魔状态
    Orbis_docked:broadcastShip(true)
    Ellinia_docked:broadcastShip(true)
    Boat_to_Orbis:broadcastEnemyShip(false)
    Boat_to_Ellinia:broadcastEnemyShip(false)
    Boat_to_Orbis:killAllMonsters()
    Boat_to_Ellinia:killAllMonsters()
    em:setProperty("haveBalrog", "false")

    -- 安排下一个周期性任务
    scheduleNew()
end

function approach()
    -- 处理蝙蝠魔船只接近的情况
    if math.floor(math.random() * 10) < 10 then
        em:setProperty("haveBalrog", "true")
        Boat_to_Orbis:broadcastEnemyShip(true)
        Boat_to_Ellinia:broadcastEnemyShip(true)

        -- 更改背景音乐
        Boat_to_Orbis:broadcastMessage(PacketCreator.musicChange("Bgm04/ArabPirate"))
        Boat_to_Ellinia:broadcastMessage(PacketCreator.musicChange("Bgm04/ArabPirate"))

        -- 安排蝙蝠魔出现的时间点
        em:schedule("invasion", invasionDelay)
    end
end

function invasion()
    -- 生成蝙蝠魔
    local map1 = Boat_to_Ellinia
    local pos1 = Point(-538, 143)
    map1:spawnMonsterOnGroundBelow(LifeFactory.getMonster(8150000), pos1)
    map1:spawnMonsterOnGroundBelow(LifeFactory.getMonster(8150000), pos1)

    local map2 = Boat_to_Orbis
    local pos2 = Point(339, 148)
    map2:spawnMonsterOnGroundBelow(LifeFactory.getMonster(8150000), pos2)
    map2:spawnMonsterOnGroundBelow(LifeFactory.getMonster(8150000), pos2)
end

function cancelSchedule()
end

-- ---------- 辅助函数 ----------
function dispose()
end

function setup(eim, leaderid)
end

function monsterValue(eim, mobid)
    return 0
end

function disbandParty(eim, player)
end

function playerDisconnected(eim, player)
end

function playerEntry(eim, player)
end

function monsterKilled(mob, eim)
end

function scheduledTimeout(eim)
end

function afterSetup(eim)
end

function changedLeader(eim, leader)
end

function playerExit(eim, player)
end

function leftParty(eim, player)
end

function clearPQ(eim)
end

function allMonstersDead(eim)
end

function playerUnregistered(eim, player)
end