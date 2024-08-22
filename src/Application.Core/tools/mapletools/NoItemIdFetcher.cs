

using provider.wz;

namespace tools.mapletools;





















/**
 * @author RonanLana
 * <p>
 * This application finds inexistent itemids within the drop data from
 * the Maplestory database specified in the URL below. This program
 * assumes all itemids uses 7 digits.
 * <p>
 * A file is generated listing all the inexistent ids.
 */
public class NoItemIdFetcher {
    private static Path OUTPUT_FILE = ToolConstants.getOutputFile("no_item_id_report.txt");
    private static Connection con = SimpleDatabaseConnection.getConnection();

    private static HashSet<int> existingIds = new ();
    private static HashSet<int> nonExistingIds = new ();

    private static PrintWriter printWriter = null;
    private static BufferedReader bufferedReader = null;
    private static byte status = 0;
    private static int itemId = -1;

    private static string getName(string token) {
        int i, j;
        char[] dest;
        string d;

        i = token.lastIndexOf("name");
        i = token.indexOf("\"", i) + 1; //lower bound of the string
        j = token.indexOf("\"", i);     //upper bound

        dest = new char[100];
        token.getChars(i, j, dest, 0);

        d = new string(dest);
        return (d);
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
            if (status == 1) {           //getting ItemId
                d = getName(token);
                itemId = int.Parse(d.Substring(1, 8));

                existingIds.Add(itemId);
                forwardCursor(status);
            }

            status += 1;
        }
    }

    private static void readItemDataFile(File file) {
        // This will reference one line at a time
        string line = null;

        try {
            InputStreamReader fileReader = new InputStreamReader(new FileInputStream(file), StandardCharsets.UTF_8);
            bufferedReader = new BufferedReader(fileReader);

            status = 0;
            try {
                while ((line = bufferedReader.readLine()) != null) {
                    translateToken(line);
                }
            } catch (NumberFormatException npe) {
                // second criteria, itemid is on the name of the file

                try {
                    itemId = int.Parse(file.getName().Substring(0, 7));
                    existingIds.Add(itemId);
                } catch (NumberFormatException npe2) {
                }
            }

            bufferedReader.close();
            fileReader.close();
        } catch (FileNotFoundException ex) {
            Console.WriteLine("Unable to open file '" + file.getName() + "'");
        } catch (IOException ex) {
            Console.WriteLine("Error reading file '" + file.getName() + "'");
        }
    }

    private static void readEquipDataDirectory(string dirPath) {
        File[] folders = new File(dirPath).listFiles();
        //If this pathname does not denote a directory, then listFiles() returns null.

        foreach(File folder in folders) {   // enter all subfolders
            if (folder.isDirectory()) {
                Console.WriteLine("Reading '" + dirPath + "/" + folder.getName() + "'...");

                try {
                    File[] files = folder.listFiles();

                    foreach(File file in files) {   // enter all XML files under subfolders
                        if (file.isFile()) {
                            itemId = int.Parse(file.getName().Substring(0, 8));
                            existingIds.Add(itemId);
                        }
                    }
                } catch (NumberFormatException nfe) {
                }
            }
        }
    }

    private static void readItemDataDirectory(string dirPath) {
        File[] folders = new File(dirPath).listFiles();
        //If this pathname does not denote a directory, then listFiles() returns null.

        foreach(File folder in folders) {   // enter all subfolders
            if (folder.isDirectory()) {
                Console.WriteLine("Reading '" + dirPath + "/" + folder.getName() + "'...");

                File[] files = folder.listFiles();

                foreach(File file in files) {   // enter all XML files under subfolders
                    if (file.isFile()) {
                        readItemDataFile(file);
                    }
                }
            }
        }
    }

    private static void evaluateDropsFromTable(string table) {
        PreparedStatement ps = con.prepareStatement("SELECT DISTINCT itemid FROM " + table + ";");
        ResultSet rs = ps.executeQuery();

        while (rs.next()) {
            if (!existingIds.Contains(rs.getInt(1))) {
                nonExistingIds.Add(rs.getInt(1));
            }
        }

        rs.close();
        ps.close();
    }

    private static void evaluateDropsFromDb() {
        try (con) {
            Console.WriteLine("Evaluating item data on DB...");

            evaluateDropsFromTable("drop_data");
            evaluateDropsFromTable("reactordrops");

            if (nonExistingIds.Count > 0) {
                List<int> list = new (nonExistingIds);
                Collections.sort(list);

                foreach(int i in list) {
                    printWriter.println(i);
                }
            }

            Console.WriteLine("Inexistent itemid count: " + nonExistingIds.Count);
            Console.WriteLine("Total itemid count: " + existingIds.Count);

        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    public static void main(string[] args) {
        try (PrintWriter pw = new PrintWriter(Files.newOutputStream(OUTPUT_FILE))) {
            printWriter = pw;
            existingIds.Add(0); // meso itemid
            readEquipDataDirectory(WZFiles.CHARACTER.getFilePath());
            readItemDataDirectory(WZFiles.ITEM.getFilePath());

            evaluateDropsFromDb();
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }
}
