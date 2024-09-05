using database;
using database.note;
using net.packet.outs;
using net.server;

namespace service;


public class NoteService
{
    private static ILogger log = LogFactory.GetLogger(LogType.NoteService);

    private NoteDao noteDao;

    public NoteService(NoteDao noteDao)
    {
        this.noteDao = noteDao;
    }

    /**
     * Send normal note from one character to another
     *
     * @return Send success
     */
    public bool sendNormal(string message, string senderName, string receiverName)
    {
        var normalNote = DB_Note.createNormal(message, senderName, receiverName, Server.getInstance().getCurrentTime());
        return send(normalNote);
    }

    /**
     * Send note which will increase the receiver's fame by one.
     *
     * @return Send success
     */
    public bool sendWithFame(string message, string senderName, string receiverName)
    {
        var noteWithFame = DB_Note.createGift(message, senderName, receiverName, Server.getInstance().getCurrentTime());
        return send(noteWithFame);
    }

    private bool send(DB_Note note)
    {
        // TODO: handle the following cases (originally listed at PacketCreator#noteError)
        /*
         *  0 = Player online, use whisper
         *  1 = Check player's name
         *  2 = Receiver inbox full
         */
        try
        {
            noteDao.save(note);
            return true;
        }
        catch (Exception e)
        {
            log.Error(e, "Failed to send note {Note}", note);
            return false;
        }
    }

    /**
     * Show unread notes
     *
     * @param chr Note recipient
     */
    public void show(IPlayer? chr)
    {
        if (chr == null)
        {
            throw new ArgumentException("Unable to show notes - chr is null");
        }

        var notes = getNotes(chr.getName());
        if (notes.Count == 0)
        {
            return;
        }

        chr.sendPacket(new ShowNotesPacket(notes));
    }

    private List<DB_Note> getNotes(string to)
    {
        try
        {
            return noteDao.findAllByTo(to);
        }
        catch (Exception e)
        {
            log.Error(e, "Failed to find notes sent to chr name {CharacterName}", to);
            return [];
        }
    }

    /**
     * Delete a read note
     *
     * @param noteId Id of note to discard
     * @return Discarded note. Empty optional if failed to discard.
     */
    public DB_Note? delete(int noteId)
    {
        try
        {
            return noteDao.delete(noteId);
        }
        catch (DaoException e)
        {
            log.Error(e, "Failed to discard note with id {NoteId}", noteId);
            return null;
        }
    }

}
