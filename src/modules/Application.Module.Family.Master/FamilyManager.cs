using Application.Core.Login;
using Application.EF;
using Application.Module.Family.Common;
using Application.Utility.Exceptions;
using AutoMapper;
using Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Application.Module.Family.Master
{
    public class FamilyManager
    {
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly MasterServer _server;
        readonly ILogger<FamilyManager> _logger;
        readonly DataService _dataService;
        readonly FamilyConfigs _config;
        readonly IMapper _mapper;
        readonly MasterFamilyModuleTransport _transport;

        ConcurrentDictionary<int, FamilyModel> _dataSource = new();
        ConcurrentDictionary<int, FamilyModel> _chrFamilyDataSource = new();
        int _currentId = 1;

        public FamilyManager(
            IDbContextFactory<DBContext> dbContextFactory,
            MasterServer server,
            ILogger<FamilyManager> logger,
            DataService dataService,
            IMapper mapper,
            IOptions<FamilyConfigs> options,
            MasterFamilyModuleTransport transport)
        {
            _dbContextFactory = dbContextFactory;
            _server = server;
            _logger = logger;
            _dataService = dataService;
            _mapper = mapper;
            _config = options.Value;
            _transport = transport;
        }

        public async Task LoadAllFamilyAsync(DBContext dbContext)
        {
            var allData = await dbContext.FamilyCharacters.AsNoTracking().ToListAsync();
            _dataSource = new(allData.GroupBy(x => x.Familyid).ToDictionary(
                x => x.Key,
                x => new FamilyModel(x.Key)
                {
                    Members = new(x.Where(y => y.Familyid == x.Key).ToList().ToDictionary(y => y.Cid, y => _mapper.Map<FamilyMemberModel>(y)))
                }));
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
            var resetTime = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeMilliseconds();
            try
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                try
                {
                    dbContext.FamilyCharacters.Where(x => x.Lastresettime <= resetTime).ExecuteUpdate(x => x.SetProperty(y => y.Todaysrep, 0).SetProperty(y => y.Reptosenior, 0));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not reset daily rep for families");
                }
                try
                {
                    dbContext.FamilyEntitlements.Where(x => x.Timestamp <= resetTime).ExecuteDelete();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not do daily reset for family entitlements");
                }
                dbTrans.Commit();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get connection to DB");
            }
        }

        public Dto.GetFamilyResponse AcceptInvite(int masterId, int memberId)
        {
            var memberChr = _server.CharacterManager.FindPlayerById(memberId)!;
            if (_dataSource.TryGetValue(memberChr.Character.FamilyId, out var theFamily))
            {
                // 接受邀请者已有学院
                if (theFamily.Members.Values.Any(x => x.Seniorid == 0 && x.Cid == masterId))
                    return new Dto.GetFamilyResponse { Code = ErrorCode.NotLeader };
            }

            var masterChr = _server.CharacterManager.FindPlayerById(masterId)!;
            if (_dataSource.TryGetValue(masterChr.Character.FamilyId, out var family))
            {
                if (theFamily != null)
                {
                    if (GetDepthOfMember(family.Members, masterId) + GetAllJuniorsAndDepth(theFamily.Members, memberId).maxDepth > _config.FAMILY_MAX_GENERATIONS)
                    {
                        return new Dto.GetFamilyResponse { Code = ErrorCode.OverMaxGenerations };
                    }
                }

            }
            else
            {
                family = new FamilyModel(Interlocked.Increment(ref _currentId));
                var masterMember = new FamilyMemberModel { Cid = masterId, Seniorid = 0 };
                family.Members[masterMember.Cid] = masterMember;
                _dataSource[family.Id] = family;
                _dataService.SetDirty(family);
            }


            // 对方有学院、则合并。没有则加入
            if (theFamily != null)
            {
                foreach (var item in theFamily.Members.Values)
                {
                    var chr = _server.CharacterManager.FindPlayerById(item.Cid);
                    if (chr != null)
                        chr.Character.FamilyId = family.Id;

                    if (item.Cid == memberId)
                    {
                        item.Seniorid = masterId;
                        item.Reptosenior = 0;
                    }
                }
            }
            else
            {
                var member = new FamilyMemberModel { Cid = memberId, Seniorid = masterId };
                family.Members[member.Cid] = member;
            }
            return new Dto.GetFamilyResponse { Model = _mapper.Map<Dto.FamilyDto>(family) };
        }

        public Dto.GetFamilyResponse ForkFamily(int id)
        {
            var masterChr = _server.CharacterManager.FindPlayerById(id)!;

            if (!_dataSource.TryGetValue(masterChr.Character.FamilyId, out var family))
                return new Dto.GetFamilyResponse();

            var master = family.Members.GetValueOrDefault(id);
            if (master == null)
                throw new BusinessFatalException($"FamilyId 与 Family成员不匹配， FamilyId = {masterChr.Character.FamilyId}, CharacterId = {id}");


            var members = GetAllChildren(family.Members.Values.ToList(), id);
            foreach (var item in members)
            {
                family.Members.TryRemove(item.Cid, out _);
            }

            var newFamily = new FamilyModel(Interlocked.Increment(ref _currentId));

            family.Members.TryRemove(id, out _);
            master.Seniorid = 0;
            master.Reptosenior = 0;
            masterChr.Character.FamilyId = newFamily.Id;
            newFamily.Members[id] = master;

            foreach (var item in members)
            {
                newFamily.Members[item.Cid] = item;
                var itemChr = _server.CharacterManager.FindPlayerById(item.Cid);
                if (itemChr != null)
                    itemChr.Character.FamilyId = newFamily.Id;
            }
            _dataSource[newFamily.Id] = newFamily;

            _dataService.SetDirty(family);
            _dataService.SetDirty(newFamily);

            return new Dto.GetFamilyResponse { Model = _mapper.Map<Dto.FamilyDto>(newFamily) };
        }

        internal FamilyModel? GetFamily(int familyId)
        {
            return _dataSource.GetValueOrDefault(familyId);
        }
        internal FamilyModel? GetFamilyByPlayer(int chrId)
        {
            return _chrFamilyDataSource.GetValueOrDefault(chrId);
        }
        List<FamilyMemberModel> GetAllChildren(IEnumerable<FamilyMemberModel> flatList, int parentId)
        {
            var result = new List<FamilyMemberModel>();

            // 找到直接子项
            var directChildren = flatList.Where(n => n.Seniorid == parentId).ToList();

            foreach (var child in directChildren)
            {
                result.Add(child);
                result.AddRange(GetAllChildren(flatList, child.Cid)); // 递归查找孙子节点
            }

            return result;
        }

        public static int GetDepthOfMember(IDictionary<int, FamilyMemberModel> memberDict, int cid)
        {
            int depth = 0;
            int currentId = cid;

            while (memberDict.TryGetValue(currentId, out var member))
            {
                if (member.Seniorid == 0 || member.Seniorid == member.Cid)
                    break; // 到达最上层或异常数据（避免死循环）

                depth++;
                currentId = member.Seniorid;
            }

            return depth;
        }

        public static (List<FamilyMemberModel> juniors, int maxDepth) GetAllJuniorsAndDepth(
            IDictionary<int, FamilyMemberModel> memberDict,
            int rootCid)
        {
            var juniors = new List<FamilyMemberModel>();
            var queue = new Queue<(FamilyMemberModel member, int depth)>();
            var allMembers = memberDict.Values.ToList();
            int maxDepth = 0;

            if (!memberDict.TryGetValue(rootCid, out var root))
                return (juniors, maxDepth); // root 不存在，返回空

            queue.Enqueue((root, 0));

            while (queue.Count > 0)
            {
                var (current, depth) = queue.Dequeue();
                maxDepth = Math.Max(maxDepth, depth);

                // 找所有 junior
                var children = allMembers.Where(m => m.Seniorid == current.Cid).ToList();
                foreach (var child in children)
                {
                    juniors.Add(child);
                    queue.Enqueue((child, depth + 1));
                }
            }

            return (juniors, maxDepth);
        }

        public void UseEntitlement(Dto.UseEntitlementRequest request)
        {
            if (_chrFamilyDataSource.TryGetValue(request.MatserId, out var family))
            {
                if (family.Members.TryGetValue(request.MatserId, out var member))
                {
                    var entitlement = FamilyEntitlement.Parse(request.EntitlementId);
                    if (member.Reputation < entitlement.getRepCost())
                    {
                        _transport.Send(request.MatserId, new Dto.UseEntitlementResponse { Code = 1 });
                        return;
                    }
                    if (member.EntitlementUseRecord.Count(x => x.Id == request.EntitlementId) >= entitlement.getUsageLimit())
                    {
                        _transport.Send(request.MatserId, new Dto.UseEntitlementResponse { Code = 1 });
                        return;
                    }


                    if (request.EntitlementId == FamilyEntitlement.FAMILY_REUINION.Value)
                    {
                        _server.Transport.SummonPlayer(request.MatserId, request.TargetPlayerId);
                    }
                    var chr = _server.CharacterManager.FindPlayerById(member.Cid);
                    GainReputation(member, -entitlement.getRepCost(), false, chr.Character.Name);
                    member.Reputation -= entitlement.getRepCost();
                    member.EntitlementUseRecord.Add(new FamilyEntitlementUseRecord(request.EntitlementId));

                    _transport.Send(request.MatserId, new Dto.UseEntitlementResponse { Code = 0 });
                }
            }
        }

        void GainReputation(FamilyMemberModel member, int gain, bool countTowardsTotal, string? from = null)
        {
            member.Reputation += gain;
            member.Todaysrep += gain;
            if (gain > 0 && countTowardsTotal)
            {
                member.Totalreputation += gain;
            }
        }

        public void Refund(int usePlayer)
        {
            if (_chrFamilyDataSource.TryGetValue(usePlayer, out var family))
            {
                if (family.Members.TryGetValue(usePlayer, out var member))
                {
                    GainReputation(member, FamilyEntitlement.SUMMON_FAMILY.getRepCost(), false);
                    member.RemoveRecord(FamilyEntitlement.SUMMON_FAMILY.Value);


                    _transport.Send(usePlayer, new Dto.UseEntitlementResponse { Code = 0 });
                }
            }

        }

        internal void ResetDailyReps()
        {
            foreach (var family in _dataSource.Values)
            {
                foreach (var member in family.Members.Values)
                {
                    member.Reptosenior = 0;
                    member.Todaysrep = 0;
                    member.EntitlementUseRecord.Clear();
                }
            }
        }
    }
}
