using Application.Core.Game.Skills;
using Application.Shared.GameProps;
using Application.Templates.Providers;
using Application.Templates.Skill;
using Application.Templates.XmlWzReader.Provider;
using Newtonsoft.Json;
using System.Globalization;

namespace ServiceTest.Infrastructure.WZ
{
    internal class SkillTests : WzTestBase
    {
        JsonSerializerSettings options;

        public SkillTests()
        {
            options = new JsonSerializerSettings
            {
                ContractResolver = new PrivateContractResolver(),
                Formatting = Formatting.Indented
            };
        }
        protected override void OnProviderRegistering()
        {
            _providerSource.TryRegisterProvider<SkillProvider>(o => new SkillProvider(o));
            _providerSource.TryRegisterProvider<StringProvider>(o => new StringProvider(o, CultureInfo.GetCultureInfo("en-US")));
            ProviderSource.Instance = _providerSource;
        }
        string ToJson(object? obj)
        {
            return JsonConvert.SerializeObject(obj, options);
        }

        [Test]
        public void SkillStatEffectTest()
        {
            var allSkills = _providerSource.GetProvider<SkillProvider>().LoadAll().OfType<SkillTemplate>().ToArray();
            OldSkillFactory.LoadAllSkills();
            SkillFactory.LoadAllSkills();
            foreach (var item in allSkills)
            {
                var oldData = OldSkillFactory.getSkill(item.TemplateId);
                var newData = SkillFactory.getSkill(item.TemplateId);

                var oldDataStr = ToJson(oldData);
                var newDataStr = ToJson(newData);
                Assert.That(newDataStr, Is.EqualTo(oldDataStr), $"SkillId = {item.TemplateId}");
            }

        }
    }
}
