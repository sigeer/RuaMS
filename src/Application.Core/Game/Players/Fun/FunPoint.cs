using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private int cp = 0;
        private int totCP = 0;
        private int FestivalPoints;
        private bool challenged = false;
        public int totalCP, availableCP;

        public void gainFestivalPoints(int gain)
        {
            this.FestivalPoints += gain;
        }

        public int getFestivalPoints()
        {
            return this.FestivalPoints;
        }

        public void setFestivalPoints(int pontos)
        {
            this.FestivalPoints = pontos;
        }

        public int getCP()
        {
            return cp;
        }

        public void addCP(int ammount)
        {
            totalCP += ammount;
            availableCP += ammount;
        }

        public void useCP(int ammount)
        {
            availableCP -= ammount;
        }

        public void gainCP(int gain)
        {
            var monsterCarnival = getMonsterCarnival();
            if (monsterCarnival != null)
            {
                if (gain > 0)
                {
                    this.setTotalCP(this.getTotalCP() + gain);
                }
                this.setCP(this.getCP() + gain);
                if (this.getParty() != null)
                {
                    monsterCarnival.setCP(monsterCarnival.getCP(team) + gain, team);
                    if (gain > 0)
                    {
                        monsterCarnival.setTotalCP(monsterCarnival.getTotalCP(team) + gain, team);
                    }
                }
                if (this.getCP() > this.getTotalCP())
                {
                    this.setTotalCP(this.getCP());
                }
                sendPacket(PacketCreator.CPUpdate(false, this.getCP(), this.getTotalCP(), getTeam()));
                if (this.getParty() != null && getTeam() != -1)
                {
                    this.MapModel.broadcastMessage(PacketCreator.CPUpdate(true, monsterCarnival.getCP(team), monsterCarnival.getTotalCP(team), getTeam()));
                }
            }
        }

        public void setTotalCP(int a)
        {
            this.totCP = a;
        }

        public void setCP(int a)
        {
            this.cp = a;
        }

        public int getTotalCP()
        {
            return totCP;
        }

        public int getAvailableCP()
        {
            return availableCP;
        }

        public void resetCP()
        {
            this.cp = 0;
            this.totCP = 0;
            this.monsterCarnival = null;
        }


        public string getPartyQuestItems()
        {
            return DataString;
        }

        public bool gotPartyQuestItem(string partyquestchar)
        {
            return DataString.Contains(partyquestchar);
        }

        public void removePartyQuestItem(string letter)
        {
            if (gotPartyQuestItem(letter))
            {
                DataString = DataString!.Substring(0, DataString.IndexOf(letter)) + DataString.Substring(DataString.IndexOf(letter) + letter.Length);
            }
        }

        public void setPartyQuestItemObtained(string partyquestchar)
        {
            if (!DataString.Contains(partyquestchar))
            {
                this.DataString += partyquestchar;
            }
        }

    }
}
