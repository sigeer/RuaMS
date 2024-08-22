namespace tools.mapletools;



class ToolConstants {
    static Path INPUT_DIRECTORY = Path.of("tools/input");
    static Path OUTPUT_DIRECTORY = Path.of("tools/output");
    static string SCRIPTS_PATH = "scripts";
    static string HANDBOOK_PATH = "handbook";

    static Path getInputFile(string fileName) {
        return INPUT_DIRECTORY.resolve(fileName);
    }

    static Path getOutputFile(string fileName) {
        return OUTPUT_DIRECTORY.resolve(fileName);
    }
}
