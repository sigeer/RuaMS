

using provider.wz;
using server;
using tools;
using tools;

namespace tools.mapletools;




















/**
 * @author RonanLana
 * <p>
 * This application has 2 objectives: fetch missing drop data relevant to quests,
 * and update the questid from items that are labeled as "Quest Item" on the DB.
 * <p>
 * Running it should generate a report file under "output" folder with the search results.
 * <p>
 * Estimated parse time: 1 minute
 */
public class QuestItemFetcher {
    private static Path OUTPUT_FILE = ToolConstants.getOutputFile("quest_report.txt");
    private static Collection<string> RELEVANT_FILE_EXTENSIONS = Set.of(".sql", ".js", ".txt", ".java");
    private static int INITIAL_STRING_LENGTH = 50;
    private static int INITIAL_LENGTH = 200;
    private static bool DISPLAY_EXTRA_INFO = true;     // display items with zero quantity over the quest act WZ

    private static Connection con = SimpleDatabaseConnection.getConnection();
    private static Dictionary<int, HashSet<int>> startQuestItems = new (INITIAL_LENGTH);
    private static Dictionary<int, HashSet<int>> completeQuestItems = new (INITIAL_LENGTH);
    private static Dictionary<int, HashSet<int>> zeroedStartQuestItems = new ();
    private static Dictionary<int, HashSet<int>> zeroedCompleteQuestItems = new ();
    private static Dictionary<int, int[]> mixedQuestidItems = new ();
    private static HashSet<int> limitedQuestids = new ();

    private static ItemInformationProvider ii;
    private static PrintWriter printWriter = null;
    private static BufferedReader bufferedReader = null;
    private static byte status = 0;
    private static int questId = -1;
    private static int isCompleteState = 0;
    private static int currentItemid = 0;
    private static int currentCount = 0;

    private static string getName(string token) {
        int i, j;
        char[] dest;
        string d;

        i = token.lastIndexOf("name");
        i = token.indexOf("\"", i) + 1; //lower bound of the string
        j = token.indexOf("\"", i);     //upper bound

        if (j < i) {
            return "0";           //node value containing 'name' in it's scope, cheap fix since we don't deal with strings anyway
        }

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

    private static void inspectQuestItemList(int st) {
        string line = null;

        try {
            while (status >= st && (line = bufferedReader.readLine()) != null) {
                readItemToken(line);
            }
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void processCurrentItem() {
        try {
            if (ii.isQuestItem(currentItemid)) {
                if (currentCount != 0) {
                    if (isCompleteState == 1) {
                        if (currentCount < 0) {
                            HashSet<int> qi = completeQuestItems.get(questId);
                            if (qi == null) {
                                HashSet<int> newSet = new ();
                                newSet.Add(currentItemid);

                                completeQuestItems.Add(questId, newSet);
                            } else {
                                qi.Add(currentItemid);
                            }
                        }
                    } else {
                        if (currentCount > 0) {
                            HashSet<int> qi = startQuestItems.get(questId);
                            if (qi == null) {
                                HashSet<int> newSet = new ();
                                newSet.Add(currentItemid);

                                startQuestItems.Add(questId, newSet);
                            } else {
                                qi.Add(currentItemid);
                            }
                        }
                    }
                } else {
                    if (isCompleteState == 1) {
                        HashSet<int> qi = zeroedCompleteQuestItems.get(questId);
                        if (qi == null) {
                            HashSet<int> newSet = new ();
                            newSet.Add(currentItemid);

                            zeroedCompleteQuestItems.Add(questId, newSet);
                        } else {
                            qi.Add(currentItemid);
                        }
                    } else {
                        HashSet<int> qi = zeroedStartQuestItems.get(questId);
                        if (qi == null) {
                            HashSet<int> newSet = new ();
                            newSet.Add(currentItemid);

                            zeroedStartQuestItems.Add(questId, newSet);
                        } else {
                            qi.Add(currentItemid);
                        }
                    }
                }
            }
        } catch (Exception e) {
        }
    }

    private static void readItemToken(string token) {
        if (token.Contains("/imgdir")) {
            status -= 1;

            processCurrentItem();

            currentItemid = 0;
            currentCount = 0;
        } else if (token.Contains("imgdir")) {
            status += 1;
        } else {
            string d = getName(token);

            if (d.Equals("id")) {
                currentItemid = int.Parse(getValue(token));
            } else if (d.Equals("count")) {
                currentCount = int.Parse(getValue(token));
            }
        }
    }

    private static void translateActToken(string token) {
        string d;
        int temp;

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
                d = getName(token);

                if (d.Contains("item")) {
                    temp = status;
                    inspectQuestItemList(temp);
                } else {
                    forwardCursor(status);
                }
            }

            status += 1;
        } else {
            if (status == 3) {
                d = getName(token);

                if (d.Equals("end")) {
                    limitedQuestids.Add(questId);
                }
            }
        }
    }

    private static void translateCheckToken(string token) {
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
            if (status == 3) {
                d = getName(token);

                if (d.Equals("end")) {
                    limitedQuestids.Add(questId);
                }
            }
        }
    }

