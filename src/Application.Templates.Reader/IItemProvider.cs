using Application.Templates.Item.Consume;

namespace Application.Templates.Reader
{
    public interface IItemProvider : IProvider<AbstractItemTemplate>
    {
        List<MonsterCardItemTemplate> GetAllMonsterCard();
        List<MasteryItemTemplate> GetAllSkillBook();
        List<ConsumeItemTemplate> GetAllConsume();
    }
}
