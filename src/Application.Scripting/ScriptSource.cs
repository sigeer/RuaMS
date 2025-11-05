using Microsoft.Extensions.Configuration;

namespace Application.Scripting
{
    public class ScriptSource
    {
        /// <summary>
        /// 脚本根目录
        /// </summary>
        public string BaseDir { get; set; }
        /// <summary>
        /// 默认使用脚本类型
        /// </summary>
        public ScriptType DefaultScriptType { get; set; }
        /// <summary>
        /// 事件脚本目录名
        /// </summary>
        public string EventDirName { get; set; }

        public ScriptSource(IConfiguration configuration)
        {
            BaseDir = configuration.GetValue<string>("BaseDir", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts"));
            DefaultScriptType = configuration.GetValue<ScriptType>("DefaultScriptType", ScriptType.Lua);
            EventDirName = configuration.GetValue("EventDirName", "event");
        }

        public List<FileInfo> LoadAllScript()
        {
            return new DirectoryInfo(BaseDir).GetFiles("*", SearchOption.AllDirectories).ToList();
        }

        public string GetScriptFullPath(string relativePath)
        {
            if (!Path.IsPathRooted(relativePath))
                return Path.Combine(BaseDir, relativePath);

            return relativePath;
        }

        public string[] GetEvents()
        {
            return Directory.GetFiles(Path.Combine(BaseDir, EventDirName))
                .Select(x => Path.GetFileNameWithoutExtension(x))
                .Where(x => !x.StartsWith("__")).ToArray();
        }

        public static ScriptSource Instance { get; set; } = null!;
    }
}
