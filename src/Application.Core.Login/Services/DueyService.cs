using Application.Core.EF.Entities.Items;
using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.EF;
using Application.Shared.Items;
using AutoMapper;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Services
{
    public class DueyService
    {
        readonly ILogger<DueyService> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;
        readonly MasterServer _server;

        public DueyService(ILogger<DueyService> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, MasterServer server)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _server = server;
        }

    }
}
