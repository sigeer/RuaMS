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
    local posX = math.floor(math.random(600) - 300)
    local posY = 1125
    Common_AreaBoss.spawnBoss(
        240040401,          -- 地图ID
        8220003,            -- 怪物ID 大海兽
        posX,               -- X坐标
        posY,               -- Y坐标
        "Leviathan emerges from the canyon and the cold icy wind blows."  -- 提示消息
    )
end