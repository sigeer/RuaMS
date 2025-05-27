

using provider;
using provider;
using provider;
using provider;
using provider;
using provider;
using provider.wz;

namespace tools.mapletools;












/**
 * @author RonanLana
 * <p>
 * This application finds itemids with inexistent name and description from
 * within the server-side XMLs, then identify them on a report file along
 * with a XML excerpt to be appended on the string.wz xml nodes. This program
 * assumes all equipids are depicted using 8 digits and item using 7 digits.
 * <p>
 * Estimated parse time: 2 minutes
 */
public class NoItemNameFetcher {
    private static Path OUTPUT_FILE = ToolConstants.getOutputFile("no_item_name_result.txt");
    private static Path OUTPUT_XML_FILE = ToolConstants.getOutputFile("no_item_name_xml.txt");

    private static Dictionary<int, string> itemsWzPath = new ();
    private static Dictionary<int, EquipType> equipTypes = new ();
    private static Dictionary<int, ItemType> itemsWithNoNameProperty = new ();
    private static HashSet<int> equipsWithNoCashProperty = new ();
    private static Dictionary<int, string> nameContentCache = new ();
    private static Dictionary<int, string> descContentCache = new ();

    private static PrintWriter printWriter = null;
    private static ItemType curType = ItemType.UNDEF;

    private enum ItemType {
        UNDEF, CASH, CONSUME, EQP, ETC, INS, PET
    }

    private enum EquipType {
        UNDEF, ACCESSORY, CAP, CAPE, COAT, FACE, GLOVE, HAIR, LONGCOAT, PANTS, PETEQUIP, RING, SHIELD, SHOES, TAMING, WEAPON
    }

    private static void processStringSubdirectoryData(Data subdirData, string subdirPath) {
        foreach(Data md in subdirData.getChildren()) {
            try {
                Data nameData = md.getChildByPath("name");
                Data descData = md.getChildByPath("desc");

                int itemId = int.Parse(md.getName());
                if (nameData != null && descData != null) {
                    itemsWithNoNameProperty.Remove(itemId);
                } else {
                    if (nameData != null) {
                        nameContentCache.Add(itemId, DataTool.getString("name", md));
                    } else if (descData != null) {
                        descContentCache.Add(itemId, DataTool.getString("desc", md));
                    }

                    Console.WriteLine("Found itemid on string.wz with no full property: " + subdirPath + subdirData.getName() + "/" + md.getName());
                }
            } catch (NumberFormatException nfe) {
                Console.WriteLine("Error reading string image: " + subdirPath + subdirData.getName() + "/" + md.getName());
            }
        }
    }

    private static void readStringSubdirectoryData(Data subdirData, int depth, string subdirPath) {
        if (depth > 0) {
            foreach(Data mDir in subdirData.getChildren()) {
                readStringSubdirectoryData(mDir, depth - 1, subdirPath + mDir.getName() + "/");
            }
        } else {
            processStringSubdirectoryData(subdirData, subdirPath);
        }
    }

    private static void readStringSubdirectoryData(Data subdirData, int depth) {
        readStringSubdirectoryData(subdirData, depth, "");
    }

    private static void readStringWZData() {
        Console.WriteLine("Parsing string.wz...");
        DataProvider stringData = DataProviderFactory.getDataProvider(WZFiles.STRING);

        Data cashStringData = stringData.getData("Cash.img");
        readStringSubdirectoryData(cashStringData, 0);

        Data consumeStringData = stringData.getData("Consume.img");
        readStringSubdirectoryData(consumeStringData, 0);

        Data eqpStringData = stringData.getData("Eqp.img");
        readStringSubdirectoryData(eqpStringData, 2);

        Data etcStringData = stringData.getData("Etc.img");
        readStringSubdirectoryData(etcStringData, 1);

        Data insStringData = stringData.getData("Ins.img");
        readStringSubdirectoryData(insStringData, 0);

        Data petStringData = stringData.getData("Pet.img");
        readStringSubdirectoryData(petStringData, 0);
    }

    private static bool isTamingMob(int itemId) {
        int itemType = itemId / 1000;
        return itemType == 1902 || itemType == 1912;
    }

    private static bool isAccessory(int itemId) {
        return itemId >= 1110000 && itemId < 1140000;
    }

