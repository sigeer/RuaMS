using Application.Templates.Etc;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
{
    public class OxQuizProvider : AbstractAllProvider<OxQuizTemplate>
    {
        public override ProviderType Type => ProviderType.OxQuiz;

        public OxQuizProvider(IWzPathResolver resolver, bool useCache = true) : base(resolver, useCache)
        {
        }

        protected override IEnumerable<OxQuizTemplate> GetDataFromImg()
        {
            try
            {
                List<OxQuizTemplate> list = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var root = new WZImage(fullPath);

                    
                    foreach (var round in root.Children)
                    {
                        if (!int.TryParse(round.Name, out var roundId))
                            continue;

                        var roundData = new OxQuizTemplate(roundId);
                        List<OxQuizQuestionEntry> questions = [];
                        foreach (var item in round.Children)
                        {
                            if (!int.TryParse(item.Name, out var questionId))
                                continue;

                            questions.Add(new OxQuizQuestionEntry()
                            {
                                Answer = item.GetIntValue("a"),
                                QuestionId = questionId
                            });
                        }
                        roundData.Questions = questions.ToArray();
                        InsertItem(roundData);
                        list.Add(roundData);
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }
    }
}
