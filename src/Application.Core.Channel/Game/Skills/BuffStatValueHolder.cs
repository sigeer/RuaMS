using server;

namespace Application.Core.Game.Skills
{
    public class BuffStatValueHolder
    {

        public StatEffect effect;
        public long startTime;
        public int value;
        public bool bestApplied;

        public BuffStatValueHolder(StatEffect effect, long startTime, int value)
        {
            this.effect = effect;
            this.startTime = startTime;
            this.value = value;
            this.bestApplied = false;
        }
    }
}
