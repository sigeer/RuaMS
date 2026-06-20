namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private bool challenged = false;
        public bool isChallenged()
        {
            return challenged;
        }

        public void setChallenged(bool challenged)
        {
            this.challenged = challenged;
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
