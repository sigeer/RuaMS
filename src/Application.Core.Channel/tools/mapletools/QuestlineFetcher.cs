

using provider.wz;

namespace tools.mapletools;























/**
 * @author RonanLana
 * <p>
 * This application parses the Quest.wz file inputted and generates a report showing
 * all cases where quest script files have not been found for quests that requires a
 * script file.
 * As an extension, it highlights missing script files for questlines that hand over
 * skills as rewards.
 * <p>
 * Running it should generate a report file under "output" folder with the search results.
 */
public class QuestlineFetcher {
    private static Path OUTPUT_FILE = ToolConstants.getOutputFile("questline_report.txt");
    private static string ACT_NAME = WZFiles.QUEST.getFilePath() + "/Act.img.xml";
    private static string CHECK_NAME = WZFiles.QUEST.getFilePath() + "/Check.img.xml";
    private static int INITIAL_STRING_LENGTH = 50;

    private static Stack<int> skillObtainableQuests = new Stack<>();
    private static HashSet<int> scriptedQuestFiles = new ();
    private static HashSet<int> expiredQuests = new ();
    private static Dictionary<int, List<int>> questDependencies = new ();
    private static HashSet<int> nonScriptedQuests = new ();
    private static HashSet<int> skillObtainableNonScriptedQuests = new ();

    private static PrintWriter printWriter = null;
    private static InputStreamReader fileReader = null;
    private static BufferedReader bufferedReader = null;
    private static byte status = 0;
    private static int questId = -1;
    private static int isCompleteState = 0;
    private static bool isScriptedQuest;
    private static bool isExpiredQuest;
    private static List<int> questDependencyList;
    private static int curQuestId;
    private static int curQuestState;

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