    private static void calculateQuestItemDiff() {
        // This will remove started quest items from the "to complete" item set.

        foreach(Map.Entry<int, HashSet<int>> qd in startQuestItems) {
            foreach(int qi in qd.getValue()) {
                HashSet<int> questSet = completeQuestItems.get(qd.Key);

                if (questSet != null) {
                    if (questSet.Remove(qi)) {
                        if (completeQuestItems.Count == 0) {
                            completeQuestItems.Remove(qd.Key);
                        }
                    }
                }
            }
        }
    }

    private static List<Pair<int, int>> getPairsQuestItem() {   // quest items not gained at WZ's quest start
        List<Pair<int, int>> list = new (INITIAL_LENGTH);

        foreach(Map.Entry<int, HashSet<int>> qd in completeQuestItems) {
            foreach(int qi in qd.getValue()) {
                list.Add(new (qi, qd.Key));
            }
        }

        return list;
    }

    private static string getTableName(bool dropdata) {
        return dropdata ? "drop_data" : "reactordrops";
    }

    private static void filterQuestDropsOnTable(Pair<int, int> iq, List<Pair<int, int>> itemsWithQuest, bool dropdata) {
        PreparedStatement ps = con.prepareStatement("SELECT questid FROM " + getTableName(dropdata) + " WHERE itemid = ?;");
        ps.setInt(1, iq.getLeft());
        ResultSet rs = ps.executeQuery();

        if (rs.isBeforeFirst()) {
            while (rs.next()) {
                int curQuest = rs.getInt(1);
                if (curQuest != iq.getRight()) {
                    HashSet<int> sqSet = startQuestItems.get(curQuest);
                    if (sqSet != null && sqSet.Contains(iq.getLeft())) {
                        continue;
                    }

                    int[] mixed = new int[3];
                    mixed[0] = iq.getLeft();
                    mixed[1] = curQuest;
                    mixed[2] = iq.getRight();

                    mixedQuestidItems.Add(iq.getLeft(), mixed);
                }
            }

            itemsWithQuest.Remove(iq);
        }

        rs.close();
        ps.close();
    }

