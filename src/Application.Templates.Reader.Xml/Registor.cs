using Application.Templates.Reader.Xml.Provider;
using Application.Templates.Reader;
using System.Globalization;

namespace Application.Templates.Reader.Xml
{
    public static class Registor
    {
        public static ProviderSource Register(this ProviderSource ps)
        {
            ps.RegisterProvider(() => new MapProvider(ps.Resolver))
                .RegisterProvider(() => new ReactorProvider(ps.Resolver, false))
                .RegisterProvider(() => new QuestProvider(ps.Resolver))
                .RegisterProvider(() => new EquipProvider(ps.Resolver))
                .RegisterProvider(() => new ItemProvider(ps.Resolver))
                .RegisterProvider(() => new MobSkillProvider(ps.Resolver, false))
                .RegisterProvider(() => new EtcNpcLocationProvider(ps.Resolver))
                .RegisterProvider(() => new EtcScriptInfoProvider(ps.Resolver))
                .RegisterProvider(() => new NpcProvider(ps.Resolver))
                .RegisterProvider(() => new MobProvider(ps.Resolver))
                .RegisterProvider(() => new SkillProvider(ps.Resolver, false))
                .RegisterProvider(() => new MobWithBossHpBarProvider(ps.Resolver, false))
                .RegisterProvider(() => new CashCommodityProvider(ps.Resolver))
                .RegisterProvider(() => new CashPackageProvider(ps.Resolver))
                .RegisterProvider(() => new CarnivalSkillProvider(ps.Resolver))
                .RegisterProvider(() => new CarnivalGuardianProvider(ps.Resolver))
                .RegisterProvider(() => new MapObstacleProvider(ps.Resolver))
                .RegisterProvider(() => new OxQuizProvider(ps.Resolver))

                .RegisterKeydProvider("zh-CN", () => new StringProvider(CultureInfo.GetCultureInfo("zh-CN"), ps.Resolver))
                .RegisterKeydProvider("en-US", () => new StringProvider(CultureInfo.GetCultureInfo("en-US"), ps.Resolver));
            return ps;
        }
    }
}
