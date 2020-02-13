using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIModel;
using PunchItClient.PersistenceBackend;

namespace PunchItClient.Actions
{
    class StartWorkOnPackage : SelectableUserAction<bool>
    {
        private Package _package { get; set; }

        public StartWorkOnPackage(State state, Project currentProject, Record currentRecord, DataAccess dataAccess, int baseIndent) : base(state, currentProject, currentRecord, dataAccess, baseIndent, Enumerable.Range(0,currentProject.Packages.Count -1).Select(x=>x.ToString()).ToArray())
        {
        }

        public void SetPackage(Package package)
        {
            _package = package;
        }

        public override bool Run()
        {
            if (_package == null)
            {
                UserInterface.Print(BaseIndent,"Package not defined");
                return false;
            }

            DataAccess.StartWorkOnPackage(State, CurrentRecord, _package);
            SetPackage(null);
            return true;
        }
    }
}
