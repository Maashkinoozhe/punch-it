using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIModel;
using PunchItClient.PersistenceBackend;

namespace PunchItClient.Actions
{
    public abstract class SelectableUserAction<Result> : UserActionBase<Result>
    {
        protected List<string> Identifiers;

        protected SelectableUserAction(State state, Project currentProject, Record currentRecord, DataAccess dataAccess,int baseIndent, params string[] identifiers) : base(state: state, currentProject: currentProject, currentRecord: currentRecord, dataAccess: dataAccess,baseIndent: baseIndent)
        {
            Identifiers = new List<string>();
            SetIdentifiers(arg: identifiers);
        }

        public string[] GetIdentifiers()
        {
            return Identifiers.ToArray();
        }

        public bool MatchesIdentifier(string identifier)
        {
            return Identifiers.Contains(identifier);
        }

        protected void SetIdentifiers(params string[] arg)
        {
            foreach (var identifier in arg)
            {
                Identifiers.Add(identifier);
            }
        }
    }
}
