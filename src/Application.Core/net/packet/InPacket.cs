namespace net.packet;



public interface InPacket : Packet
{
    byte readByte();
    sbyte ReadSByte();
    short readShort();
    int readInt();
    long readLong();
    Point readPos();
    string readString();
    byte[] readBytes(int numberOfBytes);
    void skip(int numberOfBytes);
    int available();
    void seek(int byteOffset);
    int getPosition();
}
