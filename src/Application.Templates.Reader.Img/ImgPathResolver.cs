using Application.Templates.Reader;

namespace Application.Templates.Reader.Img;

/// <summary>
/// IMG 形态路径解析器
/// </summary>
public class ImgPathResolver : IWzPathResolver
{
    public string BaseDir { get; }

    public ImgPathResolver(string baseDir)
    {
        BaseDir = baseDir;
    }

    /// <summary>
    /// 找到 <paramref name="type"/> 的所有文件
    /// </summary>
    public string[] ResolveGroup(ProviderType type)
    {
        var imgDir = GetImgDir(type);
        switch (type)
        {
            case ProviderType.CarnivalSkill:
                return [Path.Combine(imgDir, "MCSkill.img")];
            case ProviderType.CarnivalGuardian:
                return [Path.Combine(imgDir, "MCGuardian.img")];
            case ProviderType.OxQuiz:
                return [Path.Combine(imgDir, "OXQuiz.img")];
            case ProviderType.MapObstacle:
                return [Path.Combine(imgDir, "Obj", "trap.img")];
            case ProviderType.EtcCashCommodity:
                return [Path.Combine(imgDir, "Commodity.img")];
            case ProviderType.EtcCashPackage:
                return [Path.Combine(imgDir, "CashPackage.img")];
            case ProviderType.EtcNpcLocation:
                return [Path.Combine(imgDir, "NpcLocation.img")];
            case ProviderType.EtcMakeCharInfo:
                return [Path.Combine(imgDir, "MakeCharInfo.img")];
            case ProviderType.EtcScriptInfo:
                return [Path.Combine(imgDir, "ScriptInfo.img")];
            case ProviderType.UIMobWithBossHpBar:
                return [Path.Combine(imgDir, "UIWindow.img")];
            case ProviderType.Map:
                {
                    var dir = Path.Combine(BaseDir, imgDir, "Map");
                    return Directory.Exists(dir)
                        ? Directory.EnumerateFiles(dir, "*.img", SearchOption.AllDirectories)
                        .Select(x => Path.GetRelativePath(BaseDir, x))
                        .ToArray()
                        : [];
                }
            case ProviderType.MobSkill:
                return [Path.Combine(imgDir, "MobSkill.img")];
            case ProviderType.Quest:
                string[] names = ["QuestInfo.img", "Act.img", "Check.img"];
                return names.Select(f => Path.Combine(imgDir, f)).ToArray();
            case ProviderType.Skill:
                {
                    var dir = Path.Combine(BaseDir, imgDir);
                    return Directory.Exists(dir)
                        ? Directory.EnumerateFiles(dir, "*.img", SearchOption.AllDirectories)
                        .Where(x => int.TryParse(Path.GetFileNameWithoutExtension(x), out _))
                        .Select(x => Path.GetRelativePath(BaseDir, x))
                        .ToArray()
                        : [];
                }
            case ProviderType.Equip:
            case ProviderType.Item:
                {
                    var dir = Path.Combine(BaseDir, imgDir);
                    return Directory.Exists(dir)
                        ? Directory.EnumerateFiles(dir, "*.img", SearchOption.AllDirectories)
                        .Select(x => Path.GetRelativePath(BaseDir, x))
                        .ToArray()
                        : [];
                }
            case ProviderType.StringItem:
                return new string[] { "Eqp.img", "Consume.img", "Cash.img", "Ins.img", "Etc.img", "Pet.img" }.Select(f => Path.Combine(imgDir, f)).ToArray();
            case ProviderType.StringMap:
                return [Path.Combine(imgDir, "Map.img")];
            case ProviderType.StringMob:
                return [Path.Combine(imgDir, "Mob.img")];
            case ProviderType.StringNpc:
                return [Path.Combine(imgDir, "Npc.img")];
            case ProviderType.StringQuest:
                return [Path.Combine(imgDir, "QuestInfo.img")];
            case ProviderType.StringSkill:
                return [Path.Combine(imgDir, "Skill.img")];
            default:
                {
                    var dir = Path.Combine(BaseDir, imgDir);
                    return Directory.Exists(dir)
                        ? Directory.EnumerateFiles(dir, "*.img")
                        .Select(x => Path.GetRelativePath(BaseDir, x))
                        .ToArray()
                        : [];
                }
        }

    }

