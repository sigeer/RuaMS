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
    local posX = math.floor(math.random(800) + 400)
    local posY = 1280
    Common_AreaBoss.spawnBoss(
        101030404,          -- 地图ID
        3220000,            -- 怪物ID 树妖王
        posX,               -- X坐标
        posY,               -- Y坐标
        "Stumpy has appeared with a stumping sound that rings the Stone Mountain."  -- 提示消息
    )
end