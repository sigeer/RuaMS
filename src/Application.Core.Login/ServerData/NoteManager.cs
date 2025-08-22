using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Core.Login.ServerData;


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

    public override async Task InitializeAsync(DBContext dbContext)
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
        {
            foreach (var item in notes)
            {
                item.From = _server.CharacterManager.GetPlayerName(item.FromId);
                item.To = liveObject.Character.Name;
            }
            _server.Transport.SendNotes(liveObject.Channel, liveObject.Character.Id, notes);
        }
    }

    public Dto.NoteDto? SetRead(int id)
    {
        var model = Query(x => x.Id == id && !x.IsDeleted).FirstOrDefault();
        if (model == null)
            return null;

        model.IsDeleted = true;
        SetDirty(model.Id, new StoreUnit<NoteModel>(StoreFlag.AddOrUpdate, model));
        var dto = _mapper.Map<Dto.NoteDto>(model);
        dto.From = _server.CharacterManager.GetPlayerName(model.FromId);
        dto.To = _server.CharacterManager.GetPlayerName(model.ToId);
        return dto;
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
                dbModel.Deleted = obj.IsDeleted;
                dbContext.Notes.Add(dbModel);

            }
        }

        await dbContext.SaveChangesAsync();
    }

    public override List<NoteModel> Query(Expression<Func<NoteModel, bool>> expression)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var dataFromDB = dbContext.Notes.AsNoTracking().ProjectToType<NoteModel>().Where(expression).ToList();

        return QueryWithDirty(dataFromDB, expression.Compile());
    }
}
