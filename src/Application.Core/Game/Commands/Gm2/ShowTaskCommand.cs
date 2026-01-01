using Application.Core.Channel.Services;
using Application.Core.scripting.npc;
using System.Text;

namespace Application.Core.Game.Commands.Gm2
{
    internal class ShowTaskCommand : CommandBase
    {
        readonly AdminService _adminService;
        public ShowTaskCommand(AdminService adminService) : base(2, "showtask")
        {
            _adminService = adminService;
            Description = "列举当前频道服务器上的所有任务。";
        }

        public override Task Execute(IChannelClient client, string[] values)
        {
            var data = _adminService.GetChannelServerTasks();

            var message = new StringBuilder();
            message.Append("共有").Append(data.Count).Append("个任务正在运行：\r\n");
            foreach (var item in data)
            {
                message.Append("-").Append(item).Append("\r\n");
            }

            TempConversation.Create(client)?.RegisterTalk(message.ToString());
            return Task.CompletedTask;
        }
    }
}
