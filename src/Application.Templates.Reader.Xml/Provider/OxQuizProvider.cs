using Application.Templates.Etc;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public sealed class OxQuizProvider : AbstractAllProvider<OxQuizTemplate>
    {
        public override ProviderType Type => ProviderType.OxQuiz;

        public OxQuizProvider(IWzPathResolver resolver) : base(resolver)
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
                    var xDoc = XDocument.Load(fullPath);
                    var root = xDoc.Root!;
                    foreach (var imgdir in root.Elements())
                    {
                        if (!int.TryParse(imgdir.GetName(), out var imgdirId))
                            continue;
                        var questions = new List<OxQuizQuestionEntry>();
                        foreach (var question in imgdir.Elements())
                        {
                            if (!int.TryParse(question.GetName(), out var questionId))
                                continue;
                            var answerEl = question.Elements().FirstOrDefault(e => e.GetName() == "a");
                            questions.Add(new OxQuizQuestionEntry
                            {
                                QuestionId = questionId,
                                Answer = answerEl?.GetIntValue() ?? 0,
                            });
                        }
                        var template = new OxQuizTemplate(imgdirId)
                        {
                            Questions = questions.ToArray(),
                        };
                        InsertItem(template);
                        list.Add(template);
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
