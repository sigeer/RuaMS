namespace Application.Core.Game
{
    public class ScriptResFactory
    {
        public static string ScriptsParentDir = AppDomain.CurrentDomain.BaseDirectory;
        public readonly static string ScriptDirName = GetScriptDir();

        public static string GetScriptFullPath(string relativePath)
        {
            var dir = GetScriptDir();
            if (Path.IsPathRooted(ScriptDirName))
                return Path.Combine(ScriptDirName, relativePath);

            return Path.Combine(ScriptsParentDir, ScriptDirName, relativePath);
        }

        private static string GetScriptDir()
        {
            var dir = Environment.GetEnvironmentVariable("ms-script");
            if (dir == null)
                dir = "scripts";
            return dir;
        }

        public static List<FileInfo> LoadAllScript()
        {
            return new DirectoryInfo(Path.Combine(ScriptsParentDir, ScriptDirName)).GetFiles("*", SearchOption.AllDirectories).ToList();
        }
    }
}
