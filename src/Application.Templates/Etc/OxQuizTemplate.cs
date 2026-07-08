namespace Application.Templates.Etc
{
    public class OxQuizQuestionEntry
    {
        public int QuestionId { get; set; }
        public int Answer { get; set; }
    }

    public class OxQuizTemplate : AbstractTemplate
    {
        public OxQuizTemplate(int templateId) : base(templateId)
        {
            Questions = [];
        }
        public OxQuizQuestionEntry[] Questions { get; set; } = [];

    }
}
