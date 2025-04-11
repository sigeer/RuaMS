local Common_AreaBoss = require("scripts/event/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 只需要重写 start 方法
function start()
    local posX = 208
    local posY = 83
    Common_AreaBoss.spawnBoss(
        200010300,          -- 地图ID
        8220000,            -- 怪物ID 艾利杰
        posX,               -- X坐标
        posY,               -- Y坐标
        "Eliza has appeared with a black whirlwind."  -- 提示消息
    )
end