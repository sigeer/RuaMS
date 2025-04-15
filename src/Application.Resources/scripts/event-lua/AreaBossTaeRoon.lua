local Common_AreaBoss = require("scripts/event-lua/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 重写 start 方法
function start()
    local posX = math.floor(math.random(700) - 800)
    local posY = 390
    Common_AreaBoss.spawnBoss(
        250010304,          -- 地图ID
        7220000,            -- 怪物ID 肯德熊
        posX,               -- X坐标
        posY,               -- Y坐标
        "Tae Roon has appeared with a soft whistling sound."  -- 提示消息
    )
end