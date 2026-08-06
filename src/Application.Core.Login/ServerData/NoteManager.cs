using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.ServerData;


public class NoteManager : DataStorageBase<int, ProtoModel.NoteProto, NoteEntity>
{
    readonly MasterServer _server;

    public NoteManager(IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, MasterServer masterServer, ILogger<NoteManager> logger) 
        : base(StorageCategory.Note, dbContextFactory, mapper, logger)
    {
        _server = masterServer;
    }


    protected override int GetKey(ProtoModel.NoteProto model) => model.Id;

    protected override ProtoModel.NoteProto MapModel(NoteEntity entities)
    {
        var item = base.MapModel(entities);
        item.From = _server.CharacterManager.GetPlayerName(item.FromId);
        item.To = _server.CharacterManager.GetPlayerName(item.ToId);
        return item;
    }

    /**
     * Send normal note from one character to another
     *
     * @return Send success
     */
    public async Task<bool> SendNormal(string message, int sender, string receiverName)
    {
        var chr = _server.CharacterManager.FindPlayerByName(receiverName);
        if (chr == null)
            return false;

        var model = new ProtoModel.NoteProto()
        {
            Id = Interlocked.Increment(ref _localId),
            ToId = chr.Character.Id,
            To = chr.Character.Name,
            FromId = sender,
            From = _server.CharacterManager.GetPlayerName(sender),
            Message = message,
            Timestamp = _server.getCurrentTime()
        };
        await SendNote(chr);
        SetDirty(model);
        return true;
    }

    public async Task SendNormal(string message, int sender, int reciverId)
    {
        var chr = _server.CharacterManager.FindPlayerById(reciverId);
        if (chr == null)
            return;

        var model = new ProtoModel.NoteProto()
        {
            Id = Interlocked.Increment(ref _localId),
            ToId = chr.Character.Id,
            To = chr.Character.Name,
            FromId = sender,
            From = _server.CharacterManager.GetPlayerName(sender),
            Message = message,
            Timestamp = _server.getCurrentTime()
        };
        await SendNote(chr);
        SetDirty(model);
    }

    /**
     * Send note which will increase the receiver's fame by one.
     *
     * @return Send success
     */
    public async Task SendWithFame(string message, int sender, string receiverName)
    {
        var chr = _server.CharacterManager.FindPlayerByName(receiverName);
        if (chr == null)
            return;

        var model = new ProtoModel.NoteProto()
        {
            Id = Interlocked.Increment(ref _localId),
            ToId = chr.Character.Id,
            To = chr.Character.Name,
            FromId = sender,
            From = _server.CharacterManager.GetPlayerName(sender),
            Message = message,
            Timestamp = _server.getCurrentTime(),
            Fame = 1
        };
        await SendNote(chr);
        SetDirty(model);
    }

    /**
     * Show unread notes
     *
     * @param chr Note recipient
     */
    public async Task SendNote(CharacterLiveObject liveObject)
    {
        if (liveObject.Channel <= 0)
            return;

        var notes = QueryByToId(liveObject.Character.Id).ToArray();
        if (notes.Length > 0)
            await _server.Transport.SendNotes(liveObject.Channel, liveObject.Character.Id, notes);
    }

    public ProtoModel.NoteProto? SetRead(int id)
    {
        var model = QueryById(id);
        if (model == null)
            return null;

        SetRemoved(model);
        return model;
    }


    public void removeFredrickReminders(List<int> expiredCids)
    {
        var toRemove = QueryFredrickExpired(expiredCids);
        foreach (var item in toRemove)
        {
            SetRemoved(item);
        }
    }

    protected override void CommitRemove(DBContext dbContext, NoteEntity? dbModel, ProtoModel.NoteProto localModel)
    {
        if (dbModel != null)
        {
            dbModel.Deleted = true;
        }
    }


    List<ProtoModel.NoteProto> QueryByToId(int toId) => Query(x => x.ToId == toId, x => x.ToId == toId);

    ProtoModel.NoteProto? QueryById(int id)
        => Find(id);

    List<ProtoModel.NoteProto> QueryFredrickExpired(List<int> expiredCids)
        => Query(
            x => x.FromId == -NpcId.FREDRICK && expiredCids.Contains(x.ToId),
            x => x.FromId == -NpcId.FREDRICK && expiredCids.Contains(x.ToId));
}
