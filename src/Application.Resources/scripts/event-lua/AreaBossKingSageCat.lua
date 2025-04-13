local Common_AreaBoss = require("scripts/event-lua/__BaseAreaBoss")

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
    local posX = math.floor(math.random(1300) - 500)
    local posY = 540
    Common_AreaBoss.spawnBoss(
        250010504,          -- 地图ID
        7220002,            -- 怪物ID 妖怪禅师
        posX,               -- X坐标
        posY,               -- Y坐标
        "周围的鬼气更加浓重了。传来一阵令人不快的猫叫声。"  -- 提示消息
    )
end