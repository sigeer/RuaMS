using Application.Core.Channel;
using Application.Core.Game.Relation;
using Application.Core.scripting.Events.Abstraction;
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
            if (base.RegisterInstanceInternal(instanceName, eim))
            {
                var room = Rooms.FirstOrDefault(x => x.InstanceName == instanceName);
                if (room == null)
                    return false;

                if (room.Instance != null)
                    return false;

                room.Instance = eim as MonsterCarnivalEventInstanceManager;
                return true;
            }
            return true;
        }

        protected override void DisposeInstanceInternal(string name)
        {
            base.DisposeInstanceInternal(name);
            var room = Rooms.FirstOrDefault(x => x.InstanceName == name);
            if (room != null)
            {
                room.Instance = null;
            }
        }

        public List<Player> GetPreparationParty(int roomIndex, Team party)
        {
            if (party == null)
            {
                return new();
            }
            try
            {
                var room = Rooms[roomIndex];
                var result = iv.CallFunction("getEligibleParty", party.GetChannelMembers(cserv), room, 0);
                var eligibleParty = result.ToObject<List<Player>>() ?? [];
                return eligibleParty;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Script: {Script}", _name);
            }

            return new();
        }


        public bool Check2(MonsterCarnivalEventInstanceManager eim)
        {
            try
            {
                var t0 = iv.CallFunction("getEligibleParty", eim.Team0.EligibleMembers, eim.Room, 1).ToObject<List<Player>>() ?? [];
                var t1 = iv.CallFunction("getEligibleParty", eim.Team1.EligibleMembers, eim.Room, 1).ToObject<List<Player>>() ?? [];

                return t0.Count == t1.Count && t0.Count >= eim.Room.MinCount;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Script: {Script}", _name);
                return false;
            }
        }


        public bool StartInstance(Player chr, List<Player> eligiMembers, int roomIndex)
        {
            if (roomIndex < 0 || roomIndex >= Rooms.Count)
                return false;

            if (chr.Party == 0)
                return false;

            var room = Rooms[roomIndex];
            if (room.Instance != null)
                return false;

            var eim = createInstance<MonsterCarnivalEventInstanceManager>("setup", roomIndex);
            eim.Initialize(new TeamRegistry(chr.Party, eligiMembers), room);

            foreach (var item in eligiMembers)
            {
                eim.registerPlayer(item);
            }

            eim.setLeader(chr);

            eim.startEvent();
            registerEventInstance(eim, roomIndex);
            return true;
        }

        public int JoinInstance(Player chr, List<Player> eligibleMembers, int roomIndex)
        {
            if (roomIndex < 0 || roomIndex >= Rooms.Count)
                return 1;

            var room = Rooms[roomIndex];
            if (room.Instance == null || room.Instance.getLeader() == null || room.Instance.Team0 == null)
                return 3;

            var chrParty = chr.getParty();
            if (chrParty == null || eligibleMembers.Count != room.Instance.Team0.EligibleMembers.Count)
                return 2;

            if (room.Instance.Team1 != null)
                return 4;

            if (room.Instance.getLeader()!.Client.NPCConversationManager != null)
                return 4;

            room.Instance.Team1 = new TeamRegistry(chr.Party, eligibleMembers);
            // send challenge
            if (getChannelServer().NPCScriptManager.start(
                room.Instance.getLeader()!.Client,
                2042001,
                "mc_enter1",
                null))
            {
                return 0;
            }
            room.Instance.Team1 = null;
            return 4;
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
