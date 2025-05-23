/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using System.Collections.Concurrent;

namespace Application.Core.Game.Relation
{
    public interface IAlliance
    {
        public int AllianceId { get; set; }
        public int Capacity { get; set; }
        public string Name { get; set; }
        public string Notice { get; set; }
        public string[] RankTitles { get; set; }
        ConcurrentDictionary<int, IGuild> Guilds { get; }
        void Disband();
        bool AddGuild(int gid);
        void broadcastMessage(Packet packet, int exception = -1, int exceptedGuildId = -1);
        void dropMessage(int type, string message);
        void dropMessage(string message);
        string getAllianceNotice();
        int getCapacity();
        List<int> getGuilds();
        int getId();
        IPlayer getLeader();
        string getName();
        string getNotice();
        string getRankTitle(int rank);
        void increaseCapacity(int inc);
        void saveToDB();
        void setCapacity(int newCapacity);
        void setNotice(string notice);
        void setRankTitle(string[] ranks);
        void updateAlliancePackets(IPlayer chr);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="method">1. 退出 2. 踢出</param>
        /// <returns>false：无法移除，需要解散联盟</returns>
        bool RemoveGuildFromAlliance(int guildId, int method);
    }
}