using Application.Core.Channel.Events;
using Application.Core.Channel.ServerData;
using Application.Core.Game.Players;
using Application.Core.Game.Relation;
using Application.Core.ServerTransports;
using Application.Shared.Configs;
using Application.Shared.Invitations;
using Application.Shared.Login;
using Application.Shared.Servers;
using client;
using Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI.Common;
using net.server.guild;
using Org.BouncyCastle.Asn1.Ocsp;
using server;
using server.quest;
using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Security.Cryptography.Xml;
using tools;
using XmlWzReader;
using static Grpc.Core.Metadata;

namespace Application.Core.Channel
{
    public class WorldChannelServer : IServerBase<IChannelServerTransport>
    {
        readonly IServiceProvider _sp;
        public IChannelServerTransport Transport { get; }
        public Dictionary<int, WorldChannel> Servers { get; set; }
        public bool IsClosed { get; private set; }

        public ChannelServerConfig ServerConfig { get; set; }
        public string ServerName => ServerConfig.ServerName;
        public SkillbookInformationProvider SkillbookInformationProvider { get; }
        readonly ILogger<WorldChannelServer> _logger;

        public DateTimeOffset StartupTime { get; private set; }

        #region Data
        public GuildManager GuildManager { get; private set; } = null!;
        public TeamManager TeamManager { get; private set; } = null!;
        public ChatRoomService ChatRoomService { get; private set; } = null!;
        #endregion

        #region Task
        public CharacterDiseaseManager CharacterDiseaseManager { get; }
        public PetHungerManager PetHungerManager { get; }
        public ServerMessageManager ServerMessageManager { get; }
        public CharacterHpDecreaseManager CharacterHpDecreaseManager { get; }
        public MapObjectManager MapObjectManager { get; }
        public MountTirednessManager MountTirednessManager { get; }
        public MapOwnershipManager MapOwnershipManager { get; }
        #endregion

        #region GameConfig
        public float WorldMobRate { get; private set; }

        public float WorldMesoRate { get; private set; }
        public float WorldExpRate { get; private set; }
        public float WorldDropRate { get; private set; }
        public float WorldBossDropRate { get; private set; }
        public float WorldQuestRate { get; private set; }
        public float WorldTravelRate { get; private set; }
        public float WorldFishingRate { get; private set; }
        public string WorldServerMessage { get; private set; }

        public Dictionary<int, int> CouponRates { get; set; } = new(30);
        public List<int> ActiveCoupons { get; set; } = new();

        #endregion
        public List<IChannelModule> Plugins { get; }
        public WorldChannelServer(IServiceProvider sp, IChannelServerTransport transport, ChannelServerConfig serverConfig, ILogger<WorldChannelServer> logger)
        {
            _sp = sp;
            Transport = transport;
            _logger = logger;

            Servers = new();
            ServerConfig = serverConfig;

            SkillbookInformationProvider = _sp.GetRequiredService<SkillbookInformationProvider>();
            Plugins = _sp.GetServices<IChannelModule>().ToList();

            CharacterDiseaseManager = new CharacterDiseaseManager(this);
            PetHungerManager = new PetHungerManager(this);
            ServerMessageManager = new ServerMessageManager(this);
            CharacterHpDecreaseManager = new CharacterHpDecreaseManager(this);
            MapObjectManager = new MapObjectManager(this);
            MountTirednessManager = new MountTirednessManager(this);
            MapOwnershipManager = new MapOwnershipManager(this);
        }

        #region 时间
        private AtomicLong currentTime = new AtomicLong(0);
        private long serverCurrentTime = 0;

        public int getCurrentTimestamp()
        {
            return Transport.GetCurrentTimestamp();
        }

        public long getCurrentTime()
        {
            return serverCurrentTime;
        }
        public void UpdateServerTime()
        {
            serverCurrentTime = currentTime.addAndGet(YamlConfig.config.server.UPDATE_INTERVAL);
        }

