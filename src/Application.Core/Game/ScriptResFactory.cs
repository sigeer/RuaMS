namespace Application.Core.Game
{
    public class ScriptResFactory
    {
        public static readonly string ScriptsParentDir = AppDomain.CurrentDomain.BaseDirectory;
        public const string ScriptDirName = "scripts";
        public static string GetScriptFullPath(string relativePath)
        {
            if (relativePath.StartsWith(ScriptResFactory.ScriptDirName))
            {
                return Path.Combine(ScriptsParentDir, relativePath);
            }

            return Path.Combine(ScriptsParentDir, ScriptDirName, relativePath);
        }

        public static List<FileInfo> LoadAllScript()
        {
            return new DirectoryInfo(Path.Combine(ScriptsParentDir, ScriptDirName)).GetFiles("*", SearchOption.AllDirectories).ToList();
        }
    }
}
