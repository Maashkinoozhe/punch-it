using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIModel;
using PunchItClient.Actions;
using PunchItClient.PersistenceBackend;

namespace PunchItClient
{
    public class PrimaryUserInteractionControl
    {
        public State State { get; set; }
        private Project CurrentProject { get; set; }
        private Record CurrentRecord { get; set; }
        private DataAccess DataAccess { get; set; }

        public PrimaryUserInteractionControl(State state, Project currentProject, Record currentRecord, DataAccess dataAccess)
        {
            State = state;
            CurrentProject = currentProject;
            CurrentRecord = currentRecord;
            DataAccess = dataAccess;
        }

        public void Run()
        {

            var stop = false;
            while (!stop)
            {
                var numberOfPackages = CurrentProject.Packages.Count;

                var actionStartWork = new StartWorkOnPackage(State, CurrentProject, CurrentRecord, DataAccess, 0);

                //List current work state
                ListCurrentWorkState(2, CurrentProject, CurrentRecord, CurrentRecord.LastOpenEntry());
                //List actions
                ListActions(2, CurrentProject, CurrentRecord, CurrentRecord.LastOpenEntry(),actionStartWork);

                var answer = UserInterface.GetUserChar(1, "Select on what to do", null, x => StopReadUserInteraction(x, numberOfPackages), "please type sensible things > ? <", numberOfPackages.ToString().Length);

                answer.TrimEnd('\r').TrimEnd('\n');
                
                if (actionStartWork.MatchesIdentifier(answer) && int.TryParse(answer, out var number))
                {
                    actionStartWork.SetPackage(CurrentProject.Packages[number]);
                    actionStartWork.Run();
                    stop = true;
                }
                else if (answer.Equals(","))
                {
                    DataAccess.StopWorkOnOpenPackage(State, CurrentRecord);
                    stop = true;
                }
                else if (answer.Equals("+"))
                {
                    CreateNewPackage(State, CurrentProject);
                }
                else if (answer.Equals("-"))
                {
                    DeletePackage(State, CurrentProject);
                }
                else if (answer.Equals("*"))
                {
                    CurrentProject = SwitchProject(State);
                }
                else if (answer.Equals("l"))
                {
                    ListTodaysActivities(State, CurrentProject, CurrentRecord);
                }
                else if (answer.Equals("e"))
                {
                    EditRecordEntries(State, CurrentProject, CurrentRecord);
                }
                else if (answer.Equals("x"))
                {
                    Program.ExportAction();
                }
                else if (answer.Equals("q"))
                {
                    stop = true;
                }

                if (!stop) UserInterface.ClearConsole();
            }
        }

        private void ListCurrentWorkState(int indent, Project currentProject, Record currentRecord, RecordEntry lastOpenEntry)
        {
            var indentInfo = indent - 1;

            if (currentRecord.RecordEntries.Any())
            {
                var currentDay = currentRecord.GetWorkingTime();
                var currentWorkDay = currentRecord.GetWorkingTime(currentProject, true, false);
                var currentWorkDayPause = currentRecord.GetWorkingTime(currentProject, false, true);
                var restWorkDay = TimeSpan.FromHours(8) - currentWorkDay;

                UserInterface.Print(indentInfo, $"Your day so far              - [ ALL   ] --> {currentDay.Hours:00}:{currentDay.Minutes:00} (hh:mm)");
                UserInterface.Print(indentInfo, $"Your work day so far         - [ WORK  ] --> {currentWorkDay.Hours:00}:{currentWorkDay.Minutes:00}");
                UserInterface.Print(indentInfo, $"Your work day so far         - [ PAUSE ] --> {currentWorkDayPause.Hours:00}:{currentWorkDayPause.Minutes:00}");
                UserInterface.Print("");
                UserInterface.Print(indentInfo, $"Rest of your working hours   - [ REST  ] --> {restWorkDay.Hours:00}:{restWorkDay.Minutes:00} ({(DateTime.Now + restWorkDay).ToShortTimeString()} o'clock)");
                UserInterface.Print("");

                if (lastOpenEntry != null)
                {
                    var currentPackage = (lastOpenEntry.Duration);
                    var currentPackageTotal = TimeSpan.FromSeconds(currentRecord.RecordEntries.Where(x => x.PackageKey == lastOpenEntry.PackageKey).Sum(y => y.Duration.TotalSeconds));

                    UserInterface.PrintSameLine(indentInfo, $"You are currently working on - [");
                    UserInterface.PrintSameLine(0, $" {lastOpenEntry.PackageKey} ", ConsoleColor.Red);
                    UserInterface.PrintSameLine(0, $"] --> ");
                    UserInterface.PrintSameLine(0, $"{currentPackage.Hours:00}:{currentPackage.Minutes:00}", ConsoleColor.Yellow);
                    UserInterface.PrintSameLine(0, $" (total: ");
                    UserInterface.PrintSameLine(0, $"{currentPackageTotal.Hours:00}:{currentPackageTotal.Minutes:00}", ConsoleColor.Green);
                    UserInterface.Print(0, $")");
                }

                UserInterface.Print("");
                UserInterface.Print("        -------------------------------------------------------------------------------");
                UserInterface.Print("");
            }

            UserInterface.Print(indentInfo, "What do you want to do?");
            UserInterface.Print("");
        }

