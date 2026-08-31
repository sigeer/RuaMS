using Application.Core.scripting.npc;
using Application.Shared.MapObjects.Players;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public async Task TypedMessage(int type, string messageKey, params string[] param)
        {
            if (string.IsNullOrEmpty(messageKey))
            {
                return;
            }

            if (type == -1)
            {
                await SendPacket(PacketCommon.SendYellowTip(GetMessageByKey(messageKey, param)));
            }
            else if (type == -2)
            {
                await SendPacket(PacketCreator.earnTitleMessage(GetMessageByKey(messageKey, param)));
            }
            else if (type == -3)
            {
                await Dialog(messageKey, param: param);
            }
            else if (type == 4)
            {
                await SendPacket(PacketCommon.serverMessage(GetMessageByKey(messageKey, param)));
            }
            else
            {
                await SendPacket(PacketCommon.serverNotice(type, GetMessageByKey(messageKey, param)));
            }
        }
        public Task Notice(string key, params string[] param) => TypedMessage(0, key, param);

        public Task Popup(string key, params string[] param) => TypedMessage(1, key, param);

        public Task TopScrolling(string key, params string[] param) => TypedMessage(4, key, param);

        public Task Pink(string key, params string[] param) => TypedMessage(5, key, param);

        public Task LightBlue(string key, params string[] param) => TypedMessage(6, key, param);

        public Task Yellow(string key, params string[] param) => TypedMessage(-1, key, param);
        public Task EarnTitle(string key, params string[] param) => TypedMessage(-2, key, param);
        public async Task Dialog(string key, int npcId = NpcId.MAPLE_ADMINISTRATOR, params string[] param)
        {
            await TempConversation.CreateScope(Client, async ctx =>
            {
                await ctx.SayOK(GetMessageByKey(key, param));
            }, npcId);
        }

        public Task LightBlue(Func<IClientCulture, string> action)
        {
            return SendPacket(PacketCommon.serverNotice(6, action(Client.CurrentCulture)));
        }

    }
}
