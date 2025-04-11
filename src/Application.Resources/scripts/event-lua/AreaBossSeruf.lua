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
    local posX = math.floor(math.random(2300) - 1500)
    local posY = 520
    Common_AreaBoss.spawnBoss(
        230020100,          -- 地图ID
        4220001,            -- 怪物ID 歇尔夫
        posX,               -- X坐标
        posY,               -- Y坐标
        "A strange shell has appeared from a grove of seaweed"  -- 提示消息
    )
end