using System.Text.RegularExpressions;

namespace net.server.coordinator.session;



public partial record Hwid(string hwid)
{
    private static int HWID_LENGTH = 8;
    // First part is a mac address (without dashes), second part is the hwid
    private static Regex VALID_HOST_STRING_PATTERN = CheckHwidReg();

    private static bool isValidHostString(string hostString)
    {
        return VALID_HOST_STRING_PATTERN.IsMatch(hostString);
    }

    public static Hwid fromHostString(string hostString)
    {
        if (hostString == null || !isValidHostString(hostString))
        {
            throw new ArgumentException("hostString has invalid format");
        }

        string[] Split = hostString.Split("_");
        if (Split.Length != 2 || Split[1].Length != HWID_LENGTH)
        {
            throw new ArgumentException("Hwid validation failed for hwid: " + hostString);
        }

        return new Hwid(Split[1]);
    }

    public static Hwid Default()
    {
        return new Hwid("");
    }

    [GeneratedRegex("[0-9A-F]{12}_[0-9A-F]{8}")]
    private static partial Regex CheckHwidReg();
}
