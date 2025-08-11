namespace client.keybind;



/**
 * @author Shavit
 */
public class QuickslotBinding
{
    public static int QUICKSLOT_SIZE = 8;
    public static byte[] DEFAULT_QUICKSLOTS = { 0x2A, 0x52, 0x47, 0x49, 0x1D, 0x53, 0x4F, 0x51 };

    private byte[] m_aQuickslotKeyMapped;

    // Initializes quickslot object for the user.
    // aKeys' length has to be 8.
    public QuickslotBinding(byte[] aKeys)
    {
        if (aKeys.Length != QUICKSLOT_SIZE)
        {
            throw new ArgumentException(string.Format("aKeys' size should be {0}", QUICKSLOT_SIZE));
        }

        this.m_aQuickslotKeyMapped = Arrays.copyOf(aKeys, aKeys.Length);
    }

    public void encode(OutPacket p)
    {
        // Quickslots are default.
        // The client will skip them and call CQuickslotKeyMappedMan::DefaultQuickslotKeyMap.
        if (this.m_aQuickslotKeyMapped.SequenceEqual(DEFAULT_QUICKSLOTS))
        {
            p.writeBool(false);
            return;
        }

        p.writeBool(true);

        foreach (byte nKey in this.m_aQuickslotKeyMapped)
        {
            // For some reason Nexon sends these as integers, similar to CFuncKeyMappedMan.
            // However there's no evidence any key can be above 0xFF anyhow.
            // Regardless, we need to encode an integer to avoid an error 38 crash; as CFuncKeyMapped::m_aQuickslotKeyMapped is int[8].
            p.writeInt(nKey);
        }
    }

    public byte[] GetKeybindings()
    {
        return m_aQuickslotKeyMapped;
    }

}