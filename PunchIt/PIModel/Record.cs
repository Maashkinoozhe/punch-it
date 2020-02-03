using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIFilePersistence;

namespace PIModel
{
    public class Record : PersistentObjectBase
    {
        public Record(DateTime startOfRecord)
        {
            StartOfRecord = startOfRecord;
            RecordEntries = new List<RecordEntry>();
        }

        public DateTime StartOfRecord { get; }
        public List<RecordEntry> RecordEntries;

        public RecordEntry LastOpenEntry()
        {
            return RecordEntries.LastOrDefault(x => !x.Closed());
        }

        public TimeSpan GetWorkingTime(Project project, bool countRelevantTimes, bool countIrrelevantTimes)
        {
            var time = TimeSpan.Zero;
            foreach (var entry in RecordEntries)
            {
                var package = project.Packages.FirstOrDefault(p => p.Key == entry.PackageKey);
                if (package == null)
                {
                    //Package is considered relevant
                    IncreaseDuration(entry, ref time);
                }
                else
                {
                    if ((countRelevantTimes && package.RelevantForTimeTracking) || (countIrrelevantTimes && !package.RelevantForTimeTracking))
                    {
                        IncreaseDuration(entry, ref time);
                    }
                }
            }
            return time;
        }

        public TimeSpan GetWorkingTime()
        {
            var time = TimeSpan.Zero;
            foreach (var entry in RecordEntries)
            {
                //Package is considered relevant
                IncreaseDuration(entry, ref time);
            }
            return time;
        }

        private void IncreaseDuration(RecordEntry entry, ref TimeSpan time)
        {
            if (entry.End.HasValue)
            {
                time += entry.Duration;
            }
            else
            {
                time += DateTime.Now - entry.Start.Value;
            }
        }


        public override string GetFileName()
        {
            return StartOfRecord.ToString(FilePattern.DateFormat);
        }
    }
}
