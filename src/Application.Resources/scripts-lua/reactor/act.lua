
function vFlowerItem( idx )
	-- body
	rm:dropItems()
end

function go( map )
	-- body
	if map == 221024400 then
		rm:Pink("差一点就成功了！下次再挑战吧！")
		rm:warp(221024400, 1)
	end
	if map == 280010000 then
		rm:Pink("未知力量将你传送至起始点！")
		rm:warp(280010000, 0)
	end
	if map == 922010201 then
		rm:Pink("未知力量将你卷入致命陷阱！")
		rm:warpMap(922010201)
	end
end

function mBoxItem( idx )
	-- body
	rm:dropItems(true, 2, 8, 15, 1)
end

function BF_item( idx )
	-- body
end

function s4hitmanMap( idx )
	-- body
	rm:warp(910200000, "pt0"..idx)
end

function s4hitmanMob( idx )
	-- body
	rm:spawnMonster(9300091)
end

function s4hitmanItem( idx )
	-- body
	rm:dropItems()
end

function periItem( idx )
	-- body
	rm:dropItems()
end

function EpisodeQuest( idx )
	-- body
	rm:dropItems()
end

function scaScript( idx )
	if idx == 0 then
		if rm:isAllReactorState(1029000, 0x04) then
			rm:killMonster(3230300)
			rm:killMonster(3230301)
			rm:playerMessage(6, "幼魔精灵痛苦哀嚎，身形逐渐消散！")
		end
	end
end

function s4berserkMap( )
    if math.random() > 0.7 then
        rm:dropItems()
    else 
        rm:warp(105090200, 0);
    end
end

function s4berserkItem( ... )
	-- body
	rm:dropItems()
end

function balogItem0( ... )
	-- body
	rm:sprayItems(true, 1, 500, 1000, 15)
end

-- 1058001, 1058003, 1058004, 1058011, 1058013, 1058014
function minibalogSummon( ... )
	-- body
end

-- 1058005
function balogReactor( ... )
	-- body
end

function Easy_balogReactor( ... )
	-- body
end

function coconut( idx )
	-- body
	rm:dropItems()
end

function florinaBox( ... )
	-- body
end

function ntQuest( idx )
	-- body
	if idx == 1 then
		if rm:isQuestStarted(6400) then
			rm:setQuestProgress(6400, 1, 2);
			rm:setQuestProgress(6400, 6401, "q3");
		end

		rm:Pink("Real Bart has been found. Return to Jonathan through the portal.");
	end
	if idx == 2 then
		rm:Pink("Failed to find Bart. Returning to the original location.")
		rm:warp(120000102)
	end
end

function ntItem( idx )
	-- body
	rm:dropItems()
end

function erebItem( idx )
	-- body
	rm:dropItems(true, 2, 8, 12, 2);
end

function rienItem( idx )
	-- body
	rm:dropItems(true, 2, 8, 15)
end

function fgodMob( idx )
	-- body
	if idx == 0 or idx == 1 then
		if (rm.getMap().getSummonState()) then
			local count = Number(rm.getEventInstance().getIntProperty("statusStg7_c"));

			if (count < 7) then
				local nextCount = (count + 1);
				-- 修复召唤出黑花后，禁止再召唤黑花，防止精灵爸爸重复出现
				var monsterId = math.random() >= .6 ? 9300049 : 9300048;
				if(monsterId == 9300049) then
					rm.getMap().allowSummonState(false);
				end
				rm.spawnMonster(monsterId);
				rm.getEventInstance().setProperty("statusStg7_c", nextCount);
			else
				rm.spawnMonster(9300049);
				rm.getMap().allowSummonState(false);
			end
		end
	end
	else if idx >= 2 and idx <= 15 then
		local eim = rm.getEventInstance()

		if eim:getIntProperty("statusStg2") == -1 then
			local rnd = math.random(0, 13)  -- 因为原代码随机0-13
			eim:setProperty("statusStg2", tostring(rnd))
			eim:setProperty("statusStg2_c", "0")
		end

		local limit = tonumber(eim:getIntProperty("statusStg2"))  -- getIntProperty可能返回数字，但为了安全用tonumber
		local count = tonumber(eim:getIntProperty("statusStg2_c"))

		if count >= limit then
			rm:dropItems()
			eim:giveEventPlayersExp(3500)
			eim:setProperty("statusStg2", "1")
			eim:showClearEffect(true)
		else
			count = count + 1
			eim:setProperty("statusStg2_c", tostring(count))
			local nextHashed = (11 * count) % 14
			local nextPos = rm:getMap():getReactorById(2001002 + nextHashed):getPosition()
			rm:spawnMonster(9300040, 1, nextPos)
		end
	end
end

function fgodBoss( ... )
    rm:getMap():killAllMonsters()
    rm:getMap():allowSummonState(false)
    rm:spawnMonster(9300039, 260, 490)
    rm:Pink("As the air on the tower outskirts starts to become more dense, Papa Pixie appears.")
end

function fgodItem( idx )
	if idx == 16 or idx == 17 then
		rm:sprayItems(true, 1, 100, 400, 15)
	else
		rm:dropItems()

		if idx == 2 then
			rm:getEventInstance():setProperty("statusStg7", "1");
		end

		if idx == 13 then
			local eim = rm.getEventInstance()
			if (eim:getProperty("statusStgBonus") != "1") then
				rm:spawnNpc(2013002, Point(46, 840))
				eim:setProperty("statusStgBonus", "1")
			end
		end
	end
end

function fgodNPC( idx )
	if idx == 0 then
		rm:getMap():Pink("光影闪烁间，有人自光芒中现身！")
		rm:spawnNpc(2013001)
	else if idx == 1 then
		rm:spawnNpc(2013002);

		local eim = rm:getEventInstance()
		eim:clearPQ();

		eim.setProperty("statusStg8", "1");
		eim:giveEventPlayersExp(3500);
		eim:showClearEffect(true);

		eim:startEventTimer(5 * 60000); //bonus time
	end
end



function oBoxItem( ... )
	-- body
	rm:dropItems(true, 2, 60, 80)
end

function snowdrop( ... )
	-- body
end

function PB_boxCount( ... )
	-- body
end


function mcGuardian( idx )
	-- body
	rm:dispelAllMonsters(tonumber(rm:getReactor():getName():substring(1, 2)), tonumber(rm:getReactor():getName():substring(0, 1)))
end
