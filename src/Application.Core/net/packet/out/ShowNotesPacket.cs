using Application.Core.Models;

namespace net.packet.outs;

public class ShowNotesPacket : ByteBufOutPacket
{

    public ShowNotesPacket(List<NoteObject> notes) : base(SendOpcode.MEMO_RESULT)
    {
        writeByte(3);
        writeByte(notes.Count);
        notes.ForEach(writeNote);
    }

    private void writeNote(NoteObject note)
    {
        writeInt(note.Id);
        writeString(note.From + " "); //Stupid nexon forgot space lol
        writeString(note.Message);
        writeLong(PacketCommon.getTime(note.Timestamp));
        writeByte(note.Fame);
    }
}
