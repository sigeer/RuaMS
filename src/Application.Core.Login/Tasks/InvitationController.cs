using Application.Core.Game.Controllers;
using Application.Core.Game.Invites;
using Application.Utility;

namespace Application.Core.Login.Tasks
{
    /// <summary>
    /// 邀请过期检查
    /// </summary>
    public class InvitationController : TimelyControllerBase
    {
        readonly MasterServer _server;
        public List<InviteType> AllInviteTypes { get; set; } = EnumClassUtils.GetValues<InviteType>();

        public InvitationController(MasterServer server)
            : base("InvitationExpireCheckTask", TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30))
        {
            _server = server;
        }

        public void RemovePlayerIncomingInvites(int cid)
        {
            foreach (InviteType it in AllInviteTypes)
            {
                it.RemoveRequest(cid);
            }
        }

        protected override void HandleRun()
        {
            var now = _server.getCurrentTime();
            foreach (InviteType it in AllInviteTypes)
            {
                it.CheckExpired(now);
            }
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            foreach (var item in AllInviteTypes)
            {
                item.Dispose();
            }
        }
    }

}

