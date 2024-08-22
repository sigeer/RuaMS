

using provider.wz;
using tools;

namespace tools.mapletools;
















/**
 * @author RonanLana
 * <p>
 * This application parses the Quest.wz file inputted and generates a report showing
 * all cases where a quest requires an item, but doesn't take them, which may happen
 * because the node representing the item doesn't have a "count" clause.
 * <p>
 * Running it should generate a report file under "output" folder with the search results.
 */
public class QuestItemCountFetcher {
    private static Path OUTPUT_FILE = ToolConstants.getOutputFile("quest_item_count_report.txt");
    private static string ACT_NAME = WZFiles.QUEST.getFilePath() + "/Act.img.xml";
    private static string CHECK_NAME = WZFiles.QUEST.getFilePath() + "/Check.img.xml";
    private static int INITIAL_STRING_LENGTH = 50;

    private static Dictionary<int, Dictionary<int, int>> checkItems = new ();
    private static Dictionary<int, Dictionary<int, int>> actItems = new ();

    private static PrintWriter printWriter = null;
    private static BufferedReader bufferedReader = null;
    private static byte status = 0;
    private static int questId = -1;
    private static int isCompleteState = 0;
    private static int curItemId;
    private static int curItemCount;

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

    private static void readItemLabel(string token) {
        string name = getName(token);
        string value = getValue(token);

        switch (name) {
            case "id" -> curItemId = int.Parse(value);
            case "count" -> curItemCount = int.Parse(value);
        }
    }

    private static void commitQuestItemPair(Dictionary<int, Dictionary<int, int>> map) {
        Dictionary<int, int> list = map.get(questId);
        if (list == null) {
            list = new ();
            map.Add(questId, list);
        }

        list.Add(curItemId, curItemCount);
    }

    private static void translateTokenAct(string token) {
        string d;

        if (token.Contains("/imgdir")) {
            status -= 1;

            if (status == 4) {
                if (curItemCount == int.MaxValue && isCompleteState == 1) {
                    commitQuestItemPair(actItems);
                }
            }
        } else if (token.Contains("imgdir")) {
            if (status == 1) {           //getting QuestId
                d = getName(token);
                questId = int.Parse(d);
            } else if (status == 2) {      //start/complete
                d = getName(token);
                isCompleteState = int.Parse(d);
            } else if (status == 3) {
                if (!token.Contains("item")) {
                    forwardCursor(status);
                }
            } else if (status == 4) {
                curItemId = int.MaxValue;
                curItemCount = int.MaxValue;
            }

            status += 1;
        } else {
            if (status == 5) {
                readItemLabel(token);
            }
        }
    }

    private static void translateTokenCheck(string token) {
        string d;

        if (token.Contains("/imgdir")) {
            status -= 1;

            if (status == 4) {
                Dictionary<int, int> missedItems = actItems.get(questId);

                if (missedItems != null && missedItems.ContainsKey(curItemId) && isCompleteState == 1) {
                    commitQuestItemPair(checkItems);
                }
            }
        } else if (token.Contains("imgdir")) {
            if (status == 1) {           //getting QuestId
                d = getName(token);
                questId = int.Parse(d);
            } else if (status == 2) {      //start/complete
                d = getName(token);
                isCompleteState = int.Parse(d);
            } else if (status == 3) {
                if (!token.Contains("item")) {
                    forwardCursor(status);
                }
            } else if (status == 4) {
                curItemId = int.MaxValue;
                curItemCount = int.MaxValue;
            }

            status += 1;
        } else {
            if (status == 5) {
                readItemLabel(token);
            }
        }
    }

    private static void readQuestItemCountData() throws IOException {
        string line;

        InputStreamReader fileReader = new InputStreamReader(new FileInputStream(ACT_NAME), StandardCharsets.UTF_8);
        bufferedReader = new BufferedReader(fileReader);

        while ((line = bufferedReader.readLine()) != null) {
            translateTokenAct(line);
        }

        bufferedReader.close();
        fileReader.close();

        fileReader = new InputStreamReader(new FileInputStream(CHECK_NAME), StandardCharsets.UTF_8);
        bufferedReader = new BufferedReader(fileReader);

        while ((line = bufferedReader.readLine()) != null) {
            translateTokenCheck(line);
        }

        bufferedReader.close();
        fileReader.close();
    }

    private static void printReportFileHeader() {
        printWriter.println(" # Report File autogenerated from the MapleQuestItemCountFetcher feature by Ronan Lana.");
        printWriter.println(" # Generated data takes into account several data info from the server-side WZ.xmls.");
        printWriter.println();
    }

    private static void printReportFileResults() {
        List<Pair<int, Pair<int, int>>> reports = new ();
        List<Pair<int, int>> notChecked = new ();

        foreach(Map.Entry<int, Dictionary<int, int>> actItem in actItems) {
            int questid = actItem.Key;

            foreach(Map.Entry<int, int> actData in actItem.getValue()) {
                int itemid = actData.Key;

                Dictionary<int, int> checkData = checkItems.get(questid);
                if (checkData != null) {
                    int count = checkData.get(itemid);
                    if (count != null) {
                        reports.Add(new (questid, new (itemid, -count)));
                    }
                } else {
                    notChecked.Add(new (questid, itemid));
                }
            }
        }

        foreach(Pair<int, Pair<int, int>> r in reports) {
            printWriter.println("Questid " + r.left + " : Itemid " + r.right.left + " should have qty " + r.right.right);
        }

        foreach(Pair<int, int> r in notChecked) {
            printWriter.println("Questid " + r.left + " : Itemid " + r.right + " is unchecked");
        }
    }

    private static void reportQuestItemCountData() {
        // This will reference one line at a time

        try (PrintWriter pw = new PrintWriter(Files.newOutputStream(OUTPUT_FILE))) {
            Console.WriteLine("Reading WZs...");
            readQuestItemCountData();

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

    public static void main(string[] args) {
        reportQuestItemCountData();
    }
}
