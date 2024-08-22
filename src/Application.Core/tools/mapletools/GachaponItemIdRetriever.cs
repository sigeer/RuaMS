namespace tools.mapletools;



















/**
 * @author RonanLana
 * <p>
 * This application reads metadata for the gachapons found on the "gachapon_items.txt"
 * recipe file, then checks up the Handbook DB (installed through MapleIdRetriever)
 * and translates the item names from the recipe file into their respective itemids.
 * The translated itemids are then stored in specific gachapon files inside the
 * "lib/gachapons" folder.
 * <p>
 * Estimated parse time: 1 minute
 */
public class GachaponItemIdRetriever {
    private static Path INPUT_FILE = ToolConstants.getInputFile("gachapon_items.txt");
    private static Path OUTPUT_DIRECTORY = ToolConstants.getOutputFile("gachapons");
    private static Connection con = SimpleDatabaseConnection.getConnection();
    private static Pattern pattern = Pattern.compile("(\\d*)%");
    private static int[] scrollsChances = new int[]{10, 15, 30, 60, 65, 70, 100};
    private static Dictionary<GachaponScroll, List<int>> scrollItemids = new ();

    private static PrintWriter printWriter = null;

    private static void insertGachaponScrollItemid(int id, string name, string description, bool both) {
        GachaponScroll gachaScroll = getGachaponScroll(name, description, both);

        List<int> list = scrollItemids.get(gachaScroll);
        if (list == null) {
            list = new ();
            scrollItemids.Add(gachaScroll, list);
        }

        list.Add(id);
    }

    private static void loadHandbookUseNames() {
        PreparedStatement ps = con.prepareStatement("SELECT * FROM `handbook` WHERE `id` >= 2040000 AND `id` < 2050000 ORDER BY `id` ASC;");
        ResultSet rs = ps.executeQuery();

        while (rs.next()) {
            int id = rs.getInt("id");
            string name = rs.getString("name");

            if (isUpgradeScroll(name)) {
                string description = rs.getString("description");
                insertGachaponScrollItemid(id, name, description, false);
                insertGachaponScrollItemid(id, name, description, true);
            }
        }

        rs.close();
        ps.close();

        /*
        foreach(Entry<GachaponScroll, List<int>> e in scrollItemids) {
            Console.WriteLine(e);
        }
        Console.WriteLine("------------");
        */
    }

    private static class GachaponScroll {
        private string header;
        private string target;
        private string buff;
        private int prop;

        private GachaponScroll(GachaponScroll from, int prop) {
            this.header = from.header;
            this.target = from.target;
            this.buff = from.buff;
            this.prop = prop;
        }

        private GachaponScroll(string name, string description, bool both) {
            string[] params = name.Split(" for ");
            if (paramsValue.Length < 3) {
                return;
            }

            string header = both ? "scroll" : " " + paramsValue[0];
            string target = paramsValue[1];

            int prop = 0;
            string buff = params[2];

            Matcher m = pattern.matcher(buff);
            if (m.find()) {
                prop = int.Parse(m.group(1));
                buff = buff.Substring(0, m.start() - 1).trim();
            } else {
                m = pattern.matcher(description);

                if (m.find()) {
                    prop = int.Parse(m.group(1));
                }
            }

            int idx = buff.indexOf(" (");   // remove percentage & dots from name checking
            if (idx > -1) {
                buff = buff.Substring(0, idx);
            }
            buff = buff.replace(".", "");

            this.header = header;
            this.target = target;
            this.buff = buff;
            this.prop = prop;
        }

        public override int GetHashCode() {
            int result = prop ^ (prop >>> 32);
            result = 31 * result + (header != null ? header.GetHashCode()() : 0);
            result = 31 * result + (target != null ? target.GetHashCode()() : 0);
            result = 31 * result + (buff != null ? buff.GetHashCode()() : 0);
            return result;
        }

        public override bool equals(object o) {
            if (this == o) {
                return true;
            }
            if (o == null || GetType() != o.GetType()) {
                return false;
            }
            GachaponScroll sc = (GachaponScroll) o;
            if (header != null ? !header.Equals(sc.header) : sc.header != null) {
                return false;
            }
            if (target != null ? !target.Equals(sc.target) : sc.target != null) {
                return false;
            }
            if (buff != null ? !buff.Equals(sc.buff) : sc.buff != null) {
                return false;
            }
            return prop == sc.prop;
        }

        public override string ToString() {
            return header + " for " + target + " for " + buff + " - " + prop + "%";
        }

    }

