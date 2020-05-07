using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogFile
{
    public class Table
    {
        public string name;
        public List<Column> columns;

        public Table(string name, List<Column> columns)
        {
            this.name = name;
            this.columns= columns;
        }
        public string getName()
        {
            return name;
        }
        public List<Column> getColumns()
        {
            return columns;
        }
    }
}
