using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIFilePersistence;
using PIModel;

namespace PIModelFileAccess
{
    public class ProjectManager : ModelManager<Project>
    {
        public Project LoadProject(State state, string projectKey)
        {
            var root = state.RootPath;
            var path = FolderPattern.GetPathForProjectRoot(root); ;
            var file = FilePattern.GetFileName(projectKey, FilePattern.ProjectFileEnding);
            return Load(path, file);
        }

        public List<Project> LoadAllProject(State state)
        {
            var projects = new List<Project>();
            var root = state.RootPath;
            var path = FolderPattern .GetPathForProjectRoot( root);

            if (!DoesFolderExist(path)) return projects;

            var files = GetProjectFileNames(path);

            foreach (var file in files)
            {
                var filename = file.Split('\\');
                var data = Load(path, filename.Last());
                if (data != null)
                {
                    projects.Add(data);
                }
            }

            return projects;
        }

        public void SaveProject(State state,Project project)
        {
            var file = FilePattern.GetFileName(project.GetFileName(), FilePattern.ProjectFileEnding);
            Save(FolderPattern.GetPathForProjectRoot(state.RootPath), file, project);
        }

        private List<string> GetProjectFileNames(string path)
        {
            var files = System.IO.Directory.EnumerateFiles(path).Where(x => x.EndsWith(FilePattern.ProjectFileEnding)).ToList();
            return files;
        }
    }
}
