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
    local posX = math.floor(math.random(1300) - 800)
    local posY = 33
    Common_AreaBoss.spawnBoss(
        222010310,          -- 地图ID
        7220001,            -- 怪物ID 九尾狐
        posX,               -- X坐标
        posY,               -- Y坐标
        "As the moon light dims, a long fox cry can be heard and the presence of the old fox can be felt"  -- 提示消息
    )
end