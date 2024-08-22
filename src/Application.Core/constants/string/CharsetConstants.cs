/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */



/*
 * Thanks to GabrielSin (EllinMS) - gabrielsin@playellin.net
 * Ellin
 * MapleStory Server
 * CharsetConstants
 */

namespace constants.String;

//public class CharsetConstants
//{
//    private static ILogger log = LogFactory.GetLogger("CharsetConstants");
//    public static Charset CHARSET = loadCharset();

//    private class Language : EnumClass
//    {

//        public static readonly Language LANGUAGE_PT_BR = new("ISO-8859-1");
//        public static readonly Language LANGUAGE_US = new("US-ASCII");
//        public static readonly Language LANGUAGE_THAI = new("TIS620");
//        public static readonly Language LANGUAGE_KOREAN = new("MS949");

//        private string charset;

//        Language(string charset)
//        {
//            this.charset = charset;
//        }

//        public string getCharset()
//        {
//            return charset;
//        }

//        public static Language fromCharset(string charset)
//        {
//            var ds = Language.values<Language>().Where(x => x.charset == charset).FirstOrDefault();
//            if (ds == null)
//            {
//                log.Warning("Charset {Charset} was not found, defaulting to US-ASCII", charset);
//                return LANGUAGE_US;
//            }

//            return ds;
//        }
//    }

//    private static string loadCharsetFromConfig()
//    {
//        try
//        {
//            YamlReader reader = new YamlReader(Files.newBufferedReader(Path.of(YamlConfig.CONFIG_FILE_NAME), StandardCharsets.US_ASCII));
//            reader.getConfig().readConfig.setIgnoreUnknownProperties(true);
//            StrippedYamlConfig charsetConfig = reader.read(StrippedYamlConfig);
//            reader.close();
//            return charsetConfig.server.CHARSET;
//        }
//        catch (FileNotFoundException e)
//        {
//            throw new Exception("Could not read config file " + YamlConfig.CONFIG_FILE_NAME + ": " + e.Message);
//        }
//        catch (IOException e)
//        {
//            throw new Exception("Could not successfully parse charset from config file " + YamlConfig.CONFIG_FILE_NAME + ": " + e.Message);
//        }
//    }

//    private static Charset loadCharset()
//    {
//        string configCharset = loadCharsetFromConfig();
//        if (configCharset != null)
//        {
//            Language language = Language.fromCharset(configCharset);
//            return Charset.forName(language.getCharset());
//        }

//        return StandardCharsets.US_ASCII;
//    }

//    public static class StrippedYamlConfig
//    {
//        public StrippedServerConfig server;

//        public class StrippedServerConfig
//        {
//            public string CHARSET;
//        }
//    }
//}