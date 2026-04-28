using Application.Core.Channel;
using Application.Core.scripting.Events.Abstraction;
using Application.Resources.Messages;
using System.Timers;
using tools;
using tools.exceptions;

namespace Application.Core.Scripting.Events
{
    public abstract class ExpeditionEventManager : AbstractInstancedEventManager
    {
        public int BossId { get; }
        public int RegistrationTime { get; init; }
        public ExpeditionEventManager(WorldChannel cserv, string name, int bossId) : base(cserv, name)
        {
            BossId = bossId;

            MaxLobbys = 1;
        }

        protected override AbstractEventInstanceManager CreateNewInstance(string instanceName)
        {
            return new ExpeditionEventInstanceManager(cserv, Name, Name);
        }

        public ExpeditionEventInstanceManager? GetExpeditionEventInstanceManager() => getInstance(Name) as ExpeditionEventInstanceManager;


        public override CreateInstanceResult StartInstance(Player leader, int difficulty = 1, int lobbyId = -1)
        {
            if (this.isDisposed())
            {
                return CreateInstanceResult.Disposed;
            }

            try
            {
                if (!playerPermit.Contains(leader.getId()) && startSemaphore.Wait(7777))
                {
                    playerPermit.Add(leader.getId());

                    try
                    {
                        try
                        {
                            if (lobbyId == -1)
                            {
                                lobbyId = GetAvailableLobbyInstance();
                                if (lobbyId == -1)
                                {
                                    return CreateInstanceResult.LobbyLimited;
                                }
                            }
                            else
                            {
                                if (!TryRegisterLobby(lobbyId))
                                {
                                    return CreateInstanceResult.LobbyLimited;
                                }
                            }

                            ExpeditionEventInstanceManager? eim = null;
                            try
                            {
                                eim = CreateInstance(1, lobbyId) as ExpeditionEventInstanceManager;
                                registerEventInstance(eim, lobbyId);
                            }
                            catch (EventInstanceInProgressException)
                            {
                                UnregisterLobby(lobbyId);
                                throw;
                            }

                            eim.setLeader(leader);
                            eim.registerPlayer(leader);

                            eim.startEvent();
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "Event script startInstance");
                            return CreateInstanceResult.Unknown;
                        }

                        return CreateInstanceResult.Success;
                    }
                    finally
                    {
                        playerPermit.Remove(leader.getId());
                        startSemaphore.Release();
                    }
                }
            }
            catch (ThreadInterruptedException ie)
            {
                log.Error(ie.ToString());
                playerPermit.Remove(leader.getId());
            }

            return CreateInstanceResult.Unknown;
        }

        public override AbstractEventInstanceManager Setup(int level, int lobbyId)
        {
            var eim = newInstance(_name + lobbyId);
            eim.setProperty("level", level);

            OnSetup(eim, level, lobbyId);

            respawnStages(eim);

            eim.startEventTimer(RegistrationTime * 1000);

            return eim;
        }

        public override void AfterSeup(AbstractEventInstanceManager eim)
        {
            base.AfterSeup(eim);

            eim.getLeader().getMap().BroadcastAll(e =>
            {
                if (e != eim.getLeader())
                    e.LightBlue(nameof(ClientMessage.Expedition_Captain_NoticeMap));
            });
            eim.getLeader()?.LightBlue(nameof(ClientMessage.Expedition_Captain_Notice));
        }

        public override void OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            var pEim = eim as ExpeditionEventInstanceManager;
            if (pEim.isRegistering())
            {
                chr.sendPacket(PacketCreator.getClock((int)(eim.getTimeLeft() / 1000)));
            }
            else
            {
                base.OnPlayerEntry(eim, chr);
            }
        }

        public override void OnPlayerExit(AbstractEventInstanceManager eim, Player player)
        {
            var pEim = eim as ExpeditionEventInstanceManager;
            if (pEim.isRegistering())
            {
                eim.unregisterPlayer(player);
                player.sendPacket(PacketCreator.removeClock());

                eim.LightBlue(nameof(ClientMessage.Expedition_Left), player.Name);
                player.LightBlue(nameof(ClientMessage.Expedition_ChrLeft));
            }
            else
            {
                base.OnPlayerExit(eim, player);
            }
        }

        public override void OnTimeOut(AbstractEventInstanceManager eim)
        {
            var pEim = eim as ExpeditionEventInstanceManager;
            if (pEim.isRegistering())
            {
                eim.LightBlue(nameof(ClientMessage.Expedition_Timeout_Disband));
            }
            else
            {
                base.OnTimeOut(eim);
            }
        }

        public override string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    return "#r" + c.CurrentCulture.GetMobName(BossId) + " 远征#k 已经创建。\r\n\r\n再次与我交谈，查看当前队伍，或开始战斗！";
                case CreateInstanceResult.LobbyLimited:
                    return "抱歉，您已经达到了此次远征的尝试配额！请另选他日再试……";
                default:
                    return "在开始远征时发生了意外错误，请稍后重试。";
            }
        }

    }
}
