using Application.Core.Models;

namespace net.packet.outs;

public class ShowNotesPacket : ByteBufOutPacket
{

    public ShowNotesPacket(IChannelClient client, List<NoteObject> notes) : base(SendOpcode.MEMO_RESULT)
    {
        writeByte(3);
        writeByte(notes.Count);
        notes.ForEach(x => writeNote(client, x));
    }

    private void writeNote(IChannelClient client, NoteObject note)
    {
        writeInt(note.Id);
        writeString(note.GetFromName(client) + " "); //Stupid nexon forgot space lol
        writeString(note.Message);
        writeLong(PacketCommon.getTime(note.Timestamp));
        writeByte(note.Fame);
    }
}
