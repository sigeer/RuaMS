namespace tools.mapletools;















/**
 * @author RonanLana
 * <p>
 * This application objective is to read all scripts from the event folder
 * and fill empty functions for every function name not yet present in the
 * script.
 * <p>
 * Estimated parse time: 10 seconds
 */
public class EventMethodFiller {
    private static Collection<string> RELEVANT_FILE_EXTENSIONS = Set.of("sql", "js", "txt", "java");

    private static bool foundMatchingDataOnFile(string fileContent, Pattern pattern) {
        Matcher matcher = pattern.matcher(fileContent);
        return matcher.find();
    }

    private static void filterDirectorySearchMatchingData(string directoryPath, Dictionary<Pattern, string> functions)
            throws IOException {
        Files.walk(Path.of(directoryPath))
                .filter(EventMethodFiller::isRelevantFile)
                .forEach(path -> fileSearchMatchingData(path, functions));
    }

    private static bool isRelevantFile(Path path) {
        string fileName = path.getFileName().ToString();
        return RELEVANT_FILE_EXTENSIONS.stream().anyMatch(fileName::endsWith);
    }

    private static void fileSearchMatchingData(Path file, Dictionary<Pattern, string> functions) {
        try {
            string fileContent = Files.readString(file);
            List<string> fillFunctions = new ();

            foreach(Map.Entry<Pattern, string> f in functions) {
                if (!foundMatchingDataOnFile(fileContent, f.Key)) {
                    fillFunctions.Add(f.getValue());
                }
            }

            if (fillFunctions.Count > 0) {
                Console.WriteLine("Filling out " + file.getFileName().ToString() + "...");

                try (PrintWriter printWriter = new PrintWriter(Files.newBufferedWriter(file, StandardOpenOption.APPEND))) {
                    printWriter.println();
                    printWriter.println();
                    printWriter.println("// ---------- FILLER FUNCTIONS ----------");
                    printWriter.println();

                    foreach(string s in fillFunctions) {
                        printWriter.println(s);
                        printWriter.println();
                    }
                }
            }
        } catch (IOException e) {
            Log.Logger.Error(e.ToString());
        }
    }

    private static Pattern compileJsFunctionPattern(string function) {
        string jsFunction = "function(\\s)+";
        return Pattern.compile(jsFunction + function);
    }

    private static Dictionary<Pattern, string> getFunctions() {
        Dictionary<Pattern, string> functions = new ();
        functions.Add(compileJsFunctionPattern("playerEntry"), "function playerEntry(eim, player) {}");
        functions.Add(compileJsFunctionPattern("playerExit"), "function playerExit(eim, player) {}");
        functions.Add(compileJsFunctionPattern("scheduledTimeout"), "function scheduledTimeout(eim) {}");
        functions.Add(compileJsFunctionPattern("playerUnregistered"), "function playerUnregistered(eim, player) {}");
        functions.Add(compileJsFunctionPattern("changedLeader"), "function changedLeader(eim, leader) {}");
        functions.Add(compileJsFunctionPattern("monsterKilled"), "function monsterKilled(mob, eim) {}");
        functions.Add(compileJsFunctionPattern("allMonstersDead"), "function allMonstersDead(eim) {}");
        functions.Add(compileJsFunctionPattern("playerDisconnected"), "function playerDisconnected(eim, player) {}");
        functions.Add(compileJsFunctionPattern("monsterValue"), "function monsterValue(eim, mobid) {return 0;}");
        functions.Add(compileJsFunctionPattern("dispose"), "function dispose() {}");
        functions.Add(compileJsFunctionPattern("leftParty"), "function leftParty(eim, player) {}");
        functions.Add(compileJsFunctionPattern("disbandParty"), "function disbandParty(eim, player) {}");
        functions.Add(compileJsFunctionPattern("clearPQ"), "function clearPQ(eim) {}");
        functions.Add(compileJsFunctionPattern("afterSetup"), "function afterSetup(eim) {}");
        functions.Add(compileJsFunctionPattern("cancelSchedule"), "function cancelSchedule() {}");
        functions.Add(compileJsFunctionPattern("setup"), "function setup(eim, leaderid) {}");
        //put(compileJsFunctionPattern("getEligibleParty"), "function getEligibleParty(party) {}"); not really needed
        return functions;
    }

    public static void main(string[] args) throws IOException {
        filterDirectorySearchMatchingData(ToolConstants.SCRIPTS_PATH + "/event", getFunctions());
    }

}
