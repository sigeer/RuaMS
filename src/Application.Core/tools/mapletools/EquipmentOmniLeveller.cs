

using provider.wz;

namespace tools.mapletools;











/**
 * @author RonanLana
 * <p>
 * This application parses the Character.wz folder inputted and adds/updates the "info/level"
 * node on every known equipment id. This addition enables client-side view of the equipment
 * level attribute on every equipment in the game, given proper item visibility, be it from
 * own equipments or from other players.
 * <p>
 * Estimated parse time: 7 minutes
 */
public class EquipmentOmniLeveller {
    private static Path INPUT_DIRECTORY = WZFiles.CHARACTER.getFile();
    private static Path OUTPUT_DIRECTORY = ToolConstants.getOutputFile("equips-with-levels");
    private static int INITIAL_STRING_LENGTH = 250;
    private static int FIXED_EXP = 10000;
    private static int MAX_EQP_LEVEL = 30;

    private static PrintWriter printWriter = null;
    private static BufferedReader bufferedReader = null;

    private static int infoTagState = -1;
    private static int infoTagExpState = -1;
    private static bool infoTagLevel;
    private static bool infoTagLevelExp;
    private static bool infoTagLevelInfo;
    private static int parsedLevels = 0;
    private static byte status;
    private static bool upgradeable;
    private static bool cash;

