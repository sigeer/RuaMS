using Application.Core.Channel.DataProviders;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using server.life;
using server.quest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.server.quest
{
    public class QuestFactory : DataBootstrap, IStaticService
    {
        private static QuestFactory? _instance;

        public static QuestFactory Instance => _instance ?? throw new BusinessFatalException("QuestFactory 未注册");
        public QuestFactory(ILogger<DataBootstrap> logger) : base(logger)
        {
        }

        public void Register(IServiceProvider sp)
        {
            _instance = sp.GetService<QuestFactory>() ?? throw new BusinessFatalException("QuestFactory 未注册");
        }

        protected override void LoadDataInternal()
        {
            var provider = ProviderFactory.GetProvider<QuestProvider>();
            foreach (var item in provider.Values)
            {
                quests[item.TemplateId] = new Quest(item);
            }
        }

        private Dictionary<short, int> medals = new();
        public int GetMedalRequirement(short questId)
        {
            return medals.GetValueOrDefault(questId, -1);
        }

        public void AddMedal(short questId, int medalId)
        {
            medals[questId] = medalId;
        }

        private Dictionary<int, Quest> quests = new();
        public Quest? GetInstance(int id)
        {
            return quests.GetValueOrDefault(id);
        }

        public void clearCache(int quest)
        {
            quests.Remove(quest);
        }

        public void clearCache()
        {
            quests.Clear();
        }

    }
}
