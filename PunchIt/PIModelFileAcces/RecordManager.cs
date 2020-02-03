using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIFilePersistence;
using PIModel;

namespace PIModelFileAccess
{
    public class RecordManager : ModelManager<Record>
    {
        public List<Record> LoadRecords(State state)
        {
            var records = new List<Record>();
            if (state?.CurrentProject == null) return records;

            var root = state.RootPath;
            var path = FolderPattern.GetPathForProject(root, state.CurrentProject.GetFileName());

            if (!DoesFolderExist(path)) return records;

            var files = GetRecordFileNames(path);

            foreach (var filepath in files)
            {
                var filename = filepath.Split('\\').Last();
                var data = Load(path, filename);
                if (data != null)
                {
                    records.Add(data);
                }
            }

            state.CurrentProject.Records = records;
            return records;
        }

        public void SaveRecords(State state, Project project)
        {
            var root = state.RootPath;
            var pathForProject = FolderPattern.GetPathForProject(root, project.GetFileName());
            foreach (var record in state.CurrentProject.Records)
            {
                SaveRecordInternal(pathForProject,  record);
            }
        }

        public void SaveRecord(State state, Project project, Record record)
        {
            var root = state.RootPath;
            var pathForProject = FolderPattern.GetPathForProject(root, project.GetFileName());
            SaveRecordInternal(pathForProject, record);
        }

        private void SaveRecordInternal(string pathForProject, Record record)
        {
            var file = FilePattern.GetFileName(record.GetFileName(), FilePattern.DataFileEnding);
            Save(pathForProject, file, record);
        }

        private List<string> GetRecordFileNames(string path)
        {
            var files = System.IO.Directory.EnumerateFiles(path).Where(x => x.EndsWith(FilePattern.DataFileEnding)).ToList();
            return files;
        }
    }
}
