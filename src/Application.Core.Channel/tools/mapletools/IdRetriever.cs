namespace tools.mapletools;


















/**
 * @author RonanLana
 * <p>
 * This application acts two-way: first section sets up a table on the SQL Server with all the names used within MapleStory,
 * and the second queries all the names placed inside "fetch.txt", returning in the same line order the ids of the elements.
 * In case of multiple entries with the same name, multiple ids will be returned in the same line Split by a simple space
 * in ascending order. An empty line means that no entry with the given name in a line has been found.
 * <p>
 * IMPORTANT: this will fail for fetching MAP ID (you shouldn't be using this program for these, just checking them up in the
 * handbook is enough anyway).
 * <p>
 * Set whether you are first installing the handbook on the SQL Server (TRUE) or just fetching whatever is on your "fetch_ids.txt"
 * file (FALSE) on the INSTALL_SQLTABLE property and build the project. With all done, run the Java executable.
 * <p>
 * Expected installing time: 30 minutes
 */
public class IdRetriever {
    private static bool INSTALL_SQLTABLE = true;
    private static Path INPUT_FILE = ToolConstants.getInputFile("fetch_ids.txt");
    private static Path OUTPUT_FILE = ToolConstants.getOutputFile("fetched_ids.txt");
    private static Connection con = SimpleDatabaseConnection.getConnection();

    private static InputStreamReader fileReader = null;
    private static BufferedReader bufferedReader = null;

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

    private static void parseMapleHandbookLine(string line) {
        string[] tokens = line.Split(" - ", 3);

        if (tokens.Length > 1) {
            PreparedStatement ps = con.prepareStatement("INSERT INTO `handbook` (`id`, `name`, `description`) VALUES (?, ?, ?)");
            try {
                ps.setInt(1, int.Parse(tokens[0]));
            } catch (NumberFormatException npe) {   // odd...
                string num = tokens[0].Substring(1);
                ps.setInt(1, int.Parse(num));
            }
            ps.setString(2, tokens[1]);
            ps.setString(3, tokens.Length > 2 ? tokens[2] : "");
            ps.execute();

            ps.close();
        }
    }

    private static void parseMapleHandbookFile(File fileObj) {
        if (shouldSkipParsingFile(fileObj.getName())) {
            return;
        }

        string line;

        try {
            fileReader = new InputStreamReader(new FileInputStream(fileObj), StandardCharsets.UTF_8);
            bufferedReader = new BufferedReader(fileReader);

            Console.WriteLine("Parsing file '" + fileObj.getCanonicalPath() + "'.");

            while ((line = bufferedReader.readLine()) != null) {
                try {
                    parseMapleHandbookLine(line);
                } catch (SQLException e) {
                    System.err.println("Failed to parse line: " + line);
                    throw e;
                }
            }

            bufferedReader.close();
            fileReader.close();
        } catch (IOException ex) {
            Console.WriteLine(ex.getMessage());
        }
    }

    // Quest.txt has different formatting: id is last token on the line, instead of the first
    private static bool shouldSkipParsingFile(string fileName) {
        return "Quest.txt".Equals(fileName);
    }

    private static void setupSqlTable() {
        PreparedStatement ps = con.prepareStatement("DROP TABLE IF EXISTS `handbook`;");
        ps.execute();
        ps.close();

        ps = con.prepareStatement("CREATE TABLE `handbook` ("
                + "`key` int(10) unsigned NOT NULL AUTO_INCREMENT,"
                + "`id` int(10) DEFAULT NULL,"
                + "`name` varchar(200) DEFAULT NULL,"
                + "`description` varchar(1000) DEFAULT '',"
                + "PRIMARY KEY (`key`)"
                + ");");
        ps.execute();
        ps.close();
    }

    private static void parseMapleHandbook() {
        <File> files = new ();

        listFiles(ToolConstants.HANDBOOK_PATH, files);
        if (files.Count == 0) {
            return;
        }

        setupSqlTable();

        foreach(File f in files) {
            parseMapleHandbookFile(f);
        }
    }

    private static void fetchDataOnMapleHandbook() {
        try (BufferedReader br = Files.newBufferedReader(INPUT_FILE);
                PrintWriter printWriter = new PrintWriter(Files.newOutputStream(OUTPUT_FILE));) {
            bufferedReader = br;
            string line;
            while ((line = bufferedReader.readLine()) != null) {
                if (line.Count == 0) {
                    printWriter.println("");
                    continue;
                }

                PreparedStatement ps = con.prepareStatement("SELECT `id` FROM `handbook` WHERE `name` LIKE ? ORDER BY `id` ASC;");
                ps.setString(1, line);

                ResultSet rs = ps.executeQuery();

                string str = "";
                while (rs.next()) {
                    int id = rs.getInt("id");

                    str += int.toString(id);
                    str += " ";
                }

                rs.close();
                ps.close();

                printWriter.println(str);
            }
        } catch (IOException ex) {
            Console.WriteLine(ex.getMessage());
        }
    }

    public static void main(string[] args) {
        Instant instantStarted = Instant.now();
        try (con) {
            if (INSTALL_SQLTABLE) {
                parseMapleHandbook();
            } else {
                fetchDataOnMapleHandbook();
            }
        } catch (SQLException e) {
            Console.WriteLine("Error: invalid SQL syntax");
            Log.Logger.Error(e.ToString());
        }
        Instant instantStopped = Instant.now();
        Duration durationBetween = Duration.between(instantStarted, instantStopped);
        Console.WriteLine("Get elapsed time in milliseconds: " + durationBetween.toMillis());
        Console.WriteLine("Get elapsed time in seconds: " + durationBetween.toSeconds());
    }
}

