

using provider.wz;

namespace tools.mapletools;












/**
 * @author RonanLana
 * <p>
 * This application updates the Monster Book drop data with the actual underlying drop data from
 * the Maplestory database specified in the URL below.
 * <p>
 * In other words all items drops from monsters listed inside the Mob Book feature will be patched to match exactly like the item
 * drop list specified in the URL's Maplestory database.
 * <p>
 * The original file "MonsterBook.img.xml" from string.wz must be copied to the directory of this application and only then
 * executed. This program will generate another file that must replace the original server file to make the effects take place
 * to on your server.
 * <p>
 * After replacing on server, this XML must be updated on the client via WZ Editor (HaRepack for instance). Once inside the repack,
 * remove the property 'MonsterBook.img' inside 'string.wz' and choose to import the xml generated with this software.
 */
public class MobBookUpdate {
    private static Path INPUT_FILE = WZFiles.STRING.getFile().resolve("MonsterBook.img.xml");
    private static Path OUTPUT_FILE = ToolConstants.getOutputFile("MonsterBook_updated.img.xml");
    private static Connection con = SimpleDatabaseConnection.getConnection();

    private static PrintWriter printWriter = null;
    private static BufferedReader bufferedReader = null;
    private static byte status = 0;
    private static int mobId = -1;

    private static string getName(string token) {
        int i, j;
        char[] dest;
        string d;

        i = token.lastIndexOf("name");
        i = token.indexOf("\"", i) + 1; //lower bound of the string
        j = token.indexOf("\"", i);     //upper bound

        if (j - i < 7) {
            dest = new char[6];
        } else {
            dest = new char[7];
        }
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
            if (line != null) {
                printWriter.println(line);
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

    private static void loadDropsFromMob() {
        Console.WriteLine("Loading mob id " + mobId);

        try {
            string toPrint;
            int itemId, cont = 0;

            PreparedStatement ps = con.prepareStatement("SELECT itemid FROM drop_data WHERE (dropperid = ? AND itemid > 0) GROUP BY itemid;");
            ps.setInt(1, mobId);
            ResultSet rs = ps.executeQuery();

            while (rs.next()) {
                toPrint = "";
                for (int k = 0; k <= status; k++) {
                    toPrint += "  ";
                }

                toPrint += "<int name=\"";
                toPrint += cont;
                toPrint += "\" value=\"";

                itemId = rs.getInt("itemid");
                toPrint += itemId;
                toPrint += "\" />";

                printWriter.println(toPrint);
                cont++;
            }

            rs.close();
            ps.close();
        } catch (SQLException e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void translateToken(string token) {
        string d;
        int temp;

        printWriter.println(token);

        if (token.Contains("/imgdir")) {
            status -= 1;
        } else if (token.Contains("imgdir")) {
            if (status == 1) {           //getting MobId
                d = getName(token);
                mobId = int.Parse(d);
            } else if (status == 2) {
                d = getName(token);

                if (d.Contains("reward")) {
                    temp = status;

                    loadDropsFromMob();
                    forwardCursor(temp);
                }
            }

            status += 1;
        }

    }

    private static void updateFromDropData() {
        try (con;
                PrintWriter pw = new PrintWriter(Files.newOutputStream(OUTPUT_FILE));
                BufferedReader br = Files.newBufferedReader(INPUT_FILE);) {
            printWriter = pw;
            bufferedReader = br;

            string line = null;

            while ((line = bufferedReader.readLine()) != null) {
                translateToken(line);
            }
        } catch (FileNotFoundException ex) {
            Console.WriteLine("Unable to open file '" + INPUT_FILE + "'");
        } catch (IOException ex) {
            Console.WriteLine("Error reading file '" + INPUT_FILE + "'");
        } catch (SQLException e) {
            Console.WriteLine("Warning: Could not establish connection to database to change card chance rate.");
            Console.WriteLine(e.getMessage());
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    public static void main(string[] args) {
        updateFromDropData();
    }
}

