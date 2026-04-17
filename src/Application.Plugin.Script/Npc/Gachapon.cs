using Application.Core.Models;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        // Npc: 9100100 
        public Task gachapon1()
        {
            return GachaponNormal();
        }

        public async Task GachaponNormal()
        {
            var ticketId = 5220000;

            var curMapName = GetGachaponMapName();

            var option = await SayOption($"欢迎来到${curMapName}扭蛋机。我可以为您做些什么呢？", [
                "什么是扭蛋机？",
                $"在哪里可以购买#t${ticketId}",
                $"使用1张#t${ticketId}",
                $"使用10张#t${ticketId}",
                "查看我的#r奖品仓库"
            ]);

            switch (option)
            {
                case 0:
                    await SaySpeech([
                        $"玩转扭蛋机，赢得稀有卷轴、装备、椅子、熟练书和其他酷炫物品！你只需要一张 #i${ticketId}##b#t${ticketId}##k 就有机会成为随机物品的幸运获得者。",
                        $"你会在" + curMapName + "的扭蛋机中找到各种物品，但最有可能找到与" + curMapName + "相关的物品和卷轴。"
                        ]);
                    break;
                case 1:
                    await SayNext($"#i${ticketId}##b#t${ticketId}##k 可以在#r现金商店#k使用NX或枫叶点购买。点击屏幕右下角的红色商店图标访问#r现金商店#k。");
                    break;
                case 2:
                    await DoGachapon(ticketId, 1);
                    break;
                case 3:
                    await DoGachapon(ticketId, 10);
                    break;
                case 4:
                    OpenGachaponStorage();
                    break;
                default:
                    break;
            }
        }

        async Task DoGachapon(int ticketId, int count)
        {
            if (!haveItem(ticketId, count))
            {
                await SayOK(GetTalkMessage("Tip_CheckItemWithId", ticketId, count));
                return;
            }
            if (!CheckGachaponStorage(count))
            {
                await SayOK(GetTalkMessage("Storage_CheckGachaponStorage", count));
                return;
            }

            gainItem(ticketId, -count);
            List<GachaponPoolItemDataObject> rewards = [];
            for (var i = 0; i < count; i++)
            {
                rewards.Add(doGachapon());
            }

            await SaySpeech(rewards.Select(itemObj => itemObj == null ? GetTalkMessage("Tip_ThankPatronage") : GetTalkMessage("Tip_ObtainItem", itemObj.ItemId, itemObj.Quantity)).ToArray());
        }

        // Npc: 9100101 
        public Task gachapon2()
        {
            return GachaponNormal();
        }


        // Npc: 9100102, 9110011 
        public Task gachapon3()
        {
            return GachaponNormal();
        }


        // Npc: 9100103, 9110012 
        public Task gachapon4()
        {
            return GachaponNormal();
        }


        // Npc: 9100104, 9110013 
        public Task gachapon5()
        {
            return GachaponNormal();
        }


        // Npc: 9100105, 9110014 
        public Task gachapon6()
        {
            return GachaponNormal();
        }


        // Npc: 9100106, 9110016 
        public Task gachapon7()
        {
            return GachaponNormal();
        }


        // Npc: 9100117, 9310092 
        public Task gachapon18()
        {
            return GachaponNormal();
        }

        #region MyRegion
        // Npc: 9050000, 9100107, 9110017, 9310023 
        public Task gachapon8()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050001, 9100108, 9100111, 9310024 
        public Task gachapon9()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050002, 9100109, 9310025 
        public Task gachapon10()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050003, 9100110, 9310026 
        public Task gachapon11()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050004, 9310027 
        public Task gachapon12()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050005, 9100112, 9270043, 9310028 
        public Task gachapon13()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050006, 9310029 
        public Task gachapon14()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050007, 9310061 
        public Task gachapon15()
        {
            // TODO
            return Task.CompletedTask;
        }
        #endregion
    }
}
