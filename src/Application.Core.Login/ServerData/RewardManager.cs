using Application.Core.EF.Entities.Gachapons;
using Application.Core.Login.Dtos.CDK;
using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.Shared.Constants.Item;
using Application.Shared.Items;
using Application.Templates.Reader;
using Application.Utility;
using Application.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Runtime.ConstrainedExecution;
using XmlWzReader;

namespace Application.Core.Login.ServerData
{
    public class RewardManager : DataStorageBase<int, CdkRecordModel, RewardRecordEntity>
    {
        readonly MasterServer _server;
        IMemoryCache _cache;
        /// <summary>
        /// 在途领取的占位，和 MTS 共用同一套机制（见 <see cref="ItemTransferJournal"/>）
        /// </summary>
        readonly ItemTransferJournal _transfers = new();

        public RewardManager(MasterServer server, IMapper mapper, IDbContextFactory<DBContext> dbContextFactory, IMemoryCache cache, ILogger<RewardManager> logger)
            : base(StorageCategory.Reward, dbContextFactory, mapper, logger)
        {
            _server = server;
            _cache = cache;
        }

        protected override int GetKey(CdkRecordModel model) => model.Id;

        public async Task<ProtoModel.GetRewardsResponse> GetActiveRewards(ProtoModel.GetRewardsRequest request)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var sameAccountChrs = _server.AccountManager.GetAccountPlayerIds(request.PlayerId);
            var now = _server.GetCurrentTimeDateTimeOffset();
            var accountRecords = Query(x => sameAccountChrs.Contains(x.RecipientId), x => sameAccountChrs.Contains(x.RecipientId));

            var allActiveRewards = await dbContext.RewardCodes.Where(x => 
                x.Code == null 
                && x.StartTime <= now && (x.EndTime == null || x.EndTime >= now)
                && !x.IsDeleted).ToListAsync();

            List<ProtoModel.RewardPreviewProto> list = [];
            foreach (var item in allActiveRewards)
            {
                if (item.AccountOnce)
                {
                    if (!accountRecords.Any(x => x.CodeId == item.Id))
                    {
                        list.Add(new ProtoModel.RewardPreviewProto { Id = item.Id, Title = item.Title, Description = item.Description});
                    }
                }
                else
                {
                    if (!accountRecords.Any(x => x.CodeId == item.Id && x.RecipientId == request.PlayerId))
                    {
                        list.Add(new ProtoModel.RewardPreviewProto { Id = item.Id, Title = item.Title, Description = item.Description });
                    }
                }
            }
            var res = new ProtoModel.GetRewardsResponse { PlayerId = request.PlayerId };
            res.Rewards.AddRange(list);
            return res;
        }

