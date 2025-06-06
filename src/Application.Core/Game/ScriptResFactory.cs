namespace Application.Core.Game
{
    public class ScriptDir
    {
        public static string Event = "event-lua";
    }
    public class ScriptResFactory
    {
        public readonly static string ScriptDirName = GetScriptDir();
        public static string GetScriptFullPath(string relativePath)
        {
            if (!Path.IsPathRooted(relativePath))
                return Path.Combine(ScriptDirName, relativePath);

            return Path.Combine(ScriptDirName, relativePath);
        }

        private static string GetScriptDir()
        {
            var dir = Environment.GetEnvironmentVariable("ms-script");
            if (dir == null || !Directory.Exists(dir))
                dir = "scripts";
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir);
        }

        public static List<FileInfo> LoadAllScript()
        {
            return new DirectoryInfo(ScriptDirName).GetFiles("*", SearchOption.AllDirectories).ToList();
        }

        public static string[] GetEvents()
        {
            return Directory.GetFiles(GetScriptFullPath(ScriptDir.Event)).Select(x => Path.GetFileNameWithoutExtension(x)).Where(x => !x.StartsWith("__")).ToArray();
        }
    }
}
