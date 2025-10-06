using Application.Templates.Exceptions;
using Application.Templates.Providers;

namespace Application.Templates
{
    public class TemplateOptions
    {
        /// <summary>
        /// 是否启动内部缓存，默认true。（有些数据已经有了上层缓存，比如RactorFactory.reactorStats, QuestFactory.quests）
        /// </summary>
        public bool UseCache { get; set; } = true;
        /// <summary>
        /// wz所在dir
        /// </summary>
        public string? DataDir { get; set; }


        static Dictionary<ProviderType, string> _defaultPath = new Dictionary<ProviderType, string>()
        {
            { ProviderType.CashCommodity, "Etc.wz" }  ,
            { ProviderType.CashPackage, "Etc.wz" }  ,
            { ProviderType.Equip, "Character.wz" }  ,
            { ProviderType.Item, "Item.wz" }  ,
            { ProviderType.Map, "Map.wz" }  ,
            { ProviderType.Mob, "Mob.wz" }  ,
            { ProviderType.Npc, "Npc.wz" }  ,
            { ProviderType.Quest, "Quest.wz" }  ,
            { ProviderType.Reactor, "Reactor.wz" }  ,
            { ProviderType.Skill, "Skill.wz" }  ,
            { ProviderType.MobSkill, "MobSkill.wz" }  ,
            { ProviderType.String, "String.wz" }
        };
        public static string GetDefaultPath(ProviderType type) => _defaultPath.GetValueOrDefault(type) ?? throw new ProviderNotFoundException(type.ToString());
    }
}
