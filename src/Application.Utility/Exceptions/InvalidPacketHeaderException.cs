namespace Application.Utility.Exceptions;

public class InvalidPacketHeaderException : Exception
{
    private int header;

    public InvalidPacketHeaderException(string message, int header) : base(message)
    {
        this.header = header;
    }

    public int getHeader()
    {
        return header;
    }
}
