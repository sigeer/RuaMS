using client.newyear;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private HashSet<NewYearCardRecord> newyears = new();
        public HashSet<NewYearCardRecord> getNewYearRecords()
        {
            return newyears;
        }

        public HashSet<NewYearCardRecord> getReceivedNewYearRecords()
        {
            HashSet<NewYearCardRecord> received = new();

            foreach (NewYearCardRecord nyc in newyears)
            {
                if (nyc.isReceiverCardReceived())
                {
                    received.Add(nyc);
                }
            }

            return received;
        }

        public NewYearCardRecord? getNewYearRecord(int cardid)
        {
            foreach (NewYearCardRecord nyc in newyears)
            {
                if (nyc.getId() == cardid)
                {
                    return nyc;
                }
            }

            return null;
        }

        public void addNewYearRecord(NewYearCardRecord newyear)
        {
            newyears.Add(newyear);
        }

        public void removeNewYearRecord(NewYearCardRecord newyear)
        {
            newyears.Remove(newyear);
        }
    }
}
