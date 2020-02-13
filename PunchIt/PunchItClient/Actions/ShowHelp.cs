using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIModel;
using PunchItClient.PersistenceBackend;

namespace PunchItClient.Actions
{
    class ShowHelp : SelectableUserAction<bool>
    {
        public ShowHelp(State state, Project currentProject, Record currentRecord, DataAccess dataAccess, int baseIndent, params string[] identifiers) : base(state, currentProject, currentRecord, dataAccess, baseIndent,identifiers)
        {
        }

        public override bool Run()
        {
            PrintHelp();
            return true;
        }

        private void PrintHelp()
        {
            UserInterface.Print("");
            UserInterface.Print("This is \"Punch It!\"");
            UserInterface.Print("");

            UserInterface.Print("\t-h\t--help\t\t\t Print this help.");

            UserInterface.Print("\t-e\t--export\t\t Run in export mode, export saved records into csv formatted file.");

            UserInterface.Print("\t-s\t--start-work\t\t Start working on a package in the currently)");
            UserInterface.Print("\t\t\t\t\t active project if the day has started(no RecordEntries created so far)");
            UserInterface.Print("\t\tusage\t -s <PackageKey>");
            UserInterface.Print("\t\tusage\t --start-work <PackageKey>");

            UserInterface.Print("");
        }
    }
}