    private static ItemType getItemTypeFromDirectoryName(string dirName) {
        return switch (dirName) {
            case "Cash" -> ItemType.CASH;
            case "Consume" -> ItemType.CONSUME;
            case "Etc" -> ItemType.ETC;
            case "Install" -> ItemType.INS;
            case "Pet" -> ItemType.PET;
            default -> ItemType.UNDEF;
        };
    }

    private static EquipType getEquipTypeFromDirectoryName(string dirName) {
        return switch (dirName) {
            case "Accessory" -> EquipType.ACCESSORY;
            case "Cap" -> EquipType.CAP;
            case "Cape" -> EquipType.CAPE;
            case "Coat" -> EquipType.COAT;
            case "Face" -> EquipType.FACE;
            case "Glove" -> EquipType.GLOVE;
            case "Hair" -> EquipType.HAIR;
            case "Longcoat" -> EquipType.LONGCOAT;
            case "Pants" -> EquipType.PANTS;
            case "PetEquip" -> EquipType.PETEQUIP;
            case "Ring" -> EquipType.RING;
            case "Shield" -> EquipType.SHIELD;
            case "Shoes" -> EquipType.SHOES;
            case "TamingMob" -> EquipType.TAMING;
            case "Weapon" -> EquipType.WEAPON;
            default -> EquipType.UNDEF;
        };
    }

    private static string getStringDirectoryNameFromEquipType(EquipType eType) {
        return switch (eType) {
            case ACCESSORY -> "Accessory";
            case CAP -> "Cap";
            case CAPE -> "Cape";
            case COAT -> "Coat";
            case FACE -> "Face";
            case GLOVE -> "Glove";
            case HAIR -> "Hair";
            case LONGCOAT -> "Longcoat";
            case PANTS -> "Pants";
            case PETEQUIP -> "PetEquip";
            case RING -> "Ring";
            case SHIELD -> "Shield";
            case SHOES -> "Shoes";
            case TAMING -> "Taming";
            case WEAPON -> "Weapon";
            default -> "Undefined";
        };
    }

    private static void readEquipNodeData(DataProvider data, DataDirectoryEntry mDir, string wzFileName, string dirName) {
        EquipType eqType = getEquipTypeFromDirectoryName(dirName);

        foreach(DataFileEntry mFile in mDir.getFiles()) {
            string fileName = mFile.getName();

            try {
                int itemId = int.Parse(fileName.Substring(0, 8));
                itemsWithNoNameProperty.Add(itemId, curType);
                equipTypes.Add(itemId, eqType);

                itemsWzPath.Add(itemId, wzFileName + "/" + dirName + "/" + fileName);

                if (!isAccessory(itemId) && !isTamingMob(itemId)) {
                    try {
                        Data fileData = data.getData(dirName + "/" + fileName);
                        Data mdinfo = fileData.getChildByPath("info");
                        if (mdinfo.getChildByPath("cash") == null) {
                            equipsWithNoCashProperty.Add(itemId);
                        }
                    } catch (NullReferenceException npe) {
                        Console.WriteLine("[SEVERE] " + mFile.getName() + " failed to load. Issue: " + npe.getMessage() + "\n\n");
                    }
                }
            } catch (Exception e) {
            }
        }
    }

    private static void readEquipWZData() {
        string wzFileName = "Character.wz";

        DataProvider data = DataProviderFactory.getDataProvider(WZFiles.CHARACTER);
        DataDirectoryEntry root = data.getRoot();

        Console.WriteLine("Parsing " + wzFileName + "...");
        foreach(DataDirectoryEntry mDir in root.getSubdirectories()) {
            string dirName = mDir.getName();
            if (dirName.contentEquals("Dragon")) {
                continue;
            }

            readEquipNodeData(data, mDir, wzFileName, dirName);
        }
    }