        CdkCodeModel? GetCdkData(string cdk)
        {
            return _cache.GetOrCreate($"CDK:{cdk}", e =>
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var model = dbContext.RewardCodes.FirstOrDefault(x => x.Code == cdk);
                if (model != null)
                {
                    var items = dbContext.RewardItems.Where(x => x.CodeId == model.Id).ToList();
                    var dto = _mapper.Map<CdkCodeModel>(model);
                    dto.Items = _mapper.Map<List<CdkItemModel>>(items);
                    return dto;
                }
                return null;
            });
        }

        CdkCodeModel? GetRewardData(int id)
        {
            return _cache.GetOrCreate($"Reward_Id:{id}", e =>
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var model = dbContext.RewardCodes.FirstOrDefault(x => x.Id == id);
                if (model != null)
                {
                    var items = dbContext.RewardItems.Where(x => x.CodeId == model.Id).ToList();
                    var dto = _mapper.Map<CdkCodeModel>(model);
                    dto.Items = _mapper.Map<List<CdkItemModel>>(items);
                    return dto;
                }
                return null;
            });
        }

        public ProtoService.UseCdkResponse UseCdk(ProtoService.UseCdkRequest request)
        {
            if (request.Stage == ProtoModel.TransferStage.Abort)
            {
                return AbortReward(request.MasterId, request.Token);
            }

            var data = GetCdkData(request.Cdk);
            if (data == null)
            {
                return new ProtoService.UseCdkResponse { Code = (int)UseCdkResponseCode.NotFound };
            }

            return TakeReward(data, request.MasterId, request.Token, request.Stage);
        }

        public ProtoService.UseCdkResponse UseId(ProtoService.UseIdRequest request)
        {
            if (request.Stage == ProtoModel.TransferStage.Abort)
            {
                return AbortReward(request.MasterId, request.Token);
            }

            var data = GetRewardData(request.Id);
            if (data == null)
            {
                return new ProtoService.UseCdkResponse { Code = (int)UseCdkResponseCode.NotFound };
            }

            return TakeReward(data, request.MasterId, request.Token, request.Stage);
        }

        /// <summary>
        /// 放弃领取：只释放占位，不核销
        /// </summary>
        ProtoService.UseCdkResponse AbortReward(int playerId, long token)
        {
            _transfers.Drop(token, playerId);
            return new ProtoService.UseCdkResponse { Code = (int)UseCdkResponseCode.Success };
        }

        /// <summary>
        /// 领取奖励 / 兑换 CDK。
        /// <para>
        /// 和 MTS 的领取是同一类操作：中心服先把奖励标记为已领，频道侧再把道具塞进背包。
        /// 旧的实现只做了一次「标记 + 返回物品」，频道把物品发到背包时结果根本不检查 ——
        /// 背包满、或者回复丢包，奖励就凭空消失了。
        /// </para>
        /// <para>
        /// 现在按 <see cref="ProtoModel.TransferStage"/> 分三段：<c>PREPARE</c> 只校验并占位
        /// （核销记录留到 <c>COMMIT</c>），<c>ABORT</c> 释放占位。同一 token 重放 <c>PREPARE</c>
        /// 或 <c>COMMIT</c> 返回同一结论，所以频道侧超时后可以放心重试。
        /// </para>
        /// </summary>
        public ProtoService.UseCdkResponse TakeReward(CdkCodeModel data, int playerId, long token = 0, ProtoModel.TransferStage stage = ProtoModel.TransferStage.Prepare)
        {
            // 重放：这一趟已经核销过了，按成功返回（否则频道会把已经发下去的道具又收回去）
            var entry = _transfers.Get(token, playerId);
            if (entry != null && entry.Stage == ProtoModel.TransferStage.Commit)
            {
                var replay = new ProtoService.UseCdkResponse();
                replay.Items.AddRange(_mapper.Map<ProtoModel.CdkRewordPackageProto[]>(data.Items));
                return replay;
            }

            var chr = _server.CharacterManager.FindPlayerById(playerId);
            if (chr == null)
                return new ProtoService.UseCdkResponse { Code = (int)UseCdkResponseCode.FetalError };

            var now = _server.GetCurrentTimeDateTimeOffset();
            if (now < data.StartTime || now > data.EndTime)
                return new ProtoService.UseCdkResponse { Code = (int)UseCdkResponseCode.Expired };

            var histories = Query(x => x.CodeId == data.Id, x => x.CodeId == data.Id).ToList();

            if (data.MaxCount > 0 && histories.Count >= data.MaxCount)
                return new ProtoService.UseCdkResponse { Code = (int)UseCdkResponseCode.Used };

            if (histories.Any(x => x.RecipientId == playerId))
                return new ProtoService.UseCdkResponse { Code = (int)UseCdkResponseCode.Used };

            if (data.AccountOnce)
            {
                var sameAccountChrs = _server.AccountManager.GetAccountPlayerIds(chr.Character.AccountId);
                if (histories.Any(x => sameAccountChrs.Contains(x.RecipientId)))
                {
                    return new ProtoService.UseCdkResponse { Code = (int)UseCdkResponseCode.Used };
                }
            }

            if (stage != ProtoModel.TransferStage.Commit)
            {
                // PREPARE：只占位。核销记录留到频道侧确认道具已经进背包之后
                if (_transfers.Prepare(token, playerId, 0, now.ToUnixTimeMilliseconds()) == null)
                {
                    return new ProtoService.UseCdkResponse { Code = (int)UseCdkResponseCode.TooManyError };
                }

                var prepared = new ProtoService.UseCdkResponse();
                prepared.Items.AddRange(_mapper.Map<ProtoModel.CdkRewordPackageProto[]>(data.Items));
                return prepared;
            }

            SetDirty(new CdkRecordModel
            {
                Id = Interlocked.Increment(ref _localId),
                CodeId = data.Id,
                RecipientId = playerId,
                RecipientTime = now
            });

            _transfers.Reach(token, playerId, ProtoModel.TransferStage.Commit);

            var res = new ProtoService.UseCdkResponse();
            res.Items.AddRange(_mapper.Map<ProtoModel.CdkRewordPackageProto[]>(data.Items));
            return res;
        }

        #region Console
        public (List<RewardResponseDto> Data, int Total) GetPagedRewardAsync(int expired, int filterItemId, int pageIndex, int pageSize)
        {
            List<int> ids = [];
            List<RewardResponseDto> data = [];
            int total = 0;
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var now = _server.GetCurrentTimeDateTimeOffset();
                var codes = dbContext.RewardCodes.Where(x => !x.IsDeleted).AsNoTracking();
                if (expired == 0)
                {
                    codes = codes.Where(x => x.EndTime >= now);
                }
                else if (expired > 0)
                {
                    codes = codes.Where(x => x.EndTime < now);
                }

                if (filterItemId > 0)
                {
                    var filterCodes = dbContext.RewardItems.Where(x => x.ItemId == filterItemId).Select(x => x.CodeId).ToList();
                    codes = codes.Where(x => filterCodes.Contains(x.Id));
                }

                data = codes.OrderBy(x => x.StartTime).ProjectToType<RewardResponseDto>().ToPage(pageIndex, pageSize).ToList();
                ids = data.Select(x => x.Id).ToList();
                total = codes.Count();
            }

            var records = Query(x => ids.Contains(x.CodeId), x => ids.Contains(x.CodeId));
            foreach (var item in data)
            {
                item.UsedCount = records.Count(x => x.CodeId == item.Id);
            }
            return (data.ToList(), total);
        }

        public async Task<string> SubmitReward(RewardDetailRequestDto editModel)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            if (await dbContext.RewardCodes.AnyAsync(x => x.Id != editModel.Id && editModel.Code != null && x.Code == editModel.Code))
            {
                return "Reward:duplicate";
            }

            var dbModel = await dbContext.RewardCodes.FirstOrDefaultAsync(x => x.Id == editModel.Id);
            if (dbModel == null)
            {
                using var dbTrans = dbContext.Database.BeginTransaction();

                dbModel = new(editModel.Title, editModel.Description, editModel.Code, editModel.StartTime, editModel.EndTime, editModel.MaxCount, editModel.AccountOnce);
                await dbContext.RewardCodes.AddAsync(dbModel);

                await dbContext.SaveChangesAsync();
                dbContext.RewardItems.AddRange(editModel.Items.Select(x => new RewardItemEntity(dbModel.Id, x.ItemId, x.Quantity)));

                await dbContext.SaveChangesAsync();
                await dbTrans.CommitAsync();
            }
            else
            {
                // code 不能修改
                dbModel.Title = editModel.Title;
                dbModel.Description = editModel.Description;

                dbModel.MaxCount = editModel.MaxCount;
                dbModel.StartTime = editModel.StartTime;
                dbModel.EndTime = editModel.EndTime;

                await dbContext.SaveChangesAsync();
            }


            return "Reward:success";
        }

        public async Task RemoveReward(int id)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            await dbContext.RewardCodes.Where(x => x.Id == id).ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDeleted, true));
        }

        public async Task<RewardDetailResponseDto?> GetRewardDetail(int id, string locale)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var dbModel = await dbContext.RewardCodes.FirstOrDefaultAsync(x => x.Id == id);
            if (dbModel == null)
            {
                return null;
            }

            var items = await dbContext.RewardItems.Where(x => x.CodeId == id).ToListAsync();

            var model = _mapper.Map<RewardDetailResponseDto>(dbModel);
            model.Items = _mapper.Map<List<RewardItemResponseDto>>(items);
            foreach (var item in model.Items)
            {
                item.ItemName = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale).GetSubProvider(Templates.String.StringCategory.Item)
                    ?.GetItem(item.ItemId)?.Name ?? "";
            }
            return model;
        }

        public async Task<List<RewardRecordResponseDto>> GetRewardRecords(int id)
        {
            var records = Query(x => id == x.CodeId, x => id == x.CodeId);
            var items = _mapper.Map<List<RewardRecordResponseDto>>(records);
            foreach (var item in items)
            {
                item.RecipientName = _server.CharacterManager.GetPlayerName(item.RecipientId);
            }
            return items;
        }
        #endregion
    }
}
