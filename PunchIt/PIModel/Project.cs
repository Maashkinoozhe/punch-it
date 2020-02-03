using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PIModel
{
    public class Project : PersistentObjectBase
    {
        public Project(string key)
        {
            Key = key.ToLower();
            Packages = new List<Package>();
        }

        public string Key { get; }
        public string Abbreviation { get; set; }
        public string DisplayName { get; set; }

        [JsonIgnore]
        public List<Record> Records;

        public List<Package> Packages;

        public override string GetFileName()
        {
            return Key;
        }

        public static bool IsValidKey(string key)
        {
            string[] forbiddenChar = {" ", ":","/", "\\", "\"", "\'", "*", "+", "~", ","};
            return !forbiddenChar.Any(x => key.Contains(x));
        }
    }
}
