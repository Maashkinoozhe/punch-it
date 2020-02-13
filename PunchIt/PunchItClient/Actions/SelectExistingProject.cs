using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIModel;
using PunchItClient.PersistenceBackend;

namespace PunchItClient.Actions
{
    class SelectExistingProject : SelectableUserAction<Project>
    {
        public SelectExistingProject(State state, Project currentProject, Record currentRecord, DataAccess dataAccess, int baseIndent,params string[] identifiers) : base(state, currentProject, currentRecord, dataAccess, baseIndent,identifiers)
        {
        }

        public override Project Run()
        {
            var selectedProject = SelectProject();
            SafeProject(selectedProject);
            return selectedProject;
        }

        private  Project SelectProject()
        {
            var projects = this.DataAccess.GetProjects(State);
            Project selectedProject = null;
            if (projects.Any())
            {
                while (selectedProject == null)
                {
                    UserInterface.Print(BaseIndent, "Select an existing Project:");
                    projects.ForEach(x => UserInterface.Print(BaseIndent + 1, $"#{projects.IndexOf(x)} - {x.Key}, {x.Abbreviation}, {x.DisplayName}"));
                    var index = UserInterface.GetUserInt(BaseIndent - 1,
                        "",
                        value => value >= 0 && value < projects.Count,
                        $"Only numbers between {0} and {projects.Count - 1} are valid!",
                        " -> #");
                    selectedProject = projects[index];
                }
            }

            return selectedProject;
        }

        private void SafeProject(Project selectedProject)
        {
            DataAccess.UpdateCurrentProject(State, selectedProject);
        }
    }
}
