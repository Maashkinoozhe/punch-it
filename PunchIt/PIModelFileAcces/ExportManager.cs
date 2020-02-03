using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIFilePersistence;
using PIModel;

namespace PIModelFileAccess
{
   public class ExportManager
    {
        public void SaveExport(State state, Project project,Export export)
        {
            var root = state.RootPath;
            var path = FolderPattern .GetPathForProject(root ,project.GetFileName());
            var file = FilePattern.GetFileName(export.GetFileName(), FilePattern.DataExportFileEnding);

            Write(path, file,export.data);
        }

        private void Write(string path, string file, string exportData)
        {
            if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
            if (System.IO.File.Exists(path  + file)) System.IO.File.Delete(path +file);
            System.IO.File.WriteAllText(path + file,exportData);
        }
    }
}
