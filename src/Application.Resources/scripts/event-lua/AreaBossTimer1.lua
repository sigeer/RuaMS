local Common_AreaBoss = require("scripts/event-lua/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 重写 start 方法
function start()
    local posX = math.floor(math.random(770) - 770)
    local posY = 1030
    Common_AreaBoss.spawnBoss(
        220050100,          -- 地图ID
        5220003,            -- 怪物ID 提莫
        posX,               -- X坐标
        posY,               -- Y坐标
        "Tick-Tock Tick-Tock! Timer makes it's presence known."  -- 提示消息
    )
end