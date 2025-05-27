

using provider.wz;

namespace tools.mapletools;










/**
 * @author RonanLana
 * <p>
 * This application parses skillbook XMLs, filling up stack amount of those
 * items to 100 (eliminating limitations on held skillbooks, now using
 * default stack quantity expected from USE items).
 * <p>
 * Estimated parse time: 10 seconds
 */
public class SkillbookStackUpdate {
    private static Path INPUT_DIRECTORY = WZFiles.ITEM.getFile().resolve("Consume");
    private static Path OUTPUT_DIRECTORY = ToolConstants.getOutputFile("skillbook-update");
    private static int INITIAL_STRING_LENGTH = 50;

    private static PrintWriter printWriter = null;
    private static BufferedReader bufferedReader = null;
    private static int status = 0;

    private static bool isSkillMasteryBook(int itemid) {
        return itemid >= 2280000 && itemid < 2300000;
    }

    private static string getName(string token) {
        int i, j;
        char[] dest;
        string d;

        i = token.lastIndexOf("name");
        i = token.indexOf("\"", i) + 1; //lower bound of the string
        j = token.indexOf("\"", i);     //upper bound

        if (j < i) {
            return "0";           //node value containing 'name' in it's scope, cheap fix since we don't deal with strings anyway
        }

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
        printWriter.println(token);
    }

    private static void translateItemToken(string token) {
        if (token.Contains("/imgdir")) {
            status -= 1;
        } else if (token.Contains("imgdir")) {
            status += 1;

            if (status == 2) {      //itemid
                int itemid = int.Parse(getName(token));

                if (!isSkillMasteryBook(itemid)) {
                    printWriter.println(token);
                    forwardCursor(status);
                    return;
                }
            }
        } else {
            if (status == 3) {
                if (getName(token).contentEquals("slotMax")) {
                    printWriter.println("      <int name=\"slotMax\" value=\"100\"/>");
                    return;
                }
            }
        }

        printWriter.println(token);
    }

    private static void parseItemFile(Path file, Path outputFile) {
		setupDirectories(outputFile);

		try (BufferedReader br = Files.newBufferedReader(file);
				PrintWriter pw = new PrintWriter(Files.newOutputStream(outputFile))) {
			bufferedReader = br;
			printWriter = pw;
			string line;
			while ((line = bufferedReader.readLine()) != null) {
				translateItemToken(line);
			}
		} catch (IOException ex) {
			Console.WriteLine("Error reading file '" + file.getFileName() + "'");
			Log.Logger.Error(ex.ToString());
		} catch (Exception e) {
			Log.Logger.Error(e.ToString());
		}
    }

    private static void setupDirectories(Path file) {
    	if (!Files.exists(file.getParent())) {
    		try {
				Files.createDirectories(file.getParent());
			} catch (IOException e) {
				Console.WriteLine("Error creating folder '" + file.getParent() + "'");
				Log.Logger.Error(e.ToString());
			}
    	}
    }

    private static void parseItemDirectory(Path inputDirectory, Path outputDirectory) {
    	try (DirectoryStream<Path> stream = Files.newDirectoryStream(inputDirectory)) {
        	foreach(Path path in stream) {
        		parseItemFile(path, outputDirectory.resolve(path.getFileName()));
            }
        } catch (IOException e) {
			Log.Logger.Error(e.ToString());
		}
    }

    public static void main(string[] args) {
    	Instant instantStarted = Instant.now();
        Console.WriteLine("Reading item files...");
        parseItemDirectory(INPUT_DIRECTORY, OUTPUT_DIRECTORY);
        Console.WriteLine("Done!");
        Instant instantStopped = Instant.now();
        Duration durationBetween = Duration.between(instantStarted, instantStopped);
        Console.WriteLine("Get elapsed time in milliseconds: " + durationBetween.toMillis());
        Console.WriteLine("Get elapsed time in seconds: " + durationBetween.toSeconds());
    }

}
