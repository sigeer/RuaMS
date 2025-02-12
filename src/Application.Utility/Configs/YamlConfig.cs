using YamlDotNet.Serialization;

namespace Application.Utility.Configs;

public class YamlConfig
{
    public const string CONFIG_FILE_NAME = "config.yaml";
    public static YamlConfig config = FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONFIG_FILE_NAME));

    public ServerConfig server;

    public static void Reload()
    {
        config = FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONFIG_FILE_NAME));
    }

    public static YamlConfig FromFile(string filename)
    {
        try
        {
            var content = File.ReadAllText(filename);
            var deserializer = new Deserializer();
            var fromConfigData = deserializer.Deserialize<YamlConfig>(content);

            var members = typeof(ServerConfig).GetFields();
            foreach (var item in members)
            {
                var envValue = Environment.GetEnvironmentVariable("COSMIC_DOTNET_" + item.Name);
                if (!string.IsNullOrEmpty(envValue))
                {
                    item.SetValue(fromConfigData.server, envValue);
                }
            }
            return fromConfigData;
        }
        catch (FileNotFoundException e)
        {
            string message = "Could not read config file " + filename + ": " + e.Message;
            throw new Exception(message);
        }
        catch (Exception e)
        {
            string message = "Could not successfully parse config file " + filename + ": " + e.Message;
            throw new Exception(message);
        }
    }
}
