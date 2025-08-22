using Application.Core.Channel.ResourceTransaction.Handlers;
using Application.Core.model;
using client.inventory;
using System.Collections.Concurrent;
using tools;

namespace Application.Core.Channel.ResourceTransaction
{
    public class ResourceConsumeRequest
    {
        public int CostMeso { get; set; }
        public int GainMeso { get; set; }
        public List<ItemObjectQuantity> CostItems { get; set; } = [];
        public List<Item> GainItems { get; set; } = [];
        public List<ItemQuantity> CostItemsById { get; set; } = [];
        public List<ItemQuantity> GainItemsById { get; set; } = [];
        public int CostNxPrepaid { get; set; }
        public int GainNxPrepaid { get; set; }
        public int CostMaplePoint { get; set; }
        public int GainMaplePoint { get; set; }
        public int CostNxCredit { get; set; }
        public int GainNxCredit { get; set; }
    }

    public class ResourceConsumeBuilder
    {
        private ResourceConsumeRequest _request = new();

        public ResourceConsumeBuilder ConsumeMeso(int amount)
        {
            _request.CostMeso = amount;
            return this;
        }

        public ResourceConsumeBuilder GainMeso(int amount)
        {
            _request.GainMeso = amount;
            return this;
        }

        public ResourceConsumeBuilder ConsumeItem(int itemId, int quantity)
        {
            _request.CostItemsById.Add(new ItemQuantity(itemId, quantity));
            return this;
        }

        public ResourceConsumeBuilder GainItem(int itemId, int quantity)
        {
            _request.GainItemsById.Add(new ItemQuantity(itemId, quantity));
            return this;
        }

        public ResourceConsumeBuilder ConsumeItem(Item item, int quantity)
        {
            _request.CostItems.Add(new ItemObjectQuantity(item, quantity));
            return this;
        }

        public ResourceConsumeBuilder GainItem(Item item)
        {
            _request.GainItems.Add(item);
            return this;
        }

        public ResourceConsumeBuilder ConsumeCash(int cashType, int amount)
        {
            if (cashType == CashType.NX_CREDIT)
                _request.CostNxCredit = amount;
            if (cashType == CashType.NX_PREPAID)
                _request.CostNxPrepaid = amount;
            if (cashType == CashType.MAPLE_POINT)
                _request.CostMaplePoint = amount;
            return this;
        }

        public ResourceConsumeBuilder GainCash(int cashType, int amount)
        {
            if (cashType == CashType.NX_CREDIT)
                _request.GainNxCredit = amount;
            if (cashType == CashType.NX_PREPAID)
                _request.GainNxPrepaid = amount;
            if (cashType == CashType.MAPLE_POINT)
                _request.GainMaplePoint = amount;
            return this;
        }

        public bool Execute(IPlayer player, Func<CancellationToken, bool> businessLogic, bool showMessage = false, bool showMessageInChat = false)
        {
            var res = ResourceManager.ConsumeResources(player, _request, businessLogic);

            if (res && showMessage)
            {
                if (_request.CostMeso != 0)
                {
                    player.sendPacket(PacketCreator.getShowMesoGain(_request.CostMeso, showMessageInChat));
                }

                if (_request.GainMeso != 0)
                {
                    player.sendPacket(PacketCreator.getShowMesoGain(_request.GainMeso, showMessageInChat));
                }

                foreach (var item in _request.CostItems)
                {
                    player.sendPacket(PacketCreator.getShowItemGain(item.Item.getItemId(), (short)-item.Quantity, showMessageInChat));
                }

                foreach (var item in _request.GainItems)
                {
                    player.sendPacket(PacketCreator.getShowItemGain(item.getItemId(), (short)item.getQuantity(), showMessageInChat));
                }
            }
            return res;
        }
    }

    public class ResourceManager
    {
        static ConcurrentDictionary<int, ResourceTransactionManager> RunningTransactions { get; set; } = new();
        public static bool ConsumeResources(IPlayer player, ResourceConsumeRequest request, Func<CancellationToken, bool> businessLogic)
        {
            if (RunningTransactions.ContainsKey(player.Id))
            {
                player.dropMessage("正在处理请求");
                return false;
            }

            try
            {
                var tx = new ResourceTransactionManager();
                RunningTransactions[player.Id] = tx;

                if (request.CostMeso > 0)
                    tx.AddResourceHandler(new PlayerCostMesoHandler(player, request.CostMeso));
                if (request.GainMeso > 0)
                    tx.AddResourceHandler(new PlayerGainMesoHandler(player, request.GainMeso));

                foreach (var item in request.CostItemsById)
                    tx.AddResourceHandler(new PlayerCostItemIdHandler(player, item));
                foreach (var item in request.GainItemsById)
                    tx.AddResourceHandler(new PlayerGainItemIdHandler(player, item));

                foreach (var item in request.CostItems)
                    tx.AddResourceHandler(new PlayerCostItemHandler(player, item));
                foreach (var item in request.GainItems)
                    tx.AddResourceHandler(new PlayerGainItemHandler(player, item));

                if (request.CostNxPrepaid > 0)
                    tx.AddResourceHandler(new PlayerCostCashHandler(player, CashType.NX_PREPAID, request.CostNxPrepaid));
                if (request.GainNxPrepaid > 0)
                    tx.AddResourceHandler(new PlayerGainCashHandler(player, CashType.NX_PREPAID, request.GainNxPrepaid));

                if (request.CostNxCredit > 0)
                    tx.AddResourceHandler(new PlayerCostCashHandler(player, CashType.NX_CREDIT, request.CostNxCredit));
                if (request.GainNxCredit > 0)
                    tx.AddResourceHandler(new PlayerGainCashHandler(player, CashType.NX_CREDIT, request.GainNxCredit));

                if (request.CostMaplePoint > 0)
                    tx.AddResourceHandler(new PlayerCostCashHandler(player, CashType.MAPLE_POINT, request.CostMaplePoint));
                if (request.GainMaplePoint > 0)
                    tx.AddResourceHandler(new PlayerGainCashHandler(player, CashType.MAPLE_POINT, request.GainMaplePoint));

                return tx.ExecuteTransaction(businessLogic);
            }
            finally
            {
                RunningTransactions.TryRemove(player.Id, out _);
            }
        }

        public static void Cancel(IPlayer chr)
        {
            if (RunningTransactions.TryRemove(chr.Id, out var tsc))
                tsc.Cancel();
        }

        public static void Cancel()
        {
            foreach (var tx in RunningTransactions.Values)
            {
                tx.Cancel();
            }
            RunningTransactions.Clear();
        }
    }
}
