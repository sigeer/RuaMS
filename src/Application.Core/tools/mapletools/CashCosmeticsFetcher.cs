

using server;
using tools;

namespace tools.mapletools;












/**
 * @author RonanLana
 * <p>
 * This application gathers info from the WZ.XML files, fetching all cosmetic coupons and tickets from there, and then
 * searches the NPC script files, identifying the stylish NPCs that supposedly uses them. It will reports all NPCs that
 * uses up a card, as well as report those currently unused.
 * <p>
 * Estimated parse time: 10 seconds
 */
public class CashCosmeticsFetcher {
    private static Dictionary<int, string> scriptEntries = new (500);

    private static void listFiles(string directoryName, <File> files) {
        File directory = new File(directoryName);

        // get all the files from a directory
        File[] fList = directory.listFiles();
        foreach(File file in fList) {
            if (file.isFile()) {
                files.Add(file);
            } else if (file.isDirectory()) {
                listFiles(file.getAbsolutePath(), files);
            }
        }
    }

    private static int getNpcIdFromFilename(string name) {
        try {
            return int.Parse(name.Substring(0, name.indexOf('.')));
        } catch (Exception e) {
            return -1;
        }
    }

    private static void loadScripts() throws Exception {
        <File> files = new ();
        listFiles(ToolConstants.SCRIPTS_PATH + "/npc", files);

        foreach(File f in files) {
            int npcid = getNpcIdFromFilename(f.getName());

            //Console.WriteLine("Parsing " + f.getAbsolutePath());
            InputStreamReader fileReader = new InputStreamReader(new FileInputStream(f), StandardCharsets.UTF_8);
            BufferedReader bufferedReader = new BufferedReader(fileReader);

            StringBuilder stringBuffer = new StringBuilder();
            string line;

            while ((line = bufferedReader.readLine()) != null) {
                stringBuffer.Append(line).Append("\n");
            }

            scriptEntries.Add(npcid, stringBuffer.ToString());

            bufferedReader.close();
            fileReader.close();
        }
    }

    private static List<int> findItemidOnScript(int itemid) {
        List<int> files = new ();
        string t = string.valueOf(itemid);

        foreach(Map.Entry<int, string> text in scriptEntries) {
            if (text.getValue().Contains(t)) {
                files.Add(text.Key);
            }
        }

        return files;
    }

    private static void reportCosmeticCouponResults() {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        for (int itemid = 5150000; itemid <= 5154000; itemid++) {
            string itemName = ii.getName(itemid);

            if (itemName != null) {
                List<int> npcids = findItemidOnScript(itemid);

                if (npcids.Count > 0) {
                    Console.WriteLine("Itemid " + itemid + " found on " + npcids + ". (" + itemName + ")");
                } else {
                    Console.WriteLine("NOT FOUND ITEMID " + itemid + " (" + itemName + ")");
                }
            }
        }
    }

    public static void main(string[] args) {
        DatabaseConnection.initializeConnectionPool(); // ItemInformationProvider loads unrelated stuff from the db
        try {
            loadScripts();
            Console.WriteLine("Loaded scripts");

            reportCosmeticCouponResults();
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }
}
