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
        Action<int, TempConversation>? _onSelect;
        Action<long, TempConversation>? _onInputNumber;
        Action<string, TempConversation>? _onInputText;

        Action<TempConversation>? _onYes;
        Action<TempConversation>? _onNo;
        TempConversationType _type;

        private TempConversation(IChannelClient c, int npc = NpcId.MAPLE_ADMINISTRATOR) : base(c, npc, null)
        {
        }

        /// <summary>
        /// 创建临时对话
        /// </summary>
        /// <param name="c"></param>
        /// <param name="npc"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public static TempConversation? Create(IChannelClient c, int npc = NpcId.MAPLE_ADMINISTRATOR, bool force = true)
        {
            if (force)
                c.NPCConversationManager?.dispose();
            else if (c.NPCConversationManager != null)
            {
                c.sendPacket(PacketCreator.sendYellowTip("有正在进行的对话，使用!dispose解卡"));
                return null;
            }

            var value = new TempConversation(c, npc);
            c.NPCConversationManager = value;
            return value;
        }

        public static bool TryCreate(IChannelClient c, out TempConversation? data, int npc = NpcId.MAPLE_ADMINISTRATOR, bool force = true)
        {
            data = null;
            if (force)
                c.NPCConversationManager?.dispose();
            else if (c.NPCConversationManager != null)
            {
                c.sendPacket(PacketCreator.sendYellowTip("有正在进行的对话，使用!dispose解卡"));
                return false;
            }

            data = new TempConversation(c, npc);
            c.NPCConversationManager = data;
            return true;
        }

        public void RegisterTalk(string text)
        {
            _type = TempConversationType.Default;
            sendOk(text);
        }

        public void RegisterSelect(string text, Action<int, TempConversation> onSelect)
        {
            _onSelect = onSelect;
            _type = TempConversationType.Select;
            sendSimple(text);
        }

        public void RegisterInputNumber(string text, Action<long, TempConversation> onInputNumber)
        {
            _onInputNumber = onInputNumber;
            _type = TempConversationType.InputNumber;
            sendGetNumber(text, 0, int.MinValue, int.MaxValue);
        }

        public void RegisterInput(string text, Action<string, TempConversation> onInputText)
        {
            _onInputText = onInputText;
            _type = TempConversationType.InputText;
            sendGetText(text);
        }

        public void RegisterYesOrNo(string text, Action<TempConversation> yesCallback, Action<TempConversation>? noCallback = null)
        {
            _type = TempConversationType.YesNo;
            sendYesNo(text);
            _onYes = yesCallback;
            _onNo = noCallback;
        }

        public void Handle(sbyte mode, sbyte type, int selection)
        {
            if (_type == TempConversationType.Select)
            {
                if (mode <= 0)
                {
                    dispose();
                    return;
                }

                _onSelect?.Invoke(selection, this);
            }
            else if (_type == TempConversationType.InputNumber)
            {
                if (mode <= 0)
                {
                    dispose();
                    return;
                }

                _onInputNumber?.Invoke(selection, this);
            }
            else if (_type == TempConversationType.InputText)
            {
                if (mode <= 0)
                {
                    dispose();
                    return;
                }

                _onInputText?.Invoke(getText() ?? "", this);
            }
            else if (_type == TempConversationType.YesNo)
            {
                if (mode == 1)
                    _onYes?.Invoke(this);
                else if (mode == 0)
                    _onNo?.Invoke(this);
                else
                {
                    dispose();
                    return;
                }
            }
            else
            {
                dispose();
                return;
            }
        }
    }
}
