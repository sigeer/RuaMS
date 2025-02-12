using YamlDotNet.Core.Tokens;
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

    public static string? SetValue(string name, string value)
    {
        var member = typeof(ServerConfig).GetField(name);
        if (member == null)
            return "没有找到配置 " + name;

        member.SetValue(config.server, Convert.ChangeType(value, member.FieldType));
        return null;
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
                    item.SetValue(fromConfigData.server, Convert.ChangeType(envValue, item.FieldType));
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
