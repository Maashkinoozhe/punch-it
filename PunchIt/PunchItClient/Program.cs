using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PIExport;
using PIModel;
using PIModelFileAccess;
using PunchItClient.Actions;
using PunchItClient.PersistenceBackend;

namespace PunchItClient
{
    class Program
    {
        private static DataAccess _dataAccess = new DataAccess();

        static void Main(string[] args)
        {
            UserInterface.Init();
            var showHelpAction = new ShowHelp(null,null,null,null,0,"-h","--help");

            if (args.Length > 0)
            {
                if (showHelpAction.MatchesIdentifier(args[0]))
                {
                    showHelpAction.Run();
                }
                else if (args[0] == "-e" || args[0] == "--export")
                {
                    ExportAction();
                }

                else if ((args[0] == "-s" || args[0] == "--start-work") && args.Length == 2)
                {
                    StartWork(args[1]);
                }
            }
            else
            {
                UserInteraction();
            }

            return;
        }

        private static void UserInteraction()
        {
            var cachedPermissions = _dataAccess.DataWritePermission;
            _dataAccess.DataWritePermission = new DataWritePermission(true, true, true);

            // This listing represents the Main workflow
            State state = _dataAccess.GetState() ?? _dataAccess.CreateState();

            Project currentProject = null;
            while (currentProject == null)
            {
                currentProject = (_dataAccess.GetCurrentProject(state) ?? CreateProject(state, 1)) ?? SelectExistingProject(state);
            }

            Record currentRecord = _dataAccess.GetCurrentRecord(state) ?? _dataAccess.CreateNewRecord(state);

            //Users can start logging work now
            HandleUserInteraction(state, currentProject, currentRecord);

            _dataAccess.DataWritePermission = cachedPermissions;
        }

        public static void ExportAction()
        {
            var cachedPermissions = _dataAccess.DataWritePermission;
            _dataAccess.DataWritePermission = new DataWritePermission(false, false, false);

            State state = _dataAccess.CreateState();
            var project = SelectExistingProject(state);

            _dataAccess.GetCurrentRecord(state);

            var days = SelectTimeSpan();
            var aggregatePackages = UserInterface.GetUserConfirmation(1, "Aggreagate Packages per Day?");

            var exporter = new Exporter(project, DateTime.Now - TimeSpan.FromDays(days), DateTime.Now);
            exporter.AggregatePackages = aggregatePackages;
            var export = exporter.CreateExport();

            var manager = new ExportManager();
            manager.SaveExport(state, project, export);

            _dataAccess.DataWritePermission = cachedPermissions;
        }

        public static void StartWork(string packageKey)
        {
            var cachedPermissions = _dataAccess.DataWritePermission;
            _dataAccess.DataWritePermission = new DataWritePermission(false, false, true);

            if (string.IsNullOrEmpty(packageKey))
            {
                UserInterface.Print("No Package defined");
                return;
            }

            State state = _dataAccess.GetState();

            if (state == null)
            {
                UserInterface.Print("Program seems to have never been started.");
                return;
            }
            Project project = _dataAccess.GetCurrentProject(state);

            if (project == null)
            {
                UserInterface.Print("No Project selected");
                return;
            }

            Record currentRecord = _dataAccess.GetCurrentRecord(state) ?? _dataAccess.CreateNewRecord(state);

            if (!currentRecord.RecordEntries.Any())
            {
                Package packageObject = project.Packages.FirstOrDefault(p => p.Key == packageKey);
                if (packageObject == null)
                {
                    UserInterface.Print("Package key <" + packageKey + "> is unknown.");
                    return;
                }
                _dataAccess.StartWorkOnPackage(state, currentRecord, packageObject);
            }

            _dataAccess.DataWritePermission = cachedPermissions;
        }

        private static int SelectTimeSpan()
        {
            var days = UserInterface.GetUserInt(1,
                "What timespan do you want to export(going back, starting now)?",
                x => x > 0,
                "only positive numbers",
                " -> days: ");

            return days;
        }

        private static Project CreateProject(State state, int indent)
        {
            Project project = null;

            UserInterface.Print(indent, "It seems there is no project configured.");
            UserInterface.Print(indent, "");

            var createProject = UserInterface.GetUserConfirmation(indent - 1, "Do you want to create one?");

            if (createProject)
            {
                project = CreateProjectDetails(indent);

            }

            if (project != null)
            {
                _dataAccess.SaveProject(state, project);
                _dataAccess.UpdateCurrentProject(state, project);
            }

            return project;
        }

        private static Project CreateProjectDetails(int indent)
        {
            var key = UserInterface.GetUserString(indent,
                "What is the Key for the Project?",
                Project.IsValidKey,
                "Please use only valid FileName Characters!");

            var project = new Project(key);

            project.Abbreviation = UserInterface.GetUserString(indent, "Enter Project Abbreviation:");
            project.DisplayName = UserInterface.GetUserString(indent, "Enter Project DisplayName:");

            return project;
        }

        private static void HandleUserInteraction(State state, Project currentProject, Record currentRecord)
        {
            var interactionControl = new PrimaryUserInteractionControl(state,currentProject,currentRecord,_dataAccess);
            interactionControl.Run();
        }

        private static Project SelectExistingProject(State state)
        {
            var action = new SelectExistingProject(state, null, null, _dataAccess, 1);
            Project project = action.Run();
            return project;
        }
    }
}
