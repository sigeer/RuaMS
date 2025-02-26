using scripting.npc;

namespace Application.Core.scripting.npc
{
    public enum TempConversationType
    {
        Select,
        InputNumber,
        YesNo
    }
    public class TempConversation : NPCConversationManager
    {
        Action<int, TempConversation>? _onSelect;
        Action<long, TempConversation>? _onInputNumber;

        Action<TempConversation>? _onYes;
        Action<TempConversation>? _onNo;
        TempConversationType _type;

        public TempConversation(IClient c, int npc) : base(c, npc, null)
        {

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
                    dispose();

                _onSelect?.Invoke(selection, this);
            }
            else if (_type == TempConversationType.InputNumber)
            {
                if (mode <= 0)
                    dispose();

                _onInputNumber?.Invoke(selection, this);
            }
            else if (_type == TempConversationType.YesNo)
            {
                if (mode == 1)
                    _onYes?.Invoke(this);
                else if (mode == 0)
                    _onNo?.Invoke(this);
                else
                    dispose();
            }
            else
            {
                dispose();
            }
        }
    }
}
