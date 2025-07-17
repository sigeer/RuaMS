using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.Utility.Compatible.Atomics;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Module.Duey.Master.Models
{
    public class DueyPackageModel: ITrackableEntityKey<int>
    {
        public int Id { get; set; }

        public int ReceiverId { get; set; }

        public int SenderId { get; set; }

        public int Mesos { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string? Message { get; set; }

        public bool Checked { get; set; } = true;

        public bool Type { get; set; } = false;
        public ItemModel? Item { get; set; }

        public void UpdateSentTime()
        {
            if (Type)
            {
                TimeStamp = TimeStamp.AddDays(-1);
            }
        }


        public AtomicBoolean IsFrozen { get; set; } = new AtomicBoolean(false);
    }
}
