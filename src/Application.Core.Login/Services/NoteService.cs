using Application.Core.Login.Datas;
using Application.Core.model;
using Application.Core.ServerTransports;
using Application.EF;
using Application.Shared.Dto;
using Application.Utility.Exceptions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Application.Core.Login.Services;


public class NoteService
{
    readonly IDbContextFactory<DBContext> _dbContextFactory;
    readonly ILogger<NoteService> _logger;
    readonly CharacterManager _chrManager;
    readonly IMapper _mapper;
    readonly IMasterServerTransport _transport;

    public NoteService(IDbContextFactory<DBContext> dbContextFactory, ILogger<NoteService> logger, CharacterManager chrManager, IMapper mapper, IMasterServerTransport transport)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _chrManager = chrManager;
        _mapper = mapper;
        _transport = transport;
    }

    /**
     * Send normal note from one character to another
     *
     * @return Send success
     */
    public bool sendNormal(string message, string senderName, string receiverName, long sendTime)
    {
        var normalNote = NoteEntity.createNormal(message, senderName, receiverName, sendTime);
        return send(normalNote);
    }

    /**
     * Send note which will increase the receiver's fame by one.
     *
     * @return Send success
     */
    public bool sendWithFame(string message, string senderName, string receiverName, long sendTime)
    {
        var noteWithFame = NoteEntity.createGift(message, senderName, receiverName, sendTime);
        return send(noteWithFame);
    }

    private bool send(NoteEntity note)
    {
        // TODO: handle the following cases (originally listed at PacketCreator#noteError)
        /*
         *  0 = Player online, use whisper
         *  1 = Check player's name
         *  2 = Receiver inbox full
         */
        try
        {
            save(note);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to send note {Note}", note);
            return false;
        }
    }

    /**
     * Show unread notes
     *
     * @param chr Note recipient
     */
    public void show(string receiverName)
    {
        var chr = _chrManager.FindPlayerByName(receiverName);
        if (chr == null || chr.Channel <= 0)
            return;

        var notes = findAllByTo(chr.Character.Name);
        if (notes.Length > 0)
            _transport.SendNotes(chr.Channel, chr.Character.Id, notes);
    }


    public void save(NoteEntity note)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Notes.Add(note);
            dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            throw new BusinessException(string.Format("Failed to save note: {0}", note.ToString()), e);
        }
    }

    public NoteDto[] findAllByTo(string to)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return _mapper.Map<NoteDto[]>(dbContext.Notes.Where(x => x.To == to && x.Deleted == 0).ToList());
        }
        catch (Exception e)
        {
            throw new BusinessException(string.Format("Failed to find notes sent to: {0}", to), e);
        }
    }

    public NoteDto? delete(int id)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var note = dbContext.Notes.FirstOrDefault(x => x.Id == id && x.Deleted == 0);
            if (note == null)
            {
                return null;
            }
            note.Deleted = 1;
            dbContext.SaveChanges();

            return _mapper.Map<NoteDto>(note);
        }
        catch (Exception e)
        {
            throw new BusinessException(string.Format("Failed to delete note with id: {0}", id), e);
        }
    }

    public void removeFredrickReminders(DBContext dbContext, List<CharacterIdWorldPair> expiredCids)
    {
        List<string> expiredCnames = expiredCids.Select(x => x.Name).ToList();
        try
        {
            dbContext.Notes.Where(x => x.From == "FREDRICK" && expiredCnames.Contains(x.To)).ExecuteDelete();
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
    }

}
