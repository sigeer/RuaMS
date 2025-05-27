

using provider.wz;

namespace tools.mapletools;











/**
 * @author RonanLana
 * <p>
 * This application simply gets from the MonsterBook.img.xml all mobid's and
 * puts them on a SQL table with the correspondent mob cardid.
 */
public class MobBookIndexer {
    private static Path INPUT_FILE = WZFiles.STRING.getFile().resolve("MonsterBook.img.xml");
    private static Connection con = SimpleDatabaseConnection.getConnection();

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

    private static bool isCard(int itemId) {
        return itemId / 10000 == 238;
    }

    private static void loadPairFromMob() {
        Console.WriteLine("Loading mob id " + mobId);

        try {
            PreparedStatement ps, ps2;
            ResultSet rs;

            ps = con.prepareStatement("SELECT itemid FROM drop_data WHERE (dropperid = ? AND itemid > 0) GROUP BY itemid;");
            ps.setInt(1, mobId);
            rs = ps.executeQuery();

            while (rs.next()) {
                int itemId = rs.getInt("itemid");
                if (isCard(itemId)) {
                    ps2 = con.prepareStatement("INSERT INTO `monstercardwz` (`cardid`, `mobid`) VALUES (?, ?)");
                    ps2.setInt(1, itemId);
                    ps2.setInt(2, mobId);

                    ps2.executeUpdate();
                }
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

                    loadPairFromMob();
                    forwardCursor(temp);
                }
            }

            status += 1;
        }

    }

    private static void indexFromDropData() {

		try (con; BufferedReader br = Files.newBufferedReader(INPUT_FILE);) {
			bufferedReader = br;
			string line = null;

			PreparedStatement ps = con.prepareStatement("DROP TABLE IF EXISTS monstercardwz;");
			ps.execute();

            ps = con.prepareStatement("CREATE TABLE `monstercardwz` ("
                    + "`id` int(10) unsigned NOT NULL AUTO_INCREMENT,"
                    + "`cardid` int(10) NOT NULL DEFAULT '-1',"
                    + "`mobid` int(10) NOT NULL DEFAULT '-1',"
                    + "PRIMARY KEY (`id`)"
                    + ");");
            ps.execute();

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
			Log.Logger.Error(e.ToString());
		} catch (Exception e) {
			Log.Logger.Error(e.ToString());
		}
    }

    public static void main(string[] args) {
        indexFromDropData();
    }
}

