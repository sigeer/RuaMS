using Application.Templates.Quest;
using Application.Templates.Reader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using server.quest;
using static client.QuestStatus;

namespace Application.Core.Channel.DataProviders
{
    public class QuestFactory : DataBootstrap, IStaticService
    {
        private static QuestFactory? _instance;

        public static QuestFactory Instance => _instance ?? throw new BusinessFatalException("QuestFactory 未注册");
        IProvider<QuestTemplate> provider = ProviderSource.Instance.GetProvider<IProvider<QuestTemplate>>(ProviderType.Quest);
        public QuestFactory(ILogger<DataBootstrap> logger) : base(logger)
        {
            Name = "任务";
            provider.AddRef();
        }

        public void Register(IServiceProvider sp)
        {
            _instance = sp.GetService<QuestFactory>() ?? throw new BusinessFatalException("QuestFactory 未注册");
        }


        private volatile Dictionary<int, int> infoNumberQuests = new();

        protected override void LoadDataInternal()
        {
            foreach (QuestTemplate item in provider.LoadAll())
            {
                var q = new Quest(item);
                quests[item.TemplateId] = q;

                int infoNumber;

                infoNumber = q.getInfoNumber(Status.STARTED);
                if (infoNumber > 0)
                {
                    infoNumberQuests[infoNumber] = q.getId();
                }

                infoNumber = q.getInfoNumber(Status.COMPLETED);
                if (infoNumber > 0)
                {
                    infoNumberQuests[infoNumber] = q.getId();
                }
            }
            provider.Release();
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
        public Quest GetInstance(int id)
        {
            if (quests.TryGetValue(id, out var q))
                return q;

            return quests[id] = new Quest(ProviderSource.Instance.GetProvider<IProvider<QuestTemplate>>(ProviderType.Quest).GetItem(id) ?? new QuestTemplate(new QuestInfoTemplate(id)));
        }

        public Quest GetInstanceFromInfoNumber(int infoNumber)
        {
            return GetInstance(infoNumberQuests.GetValueOrDefault(infoNumber, infoNumber));
        }

        public void clearCache(int quest)
        {
            quests.Remove(quest);
        }

        public void clearCache()
        {
            quests.Clear();
        }

        private HashSet<short> exploitableQuests = new() { 2338, 3637, 3714, 21752 };
        public bool isExploitableQuest(short questid)
        {
            return exploitableQuests.Contains(questid);
        }
    }
}
