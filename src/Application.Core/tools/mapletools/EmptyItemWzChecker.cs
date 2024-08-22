

using provider.wz;

namespace tools.mapletools;



















/**
 * @author RonanLana
 * <p>
 * This application has two objectives: it reports in a detailed file all itemids which is
 * currently missing either a name entry in the string.wz or an item entry in the Item.wz;
 * And it removes from the string.wz XMLs all entries which misses properties on Item.wz.
 */
public class EmptyItemWzChecker {
    private static Path OUTPUT_FILE = ToolConstants.getOutputFile("empty_item_wz_report.txt");
    private static string OUTPUT_PATH = ToolConstants.OUTPUT_DIRECTORY.ToString();
    private static int INITIAL_STRING_LENGTH = 50;
    private static int ITEM_FILE_NAME_SIZE = 13;

    private static Stack<string> currentPath = new Stack<>();
    private static Dictionary<int, string> stringWzItems = new ();
    private static Dictionary<int, string> contentWzItems = new ();
    private static HashSet<int> handbookItems = new ();

    private static PrintWriter printWriter = null;
    private static InputStreamReader fileReader = null;
    private static BufferedReader bufferedReader = null;
    private static byte status = 0;
    private static int currentItemid = 0;
    private static int currentDepth = 0;
    private static string currentFile;
    private static HashSet<int> nonPropItems;

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

    private static int getItemIdFromFilename(string name) {
        try {
            return int.Parse(name.Substring(0, name.indexOf('.')));
        } catch (Exception e) {
            return -1;
        }
    }

