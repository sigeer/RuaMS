using Application.Core.scripting.Infrastructure;
using scripting.npc;
using tools;

namespace Application.Core.scripting.npc
{
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
            try
            {
                c.NPCConversationManager = value;
                await action(value);
            }
            catch (ConversationInterruptException)
            {
                // 结束对话
                return value;
            }
            catch (ConversationDiffInstanceException cdiEx)
            {
                await c.OnlinedCharacter.Pink("你是怎么到这里来的？让我带你离开这里。");
                Log.Logger.Error(cdiEx, "临时对话失败");
                await c.OnlinedCharacter.ForcedWarpOut();
                return value;
            }
            catch (BusinessException be)
            {
                await c.OnlinedCharacter.Debug(NoticeType.Pink, be.Message);
                Log.Logger.Error(be, "临时对话失败");
                return value;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "临时对话失败");
                return value;
            }
            return value;
        }
    }
}
