using Application.EF;
using Application.Shared.Items;
using Application.Utility;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using net.server;

namespace Application.Core.Login.Services
{
    public class FredrickService
    {
        private static int[] dailyReminders = new int[] { 2, 5, 10, 15, 30, 60, 90, int.MaxValue };


        readonly ILogger<FredrickService> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;
        readonly NoteService _noteService;

        public FredrickService(ILogger<FredrickService> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, NoteService noteService)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _noteService = noteService;
        }

        public void runFredrickSchedule()
        {
            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                List<CharacterIdWorldPair> expiredCids = new();
                List<KeyValuePair<CharacterIdNamePair, int>> notifCids = new();
                var dataList = (from a in dbContext.Fredstorages
                                join b in dbContext.Characters on a.Cid equals b.Id
                                select new { data = a, CId = b.Id, b.Name, b.World, b.LastLogoutTime }).ToList();

                dataList.ForEach(x =>
                {
                    int daynotes = Math.Min(dailyReminders.Length - 1, x.data.Daynotes);

                    int elapsedDays = TimeUtils.DayDiff(x.data.Timestamp, DateTimeOffset.UtcNow);
                    if (elapsedDays > 100)
                    {
                        expiredCids.Add(new(x.CId, x.Name, x.World));
                    }
                    else
                    {
                        int notifDay = dailyReminders[daynotes];

                        if (elapsedDays >= notifDay)
                        {
                            do
                            {
                                daynotes++;
                                notifDay = dailyReminders[daynotes];
                            } while (elapsedDays >= notifDay);

                            int inactivityDays = TimeUtils.DayDiff(x.LastLogoutTime, DateTimeOffset.UtcNow);

                            if (inactivityDays < 7 || daynotes >= dailyReminders.Length - 1)
                            {  // don't spam inactive players
                                string name = x.Name;
                                notifCids.Add(new(new(x.CId, name), daynotes));
                            }
                        }
                    }
                });


                if (expiredCids.Count > 0)
                {
                    var cidList = expiredCids.Select(x => x.CharacterId).ToList();
                    var itemType = (int)ItemType.Merchant;
                    dbContext.Inventoryitems.Where(x => x.Type == itemType && cidList.Contains(x.Characterid ?? 0)).ExecuteDelete();

                    foreach (var cid in expiredCids)
                    {
                        var wserv = Server.getInstance().getWorld(cid.World);
                        if (wserv != null)
                        {
                            var chr = wserv.getPlayerStorage().getCharacterById(cid.CharacterId);
                            if (chr != null && chr.IsOnlined)
                            {
                                chr.setMerchantMeso(0);
                            }
                        }
                    }
                    dbContext.Characters.Where(x => cidList.Contains(x.Id)).ExecuteUpdate(x => x.SetProperty(y => y.MerchantMesos, 0));


                    _noteService.removeFredrickReminders(dbContext, expiredCids);

                    dbContext.Fredstorages.Where(x => cidList.Contains(x.Cid)).ExecuteDelete();
                }

                if (notifCids.Count > 0)
                {

                    foreach (var cid in notifCids)
                    {
                        dbContext.Fredstorages.Where(x => x.Cid == cid.Key.Id).ExecuteUpdate(x => x.SetProperty(y => y.Daynotes, cid.Value));


                        string msg = fredrickReminderMessage(cid.Value - 1);
                        _noteService.sendNormal(msg, "FREDRICK", cid.Key.Name, 0);
                    }

                }
                dbTrans.Commit();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        private static string fredrickReminderMessage(int daynotes)
        {
            string msg;

            if (daynotes < 4)
            {
                msg = "Hi customer! I am Fredrick, the Union Chief of the Hired Merchant Union. A reminder that " + dailyReminders[daynotes] + " days have passed since you used our service. Please reclaim your stored goods at FM Entrance.";
            }
            else
            {
                msg = "Hi customer! I am Fredrick, the Union Chief of the Hired Merchant Union. " + dailyReminders[daynotes] + " days have passed since you used our service. Consider claiming back the items before we move them away for refund.";
            }

            return msg;
        }



    }
}
