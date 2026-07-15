using scripting.npc;
using tools;

namespace Application.Core.scripting.npc
{
    public enum TempConversationType
    {
        Default,
        Select,
        InputNumber,
        InputText,
        YesNo
    }
    public class TempConversation : NPCConversationManager
    {
        private TempConversation(IChannelClient c, int npc = NpcId.MAPLE_ADMINISTRATOR) : base(c, npc)
        {
        }


        public static async Task<TempConversation?> CreateScope(IChannelClient c, Func<TempConversation, Task> action, int npc = NpcId.MAPLE_ADMINISTRATOR, bool force = false)
        {
            if (force)
                c.NPCConversationManager?.DisposeAsync();

            else if (c.NPCConversationManager != null)
            {
                await c.SendPacket(PacketCreator.sendYellowTip("有正在进行的对话，使用!dispose解卡"));
                return null;
            }

            await using var value = new TempConversation(c, npc);
            c.NPCConversationManager = value;
            await action(value);
            return value;
        }
    }
}
