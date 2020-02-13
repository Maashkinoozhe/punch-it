using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIModel;
using PIModelFileAccess;

namespace PunchItClient.PersistenceBackend
{
    public class DataAccess
    {
        internal DataWritePermission DataWritePermission { get; set; }

        public State GetState()
        {
            var manager = new StateManager();
            var state = manager.LoadState(System.IO.Directory.GetCurrentDirectory() + "\\");
            return state;
        }

        public State CreateState()
        {
            var state = new State(null);
            SaveState(state);
            return state;
        }

        public void SaveState(State state)
        {
            if (DataWritePermission.UpdateState)
            {
                var manager = new StateManager();
                manager.SaveState(state);
            }
        }

        public Project GetCurrentProject(State state)
        {
            var manager = new ProjectManager();
            var project = manager.LoadProject(state, state.CurrentProjectKey);

            if (project != null) UpdateCurrentProject(state, project);

            return project;
        }

        public List<Project> GetProjects(State state)
        {
            var manager = new ProjectManager();
            var projects = manager.LoadAllProject(state);
            return projects;
        }

        public void UpdateCurrentProject(State state, Project project)
        {
            state.SetCurrentProject(project);
            SaveState(state);
        }

        public void SaveProject(State state, Project project)
        {
            if (DataWritePermission.UpdateProject)
            {
                var manager = new ProjectManager();
                manager.SaveProject(state, project);
            }
        }

        public Record GetCurrentRecord(State state)
        {
            var manager = new RecordManager();
            var records = manager.LoadRecords(state);

            Record currentRecord = null;
            var mostRecentRecord = records?.LastOrDefault();

            if (mostRecentRecord != null &&
                mostRecentRecord.StartOfRecord.Year == DateTime.Now.Year &&
                mostRecentRecord.StartOfRecord.DayOfYear == DateTime.Now.DayOfYear)
            {
                currentRecord = mostRecentRecord;
            }

            return currentRecord;
        }

        public Record CreateNewRecord(State state)
        {
            var record = new Record(DateTime.Now);

            if (state.CurrentProject.Records == null) state.CurrentProject.Records = new List<Record>();
            state.CurrentProject.Records.Add(record);

            SaveRecord(state, record);
            return record;
        }

        public void SaveRecord(State state, Record record)
        {
            if (DataWritePermission.UpdateRecord)
            {
                var manager = new RecordManager();
                manager.SaveRecord(state, state.CurrentProject, record);
            }
        }

        public void StartWorkOnPackage(State state, Record record, Package package)
        {
            var openEntry = record.LastOpenEntry();

            if (openEntry != null)
            {
                //update open Entry
                openEntry.End = DateTime.Now;
            }

            if (openEntry == null || !openEntry.PackageKey.Equals(package.Key))
            {
                //start new Entry
                var newEntry = new RecordEntry(package.Key);
                newEntry.Start = DateTime.Now;

                //register Record with new Entry
                record.RecordEntries.Add(newEntry);
            }
            else
            {
                //nothing to do, user is already working on this package
                UserInterface.Print($"You are already working on [ {package.DisplayName} ]");
                return;
            }

            //Update Record
            SaveRecord(state, record);
        }

        public void StopWorkOnOpenPackage(State state, Record record)
        {
            var openEntry = record.LastOpenEntry();

            if (openEntry != null)
            {
                //update open Entry
                openEntry.End = DateTime.Now;
            }

            //Update Record
            SaveRecord(state, record);
        }
    }
}
