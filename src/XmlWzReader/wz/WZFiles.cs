namespace XmlWzReader.wz;

public class WZFiles
{
    public static readonly WZFiles QUEST = new("Quest");
    public static readonly WZFiles ETC = new("Etc");
    public static readonly WZFiles ITEM = new("Item");
    public static readonly WZFiles CHARACTER = new("Character");
    public static readonly WZFiles STRING = new("String");
    public static readonly WZFiles LIST = new("List");
    public static readonly WZFiles MOB = new("Mob");
    /// <summary>
    /// 地图，UseCache：true（不同频道/不同事件加载同一地图）。由于MapFactory中没有使用缓存，在这一级缓存。其他类型有上级类型缓存，无需在这一级缓存
    /// </summary>
    public static readonly WZFiles MAP = new("Map", true);
    public static readonly WZFiles NPC = new("Npc");
    public static readonly WZFiles REACTOR = new("Reactor");
    public static readonly WZFiles SKILL = new("Skill");
    public static readonly WZFiles SOUND = new("Sound");
    public static readonly WZFiles UI = new("UI");

    public static string DIRECTORY = getWzDirectory();

    private string fileName;
    public bool UseCache { get; init; }

    WZFiles(string name, bool useCache = false)
    {
        this.fileName = name + ".wz";
        UseCache = useCache;
    }

    public string getFile()
    {
        return Path.Combine(DIRECTORY, fileName);
    }

    public static string getWzDirectory()
    {
        var propertyPath = Environment.GetEnvironmentVariable("ms-wz") ?? Environment.GetEnvironmentVariable("RUA_MS_ms-wz");
        if (propertyPath != null && Directory.Exists(propertyPath))
        {
            return propertyPath;
        }

        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wz");
    }
}
