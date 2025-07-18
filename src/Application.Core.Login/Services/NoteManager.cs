using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.Utility;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using client.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Core.Login.Services;


public class NoteManager : StorageBase<int, NoteModel>
{
    readonly IDbContextFactory<DBContext> _dbContextFactory;
    readonly ILogger<NoteManager> _logger;
    readonly IMapper _mapper;
    readonly MasterServer _server;

    int _localId = 0;
    public NoteManager(IDbContextFactory<DBContext> dbContextFactory, ILogger<NoteManager> logger, IMapper mapper, MasterServer masterServer)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _mapper = mapper;
        _server = masterServer;
    }

    public async Task InitializeAsync(DBContext dbContext)
    {
        _localId = await dbContext.Notes.MaxAsync(x => (int?)x.Id) ?? 0;
    }

    /**
     * Send normal note from one character to another
     *
     * @return Send success
     */
    public bool SendNormal(string message, int sender, string receiverName)
    {
        var chr = _server.CharacterManager.FindPlayerByName(receiverName);
        if (chr == null)
            return false;

        var model = new NoteModel()
        {
            Id = Interlocked.Increment(ref _localId),
            FromId = sender,
            ToId = chr.Character.Id,
            Message = message,
            Timestamp = _server.getCurrentTime()
        };
        SendNote(chr);
        return SetDirty(model.Id, new StoreUnit<NoteModel>(StoreFlag.AddOrUpdate, model));
    }

    public bool SendNormal(string message, int sender, int reciverId)
    {
        var chr = _server.CharacterManager.FindPlayerById(reciverId);
        if (chr == null)
            return false;

        var model = new NoteModel()
        {
            Id = Interlocked.Increment(ref _localId),
            FromId = sender,
            ToId = chr.Character.Id,
            Message = message,
            Timestamp = _server.getCurrentTime()
        };
        SendNote(chr);
        return SetDirty(model.Id, new StoreUnit<NoteModel>(StoreFlag.AddOrUpdate, model));
    }

    /**
     * Send note which will increase the receiver's fame by one.
     *
     * @return Send success
     */
    public bool SendWithFame(string message, int sender, string receiverName)
    {
        var chr = _server.CharacterManager.FindPlayerByName(receiverName);
        if (chr == null)
            return false;

        var model = new NoteModel()
        {
            Id = Interlocked.Increment(ref _localId),
            FromId = sender,
            ToId = chr.Character.Id,
            Message = message,
            Timestamp = _server.getCurrentTime(),
            Fame = 1
        };
        SendNote(chr);
        return SetDirty(model.Id, new StoreUnit<NoteModel>(StoreFlag.AddOrUpdate, model));
    }

    /**
     * Show unread notes
     *
     * @param chr Note recipient
     */
    public void SendNote(CharacterLiveObject liveObject)
    {
        if (liveObject.Channel <= 0)
            return;

        var notes = _mapper.Map<Dto.NoteDto[]>(Query(x => x.ToId == liveObject.Character.Id && !x.IsDeleted));
        if (notes.Length > 0)
            _server.Transport.SendNotes(liveObject.Channel, liveObject.Character.Id, notes);
    }

    public Dto.NoteDto? SetRead(int id)
    {
        var model = Query(x => x.Id == id && !x.IsDeleted).FirstOrDefault();
        if (model == null)
            return null;

        model.IsDeleted = true;
        SetDirty(model.Id, new StoreUnit<NoteModel>(StoreFlag.AddOrUpdate, model));
        return _mapper.Map<Dto.NoteDto>(model);
    }

    public void removeFredrickReminders(List<int> expiredCids)
    {
        var toRemove = Query(x => x.FromId == -NpcId.FREDRICK && expiredCids.Contains(x.ToId));
        foreach (var item in toRemove)
        {
            SetRemoved(item.Id);
        }
    }

    protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<NoteModel>> updateData)
    {
        await dbContext.Notes.Where(x => updateData.Keys.Contains(x.Id)).ExecuteDeleteAsync();
        foreach (var item in updateData)
        {
            if (item.Value.Flag == StoreFlag.AddOrUpdate)
            {
                var obj = item.Value.Data!;

                NoteEntity dbModel = new NoteEntity(obj.Id, obj.ToId, obj.FromId, obj.Message, obj.Timestamp, obj.Fame);
                dbModel.Deleted = obj.IsDeleted ? 1 : 0;
                dbContext.Notes.Add(dbModel);

            }
        }

        await dbContext.SaveChangesAsync();
    }

    public override List<NoteModel> Query(Expression<Func<NoteModel, bool>> expression)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var entityExpression = _mapper.MapExpression<Expression<Func<NoteEntity, bool>>>(expression);

        var dataFromDB = _mapper.Map<List<NoteModel>>(dbContext.Notes.Where(entityExpression).AsNoTracking().ToList());

        return QueryWithDirty(dataFromDB, expression.Compile());
    }
}
