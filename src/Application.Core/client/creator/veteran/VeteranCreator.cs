using Application.Core.Channel;
using client.creator;

namespace Application.Core.client.creator.veteran
{
    public abstract class VeteranCreator : CharacterFactory
    {
        protected VeteranCreator(ChannelService channelService) : base(channelService)
        {
        }

        public abstract int createCharacter(int accountId, string name, int face, int hair, int skin, int gender, int improveSp);
    }
}
