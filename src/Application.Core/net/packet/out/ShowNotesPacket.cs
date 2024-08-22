using net.opcodes;
using static tools.PacketCreator;

namespace net.packet.outs;

public class ShowNotesPacket : ByteBufOutPacket
{

    public ShowNotesPacket(List<DB_Note> notes) : base(SendOpcode.MEMO_RESULT)
    {
        writeByte(3);
        writeByte(notes.Count);
        notes.ForEach(writeNote);
    }

    private void writeNote(DB_Note note)
    {
        writeInt(note.Id);
        writeString(note.From + " "); //Stupid nexon forgot space lol
        writeString(note.Message);
        writeLong(getTime(note.Timestamp));
        writeByte(note.Fame);
    }
}
