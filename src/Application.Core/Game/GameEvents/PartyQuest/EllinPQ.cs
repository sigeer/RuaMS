using Application.Core.Managers;
using client.inventory;
using client.inventory.manipulator;
using scripting.Event;

namespace Application.Core.Game.GameEvents.PartyQuest
{
    public class EllinPQ : PlayerPartyQuestBase
    {
        public EllinPQ(IPlayer player) : base("EllinPQ", "", player)
        {
            EntryNpcId = 2133000;
        }

        public override int GetStageFromMap(int mapId) => (mapId % 1000) / 100;
        public override int ClearMapId => 930000600;

        protected override void PassStage(EventInstanceManager eim, int curStg, int curMapId)
        {
            // 第1关打怪，传送点过关 party6_stage
            if (curMapId == 930000100)
            {
                AdminManager.KillAllMonster(Player);
            }
            // 第2关放4个道具，传送点过关 party6_stage
            if (curMapId == 930000200)
            {
                var mapSpine = Player.getMap().getReactorByName("spine");
                mapSpine?.setState(4);
            }
            // 第3关迷宫传送
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
                // 4001163
                InventoryManipulator.addFromDrop(Player.Client, new Item(4001163, 0, 1));
            }
            // 第6关BOSS击杀
            if (curMapId == 930000600)
            {
                AdminManager.KillAllMonster(Player);
            }
            eim.showClearEffect(curMapId);
        }
    }
}
