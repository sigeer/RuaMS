

using provider.wz;
using server;
using tools;

namespace tools.mapletools;












/**
 * @author RonanLana
 * <p>
 * The objective of this program is to uncover all maker data from the
 * ItemMaker.wz.xml files and generate a SQL file with every data info
 * for the Maker DB tables.
 */

public class SkillMakerFetcher {
    private static Path INPUT_FILE = WZFiles.ETC.getFile().resolve("ItemMake.img.xml");
    private static Path OUTPUT_FILE = ToolConstants.getOutputFile("maker-data.sql");
    private static int INITIAL_STRING_LENGTH = 50;

    private static PrintWriter printWriter = null;
    private static BufferedReader bufferedReader = null;
    private static byte status = 0;
    private static byte state = 0;

    // maker data fields
    private static int id = -1;
    private static int itemid = -1;
    private static int reqLevel = -1;
    private static int reqMakerLevel = -1;
    private static int reqItem = -1;
    private static int reqMeso = -1;
    private static int reqEquip = -1;
    private static int catalyst = -1;
    private static int quantity = -1;
    private static int tuc = -1;

    private static int recipePos = -1;
    private static int recipeProb = -1;
    private static int recipeCount = -1;
    private static int recipeItem = -1;

    static List<int[]> recipeList = null;
    static List<int[]> randomList = null;
    static List<MakerItemEntry> makerList = new (100);

    private static void resetMakerDataFields() {
        reqLevel = 0;
        reqMakerLevel = 0;
        reqItem = 0;
        reqMeso = 0;
        reqEquip = 0;
        catalyst = 0;
        quantity = 0;
        tuc = 0;

        recipePos = 0;
        recipeProb = 0;
        recipeCount = 0;
        recipeItem = 0;

        recipeList = null;
        randomList = null;
    }

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
        string s = d.trim();
        s.replaceFirst("^0+(?!$)", "");

        return (s);
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
        string s = d.trim();
        s.replaceFirst("^0+(?!$)", "");

