local Common_AreaBoss = {}

-- 基础属性
local setupTask = nil

-- 基础方法
function Common_AreaBoss.init()
    scheduleNew()
end

function Common_AreaBoss.scheduleNew()
    setupTask = em:schedule("start", 0)    --在服务器启动时生成。每3小时服务器事件会检查Boss是否存在，如果不存在则立即生成。
end

function Common_AreaBoss.cancelSchedule()
    if setupTask ~= nil then
        setupTask:cancel(true)
    end
end

-- 填充函数
function Common_AreaBoss.dispose() end

function Common_AreaBoss.setup(eim, leaderid) end

function Common_AreaBoss.monsterValue(eim, mobid) return 0 end

function Common_AreaBoss.disbandParty(eim, player) end

function Common_AreaBoss.playerDisconnected(eim, player) end

function Common_AreaBoss.playerEntry(eim, player) end

function Common_AreaBoss.monsterKilled(mob, eim) end

function Common_AreaBoss.scheduledTimeout(eim) end

function Common_AreaBoss.afterSetup(eim) end

function Common_AreaBoss.changedLeader(eim, leader) end

function Common_AreaBoss.playerExit(eim, player) end

function Common_AreaBoss.leftParty(eim, player) end

function Common_AreaBoss.clearPQ(eim) end

function Common_AreaBoss.allMonstersDead(eim) end

function Common_AreaBoss.playerUnregistered(eim, player) end

-- 辅助方法


function Common_AreaBoss.spawnBoss(mapId, monsterId, posX, posY, noticeMsg)
    local map = em:getChannelServer():getMapFactory():getMap(mapId)

    if map:getMonsterById(monsterId) ~= nil then
        em:schedule("start", 3 * 60 * 60 * 1000)
        return
    end

    local spawnpoint = Point(posX, posY)
    local monster = LifeFactory.getMonster(monsterId)
    map:spawnMonsterOnGroundBelow(monster, spawnpoint)
    map:broadcastMessage(PacketCreator.serverNotice(6, noticeMsg))
    em:schedule("start", 3 * 60 * 60 * 1000)
end

return Common_AreaBoss