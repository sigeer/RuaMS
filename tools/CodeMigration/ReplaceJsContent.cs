using System.Text;
using System.Text.RegularExpressions;

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

        Regex javaType = new Regex("[\\r\\s]*((const)|(var))\\s*.*?\\s*=\\s*Java\\.type\\((.*?)\\);?");
        Regex javaTo = new Regex("Java\\.to\\((.*?),.*\\);?");

        string RemoveJavaType(string file, string jsContent)
        {
            return javaType.Replace(jsContent, e =>
            {
                // AbstractScriptManager engine.AddHostType
                if (!javaTypes.ContainsKey(e.Groups[4].Value))
                    javaTypes[e.Groups[4].Value] = new List<string>();
                javaTypes[e.Groups[4].Value].Add(file);
                return "";
            });
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

        string ReplaceSpecial(string file, string jsContent)
        {
            jsContent = jsContent.Replace("java.awt.Point", "Point");

            return jsContent;
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
            var files = Directory.GetFiles(jsDir, "*.js", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var jsContent = File.ReadAllText(file);

                // 移除Java.type()
                jsContent = RemoveJavaType(file, jsContent);
                // 移除Java.to()
                jsContent = RemoveJavaTo(file, jsContent);
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
        }
    }
}
