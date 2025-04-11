local Common_AreaBoss = require("scripts/event/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 只需要重写 start 方法
function start()
    local posX = 474
    local posY = 278
    Common_AreaBoss.spawnBoss(
        100040106,          -- 地图ID
        5220002,            -- 怪物ID 浮士德
        posX,               -- X坐标
        posY,               -- Y坐标
        "浮士德出现在蓝色迷雾中。"  -- 提示消息
    )
end