    private static void translateTokenCheck(string token) {
        string d;

        if (token.Contains("/imgdir")) {
            status -= 1;

            if (status == 1) {
                evaluateCurrentQuest();
            } else if (status == 4) {
                evaluateCurrentQuestDependency();
            }
        } else if (token.Contains("imgdir")) {
            if (status == 1) {           //getting QuestId
                d = getName(token);
                questId = int.Parse(d);

                isScriptedQuest = false;
                isExpiredQuest = false;
                questDependencyList = new ();
            } else if (status == 2) {      //start/complete
                d = getName(token);
                isCompleteState = int.Parse(d);
            } else if (status == 3) {
                if (isCompleteState == 1 || !token.Contains("quest")) {
                    forwardCursor(status);
                }
            }

            status += 1;
        } else {
            if (status == 3) {
                d = getName(token);

                if (d.Contains("script")) {
                    isScriptedQuest = true;
                } else if (d.Contains("end")) {
                    isExpiredQuest = true;
                }
            } else if (status == 5) {
                readQuestLabel(token);
            }
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
                if (isCompleteState == 1 && token.Contains("skill")) {
                    skillObtainableQuests.Add(questId);
                }

                forwardCursor(status);
            }

            status += 1;
        }
    }

    private static void readQuestLabel(string token) {
        string name = getName(token);
        string value = getValue(token);

        switch (name) {
            case "id" -> curQuestId = int.Parse(value);
            case "state" -> curQuestState = int.Parse(value);
        }
    }

    private static void evaluateCurrentQuestDependency() {
        if (curQuestState == 2) {
            questDependencyList.Add(curQuestId);
        }
    }

    private static void evaluateCurrentQuest() {
        if (isScriptedQuest && !scriptedQuestFiles.Contains(questId)) {
            nonScriptedQuests.Add(questId);
        }
        if (isExpiredQuest) {
            expiredQuests.Add(questId);
        }

        questDependencies.Add(questId, questDependencyList);
    }

    private static void instantiateQuestScriptFiles(string directoryName) {
        File directory = new File(directoryName);

        // get all the files from a directory
        File[] fList = directory.listFiles();
        foreach(File file in fList) {
            if (file.isFile()) {
                string fname = file.getName();

                try {
                    int questid = int.Parse(fname.Substring(0, fname.indexOf('.')));
                    scriptedQuestFiles.Add(questid);
                } catch (NumberFormatException nfe) {
                }
            }
        }
    }

    private static void readQuestsWithMissingScripts() throws IOException {
        string line;

        fileReader = new InputStreamReader(new FileInputStream(CHECK_NAME), StandardCharsets.UTF_8);
        bufferedReader = new BufferedReader(fileReader);

        while ((line = bufferedReader.readLine()) != null) {
            translateTokenCheck(line);
        }

        bufferedReader.close();
        fileReader.close();
    }

    private static void readQuestsWithSkillReward() throws IOException {
        string line;

        fileReader = new InputStreamReader(new FileInputStream(ACT_NAME), StandardCharsets.UTF_8);
        bufferedReader = new BufferedReader(fileReader);

        while ((line = bufferedReader.readLine()) != null) {
            translateTokenAct(line);
        }

        bufferedReader.close();
        fileReader.close();
    }

    private static void calculateSkillRelatedMissingQuestScripts() {
        Stack<int> frontierQuests = skillObtainableQuests;
        HashSet<int> solvedQuests = new ();

        while (frontierQuests.Count > 0) {
            int questid = frontierQuests.pop();
            solvedQuests.Add(questid);

            if (nonScriptedQuests.Contains(questid)) {
                skillObtainableNonScriptedQuests.Add(questid);
                nonScriptedQuests.Remove(questid);
            }

            List<int> questDependency = questDependencies.get(questid);
            foreach(int i in questDependency) {
                if (!solvedQuests.Contains(i)) {
                    frontierQuests.Add(i);
                }
            }
        }
    }

    private static void printReportFileHeader() {
        printWriter.println(" # Report File autogenerated from the MapleQuestlineFetcher feature by Ronan Lana.");
        printWriter.println(" # Generated data takes into account several data info from the server-side WZ.xmls.");
        printWriter.println();
    }

    private static List<int> getSortedListEntries(HashSet<int> set) {
        List<int> list = new (set);
        Collections.sort(list);

        return list;
    }

    private static void printReportFileResults() {
        if (skillObtainableNonScriptedQuests.Count > 0) {
            printWriter.println("SKILL-RELATED NON-SCRIPTED QUESTS");
            foreach(int nsq in getSortedListEntries(skillObtainableNonScriptedQuests)) {
                printWriter.println("  " + nsq + (expiredQuests.Contains(nsq) ? " EXPIRED" : ""));
            }

            printWriter.println();
        }

        printWriter.println("\nCOMMON NON-SCRIPTED QUESTS");
        foreach(int nsq in getSortedListEntries(nonScriptedQuests)) {
            printWriter.println("  " + nsq + (expiredQuests.Contains(nsq) ? " EXPIRED" : ""));
        }
    }

    private static void reportQuestlineData() {
        // This will reference one line at a time

        try (PrintWriter pw = new PrintWriter(Files.newOutputStream(OUTPUT_FILE))) {
            Console.WriteLine("Reading quest scripts...");
            instantiateQuestScriptFiles(ToolConstants.SCRIPTS_PATH + "/quest");

            Console.WriteLine("Reading WZs...");
            readQuestsWithSkillReward();
            readQuestsWithMissingScripts();

            Console.WriteLine("Calculating skill related quests...");
            calculateSkillRelatedMissingQuestScripts();

            Console.WriteLine("Reporting results...");
            printWriter = pw;

            printReportFileHeader();
            printReportFileResults();

            Console.WriteLine("Done!");
        } catch (FileNotFoundException ex) {
            Console.WriteLine("Unable to open quest file.");
        } catch (IOException ex) {
            Console.WriteLine("Error reading quest file.");
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    /*
    private static List<Pair<int, List<int>>> getSortedMapEntries(Dictionary<int, List<int>> map) {
        List<Pair<int, List<int>>> list = new (map.Count);
        foreach(Map.Entry<int, List<int>> e in map) {
            List<int> il = new (2);
            foreach(int i in e.getValue()) {
                il.Add(i);
            }

            Collections.sort(il, new Comparator<int>() {
                public override int compare(int o1, int o2) {
                    return o1 - o2;
                }
            });

            list.Add(new (e.Key, il));
        }

        Collections.sort(list, new Comparator<Pair<int, List<int>>>() {
            public override int compare(Pair<int, List<int>> o1, Pair<int, List<int>> o2) {
                return o1.getLeft() - o2.getLeft();
            }
        });

        return list;
    }

    private static void DumpQuestlineData() {
        foreach(Pair<int, List<int>> questDependency in getSortedMapEntries(questDependencies)) {
            if(questDependency.right.Count > 0) {
                Console.WriteLine(questDependency);
            }
        }
    }
    */

    public static void main(string[] args) {
    	Instant instantStarted = Instant.now();
        reportQuestlineData();
        Instant instantStopped = Instant.now();
        Duration durationBetween = Duration.between(instantStarted, instantStopped);
        Console.WriteLine("Get elapsed time in milliseconds: " + durationBetween.toMillis());
        Console.WriteLine("Get elapsed time in seconds: " + durationBetween.toSeconds());

    }
}

