local timer1
local timer2
local timer3
local timer4

function init()
    --[[
        if(em:getChannelServer():getId() == 1) { -- Only run on channel 1:
        -- AEST
        timer1 = em:scheduleAtTimestamp("start", 1428220800000)
        timer2 = em:scheduleAtTimestamp("stop", 1428228000000)
        -- EDT
        timer1 = em:scheduleAtTimestamp("start", 1428271200000)
        timer2 = em:scheduleAtTimestamp("stop", 1428278400000)
    }
    --]]
end

function cancelSchedule()
    if (timer1 ~= nil) then
        timer1:cancel(true)
    end
    if (timer2 ~= nil) then
        timer2:cancel(true)
    end
    if (timer3 ~= nil) then
        timer3:cancel(true)
    end
    if (timer4 ~= nil) then
        timer4:cancel(true)
    end
end

function start()
    local world = em:getWorldServer()
    world:setExpRate(8)
    world:broadcastPacket(PacketCreator.serverNotice(6, "兔子猛攻生存扫描仪（BOSS）已检测到复活节兔子即将发动猛攻！GM团队已激活紧急经验池（EXP），在接下来的 2 小时内，玩家获得的经验将翻倍！"))
end

function stop()
    local world = em:getWorldServer()
    world:setExpRate(4)
    world:broadcastPacket(PacketCreator.serverNotice(6, "紧急经验池（EXP）目前能量已经耗尽，需要重新充能，经验倍率恢复正常。"))
end

-- ---------- FILLER FUNCTIONS ----------
function dispose()
end

function setup(eim, leaderid)
end

function monsterValue(eim, mobid)
    return 0
end

function disbandParty(eim, player)
end

function playerDisconnected(eim, player)
end

function playerEntry(eim, player)
end

function monsterKilled(mob, eim)
end

function scheduledTimeout(eim)
end

function afterSetup(eim)
end

function changedLeader(eim, leader)
end

function playerExit(eim, player)
end

function leftParty(eim, player)
end

function clearPQ(eim)
end

function allMonstersDead(eim)
end

function playerUnregistered(eim, player)
end