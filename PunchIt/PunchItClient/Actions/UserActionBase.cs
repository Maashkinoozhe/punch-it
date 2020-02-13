using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIModel;
using PunchItClient.PersistenceBackend;

namespace PunchItClient
{
    public abstract class UserActionBase<Result>
    {
        protected State State { get; set; }
        protected Project CurrentProject { get; set; }
        protected Record CurrentRecord { get; set; }
        protected DataAccess DataAccess { get; set; }
        public int BaseIndent { get; }

        protected UserActionBase(State state, Project currentProject, Record currentRecord, DataAccess dataAccess,int baseIndent)
        {
            State = state;
            CurrentProject = currentProject;
            CurrentRecord = currentRecord;
            DataAccess = dataAccess;
            BaseIndent = baseIndent;
        }

        public abstract Result Run();
    }
}
