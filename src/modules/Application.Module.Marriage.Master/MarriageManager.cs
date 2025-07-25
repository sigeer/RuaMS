using Application.Core.Login;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Module.Marriage.Common.ErrorCodes;
using Application.Module.Marriage.Master.Models;
using Application.Utility;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Module.Marriage.Master
{
    public class MarriageManager: StorageBase<int, MarriageModel>
    {
        readonly MasterServer _server;

        int _localMarriageId = 0;

        /// <summary>
        /// 订婚
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MarriageProto.CreateMarriageRelationResponse CreateMarriageRelation(MarriageProto.CreateMarriageRelationRequest request)
        {
            var husband = _server.CharacterManager.FindPlayerById(request.FromId);
            if (husband == null)
                return new MarriageProto.CreateMarriageRelationResponse { Code = (int)ProposalErrorCode.CharacterNotFound };

            if (husband.Character.EffectMarriageId != 0)
                return new MarriageProto.CreateMarriageRelationResponse { Code = (int)ProposalErrorCode.AlreadyMarried };

            var wife = _server.CharacterManager.FindPlayerById(request.ToId);
            if (wife == null)
                return new MarriageProto.CreateMarriageRelationResponse { Code = (int)ProposalErrorCode.CharacterNotFound };

            if (wife.Character.EffectMarriageId != 0)
                return new MarriageProto.CreateMarriageRelationResponse { Code =(int) ProposalErrorCode.AlreadyMarried };

            var newModel = new MarriageModel()
            {
                Id = Interlocked.Increment(ref _localMarriageId),
                Time0 = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime()),
                Status = 0,
                Husbandid = request.FromId,
                Wifeid = request.ToId
            };
            husband.Character.EffectMarriageId = newModel.Id;
            wife.Character.EffectMarriageId = newModel.Id;

            SetDirty(newModel.Id, new StoreUnit<MarriageModel>(StoreFlag.AddOrUpdate, newModel));
            return new MarriageProto.CreateMarriageRelationResponse { MarriageId = newModel.Id };
        }

        public void BreakMarriage(MarriageProto.BreakMarriageRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null)
                return;

            SetRemoved(chr.Character.EffectMarriageId);
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<MarriageModel>> updateData)
        {
            await dbContext.Marriages.Where(x => updateData.Keys.Contains(x.Marriageid)).ExecuteDeleteAsync();

            foreach (var item in updateData)
            {
                if (item.Value.Flag == StoreFlag.AddOrUpdate)
                {
                    var obj = item.Value.Data;
                    dbContext.Marriages.Add(new MarriageEntity(obj.Id, obj.Husbandid, obj.Wifeid,  obj.Status, obj.Time0, obj.Time1, obj.Time2));
                }
            }
            await dbContext.SaveChangesAsync();
        }

        public override List<MarriageModel> Query(Expression<Func<MarriageModel, bool>> expression)
        {
            throw new NotImplementedException();
        }
        internal void CompleteMarriage(int id)
        {
            var model = Query(x => x.Id == id).FirstOrDefault();
            if (model == null) 
                return;

            model.Status = 1;
            model.Time1 = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime());
            SetDirty(model.Id, new StoreUnit<MarriageModel>(StoreFlag.AddOrUpdate, model));
        }

        public void BreakMarriage(int id)
        {
            var model = Query(x => x.Id == id).FirstOrDefault();
            if (model == null)
                return;

            model.Status = 2;
            model.Time2 = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime());
            SetDirty(model.Id, new StoreUnit<MarriageModel>(StoreFlag.AddOrUpdate, model));
        }
    }
}
