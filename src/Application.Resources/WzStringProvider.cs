using Application.Shared;
using Application.Shared.Constants.Item;
using Application.Shared.Constants.Map;
using Application.Shared.Constants.Npc;
using Application.Shared.Models;
using Application.Utility.Exceptions;
using Dto;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using XmlWzReader;
using XmlWzReader.wz;
using YamlDotNet.Core;

namespace Application.Resources
{
    public class WzStringProvider
    {
        DataProvider stringData;

        #region Items
        protected Data cashStringData;
        protected Data consumeStringData;
        protected Data eqpStringData;
        protected Data etcStringData;
        protected Data insStringData;
        protected Data petStringData;
        #endregion

        #region
        Data mobStringData;
        Data npcStringData;
        #endregion

        Data skillStringData;
        Data mapStringData;

        public WzStringProvider()
        {
            stringData = DataProviderFactory.getDataProvider(WZFiles.STRING);

            cashStringData = stringData.getData("Cash.img");
            consumeStringData = stringData.getData("Consume.img");
            eqpStringData = stringData.getData("Eqp.img");
            etcStringData = stringData.getData("Etc.img");
            insStringData = stringData.getData("Ins.img");
            petStringData = stringData.getData("Pet.img");

            mobStringData = stringData.getData("Mob.img");
            npcStringData = stringData.getData("Npc.img");

            skillStringData = stringData.getData("Skill.img");
            mapStringData = stringData.getData("Map.img");
        }
        bool _isItemLoadAll = false;
        Dictionary<int, ObjectName> _itemNameCache = new();
        public List<ObjectName> GetAllItem()
        {
            if (!_isItemLoadAll)
            {
                foreach (Data itemFolder in cashStringData.getChildren())
                {
                    GetItemNameFromData(itemFolder);
                }

                foreach (Data itemFolder in consumeStringData.getChildren())
                {
                    GetItemNameFromData(itemFolder);
                }
                var itemsData = eqpStringData.getChildByPath("Eqp") ?? throw new BusinessResException("Eqp.img/Epq");
                foreach (Data eqpType in itemsData.getChildren())
                {
                    foreach (Data itemFolder in eqpType.getChildren())
                    {
                        GetItemNameFromData(itemFolder);
                    }
                }
                itemsData = etcStringData.getChildByPath("Etc") ?? throw new BusinessResException("Etc.img/Etc");
                foreach (Data itemFolder in itemsData.getChildren())
                {
                    GetItemNameFromData(itemFolder);
                }
                foreach (Data itemFolder in insStringData.getChildren())
                {
                    GetItemNameFromData(itemFolder);
                }
                foreach (Data itemFolder in petStringData.getChildren())
                {
                    GetItemNameFromData(itemFolder);
                }
                _isItemLoadAll = true;
            }
            return _itemNameCache.Values.ToList();
        }

        ObjectName GetItemNameFromData(Data stringData)
        {
            var itemId = int.Parse(stringData.getName()!);
            var itemName = DataTool.getString("name", stringData) ?? "NO-NAME";
            _itemNameCache[itemId] = new ObjectName(itemId, itemName);
            return new(itemId, itemName ?? "NO-NAME");
        }

