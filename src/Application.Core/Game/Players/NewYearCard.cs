using Application.Core.Models;
using Application.Shared.NewYear;
using System.Collections.Concurrent;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private ConcurrentDictionary<int, NewYearCardObject> newyears = new();

        public HashSet<NewYearCardObject> getReceivedNewYearRecords()
        {
            return newyears.Values.Where(x => x.Received).ToHashSet();
        }

        public void addNewYearRecord(NewYearCardObject newyear)
        {
            newyears[newyear.Id] = newyear;
        }

        public void RemoveNewYearRecord(int id)
        {
            newyears.TryRemove(id, out _);
        }

        public void DiscardNewYearRecord(bool isSender)
        {
            Client.CurrentServerContainer.NewYearCardService.DiscardNewYearCard(this, isSender);
        }
    }
}
