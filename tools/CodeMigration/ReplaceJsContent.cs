using System.Text;
using System.Text.RegularExpressions;
using Application.Utility.Configs;

namespace JSMigration
{
    /// <summary>
    /// 替换js脚本中，与java交互的部分，并输出未处理的内容
    /// </summary>
    public class ReplaceJsContent
    {
        readonly string jsDir;

        /// <summary>
        /// 要处理的js脚本目录
        /// </summary>
        /// <param name="jsDir"></param>
        public ReplaceJsContent(string jsDir)
        {
            this.jsDir = jsDir;
        }


        HashSet<string> unsupport = new HashSet<string>();
        Dictionary<string, List<string>> javaTypes = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> javaTos = new Dictionary<string, List<string>>();

        //[\\r\\s]*((const)|(var))\\s*.*?\\s*=\\s*Java\\.type\\((.*?)\\);?
        Regex javaType = new Regex("([\\r\\s]*)((const)|(var))?\\s*(.*?)\\s*=\\s*Java\\.type\\((.*?)\\)(.*)");
        Regex javaTo = new Regex("Java\\.to\\((.*?),.*\\);?");

        string RemoveJavaType(string file, string jsContent)
        {
            Dictionary<string, string> typePair = new Dictionary<string, string>();
            var m = javaType.Replace(jsContent, e =>
            {
                var typeName = e.Groups[6].Value;

                if (!javaTypes.ContainsKey(typeName))
                    javaTypes[typeName] = new List<string>();
                javaTypes[typeName].Add(file);

                var realTypeName = typeName.Split('.').LastOrDefault()!;
                realTypeName = realTypeName.Substring(0, realTypeName.Length - 1);
                if (jsContent[e.Groups[6].Index + e.Groups[6].Value.Length + 1] == '[')
                {
                    return $"{e.Groups[1].Value}{e.Groups[4].Value} {e.Groups[5].Value} = {realTypeName}{e.Groups[7].Value}";
                }
                else
                {
                    typePair[e.Groups[5].Value] = realTypeName;
                    return "";
                }
            });

            foreach (var k in typePair)
            {
                m = Regex.Replace(m, $"((var)|(let))\\s*{k.Key};?$", "");
                m = m.Replace($"{k.Key}.", $"{k.Value}.");
            }
            return m;
        }

        string RemoveJavaTo(string file, string jsContent)
        {
            return javaTo.Replace(jsContent, e =>
            {
                if (!javaTos.ContainsKey(e.Value))
                    javaTos[e.Value] = new List<string>();
                javaTos[e.Value].Add(file);
                return e.Groups[1].Value;
            });
        }

        Dictionary<string, List<string>> gameConfigList = new Dictionary<string, List<string>>();
        Regex gameConfigRegex = new Regex("GameConfig\\.\\S*?\\(\"(.*?)\"\\)");

        List<string>? configProps;
        List<string> GetConfigProps()
        {
            return configProps ??= typeof(ServerConfig).GetFields().Select(x => x.Name).ToList();
        }
        string RemoveGameConfig(string file, string jsContent)
        {
            var allProps = GetConfigProps();
            return gameConfigRegex.Replace(jsContent, e =>
            {
                var prop = e.Groups[1].Value;

                var mapValue = prop.Replace("_", "").ToLower();
                var findResult = allProps.FirstOrDefault(x => x.Replace("_", "").ToLower() == mapValue);
                if (findResult != null)
                {
                    return "YamlConfig.config.server." + findResult;
                }
                else
                {
                    if (!gameConfigList.ContainsKey(prop))
                        gameConfigList[prop] = new List<string>();
                    gameConfigList[prop].Add(file);
                    return prop;
                }
            });
        }

        string ReplaceSpecial(string file, string jsContent)
        {
            jsContent = jsContent.Replace("java.awt.Point", "Point");
            jsContent = jsContent.Replace("Server.getInstance().getWorld(em.getChannelServer().getWorld());", "em.getWorldServer();");
            return jsContent;
        }

        HashSet<string> FindIteratorResult = new HashSet<string>();
        void FindIterator(string file, string jsContent)
        {
            if (jsContent.Contains("iterator"))
            {
                FindIteratorResult.Add(file);
            }
        }

        /// <summary>
        /// 目前提供了兼容方法，暂时不考虑替换
        /// </summary>
        /// <param name="file"></param>
        /// <param name="jsContent"></param>
        /// <returns></returns>
        string Replace_toArray(string file, string jsContent)
        {
            jsContent = jsContent.Replace(".toArray()", ".ToList()");
            jsContent = jsContent.Replace(".size()", ".Count");
            return jsContent;
        }
        /// <summary>
        /// 目前提供了兼容方法，暂时不考虑替换
        /// </summary>
        /// <param name="file"></param>
        void FindJava(string file)
        {
            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(".size()")
                    || lines[i].Contains(".toArray"))
                    unsupport.Add($"File: {file}, Line {i + 1}");
            }
        }

        public void Run()
        {
            if (Directory.Exists("ns"))
                Directory.Delete("ns", true);

            var files = Directory.GetFiles(jsDir, "*.js", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var jsContent = File.ReadAllText(file);

                FindIterator(file, jsContent);
                // 移除Java.type()
                jsContent = RemoveJavaType(file, jsContent);
                // 移除Java.to()
                jsContent = RemoveJavaTo(file, jsContent);
                jsContent = RemoveGameConfig(file, jsContent);
                jsContent = ReplaceSpecial(file, jsContent);
                var newFilePath = Path.Combine("ns", Path.GetRelativePath(jsDir, file));
                var dir = Path.GetDirectoryName(newFilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(newFilePath, jsContent, Encoding.UTF8);
                // FindJava(newFilePath);
            }
            WriteFunctionCallerListToFile();
        }

        public void WriteFunctionCallerListToFile()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;

            var packageListPath = Path.Combine(dir, "Unsupported.txt");
            File.WriteAllLines(packageListPath, unsupport);

            var usedTypeFilePath = Path.Combine(dir, "JavaType.txt");
            File.WriteAllLines(usedTypeFilePath, javaTypes.SelectMany(x => x.Value.Select(y => $"Type: {x.Key} -- File: {y}")));

            var javaToPath = Path.Combine(dir, "JavaTo.txt");
            File.WriteAllLines(javaToPath, javaTos.SelectMany(x => x.Value.Select(y => $"Type: {x.Key} -- File: {y}")));

            var gameConfigListPath = Path.Combine(dir, "JavaGameConfig.txt");
            File.WriteAllLines(gameConfigListPath, gameConfigList.SelectMany(x => x.Value.Select(y => $"Type: {x.Key} -- File: {y}")));

            var findIteratorResultPath = Path.Combine(dir, "Iterator.txt");
            File.WriteAllLines(findIteratorResultPath, FindIteratorResult);
        }
    }
}
