using Application.Core.Channel;
using Application.Core.Channel.Services;
using client.creator;

namespace Application.Core.client.creator.veteran
{
    public abstract class VeteranCreator : CharacterFactory
    {
        protected VeteranCreator(DataService channelService) : base(channelService)
        {
        }

        public abstract int createCharacter(int accountId, string name, int face, int hair, int skin, int gender, int improveSp);
    }
}
