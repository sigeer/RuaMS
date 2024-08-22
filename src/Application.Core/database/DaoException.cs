namespace database;

public class DaoException : Exception
{

    public DaoException(string message, Exception cause) : base(message, cause)
    {

    }
}
