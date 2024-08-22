

using provider.wz;

namespace tools.mapletools;










/**
 * @author RonanLana
 * <p>
 * The main objective of this tool is to locate all mapids that doesn't have
 * the "info" node in their WZ node tree.
 */
public class MapInfoRetriever {
    private static Path OUTPUT_FILE = ToolConstants.getOutputFile("map_info_report.txt");
    private static List<int> missingInfo = new ();

    private static byte status = 0;
    private static bool hasInfo;

    private static string getName(string token) {
        int i, j;
        char[] dest;
        string d;

        i = token.lastIndexOf("name");
        i = token.indexOf("\"", i) + 1; //lower bound of the string
        j = token.indexOf("\"", i);     //upper bound

        dest = new char[50];
        token.getChars(i, j, dest, 0);

        d = new string(dest);
        return (d.trim());
    }

    private static void forwardCursor(BufferedReader reader, int st) {
        string line = null;

        try {
            while (status >= st && (line = reader.readLine()) != null) {
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

    private static bool translateToken(BufferedReader reader, string token) {
        string d;
        int temp;

        if (token.Contains("/imgdir")) {
            status -= 1;
        } else if (token.Contains("imgdir")) {
            if (status == 1) {
                d = getName(token);
                if (d.Contains("info")) {
                    hasInfo = true;
                    return true;
                }

                temp = status;
                forwardCursor(reader, temp);
            }

            status += 1;
        }

        return false;
    }

    private static void searchMapDirectory(int mapArea) {
        Path mapDirectory = Path.of(WZFiles.MAP.getFilePath() + "/Map/Map" + int.toString(mapArea));
        Console.WriteLine("Parsing map area " + mapArea);
        try {
            Files.walk(mapDirectory)
                    .filter(MapInfoRetriever::isRelevantFile)
                    .forEach(MapInfoRetriever::searchMapFile);
        } catch (UncheckedIOException | IOException e) {
            System.err.println("Directory " + mapDirectory.getFileName().ToString() + " does not exist");
        }
    }

    private static bool isRelevantFile(Path file) {
        return file.getFileName().ToString().endsWith(".xml");
    }

    private static void searchMapFile(Path file) {
        // This will reference one line at a time
        string line = null;

        try (BufferedReader reader = Files.newBufferedReader(file)) {
            hasInfo = false;
            status = 0;

            while ((line = reader.readLine()) != null) {
                if (translateToken(reader, line)) {
                    break;
                }
            }

            if (!hasInfo) {
                missingInfo.Add(file.getFileName(.ToString().Split(".img.xml")[0]));
            }
        } catch (IOException ex) {
            Console.WriteLine("Error reading file '" + file.getFileName().ToString() + "'");
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void writeReport() {
        try (PrintWriter printWriter = new PrintWriter(Files.newOutputStream(OUTPUT_FILE))) {
            if (missingInfo.Count > 0) {
                foreach(int i in missingInfo) {
                    printWriter.println(i);
                }
            } else {
                printWriter.println("All map files contain 'info' node.");
            }
        } catch (IOException e) {
            Log.Logger.Error(e.ToString());
        }
    }

    public static void main(string[] args) {
        for (int i = 0; i <= 9; i++) {
            searchMapDirectory(i);
        }
        writeReport();
    }

}

