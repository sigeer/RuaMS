using System.Text.Json;

namespace Application.Core.Tools
{
    public class FileCache
    {
        public const string CacheDir = "cache";
        private static string GetPath(string key)
        {
            if (!Directory.Exists(CacheDir))
                Directory.CreateDirectory(CacheDir);
            return Path.Combine(CacheDir, $"x_{key}.x");
        }
        public static void Save<TModel>(string key, TModel obj)
        {
            File.WriteAllText(GetPath(key), JsonSerializer.Serialize(obj));
        }

        public static TModel? Get<TModel>(string key)
        {
            var filePath = GetPath(key);
            if (!File.Exists(filePath))
                return default;

            try
            {
                return JsonSerializer.Deserialize<TModel>(File.ReadAllText(filePath));
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static TModel? GetOrCreate<TModel>(string key, Func<TModel> func)
        {
            var model = Get<TModel>(key);
            if (model == null)
            {
                model = func();
                Save(key, model);
            }
            return model;
        }
    }
}
