using Newtonsoft.Json;

namespace ServiceTest.TestUtilities
{
    [Explicit]
    public static class GoldenMaster
    {
        static JsonSerializerSettings options = new JsonSerializerSettings
        {
            ContractResolver = new PrivateContractResolver(),
            Formatting = Formatting.Indented
        };

        public static void Write<T>(string name, List<T> dataSource, Func<T, int> invokeName)
        {
            var dir = Path.Combine("");

            foreach (var item in dataSource)
            {
                var json = JsonConvert.SerializeObject(item, options);

                File.WriteAllText($"Golden/{name}/{invokeName(item)}.json", json);
            }
        }
    }
}
