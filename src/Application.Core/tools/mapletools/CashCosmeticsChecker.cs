

using provider.wz;
using tools;

namespace tools.mapletools;



















/**
 * @author RonanLana
 * <p>
 * This application parses the cosmetic recipes defined within "lib/care" folder, loads
 * every present cosmetic itemid from the XML data, then checks the scripts for missed
 * cosmetics within the stylist/surgeon. Results from the search are reported in a report
 * file.
 * <p>
 * Note: to best make use of this feature, set IGNORE_CURRENT_SCRIPT_COSMETICS = true. This
 * way, every available cosmetic present on the recipes will be listed on the report.
 * <p>
 * Estimated parse time: 1 minute
 */
public class CashCosmeticsChecker {
    private static string INPUT_DIRECTORY_PATH = ToolConstants.getInputFile("care").ToString();
    private static Path OUTPUT_FILE = ToolConstants.getOutputFile("cash_cosmetics_result.txt");
    private static bool IGNORE_CURRENT_SCRIPT_COSMETICS = false; // Toggle to preference
    private static int INITIAL_STRING_LENGTH = 50;

    private static Dictionary<int, HashSet<int>> scriptCosmetics = new ();
    private static Dictionary<int, string> scriptEntries = new (500);
    private static HashSet<int> allCosmetics = new ();
    private static HashSet<int> unusedCosmetics = new ();
    private static Dictionary<int, List<int>> usedCosmetics = new ();
    private static Dictionary<int, string> couponNames = new ();
    private static Dictionary<int, int> cosmeticNpcs = new (); // expected only 1 NPC per cosmetic coupon (town care/salon)
    private static Dictionary<List<string>, int> cosmeticNpcids = new ();
    private static HashSet<string> missingCosmeticNames = new ();
    private static Dictionary<string, int> cosmeticNameIds = new ();
    private static Dictionary<int, string> cosmeticIdNames = new ();
    private static Dictionary<Pair<int, string>, HashSet<int>> missingCosmeticsNpcTypes = new ();

