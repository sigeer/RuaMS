local Common_AreaBoss = require("scripts/event-lua/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 重写 start 方法
function start()
    local posX = 842
    local posY = 0
    Common_AreaBoss.spawnBoss(
        677000012,          -- 地图ID
        9400633,            -- 怪物ID 地狱大公
        posX,               -- X坐标
        posY,               -- Y坐标
        "Astaroth has appeared!"  -- 提示消息
    )
end