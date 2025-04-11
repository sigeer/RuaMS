local Common_AreaBoss = require("scripts/event/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 重写 init 方法
function init()
    scheduleNew()
end

-- 重写 start 方法
function start()
    local map = em:getChannelServer():getMapFactory():getMap(105090310)

    if map:getMonsterById(8220008) ~= nil or map:getMonsterById(8220009) ~= nil then
        em:schedule("start", 3 * 60 * 60 * 1000)
        return
    end

    local setPos = {
        { -626, -604 },
        { 735, -600 }
    }
    
    local index = math.random(1, #setPos)
    local rndPos = setPos[index]
    
    local monster = LifeFactory.getMonster(8220008)
    map:spawnMonsterOnGroundBelow(monster, Point(rndPos[1], rndPos[2]))
    map:broadcastMessage(PacketCreator.serverNotice(6, "一个可疑的小吃摊在一个奇怪的偏僻地方慢慢开张了。"))
    em:schedule("start", 3 * 60 * 60 * 1000)
end