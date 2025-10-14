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
    }
}
