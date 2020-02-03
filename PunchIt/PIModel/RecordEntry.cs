using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIModel
{
    public class RecordEntry
    {
        public RecordEntry(string packageKey)
        {
            PackageKey = packageKey;
        }

        public string PackageKey { get; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public TimeSpan Duration
        {
            get
            {
                if (Start.HasValue && End.HasValue)
                {
                    return End.Value - Start.Value;
                }
                else if (Start.HasValue)
                {
                    return DateTime.Now - Start.Value;
                }
                return TimeSpan.Zero;
            }
        }

        public bool Closed()
        {
            return End.HasValue;
        }
    }
}
