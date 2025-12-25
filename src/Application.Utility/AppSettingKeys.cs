
namespace Application.Utility
{
    public class AppSettingKeys
    {
        public const string EnvPrefix = "RUA_MS_";

        public const string Database = "Database";
        public const string ConnectStr_Mysql = "MySql";

        public const string GrpcEndpoint = "Kestrel:Endpoints:grpc";
        public const string OpenApiEndpoint = "Kestrel:Endpoints:openapi";

        public const string LongIdSeed = "LongIdSeed";

        public const string Section_Script = "ScriptConfig";
        public const string Section_WZ = "WZConfig";

        public const string Grpc_Master = "http://_grpc.ruams-master";
    }
}
