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
            return newyears.Where(x => x.isReceiverCardReceived()).ToHashSet();
        }

        public NewYearCardRecord? getNewYearRecord(int cardid)
        {
            return newyears.FirstOrDefault(x => x.getId() == cardid);
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
