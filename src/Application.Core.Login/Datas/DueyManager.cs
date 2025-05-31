using Application.Core.Login.Models;
using Application.Core.Login.Services;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using AutoMapper;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Tls;
using System.Collections.Concurrent;

namespace Application.Core.Login.Datas
{
    public class DueyManager
    {
        ConcurrentDictionary<int, DueyPackageModel> _dataSource;

        readonly ILogger<DueyManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _maper;
        readonly MasterServer _server;
        readonly DataStorage _dataStorage;

        public DueyManager(ILogger<DueyManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper maper, MasterServer server, DataStorage dataStorage)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _maper = maper;

            _dataSource = new();
            _server = server;
            _dataStorage = dataStorage;
        }

        public async Task Setup()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var allPackages = _maper.Map<List<DueyPackageModel>>(await dbContext.Dueypackages.AsNoTracking().ToListAsync());
            foreach (var package in allPackages)
            {
                _dataSource[package.PackageId] = package;
            }
        }


        public Dto.CreatePackageCheckResponse CreateDueyPackageCheck(int senderId, string recipient)
        {
            var response = new Dto.CreatePackageCheckResponse();
            response.ReceiverId = -1;
            var target = _server.CharacterManager.FindPlayerByName(recipient);
            var sender = _server.CharacterManager.FindPlayerById(senderId);
            if (target == null || sender == null)
            {
                response.Code = (int)SendDueyItemResponseCode.CharacterNotExisted;
                return response;
            }

            if (target.Character.AccountId == sender.Character.AccountId)
            {
                response.Code = (int)SendDueyItemResponseCode.SameAccount;
                return response;
            }

            response.Code = (int)SendDueyItemResponseCode.Success;
            response.ReceiverId = target.Character.Id;
            return response;
        }


        public Dto.CreatePackageResponse CreateDueyPackage(int senderId, int sendMesos, Dto.ItemDto? item, string? sendMessage, int receiverId, bool quick)
        {
            try
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var dbModel = new DueyPackageEntity(receiverId, senderId, sendMesos, sendMessage, true, quick);
                dbContext.Dueypackages.Add(dbModel);

                if (dbContext.SaveChanges() < 1)
                {
                    _logger.LogError("Error trying to create namespace [mesos: {Meso}, sender: {Sender}, quick: {Quick}, receiver chrId: {Receiver}]",
                        sendMesos, senderId, quick, receiverId);
                    return new Dto.CreatePackageResponse { IsSuccess = false };
                }

                var model = _maper.Map<DueyPackageModel>(dbModel);
                _dataSource[model.PackageId] = model;
                _dataStorage.SetDueyPackage(model);
                return new Dto.CreatePackageResponse { IsSuccess = true };
            }
            catch (Exception sqle)
            {
                _logger.LogError(sqle.ToString());
            }

            return new Dto.CreatePackageResponse { IsSuccess = false };
        }

        public void RemovePackageById(int packageId)
        {
            if (_dataSource.TryRemove(packageId, out var d))
                _dataStorage.RemoveDueyPackage(d);
        }

        public void SendDueyNotification(string characterName)
        {
            var target = _server.CharacterManager.FindPlayerByName(characterName);
            if (target != null && target.Channel > 0)
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var sender = dbContext.Dueypackages.Where(x => x.ReceiverId == target.Character.Id && x.Checked)
                    .OrderByDescending(x => x.Type).Select(x => new { x.SenderId, x.Type }).FirstOrDefault();

                if (sender != null)
                {
                    var senderName = _server.CharacterManager.FindPlayerById(sender.SenderId);
                    dbContext.Dueypackages.Where(x => x.ReceiverId == target.Character.Id).ExecuteUpdate(x => x.SetProperty(y => y.Checked, false));
                    _server.Transport.SendDueyNotification(target.Channel, target.Character.Id, senderName.Character.Name, sender.Type);
                }
            }
        }
    }
}
