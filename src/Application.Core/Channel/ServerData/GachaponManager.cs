using Application.Core.Channel.DataProviders;
using Application.Core.Models;
using Application.Core.ServerTransports;
using Application.Core.tools.RandomUtils;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Application.Core.Channel.ServerData
{
    public class GachaponManager : DataBootstrap
    {
        /// <summary>
        /// poolId
        /// </summary>
        private List<int> _globalPoolIdList;
        public GachaponManager(IMapper mapper, IChannelServerTransport transport, ILogger<DataBootstrap> logger) : base(logger)
        {
            Source = "DB";
            Name = "扭蛋机";

            _cachedPool = [];
            _cachedItems = [];

            _cachedChance = [];
            _globalPoolIdList = [];

            _mapper = mapper;
            _transport = transport;
        }

        private List<GachaponPoolLevelChanceDataObject> _cachedChance;
        private List<GachaponPoolItemDataObject> _cachedItems;

        /// <summary>
        /// npcid - pool
        /// </summary>
        Dictionary<int, GachaponDataObject> _cachedPool;

        readonly IMapper _mapper;
        readonly IChannelServerTransport _transport;

        protected override void LoadDataInternal()
        {
            var data = _transport.GetGachaponData();
            _cachedPool = _mapper.Map<List<GachaponDataObject>>(data.Pools).ToDictionary(x => x.NpcId);
            _cachedChance = _mapper.Map<List<GachaponPoolLevelChanceDataObject>>(data.Chances);
            _cachedItems = _mapper.Map<List<GachaponPoolItemDataObject>>(data.Items);

            _globalPoolIdList = _cachedPool.Where(x => x.Key <= 0).Select(x => x.Value.Id).ToList();
        }

        public void OnDataReset(ItemProto.GachaponResetBroadcast data)
        {
            Reload();
        }

        public List<GachaponPoolItemDataObject> GetItems(int poolId, int level)
        {
            return _cachedItems.Where(x => x.Level == level).Where(x => x.PoolId == poolId || _globalPoolIdList.Contains(x.PoolId)).ToList();
        }

        public GachaponPoolItemDataObject DoGachapon(int npcId)
        {
            var pool = GetByNpcId(npcId);
            if (pool == null)
                throw new BusinessResException($"没有找到NPC（{npcId}）的扭蛋机数据");

            var machine = new LotteryMachine<int>(_cachedChance.Where(x => x.PoolId == pool.Id).Select(x => new LotteryMachinItem<int>(x.Level, x.Chance)));
            var level = machine.GetRandomItem();
            return Randomizer.Select(GetItems(pool.Id, level));
        }

        public GachaponDataObject? GetByNpcId(int npcId)
        {
            return _cachedPool.GetValueOrDefault(npcId);
        }

        public List<GachaponPoolLevelChanceDataObject> GetPoolLevelList(int poolId)
        {
            return _cachedChance.Where(x => x.PoolId == poolId).ToList();
        }

        public List<GachaponDataObject> GetGachaponType()
        {
            return _cachedPool.Where(x => x.Key > 0).Select(x => x.Value).ToList();
        }

        public string[] GetLootInfo()
        {
            ItemInformationProvider ii = ItemInformationProvider.getInstance();

            var allGachaponType = GetGachaponType();
            string[] strList = new string[allGachaponType.Count + 1];

            string menuStr = "";
            int j = 0;
            foreach (var gacha in allGachaponType)
            {
                menuStr += "#L" + j + "#" + gacha.Name + "#l\r\n";
                j++;

                string str = "";
                foreach (var chance in GetPoolLevelList(gacha.Id))
                {
                    var gachaItems = GetItems(gacha.Id, chance.Level);

                    if (gachaItems.Count > 0)
                    {
                        str += "  #rTier " + chance.Level + "#k:\r\n";
                        foreach (var item in gachaItems)
                        {
                            var itemName = ii.getName(item.ItemId);
                            if (itemName == null)
                            {
                                itemName = "MISSING NAME #" + item.ItemId;
                            }

                            str += "    " + itemName + "\r\n";
                        }

                        str += "\r\n";
                    }
                }
                str += "\r\n";

                strList[j] = str;
            }
            strList[0] = menuStr;

            return strList;
        }

    }
}
