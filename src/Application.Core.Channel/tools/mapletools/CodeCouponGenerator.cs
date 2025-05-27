namespace tools.mapletools;

/**
 * @author RonanLana
 * <p>
 * This application parses the coupon descriptor XML file and automatically generates
 * code entries on the DB reflecting the descriptions found. Parse time relies on the
 * sum of coupon codes created and amount of current codes on DB.
 * <p>
 * Estimated parse time: 2 minutes (for 100 code entries)
 */
public class CodeCouponGenerator
{
    private static Path INPUT_FILE = ToolConstants.getInputFile("CouponCodes.img.xml");
    private static int INITIAL_STRING_LENGTH = 250;
    private static Connection con = SimpleDatabaseConnection.getConnection();

    private static List<CodeCouponDescriptor> activeCoupons = new();
    private static HashSet<string> usedCodes = new();
    private static List<Pair<int, int>> itemList = new();

    private static BufferedReader bufferedReader = null;
    private static long currentTime;
    private static string name;
    private static bool active;
    private static int quantity;
    private static int duration;
    private static int maplePoint;
    private static int nxCredit;
    private static int nxPrepaid;
    private static Pair<int, int> item;
    private static List<int> generatedKeys;
    private static byte status;

    private static void resetCouponPackage()
    {
        name = null;
        active = false;
        quantity = 1;
        duration = 7;
        maplePoint = 0;
        nxCredit = 0;
        nxPrepaid = 0;
        itemList.Clear();
    }

    private static string getName(string token)
    {
        int i, j;
        char[] dest;
        string d;

        i = token.lastIndexOf("name");
        i = token.indexOf("\"", i) + 1; //lower bound of the string
        j = token.indexOf("\"", i);     //upper bound

        dest = new char[INITIAL_STRING_LENGTH];
        try
        {
            token.getChars(i, j, dest, 0);
        }
        catch (StringIndexOutOfRangeException e)
        {
            // do nothing
            return "";
        }
        catch (Exception e)
        {
            Console.WriteLine("error in: " + token + "");
            Log.Logger.Error(e.ToString());
            try
            {
                Thread.sleep(100000000);
            }
            catch (Exception ex)
            {
            }
        }

        return new string(dest).trim();
    }

