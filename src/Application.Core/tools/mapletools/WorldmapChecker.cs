

using provider.wz;
using tools;

namespace tools.mapletools;


















/**
 * @author RonanLana
 * <p>
 * This application parses the Map.wz file inputted and reports areas (mapids) that are supposed to be referenced
 * throughout the map tree (area map -> continent map -> world map) but are currently missing.
 */
public class WorldmapChecker {
    private static Path OUTPUT_FILE = ToolConstants.getOutputFile("worldmap_report.txt");
    private static int INITIAL_STRING_LENGTH = 50;
    private static Dictionary<string, HashSet<int>> worldMapids = new ();
    private static Dictionary<string, string> parentWorldmaps = new ();
    private static HashSet<string> rootWorldmaps = new ();

    private static PrintWriter printWriter = null;
    private static BufferedReader bufferedReader = null;
    private static HashSet<int> currentWorldMapids;
    private static string currentParent;
    private static byte status = 0;
    private static bool isInfo;

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

    private static void translateToken(string token) {
        string d;

        if (token.Contains("/imgdir")) {
            status -= 1;
        } else if (token.Contains("imgdir")) {
            status += 1;

            if (status == 2) {
                d = getName(token);

                switch (d) {
                    case "MapList" -> isInfo = false;
                    case "info" -> isInfo = true;
                    default -> forwardCursor(status);
                }
            } else if (status == 4) {
                d = getName(token);

                if (!d.contentEquals("mapNo")) {
                    forwardCursor(status);
                }
            }
        } else {
            if (status == 4) {
                currentWorldMapids.Add(getValue(token));
            } else if (status == 2 && isInfo) {
                try {
                    d = getName(token);
                    if (d.contentEquals("parentMap")) {
                        currentParent = (getValue(token) + ".img.xml");
                    } else {
                        forwardCursor(status);
                    }
                } catch (Exception e) {
                    Console.WriteLine("failed '" + token + "'");

                }
            }
        }
    }

    private static void parseWorldmapFile(File worldmapFile) throws IOException {
        string line;

        InputStreamReader fileReader = new InputStreamReader(new FileInputStream(worldmapFile), StandardCharsets.UTF_8);
        bufferedReader = new BufferedReader(fileReader);

        currentParent = "";
        status = 0;

        currentWorldMapids = new ();
        while ((line = bufferedReader.readLine()) != null) {
            translateToken(line);
        }

        string worldmapName = worldmapFile.getName();
        worldMapids.Add(worldmapName, currentWorldMapids);

        if (currentParent.Count > 0) {
            parentWorldmaps.Add(worldmapName, currentParent);
        } else {
            rootWorldmaps.Add(worldmapName);
        }

        bufferedReader.close();
        fileReader.close();
    }

    private static void parseWorldmapDirectory() {
        File folder = new File(WZFiles.MAP.getFilePath(), "WorldMap");
        Console.WriteLine("Parsing directory '" + folder.getPath() + "'");
        foreach(File file in folder.listFiles()) {
            if (file.isFile()) {
                try {
                    parseWorldmapFile(file);
                } catch (FileNotFoundException ex) {
                    Console.WriteLine("Unable to open worldmap file " + file.getAbsolutePath() + ".");
                } catch (IOException ex) {
                    Console.WriteLine("Error reading worldmap file " + file.getAbsolutePath() + ".");
                } catch (Exception e) {
                    Log.Logger.Error(e.ToString());
                }
            }
        }
    }

    private static void printReportFileHeader() {
        printWriter.println(" # Report File autogenerated from the MapleWorldmapChecker feature by Ronan Lana.");
        printWriter.println(" # Generated data takes into account several data info from the server-side WZ.xmls.");
        printWriter.println();
    }

    private static void printReportFileResults(List<Pair<string, List<Pair<int, string>>>> results) {
        printWriter.println("Missing mapid references in top hierarchy:\n");
        foreach(Pair<string, List<Pair<int, string>>> res in results) {
            printWriter.println("'" + res.getLeft() + "':");

            foreach(Pair<int, string> i in res.getRight()) {
                printWriter.println("  " + i);
            }

            printWriter.println("\n");
        }
    }

    private static void verifyWorldmapTreeMapids() {
        try (PrintWriter pw = new PrintWriter(Files.newOutputStream(OUTPUT_FILE))) {
            printWriter = pw;
            printReportFileHeader();

            if (rootWorldmaps.Count > 1) {
                printWriter.println("[WARNING] Detected several root worldmaps: " + rootWorldmaps + "\n");
            }

            HashSet<string> worldmaps = new HashHashSet<>(parentWorldmaps.Keys);
            worldmaps.addAll(rootWorldmaps);

            Dictionary<string, HashSet<int>> tempMapids = new (worldMapids.Count);
            foreach(Map.Entry<string, HashSet<int>> e in worldMapids) {
                tempMapids.Add(e.Key, new HashHashSet<>(e.getValue()));
            }

            Dictionary<string, List<Pair<int, string>>> unreferencedMapids = new ();

            foreach(string s in worldmaps) {
                List<Pair<int, string>> currentUnreferencedMapids = new ();

                foreach(int i in tempMapids.get(s)) {
                    string parent = parentWorldmaps.get(s);

                    while (parent != null) {
                        HashSet<int> mapids = worldMapids.get(parent);
                        if (!mapids.Contains(i)) {
                            currentUnreferencedMapids.Add(new (i, parent));
                            break;
                        } else {
                            tempMapids.get(parent).Remove(i);
                        }

                        parent = parentWorldmaps.get(parent);
                    }
                }

                if (currentUnreferencedMapids.Count > 0) {
                    unreferencedMapids.Add(s, currentUnreferencedMapids);
                }
            }

            if (unreferencedMapids.Count > 0) {
                List<Pair<string, List<Pair<int, string>>>> unreferencedEntries = new (20);
                foreach(Map.Entry<string, List<Pair<int, string>>> e in unreferencedMapids) {
                    List<Pair<int, string>> list = new (e.getValue());
                    list.sort((o1, o2) -> o1.getLeft().compareTo(o2.getLeft()));

                    unreferencedEntries.Add(new (e.Key, list));
                }

                unreferencedEntries.sort((o1, o2) -> o1.getLeft().compareTo(o2.getLeft()));

                printReportFileResults(unreferencedEntries);
            }

        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    public static void main(string[] args) {
        parseWorldmapDirectory();
        verifyWorldmapTreeMapids();
    }
}

