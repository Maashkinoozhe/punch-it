using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIFilePersistence;

namespace PIModel
{
    public class Record:PersistentObjectBase
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

        public override string GetFileName()
        {
            return StartOfRecord.ToString(FilePattern.DateFormat);
        }
    }
}
