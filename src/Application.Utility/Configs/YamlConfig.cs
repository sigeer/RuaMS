using YamlDotNet.Serialization;

namespace Application.Utility.Configs;

public class YamlConfig
{
    public const string CONFIG_FILE_NAME = "config.yaml";
    public static YamlConfig config = fromFile(CONFIG_FILE_NAME);

    public List<WorldConfig> worlds;
    public ServerConfig server;

    public static YamlConfig fromFile(string filename)
    {
        try
        {
            var content = File.ReadAllText(filename);
            var deserializer = new Deserializer();
            return deserializer.Deserialize<YamlConfig>(content);
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
