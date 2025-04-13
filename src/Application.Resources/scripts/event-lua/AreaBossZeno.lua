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
    local posX = -4224
    local posY = 776
    Common_AreaBoss.spawnBoss(
        221040301,          -- 地图ID
        6220001,            -- 怪物ID 朱诺
        posX,               -- X坐标
        posY,               -- Y坐标
        "Zeno has appeared with a heavy sound of machinery."  -- 提示消息
    )
end