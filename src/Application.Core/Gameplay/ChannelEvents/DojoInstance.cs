using Application.Core.Game.Relation;
using Application.Core.Game.TheWorld;
using Application.Core.scripting.Event;
using constants.id;
using net.server;
using server;

namespace Application.Core.Gameplay.ChannelEvents
{
    public class DojoInstance: IAsyncDisposable
    {
        IWorldChannel Channel { get; }
        private int usedDojo = 0;
        private int[] dojoStage;
        private long[] dojoFinishTime;
        private ScheduledFuture?[] dojoTask;
        private Dictionary<int, int> dojoParty = new();
        private object lockObj = new object();

        public const int StageCount = 20;
        public DojoInstance(IWorldChannel channel)
        {
            Channel = channel;

            dojoStage = new int[StageCount];
            dojoFinishTime = new long[StageCount];
            dojoTask = new ScheduledFuture[StageCount];
            for (int i = 0; i < StageCount; i++)
            {
                dojoStage[i] = 0;
                dojoFinishTime[i] = 0;
                dojoTask[i] = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            for (int i = 0; i < StageCount; i++)
            {
                if (dojoTask[i] != null)
                {
                    await dojoTask[i]!.CancelAsync(false);
                    dojoTask[i] = null;
                }
                dojoStage[i] = 0;
                dojoFinishTime[i] = 0;
            }
        }

        public int LookupPartyDojo(ITeam? party)
        {
            if (party == null)
            {
                return -1;
            }

            return dojoParty.GetValueOrDefault(party.GetHashCode(), -1);
        }

        public int IngressDojo(bool isPartyDojo, ITeam? party, int fromStage)
        {
            Monitor.Enter(lockObj);
            try
            {
                int dojoList = this.usedDojo;
                int range, slot = 0;

                if (!isPartyDojo)
                {
                    dojoList = dojoList >> 5;
                    range = 15;
                }
                else
                {
                    range = 5;
                }

                while ((dojoList & 1) != 0)
                {
                    dojoList = (dojoList >> 1);
                    slot++;
                }

                if (slot < range)
                {
                    int slotMapid = (isPartyDojo ? MapId.DOJO_PARTY_BASE : MapId.DOJO_SOLO_BASE) + (100 * (fromStage + 1)) + slot;
                    int dojoSlot = GetDojoSlot(slotMapid);

                    if (party != null)
                    {
                        if (dojoParty.ContainsKey(party.GetHashCode()))
                        {
                            return -2;
                        }
                        dojoParty.Add(party.GetHashCode(), dojoSlot);
                    }

                    this.usedDojo |= (1 << dojoSlot);

                    this.ResetDojo(slotMapid);
                    this.startDojoSchedule(slotMapid);
                    return slot;
                }
                else
                {
                    return -1;
                }
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        private void FreeDojoSlot(int slot, ITeam? party)
        {
            int mask = 0b11111111111111111111;
            mask ^= (1 << slot);

            Monitor.Enter(lockObj);
            try
            {
                usedDojo &= mask;
            }
            finally
            {
                Monitor.Exit(lockObj);
            }

            if (party != null)
            {
                if (dojoParty.Remove(party.GetHashCode(), out var _))
                {
                    return;
                }
            }

            if (dojoParty.ContainsValue(slot))
            {    // strange case, no party there!
                HashSet<KeyValuePair<int, int>> es = new(dojoParty);

                foreach (var e in es)
                {
                    if (e.Value == slot)
                    {
                        dojoParty.Remove(e.Key);
                        break;
                    }
                }
            }
        }

        private static int GetDojoSlot(int dojoMapId)
        {
            return (dojoMapId % 100) + ((dojoMapId / 10000 == 92502) ? 5 : 0);
        }

        public void ResetDojoMap(int fromMapId)
        {
            for (int i = 0; i < (((fromMapId / 100) % 100 <= 36) ? 5 : 2); i++)
            {
                this.Channel.getMapFactory().getMap(fromMapId + (100 * i)).resetMapObjects();
            }
        }

        public void ResetDojo(int dojoMapId, int thisStg = -1)
        {
            int slot = GetDojoSlot(dojoMapId);
            this.dojoStage[slot] = thisStg;
        }

        public void FreeDojoSectionIfEmpty(int dojoMapId)
        {
            int slot = GetDojoSlot(dojoMapId);
            int delta = (dojoMapId) % 100;
            int stage = (dojoMapId / 100) % 100;
            int dojoBaseMap = (dojoMapId >= MapId.DOJO_PARTY_BASE) ? MapId.DOJO_PARTY_BASE : MapId.DOJO_SOLO_BASE;

            for (int i = 0; i < 5; i++)
            { //only 32 stages, but 38 maps
                if (stage + i > 38)
                {
                    break;
                }
                var dojoMap = this.Channel.getMapFactory().getMap(dojoBaseMap + (100 * (stage + i)) + delta);
                if (dojoMap.getAllPlayers().Count > 0)
                {
                    return;
                }
            }

            FreeDojoSlot(slot, null);
        }

        private void startDojoSchedule(int dojoMapId)
        {
            int slot = GetDojoSlot(dojoMapId);
            int stage = (dojoMapId / 100) % 100;
            if (stage <= dojoStage[slot])
            {
                return;
            }

            long clockTime = (stage > 36 ? 15 : (stage / 6) + 5) * 60000;

            Monitor.Enter(lockObj);
            try
            {
                if (this.dojoTask[slot] != null)
                {
                    this.dojoTask[slot]!.cancel(false);
                }
                this.dojoTask[slot] = TimerManager.getInstance().schedule(() =>
                {
                    int delta = (dojoMapId) % 100;
                    int dojoBaseMap = (slot < 5) ? MapId.DOJO_PARTY_BASE : MapId.DOJO_SOLO_BASE;
                    ITeam? party = null;

                    for (int i = 0; i < 5; i++)
                    { //only 32 stages, but 38 maps
                        if (stage + i > 38)
                        {
                            break;
                        }

                        var dojoExit = this.Channel.getMapFactory().getMap(MapId.DOJO_EXIT);
                        foreach (var chr in this.Channel.getMapFactory().getMap(dojoBaseMap + (100 * (stage + i)) + delta).getAllPlayers())
                        {
                            if (MapId.isDojo(chr.getMap().getId()))
                            {
                                chr.changeMap(dojoExit);
                            }
                            party = chr.getParty();
                        }
                    }

                    FreeDojoSlot(slot, party);
                }, clockTime + 3000);   // let the TIMES UP display for 3 seconds, then warp
            }
            finally
            {
                Monitor.Exit(lockObj);
            }

            dojoFinishTime[slot] = Server.getInstance().getCurrentTime() + clockTime;
        }

        public void DismissDojoSchedule(int dojoMapId, ITeam party)
        {
            int slot = GetDojoSlot(dojoMapId);
            int stage = (dojoMapId / 100) % 100;
            if (stage <= dojoStage[slot])
            {
                return;
            }

            Monitor.Enter(lockObj);
            try
            {
                if (this.dojoTask[slot] != null)
                {
                    this.dojoTask[slot]!.cancel(false);
                    this.dojoTask[slot] = null;
                }
            }
            finally
            {
                Monitor.Exit(lockObj);
            }

            FreeDojoSlot(slot, party);
        }

        public bool SetDojoProgress(int dojoMapId)
        {
            int slot = GetDojoSlot(dojoMapId);
            int dojoStg = (dojoMapId / 100) % 100;

            if (this.dojoStage[slot] < dojoStg)
            {
                this.dojoStage[slot] = dojoStg;
                return true;
            }
            else
            {
                return false;
            }
        }

        public long GetDojoFinishTime(int dojoMapId)
        {
            return dojoFinishTime[GetDojoSlot(dojoMapId)];
        }

    }
}