        return (s);
    }

    private static int[] generateRecipeItem() {
        int[] pair = new int[2];
        pair[0] = recipeItem;
        pair[1] = recipeCount;

        return pair;
    }

    private static int[] generateRandomItem() {
        int[] tuple = new int[3];
        tuple[0] = recipeItem;
        tuple[1] = recipeCount;
        tuple[2] = recipeProb;

        return tuple;
    }

    private static void simpleToken(string token) {
        if (token.Contains("/imgdir")) {
            status -= 1;
        } else if (token.Contains("imgdir")) {
            status += 1;
        }
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

    private static void translateToken(string token) {
        string d;

        if (token.Contains("/imgdir")) {
            status -= 1;

            if (status == 2) {   //close item maker data
                generateUpdatedItemFee();   // for equipments, this will try to update reqMeso to be conformant with the client.
                makerList.Add(new MakerItemEntry(id, itemid, reqLevel, reqMakerLevel, reqItem, reqMeso, reqEquip, catalyst, quantity, tuc, recipeCount, recipeItem, recipeList, randomList));
                resetMakerDataFields();
            } else if (status == 4) {    //close recipe/random item
                if (state == 0) {
                    recipeList.Add(generateRecipeItem());
                } else if (state == 1) {
                    randomList.Add(generateRandomItem());
                }
            }
        } else if (token.Contains("imgdir")) {
            if (status == 1) {           //getting id
                d = getName(token);
                id = int.Parse(d);
                Console.WriteLine("Parsing maker id " + id);
            } else if (status == 2) {      //getting target item id
                d = getName(token);
                itemid = int.Parse(d);
            } else if (status == 3) {
                d = getName(token);

                switch (d) {
                    case "recipe" -> {
                        recipeList = new ();
                        state = 0;
                    }
                    case "randomReward" -> {
                        randomList = new ();
                        state = 1;
                    }
                    default -> forwardCursor(3);   // unused content, read until end of block
                }
            } else if (status == 4) {  // inside recipe/random
                d = getName(token);
                recipePos = int.Parse(d);
            }

            status += 1;
        } else {
            if (status == 3) {
                d = getName(token);

                switch (d) {
                    case "itemNum" -> quantity = int.Parse(getValue(token));
                    case "meso" -> reqMeso = int.Parse(getValue(token));
                    case "reqItem" -> reqItem = int.Parse(getValue(token));
                    case "reqLevel" -> reqLevel = int.Parse(getValue(token));
                    case "reqSkillLevel" -> reqMakerLevel = int.Parse(getValue(token));
                    case "tuc" -> tuc = int.Parse(getValue(token));
                    case "catalyst" -> catalyst = int.Parse(getValue(token));
                    case "reqEquip" -> reqEquip = int.Parse(getValue(token));
                    default -> {
                        Console.WriteLine("Unhandled case: '" + d + "'");
                        state = 2;
                    }
                }
            } else if (status == 5) {  // inside recipe/random item
                d = getName(token);
                if (d.Equals("item")) {
                    recipeItem = int.Parse(getValue(token));
                } else {
                    if (state == 0) {
                        recipeCount = int.Parse(getValue(token));
                    } else {
                        if (d.Equals("itemNum")) {
                            recipeCount = int.Parse(getValue(token));
                        } else {
                            recipeProb = int.Parse(getValue(token));
                        }
                    }
                }
            }
        }
    }

    private static void generateUpdatedItemFee() {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        float adjPrice = reqMeso;

        if (itemid < 2000000) {
            Dictionary<string, int> stats = ii.getEquipStats(itemid);
            if (stats != null) {
                int val = itemid / 100000;

                if (val == 13 || val == 14) {    // is weapon-type
                    adjPrice /= 10;
                    adjPrice += reqMeso;

                    adjPrice /= 1000;
                    reqMeso = 1000 * (int) Math.Floor(adjPrice);
                } else {
                    adjPrice /= ((stats.get("reqLevel") >= 108) ? 10 : 11);
                    adjPrice += reqMeso;

                    adjPrice /= 1000;
                    reqMeso = 1000 * (int) Math.Ceiling(adjPrice);
                }
            } else {
                Console.WriteLine("null stats for itemid " + itemid);
            }
        } else {
            adjPrice /= 10;
            adjPrice += reqMeso;

            adjPrice /= 1000;
            reqMeso = 1000 * (int) Math.Ceiling(adjPrice);
        }
    }

    private static void WriteMakerTableFile() {
        printWriter.println(" # SQL File autogenerated from the MapleSkillMakerFetcher feature by Ronan Lana.");
        printWriter.println(" # Generated data is conformant with the ItemMake.img.xml file used to compile this.");
        printWriter.println();

        StringBuilder sb_create = new StringBuilder("INSERT IGNORE INTO `makercreatedata` (`id`, `itemid`, `req_level`, `req_maker_level`, `req_meso`, `req_item`, `req_equip`, `catalyst`, `quantity`, `tuc`) VALUES\r\n");
        StringBuilder sb_recipe = new StringBuilder("INSERT IGNORE INTO `makerrecipedata` (`itemid`, `req_item`, `count`) VALUES\r\n");
        StringBuilder sb_reward = new StringBuilder("INSERT IGNORE INTO `makerrewarddata` (`itemid`, `rewardid`, `quantity`, `prob`) VALUES\r\n");

        foreach(MakerItemEntry it in makerList) {
            sb_create.Append("  (" + it.id + ", " + it.itemid + ", " + it.reqLevel + ", " + it.reqMakerLevel + ", " + it.reqMeso + ", " + it.reqItem + ", " + it.reqEquip + ", " + it.catalyst + ", " + it.quantity + ", " + it.tuc + "),\r\n");

            if (it.recipeList != null) {
                foreach(int[] rit in it.recipeList) {
                    sb_recipe.Append("  (" + it.itemid + ", " + rit[0] + ", " + rit[1] + "),\r\n");
                }
            }

            if (it.randomList != null) {
                foreach(int[] rit in it.randomList) {
                    sb_reward.Append("  (" + it.itemid + ", " + rit[0] + ", " + rit[1] + ", " + rit[2] + "),\r\n");
                }
            }
        }

        sb_create.setLength(sb_create.Length - 3);
        sb_create.Append(";\r\n");

        sb_recipe.setLength(sb_recipe.Length - 3);
        sb_recipe.Append(";\r\n");

        sb_reward.setLength(sb_reward.Length - 3);
        sb_reward.Append(";");

        printWriter.println(sb_create);
        printWriter.println(sb_recipe);
        printWriter.println(sb_reward);
    }

	private static void writeMakerTableData() {
		// This will reference one line at a time
        string line = null;

        try (PrintWriter pw = new PrintWriter(Files.newOutputStream(OUTPUT_FILE));
                BufferedReader br = Files.newBufferedReader(INPUT_FILE);) {
            printWriter = pw;
            bufferedReader = br;

            resetMakerDataFields();

            while ((line = bufferedReader.readLine()) != null) {
                translateToken(line);
            }

            WriteMakerTableFile();

        } catch (FileNotFoundException ex) {
            Console.WriteLine("Unable to open file '" + INPUT_FILE + "'");
        } catch (IOException ex) {
            Console.WriteLine("Error reading file '" + INPUT_FILE + "'");
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
	}

    public static void main(string[] args) {
        DatabaseConnection.initializeConnectionPool(); // Using ItemInformationProvider which loads som unrelated things from the db
        writeMakerTableData();
    }
}

