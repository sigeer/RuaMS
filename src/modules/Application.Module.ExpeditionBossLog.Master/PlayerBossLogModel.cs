using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Module.ExpeditionBossLog.Master
{
    public class PlayerBossLogModel
    {
        public int CharacterId { get; set; }
        public string BossName { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}
