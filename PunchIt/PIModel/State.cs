using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PIModel
{
    public class State : PersistentObjectBase
    {

        private string _rootPath;
        private Project _currentProject;

        [JsonIgnore]
        public Project CurrentProject => _currentProject;
        public string CurrentProjectKey;

        public State(Project currentProject)
        {
            SetCurrentProject(currentProject);
        }

        public void SetCurrentProject(Project currentProject)
        {
            _currentProject = currentProject;
            CurrentProjectKey = currentProject?.Key;
        }

        public override string GetFileName()
        {
            return "config";
        }

        [JsonIgnore]
        public string RootPath
        {
            get
            {
                if(_rootPath == null) _rootPath = System.IO.Directory.GetCurrentDirectory() + "\\";
                return _rootPath;
            }
        }
    }
}