        private Data? GetStringDataByItemId(int itemId)
        {
            string cat = "null";
            Data theData;
            if (itemId >= 5010000)
            {
                theData = cashStringData;
            }
            else if (itemId >= 2000000 && itemId < 3000000)
            {
                theData = consumeStringData;
            }
            else if (itemId >= 1010000 && itemId < 1040000 || itemId >= 1122000 && itemId < 1123000 || itemId >= 1132000 && itemId < 1133000 || itemId >= 1142000 && itemId < 1143000)
            {
                theData = eqpStringData;
                cat = "Eqp/Accessory";
            }
            else if (itemId >= 1000000 && itemId < 1010000)
            {
                theData = eqpStringData;
                cat = "Eqp/Cap";
            }
            else if (itemId >= 1102000 && itemId < 1103000)
            {
                theData = eqpStringData;
                cat = "Eqp/Cape";
            }
            else if (itemId >= 1040000 && itemId < 1050000)
            {
                theData = eqpStringData;
                cat = "Eqp/Coat";
            }
            else if (ItemConstants.isFace(itemId))
            {
                theData = eqpStringData;
                cat = "Eqp/Face";
            }
            else if (itemId >= 1080000 && itemId < 1090000)
            {
                theData = eqpStringData;
                cat = "Eqp/Glove";
            }
            else if (ItemConstants.isHair(itemId))
            {
                theData = eqpStringData;
                cat = "Eqp/Hair";
            }
            else if (itemId >= 1050000 && itemId < 1060000)
            {
                theData = eqpStringData;
                cat = "Eqp/Longcoat";
            }
            else if (itemId >= 1060000 && itemId < 1070000)
            {
                theData = eqpStringData;
                cat = "Eqp/Pants";
            }
            else if (itemId >= 1802000 && itemId < 1842000)
            {
                theData = eqpStringData;
                cat = "Eqp/PetEquip";
            }
            else if (itemId >= 1112000 && itemId < 1120000)
            {
                theData = eqpStringData;
                cat = "Eqp/Ring";
            }
            else if (itemId >= 1092000 && itemId < 1100000)
            {
                theData = eqpStringData;
                cat = "Eqp/Shield";
            }
            else if (itemId >= 1070000 && itemId < 1080000)
            {
                theData = eqpStringData;
                cat = "Eqp/Shoes";
            }
            else if (itemId >= 1900000 && itemId < 2000000)
            {
                theData = eqpStringData;
                cat = "Eqp/Taming";
            }
            else if (itemId >= 1300000 && itemId < 1800000)
            {
                theData = eqpStringData;
                cat = "Eqp/Weapon";
            }
            else if (itemId >= 4000000 && itemId < 5000000)
            {
                theData = etcStringData;
                cat = "Etc";
            }
            else if (itemId >= 3000000 && itemId < 4000000)
            {
                theData = insStringData;
            }
            else if (ItemConstants.isPet(itemId))
            {
                theData = petStringData;
            }
            else
            {
                return null;
            }
            if (cat.Equals("null", StringComparison.OrdinalIgnoreCase))
                return theData?.getChildByPath(itemId.ToString());
            else
            {
                return theData.getChildByPath(cat + "/" + itemId);
            }
        }

        public string? GetItemNameById(int itemId)
        {
            if (_itemNameCache.TryGetValue(itemId, out var value))
                return value.Name;

            var strings = GetStringDataByItemId(itemId);
            if (strings == null)
            {
                return null;
            }
            return GetItemNameFromData(strings).Name;
        }

        Dictionary<int, string?> _itemMsgCache = new();
        public string? GetItemMsgById(int itemId)
        {
            if (_itemMsgCache.TryGetValue(itemId, out var value))
                return value;

            var strings = GetStringDataByItemId(itemId);
            if (strings == null)
            {
                return null;
            }

            var ret = DataTool.getString("msg", strings);
            return _itemMsgCache[itemId] = ret;
        }

        Dictionary<int, NpcObjectName> _npcNameCache = new();
        bool _isNpcLoadAll = false;
        void LoadAllNpcData()
        {
            if (!_isNpcLoadAll)
            {
                foreach (Data data in npcStringData.getChildren())
                {
                    var id = int.Parse(data.getName());
                    var name = DataTool.getString("name", data) ?? "NO-NAME";
                    var defaultTalk = DataTool.getString("d0", data) ?? "(...)";
                    _npcNameCache[id] = new(id, name, defaultTalk);

                    _isNpcLoadAll = true;
                }
            }
        }
        public List<NpcObjectName> GetAllNpcList()
        {
            LoadAllNpcData();
            return _npcNameCache.Values.ToList();
        }