    private static string getValue(string token)
    {
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

    private static void forwardCursor(int st)
    {
        string line = null;

        try
        {
            while (status >= st && (line = bufferedReader.readLine()) != null)
            {
                simpleToken(line);
            }
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    private static void simpleToken(string token)
    {
        if (token.Contains("/imgdir"))
        {
            status -= 1;
        }
        else if (token.Contains("imgdir"))
        {
            status += 1;
        }
    }

    private static void translateToken(string token)
    {
        if (token.Contains("/imgdir"))
        {
            status -= 1;

            if (status == 1)
            {
                if (active)
                {
                    activeCoupons.Add(new CodeCouponDescriptor(name, quantity, duration, maplePoint, nxCredit, nxPrepaid, itemList));
                }

                resetCouponPackage();
            }
            else if (status == 3)
            {
                itemList.Add(item);
            }
        }
        else if (token.Contains("imgdir"))
        {
            status += 1;

            if (status == 4)
            {
                item = new(-1, -1);
            }
            else if (status == 2)
            {
                string d = getName(token);

                Console.WriteLine("  Reading coupon '" + d + "'");
                name = d;
            }
        }
        else
        {
            string d = getName(token);

            if (status == 2)
            {
                switch (d)
                {
                    case "active":
                        if (int.Parse(getValue(token)) == 0)
                        {
                            forwardCursor(status);
                            resetCouponPackage();
                        }
                        else
                        {
                            active = true;
                        }
                        break;

                    case "quantity":
                        quantity = int.Parse(getValue(token));
                        break;
                    case "duration":
                        duration = int.Parse(getValue(token));
                        break;
                    case "maplePoint":
                        maplePoint = int.Parse(getValue(token));
                        break;
                    case "nxCredit":
                        nxCredit = int.Parse(getValue(token));
                        break;
                    case "nxPrepaid":
                        nxPrepaid = int.Parse(getValue(token));
                        break;
                }
            }
            else if (status == 4)
            {
                switch (d)
                {
                    case "count":
                        item.right = getValue(token);
                        break;
                    case "id":
                        item.left = getValue(token);
                        break;
                }
            }
        }
    }

    private static class CodeCouponDescriptor
    {
        protected string name;
        protected int quantity, duration;
        protected int nxCredit, maplePoint, nxPrepaid;
        protected List<Pair<int, int>> itemList;

        protected CodeCouponDescriptor(string name, int quantity, int duration, int maplePoint, int nxCredit, int nxPrepaid, List<Pair<int, int>> itemList)
        {
            this.name = name;
            this.quantity = quantity;
            this.duration = duration;
            this.maplePoint = maplePoint;
            this.nxCredit = nxCredit;
            this.nxPrepaid = nxPrepaid;

            this.itemList = new(itemList);
        }
    }

    private static string randomizeCouponCode()
    {
        return long.toHexString(double.doubleToLongBits(Math.random())).Substring(0, 15);
    }

    private static string generateCouponCode()
    {
        string newCode;
        do
        {
            newCode = randomizeCouponCode();
        } while (usedCodes.Contains(newCode));

        usedCodes.Add(newCode);
        return newCode;
    }

    private static List<int> getGeneratedKeys(PreparedStatement ps)
    {
        if (generatedKeys == null)
        {
            generatedKeys = new();

            ResultSet rs = ps.getGeneratedKeys();
            while (rs.next())
            {
                generatedKeys.Add(rs.getInt(1));
            }
            rs.close();
        }

        return generatedKeys;
    }

    private static void commitCodeCouponDescription(CodeCouponDescriptor recipe)
    {
        if (recipe.quantity < 1)
        {
            return;
        }

        Console.WriteLine("  Generating coupon '" + recipe.name + "'");
        generatedKeys = null;

        PreparedStatement ps = con.prepareStatement("INSERT IGNORE INTO `nxcode` (`code`, `expiration`) VALUES (?, ?)", Statement.RETURN_GENERATED_KEYS);
        ps.setLong(2, currentTime + HOURS.toMillis(recipe.duration));

        for (int i = 0; i < recipe.quantity; i++)
        {
            ps.setString(1, generateCouponCode());
            ps.addBatch();
        }
        ps.executeBatch();

        PreparedStatement ps2 = con.prepareStatement("INSERT IGNORE INTO `nxcode_items` (`codeid`, `type`, `item`, `quantity`) VALUES (?, ?, ?, ?)");
        if (recipe.itemList.Count > 0)
        {
            ps2.setInt(2, 5);
            List<int> keys = getGeneratedKeys(ps);

            foreach (Pair<int, int> p in recipe.itemList)
            {
                ps2.setInt(3, p.getLeft());
                ps2.setInt(4, p.getRight());

                foreach (int codeid in keys)
                {
                    ps2.setInt(1, codeid);
                    ps2.addBatch();
                }
            }
        }

        ps2.setInt(3, 0);
        if (recipe.nxCredit > 0)
        {
            ps2.setInt(2, 0);
            ps2.setInt(4, recipe.nxCredit);
            List<int> keys = getGeneratedKeys(ps);

            foreach (int codeid in keys)
            {
                ps2.setInt(1, codeid);
                ps2.addBatch();
            }
        }

        if (recipe.maplePoint > 0)
        {
            ps2.setInt(2, 1);
            ps2.setInt(4, recipe.maplePoint);
            List<int> keys = getGeneratedKeys(ps);

            foreach (int codeid in keys)
            {
                ps2.setInt(1, codeid);
                ps2.addBatch();
            }
        }

        if (recipe.nxPrepaid > 0)
        {
            ps2.setInt(2, 2);
            ps2.setInt(4, recipe.nxPrepaid);
            List<int> keys = getGeneratedKeys(ps);

            foreach (int codeid in keys)
            {
                ps2.setInt(1, codeid);
                ps2.addBatch();
            }
        }

        ps2.executeBatch();
        ps2.close();
        ps.close();
    }

    private static void loadUsedCouponCodes()
    {
        PreparedStatement ps = con.prepareStatement("SELECT code FROM nxcode", Statement.RETURN_GENERATED_KEYS);
        ResultSet rs = ps.executeQuery();
        while (rs.next())
        {
            usedCodes.Add(rs.getString("code"));
        }
        rs.close();
        ps.close();
    }

    private static void generateCodeCoupons(Path file) throws IOException
    {
        try (BufferedReader br = Files.newBufferedReader(file); con;) {
            bufferedReader = br;
            resetCouponPackage();
    status = 0;

            Console.WriteLine("Reading XML coupon information...");
            string line;
            while ((line = bufferedReader.readLine()) != null) {
                translateToken(line);
}
Console.WriteLine();

            Console.WriteLine("Loading DB coupon codes...");
            loadUsedCouponCodes();
Console.WriteLine();

            Console.WriteLine("Saving generated coupons...");
            currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            foreach(CodeCouponDescriptor ccd in activeCoupons) {
                commitCodeCouponDescription(ccd);
            }
            Console.WriteLine();
Console.WriteLine("Done.");

        } catch (SQLException e) {
            Log.Logger.Error(e.ToString());
        }
    }

    public static void main(string[] args)
{
    try
    {
        generateCodeCoupons(INPUT_FILE);
    }
    catch (IOException ex)
    {
        Console.WriteLine("Error reading file '" + INPUT_FILE.toAbsolutePath() + "'");
    }
}
}
