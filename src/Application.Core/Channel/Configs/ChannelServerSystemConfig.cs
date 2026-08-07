namespace Application.Core.Channel.Configs
{
    public class ChannelServerSystemConfig
    {
        public int ClientWidth { get; set; } = 1280;
        public int ClientHeight { get; set; } = 720;
        public double RangeOfConversation { get; set; } = 1000000;

        double _rangeOfVisibility = 0;
        public double GetRangedDistance()
        {
            if (_rangeOfVisibility <= 0)
            {
                if (ClientWidth <= 0 && ClientHeight <= 0)
                {
                    _rangeOfVisibility = double.PositiveInfinity;
                }
                else
                {
                    var radius = Math.Max(ClientHeight, ClientWidth) * 1.1;
                    _rangeOfVisibility = radius * radius;
                }
            }
            return _rangeOfVisibility;
        }

    }
}
