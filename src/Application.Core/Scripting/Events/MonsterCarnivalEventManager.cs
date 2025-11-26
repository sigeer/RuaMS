using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;

namespace Application.Core.Scripting.Events
{
    public class MonsterCarnivalEventManager : AbstractInstancedEventManager
    {
        public List<MonsterCarnivalPreparationRoom> Rooms { get; private set; }
        public int MinLevel { get; }
        public int MaxLevel { get; }
        public MonsterCarnivalEventManager(WorldChannel cserv, IEngine iv, ScriptFile file) : base(cserv, iv, file)
        {
            Rooms = [];
            MinLevel = iv.GetValue("minLevel").ToObject<int>();
            MaxLevel = iv.GetValue("maxLevel").ToObject<int>();
        }

        public void InsertRoom(string instanceName, int minCount, int map)
        {
            Rooms.Add(new MonsterCarnivalPreparationRoom(instanceName, minCount, map));
        }

        protected override AbstractEventInstanceManager CreateNewInstance(string instanceName)
        {
            return new MonsterCarnivalEventInstanceManager(this, instanceName);
        }

        public MonsterCarnivalPreparationRoom? GetRoom(int roomIndex) => Rooms.ElementAtOrDefault(roomIndex);

        protected override bool RegisterInstanceInternal(string instanceName, AbstractEventInstanceManager eim)
        {
            var room = Rooms.FirstOrDefault(x => x.InstanceName == instanceName);
            if (room == null)
                return false;

            if (room.Instance != null)
                return false;

            room.Instance = eim as MonsterCarnivalEventInstanceManager;
            return true;
        }

        protected override void DisposeInstanceInternal(string name)
        {
            var room = Rooms.FirstOrDefault(x => x.InstanceName == name);
            if (room != null)
                room.Instance = null;
        }

        public List<IPlayer> GetPreparationParty(int roomIndex, Team party)
        {
            if (party == null)
            {
                return new();
            }
            try
            {
                var room = Rooms[roomIndex];
                var result = iv.CallFunction("getEligibleParty", party.GetChannelMembers(cserv), room, 0);
                var eligibleParty = result.ToObject<List<IPlayer>>() ?? [];
                party.setEligibleMembers(eligibleParty);
                return eligibleParty;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Script: {Script}", _name);
            }

            return new();
        }


        public List<IPlayer> GetRoomEligibleParty(MonsterCarnivalEventInstanceManager eim, Team party)
        {
            if (party == null)
            {
                return new();
            }
            try
            {
                var result = iv.CallFunction("getEligibleParty", party.GetChannelMembers(cserv), eim.Room, 1);
                var eligibleParty = result.ToObject<List<IPlayer>>() ?? [];
                party.setEligibleMembers(eligibleParty);
                return eligibleParty;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Script: {Script}", _name);
            }

            return new();
        }


        public bool StartInstance(IPlayer chr, IMap map, int roomIndex)
        {
            lock (startLock)
            {
                if (roomIndex < 0 || roomIndex >= Rooms.Count)
                    return false;

                if (chr.TeamModel == null)
                    return false;

                var room = Rooms[roomIndex];
                if (room.Instance != null)
                    return false;

                var eim = createInstance<MonsterCarnivalEventInstanceManager>("setup", roomIndex);
                eim.Initialize(chr.TeamModel, room);
                eim.registerParty(chr.TeamModel, map);

                eim.setLeader(chr);

                eim.startEvent();
                registerEventInstance(eim, roomIndex);
                return true;
            }
        }

        public int JoinInstance(IPlayer chr, IMap map, int roomIndex)
        {
            lock (startLock)
            {
                if (roomIndex < 0 || roomIndex >= Rooms.Count)
                    return 1;

                var room = Rooms[roomIndex];
                if (room.Instance == null || room.Instance.getLeader() == null)
                    return 3;

                if (chr.TeamModel == null || chr.TeamModel.getEligibleMembers().Count != room.Instance.Team0.getEligibleMembers().Count)
                    return 2;

                if (room.Instance.Team1 != null)
                    return 4;

                room.Instance.Team1 = chr.TeamModel;
                // send challenge
                if (getChannelServer().NPCScriptManager.start(
                    room.Instance.getLeader()!.Client,
                    2042001,
                    "cpqchallenge",
                    null))
                {
                    return 0;
                }
                room.Instance.Team1 = null;
                return 4;
            }
        }
    }

    public class MonsterCarnivalPreparationRoom
    {
        public MonsterCarnivalPreparationRoom(string instanceName, int minCount, int map)
        {
            MinCount = minCount;
            Map = map;
            RecruitMap = ProviderSource.Instance.GetProvider<MapProvider>().GetItem(Map)!.ForcedReturn;
            InstanceName = instanceName;
        }

        public int MinCount { get; set; }
        public int Map { get; set; }
        public int RecruitMap { get; set; }
        public string InstanceName { get; set; }
        public MonsterCarnivalEventInstanceManager? Instance { get; set; }
    }
}
