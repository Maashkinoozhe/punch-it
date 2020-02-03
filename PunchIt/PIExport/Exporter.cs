using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIModel;

namespace PIExport
{
    public class Exporter
    {
        private const char _separator = ';';
        private const string _DateFormat = "dd.MM.yyyy";
        private const string _TimeFormat = "HH:mm:ss";
        private const string _TimeSpanFormat = "HH:mm:ss";

        public Exporter(Project project, DateTime start, DateTime end)
        {
            Project = project;
            End = end;
            Start = start;
            CleanMissingEndTimesInBetween = true;
        }

        public DateTime Start { get; }
        public DateTime End { get; }
        public Project Project { get; }

        public bool AggregatePackages { get; set; }
        public bool CleanMissingEndTimesAtEndOfDay { get; set; }
        public bool CleanMissingEndTimesInBetween { get; set; }

        public Export CreateExport()
        {
            var expo = new Export(DateTime.Now, Project);
            var builder = new StringBuilder();

            CreateHeader(builder);

            var data = Project.Records.SelectMany(r => r.RecordEntries).Where(e=> e.Start.Value.Date >= Start.Date && e.Start.Value.Date <= End.Date).ToList();

            FormatData(builder, data);

            expo.data = builder.ToString();
            return expo;
        }

        private void FormatData(StringBuilder builder, IEnumerable<RecordEntry> data)
        {
            if (AggregatePackages)
            {
                FormatDataAggregatedPackages(builder, data);
            }
            else
            {
                FormatDetailed(builder, data);
            }
        }

        private void FormatDetailed(StringBuilder builder, IEnumerable<RecordEntry> data)
        {
            var entriesSorted = data.OrderBy(e => e.Start.Value).ToList();
            CleanEntries(entriesSorted);

            foreach (var entry in entriesSorted)
            {
                var endTime = entry.End.HasValue ? entry.End.Value : (entry.Start.Value.Date.Equals(DateTime.Now.Date) ? DateTime.Now : entry.Start.Value);
                CreateEntry(builder, entry.Start.Value, endTime, entry.PackageKey);
            }
        }

        private void FormatDataAggregatedPackages(StringBuilder builder, IEnumerable<RecordEntry> data)
        {
            CleanEntries(data.OrderBy(e=>e.Start.Value).ToList());

            var entriesDayGrouped = data.GroupBy(e => e.Start.Value.Date).OrderBy(g => g.First().Start.Value.Date);

            foreach (var day in entriesDayGrouped)
            {
                foreach (var packageGroup in day.GroupBy(g => g.PackageKey))
                {
                    var date = packageGroup.First().Start.Value.Date;
                    var delta = TimeSpan.FromSeconds(packageGroup.Sum(e => (int) e.Duration.TotalSeconds));
                    CreateEntry(builder,date,date + delta,packageGroup.Key);
                }
            }
        }

        private void CleanEntries(List<RecordEntry> entriesSorted)
        {
            for (int i = 0; i < entriesSorted.Count; i++)
            {
                var entry = entriesSorted[i];
                var nextEntry = (i < entriesSorted.Count - 1) ? entriesSorted[i + 1] : null;

                if (!entry.End.HasValue)
                {
                    if (nextEntry != null && nextEntry.Start.Value.Date == entry.Start.Value.Date && CleanMissingEndTimesInBetween)
                    {
                        entry.End = nextEntry.Start;
                    }
                }
                else
                {
                    if (CleanMissingEndTimesAtEndOfDay)
                    {
                        entry.End = entry.Start;
                    }
                }
            }

        }

        private void CreateHeader(StringBuilder builder)
        {
            builder.Append("Day" + _separator);
            builder.Append("Package" + _separator);
            builder.Append("Start" + _separator);
            builder.Append("End" + _separator);
            builder.AppendLine("Duration as hours");
        }

        private void CreateEntry(StringBuilder builder, DateTime start, DateTime end, string package)
        {
            builder.Append(start.ToString(_DateFormat) + _separator);
            builder.Append(package + _separator);
            builder.Append(start.ToString(_TimeFormat) + _separator);
            builder.Append(end.ToString(_TimeFormat) + _separator);
            builder.AppendLine((end - start).TotalHours.ToString("00.00"));
        }
    }
}
