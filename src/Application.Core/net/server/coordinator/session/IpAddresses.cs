using System.Text.RegularExpressions;

namespace net.server.coordinator.session;

public class IpAddresses
{
    private static List<Regex> LOCAL_ADDRESS_PATTERNS = loadLocalAddressPatterns();

    private static List<Regex> loadLocalAddressPatterns()
    {
        return new string[] { "10\\.", "192\\.168\\.", "172\\.(1[6-9]|2[0-9]|3[0-1])\\." }.Select(x => new Regex(x)).ToList();
    }

    public static bool isLocalAddress(string inetAddress)
    {
        return inetAddress.StartsWith("127.");
    }

    public static bool isLanAddress(string inetAddress)
    {
        return LOCAL_ADDRESS_PATTERNS.Any(pattern => matchesPattern(pattern, inetAddress));
    }

    private static bool matchesPattern(Regex pattern, string searchTerm)
    {
        return pattern.IsMatch(searchTerm);
    }
}
