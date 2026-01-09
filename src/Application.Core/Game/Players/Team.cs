using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using System.Threading.Tasks;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public bool isLeader()
        {
            return isPartyLeader();
        }
        public bool isPartyLeader()
        {
            prtLock.Enter();
            try
            {
                return getParty()?.getLeaderId() == getId();
            }
            finally
            {
                prtLock.Exit();
            }
        }


        public Team? getParty()
        {
            prtLock.Enter();
            try
            {
                return Client.CurrentServerContainer.TeamManager.ForcedGetTeam(Party);
            }
            finally
            {
                prtLock.Exit();
            }
        }

        public int getPartyId()
        {
            prtLock.Enter();
            try
            {
                return Party;
            }
            finally
            {
                prtLock.Exit();
            }
        }

        public List<Player> getPartyMembersOnline()
        {
            prtLock.Enter();
            try
            {
                var chrParty = getParty();
                if (chrParty != null)
                {
                    return chrParty.GetChannelMembers(Client.CurrentServer).Where(x => x.IsOnlined).ToList();
                }
                return new List<Player>();
            }
            finally
            {
                prtLock.Exit();
            }
        }

        public List<Player> getPartyMembersOnSameMap()
        {
            List<Player> list = new();
            prtLock.Enter();
            try
            {
                var chrParty = getParty();
                if (chrParty != null)
                {
                    int thisMapHash = this.MapModel.GetHashCode();
                    foreach (var chr in chrParty.GetChannelMembers(Client.CurrentServer))
                    {
                        var chrMap = chr.getMap();
                        // 用hashcode判断地图是否相同？ -- 同一频道、同一mapid
                        if (chrMap != null && chrMap.GetHashCode() == thisMapHash && chr.isLoggedinWorld())
                        {
                            list.Add(chr);
                        }
                    }
                }
            }
            finally
            {
                prtLock.Exit();
            }

            return list;
        }

        public bool isPartyMember(Player chr)
        {
            return isPartyMember(chr.getId());
        }

        public bool isPartyMember(int cid)
        {
            prtLock.Enter();
            try
            {
                return getParty()?.containsMembers(cid) ?? false;
            }
            finally
            {
                prtLock.Exit();
            }
        }

        public async Task<bool> leaveParty()
        {
            Team? party;
            bool partyLeader;

            prtLock.Enter();
            try
            {
                party = this.getParty();
                partyLeader = isPartyLeader();
            }
            finally
            {
                prtLock.Exit();
            }

            if (party != null)
            {
                await Client.CurrentServerContainer.TeamManager.LeaveParty(this);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void updatePartyMemberHP()
        {
            prtLock.Enter();
            try
            {
                foreach (var player in this.getPartyMembersOnSameMap())
                {
                    player.sendPacket(TeamPacketCreator.updatePartyMemberHP(getId(), HP, ActualMaxHP));
                }
            }
            finally
            {
                prtLock.Exit();
            }
        }

        /// <summary>
        /// 当队伍成员数量发生变化时：更新掉落物归属，更新传送门使用权
        /// </summary>
        /// <param name="party"></param>
        /// <param name="exPartyMembers">null：新增成员，否则移除成员</param>
        public void HandleTeamMemberCountChanged(List<Player>? exPartyMembers)
        {
            List<WeakReference<IMap>> mapids;

            petLock.Enter();
            try
            {
                mapids = new(lastVisitedMaps);
            }
            finally
            {
                petLock.Exit();
            }

            List<Player> partyMembers = new();
            foreach (var mc in (exPartyMembers ?? this.getPartyMembersOnline()))
            {
                if (mc.isLoggedinWorld())
                {
                    partyMembers.Add(mc);
                }
            }

            Player? partyLeaver = null;
            if (exPartyMembers != null)
            {
                partyMembers.Remove(this);
                partyLeaver = this;
            }

            IMap map = MapModel;
            List<MapItem>? partyItems = null;

            int partyId = exPartyMembers != null ? -1 : this.getPartyId();
            foreach (var mapRef in mapids)
            {
                if (mapRef.TryGetTarget(out var mapObj))
                {
                    List<MapItem> partyMapItems = mapObj.updatePlayerItemDropsToParty(partyId, Id, partyMembers, partyLeaver);
                    if (MapModel.GetHashCode() == mapObj.GetHashCode())
                    {
                        partyItems = partyMapItems;
                    }
                }
            }

            if (partyItems != null && exPartyMembers == null)
            {
                MapModel.updatePartyItemDropsToNewcomer(this, partyItems);
            }
        }
    }
}
