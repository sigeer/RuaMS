using Application.Core.Login;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Module.Family.Common;
using Application.Module.Family.Master.Models;
using Application.Utility;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Application.Module.Family.Master
{
    public class FamilyManager : StorageBase<int, FamilyCharacterModel>
    {
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly MasterServer _server;
        readonly ILogger<FamilyManager> _logger;
        readonly FamilyConfigs _config;
        readonly IMapper _mapper;
        readonly MasterFamilyModuleTransport _transport;

        int _currentFamilyId = 0;

        public FamilyManager(
            IDbContextFactory<DBContext> dbContextFactory,
            MasterServer server,
            ILogger<FamilyManager> logger,
            IMapper mapper,
            IOptions<FamilyConfigs> options,
            MasterFamilyModuleTransport transport)
        {
            _dbContextFactory = dbContextFactory;
            _server = server;
            _logger = logger;
            _mapper = mapper;
            _config = options.Value;
            _transport = transport;
        }

        public override async Task InitializeAsync(DBContext dbContext)
        {
            _currentFamilyId = await dbContext.FamilyCharacters.MaxAsync(x => (int?)x.Familyid) ?? 0;
        }

        public override List<FamilyCharacterModel> Query(Expression<Func<FamilyCharacterModel, bool>> expression)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var entityExpression = _mapper.MapExpression<Expression<Func<FamilyCharacterEntity, bool>>>(expression);

            var allData = _mapper.Map<List<FamilyCharacterModel>>(dbContext.FamilyCharacters.AsNoTracking().Where(entityExpression).ToList());

            var cids = allData.Select(x => x.Id).ToList();
            var useRecords = dbContext.FamilyEntitlements.AsNoTracking().Where(x => cids.Contains(x.Charid)).ToList();
            foreach (var item in allData)
            {
                item.EntitlementUseRecord = _mapper.Map<List<FamilyEntitlementUseRecord>>(useRecords.Where(x => x.Charid == item.Id));
            }
            return QueryWithDirty(allData, expression.Compile());
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<FamilyCharacterModel>> updateData)
        {
            await dbContext.FamilyCharacters.Where(x => updateData.Keys.Contains(x.Cid)).ExecuteDeleteAsync();
            await dbContext.FamilyEntitlements.Where(x => updateData.Keys.Contains(x.Charid)).ExecuteDeleteAsync();
            foreach (var item in updateData.Values)
            {
                if (item.Flag != StoreFlag.Remove)
                {
                    var x = item.Data!;
                    dbContext.FamilyCharacters.Add(new FamilyCharacterEntity(x.Id, x.Familyid, x.Seniorid)
                    {
                        Lastresettime = x.Lastresettime,
                        Precepts = x.Precepts,
                        Reptosenior = x.Reptosenior,
                        Reputation = x.Reputation,
                        Todaysrep = x.Todaysrep,
                        Totalreputation = x.Totalreputation
                    });
                    dbContext.FamilyEntitlements.AddRange(x.EntitlementUseRecord.Select(y => new FamilyEntitlementEntity
                    {
                        Entitlementid = y.Id,
                        Charid = x.Id,
                        Timestamp = y.Time
                    }));
                }
            }
            await dbContext.SaveChangesAsync();
        }

        public void UpdateFamilyMessage()
        {
            //if (save)
            //{
            //    try
            //    {
            //        using var dbContext = new DBContext();
            //        dbContext.FamilyCharacters.Where(x => x.Cid == getLeader().getChrId()).ExecuteUpdate(x => x.SetProperty(y => y.Precepts, message));
            //    }
            //    catch (Exception e)
            //    {
            //        log.Error(e, "Could not save new precepts for family {FamilyId}", getID());
            //    }
            //}
        }

        public void ResetEntitlementUsage()
        {
            var resetTime = _server.getCurrentTime() + (long)TimeSpan.FromMinutes(1).TotalMilliseconds;
            try
            {
                var dataList = Query(x => x.Lastresettime <= resetTime);
                foreach (var item in dataList)
                {
                    item.Todaysrep = 0;
                    item.Reptosenior = 0;
                    item.EntitlementUseRecord.RemoveAll(x => x.Time <= resetTime);
                    SetDirty(new StoreUnit<FamilyCharacterModel>(StoreFlag.AddOrUpdate, item));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get connection to DB");
            }
        }

        public Dto.GetFamilyResponse AcceptInvite(int masterId, int memberId)
        {
            var res = new Dto.GetFamilyResponse();
            var theFamily = Query(x => x.Id == memberId).FirstOrDefault();
            if (theFamily != null && theFamily.Seniorid != 0)
            {
                // 接受邀请者已有学院 且已有上级
                return new Dto.GetFamilyResponse { Code = ErrorCode.NotLeader };
            }

            var family = Query(x => x.Id == masterId).FirstOrDefault();
            if (family != null)
            {

                if (theFamily != null)
                {
                    // 已经在同一个学院里了
                    if (family.Familyid == theFamily.Familyid)
                        return new Dto.GetFamilyResponse();

                    // 合并学院但是超出范围
                    if (GetMemberDepth(masterId) + GetMaxDepth(theFamily.Familyid) > _config.FAMILY_MAX_GENERATIONS)
                    {
                        return new Dto.GetFamilyResponse { Code = ErrorCode.OverMaxGenerations };
                    }
                }
            }
            else
            {
                family = new FamilyCharacterModel
                {
                    Id = masterId,
                    Familyid = Interlocked.Increment(ref _currentFamilyId),
                };
                SetDirty(family.Id, new StoreUnit<FamilyCharacterModel>(StoreFlag.AddOrUpdate, family));
            }


            // 对方有学院、则合并。没有则加入
            if (theFamily != null)
            {
                theFamily.Seniorid = masterId;
                theFamily.Reptosenior = 0;
                theFamily.Familyid = family.Familyid;
            }
            else
            {
                theFamily = new FamilyCharacterModel
                {
                    Id = memberId,
                    Familyid = family.Familyid,
                    Seniorid = masterId
                };
                SetDirty(theFamily.Id, new StoreUnit<FamilyCharacterModel>(StoreFlag.AddOrUpdate, theFamily));
            }
            return new Dto.GetFamilyResponse { Model = _mapper.Map<Dto.FamilyDto>(family) };
        }

        public Dto.GetFamilyResponse ForkFamily(int id)
        {
            var family = Query(x => x.Id == id).FirstOrDefault();
            if (family == null)
                return new Dto.GetFamilyResponse();

            var originTree = Query(x => x.Familyid == family.Familyid);

            var children = GetAllChildren(originTree, id);
            var newFamilyId = Interlocked.Increment(ref _currentFamilyId);

            family.Seniorid = 0;
            family.Reptosenior = 0;
            family.Familyid = newFamilyId;
            SetDirty(new StoreUnit<FamilyCharacterModel>(StoreFlag.AddOrUpdate, family));

            foreach (var item in children)
            {
                item.Familyid = newFamilyId;

                SetDirty(new StoreUnit<FamilyCharacterModel>(StoreFlag.AddOrUpdate, item));
            }
            return new Dto.GetFamilyResponse { Model = _mapper.Map<Dto.FamilyDto>(Map(children, newFamilyId)) };
        }

        private FamilyModel Map(List<FamilyCharacterModel> chrs, int familyId)
        {
            return new FamilyModel(familyId)
            {
                Members = new(chrs.Where(y => y.Familyid == familyId).ToList().ToDictionary(y => y.Id, y => _mapper.Map<FamilyMemberModel>(y)))
            };
        }

        internal FamilyModel? GetFamilyByCharacterId(int cid)
        {
            var item = GetFamilyCharacter(cid);
            if (item == null)
                return null;

            return Map(Query(x => x.Familyid == item.Familyid), item.Familyid);
        }
        public FamilyCharacterModel? GetFamilyCharacter(int cid)
        {
            return Query(x => x.Id == cid).FirstOrDefault();
        }
        List<FamilyCharacterModel> GetAllChildren(IEnumerable<FamilyCharacterModel> flatList, int parentId)
        {
            var result = new List<FamilyCharacterModel>();

            // 找到直接子项
            var directChildren = flatList.Where(n => n.Seniorid == parentId).ToList();

            foreach (var child in directChildren)
            {
                result.Add(child);
                result.AddRange(GetAllChildren(flatList, child.Id)); // 递归查找孙子节点
            }

            return result;
        }


        /// <summary>
        /// 玩家在学院里的深度
        /// </summary>
        /// <param name="cid">玩家ID</param>
        /// <returns></returns>
        public int GetMemberDepth(int cid)
        {
            var chrData = Query(x => x.Id == cid).FirstOrDefault();
            if (chrData == null)
                return -1;

            var totalFamilyList = Query(x => x.Familyid == chrData.Familyid);

            int depth = 0;
            var point = totalFamilyList.FirstOrDefault(x => x.Id == cid);
            while (point != null)
            {
                depth++;
                point = totalFamilyList.FirstOrDefault(x => x.Id == point.Seniorid);
            }
            return depth;
        }

        public int GetMaxDepth(int familyId)
        {
            var totalFamilyList = Query(x => x.Familyid == familyId);
            if (totalFamilyList.Count == 0)
                return 0;

            return GetMaxDepth(totalFamilyList);

        }

        public static int GetMaxDepth(List<FamilyCharacterModel> list)
        {
            // 构建映射：Seniorid -> List of Children
            var childrenMap = list
                .GroupBy(f => f.Seniorid)
                .ToDictionary(g => g.Key, g => g.ToList());

            int maxDepth = 0;

            void Dfs(int currentCid, int depth)
            {
                maxDepth = Math.Max(maxDepth, depth);

                if (childrenMap.TryGetValue(currentCid, out var children))
                {
                    foreach (var child in children)
                    {
                        Dfs(child.Id, depth + 1);
                    }
                }
            }

            // 寻找所有根节点（Seniorid == 0）
            var roots = list.Where(f => f.Seniorid == 0);

            foreach (var root in roots)
            {
                Dfs(root.Id, 1);
            }

            return maxDepth;
        }

        public async Task UseEntitlement(Dto.UseEntitlementRequest request)
        {
            var res = new Dto.UseEntitlementResponse() { Request = request };
            var playerFamily = Query(x => x.Id == request.MatserId).FirstOrDefault();
            if (playerFamily == null)
            {
                res.Code = 1;
                await _transport.Send(res);
                return;
            }


            var entitlement = FamilyEntitlement.Parse(request.EntitlementId);
            if (playerFamily.Reputation < entitlement.getRepCost())
            {
                res.Code = 1;
                await _transport.Send(res);
                return;
            }
            if (playerFamily.EntitlementUseRecord.Count(x => x.Id == request.EntitlementId) >= entitlement.getUsageLimit())
            {
                res.Code = 1;
                await _transport.Send(res);
                return;
            }


            if (request.EntitlementId == FamilyEntitlement.FAMILY_REUINION.Value)
            {
                await _server.CrossServerService.SummonPlayerById(request.MatserId, request.TargetPlayerId);
            }

            GainReputation(playerFamily, -entitlement.getRepCost(), false, _server.CharacterManager.GetPlayerName(playerFamily.Id));
            playerFamily.EntitlementUseRecord.Add(new FamilyEntitlementUseRecord(request.EntitlementId));

            await _transport.Send(res);
        }

        void GainReputation(FamilyCharacterModel member, int gain, bool countTowardsTotal, string? from = null)
        {
            member.Reputation += gain;
            member.Todaysrep += gain;
            if (gain > 0 && countTowardsTotal)
            {
                member.Totalreputation += gain;
            }
        }

        public async Task Refund(int usePlayer)
        {
            var playerFamily = Query(x => x.Id == usePlayer).FirstOrDefault();
            if (playerFamily == null)
                return;

            GainReputation(playerFamily, FamilyEntitlement.SUMMON_FAMILY.getRepCost(), false);
            playerFamily.RemoveRecord(FamilyEntitlement.SUMMON_FAMILY.Value);


            await _transport.Send(new Dto.UseEntitlementResponse { Code = 0 });

        }

        internal void ResetDailyReps()
        {
            var all = Query(x => true);
            foreach (var family in all)
            {
                family.Reptosenior = 0;
                family.Todaysrep = 0;
                family.EntitlementUseRecord.Clear();
            }
        }
    }
}
