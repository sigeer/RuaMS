/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace XmlWzReader.wz;

public class XMLWZFileProvider : DataProvider
{
    private string root;
    private WZDirectoryEntry rootForNavigation;

    public XMLWZFileProvider(string fileIn)
    {
        root = fileIn;
        rootForNavigation = new WZDirectoryEntry(Path.GetFileName(fileIn), 0, 0, null);
        fillMapleDataEntitys(root, rootForNavigation);
    }

    private void fillMapleDataEntitys(string lroot, WZDirectoryEntry wzdir)
    {

        if (!Directory.Exists(lroot))
            return;
        try
        {
            var allFiles = Directory.GetFiles(lroot).Concat(Directory.GetDirectories(lroot));
            foreach (var file in allFiles)
            {
                string fileName = Path.GetFileName(file);
                if (Directory.Exists(file) && !fileName.EndsWith(".img"))
                {
                    WZDirectoryEntry newDir = new WZDirectoryEntry(fileName, 0, 0, wzdir);
                    wzdir.addDirectory(newDir);
                    fillMapleDataEntitys(file, newDir);
                }
                else if (fileName.EndsWith(".xml"))
                {
                    wzdir.addFile(new WZFileEntry(fileName.Substring(0, fileName.Length - 4), 0, 0, wzdir));
                }
            }
        }
        catch (IOException e)
        {
            throw new Exception("Can not open file/directory at " + Path.GetFullPath(lroot), e);
        }
    }

    private object readFileLock = new object();
    public Data getData(string path)
    {
        lock (readFileLock)
        {
            try
            {
                var dataFile = Path.Combine(root, path + ".xml");
                using var fis = new FileStream(dataFile, FileMode.Open);
                return new XMLDomMapleData(fis);
            }
            catch (FileNotFoundException)
            {
                throw new Exception("Datafile " + path + " does not exist in " + Path.GetFullPath(root));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public DataDirectoryEntry getRoot()
    {
        return rootForNavigation;
    }
}