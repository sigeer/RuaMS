using Polly;
using scripting.npc;
using System.Threading.Channels;
using tools;
using static System.Net.Mime.MediaTypeNames;

namespace Application.Core.scripting.Infrastructure
{
    public record TalkMoreAction(sbyte mode, sbyte type, int selection, string? inputText);
    public class ConversationContext : IDisposable
    {
        public bool Finished { get; set; }
        public NPCConversationManager NPCConversationManager { get; }
        public List<ConversationItem> Items { get; } = [];

        public Channel<TalkMoreAction> TalkChannel { get; } = System.Threading.Channels.Channel.CreateBounded<TalkMoreAction>(1);

        int _idx = -1;

        public ConversationContext(NPCConversationManager nPCConversationManager)
        {
            NPCConversationManager = nPCConversationManager;
        }

        public void NewMessage(ConversationItem item)
        {
            if (Items.Contains(item))
            {
                Items.Add(item);
                _idx++;
            }
        }

        public ConversationItem GetPreviouse()
        {
            return Items[--_idx];
        }

        public void Dispose()
        {
            Items.Clear();
            TalkChannel.Writer.TryComplete();
        }
    }

    public class ConversationItem
    {
        public ConversationItem(ConversationContext context, NextLevelType type, string message, byte speaker = 0)
        {
            Context = context;
            Type = type;
            Message = message;
            Speaker = speaker;

            Context.NewMessage(this);
        }

        public NextLevelType Type { get; }
        public string Message { get; }
        public byte Speaker { get; }
        public ConversationContext Context { get; }

        public async Task WaitingForPrevious()
        {
            var action = await Context.TalkChannel.Reader.ReadAsync();
            if (action.mode == 0)
            {
                var last = Context.GetPreviouse();
                await Context.NPCConversationManager.SendNext(last);
            }
            else
            {
                Context.Finished = true;
            }
            
        }

        public async Task<int> WaitingForOption()
        {
            var action = await Context.TalkChannel.Reader.ReadAsync();
            if (action.mode <= 0)
            {
                Context.Finished = true;
                return -1;
            }
            return action.selection;
        }

        public async Task<bool> WaitingForAnswer()
        {
            var action = await Context.TalkChannel.Reader.ReadAsync();
            if (action.mode == -1)
            {
                Context.Finished = true;
            }

            return action.mode > 0;
        }

        public async Task<int> WaitingForInputNumber()
        {
            var action = await Context.TalkChannel.Reader.ReadAsync();
            if (action.mode <= 0)
            {
                Context.Finished = true;
                return -1;
            }
            return action.selection;
        }

        public async Task<string?> WaitingForInputText()
        {
            var action = await Context.TalkChannel.Reader.ReadAsync();
            if (action.mode <= 0)
            {
                Context.Finished = true;
                return null;
            }
            return action.inputText;
        }
    }
}
