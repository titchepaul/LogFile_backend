using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogFile
{
    public class TableInfo
    {
        public TableInfo(string name, string type, bool is_nullable)
        {
            this.Name = name;
            this.Type = type;
            this.Nullable = is_nullable;
        }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Nullable { get; set; }
    }
}
