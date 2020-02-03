using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIFilePersistence;

namespace PIModel
{
    public class Export
    {
        public Export(DateTime dateOfCreation, Project project)
        {
            DateOfCreation = dateOfCreation;
            Project = project;
        }

        public DateTime DateOfCreation { get; }
        public Project Project { get; }
        public string data { get; set; }

        public string GetFileName()
        {
            return Project.DisplayName + "_" + DateOfCreation.ToString(FilePattern.DateFormat);
        }
    }
}
