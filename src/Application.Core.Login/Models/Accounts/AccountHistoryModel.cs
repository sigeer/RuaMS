using Application.Core.Login.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Login.Models.Accounts
{
    public class AccountHistoryModel: ITrackableEntityKey<int>
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string IP { get; set; } = null!;
        public string MAC { get; set; } = null!;
        public string HWID { get; set; } = null!;
        public DateTimeOffset LastActiveTime { get; set; }
    }
}
