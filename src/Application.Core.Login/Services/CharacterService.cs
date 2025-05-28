using Application.EF;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Application.Core.Login.Services
{
    public class CharacterService
    {
        readonly IMapper _mapper;
        readonly IDbContextFactory<DBContext> _dbContextFactory;

        private static string[] BLOCKED_NAMES = {
            "admin", "owner", "moderator", "intern", "donor", "administrator", "FREDRICK", "help", "helper", "alert", "notice", "maplestory", "fuck", "wizet", "fucking",
            "negro", "fuk", "fuc", "penis", "pussy", "asshole", "gay", "nigger", "homo", "suck", "cum", "shit", "shitty", "condom", "security", "official", "rape", "nigga",
            "sex", "tit", "boner", "orgy", "clit", "asshole", "fatass", "bitch", "support", "gamemaster", "cock", "gaay", "gm", "operate", "master",
            "sysop", "party", "GameMaster", "community", "message", "event", "test", "meso", "Scania", "yata", "AsiaSoft", "henesys"};

        public CharacterService(IMapper mapper, IDbContextFactory<DBContext> dbContextFactory)
        {
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
        }

        public bool CheckCharacterName(string name)
        {
            // 禁用名
            if (BLOCKED_NAMES.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return false;

            var bLength = GlobalTools.Encoding.GetBytes(name).Length;
            if (bLength < 3 || bLength > 12)
                return false;

            if (!Regex.IsMatch(name, "^[a-zA-Z0-9\\u4e00-\\u9fa5]+$"))
                return false;

            using DBContext dbContext = _dbContextFactory.CreateDbContext();
            if (dbContext.Characters.Any(x => x.Name == name))
                return false;

            return true;
        }
    }
}