    private static string getName(string token) {
        int i, j;
        char[] dest;
        string d;

        i = token.lastIndexOf("name");
        i = token.indexOf("\"", i) + 1; //lower bound of the string
        j = token.indexOf("\"", i);     //upper bound

        dest = new char[INITIAL_STRING_LENGTH];
        try {
            token.getChars(i, j, dest, 0);
        } catch (StringIndexOutOfRangeException e) {
            // do nothing
            return "";
        } catch (Exception e) {
            Console.WriteLine("error in: " + token + "");
            Log.Logger.Error(e.ToString());
            try {
                Thread.sleep(100000000);
            } catch (Exception ex) {
            }
        }


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
                printWriter.println(line);
            }
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void translateLevelCursor(int st) {
        string line = null;

        try {
            infoTagLevelInfo = false;
            while (status >= st && (line = bufferedReader.readLine()) != null) {
                translateLevelToken(line);
            }
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void translateInfoTag(int st) {
        infoTagLevel = false;
        string line = null;

        try {
            while (status >= st && (line = bufferedReader.readLine()) != null) { // skipping directory & canvas definition
                translateInfoToken(line);
            }
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }

        if (!upgradeable || cash) {
            throw new RuntimeException();
        }
    }

    private static void simpleToken(string token) {
        if (token.Contains("/imgdir")) {
            status -= 1;
        } else if (token.Contains("imgdir")) {
            status += 1;
        }
    }

    private static void printUpdatedLevelExp() {
        printWriter.println("          <int name=\"exp\" value=\"" + FIXED_EXP + "\"/>");
    }

    private static void printDefaultLevel(int level) {
        printWriter.println("        <imgdir name=\"" + level + "\">");
        printUpdatedLevelExp();
        printWriter.println("        </imgdir>");
    }

    private static void printDefaultLevelInfoTag() {
        printWriter.println("      <imgdir name=\"info\">");
        for (int i = 1; i <= MAX_EQP_LEVEL; i++) {
            printDefaultLevel(i);
        }
        printWriter.println("      </imgdir>");
    }

    private static void printDefaultLevelTag() {
        printWriter.println("    <imgdir name=\"level\">");
        printDefaultLevelInfoTag();
        printWriter.println("    </imgdir>");
    }

    private static void processLevelInfoTag(int st) {
        string line;
        try {
            while (status >= st && (line = bufferedReader.readLine()) != null) {
                translateLevelExpToken(line);
            }
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void processLevelInfoSet(int st) {
        parsedLevels = (1 << MAX_EQP_LEVEL) - 1;

        string line;
        try {
            while (status >= st && (line = bufferedReader.readLine()) != null) {
                translateLevelInfoSetToken(line);
            }
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void translateLevelToken(string token) {
        if (token.Contains("/imgdir")) {
            if (status == 3) {
                if (!infoTagLevelInfo) {
                    printDefaultLevelInfoTag();
                }
            }
            printWriter.println(token);

            status -= 1;
        } else if (token.Contains("imgdir")) {
            printWriter.println(token);
            status += 1;

            if (status == 4) {
                string d = getName(token);
                if (d.contentEquals("info")) {
                    infoTagLevelInfo = true;
                    processLevelInfoSet(status);
                } else {
                    forwardCursor(status);
                }
            }
        } else {
            printWriter.println(token);
        }
    }

    private static void translateLevelInfoSetToken(string token) {
        if (token.Contains("/imgdir")) {
            status -= 1;

            if (status == 3) {
                if (parsedLevels != 0) {
                    for (int i = 0; i < MAX_EQP_LEVEL; i++) {
                        if ((parsedLevels >> i) % 2 != 0) {
                            int level = i + 1;
                            printDefaultLevel(level);
                        }
                    }
                }
            }

            printWriter.println(token);
        } else if (token.Contains("imgdir")) {
            printWriter.println(token);
            status += 1;

            if (status == 5) {
                int level = int.Parse(getName(token)) - 1;
                parsedLevels ^= (1 << level);

                infoTagLevelExp = false;
                infoTagExpState = status;  // status: 5
                processLevelInfoTag(status);
                infoTagExpState = -1;
            }
        } else {
            printWriter.println(token);
        }
    }

    private static void translateLevelExpToken(string token) {
        if (token.Contains("/imgdir")) {
            status -= 1;

            if (status < infoTagExpState) {
                if (!infoTagLevelExp) {
                    printUpdatedLevelExp();
                }
            }

            printWriter.println(token);
        } else if (token.Contains("imgdir")) {
            printWriter.println(token);
            status += 1;

            forwardCursor(status);
        } else {
            string name = getName(token);
            if (name.contentEquals("exp")) {
                infoTagLevelExp = true;
                printUpdatedLevelExp();
            } else {
                printWriter.println(token);
            }
        }
    }

    private static void translateInfoToken(string token) {
        if (token.Contains("/imgdir")) {
            status -= 1;

            if (status < infoTagState) {
                if (!infoTagLevel) {
                    printDefaultLevelTag();
                }
            }

            printWriter.println(token);
        } else if (token.Contains("imgdir")) {
            status += 1;
            printWriter.println(token);

            string d = getName(token);
            if (d.contentEquals("level")) {
                infoTagLevel = true;
                translateLevelCursor(status);
            } else {
                forwardCursor(status);
            }
        } else {
            string name = getName(token);

            switch (name) {
                case "cash":
                    if (!getValue(token).contentEquals("0")) {
                        cash = true;
                    }
                    break;

                case "tuc":
                case "incPAD":
                case "incMAD":
                case "incPDD":
                case "incMDD":
                case "incACC":
                case "incEVA":
                case "incSpeed":
                case "incJump":
                case "incMHP":
                case "incMMP":
                case "incSTR":
                case "incDEX":
                case "incINT":
                case "incLUK":
                    if (!getValue(token).contentEquals("0")) {
                        upgradeable = true;
                    }
                    break;
            }

            printWriter.println(token);
        }
    }

    private static bool translateToken(string token) {
        bool accessInfoTag = false;

        if (token.Contains("/imgdir")) {
            status -= 1;
            printWriter.println(token);
        } else if (token.Contains("imgdir")) {
            printWriter.println(token);
            status += 1;

            if (status == 2) {
                string d = getName(token);
                if (!d.contentEquals("info")) {
                    forwardCursor(status);
                } else {
                    accessInfoTag = true;
                }
            } else if (status > 2) {
                forwardCursor(status);
            }
        } else {
            printWriter.println(token);
        }

        return accessInfoTag;
    }

    private static void copyCashItemData(Path file, string curPath) throws IOException {
        try (PrintWriter pw = new PrintWriter(
                Files.newOutputStream(OUTPUT_DIRECTORY.resolve(curPath).resolve(file.getFileName())));
                BufferedReader br = Files.newBufferedReader(file);) {
            printWriter = pw;
            bufferedReader = br;
            string line;
            while ((line = bufferedReader.readLine()) != null) {
                printWriter.println(line);
            }
        }
    }

    private static void parseEquipData(Path file, string curPath) throws IOException {
        try (PrintWriter pw = new PrintWriter(
                Files.newOutputStream(OUTPUT_DIRECTORY.resolve(curPath).resolve(file.getFileName())));
                BufferedReader br = Files.newBufferedReader(file);) {
            printWriter = pw;
            bufferedReader = br;
            status = 0;
            upgradeable = false;
            cash = false;

            string line;
            while ((line = bufferedReader.readLine()) != null) {
                if (translateToken(line)) {
                    infoTagState = status; // status: 2
                    translateInfoTag(status);
                    infoTagState = -1;
                }
            }
            printFileFooter();
        } catch (RuntimeException e) {
            copyCashItemData(file, curPath);
        }
    }

    private static void printFileFooter() {
        printWriter.println("<!--");
        printWriter.println(" # WZ XML File parsed by the MapleEquipmentOmnilever feature by Ronan Lana.");
        printWriter.println(" # Generated data takes into account info from the server-side WZ.xmls.");
        printWriter.println("-->");
    }

    private static void parseDirectoryEquipData(string curPath) {
        Path folder = OUTPUT_DIRECTORY.resolve(curPath);
        if (!Files.exists(folder)) {
            try {
                Files.createDirectory(folder);
            } catch (IOException e) {
                // TODO Auto-generated catch block
                Console.WriteLine("Unable to create folder " + folder.toAbsolutePath() + ".");
                Log.Logger.Error(e.ToString());
            }
        }

        Console.WriteLine("Parsing directory '" + curPath + "'");
        folder = INPUT_DIRECTORY.resolve(curPath);
        try (DirectoryStream<Path> stream = Files.newDirectoryStream(folder)) {
            foreach(Path path in stream) {
                if (Files.isRegularFile(path)) {
                    try {
                        parseEquipData(path, curPath);
                    } catch (FileNotFoundException ex) {
                        Console.WriteLine("Unable to open dojo file " + path.toAbsolutePath() + ".");
                    } catch (IOException ex) {
                        Console.WriteLine("Error reading dojo file " + path.toAbsolutePath() + ".");
                    } catch (Exception e) {
                        Log.Logger.Error(e.ToString());
                    }
                } else {
                    parseDirectoryEquipData(curPath + path.getFileName() + "/");
                }
            }
        } catch (IOException e1) {
            Console.WriteLine("Unable to read folder " + folder.toAbsolutePath() + ".");
            // TODO Auto-generated catch block
            e1.printStackTrace();
        }
    }

    public static void main(string[] args) {
        Instant instantStarted = Instant.now();
        parseDirectoryEquipData("");
        Instant instantStopped = Instant.now();
        Duration durationBetween = Duration.between(instantStarted, instantStopped);
        Console.WriteLine("Get elapsed time in milliseconds: " + durationBetween.toMillis());
        Console.WriteLine("Get elapsed time in seconds: " + durationBetween.toSeconds());
    }
}
