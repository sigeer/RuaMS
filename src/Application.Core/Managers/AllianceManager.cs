namespace Application.Core.Managers
{
    public class AllianceManager
    {
        static readonly ILogger log = LogFactory.GetLogger(LogType.Alliance);
        public static bool canBeUsedAllianceName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Contains(" ") || name.Length > 12)
            {
                return false;
            }

            using var dbContext = new DBContext();
            return dbContext.Alliances.Any(x => x.Name == name);
        }
    }
}
