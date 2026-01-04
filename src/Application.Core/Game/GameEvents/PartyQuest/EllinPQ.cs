using Application.Core.Managers;
using Application.Core.Scripting.Events;
using client.inventory;
using client.inventory.manipulator;
using scripting.Event;

namespace Application.Core.Game.GameEvents.PartyQuest
{
    public class EllinPQ : PlayerPartyQuestBase
    {
        public EllinPQ(Player player) : base("EllinPQ", "", player)
        {
            EntryNpcId = 2133000;
        }

        public override int GetStageFromMap(int mapId) => (mapId % 1000) / 100;
        public override int ClearMapId => 930000600;

        protected override void PassStage(AbstractEventInstanceManager eim, int curStg, int curMapId)
        {
            // 第1关打怪，传送点过关 party6_stage
            if (curMapId == 930000100)
            {
                AdminManager.KillAllMonster(Player);
            }
            // 第2关，在reactor=3009000 放4个道具 4001162 * 4 ，传送点过关 party6_stage
            if (curMapId == 930000200)
            {
                var mapSpine = Player.getMap().getReactorByName("spine");
                mapSpine?.setState(4);
            }
            // 第3关迷宫传送
            if (curMapId == 930000300)
            {
                Player.changeMap(930000300, "16st");
            }
            // 第4关抓20个怪，NPC过关 2133001 需要道具 4001169 * 20
            if (curMapId == 930000400)
            {
                InventoryManipulator.addFromDrop(Player.Client, new Item(4001169, 0, 20));
                // 也可以直接传送过关
                // eim.warpEventTeam(930000500);
            }
            // 第5关跳跳，NPC对话过关 2133004 需要道具 4001163 * 1
            if (curMapId == 930000500)
            {
                InventoryManipulator.addFromDrop(Player.Client, new Item(4001163, 0, 1));
            }
            // 第6关放置物品后，触发reactor=3001000，召唤BOSS并击杀，传送点过关 party6_out
            if (curMapId == 930000600)
            {
                // 正常来说需要击杀怪物monsterKilled，然后触发clearPQ，此处由于ClearMapId强制调用了clearPQ
            }
            // 930000700：？
            // 930000800：任务完成的奖励地图 
        }
    }
}