    private static void readItemWZData() {
        string wzFileName = "Item.wz";

        DataProvider data = DataProviderFactory.getDataProvider(WZFiles.ITEM);
        DataDirectoryEntry root = data.getRoot();

        Console.WriteLine("Parsing " + wzFileName + "...");
        foreach(DataDirectoryEntry mDir in root.getSubdirectories()) {
            string dirName = mDir.getName();
            if (dirName.contentEquals("Special")) {
                continue;
            }

            curType = getItemTypeFromDirectoryName(dirName);
            if (!dirName.contentEquals("Pet")) {
                foreach(DataFileEntry mFile in mDir.getFiles()) {
                    string fileName = mFile.getName();

                    Data fileData = data.getData(dirName + "/" + fileName);
                    foreach(Data mData in fileData.getChildren()) {
                        try {
                            int itemId = int.Parse(mData.getName());
                            itemsWithNoNameProperty.Add(itemId, curType);
                            itemsWzPath.Add(itemId, wzFileName + "/" + dirName + "/" + fileName);
                        } catch (Exception e) {
                            Console.WriteLine("EXCEPTION on '" + mData.getName() + "' " + wzFileName + "/" + dirName + "/" + fileName);
                        }
                    }
                }
            } else {
                readEquipNodeData(data, mDir, wzFileName, dirName);
            }
        }
    }

    private static void printReportFileHeader() {
        printWriter.println(" # Report File autogenerated from the MapleInvalidItemWithNoNameFetcher feature by Ronan Lana.");
        printWriter.println(" # Generated data takes into account several data info from the server-side WZ.xmls.");
        printWriter.println();
    }

    private static void printReportFileResults() {
        if (itemsWithNoNameProperty.Count > 0) {
            printWriter.println("Itemids with missing 'name' property: ");

            List<int> itemids = new (itemsWithNoNameProperty.Keys);
            Collections.sort(itemids);

            foreach(int itemid in itemids) {
                printWriter.println("  " + itemid + " " + itemsWzPath.get(itemid));
            }
            printWriter.println();
        }

        if (equipsWithNoCashProperty.Count > 0) {
            printWriter.println("Equipids with missing 'cash' property: ");

            List<int> itemids = new (equipsWithNoCashProperty);
            Collections.sort(itemids);

            foreach(int itemid in itemids) {
                printWriter.println("  " + itemid + " " + itemsWzPath.get(itemid));
            }
        }
    }

    private static Dictionary<string, List<int>> filterMissingItemNames() {
        List<int> cashList = new (20);
        List<int> consList = new (20);
        List<int> eqpList = new (20);
        List<int> etcList = new (20);
        List<int> insList = new (20);
        List<int> petList = new (20);

        foreach(Map.Entry<int, ItemType> ids in itemsWithNoNameProperty) {
            switch (ids.getValue()) {
                case CASH -> cashList.Add(ids.Key);
                case CONSUME -> consList.Add(ids.Key);
                case EQP -> eqpList.Add(ids.Key);
                case ETC -> etcList.Add(ids.Key);
                case INS -> insList.Add(ids.Key);
                case PET -> petList.Add(ids.Key);
            }
        }

        Dictionary<string, List<int>> nameTags = new ();
        nameTags.Add("Cash.img", cashList);
        nameTags.Add("Consume.img", consList);
        nameTags.Add("Eqp.img", eqpList);
        nameTags.Add("Etc.img", etcList);
        nameTags.Add("Ins.img", insList);
        nameTags.Add("Pet.img", petList);

        return nameTags;
    }

    private static void printOutputFileHeader() {
        printWriter.println(" # XML File autogenerated from the MapleInvalidItemWithNoNameFetcher feature by Ronan Lana.");
        printWriter.println(" # Generated data takes into account several data info from the server-side WZ.xmls.");
        printWriter.println();
    }

    private static string getMissingEquipName(int itemid) {
        string s = nameContentCache.get(itemid);
        if (s == null) {
            s = "MISSING NAME " + itemid;
        }

        return s;
    }

    private static string getMissingEquipDesc(int itemid) {
        string s = descContentCache.get(itemid);
        if (s == null && itemid >= 2000000) {   // thanks Halcyon for noticing "missing info" on equips
            s = "MISSING INFO " + itemid;
        }

        return s;
    }

    private static void writeMissingEquipInfo(int itemid) {
        printWriter.println("      <imgdir name=\"" + itemid + "\">");

        string s;
        s = getMissingEquipName(itemid);
        printWriter.println("        <string name=\"name\" value=\"" + s + "\"/>");

        s = getMissingEquipDesc(itemid);
        printWriter.println("        <string name=\"desc\" value=\"" + s + "\"/>");
        printWriter.println("      </imgdir>");
    }

