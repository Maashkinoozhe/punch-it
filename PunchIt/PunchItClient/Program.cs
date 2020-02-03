using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PIExport;
using PIModel;
using PIModelFileAccess;

namespace PunchItClient
{
    class Program
    {
        private static DataWritePermission _dataWritePermission;

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "-h" || args[0] == "--help")
                {
                    ShowHelp();
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

        private static void ShowHelp()
        {
            UserInterface.Print("");
            UserInterface.Print("This is \"Punch It!\"");
            UserInterface.Print("");

            UserInterface.Print("\t-h\t--help\t\t Print this help.");

            UserInterface.Print("\t-e\t--export\t\t Run in export mode, export saved records into csv formatted file.");

            UserInterface.Print("\t-s\t--start-work\t\t Start working on a package in the currently active project if the day has started(no RecordEntries created so far)");
            UserInterface.Print("\t\tusage\t -s <PackageKey>)");
            UserInterface.Print("\t\tusage\t --start-work <PackageKey>)");

            UserInterface.Print("");
        }

        private static void UserInteraction()
        {
            var cachedPermissions = _dataWritePermission;
            _dataWritePermission = new DataWritePermission(true,true,true);

            // This listing represents the Main workflow
            State state = GetState() ?? CreateState();

            UserInterface.Print(2, "Welcome to \"Punch It!\"");
            UserInterface.Print("");
            UserInterface.Print("");
            UserInterface.Print("");

            Project currentProject = null;
            while (currentProject == null)
            {
                currentProject = (GetCurrentProject(state) ?? CreateProject(state, 1)) ?? SelectExistingProject(state, 1);
            }

            Record currentRecord = GetCurrentRecord(state) ?? CreateNewRecord(state);

            //Users can start logging work now
            HandleUserInteraction(state, currentProject, currentRecord);

            _dataWritePermission = cachedPermissions;
        }

        private static void ExportAction()
        {
            var cachedPermissions = _dataWritePermission;
            _dataWritePermission = new DataWritePermission(false, false, false);

            State state = CreateState();
            Project project = SelectExistingProject(state, 1);
            GetCurrentRecord(state);

            var days = SelectTimeSpan();

            var exporter = new Exporter(project, DateTime.Now - TimeSpan.FromDays(days), DateTime.Now);
            var export = exporter.CreateExport();

            var manager = new ExportManager();
            manager.SaveExport(state, project, export);

            _dataWritePermission = cachedPermissions;
        }

        private static void StartWork(string packageKey)
        {
            var cachedPermissions = _dataWritePermission;
            _dataWritePermission = new DataWritePermission(false, false, true);

            if (string.IsNullOrEmpty(packageKey))
            {
                UserInterface.Print("No Package defined");
                return;
            }

            State state = GetState();

            if (state== null)
            {
                UserInterface.Print("Program seems to have never been started.");
                return;
            }
            Project project = GetCurrentProject(state);

            if (project == null)
            {
                UserInterface.Print("No Project selected");
                return;
            }

            Record currentRecord = GetCurrentRecord(state) ?? CreateNewRecord(state);

            if (!currentRecord.RecordEntries.Any())
            {
                Package packageObject = project.Packages.FirstOrDefault(p => p.Key == packageKey);
                if (packageObject == null)
                {
                    UserInterface.Print("Package key <" + packageKey + "> is unknown.");
                    return;
                }
                StartWorkOnPackage(state,currentRecord,packageObject);
            }

            _dataWritePermission = cachedPermissions;
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

        private static State GetState()
        {
            var manager = new StateManager();
            var state = manager.LoadState(System.IO.Directory.GetCurrentDirectory() + "\\");
            return state;
        }

        private static State CreateState()
        {
            var state = new State(null);
            SaveState(state);
            return state;
        }

        private static void SaveState(State state)
        {
            if (_dataWritePermission.UpdateState)
            {
                var manager = new StateManager();
                manager.SaveState(state);
            }
        }

        private static Project GetCurrentProject(State state)
        {
            var manager = new ProjectManager();
            var project = manager.LoadProject(state, state.CurrentProjectKey);

            if (project != null) UpdateCurrentProject(state, project);

            return project;
        }

        private static Project SelectExistingProject(State state, int indent)
        {
            var projects = GetProjects(state);
            Project selectedProject = null;
            if (projects.Any())
            {
                while (selectedProject == null)
                {
                    UserInterface.Print(indent, "Select an existing Project:");
                    projects.ForEach(x => UserInterface.Print(indent + 1, $"#{projects.IndexOf(x)} - {x.Key}, {x.Abbreviation}, {x.DisplayName}"));
                    var index = UserInterface.GetUserInt(indent - 1,
                        "",
                        value => value >= 0 && value < projects.Count,
                        $"Only numbers between {0} and {projects.Count - 1} are valid!",
                        " -> #");
                    selectedProject = projects[index];
                }
            }

            UpdateCurrentProject(state, selectedProject);
            return selectedProject;
        }

        private static List<Project> GetProjects(State state)
        {
            var manager = new ProjectManager();
            var projects = manager.LoadAllProject(state);
            return projects;
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
                SaveProject(state, project);
                UpdateCurrentProject(state, project);
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

        private static void UpdateCurrentProject(State state, Project project)
        {
            state.SetCurrentProject(project);
            SaveState(state);
        }

        private static void SaveProject(State state, Project project)
        {
            if (_dataWritePermission.UpdateProject)
            {
                var manager = new ProjectManager();
                manager.SaveProject(state, project);
            }
        }

        private static Record GetCurrentRecord(State state)
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

        private static Record CreateNewRecord(State state)
        {
            var record = new Record(DateTime.Now);

            if (state.CurrentProject.Records == null) state.CurrentProject.Records = new List<Record>();
            state.CurrentProject.Records.Add(record);

            SaveRecord(state, record);
            return record;
        }

        private static void SaveRecord(State state, Record record)
        {
            if (_dataWritePermission.UpdateRecord)
            {
                var manager = new RecordManager();
                manager.SaveRecord(state, state.CurrentProject, record);
            }
        }

        private static void HandleUserInteraction(State state, Project currentProject, Record currentRecord)
        {
            var stop = false;
            while (!stop)
            {
                // List Actions
                ListActions(2, currentProject, currentRecord);

                var answer = UserInterface.GetUserString(1, "Select on what to do");
                int number;

                if (int.TryParse(answer, out number) && number >= 0 && number < currentProject.Packages.Count)
                {
                    StartWorkOnPackage(state, currentRecord, currentProject.Packages[number]);
                    stop = true;
                }
                else if (answer.Equals(","))
                {
                    StopWorkOnOpenPackage(state, currentRecord);
                    stop = true;
                }
                else if (answer.Equals("+"))
                {
                    CreateNewPackage(state, currentProject);
                }
                else if (answer.Equals("-"))
                {
                    DeletePackage(state, currentProject);
                }
                else if (answer.Equals("*"))
                {
                    currentProject = SwitchProject(state);
                }
                else if (answer.Equals("e"))
                {
                    ExportAction();
                }
                else if (answer.Equals("q"))
                {
                    stop = true;
                }
            }
        }

        private static void ListActions(int indent, Project currentProject, Record currentRecord)
        {
            var lastOpenEntry = currentRecord.LastOpenEntry();

            UserInterface.Print(indent, "What do you want to do?");
            UserInterface.Print("");

            if (lastOpenEntry != null)
            {
                var currentWorkingTime = (DateTime.Now - lastOpenEntry.Start).Value;
                UserInterface.Print(indent, $"You are currently working on - [ {lastOpenEntry.PackageKey} ] for {currentWorkingTime.Hours:#0} hours {currentWorkingTime.Minutes:#0.#} minutes");
                UserInterface.Print("");
            }

            //Start working on a package
            foreach (Package package in currentProject.Packages)
            {
                UserInterface.Print(indent + 1, $"> {currentProject.Packages.IndexOf(package)} < \t start working on - [ {package.Abbreviation}, \t {package.DisplayName} ]");
            }
            UserInterface.Print("");

            if (lastOpenEntry != null)
            {
                //stop working on open packages
                UserInterface.Print(indent + 1, $"> , < \t STOP working on the last open Package [ {lastOpenEntry.PackageKey} ]");
                UserInterface.Print("");
            }

            //create new package
            UserInterface.Print(indent + 1, $"> + < \t create new package");

            if (currentProject.Packages.Count > 0)
            {
                //delete a package
                UserInterface.Print(indent + 1, $"> - < \t delete a package(record will be kept)");
            }

            //switch project
            UserInterface.Print(indent + 1, $"> * < \t switch project");

            //export project
            UserInterface.Print(indent + 1, $"> e < \t export a project");

            //end Punch It!
            UserInterface.Print(indent + 1, $"> q < \t quit \"Punch It!\"");

            UserInterface.Print("");
        }

        private static void StartWorkOnPackage(State state, Record record, Package package)
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

        private static void StopWorkOnOpenPackage(State state, Record record)
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

        private static void CreateNewPackage(State state, Project currentProject)
        {
            var newPackage = CreatePackageDetails(currentProject.Packages);
            currentProject.Packages.Add(newPackage);
            SaveProject(state, currentProject);
        }

        private static Package CreatePackageDetails(List<Package> knownPackages)
        {
            var package = new Package();

            var question = "What is the Project KEY?";
            Func<string, bool> validator = x => !knownPackages.Any(y => y.Key.Equals(x));
            var allowedInputs = $"The following Keys are already in use:\n{string.Join("\n", knownPackages.Select(pack => pack.Key))}";
            package.Key = UserInterface.GetUserString(1, question, validator, allowedInputs);

            question = "What is the Project Abbreviation?";
            validator = x => !knownPackages.Where(p => !string.IsNullOrEmpty(p.Abbreviation)).Any(y => y.Abbreviation.Equals(x));
            allowedInputs = $"The following Abbreviations are already in use:\n{string.Join("\n", knownPackages.Select(pack => pack.Abbreviation))}";
            package.Abbreviation = UserInterface.GetUserString(1, question, validator, allowedInputs);

            question = "What is the Project DisplayName?";
            validator = x => !knownPackages.Where(p => !string.IsNullOrEmpty(p.DisplayName)).Any(y => y.DisplayName.Equals(x));
            allowedInputs = $"The following Names are already in use:\n{string.Join("\n", knownPackages.Select(pack => pack.DisplayName))}";
            package.DisplayName = UserInterface.GetUserString(1, question, validator, allowedInputs);

            return package;
        }

        private static void DeletePackage(State state, Project currentProject)
        {
            var index = UserInterface.GetUserInt(2,
                "Which package do you want to delete?(records referencing this package will NOT be deleted)",
                x => x >= 0 && x < currentProject.Packages.Count, "Use one of the listed packages numbers");
            currentProject.Packages.RemoveAt(index);
            SaveProject(state, currentProject);
        }

        private static Project SwitchProject(State state)
        {
            var project = SelectExistingProject(state, 0);
            var currentRecord = GetCurrentRecord(state) ?? CreateNewRecord(state);
            return project;
        }

        private class DataWritePermission
        {
            public DataWritePermission(bool updateState, bool updateProject, bool updateRecord)
            {
                UpdateState = updateState;
                UpdateProject = updateProject;
                UpdateRecord = updateRecord;
            }

            public bool UpdateState { get; set; }
            public bool UpdateProject  {get;set;}
            public bool UpdateRecord { get; set; }
        }
    }
}
