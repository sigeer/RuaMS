namespace Application.Core.Game.Relation
{
    /// <summary>
    /// 家族
    /// </summary>
    public interface IGuild : IDB_Guild
    {
        public bool IsValid { get; }
        public IAlliance? AllianceModel { get; }
        int addGuildMember(IPlayer chr);
        void broadcast(Packet packet);
        void broadcast(Packet packet, int exception);
        void broadcast(Packet packet, int exceptionId, Guild.BCOp bcop);
        void broadcastEmblemChanged();
        void broadcastInfoChanged();
        void broadcastMessage(Packet packet);
        void broadcastNameChanged();
        void changeRank(IPlayer mgc, int newRank);
        void changeRank(int cid, int newRank);
        void changeRankTitle(string[] ranks);
        void disbandGuild();
        void dropMessage(int type, string message);
        void dropMessage(string message);
        bool Equals(object? other);
        void expelMember(IPlayer initiator, string name, int cid);
        void gainGP(int amount);
        int getAllianceId();
        int getCapacity();
        int getGP();
        int GetHashCode();
        int getId();
        int getLeaderId();
        int getLogo();
        int getLogoBG();
        int getLogoBGColor();
        int getLogoColor();
        List<IPlayer> getMembers();
        IPlayer? getMGC(int cid);
        string getName();
        string getNotice();
        string getRankTitle(int rank);
        long getSignature();
        void guildChat(string name, int cid, string message);
        void guildMessage(Packet serverNotice);
        bool increaseCapacity();
        void leaveGuild(IPlayer mgc);
        void memberLevelJobUpdate(IPlayer mgc);
        void removeGP(int amount);
        void resetAllianceGuildPlayersRank();
        void setAllianceId(int aid);
        void setGuildEmblem(short bg, byte bgcolor, short logo, byte logocolor);
        void setGuildNotice(string notice);
        int setLeaderId(int charId);
        void setLogo(int l);
        void setLogoBG(int bg);
        void setLogoBGColor(int c);
        void setLogoColor(int c);
        void setOnline(int cid, bool online, int channel);
        void writeToDB(bool bDisband);
    }
}