    /// <summary>
    /// 根据 ProviderType + templateId 定位单个 .img 文件（相对路径）。
    /// </summary>
    public string ResolveItem(ProviderType type, int templateId)
    {
        var imgDir = GetImgDir(type);
        return type switch
        {
            ProviderType.Equip => FindFileInDir(imgDir, $"{templateId:D8}.img") ?? Path.Combine("Character", $"{templateId:D8}.img"),
            ProviderType.Item => Path.Combine(imgDir, GetItemSubDir(templateId), GetItemGroupFile(templateId)),
            ProviderType.Map => Path.Combine(imgDir, "Map", $"Map{templateId / 100000000}", $"{templateId:D9}.img"),
            ProviderType.Mob => Path.Combine(imgDir, $"{templateId:D7}.img"),
            ProviderType.Npc => Path.Combine(imgDir, $"{templateId:D7}.img"),
            ProviderType.Reactor => Path.Combine(imgDir, $"{templateId:D7}.img"),
            ProviderType.Skill => ResolveSkillPath(templateId),
            _ => ResolveGroup(type)[0]
        };
    }

    static string GetImgDir(ProviderType type) => type switch
    {
        ProviderType.Equip => "Character",
        ProviderType.Item => "Item",
        ProviderType.Map => "Map",
        ProviderType.Mob => "Mob",
        ProviderType.MobSkill => "Skill",
        ProviderType.Npc => "Npc",
        ProviderType.Quest or ProviderType.StringQuest => "Quest",
        ProviderType.Reactor => "Reactor",
        ProviderType.Skill => "Skill",

        ProviderType.StringMob or
        ProviderType.StringMap or 
        ProviderType.StringItem or 
        ProviderType.StringNpc or
        ProviderType.StringSkill => "String",

        ProviderType.UIMobWithBossHpBar => "UI",

        ProviderType.OxQuiz => "Etc",

        ProviderType.MapObstacle => "Map.wz",

        ProviderType.CarnivalSkill or
        ProviderType.CarnivalGuardian => "Skill",

        ProviderType.EtcCashCommodity or
        ProviderType.EtcCashPackage or
        ProviderType.EtcScriptInfo or
        ProviderType.EtcNpcLocation or
        ProviderType.EtcMakeCharInfo => "Etc",
        _ => type.ToString()
    };

    /// <summary>
    /// 在目录（含子目录）中按文件名查找，返回相对路径。找不到返回 null。
    /// </summary>
    string? FindFileInDir(string dir, string fileName)
    {
        var fullDir = Path.Combine(BaseDir, dir);
        if (!Directory.Exists(fullDir)) return null;
        var found = Directory.EnumerateFiles(fullDir, fileName, SearchOption.AllDirectories).FirstOrDefault();
        return found != null ? Path.GetRelativePath(BaseDir, found) : null;
    }

    /// <summary>
    /// Item 子目录：200→Consume, 300→Install, 400→Etc, 500+→Cash
    /// </summary>
    static string GetItemSubDir(int itemId)
    {
        var prefix = itemId / 10000;
        if (prefix > 500) return "Cash";
        else if (prefix == 500) return "Pet";
        else if (prefix >= 400) return "Etc";
        else if (prefix >= 300) return "Install";
        else if (prefix >= 200) return "Consume";
        return "Etc";
    }

    /// <summary>
    /// Item 文件按前缀分组命名（如 itemId=2000001 → 0200.img）。
    /// </summary>
    static string GetItemGroupFile(int itemId)
    {
        var prefix = itemId / 10000;
        if (prefix == 500)
            return $"{itemId}.img";
        return $"{prefix:D4}.img";
    }

    /// <summary>
    /// Skill 文件按 job code 分组
    /// </summary>
    static string ResolveSkillPath(int skillId)
    {
        if (skillId == 0) return Path.Combine("Skill", "000.img");
        var jobCode = skillId / 10000;
        return Path.Combine("Skill", $"{jobCode}.img");
    }
}
