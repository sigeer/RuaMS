function hasProofOfLoves(player)
    local count = 0

    for i = 4031367, 4031372 do
        if (player.haveItem(i)) then
            count++
        end
    end

    return count >= 4
end

function start() {
    levelMain()
}

function levelMain()
    if not cm:isQuestStarted(100400) then
        cm:sendOkLevel("Dispose", "你好，我们是爸爸和妈妈...")
        return
    end

    if cm:getQuestProgressInt(100400, 1) == 0 then
        cm:sendNextLevel("Conversation1", "妈妈，爸爸，我有一个请求要向你们两个提出……我想更多地了解你们一直走过的道路，那条爱和关心我所珍爱的人的道路。")
        return
    end

    if not hasProofOfLoves(cm:getPlayer()) then
        cm:sendOkLevel("Dispose", "亲爱的，我们需要确保你真的准备好爱上你选择的伴侣，请带来 #b4#k个 #b#t4031367##k。")
        return
    end

    cm:sendNextLevel("Finish", "#b#h0##k，你今天让我们感到骄傲。你现在可以选择任何人作为你的未婚夫，接受我们的祝福。你现在可以咨询#p9201000#，婚礼珠宝商。祝你前程坦途，充满爱和关怀~~")
end

function levelConversation1()
    cm:sendNextPrevLevel("Main", "Conversation2", "亲爱的！你真体贴，向我们求助。我们一定会帮助你的！")
end

function levelConversation2()
    cm:sendNextPrevLevel("Conversation1", "Conversation3", "当然，你一定已经在冒险岛世界中见过爱之仙子#rNanas#k了。从她们那里收集#b4个#t4031367#，然后带到这里来。这次旅程将解答你对爱情的一些疑问……")
end

function levelConversation3()
    cm:setQuestProgress(100400, 1, 1)
    cm:dispose()
end

function levelFinish()
    cm:sendOkLevel("Dispose", "妈妈...爸爸...非常感谢你们的温柔支持！！！")

    cm:completeQuest(100400)
    cm:gainExp(20000 * cm:getPlayer().getExpRate())
    for i = 4031367, 4031372 do
        cm:removeAll(i)
    end

end
