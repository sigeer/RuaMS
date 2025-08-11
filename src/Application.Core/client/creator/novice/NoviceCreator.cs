using Application.Core.Channel.Services;
using client.creator;

namespace Application.Core.client.creator.novice
{
    public abstract class NoviceCreator : CharacterFactory
    {
        protected NoviceCreator(DataService channelService) : base(channelService)
        {
        }

        public abstract int createCharacter(int accountId, string name, int face, int hair, int skin, int top, int bottom, int shoes, int weapon, int gender);
    }
}