        public NpcObjectName GetNpcName(int npcId)
        {
            if (_npcNameCache.TryGetValue(npcId, out var data))
                return data;

            var name = DataTool.getString(npcId + "/name", npcStringData) ?? "NO-NAME";
            var defaultTalk = DataTool.getString(npcId + "/d0", npcStringData) ?? "(...)";
            return _npcNameCache[npcId] = new(npcId, name, defaultTalk);
        }



        bool _isSkillLoadAll = false;
        Dictionary<int, ObjectName> _skillNameCache = new();
        void LoadAllSkillData()
        {
            if (!_isSkillLoadAll)
            {
                foreach (var skillData in skillStringData.getChildren())
                {
                    var skillId = int.Parse(skillData.getName());
                    var skillName = DataTool.getString("name", skillData) ?? "NO-NAME";
                    _skillNameCache[skillId] = new(skillId, skillName);
                }
                _isSkillLoadAll = true;
            }
        }
        public List<ObjectName> GetAllSkillList()
        {
            LoadAllSkillData();
            return _skillNameCache.Values.ToList();
        }
        public List<int> GetAllSkillIdList()
        {
            LoadAllSkillData();
            return _skillNameCache.Keys.ToList();
        }

        public string? GetSkillName(int skillid)
        {
            StringBuilder skill = new StringBuilder();
            skill.Append(skillid);
            if (skill.Length == 4)
            {
                skill.Remove(0, 4);
                skill.Append("000").Append(skillid);
            }

            return DataTool.getString("name", skillStringData.getChildByPath(skill.ToString()));
        }


        bool _isMapLoadAll = false;
        Dictionary<int, MapName> _mapNameCache = new();
        public List<MapName> GetAllMap()
        {
            if (!_isMapLoadAll)
            {
                string mapName, streetName;
                foreach (Data searchDataDir in mapStringData.getChildren())
                {
                    foreach (Data searchData in searchDataDir.getChildren())
                    {

                        mapName = DataTool.getString(searchData.getChildByPath("mapName")) ?? "NO-NAME";
                        streetName = DataTool.getString(searchData.getChildByPath("streetName")) ?? "NO-NAME";

                        if (int.TryParse(searchData.getName(), out var id))
                        {
                            _mapNameCache[id] = new MapName(id, mapName, streetName);
                        }
                    }
                }
                _isMapLoadAll = true;
            }
            return _mapNameCache.Values.ToList();
        }

        public MapName GetMapNameById(int mapId)
        {
            if (_mapNameCache.TryGetValue(mapId, out var data))
                return data;

            var mapData = mapStringData.getChildByPath(MapConstants.GetMapDataName(mapId));
            var mapName = DataTool.getString("mapName", mapData) ?? "NO-NAME";
            var streetName = DataTool.getString("streetName", mapData) ?? "NO-NAME";
            return _mapNameCache[mapId] = new MapName(mapId, mapName, streetName);
        }


        bool _isMobLoadAll = false;
        Dictionary<int, ObjectName> _mobNameCache = new();
        public List<ObjectName> GetAllMonster()
        {
            if (!_isMobLoadAll)
            {
                foreach (Data item in mobStringData.getChildren())
                {
                    var id = int.Parse(item.getName());
                    _mobNameCache[id] = new ObjectName(id, DataTool.getString(item.getChildByPath("name")) ?? "NO-NAME");
                }
                _isMobLoadAll = true;
            }
            return _mobNameCache.Values.ToList();
        }

        public string GetMonsterName(int id)
        {
            if (_mobNameCache.TryGetValue(id, out var data))
                return data.Name;

            var mobName = DataTool.getString(mobStringData.getChildByPath(id + "/name")) ?? "NO-NAME";
            _mobNameCache[id] = new ObjectName(id, mobName);
            return mobName;
        }
    }
}
