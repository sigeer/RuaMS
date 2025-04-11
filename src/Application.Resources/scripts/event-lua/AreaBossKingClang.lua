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
    local posX = math.floor(math.random(2400) - 1600)
    local posY = 140
    Common_AreaBoss.spawnBoss(
        110040000,          -- 地图ID
        5220001,            -- 怪物ID
        posX,               -- X坐标
        posY,               -- Y坐标
        "一个奇怪的海螺出现在了沙滩上。"  -- 提示消息
    )
end