        public void ForceUpdateServerTime()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var forceTime = Transport.GetCurrentTime();
            sw.Stop();
            forceTime = forceTime + sw.ElapsedMilliseconds;
            serverCurrentTime = forceTime;
            currentTime.set(forceTime);
        }
        #endregion
        public void UpdateCouponConfig(Config.CouponConfig config)
        {
            ActiveCoupons = config.ActiveCoupons.ToList();
            CouponRates = config.CouponRates.ToDictionary();
        }


        bool isShuttingdown = false;
        public async Task Shutdown()
        {
            if (isShuttingdown)
                return;

            isShuttingdown = true;

            await CharacterDiseaseManager.StopAsync();
            await PetHungerManager.StopAsync();
            await MapOwnershipManager.StopAsync();
            await ServerMessageManager.StopAsync();
            await CharacterHpDecreaseManager.StopAsync();
            await MapObjectManager.StopAsync();
            await MountTirednessManager.StopAsync();

            foreach (var channel in Servers.Values)
            {
                await channel.Shutdown();
            }
            IsClosed = true;
            isShuttingdown = false;
        }
        public async Task StartServer()
        {
            await Start();
            StartupTime = DateTimeOffset.UtcNow;
            ForceUpdateServerTime();
        }
        public async Task Start(int startPort = 7574, int count = 3)
        {
            _ = Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                _logger.LogInformation("服务器{ServerName}加载WZ - 能手册...", ServerName);
                SkillbookInformationProvider.LoadAllSkillbookInformation();
                _logger.LogInformation("服务器{ServerName}加载WZ - 能手册加载完成, 耗时{Cost}", ServerName, sw.Elapsed.TotalSeconds);
            });

            GuildManager = _sp.GetRequiredService<GuildManager>();
            TeamManager = _sp.GetRequiredService<TeamManager>();
            ChatRoomService = _sp.GetRequiredService<ChatRoomService>();

            CharacterDiseaseManager.Register();
            PetHungerManager.Register();
            ServerMessageManager.Register();
            CharacterHpDecreaseManager.Register();
            MapObjectManager.Register();
            MountTirednessManager.Register();
            MapOwnershipManager.Register();

            List<WorldChannel> localServers = [];
            // ping master
            for (int j = 1; j <= count; j++)
            {
                var config = new WorldChannelConfig(ServerName)
                {
                    Port = startPort + j
                };
                var scope = _sp.CreateScope();
                var channel = new WorldChannel(this, scope, config);
                await channel.StartServer();
                if (channel.IsRunning)
                {
                    localServers.Add(channel);
                }
            }

            var configs = await Transport.RegisterServer(this, localServers);
            if (configs.StartChannel > 0)
            {
                ForceUpdateServerTime();

                foreach (var server in localServers)
                {
                    var channel = configs.StartChannel++;
                    server.Register(channel);
                    Servers[channel] = server;
                }

                UpdateWorldConfig(new WorldConfigPatch
                {
                    MobRate = configs.Config.MobRate,
                    MesoRate = configs.Config.MesoRate,
                    ExpRate = configs.Config.ExpRate,
                    DropRate = configs.Config.DropRate,
                    BossDropRate = configs.Config.BossDropRate,
                    QuestRate = configs.Config.QuestRate,
                    TravelRate = configs.Config.TravelRate,
                    FishingRate = configs.Config.FishingRate,
                    ServerMessage = configs.Config.ServerMessage
                });
                UpdateCouponConfig(configs.Coupon);

                foreach (var server in Servers.Values)
                {
                    server.Initialize();
                }
            }
            else
            {
                _logger.LogError("注册服务器失败, {Message}", configs.Message);
            }

        }

        public IPlayer? FindPlayerById(int cid)
        {
            foreach (var item in Servers.Values)
            {
                var chr = item.Players.getCharacterById(cid);
                if (chr != null)
                    return chr;
            }
            return null;
        }

        public IPlayer? FindPlayerById(int channel, int cid)
        {
            if (Servers.TryGetValue(channel, out var ch))
                return ch.Players.getCharacterById(cid);

            return null;
        }

        internal WorldChannel? GetChannel(int channel)
        {
            return Servers.GetValueOrDefault(channel);
        }

        public void CommitAccountEntity(AccountCtrl accountEntity)
        {
            Transport.UpdateAccount(accountEntity);
        }

        public AccountLoginStatus UpdateAccountState(int accId, sbyte state)
        {
            return Transport.UpdateAccountState(accId, state);
        }

        public void SetCharacteridInTransition(string v, int cid)
        {
            if (YamlConfig.config.server.USE_IP_VALIDATION)
                Transport.SetCharacteridInTransition(v, cid);
        }

        public bool HasCharacteridInTransition(string clientSession)
        {
            return Transport.HasCharacteridInTransition(clientSession);
        }

        public void BroadcastWorldMessage(Packet p)
        {
            Transport.BroadcastMessage(p);
        }
        public void BroadcastWorldGMPacket(Packet packet)
        {
            Transport.BroadcastGMMessage(packet);
        }

        public void UpdateWorldConfig(WorldConfigPatch updatePatch)
        {
            if (updatePatch.MobRate.HasValue)
            {
                WorldMobRate = updatePatch.MobRate.Value;
            }
            if (updatePatch.MesoRate.HasValue)
            {
                WorldMesoRate = updatePatch.MesoRate.Value;
            }
            if (updatePatch.ExpRate.HasValue)
            {
                WorldExpRate = updatePatch.ExpRate.Value;
            }
            if (updatePatch.DropRate.HasValue)
            {
                WorldDropRate = updatePatch.DropRate.Value;
            }
            if (updatePatch.BossDropRate.HasValue)
            {
                WorldBossDropRate = updatePatch.BossDropRate.Value;
            }
            if (updatePatch.TravelRate.HasValue)
            {
                WorldTravelRate = updatePatch.TravelRate.Value;
            }
            if (updatePatch.FishingRate.HasValue)
            {
                WorldFishingRate = updatePatch.FishingRate.Value;
            }
            if (updatePatch.ServerMessage != null)
            {
                WorldServerMessage = updatePatch.ServerMessage;
            }
            foreach (var server in Servers.Values)
            {
                server.UpdateWorldConfig(updatePatch);
            }
        }

        public void SendMultiChat(int type, string nameFrom, int[] value, string chatText)
        {
            foreach (var server in Servers.Values)
            {
                foreach (var cid in value)
                {
                    var chr = server.Players.getCharacterById(cid);
                    if (chr != null)
                    {
                        chr.sendPacket(PacketCreator.multiChat(nameFrom, chatText, type));
                    }
                }
            }
        }

        public IPEndPoint GetChannelEndPoint(int channel)
        {
            return Transport.GetChannelEndPoint(channel);
        }

        public bool WarpPlayer(string name, int? channel, int mapId, int? portal)
        {
            return Transport.WarpPlayer(name, channel, mapId, portal);
        }

        public string GetExpeditionInfo()
        {
            return Transport.LoadExpeditionInfo();
        }

        public bool CheckCharacterName(string name)
        {
            return Transport.CheckCharacterName(name);
        }

        public void NotifyPartner(int id)
        {
            Transport.NotifyPartner(id);
        }

        public void DropMessage(int[] value, int type, string message)
        {
            foreach (var ch in Servers.Values)
            {
                foreach (var player in ch.Players.getAllCharacters())
                {
                    player.dropMessage(type, message);
                }
            }
        }

        public void BroadcastGuildGPUpdate(Dto.UpdateGuildGPResponse response)
        {
            GuildManager.OnGuildGPUpdate(response);
        }

        public void OnPlayerJobChanged(Dto.PlayerLevelJobChange data)
        {
            if (data.GuildId > 0)
            {
                var guild = GuildManager.GetGuildById(data.GuildId);
                if (guild != null)
                {
                    guild.SetMemberJob(data.Id, data.JobId);
                    guild.broadcast(PacketCreator.jobMessage(0, data.JobId, data.Name), data.Id);
                    guild.broadcast(GuildPackets.guildMemberLevelJobUpdate(data.GuildId, data.Id, data.Level, data.JobId));

                    if (guild.AllianceId > 0)
                    {
                        var alliance = GuildManager.GetAllianceById(guild.AllianceId);
                        if (alliance != null)
                        {
                            alliance.broadcastMessage(GuildPackets.updateAllianceJobLevel(guild, data.Id, data.Level, data.JobId), data.Id, -1);
                        }
                    }
                }
            }
            foreach (var plugin in Plugins)
            {
                plugin.OnPlayerChangeJob(data);
            }
        }

        public void OnPlayerLevelChanged(Dto.PlayerLevelJobChange data)
        {
            if (data.GuildId > 0)
            {
                var guild = GuildManager.GetGuildById(data.GuildId);
                if (guild != null)
                {
                    guild.SetMemberLevel(data.Id, data.Level);
                    guild.broadcast(PacketCreator.levelUpMessage(0, data.Level, data.Name), data.Id);
                    guild.broadcast(GuildPackets.guildMemberLevelJobUpdate(data.GuildId, data.Id, data.Level, data.JobId));

                    if (guild.AllianceId > 0)
                    {
                        var alliance = GuildManager.GetAllianceById(guild.AllianceId);
                        if (alliance != null)
                        {
                            alliance.broadcastMessage(GuildPackets.updateAllianceJobLevel(guild, data.Id, data.Level, data.JobId), data.Id, -1);
                        }
                    }

                }
            }

            //if (data.TeamId > 0)
            //{
            //    var team = TeamManager.GetParty(data.TeamId);
            //    if (team != null)
            //    {
            //        team.UpdateMemberLevel(data.Id, data.Level);
            //    }
            //}
        }

        public void OnPlayerLoginOff(Dto.PlayerOnlineChange data)
        {
            if (data.GuildId > 0)
            {
                var guild = GuildManager.GetGuildById(data.GuildId);
                if (guild != null)
                {
                    guild.SetMemberChannel(data.Id, data.Channel);
                    guild.setOnline(data.Id, data.Channel > 0, data.Channel);

                    var chr = FindPlayerById(data.Channel, data.Id);
                    if (chr != null)
                        chr.sendPacket(GuildPackets.showGuildInfo(chr));

                    if (guild.AllianceId > 0)
                    {
                        var alliance = GuildManager.GetAllianceById(guild.AllianceId);
                        if (alliance != null)
                        {
                            if (chr != null)
                            {
                                chr.sendPacket(GuildPackets.updateAllianceInfo(alliance));
                                chr.sendPacket(GuildPackets.allianceNotice(alliance.AllianceId, alliance.getNotice()));
                            }

                            if (data.IsNewComer)
                                alliance.broadcastMessage(GuildPackets.allianceMemberOnline(guild, data.Id, true), data.Id);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 成功：向受邀者发送请求，失败：向邀请者发送失败原因
        /// </summary>
        /// <param name="data"></param>
        public void OnSendInvitation(Dto.CreateInviteResponse data)
        {
            var type = (InviteTypeEnum)data.Type;
            switch (type)
            {
                case InviteTypeEnum.FAMILY:
                    break;
                case InviteTypeEnum.FAMILY_SUMMON:
                    ProcessFamilySummonInvite(data);
                    break;
                case InviteTypeEnum.MESSENGER:
                    break;
                case InviteTypeEnum.TRADE:
                    break;
                case InviteTypeEnum.PARTY:
                    {
                        var code = (InviteResponseCode)data.Code;
                        if (code == InviteResponseCode.Success)
                        {
                            var receiver = FindPlayerById(data.ReceivePlayerId);
                            if (receiver != null)
                            {
                                receiver.sendPacket(PacketCreator.partyInvite(data.Key, data.SenderPlayerName));
                            }
                        }
                        else
                        {
                            var sender = FindPlayerById(data.SenderPlayerId);
                            if (sender != null)
                            {
                                switch (code)
                                {
                                    case InviteResponseCode.MANAGING_INVITE:
                                        sender.sendPacket(PacketCreator.partyStatusMessage(22, data.ReceivePlayerName));
                                        break;
                                    case InviteResponseCode.InviteesNotFound:
                                        sender.sendPacket(PacketCreator.partyStatusMessage(19));
                                        break;
                                    case InviteResponseCode.Team_AlreadyInTeam:
                                        sender.sendPacket(PacketCreator.partyStatusMessage(16));
                                        break;
                                    case InviteResponseCode.Team_CapacityFull:
                                        sender.sendPacket(PacketCreator.partyStatusMessage(17));
                                        break;
                                    case InviteResponseCode.Team_BeginnerLimit:
                                        sender.sendPacket(PacketCreator.serverNotice(5, "The player you have invited does not meet the requirements."));
                                        break;
                                    default:
                                        _logger.LogCritical("预料之外的邀请回调: Type:{Type}, Code: {Code}", type, code);
                                        break;
                                }
                            }
                        }
                    }
                    break;
                case InviteTypeEnum.GUILD:
                    {
                        var code = (GuildResponse)data.Code;
                        if (code == GuildResponse.Success)
                        {
                            var mc = FindPlayerById(data.ReceivePlayerId);
                            if (mc != null)
                            {
                                mc.sendPacket(GuildPackets.guildInvite(data.Key, data.SenderPlayerName));
                            }

                        }
                        else
                        {
                            var sender = FindPlayerById(data.SenderPlayerId);
                            if (sender != null)
                            {
                                sender.sendPacket(code.getPacket(data.ReceivePlayerName));
                            }
                        }
                    }
                    break;
                case InviteTypeEnum.ALLIANCE:
                    {
                        var code = (InviteResponseCode)data.Code;
                        if (code == InviteResponseCode.Success)
                        {
                            var receiver = FindPlayerById(data.ReceivePlayerId);
                            if (receiver != null)
                            {
                                receiver.sendPacket(GuildPackets.allianceInvite(data.Key, receiver));
                            }

                        }
                        else
                        {
                            var sender = FindPlayerById(data.SenderPlayerId);
                            if (sender != null)
                            {
                                // sender.dropMessage(5, "The master of the guild that you offered an invitation is currently managing another invite.");
                                switch (code)
                                {
                                    case InviteResponseCode.Alliance_AlreadyInAlliance:
                                        sender.dropMessage(5, "The entered guild is already registered on a guild alliance.");
                                        break;
                                    case InviteResponseCode.Alliance_GuildNotFound:
                                        sender.dropMessage(5, "The entered guild does not exist.");
                                        break;
                                    case InviteResponseCode.Alliance_GuildLeaderNotFound:
                                        sender.dropMessage(5, "The master of the guild that you offered an invitation is currently not online.");
                                        break;
                                    case InviteResponseCode.Alliance_CapacityFull:
                                        sender.dropMessage(5, "Your alliance cannot comport any more guilds at the moment.");
                                        break;
                                    default:
                                        _logger.LogCritical("预料之外的邀请回调: Type:{Type}, Code: {Code}", type, code);
                                        break;
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void ProcessFamilySummonInvite(Dto.CreateInviteResponse data)
        {
            {
                var code = (InviteResponseCode)data.Code;
                if (code == InviteResponseCode.Success)
                {
                    var receiver = FindPlayerById(data.ReceivePlayerId);
                    if (receiver != null)
                    {
                        receiver.sendPacket(PacketCreator.sendFamilySummonRequest(receiver.getFamily()!.getName(), data.SenderPlayerName));
                    }
                    var sender = FindPlayerById(data.SenderPlayerId);
                    if (sender != null)
                    {
                        var entry = sender.getFamilyEntry();
                        if (entry.useEntitlement(FamilyEntitlement.SUMMON_FAMILY))
                        {
                            entry.gainReputation(-FamilyEntitlement.SUMMON_FAMILY.getRepCost(), false);
                            sender.sendPacket(PacketCreator.getFamilyInfo(entry));
                        }
                    }
                }
                else
                {
                    var sender = FindPlayerById(data.SenderPlayerId);
                    if (sender != null)
                    {
                        switch (code)
                        {
                            case InviteResponseCode.MANAGING_INVITE:
                                sender.sendPacket(PacketCreator.sendFamilyMessage(74, 0));
                                break;
                            case InviteResponseCode.InviteesNotFound:
                                // 
                                break;
                            default:
                                _logger.LogCritical("预料之外的邀请回调: Type:{Type}, Code: {Code}", "FamilySummon", code);
                                break;
                        }
                    }
                }
            }
        }

        public void OnAnswerInvitation(AnswerInviteResponse data)
        {
            var type = (InviteTypeEnum)data.Type;
            var result = (InviteResultType)data.Result;
            switch (type)
            {
                case InviteTypeEnum.FAMILY:
                    break;
                case InviteTypeEnum.FAMILY_SUMMON:
                    ProcessFamilySummonInviteAnswer(data);
                    break;
                case InviteTypeEnum.MESSENGER:
                    break;
                case InviteTypeEnum.TRADE:
                    break;
                case InviteTypeEnum.PARTY:
                    {
                        if (result != InviteResultType.ACCEPTED)
                        {
                            var sender = FindPlayerById(data.SenderPlayerId);
                            if (sender != null)
                            {
                                sender.sendPacket(PacketCreator.serverNotice(5, "You couldn't join the party due to an expired invitation request."));
                            }
                        }
                    }
                    break;
                case InviteTypeEnum.GUILD:
                    {
                        if (result != InviteResultType.ACCEPTED)
                        {
                            var sender = FindPlayerById(data.SenderPlayerId);
                            if (sender != null)
                            {
                                var code = result == InviteResultType.DENIED ? GuildResponse.DENIED_INVITE : GuildResponse.NOT_FOUND_INVITE;
                                sender.sendPacket(code.getPacket(data.ReceivePlayerName));
                            }
                        }
                    }
                    break;
                case InviteTypeEnum.ALLIANCE:
                    {
                        if (result != InviteResultType.ACCEPTED)
                        {
                            var sender = FindPlayerById(data.SenderPlayerId);
                            if (sender != null)
                            {
                                string msg = "";
                                if (result == InviteResultType.DENIED)
                                    msg = "[" + data.TargetName + "] guild has denied your guild alliance invitation.";
                                if (result == InviteResultType.NOT_FOUND)
                                    msg = "The guild alliance request has not been accepted, since the invitation expired.";
                                sender.dropMessage(5, msg);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void ProcessFamilySummonInviteAnswer(AnswerInviteResponse data)
        {
            var result = (InviteResultType)data.Result;

            if (result != InviteResultType.ACCEPTED)
            {
                var sender = FindPlayerById(data.SenderPlayerId);
                if (sender != null)
                {
                    var inviterEntry = sender.getFamilyEntry();
                    if (inviterEntry == null)
                    {
                        return;
                    }

                    inviterEntry.refundEntitlement(FamilyEntitlement.SUMMON_FAMILY);
                    inviterEntry.gainReputation(FamilyEntitlement.SUMMON_FAMILY.getRepCost(), false); //refund rep cost if declined

                    if (result == InviteResultType.DENIED)
                    {
                        sender.sendPacket(PacketCreator.getFamilyInfo(inviterEntry));
                        sender.dropMessage(5, data.ReceivePlayerName + " has denied the summon request.");
                    }
                }
            }
            else
            {
                var receiver = FindPlayerById(data.ReceivePlayerId);
                if (receiver != null)
                {
                    receiver.changeMap(data.Key);
                }
            }

        }
    }
}
