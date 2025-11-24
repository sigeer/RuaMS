using Application.Shared.Events;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private int FestivalPoints;
        private bool challenged = false;
        public bool isChallenged()
        {
            return challenged;
        }

        public void setChallenged(bool challenged)
        {
            this.challenged = challenged;
        }

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

        public void addCP(int ammount)
        {
            TotalCP += ammount;
            AvailableCP += ammount;
        }

        public void useCP(int ammount)
        {
            AvailableCP -= ammount;
        }

        public void gainCP(int gain)
        {
            if (MCTeam != null)
            {
                if (gain > 0)
                    MCTeam.AddCP(this, gain);
                else
                    MCTeam.UseCP(this, -gain);

                sendPacket(PacketCreator.CPUpdate(false, AvailableCP, TotalCP, MCTeam.TeamFlag));
                if (MCTeam.TeamFlag != TeamGroupEnum.None)
                {
                    this.MapModel.broadcastMessage(PacketCreator.CPUpdate(true, MCTeam.AvailableCP, MCTeam.TotalCP, MCTeam.TeamFlag));
                }
            }
        }

        public void resetCP()
        {
            this.AvailableCP = 0;
            this.TotalCP = 0;
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
