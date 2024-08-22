

using provider.wz;

namespace tools.mapletools;










/**
 * @author RonanLana
 * <p>
 * This application gathers information about the Cash Shop's EXP & DROP coupons,
 * such as applied rates, active times of day and days of week and dumps them in
 * a SQL table that the server will make use.
 */
public class CouponInstaller {
    private static Path COUPON_INPUT_FILE_1 = WZFiles.ITEM.getFile().resolve("Cash/0521.img.xml");
    private static Path COUPON_INPUT_FILE_2 = WZFiles.ITEM.getFile().resolve("Cash/0536.img.xml");
    private static Connection con = SimpleDatabaseConnection.getConnection();
    private static BufferedReader bufferedReader = null;
    private static byte status = 0;
    private static int itemId = -1;
    private static int itemMultiplier = 1;
    private static int startHour = -1;
    private static int endHour = -1;
    private static int activeDay = 0;

    private static string getName(string token) {
        int i, j;
        char[] dest;
        string d;

        i = token.lastIndexOf("name");
        if (i < 0) {
            return "";
        }

        i = token.indexOf("\"", i) + 1; //lower bound of the string
        j = token.indexOf("\"", i);     //upper bound

        dest = new char[8];
        token.getChars(i, j, dest, 0);

        d = new string(dest);
        return (d);
    }

    private static string getNodeValue(string token) {
        int i, j;
        char[] dest;
        string d;

        i = token.lastIndexOf("value=");
        i = token.indexOf("\"", i) + 1; //lower bound of the string
        j = token.indexOf("\"", i);     //upper bound

        if (j - i < 1) {
            return "";
        }

        dest = new char[j - i];
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

    private static int getDayOfWeek(string day) {
        return switch (day) {
            case "SUN" -> 1;
            case "MON" -> 2;
            case "TUE" -> 3;
            case "WED" -> 4;
            case "THU" -> 5;
            case "FRI" -> 6;
            case "SAT" -> 7;
            default -> 0;
        };
    }

    private static void processHourTimeString(string time) {
        startHour = int.Parse(time.Substring(4, 6));
        endHour = int.Parse(time.Substring(7, 9));
    }

    private static void processDayTimeString(string time) {
        string day = time.Substring(0, 3);
        int d = getDayOfWeek(day);

        activeDay |= (1 << d);
    }

    private static void loadTimeFromCoupon(int st) {
        Console.WriteLine("Loading coupon id " + itemId + ". Rate: " + itemMultiplier + "x.");

        string line = null;
        try {
            startHour = -1;
            endHour = -1;
            activeDay = 0;

            string time = null;
            while ((line = bufferedReader.readLine()) != null) {
                simpleToken(line);
                if (status < st) {
                    break;
                }

                time = getNodeValue(line);
                processDayTimeString(time);

                simpleToken(line);
            }

            if (time != null) {
                processHourTimeString(time);

                PreparedStatement ps = con.prepareStatement("INSERT INTO nxcoupons (couponid, rate, activeday, starthour, endhour) VALUES (?, ?, ?, ?, ?)");
                ps.setInt(1, itemId);
                ps.setInt(2, itemMultiplier);
                ps.setInt(3, activeDay);
                ps.setInt(4, startHour);
                ps.setInt(5, endHour);
                ps.execute();

                ps.close();
            }
        } catch (SQLException | IOException e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void translateToken(string token) {
        string d;

        if (token.Contains("/imgdir")) {
            status -= 1;
        } else if (token.Contains("imgdir")) {
            if (status == 1) {           //getting ItemId
                d = getName(token);
                itemId = int.Parse(d);
            } else if (status == 2) {
                d = getName(token);

                if (!d.Contains("info")) {
                    forwardCursor(status);
                }
            } else if (status == 3) {
                d = getName(token);

                if (!d.Contains("time")) {
                    forwardCursor(status);
                } else {
                    loadTimeFromCoupon(status);
                }
            }

            status += 1;
        } else {
            if (status == 3) {
                d = getName(token);

                if (d.Contains("rate")) {
                    string r = getNodeValue(token);

                    double db = double.parseDouble(r);
                    itemMultiplier = (int) db;
                }
            }
        }
    }

    private static void installRateCoupons(Path file) {
        // This will reference one line at a time
        string line = null;

        try (BufferedReader br = Files.newBufferedReader(file)) {
            bufferedReader = br;

            while ((line = bufferedReader.readLine()) != null) {
                translateToken(line);
            }

        } catch (FileNotFoundException ex) {
            Console.WriteLine("Unable to open file '" + file + "'");
        } catch (IOException ex) {
            Console.WriteLine("Error reading file '" + file + "'");
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void installCouponsTable() {
        try {
            PreparedStatement ps = con.prepareStatement("DROP TABLE IF EXISTS `nxcoupons`;");
            ps.execute();
            ps.close();

            ps = con.prepareStatement(
                    """
                            CREATE TABLE IF NOT EXISTS `nxcoupons` (
                              `id` int(11) NOT NULL AUTO_INCREMENT,
                              `couponid` int(11) NOT NULL DEFAULT '0',
                              `rate` int(11) NOT NULL DEFAULT '0',
                              `activeday` int(11) NOT NULL DEFAULT '0',
                              `starthour` int(11) NOT NULL DEFAULT '0',
                              `endhour` int(11) NOT NULL DEFAULT '0',
                              PRIMARY KEY (`id`)
                            ) ENGINE=InnoDB DEFAULT CHARSET=latin1 AUTO_INCREMENT=1;"""
            );

            ps.execute();
            ps.close();

            installRateCoupons(COUPON_INPUT_FILE_1);
            installRateCoupons(COUPON_INPUT_FILE_2);

            con.close();
        } catch (SQLException e) {
            Console.WriteLine("Warning: Could not establish connection to database to change card chance rate.");
            Console.WriteLine(e.getMessage());
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }

    public static void main(string[] args) {
        installCouponsTable();
    }
}
