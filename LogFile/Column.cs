using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogFile
{
    public class Column
    {
        private  string name;
        private  string defaultValue;
        private string type;
        private string value;
    

        public Column()
        {
        }
        public Column(string name,string type, string defaultValue)
        {
            this.name = name;
            this.defaultValue = defaultValue;
            this.type = type;

        }
        public Column(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
        public void  setName(string name)
        {
            this.name = name;
        }
        public void setValue(string value)
        {
            this.value = value;
        }
        public string getName()
        {
            return name;
        }
        public string getType()
        {
            return type;
        }
        public string getDefaultValue()
        {
            return defaultValue;
        }
        public string getValue()
        {
            return value;
        }
    }
}