    private static void writeEquipSubdirectoryHeader(EquipType eType) {
        printWriter.println("    <imgdir name=\"" + getStringDirectoryNameFromEquipType(eType) + "\">");
    }

    private static void writeEquipSubdirectoryFooter() {
        printWriter.println("    </imgdir>");
    }

    private static void writeEquipXMLHeader() {
        printWriter.println("  <imgdir name=\"Eqp\">");
    }

    private static void writeEquipXMLFooter() {
        printWriter.println("  </imgdir>");
    }

    private static void writeMissingItemInfo(int itemid) {
        printWriter.println("  <imgdir name=\"" + itemid + "\">");
        printWriter.println("    <string name=\"name\" value=\"MISSING NAME\"/>");
        printWriter.println("    <string name=\"desc\" value=\"MISSING INFO\"/>");
        printWriter.println("  </imgdir>");
    }

    private static void writeXMLHeader(string fileName) {
        printWriter.println("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        printWriter.println("<imgdir name=\"" + fileName + "\">");
    }

    private static void writeXMLFooter() {
        printWriter.println("</imgdir>");
    }

    private static void writeMissingEquipWZNode(EquipType eType, List<int> missingNames) {
        if (missingNames.Count > 0) {
            Collections.sort(missingNames);
            writeEquipSubdirectoryHeader(eType);

            foreach(int equipid in missingNames) {
                writeMissingEquipInfo(equipid);
            }

            writeEquipSubdirectoryFooter();
        }
    }

    private static void writeMissingStringWZNode(string nodePath, List<int> missingNames, bool isEquip) {
        if (missingNames.Count > 0) {
            if (!isEquip) {
                Collections.sort(missingNames);

                printWriter.println(nodePath + ":");
                printWriter.println();

                writeXMLHeader(nodePath);

                foreach(int i in missingNames) {
                    writeMissingItemInfo(i);
                }

                writeXMLFooter();

                printWriter.println();
            } else {
                int arraySize = EquipType.values().Length;

                List<int>[] equips = new List[arraySize];
                for (int i = 0; i < arraySize; i++) {
                    equips[i] = new (42);
                }

                foreach(int itemid in missingNames) {
                    equips[equipTypes.get(itemid).ordinal()].Add(itemid);
                }

                printWriter.println(nodePath + ":");
                printWriter.println();

                writeXMLHeader(nodePath);
                writeEquipXMLHeader();

                foreach(EquipType eType in EquipType.values()) {
                    writeMissingEquipWZNode(eType, equips[eType.ordinal()]);
                }

                writeEquipXMLFooter();
                writeXMLFooter();

                printWriter.println();
            }
        }
    }

    private static void writeMissingStringWZNames(Dictionary<string, List<int>> missingNames) throws Exception {
        Console.WriteLine("Writing remaining 'string.wz' names...");
        try (PrintWriter pw = new PrintWriter(Files.newOutputStream(OUTPUT_XML_FILE))) {
            printWriter = pw;

            printOutputFileHeader();

            string[] nodePaths = { "Cash.img", "Consume.img", "Eqp.img", "Etc.img", "Ins.img", "Pet.img" };
            for (int i = 0; i < nodePaths.Length; i++) {
                writeMissingStringWZNode(nodePaths[i], missingNames.get(nodePaths[i]), i == 2);
            }

        }
    }

    public static void main(string[] args) {
        try (PrintWriter pw = new PrintWriter(Files.newOutputStream(OUTPUT_FILE))) {
            printWriter = pw;
            curType = ItemType.EQP;
            readEquipWZData();

            curType = ItemType.UNDEF;
            readItemWZData();
            readStringWZData(); // calculates the diff and effectively holds all items with no name property on the WZ

            Console.WriteLine("Reporting results...");
            printReportFileHeader();
            printReportFileResults();

            Dictionary<string, List<int>> missingNames = filterMissingItemNames();
            writeMissingStringWZNames(missingNames);

            Console.WriteLine("Done!");
        } catch (Exception e) {
            Log.Logger.Error(e.ToString());
        }
    }
}
