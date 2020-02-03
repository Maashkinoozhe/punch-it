using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIFilePersistence;
using PIModel;

namespace PIModelFileAccess
{
    public class StateManager : ModelManager<State>
    {
        public State LoadState(string path)
        {
            var file = FilePattern.GetFileName(FilePattern.StateFile, FilePattern.ConfigFileEnding);
            return Load(path, file);
        }

        public void SaveState(State state)
        {
            var file = FilePattern.GetFileName(state.GetFileName(), FilePattern.ConfigFileEnding);
            Save(state.RootPath, file,state);
        }
    }
}