    private static void filterQuestDropsOnDB(List<Pair<int, int>> itemsWithQuest) {
        List<Pair<int, int>> copyItemsWithQuest = new (itemsWithQuest);
        try {
            foreach(Pair<int, int> iq in copyItemsWithQuest) {
                filterQuestDropsOnTable(iq, itemsWithQuest, true);
                filterQuestDropsOnTable(iq, itemsWithQuest, false);
            }
        } catch (SQLException e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void filterDirectorySearchMatchingData(string filePath, List<Pair<int, int>> itemsWithQuest) {
        try {
            Files.walk(Path.of(filePath))
                    .filter(QuestItemFetcher::isRelevantFile)
                    .forEach(path -> fileSearchMatchingData(path, itemsWithQuest));
        } catch (IOException e) {
            throw new RuntimeException("Error during recursive file walk", e);
        }
    }

    private static bool isRelevantFile(Path file) {
        string fileName = file.getFileName().ToString();
        return RELEVANT_FILE_EXTENSIONS.stream().anyMatch(fileName::endsWith);
    }

    private static bool foundMatchingDataOnFile(string fileContent, string searchStr) {
        return fileContent.Contains(searchStr);
    }

    private static void fileSearchMatchingData(Path file, List<Pair<int, int>> itemsWithQuest) {
        try {
            string fileContent = Files.readString(file);

            List<Pair<int, int>> copyItemsWithQuest = new (itemsWithQuest);
            foreach(Pair<int, int> iq in copyItemsWithQuest) {
                if (foundMatchingDataOnFile(fileContent, string.valueOf(iq.getLeft()))) {
                    itemsWithQuest.Remove(iq);
                }
            }
        } catch (IOException ioe) {
            Console.WriteLine("Failed to read file: " + file.getFileName().toAbsolutePath().ToString());
            ioLog.Logger.Error(e.ToString());
        }
    }

    private static void printReportFileHeader() {
        printWriter.println(" # Report File autogenerated from the MapleQuestItemFetcher feature by Ronan Lana.");
        printWriter.println(" # Generated data takes into account several data info from the underlying DB, server source files and the server-side WZ.xmls.");
        printWriter.println();
    }

    private static List<Map.Entry<int, int>> getSortedMapEntries0(Dictionary<int, int> map) {
        List<Map.Entry<int, int>> list = new (map.Count);
        list.addAll(map);

        list.sort(Comparator.comparingInt(Map.Entry::getKey));

        return list;
    }

    private static List<Map.Entry<int, int[]>> getSortedMapEntries1(Dictionary<int, int[]> map) {
        List<Map.Entry<int, int[]>> list = new (map.Count);
        list.addAll(map);

        list.sort(Comparator.comparingInt(Map.Entry::getKey));

        return list;
    }

    private static List<Pair<int, List<int>>> getSortedMapEntries2(Dictionary<int, HashSet<int>> map) {
        List<Pair<int, List<int>>> list = new (map.Count);
        foreach(Map.Entry<int, HashSet<int>> e in map) {
            List<int> il = new (2);
            il.addAll(e.getValue());

            il.sort(Comparator.comparingInt(o -> o));

            list.Add(new (e.Key, il));
        }

        list.sort(Comparator.comparingInt(Pair::getLeft));

        return list;
    }

    private static string getExpiredStringLabel(int questid) {
        return (!limitedQuestids.Contains(questid) ? "" : " EXPIRED");
    }

    private static void reportQuestItemData() {
        // This will reference one line at a time
        string line = null;
        Path file = null;

        try (PrintWriter pw = new PrintWriter(Files.newOutputStream(OUTPUT_FILE))) {
            Console.WriteLine("Reading WZs...");

            file = WZFiles.QUEST.getFile().resolve("Check.img.xml");
            bufferedReader = Files.newBufferedReader(file);

            while ((line = bufferedReader.readLine()) != null) {
                translateCheckToken(line); // fetch expired quests through here as well
            }

            bufferedReader.close();

            file = WZFiles.QUEST.getFile().resolve("Act.img.xml");
            bufferedReader = Files.newBufferedReader(file);

            while ((line = bufferedReader.readLine()) != null) {
                translateActToken(line);
            }

            bufferedReader.close();

            Console.WriteLine("Calculating table diffs...");
            calculateQuestItemDiff();

            Console.WriteLine("Filtering drops on DB...");
            List<Pair<int, int>> itemsWithQuest = getPairsQuestItem();

            filterQuestDropsOnDB(itemsWithQuest);
            con.close();

            Console.WriteLine("Filtering drops on project files...");
            // finally, filter whether this item is mentioned on the source code or not.
            filterDirectorySearchMatchingData("scripts", itemsWithQuest);
            filterDirectorySearchMatchingData("database/sql", itemsWithQuest);
            filterDirectorySearchMatchingData("src", itemsWithQuest);

            Console.WriteLine("Reporting results...");
            // report suspects of missing quest drop data, as well as those drop data that
            // may have incorrect questids.
            printWriter = pw;

            printReportFileHeader();

            if (mixedQuestidItems.Count > 0) {
                printWriter.println("INCORRECT QUESTIDS ON DB");
                foreach(Map.Entry<int, int[]> emqi in getSortedMapEntries1(mixedQuestidItems)) {
                    int[] mqi = emqi.getValue();
                    printWriter.println(mqi[0] + " : " + mqi[1] + " -> " + mqi[2] + getExpiredStringLabel(mqi[2]));
                }
                printWriter.println("\n\n\n\n\n");
            }

            if (itemsWithQuest.Count > 0) {
                Dictionary<int, int> mapIwq = new (itemsWithQuest.Count);
                foreach(Pair<int, int> iwq in itemsWithQuest) {
                    mapIwq.Add(iwq.getLeft(), iwq.getRight());
                }

                printWriter.println("ITEMS WITH NO QUEST DROP DATA ON DB");
                foreach(Map.Entry<int, int> iwq in getSortedMapEntries0(mapIwq)) {
                    printWriter.println(iwq.Key + " - " + iwq.getValue() + getExpiredStringLabel(iwq.getValue()));
                }
                printWriter.println("\n\n\n\n\n");
            }

            if (DISPLAY_EXTRA_INFO) {
                if (zeroedStartQuestItems.Count > 0) {
                    printWriter.println("START QUEST ITEMS WITH ZERO QUANTITY");
                    foreach(Pair<int, List<int>> iwq in getSortedMapEntries2(zeroedStartQuestItems)) {
                        printWriter.println(iwq.getLeft() + getExpiredStringLabel(iwq.getLeft()) + ":");
                        foreach(int i in iwq.getRight()) {
                            printWriter.println("  " + i);
                        }
                        printWriter.println();
                    }
                    printWriter.println("\n\n\n\n\n");
                }

                if (zeroedCompleteQuestItems.Count > 0) {
                    printWriter.println("COMPLETE QUEST ITEMS WITH ZERO QUANTITY");
                    foreach(Pair<int, List<int>> iwq in getSortedMapEntries2(zeroedCompleteQuestItems)) {
                        printWriter.println(iwq.getLeft() + getExpiredStringLabel(iwq.getLeft()) + ":");
                        foreach(int i in iwq.getRight()) {
                            printWriter.println("  " + i);
                        }
                        printWriter.println();
                    }
                    printWriter.println("\n\n\n\n\n");
                }
            }

            Console.WriteLine("Done!");
        } catch (FileNotFoundException ex) {
            Console.WriteLine("Unable to open file '" + file + "'");
        } catch (IOException ex) {
            Console.WriteLine("Error reading file '" + file + "'");
        } catch (SQLException e) {
            Console.WriteLine("Warning: Could not establish connection to database to report quest data.");
            Console.WriteLine(e.getMessage());
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    public static void main(string[] args) {
        DatabaseConnection.initializeConnectionPool(); // ItemInformationProvider loads some unrelated db data
        ii = ItemInformationProvider.getInstance();

        reportQuestItemData();
    }
}