        private void ListActions(int indent, Project currentProject, Record currentRecord, RecordEntry lastOpenEntry, StartWorkOnPackage actionStartWork)
        {
            //Start working on a package
            foreach (Package package in currentProject.Packages)
            {
                UserInterface.PrintSameLine(indent + 1, $"> ");
                UserInterface.PrintSameLine(0, $"{currentProject.Packages.IndexOf(package)}", ConsoleColor.Blue);
                UserInterface.Print(0, $" < \t [{(package.RelevantForTimeTracking ? "X" : "O")}| {package.Abbreviation}, \t {package.DisplayName} ]");
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
            UserInterface.Print(indent + 1, $"> x < \t export a project");

            //export project
            UserInterface.Print(indent + 1, $"> l < \t list todays activities");

            //export project
            UserInterface.Print(indent + 1, $"> e < \t edit todays activities");

            //end Punch It!
            UserInterface.Print(indent + 1, $"> q < \t quit \"Punch It!\"");

            UserInterface.Print("");
        }

        private bool StopReadUserInteraction(string input, int NumberOfPackages)
        {
            var allowedLetters = ",+-*eqlx".ToCharArray();
            if (allowedLetters.Any(x => input.Contains(x)))
            {
                return true;
            }

            int number = 0;
            if (int.TryParse(input, out number))
            {
                //It is a number
                if (input.Length >= (NumberOfPackages-1).ToString().Length)
                {
                    return true;
                }
            }
            return false;
        }

        private void CreateNewPackage(State state, Project currentProject)
        {
            var newPackage = CreatePackageDetails(currentProject.Packages);
            currentProject.Packages.Add(newPackage);
            DataAccess.SaveProject(state, currentProject);
        }

        private Package CreatePackageDetails(List<Package> knownPackages)
        {
            var package = new Package();

            var question = "What is the Package KEY?";
            Func<string, bool> validator = x => !knownPackages.Any(y => y.Key.Equals(x));
            var allowedInputs = $"The following Keys are already in use:\n{string.Join("\n", knownPackages.Select(pack => pack.Key))}";
            package.Key = UserInterface.GetUserString(1, question, validator, allowedInputs);

            question = "What is the Package Abbreviation?";
            validator = x => !knownPackages.Where(p => !string.IsNullOrEmpty(p.Abbreviation)).Any(y => y.Abbreviation.Equals(x));
            allowedInputs = $"The following Abbreviations are already in use:\n{string.Join("\n", knownPackages.Select(pack => pack.Abbreviation))}";
            package.Abbreviation = UserInterface.GetUserString(1, question, validator, allowedInputs);

            question = "What is the Package DisplayName?";
            validator = x => !knownPackages.Where(p => !string.IsNullOrEmpty(p.DisplayName)).Any(y => y.DisplayName.Equals(x));
            allowedInputs = $"The following Names are already in use:\n{string.Join("\n", knownPackages.Select(pack => pack.DisplayName))}";
            package.DisplayName = UserInterface.GetUserString(1, question, validator, allowedInputs);

            question = "Is this Package relevant for TimeTracking?";
            package.RelevantForTimeTracking = UserInterface.GetUserConfirmation(1, question);

            return package;
        }

        private void DeletePackage(State state, Project currentProject)
        {
            var index = UserInterface.GetUserInt(2,
                "Which package do you want to delete?(records referencing this package will NOT be deleted)",
                x => x >= 0 && x < currentProject.Packages.Count, "Use one of the listed packages numbers");
            currentProject.Packages.RemoveAt(index);
            DataAccess.SaveProject(state, currentProject);
        }

        private Project SwitchProject(State state)
        {
            var selectAction = new SelectExistingProject(state, null, null, DataAccess, 1);
            var project = selectAction.Run();
            var currentRecord = DataAccess.GetCurrentRecord(state) ?? DataAccess.CreateNewRecord(state);
            return project;
        }

        private void ListTodaysActivities(State state, Project currentProject, Record currentRecord)
        {
            var useAggregation = UserInterface.GetUserConfirmation(1, "Do you want to aggregate packages?");

            ListTodaysActivities(currentProject, currentRecord, useAggregation);
            UserInterface.Print(0, "");
            UserInterface.Print(1, "-------------------------------------");
            UserInterface.Print(0, "");
            PrintTotal(currentProject, currentRecord);
            UserInterface.Print(0, "");
            UserInterface.Print(0, "");

            UserInterface.Pause();
        }

        private void ListTodaysActivities(Project currentProject, Record currentRecord, bool useAggregation)
        {
            if (useAggregation)
            {
                ListTodaysActivitiesAggregated(currentProject, currentRecord);
            }
            else
            {
                ListTodaysActivitiesFull(currentProject, currentRecord);
            }
        }

        private void ListTodaysActivitiesAggregated(Project currentProject, Record currentRecord)
        {
            foreach (var entryGroup in currentRecord.RecordEntries.GroupBy(x => x.PackageKey))
            {
                var duration = TimeSpan.FromMinutes(entryGroup.Sum(x => x.Duration.TotalMinutes));

                UserInterface.PrintSameLine(2, "Total: ");
                UserInterface.PrintSameLine(0, $"{duration.Hours:00}:{duration.Minutes:00}", ConsoleColor.Green);
                UserInterface.PrintSameLine(0, " --> [ ");
                UserInterface.PrintSameLine(0, $"{entryGroup.Key}", ConsoleColor.Red);
                UserInterface.Print(0, " ]");
            }
        }

        private void ListTodaysActivitiesFull(Project currentProject, Record currentRecord)
        {
            foreach (var entry in currentRecord.RecordEntries)
            {
                var start = entry.Start.Value;
                var end = entry.Start.Value.Add(entry.Duration);
                var duration = entry.Duration;

                UserInterface.PrintSameLine(2, "From: ");
                UserInterface.PrintSameLine(0, $"{start.Hour:00}:{start.Minute:00}", ConsoleColor.Yellow);
                UserInterface.PrintSameLine(0, " To: ");
                UserInterface.PrintSameLine(0, $"{end.Hour:00}:{end.Minute:00}", ConsoleColor.Yellow);
                UserInterface.PrintSameLine(0, " Total: ");
                UserInterface.PrintSameLine(0, $"{duration.Hours:00}:{duration.Minutes:00}", ConsoleColor.Green);
                UserInterface.PrintSameLine(0, " --> [ ");
                UserInterface.PrintSameLine(0, $"{entry.PackageKey}", ConsoleColor.Red);
                UserInterface.Print(0, " ]");
            }
        }

        private void PrintTotal(Project currentProject, Record currentRecord)
        {
            var duration = TimeSpan.FromMinutes(currentRecord.RecordEntries.Where(y => (currentProject.Packages.FirstOrDefault(z => z.Key == y.PackageKey) != null) ? (currentProject.Packages.FirstOrDefault(z => z.Key == y.PackageKey).RelevantForTimeTracking) : true).Sum(x => x.Duration.TotalMinutes));

            UserInterface.PrintSameLine(2, "Total: ");
            UserInterface.Print(0, $"{duration.Hours:00}:{duration.Minutes:00}", ConsoleColor.Cyan);
        }

        private void EditRecordEntries(State state, Project currentProject, Record currentRecord)
        {
            var adapter = new EditRecordsInteractiveMode(currentRecord);
            UserInterface.RunUserInteractiveMode(adapter);
            DataAccess.SaveRecord(state, currentRecord);
        }


    }
}
