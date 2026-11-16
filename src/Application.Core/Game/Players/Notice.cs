using Application.Core.scripting.npc;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public async Task TypedMessage(NoticeType type, string? messageKey, params object?[] param)
        {
            if (string.IsNullOrEmpty(messageKey))
            {
                return;
            }

            var formatedMessage = GetMessageByKey(messageKey, param);
            if (string.IsNullOrEmpty(formatedMessage))
            {
                return;
            }
            if (type == NoticeType.Yellow)
            {
                await SendPacket(PacketCommon.SendYellowTip(formatedMessage));
            }
            else if (type == NoticeType.EarnTitle)
            {
                await SendPacket(PacketCreator.earnTitleMessage(formatedMessage));
            }
            else if (type == NoticeType.NpcTalk)
            {
                await TempConversation.CreateScope(Client, async ctx =>
                {
                    await ctx.SayOK(formatedMessage);
                }, NpcId.MAPLE_ADMINISTRATOR);
            }
            else if (type == NoticeType.TopScrolling)
            {
                await SendPacket(PacketCommon.serverMessage(formatedMessage));
            }
            else
            {
                await SendPacket(PacketCommon.serverNotice((int)type, formatedMessage));
            }
        }
        public async Task Dialog(string? key, int npcId = NpcId.MAPLE_ADMINISTRATOR, params object?[] param)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var formatedMessage = GetMessageByKey(key, param);
            if (string.IsNullOrEmpty(formatedMessage))
            {
                return;
            }

            await TempConversation.CreateScope(Client, async ctx =>
            {
                await ctx.SayOK(formatedMessage);
            }, npcId);
        }

        public string? GetMessageByCategory(string category, string? key, params object?[] paramsValue)
            => Client.CurrentCulture.GetMessageByCategory(category, key, paramsValue);
    }
}
