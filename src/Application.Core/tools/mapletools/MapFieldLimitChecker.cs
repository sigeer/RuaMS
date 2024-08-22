

using provider.wz;

namespace tools.mapletools;








/**
 * @author RonanLana
 * <p>
 * This application seeks from the XMLs all mapid entries that holds the specified
 * fieldLimit.
 */
public class MapFieldLimitChecker {
    private static int INITIAL_STRING_LENGTH = 50;
    private static int FIELD_LIMIT = 0x400000;

    private static BufferedReader bufferedReader = null;
    private static byte status = 0;
    private static int mapid = 0;

    private static string getName(string token) {
        int i, j;
        char[] dest;
        string d;

        i = token.lastIndexOf("name");
        i = token.indexOf("\"", i) + 1; //lower bound of the string
        j = token.indexOf("\"", i);     //upper bound

        dest = new char[INITIAL_STRING_LENGTH];
        token.getChars(i, j, dest, 0);

        d = new string(dest);
        return (d.trim());
    }

    private static string getValue(string token) {
        int i, j;
        char[] dest;
        string d;

        i = token.lastIndexOf("value");
        i = token.indexOf("\"", i) + 1; //lower bound of the string
        j = token.indexOf("\"", i);     //upper bound

        dest = new char[INITIAL_STRING_LENGTH];
        token.getChars(i, j, dest, 0);

        d = new string(dest);
        return (d.trim());
    }

    private static void forwardCursor(int st) {
        string line = null;

        try {
            while (status >= st && (line = bufferedReader.readLine()) != null) {
                simpleToken(line);
            }
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void simpleToken(string token) {
        if (token.Contains("/imgdir")) {
            status -= 1;
        } else if (token.Contains("imgdir")) {
            status += 1;
        }
    }

    private static void listFiles(Path directory, <Path> files) {
        try (DirectoryStream<Path> stream = Files.newDirectoryStream(directory)) {
            foreach(Path path in stream) {
                if (Files.isRegularFile(path)) {
                    files.Add(path);
                } else if (Files.isDirectory(path)) {
                    listFiles(path, files);
                }
            }
        } catch (IOException e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static int getMapIdFromFilename(string name) {
        try {
            return int.Parse(name.Substring(0, name.indexOf('.')));
        } catch (Exception e) {
            return -1;
        }
    }

    private static void translateToken(string token) {
        if (token.Contains("/imgdir")) {
            status -= 1;
        } else if (token.Contains("imgdir")) {
            status += 1;

            if (status == 2) {
                string d = getName(token);
                if (!d.contentEquals("info")) {
                    forwardCursor(status);
                }
            }
        } else {
            if (status == 2) {
                string d = getName(token);

                if (d.contentEquals("fieldLimit")) {
                    int value = int.Parse(getValue(token));
                    if ((value & FIELD_LIMIT) == FIELD_LIMIT) {
                        Console.WriteLine(mapid + " " + value);
                    }
                }
            }
        }
    }

    private static void inspectMapEntry() {
        string line = null;

        try {
            while ((line = bufferedReader.readLine()) != null) {
                translateToken(line);
            }
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void loadMapWz() throws IOException {
        Console.WriteLine("Reading Map.wz ...");
        <Path> files = new ();
        listFiles(WZFiles.MAP.getFile().resolve("Map"), files);

        foreach(Path f in files) {
            try (BufferedReader br = Files.newBufferedReader(f)) {
                bufferedReader = br;

                mapid = getMapIdFromFilename(f.getFileName().ToString());
                inspectMapEntry();

            }
        }
    }

    public static void main(string[] args) {
        try {
            loadMapWz();
            Console.WriteLine("Done!");
        } catch (IOException ioe) {
            ioLog.Logger.Error(e.ToString());
        }
    }
}
