using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.Shared.Login;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using net.server;
using net.server.coordinator.session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Core.Client
{
    public abstract class ClientBase : SocketClient, IClientBase
    {
        protected ClientBase(long sessionId, IServerBase<IServerTransport> currentServer, IChannel nettyChannel, ILogger<IClientBase> log) : base(sessionId, currentServer, nettyChannel, log)
        {
        }

        public AccountEntity? AccountEntity { get; set; }
        public bool IsServerTransition { get; private set; }

        public bool CheckBirthday(DateTime date)
        {
            if (AccountEntity == null)
                return false;

            return date.Month == AccountEntity.Birthday.Month && date.Day == AccountEntity.Birthday.Day;
        }

        public bool CheckBirthday(int dateInt)
        {
            if (DateTime.TryParse(dateInt.ToString(), out var d))
                return CheckBirthday(d);
            return false;
        }

        public virtual void updateLoginState(sbyte newState)
        {
            if (AccountEntity == null)
                return;

            CurrentServer.UpdateAccountState(AccountEntity.Id, newState);

            if (newState == AccountStage.LOGIN_NOTLOGGEDIN)
            {
                AccountEntity = null;
            }
            IsServerTransition = newState == AccountStage.LOGIN_SERVER_TRANSITION;
        }
        /// <summary>
        /// q
        /// </summary>
        /// <param name="cid"></param>
        public void SetCharacterOnSessionTransitionState(int cid)
        {
            this.updateLoginState(AccountStage.LOGIN_SERVER_TRANSITION);
            CurrentServer.SetCharacteridInTransition(GetSessionRemoteHost(), cid);
        }
        protected HashSet<string> GetMac()
        {
            if (AccountEntity == null || string.IsNullOrEmpty(AccountEntity.Macs))
                return [];

            return AccountEntity.Macs.Split(",").Select(x => x.Trim()).ToHashSet();
        }
        public void BanMacs()
        {
            try
            {
                using var dbContext = new DBContext();
                List<string> filtered = dbContext.Macfilters.Select(x => x.Filter).ToList();

                foreach (string mac in GetMac())
                {
                    if (!filtered.Any(x => Regex.IsMatch(mac, x)))
                    {
                        dbContext.Macbans.Add(new Macban(mac, AccountEntity!.Id.ToString()));
                    }
                }
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
            }
        }
    }
}
