using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIFilePersistence
{
    public class FolderPattern
    {
        public static string ProjectRootFolder = "project";
        public static string ProjectFolder = "{0}";

        public static string GetPathForProject(string root, string projectName)
        {
            return root + ProjectRootFolder + "\\" + String.Format(ProjectFolder, projectName) + "\\";
        }
        public static string GetPathForProjectRoot(string root)
        {
            return root + ProjectRootFolder + "\\" ;
        }
    }
}