    private static void inspectItemWzEntry() {
        string line = null;

        try {
            while ((line = bufferedReader.readLine()) != null) {
                translateItemToken(line);
            }
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static string currentItemPath() {
        string s = currentFile + " -> ";

        foreach(string p in currentPath) {
            s += (p + "\\");
        }

        return s;
    }

    private static void translateItemToken(string token) {
        if (token.Contains("/imgdir")) {
            status -= 1;

            currentPath.pop();
        } else if (token.Contains("imgdir")) {
            status += 1;
            string d = getName(token);

            if (status == 2) {
                currentItemid = int.Parse(d);
                contentWzItems.Add(currentItemid, currentItemPath());

                forwardCursor(status);
            } else {
                currentPath.push(d);
            }
        }
    }

    private static void inspectStringWzEntry() {
        string line = null;

        try {
            while ((line = bufferedReader.readLine()) != null) {
                translateStringToken(line);
            }
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void translateStringToken(string token) {
        if (token.Contains("/imgdir")) {
            status -= 1;
            currentPath.pop();
        } else if (token.Contains("imgdir")) {
            status += 1;
            string d = getName(token);

            if (status == currentDepth) {
                currentItemid = int.Parse(d);
                stringWzItems.Add(currentItemid, currentItemPath());
                //if (currentItemid >= 4000000) Console.WriteLine("  " + currentItemid);

                forwardCursor(status);
            } else {
                currentPath.push(d);
            }
        }
    }

    private static void loadStringWzFile(string filePath, int depth) throws IOException {
        fileReader = new InputStreamReader(new FileInputStream(filePath), StandardCharsets.UTF_8);
        bufferedReader = new BufferedReader(fileReader);

        currentFile = filePath;
        currentDepth = 2 + depth;
        //Console.WriteLine(filePath + " depth " + depth);
        inspectStringWzEntry();

        bufferedReader.close();
        fileReader.close();
    }

    private static void loadStringWz() throws IOException {
        Console.WriteLine("Reading string.wz ...");
        string[][] stringWzFiles = {{"Cash", "Consume", "Ins", "Pet"}, {"Etc"}, {"Eqp"}};

        foreach(int i = 0; i < stringWzFiles.Length; i++) { for (string dirFile in stringWzFiles[i]) {
                string fileName = "/" + dirFile + ".img.xml";
                loadStringWzFile(WZFiles.STRING.getFilePath() + fileName, i);
            }
        }
    }

    private static void loadItemWz() throws IOException {
        Console.WriteLine("Reading Item.wz ...");
        <File> files = new ();
        listFiles(WZFiles.ITEM.getFilePath(), files);

        foreach(File f in files) {
            if (f.getParentFile().getName().contentEquals("Special")) {
                continue;
            }

            //Console.WriteLine("Parsing " + f.getAbsolutePath());
            fileReader = new InputStreamReader(new FileInputStream(f), StandardCharsets.UTF_8);
            bufferedReader = new BufferedReader(fileReader);

            currentFile = f.getCanonicalPath();

            if (f.getName().Length <= ITEM_FILE_NAME_SIZE) {
                inspectItemWzEntry();
            } else {    // pet file structure is similar to equips, maybe there are other item-types following this behaviour?
                int itemid = getItemIdFromFilename(f.getName());
                if (itemid < 0) {
                    continue;
                }

                currentItemid = itemid;
                contentWzItems.Add(currentItemid, currentItemPath());
            }

            bufferedReader.close();
            fileReader.close();
        }
    }

    private static void loadCharacterWz() throws IOException {
        Console.WriteLine("Reading Character.wz ...");
        <File> files = new ();
        listFiles(WZFiles.CHARACTER.getFilePath(), files);

        foreach(File f in files) {
            if (f.getParentFile().getName().contentEquals("Character.wz")) {
                continue;
            }

            int itemid = getItemIdFromFilename(f.getName());
            if (itemid < 0) {
                continue;
            }

            currentFile = f.getCanonicalPath();
            currentItemid = itemid;
            contentWzItems.Add(currentItemid, currentItemPath());
        }
    }

    private static void calculateItemNameDiff(HashSet<int> emptyItemNames, HashSet<int> emptyNameItems) {
        foreach(int i in contentWzItems.Keys) {
            if (!stringWzItems.ContainsKey(i)) {
                emptyNameItems.Add(i);
            }
        }

        foreach(int i in stringWzItems.Keys) {
            if (!contentWzItems.ContainsKey(i)) {
                emptyItemNames.Add(i);
            }
        }
    }

    private static void readHandbookItems() throws IOException {
        Console.WriteLine("Reading handbook ...");
        string[] handbookPaths = {"Equip", "Cash.txt", "Etc.txt", "Pet.txt", "Setup.txt", "Use.txt"};

        foreach(string path in handbookPaths) {
            readHandbookPath(ToolConstants.HANDBOOK_PATH + "/" + path);
        }
    }

    private static void readHandbookPath(string filePath) throws IOException {
        <File> files = new ();

        File testFile = new File(filePath);
        if (testFile.isDirectory()) {
            listFiles(filePath, files);
        } else {
            files.Add(testFile);
        }

        foreach(File f in files) {
            fileReader = new InputStreamReader(new FileInputStream(f), StandardCharsets.UTF_8);
            bufferedReader = new BufferedReader(fileReader);

            string line = null;

            try {
                while ((line = bufferedReader.readLine()) != null) {
                    string[] tokens = line.Split(" - ");

                    if (tokens[0].Length > 0) {
                        int itemid = int.Parse(tokens[0]);
                        handbookItems.Add(itemid);
                    }
                }
            } catch (Exception e) {
                Log.Logger.Error(e.ToString());
            }

            bufferedReader.close();
            fileReader.close();
        }
    }

    private static void printReportFileHeader() {
        printWriter.println(" # Report File autogenerated from the MapleEmptyItemWzChecker feature by Ronan Lana.");
        printWriter.println(" # Generated data takes into account several data info from the server-side WZ.xmls.");
        printWriter.println();
    }

    private static List<int> getSortedItems(HashSet<int> items) {
        List<int> sortedItems = new (items);
        Collections.sort(sortedItems);

        return sortedItems;
    }

    private static void printReportFileResults(HashSet<int> emptyItemNames, HashSet<int> emptyNameItems) {
        if (emptyItemNames.Count > 0) {
            printWriter.println("string.wz NAMES with no Item.wz node, " + emptyItemNames.Count + " entries:");

            foreach(int itemid in getSortedItems(emptyItemNames)) {
                printWriter.println("  " + itemid + " " + stringWzItems.get(itemid) + (handbookItems.Contains(itemid) ? "" : " NOT FOUND"));
            }

            printWriter.println();
        }

        if (emptyNameItems.Count > 0) {
            printWriter.println("Item.wz ITEMS with no string.wz node, " + emptyNameItems.Count + " entries:");

            foreach(int itemid in getSortedItems(emptyNameItems)) {
                printWriter.println("  " + itemid + " " + contentWzItems.get(itemid) + (handbookItems.Contains(itemid) ? "" : " NOT FOUND"));
            }

            printWriter.println();
        }
    }

    private static void reportItemNameDiff(HashSet<int> emptyItemNames, HashSet<int> emptyNameItems) throws IOException {
        Console.WriteLine("Reporting results...");
        try(PrintWriter pw = new PrintWriter(Files.newOutputStream(OUTPUT_FILE))) {
        	printWriter = pw;
        	printReportFileHeader();
            printReportFileResults(emptyItemNames, emptyNameItems);
        }
    }

    private static void locateItemStringWzDiff() throws IOException {
        HashSet<int> emptyItemNames = new HashHashSet<>(), emptyNameItems = new ();
        calculateItemNameDiff(emptyItemNames, emptyNameItems);

        reportItemNameDiff(emptyItemNames, emptyNameItems);
        nonPropItems = emptyItemNames;
    }

    private static void runEmptyItemWzChecker() throws IOException {
        readHandbookItems();

        loadCharacterWz();
        loadItemWz();
        loadStringWz();

        locateItemStringWzDiff();
    }

    private static void generateStringWzEntry() {
        string line = null;

        try {
            while ((line = bufferedReader.readLine()) != null) {
                updateStringToken(line);
            }
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void updateStringToken(string token) {
        if (token.Contains("/imgdir")) {
            status -= 1;

        } else if (token.Contains("imgdir")) {
            status += 1;

            if (status == currentDepth && nonPropItems.Contains(getName(token))) {
                forwardCursor(status);
                return;
            }
        }

        printWriter.println(token);
    }

    private static void generateStringWzFile(string filePath, int depth) throws IOException {
        fileReader = new InputStreamReader(new FileInputStream(WZFiles.DIRECTORY + filePath), StandardCharsets.UTF_8);
        bufferedReader = new BufferedReader(fileReader);
        printWriter = new PrintWriter(OUTPUT_PATH + filePath, StandardCharsets.UTF_8);
        currentDepth = 2 + depth;

        //Console.WriteLine(filePath + " depth " + depth);
        generateStringWzEntry();

        printWriter.close();
        bufferedReader.close();
        fileReader.close();
    }

    private static void generateStringWz() throws IOException {
        Console.WriteLine("Generating clean string.wz ...");
        string[][] stringWzFiles = { { "Cash", "Consume", "Ins", "Pet" }, { "Etc" }, { "Eqp" } };
        string stringWzPath = "/string.wz/";

        File folder = new File(OUTPUT_PATH + "/string.wz/");
        if (!folder.exists()) {
            folder.mkdirs();
        }

        foreach(int i = 0; i < stringWzFiles.Length; i++) { for (string dirFile in stringWzFiles[i]) {
                generateStringWzFile(stringWzPath + dirFile + ".img.xml", i);
            }
        }
    }

    public static void main(string[] args) {
        try {
            runEmptyItemWzChecker();
            generateStringWz();

            Console.WriteLine("Done!");
        } catch (IOException ioe) {
            ioLog.Logger.Error(e.ToString());
        }
    }

}
