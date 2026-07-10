using Application.Templates.Exceptions;
using Application.Templates.Reader;
using System.Globalization;

namespace Application.Templates.Reader.Xml
{
    public class ServerXmlResolver: IWzPathResolver
    {
        public string BaseDir { get; }

        public ServerXmlResolver(string baseDir)
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
                    return [Path.Combine(imgDir, "MCSkill.img.xml")];
                case ProviderType.CarnivalGuardian:
                    return [Path.Combine(imgDir, "MCGuardian.img.xml")];
                case ProviderType.OxQuiz:
                    return [Path.Combine(imgDir, "OXQuiz.img.xml")];
                case ProviderType.MapObstacle:
                    return [Path.Combine(imgDir, "Obj", "trap.img.xml")];
                case ProviderType.EtcCashCommodity:
                    return [Path.Combine(imgDir, "Commodity.img.xml")];
                case ProviderType.EtcCashPackage:
                    return [Path.Combine(imgDir, "CashPackage.img.xml")];
                case ProviderType.EtcNpcLocation:
                    return [Path.Combine(imgDir, "NpcLocation.img.xml")];
                case ProviderType.EtcMakeCharInfo:
                    return [Path.Combine(imgDir, "MakeCharInfo.img.xml")];
                case ProviderType.EtcScriptInfo:
                    return [Path.Combine(imgDir, "ScriptInfo.img.xml")];
                case ProviderType.UIMobWithBossHpBar:
                    return [Path.Combine(imgDir, "UIWindow.img.xml")];
                case ProviderType.Map:
                    {
                        var dir = Path.Combine(BaseDir, imgDir, "Map");
                        return Directory.Exists(dir)
                            ? Directory.EnumerateFiles(dir, "*.img.xml", SearchOption.AllDirectories)
                            .Select(x => Path.GetRelativePath(BaseDir, x))
                            .ToArray()
                            : [];
                    }
                case ProviderType.MobSkill:
                    return [Path.Combine(imgDir, "MobSkill.img.xml")];
                case ProviderType.Quest:
                    string[] names = ["QuestInfo.img.xml", "Act.img.xml", "Check.img.xml"];
                    return names.Select(f => Path.Combine(imgDir, f)).ToArray();
                case ProviderType.Skill:
                    {
                        var dir = Path.Combine(BaseDir, imgDir);
                        return Directory.Exists(dir)
                            ? Directory.EnumerateFiles(dir, "*.img.xml", SearchOption.AllDirectories)
                            .Where(x => int.TryParse(Path.GetFileNameWithoutExtension(x).Split(".")[0], out _))
                            .Select(x => Path.GetRelativePath(BaseDir, x))
                            .ToArray()
                            : [];
                    }
                case ProviderType.Equip:
                case ProviderType.Item:
                    {
                        var dir = Path.Combine(BaseDir, imgDir);
                        return Directory.Exists(dir)
                            ? Directory.EnumerateFiles(dir, "*.img.xml", SearchOption.AllDirectories)
                            .Select(x => Path.GetRelativePath(BaseDir, x))
                            .ToArray()
                            : [];
                    }
                case ProviderType.StringItem:
                    return new string[] { "Eqp.img.xml", "Consume.img.xml", "Cash.img.xml", "Ins.img.xml", "Etc.img.xml", "Pet.img.xml" }.Select(f => Path.Combine(imgDir, f)).ToArray();
                case ProviderType.StringMap:
                    return [Path.Combine(imgDir, "Map.img.xml")];
                case ProviderType.StringMob:
                    return [Path.Combine(imgDir, "Mob.img.xml")];
                case ProviderType.StringNpc:
                    return [Path.Combine(imgDir, "Npc.img.xml")];
                case ProviderType.StringQuest:
                    return [Path.Combine(imgDir, "QuestInfo.img.xml")];
                case ProviderType.StringSkill:
                    return [Path.Combine(imgDir, "Skill.img.xml")];
                default:
                    {
                        var dir = Path.Combine(BaseDir, imgDir);
                        return Directory.Exists(dir)
                            ? Directory.EnumerateFiles(dir, "*.img.xml")
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
                ProviderType.Equip => FindFileInDir(imgDir, $"{templateId:D8}.img.xml") ?? Path.Combine("Character", $"{templateId:D8}.img.xml"),
                ProviderType.Item => Path.Combine(imgDir, GetItemSubDir(templateId), GetItemGroupFile(templateId)),
                ProviderType.Map => Path.Combine(imgDir, "Map", $"Map{templateId / 100000000}", $"{templateId:D9}.img.xml"),
                ProviderType.Mob => Path.Combine(imgDir, $"{templateId:D7}.img.xml"),
                ProviderType.Npc => Path.Combine(imgDir, $"{templateId:D7}.img.xml"),
                ProviderType.Reactor => Path.Combine(imgDir, $"{templateId:D7}.img.xml"),
                ProviderType.Skill => ResolveSkillPath(templateId),
                _ => ResolveGroup(type)[0]
            };
        }

        static string GetImgDir(ProviderType type) => type switch
        {
            ProviderType.Equip => "Character.wz",
            ProviderType.Item => "Item.wz",
            ProviderType.Map => "Map.wz",
            ProviderType.Mob => "Mob.wz",
            ProviderType.MobSkill => "Skill.wz",
            ProviderType.Npc => "Npc.wz",
            ProviderType.Quest or ProviderType.StringQuest => "Quest.wz",
            ProviderType.Reactor => "Reactor.wz",
            ProviderType.Skill => "Skill.wz",

            ProviderType.StringMob or
            ProviderType.StringMap or
            ProviderType.StringItem or
            ProviderType.StringNpc or
            ProviderType.StringSkill => "String.wz",

            ProviderType.UIMobWithBossHpBar => "UI.wz",

            ProviderType.OxQuiz => "Etc.wz",

            ProviderType.MapObstacle => "Map.wz",

            ProviderType.CarnivalSkill or
            ProviderType.CarnivalGuardian => "Skill.wz",

            ProviderType.EtcCashCommodity or
            ProviderType.EtcCashPackage or
            ProviderType.EtcScriptInfo or
            ProviderType.EtcNpcLocation or
            ProviderType.EtcMakeCharInfo => "Etc.wz",
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
                return $"{itemId}.img.xml";
            return $"{prefix:D4}.img.xml";
        }

        /// <summary>
        /// Skill 文件按 job code 分组
        /// </summary>
        static string ResolveSkillPath(int skillId)
        {
            if (skillId == 0) return Path.Combine("Skill", "000.img");
            var jobCode = skillId / 10000;
            return Path.Combine("Skill.wz", $"{jobCode}.img.xml");
        }
    }
}
