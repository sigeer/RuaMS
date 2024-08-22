using System.Text.RegularExpressions;

namespace JSMigration
{
    /// <summary>
    /// 查找Dictionary中直接通过索引器[]取值的代码
    /// </summary>
    public class FindDictionaryReader
    {
        Regex nameReg = new Regex("Dictionary<.*?>\\s*(\\w*)\\s*=\\s*new\\s*.*?;");
        HashSet<string> output = new HashSet<string>();

        string codeDir;

        public FindDictionaryReader(string codeDir)
        {
            this.codeDir = codeDir;
        }

        public void Run()
        {
            var files = Directory.GetFiles(codeDir, "*.cs", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                ReadFile(file);
            }
        }

        public void ReadFile(string filePath)
        {
            Console.WriteLine("Searching: " + filePath);
            var content = File.ReadAllText(filePath);

            var variables = nameReg.Matches(content).Select(x => x.Groups[1].Value + "[");
            if (variables.Count() > 0)
            {
                Console.WriteLine("Searching: " + filePath + "matched " + variables.Count());

                var lines = File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (variables.Any(x => lines[i].Contains(x)))
                        output.Add($"File: {filePath}, Line {i + 1}");
                }
            }
        }

        public void WriteOutput()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;

            var guid = Guid.NewGuid().ToString();
            var tempFile = guid + "_dictionary.txt";
            var tempFilePath = Path.Combine(dir, tempFile);
            File.WriteAllLines(tempFilePath, output);
        }
    }
}