    private static PrintWriter printWriter = null;
    private static InputStreamReader fileReader = null;
    private static BufferedReader bufferedReader = null;
    private static byte status = 0;

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
        if (token.Contains("/imgdir")) {
            status -= 1;
        } else if (token.Contains("imgdir")) {
            status += 1;

            if (status == 3) {
                string d = getName(token);

                if (!(d.contentEquals("Face") || d.contentEquals("Hair"))) {
                    forwardCursor(status);
                }
            } else if (status == 4) {
                string d = getName(token);
                int itemid = int.Parse(d);

                int cosmeticid;
                if (itemid >= 30000) {
                    cosmeticid = (itemid / 10) * 10;
                } else {
                    cosmeticid = itemid - ((itemid / 100) % 10) * 100;
                }

                allCosmetics.Add(cosmeticid);
                forwardCursor(status);
            }
        }
    }

    private static void readEqpStringData(string eqpStringDirectory) throws IOException {
        string line;

        fileReader = new InputStreamReader(new FileInputStream(eqpStringDirectory), StandardCharsets.UTF_8);
        bufferedReader = new BufferedReader(fileReader);

        while ((line = bufferedReader.readLine()) != null) {
            translateToken(line);
        }

        bufferedReader.close();
        fileReader.close();
    }

    private static void loadCosmeticWzData() throws IOException {
        Console.WriteLine("Reading string.wz ...");
        readEqpStringData(WZFiles.STRING.getFilePath() + "/Eqp.img.xml");
    }

    private static void setCosmeticUsage(List<int> usedByNpcids, int cosmeticid) {
        if (usedByNpcids.Count > 0) {
            usedCosmetics.Add(cosmeticid, usedByNpcids);
        } else {
            unusedCosmetics.Add(cosmeticid);
        }
    }

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

    private static List<int> findCosmeticDataNpcids(int itemid) {
        List<int> npcids = new ();
        foreach(Map.Entry<int, HashSet<int>> sc in scriptCosmetics) {
            if (sc.getValue().Contains(itemid)) {
                npcids.Add(itemid);
            }
        }

        return npcids;
    }

    private static void loadScripts() throws IOException {
        <File> files = new ();
        listFiles(ToolConstants.SCRIPTS_PATH + "/npc", files);

        foreach(File f in files) {
            int npcid = getNpcIdFromFilename(f.getName());

            //Console.WriteLine("Parsing " + f.getAbsolutePath());
            fileReader = new InputStreamReader(new FileInputStream(f), StandardCharsets.UTF_8);
            bufferedReader = new BufferedReader(fileReader);

            string line;

            StringBuilder stringBuffer = new StringBuilder();

            bool cosmeticNpc = false;
            HashSet<int> cosmeticids = new ();
            while ((line = bufferedReader.readLine()) != null) {
                string[] s = line.Split("hair_. = Array\\(", 2);

                if (s.Length > 1) {
                    cosmeticNpc = true;
                    s = s[1].Split("\\)", 2);
                    s = s[0].Split(", ");

                    foreach(string st in s) {
                        if (st.Count > 0) {
                            int itemid = int.Parse(st);
                            cosmeticids.Add(itemid);
                        }
                    }
                } else {
                    s = line.Split("face_. = Array\\(", 2);

                    if (s.Length > 1) {
                        cosmeticNpc = true;
                        s = s[1].Split("\\)", 2);
                        s = s[0].Split(", ");

                        foreach(string st in s) {
                            if (st.Count > 0) {
                                int itemid = int.Parse(st);
                                cosmeticids.Add(itemid);
                            }
                        }
                    }
                }

                stringBuffer.Append(line).Append("\n");
            }

            scriptEntries.Add(npcid, stringBuffer.ToString());

            if (cosmeticNpc) {
                scriptCosmetics.Add(npcid, cosmeticids);
            }

            bufferedReader.close();
            fileReader.close();
        }
    }

    private static void processCosmeticScriptData() throws IOException {
        Console.WriteLine("Reading script files ...");
        loadScripts();

        if (IGNORE_CURRENT_SCRIPT_COSMETICS) {
            foreach(HashSet<int> npcCosmetics in scriptCosmetics.values()) {
                npcCosmetics.Clear();
            }
        }

        foreach(int itemid in allCosmetics) {
            List<int> npcids = findCosmeticDataNpcids(itemid);
            setCosmeticUsage(npcids, itemid);
        }
    }

    private static List<int> loadCosmeticCouponids() throws IOException {
        List<int> couponItemids = new ();

        fileReader = new InputStreamReader(new FileInputStream(getHandbookFileName("/Cash.txt")), StandardCharsets.UTF_8);
        bufferedReader = new BufferedReader(fileReader);

        string line;
        while ((line = bufferedReader.readLine()) != null) {
            if (line.Count == 0) {
                continue;
            }
            string[] s = line.Split(" - ", 3);

            int itemid = int.Parse(s[0]);
            if (itemid >= 5150000 && itemid < 5160000) {
                couponItemids.Add(itemid);
                couponNames.Add(itemid, s[1]);
            }
        }

        bufferedReader.close();
        fileReader.close();

        return couponItemids;
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

    private static void loadCosmeticCouponNpcs() throws IOException {
        Console.WriteLine("Locating cosmetic NPCs ...");

        foreach(int itemid in loadCosmeticCouponids()) {
            List<int> npcids = findItemidOnScript(itemid);

            if (npcids.Count > 0) {
                cosmeticNpcs.Add(itemid, npcids.get(0));
            }
        }
    }

    private enum CosmeticType {
        HAIRSTYLE,
        HAIRCOLOR,
        DIRTYHAIR,
        FACE_SURGERY,
        EYE_COLOR,
        SKIN_CARE
    }

    private static Pair<int, CosmeticType> parseCosmeticCoupon(string[] tokens) {
        for (int i = 0; i < tokens.Length; i++) {
            string s = tokens[i];

            if (s.startsWith("Hair")) {
                if (s.contentEquals("Hairstyle")) {
                    return new (i, CosmeticType.HAIRSTYLE);
                } else {
                    if (i - 1 >= 0 && tokens[i - 1].contentEquals("Dirty")) {
                        return new (i - 1, CosmeticType.DIRTYHAIR);
                    } else if (i + 1 < tokens.Length && tokens[i + 1].contentEquals("Color")) {
                        return new (i, CosmeticType.HAIRCOLOR);
                    } else {
                        return new (i, CosmeticType.HAIRSTYLE);
                    }
                }
            } else if (s.startsWith("Face")) {
                return new (i, CosmeticType.FACE_SURGERY);
            } else if (s.startsWith("Cosmetic")) {
                return new (i, CosmeticType.EYE_COLOR);
            } else if (s.startsWith("Plastic")) {
                return new (i, CosmeticType.FACE_SURGERY);
            } else if (s.startsWith("Skin")) {
                return new (i, CosmeticType.SKIN_CARE);
            }
        }

        return null;
    }

    private static List<string> getCosmeticCouponData(string town, string type, string subtype) {
        List<string> ret = new (3);
        ret.Add(town);
        ret.Add(type);
        ret.Add(subtype);
        return ret;
    }

    private static List<string> parseCosmeticCoupon(string couponName) {
        string town, type, subtype = "EXP";

        string[] s = couponName.Split(" Coupon ", 2);

        if (s.Length > 1) {
            subtype = s[1].Substring(1, s[1].Length - 1);
        }

        string[] tokens = s[0].Split(" ");
        Pair<int, CosmeticType> cosmeticData = parseCosmeticCoupon(tokens);
        if (cosmeticData == null) {
            return null;
        }

        town = "";
        for (int i = 0; i < cosmeticData.left; i++) {
            town += (tokens[i] + "_");
        }
        town = town.Substring(0, town.Length - 1).ToLower();

        switch (cosmeticData.right) {
            case HAIRSTYLE:
                type = "hair";
                break;

            case FACE_SURGERY:
                type = "face";
                break;

            default:
                return null;
        }

        return getCosmeticCouponData(town, type, subtype);
    }

    private static void generateCosmeticPlaceNpcs() {
        foreach(Map.Entry<int, string> e in couponNames) {
            int npcid = cosmeticNpcs.get(e.Key);
            if (npcid == null) {
                continue;
            }

            string couponName = e.getValue();
            List<string> couponData = parseCosmeticCoupon(couponName);

            if (couponData == null) {
                continue;
            }
            cosmeticNpcids.Add(couponData, npcid);
        }
    }

    private static int getCosmeticNpcid(string townName, string typeCosmetic, string typeCoupon) {
        return cosmeticNpcids.get(getCosmeticCouponData(townName, typeCosmetic, typeCoupon));
    }

    private static string getCosmeticName(string name, bool gender) {
        string genderString = gender ? "F" : "M";
        return string.format("%s (%s)", name, genderString);
    }

    private static void loadCosmeticNames(string cosmeticPath) throws IOException {
        fileReader = new InputStreamReader(new FileInputStream(cosmeticPath), StandardCharsets.UTF_8);
        bufferedReader = new BufferedReader(fileReader);

        string line;
        while ((line = bufferedReader.readLine()) != null) {
            string[] s = line.Split(" - ", 3);
            int itemid = int.Parse(s[0]);

            string name;
            if (itemid < 30000) {
                itemid = itemid - ((itemid / 100) % 10) * 100;

                int idx = s[1].lastIndexOf(" ");
                if (idx > -1) {
                    name = s[1].Substring(0, idx);
                } else {
                    name = s[1];
                }
            } else {
                itemid = (s[0] / 10) * 10;

                int idx = s[1].indexOf(" ");
                if (idx > -1) {
                    name = s[1].Substring(idx + 1);
                } else {
                    name = s[1];
                }
            }

            name = name.trim();

            string cname = getCosmeticName(name, (((itemid / 1000) % 10) % 3) != 0);

            /*
            if (cosmeticNameIds.ContainsKey(cname) && Math.Abs(cosmeticNameIds.get(cname) - itemid) > 50) {
                Console.WriteLine("Clashing '" + name + "' " + itemid + "/" + cosmeticNameIds.get(cname));
            }
            */

            cosmeticNameIds.Add(cname, itemid);
            cosmeticIdNames.Add(itemid, name);
        }

        bufferedReader.close();
        fileReader.close();
    }

    private static void loadCosmeticNames() throws IOException {
        Console.WriteLine("Reading cosmetics from handbook ...");

        loadCosmeticNames(getHandbookFileName("/Equip/Face.txt"));
        loadCosmeticNames(getHandbookFileName("/Equip/Hair.txt"));
    }

    private static string getHandbookFileName(string fileName) {
        return ToolConstants.HANDBOOK_PATH + fileName;
    }

    private static List<int> fetchExpectedCosmetics(string[] cosmeticList, bool gender) {
        List<int> list = new ();

        foreach(string cosmetic in cosmeticList) {
            string cname = getCosmeticName(cosmetic, gender);
            int itemid = cosmeticNameIds.get(cname);
            if (itemid != null) {
                list.Add(itemid);
            } else {
                missingCosmeticNames.Add(cosmetic);
            }
        }

        return list;
    }

    private static void verifyCosmeticExpectedFile(File f) throws IOException {
        string townName = f.getParent().Substring(f.getParent().lastIndexOf("\\") + 1);
        string typeCosmetic = f.getName().Substring(0, f.getName().indexOf("."));

        fileReader = new InputStreamReader(new FileInputStream(f), StandardCharsets.UTF_8);
        bufferedReader = new BufferedReader(fileReader);

        string line;
        while ((line = bufferedReader.readLine()) != null) {
            string[] s = line.Split(": ", 2);
            string[] t = s[0].Split("ale ");

            string typeCoupon = t[1];
            bool gender = !t[0].contentEquals("M");

            int npcid = getCosmeticNpcid(townName, typeCosmetic, typeCoupon);
            if (npcid != null) {
                string[] cosmetics = s[1].Split(", ");
                List<int> cosmeticItemids = fetchExpectedCosmetics(cosmetics, gender);

                HashSet<int> npcCosmetics = scriptCosmetics.get(npcid);
                HashSet<int> missingCosmetics = new ();
                foreach(int itemid in cosmeticItemids) {
                    if (!npcCosmetics.Contains(itemid)) {
                        missingCosmetics.Add(itemid);
                    }
                }

                if (missingCosmetics.Count > 0) {
                    Pair<int, string> key = new (npcid, typeCoupon);

                    HashSet<int> list = missingCosmeticsNpcTypes.get(key);
                    if (list == null) {
                        missingCosmeticsNpcTypes.Add(key, missingCosmetics);
                    } else {
                        list.addAll(missingCosmetics);
                    }
                }
            }
        }

        bufferedReader.close();
        fileReader.close();
    }

    private static void verifyCosmeticExpectedData() throws IOException {
        Console.WriteLine("Analyzing cosmetic NPC scripts ...");

        <File> cosmeticRecipes = new ();
        listFiles(INPUT_DIRECTORY_PATH, cosmeticRecipes);

        foreach(File f in cosmeticRecipes) {
            verifyCosmeticExpectedFile(f);
        }
    }

    private static List<Pair<Pair<int, string>, List<int>>> getSortedMapEntries(Dictionary<Pair<int, string>, HashSet<int>> map) {
        List<Pair<Pair<int, string>, List<int>>> list = new (map.Count);
        foreach(Map.Entry<Pair<int, string>, HashSet<int>> e in map) {
            List<int> il = new (2);
            il.addAll(e.getValue());

            il.sort((o1, o2) -> o1 - o2);

            list.Add(new (e.Key, il));
        }

        list.sort((o1, o2) -> {
            int cmp = o1.getLeft().getLeft() - o2.getLeft().getLeft();
            if (cmp == 0) {
                return o1.getLeft().getRight().compareTo(o2.getLeft().getRight());
            } else {
                return cmp;
            }
        });

        return list;
    }

    private static void printReportFileHeader() {
        printWriter.println(" # Report File autogenerated from the MapleCashCosmeticsChecker feature by Ronan Lana.");
        printWriter.println(" # Generated data takes into account several data info from the server source files and the server-side WZ.xmls.");
        printWriter.println();
    }

    private static Pair<List<int>, List<int>> getCosmeticReport(List<int> itemids) {
        List<int> maleItemids = new ();
        List<int> femaleItemids = new ();

        foreach(int i in itemids) {
            if ((((i / 1000) % 10) % 3) == 0) {
                maleItemids.Add(i);
            } else {
                femaleItemids.Add(i);
            }
        }

        return new (maleItemids, femaleItemids);
    }

    private static void reportNpcCosmetics(List<int> itemids) {
        if (itemids.Count > 0) {
            string res = "    ";
            foreach(int i in itemids) {
                res += (i + ", ");
                unusedCosmetics.Remove(i);
            }

            printWriter.println(res.Substring(0, res.Length - 2));
        }
    }

    private static void reportCosmeticResults() throws IOException {
        Console.WriteLine("Reporting results ...");

        try (PrintWriter pw = new PrintWriter(Files.newOutputStream(OUTPUT_FILE));) {
            printWriter = pw;
            printReportFileHeader();

            if (missingCosmeticsNpcTypes.Count > 0) {
                printWriter.println(
                        "Found " + missingCosmeticsNpcTypes.Count + " entries with missing cosmetic entries.");

                foreach(Pair<Pair<int, string>, List<int>> mcn in getSortedMapEntries(missingCosmeticsNpcTypes)) {
                    printWriter.println("  NPC " + mcn.getLeft());

                    Pair<List<int>, List<int>> genderItemids = getCosmeticReport(mcn.getRight());
                    reportNpcCosmetics(genderItemids.getLeft());
                    reportNpcCosmetics(genderItemids.getRight());
                    printWriter.println();
                }
            }

            if (unusedCosmetics.Count > 0) {
                printWriter.println("Unused cosmetics: " + unusedCosmetics.Count);

                List<int> list = new (unusedCosmetics);
                Collections.sort(list);

                foreach(int i in list) {
                    printWriter.println(i + " " + cosmeticIdNames.get(i));
                }

                printWriter.println();
            }

            if (missingCosmeticNames.Count > 0) {
                printWriter.println("Missing cosmetic itemids: " + missingCosmeticNames.Count);

                List<string> listString = new (missingCosmeticNames);
                Collections.sort(listString);

                foreach(string c in listString) {
                    printWriter.println(c);
                }

                printWriter.println();
            }
        }
    }

    public static void main(string[] args) {
        try {
            loadCosmeticWzData();
            processCosmeticScriptData();

            loadCosmeticCouponNpcs();
            generateCosmeticPlaceNpcs();

            loadCosmeticNames();
            verifyCosmeticExpectedData();

            reportCosmeticResults();
            Console.WriteLine("Done!");
        } catch (IOException ioe) {
            ioLog.Logger.Error(e.ToString());
        }
    }
}
