namespace Application.Shared.Constants
{
    public class KeyBinding
    {
        private int type;
        private int action;

        public KeyBinding(int type, int action)
        {
            this.type = type;
            this.action = action;
        }

        public int getType()
        {
            return type;
        }

        public int getAction()
        {
            return action;
        }
    }
}
