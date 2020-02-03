using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIFilePersistence
{
    public class FilePattern
    {
        public static string StateFile = "config";
        public static string ConfigFileEnding = "ini";
        public static string ProjectFileEnding = "project";
        public static string DataFileEnding = "save";
        public static string DataExportFileEnding = "csv";

        public static string DateFormat = "yyyy-MM-dd_HH-mm-ss";

        public static string GetFileName(string fileName, string fileEnding)
        {
            return fileName + "." + fileEnding;
        }
    }
}
