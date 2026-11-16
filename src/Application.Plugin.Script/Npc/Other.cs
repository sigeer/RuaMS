
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {

        // Npc: 9209001 
        public async Task MapleMarket7_Enter()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Npc9209001_SevenDayMarketUnavailable));
        }


        // Npc: 9209000 
        public async Task dealerA()
        {
            var greeting = this.GetTalkMessage(ScriptMessage.Npc9209000_Greeting, npc);

            if (getPlayer().isCygnus())
            {
                await SayOK(greeting + this.GetTalkMessage(ScriptMessage.Npc9209000_NoBookCygnus));
                return;
            }

            var jobrank = getJobId() % 10;
            if (jobrank < 2)
            {
                await SayOK(greeting + this.GetTalkMessage(ScriptMessage.Npc9209000_TrainMore));
                return;
            }

            var skillbook = getAvailableSkillBooks();
            var masterybook = getAvailableMasteryBooks();

            if (skillbook.Length == 0 && masterybook.Length == 0)
            {
                await SayOK(greeting + this.GetTalkMessage(ScriptMessage.Npc9209000_NoBooks));
                return;
            }

            int selected = 0;
            if (skillbook.Length > 0 && masterybook.Length > 0)
            {
                selected = await AskMenu(greeting + this.GetTalkMessage(ScriptMessage.Npc9209000_BookMenu));
            }
            else if (skillbook.Length > 0)
            {
                selected = 1;
                await SayNext(greeting + this.GetTalkMessage(ScriptMessage.Npc9209000_OnlySkillBooks));
            }
            else
            {
                selected = 2;
                await SayNext(greeting + this.GetTalkMessage(ScriptMessage.Npc9209000_OnlyMasteryBooks));
            }

            var table = selected == 1 ? skillbook : masterybook;
            var sendStr = this.GetTalkMessage(ScriptMessage.Npc9209000_AvailableBooks);
            for (int i = 0; i < table.Length; i++)
            {
                if (table[i] > 0)
                {
                    var itemid = table[i];
                    sendStr += $"  #L{i}# #i{itemid}#  #t{itemid}##l\r\n";
                }
                else
                {
                    var skillid = -table[i];
                    sendStr += $"  #L{i}# #s{skillid}#  #q{skillid}##l\r\n";
                }
            }

            var itemSel = await AskMenu(sendStr);
            var chosen = table[itemSel];

            var resultStr = "";
            if (chosen > 0)
            {
                var mobList = getNamesWhoDropsItem(chosen);
                if (mobList.Length == 0)
                {
                    resultStr = this.GetTalkMessage(ScriptMessage.Npc9209000_NoDropMonster) + chosen;
                }
                else
                {
                    resultStr = this.GetTalkMessage(ScriptMessage.Npc9209000_DropMonsters) + chosen;
                    foreach (var mob in mobList)
                    {
                        resultStr += $"  #r{mob}#k\r\n";
                    }
                    resultStr += "\r\n\r\n";
                }
            }

            resultStr += getSkillBookInfo(chosen);
            await SayNext(resultStr);
        }

        // Npc: 9209100 
        public async Task xmas_party_ent()
        {
            var chimney = getMap().getPortal("chimney01");
            if (chimney != null)
            {
                var plyPos = getPlayer().getPosition();
                var portPos = chimney.getPosition();
                var dist = Math.Sqrt(Math.Pow(portPos.X - plyPos.X, 2) + Math.Pow(portPos.Y - plyPos.Y, 2));
                if (dist < 77)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc9209100_NoSneaking));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc9209100_HappyNewYear));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9209100_HappyNewYear));
            }
        }

        // Npc: 9220004 
        public async Task wxmasB()
        {
            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc9220004_MaplemasMenu));
            if (sel == 0)
            {
                if (getMap().countMonsters() > 1)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc9220004_ClearMobsToSummon));
                    return;
                }
                await getMap().spawnMonsterOnGroundBelow(9500317, 1700, 80);
            }
            else if (sel == 1)
            {
                if (getMap().countMonsters() > 6)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc9220004_TooCrowded));
                    return;
                }
                await getMap().spawnMonsterOnGroundBelow(9500320, 1700, 80);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9220004_Ok));
            }
        }

    }
}
