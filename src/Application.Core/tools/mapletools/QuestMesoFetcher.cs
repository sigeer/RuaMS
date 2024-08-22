

using provider.wz;

namespace tools.mapletools;














/**
 * @author RonanLana
 * <p>
 * This application parses the Quest.wz file inputted and generates a report showing
 * all cases where a quest takes a meso fee to complete a quest, but it doesn't
 * properly checks the player for the needed amount before completing it.
 * <p>
 * Running it should generate a report file under "output" folder with the search results.
 */
public class QuestMesoFetcher {
    private static Path OUTPUT_FILE = ToolConstants.getOutputFile("quest_meso_report.txt");
    private static bool PRINT_FEES = true;    // print missing values as additional info report
    private static int INITIAL_STRING_LENGTH = 50;

    private static Dictionary<int, int> checkedMesoQuests = new ();
    private static Dictionary<int, int> appliedMesoQuests = new ();
    private static HashSet<int> checkedEndscriptQuests = new ();

    private static PrintWriter printWriter = null;
    private static BufferedReader bufferedReader = null;
    private static byte status = 0;
    private static int questId = -1;
    private static int isCompleteState = 0;
    private static int currentMeso = 0;

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

    private static void translateTokenAct(string token) {
        string d;

        if (token.Contains("/imgdir")) {
            status -= 1;
        } else if (token.Contains("imgdir")) {
            if (status == 1) {           //getting QuestId
                d = getName(token);
                questId = int.Parse(d);
            } else if (status == 2) {      //start/complete
                d = getName(token);
                isCompleteState = int.Parse(d);
            } else if (status == 3) {
                forwardCursor(status);
            }

            status += 1;
        } else {
            if (token.Contains("money")) {
                if (isCompleteState != 0) {
                    d = getValue(token);

                    currentMeso = -1 * int.Parse(d);

                    if (currentMeso > 0) {
                        appliedMesoQuests.Add(questId, currentMeso);
                    }
                }
            }
        }
    }

    private static void translateTokenCheck(string token) {
        string d;

        if (token.Contains("/imgdir")) {
            status -= 1;
        } else if (token.Contains("imgdir")) {
            if (status == 1) {           //getting QuestId
                d = getName(token);
                questId = int.Parse(d);
            } else if (status == 2) {      //start/complete
                d = getName(token);
                isCompleteState = int.Parse(d);
            } else if (status == 3) {
                forwardCursor(status);
            }

            status += 1;
        } else {
            if (token.Contains("endmeso")) {
                d = getValue(token);
                currentMeso = int.Parse(d);

                checkedMesoQuests.Add(questId, currentMeso);
            } else if (token.Contains("endscript")) {
                checkedEndscriptQuests.Add(questId);
            }
        }
    }

    private static void readQuestMesoData() throws IOException {
        string line;

        bufferedReader = Files.newBufferedReader(WZFiles.QUEST.getFile().resolve("Act.img.xml"));

        while ((line = bufferedReader.readLine()) != null) {
            translateTokenAct(line);
        }

        bufferedReader.close();

        bufferedReader = Files.newBufferedReader(WZFiles.QUEST.getFile().resolve("Check.img.xml"));

        while ((line = bufferedReader.readLine()) != null) {
            translateTokenCheck(line);
        }

        bufferedReader.close();
    }

    private static void printReportFileHeader() {
        printWriter.println(" # Report File autogenerated from the MapleQuestMesoFetcher feature by Ronan Lana.");
        printWriter.println(" # Generated data takes into account several data info from the server-side WZ.xmls.");
        printWriter.println();
    }

    private static void printReportFileResults(Dictionary<int, int> target, Dictionary<int, int> base, bool testingCheck) {
        List<int> result = new ();
        List<int> error = new ();

        Dictionary<int, int> questFee = new ();

        foreach(Map.Entry<int, int> e in base) {
            int v = target.get(e.Key);

            if (v == null) {
                if (testingCheck || !checkedEndscriptQuests.Contains(e.Key)) {
                    result.Add(e.Key);
                    questFee.Add(e.Key, e.getValue());
                }
            } else if (v != e.getValue()) {
                error.Add(e.Key);
            }
        }

        if (result.Count > 0 || error.Count > 0) {
            printWriter.println("MISMATCH INFORMATION ON '" + (testingCheck ? "check" : "act") + "':");
            if (result.Count > 0) {
                result.sort((o1, o2) -> o1 - o2);

                printWriter.println("# MISSING");

                if (!PRINT_FEES) {
                    foreach(int i in result) {
                        printWriter.println(i);
                    }
                } else {
                    foreach(int i in result) {
                        printWriter.println(i + " " + questFee.get(i));
                    }
                }

                printWriter.println();
            }

            if (error.Count > 0 && testingCheck) {
                error.sort((o1, o2) -> o1 - o2);

                printWriter.println("# WRONG VALUE");

                foreach(int i in error) {
                    printWriter.println(i);
                }

                printWriter.println();
            }

            printWriter.println("\r\n");
        }
    }

    private static void reportQuestMesoData() {
        // This will reference one line at a time

        try (PrintWriter pw = new PrintWriter(Files.newOutputStream(OUTPUT_FILE))) {
            Console.WriteLine("Reading WZs...");
            readQuestMesoData();

            Console.WriteLine("Reporting results...");
            // report missing meso checks on quest completes
            printWriter = pw;

            printReportFileHeader();

            printReportFileResults(checkedMesoQuests, appliedMesoQuests, true);
            printReportFileResults(appliedMesoQuests, checkedMesoQuests, false);

            Console.WriteLine("Done!");
        } catch (FileNotFoundException ex) {
            Console.WriteLine("Unable to open quest file.");
        } catch (IOException ex) {
            Console.WriteLine("Error reading quest file.");
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    public static void main(string[] args) {
        reportQuestMesoData();
    }
}

