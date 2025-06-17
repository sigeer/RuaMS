using Application.EF;
using Application.EF.Entities;
using Application.Utility;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Application.Module.Family.Master
{
    public class DataService
    {
        ConcurrentDictionary<int, UpdateField<FamilyModel>> _dirtyFlag = new();

        public void SetDirty(FamilyModel model)
        {
            _dirtyFlag[model.Id] = new UpdateField<FamilyModel>(UpdateMethod.AddOrUpdate, model);
        }

        public void SetRemove(FamilyModel model)
        {
            _dirtyFlag[model.Id] = new UpdateField<FamilyModel>(UpdateMethod.Remove, model);
        }

        public async Task CommitAsync(DBContext dbContext)
        {
            var updateData = new Dictionary<int, UpdateField<FamilyModel>>();
            foreach (var key in _dirtyFlag.Keys.ToList())
            {
                _dirtyFlag.TryRemove(key, out var d);
                updateData[key] = d;
            }

            var updateCount = updateData.Count;
            if (updateCount == 0)
                return;

            await dbContext.FamilyCharacters.Where(x => updateData.Keys.Contains(x.Familyid)).ExecuteDeleteAsync();
            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                if (item.Method != UpdateMethod.Remove)
                {
                    dbContext.FamilyCharacters.AddRange(obj.Members.Values.Select(x => new FamilyCharacter(x.Cid, obj.Id, x.Seniorid)
                    {
                        Lastresettime = x.Lastresettime,
                        Precepts = x.Precepts,
                        Reptosenior = x.Reptosenior,
                        Reputation = x.Reputation,
                        Todaysrep = x.Todaysrep,
                        Totalreputation = x.Totalreputation
                    }));
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
