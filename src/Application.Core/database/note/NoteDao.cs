namespace database.note;


public class NoteDao
{

    public void save(DB_Note note)
    {
        try
        {
            using var dbContext = new DBContext();
            dbContext.Notes.Add(note);
            dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            throw new DaoException(string.Format("Failed to save note: {0}", note.ToString()), e);
        }
    }

    public List<DB_Note> findAllByTo(string to)
    {
        try
        {
            using var dbContext = new DBContext();
            return dbContext.Notes.Where(x => x.To == to && x.Deleted == 0).ToList();
        }
        catch (Exception e)
        {
            throw new DaoException(string.Format("Failed to find notes sent to: %s", to), e);
        }
    }

    public DB_Note? delete(int id)
    {
        try
        {
            using var dbContext = new DBContext();
            var note = dbContext.Notes.FirstOrDefault(x => x.Id == id && x.Deleted == 0);
            if (note == null)
            {
                return null;
            }
            note.Deleted = 1;
            dbContext.SaveChanges();

            return note;
        }
        catch (Exception e)
        {
            throw new DaoException(string.Format("Failed to delete note with id: {0}", id), e);
        }
    }
}