    private static string getGachaponScrollResults(string line, bool both) {
        string str = "";
        List<GachaponScroll> gachaScrollList;

        GachaponScroll gachaScroll = getGachaponScroll(line, "", both);
        if (gachaScroll.prop != 0) {
            gachaScrollList = Collections.singletonList(gachaScroll);
        } else {
            gachaScrollList = new (scrollsChances.Length);

            foreach(int prop in scrollsChances) {
                gachaScrollList.Add(new GachaponScroll(gachaScroll, prop));
            }
        }

        foreach(GachaponScroll gs in gachaScrollList) {
            List<int> gachaItemids = scrollItemids.get(gs);
            if (gachaItemids != null) {
                string listStr = "";
                foreach(int id in gachaItemids) {
                    listStr += id.ToString();
                    listStr += " ";
                }

                if (gachaItemids.Count > 1) {
                    str += "[" + listStr + "]";
                } else {
                    str += listStr;
                }
            }
        }

        return str;
    }

    private static GachaponScroll getGachaponScroll(string name, string description, bool both) {
        name = name.ToLower();
        name = name.replace("for acc ", "for accuracy ");
        name = name.replace("blunt weapon", "bw");
        name = name.replace("eye eqp.", "eye accessory");
        name = name.replace("face eqp.", "face accessory");
        name = name.replace("for attack", "for att");
        name = name.replace("1-handed", "one-handed");
        name = name.replace("2-handed", "two-handed");

        return new GachaponScroll(name, description, both);
    }

    private static bool isUpgradeScroll(string name) {
        return name.matches("^(([D|d]ark )?[S|s]croll for).*");
    }

    private static void fetchLineOnMapleHandbook(string line, string rarity) {
        string str = "";
        if (!isUpgradeScroll(line)) {
            PreparedStatement ps = con.prepareStatement("SELECT `id` FROM `handbook` WHERE `name` LIKE ? ORDER BY `id` ASC;");
            ps.setString(1, line);

            ResultSet rs = ps.executeQuery();
            while (rs.next()) {
                int id = rs.getInt("id");

                str += int.toString(id);
                str += " ";
            }

            rs.close();
            ps.close();
        } else {
            str += getGachaponScrollResults(line, false);
            if (str.Count == 0) {
                str += getGachaponScrollResults(line, true);

                if (str.Count == 0) {
                    Console.WriteLine("NONE for '" + line + "' : " + getGachaponScroll(line, "", false));
                }
            }
        }

        if (str.Count == 0) {
            str += line;
        }

        if (rarity != null) {
            str += ("- " + rarity);
        }

        printWriter.println(str);
    }

    private static void fetchDataOnMapleHandbook() {
        string line;
        try (BufferedReader bufferedReader = Files.newBufferedReader(INPUT_FILE)) {
            int skip = 0;
            bool lineHeader = false;
            while ((line = bufferedReader.readLine()) != null) {
                if (skip > 0) {
                    skip--;

                    if (lineHeader) {
                        if (line.Count > 0) {
                            lineHeader = false;
                            printWriter.println();
                            printWriter.println(line + ":");
                        }
                    }
                } else if (line.Count == 0) {
                    printWriter.println("");
                } else if (line.startsWith("Gachapon ")) {
                    string[] s = line.Split("� ");
                    string gachaponName = s[s.Length - 1];
                    gachaponName = gachaponName.replace(" ", "_");
                    gachaponName = gachaponName.ToLower();

                    if (printWriter != null) {
                        printWriter.close();
                    }
                    Path outputFile = OUTPUT_DIRECTORY.resolve(gachaponName + ".txt");
                    setupDirectories(outputFile);

                    printWriter = new PrintWriter(Files.newOutputStream(outputFile));

                    skip = 2;
                    lineHeader = true;
                } else if (line.startsWith(".")) {
                    skip = 1;
                    lineHeader = true;
                } else {
                    line = line.replace("�", "'");
                    foreach(string item in line.Split("\\s\\|\\s")) {
                        item = item.trim();
                        if (!item.contentEquals("n/a")) {
                            string[] itemInfo = item.Split(" - ");
                            fetchLineOnMapleHandbook(itemInfo[0], itemInfo.Length > 1 ? itemInfo[1] : null);
                        }
                    }
                }
            }
        } catch (IOException ex) {
            Console.WriteLine(ex.getMessage());
            Log.Logger.Error(ex.ToString());
        }
    }

    private static void setupDirectories(Path file) {
        if (!Files.exists(file.getParent())) {
            try {
                Files.createDirectories(file.getParent());
            } catch (IOException e) {
                // TODO Auto-generated catch block
                Log.Logger.Error(e.ToString());
            }
        }
    }

    public static void main(string[] args) {
        try (con) {
            loadHandbookUseNames();
            fetchDataOnMapleHandbook();
        } catch (SQLException e) {
            Console.WriteLine("Error: invalid SQL syntax");
            Console.WriteLine(e.getMessage());
        }
    }
}

