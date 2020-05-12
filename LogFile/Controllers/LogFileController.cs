using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace LogFile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class LogFileController : ControllerBase
    {
        private static string connectionString = "Server=VDI-NETC096\\SQLEXPRESS;Database=ServiceLog;Trusted_Connection=True";
        private readonly SqlConnection connect = new SqlConnection(connectionString);

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }
        /// <summary>
        /// return list of the all column_name in dataTable
        /// </summary>
        /// <param name="name">dataTable_name</param>
        /// <returns> list </returns>
        public List<String> GetList(string name)
        {
            List<String> list = new List<String>();
            try
            {
                connect.Open();
                string query = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N" + "'" + name + "'";  //N permet de gérer l'encodage unicode
                SqlCommand sql = new SqlCommand();
                sql.CommandText = query;
                sql.Connection = connect;
                sql.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = sql.ExecuteReader();
                while (reader.Read() != false)
                {
                    list.Add(reader[3].ToString().ToUpper());
                }
                reader.Close();
                connect.Close();
                return list;
            }
            catch (Exception e)
            {
                connect.Close();    
                Debug.WriteLine(e.Message);
                return null; ;
            }
        }
        /// <summary>
        /// return the type of columns like (int,bit, char, datetime)
        /// </summary>
        /// <param name="tbName">dataTable_name</param>
        /// <param name="columnName">column_name</param>
        /// <returns> string </returns>
        public string GetColumnType(string tbName, string columnName)
        {     
            string str = "";
            try
            {
                connect.Open();
                string queryString = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + tbName + "' and COLUMN_NAME = " + "'" + columnName + "'";
                SqlCommand sql = new SqlCommand();
                sql.CommandText = queryString;
                sql.Connection = connect;
                sql.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = sql.ExecuteReader();
                while (reader.Read())
                {
                    str += reader[0];
                }
                reader.Close();
                connect.Close();
                return str;
            }
            catch (Exception e)
            {

                connect.Close();
                Debug.WriteLine(e.Message);
                return str;
            }
        }
        public List<String> GetOnlyDateOfName(string dbname)
        {
            List<String> list = new List<String>();
            try
            {
                connect.Open();
                string queryString = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + dbname.ToString() + "'";
                SqlCommand sql = new SqlCommand();
                sql.CommandText = queryString;
                sql.Connection = connect;
                sql.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = sql.ExecuteReader();
                while (reader.Read())
                {
                    if (reader[7].ToString().Equals("datetime"))
                    {
                        list.Add(reader[3].ToString());
                    }
                }
                reader.Close();
                connect.Close();
                return list;
            }
            catch (Exception e)
            {

                connect.Close();
                Debug.WriteLine(e.Message);
                return null;
            }
        }
        /// <summary>
        /// check if the date is valid
        /// </summary>
        /// <param name="value">string date format</param>
        /// <returns> boolean </returns>
        public bool DateValide(string value)
        {
            ///string st = "20200214"; //sous cette format --> value
            string format = "yyyy-MM-dd";
            string format_1 = "yyyyMMdd";
            string format_2 = "yyyy/MM/dd";
            DateTime datetime;
            DateTime dt;
            bool result = false; ;
            if (value.Contains('-'))
            {
                try
                {
                    datetime = DateTime.ParseExact(value, format, System.Globalization.CultureInfo.InvariantCulture);
                    string str = datetime + "";
                    result = DateTime.TryParse(str, out dt);
                    return result;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return result;
                }
            }
            else if (value.Contains('/'))
            {
                try
                {
                    datetime = DateTime.ParseExact(value, format_2, System.Globalization.CultureInfo.InvariantCulture);
                    string str = datetime + "";
                    result = DateTime.TryParse(str, out dt);
                    return result;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return result;
                }
            }
            else
            {
                try
                {
                    datetime = DateTime.ParseExact(value, format_1, System.Globalization.CultureInfo.InvariantCulture);
                    string str = datetime + "";
                    result = DateTime.TryParse(str, out dt);
                    return result;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return result;
                }
            }
        }
        /// <summary>
        /// gestion des guillemets (')
        /// management of quotation marks
        /// </summary>
        /// <param name="value"> string value</param>
        /// <returns></returns>
        public string Transform_string_value(string value)
        {
            string str = "";
            string st = "";
            for (int i = 0; i <= value.Length - 1; i++)
            {
                st = value[i] + "";
                if (st.Equals("'"))
                {
                    str += value[i] + "'";
                }
                else
                {
                    str += value[i];
                }
            }
            return str;
        }
        /// <summary>
        /// change * to % (varchar)
        /// </summary>
        /// <param name="value"></param>
        /// <returns> string value</returns>
        public string Change_operator(string value)
        {
            string str = "";
            for (int i = 0; i <= value.Length - 1; i++)
            {
                if (value[i].Equals('*'))
                {
                    str += "%";
                }
                else
                {
                    str += value[i];
                }
            }
            return str;
        }
        /// <summary>
        /// make a query format
        /// </summary>
        /// <param name="tbname"></param>
        /// <param name="colname"></param>
        /// <param name="value"></param>
        /// <returns>string value of query format</returns>
        public string Get_Query(string tbname, string colname, string value)
        {
            string str = GetColumnType(tbname, colname);
            string save_operator = "";
            string date_save = "";
            string value_save = "";
            if (str.Equals("datetime"))
            {
                string v1 = "";
                string v2 = "";
                int i = 0;
                int j = value.Length - 1;

                if (value.Contains('<') && value.Contains('>'))
                {
                    while (!(value[i].Equals('>') || value[i].Equals(' ')))
                    {
                        v1 += value[i];   // date 1
                        i++;
                    }
                    while (!(value[j].Equals('<') || value[j].Equals(' ')))
                    {
                        v2 = value[j] + v2; //date 2
                        j--;
                    }
                    bool valid_date_1 = DateValide(v1);
                    bool valid_date_2 = DateValide(v2);
                    if (valid_date_1 && valid_date_2)
                    {
                        return " between " + "'" + v1 + "'" + " and " + "'" + v2 + "'";
                    }
                }
                else
                {
                    for (int k = 0; k <= value.Length - 1; k++)
                    {
                        if (value[k].Equals('<') || value[k].Equals('>') || value[k].Equals('='))
                        {
                            save_operator += value[k];
                        }
                        else
                        {
                            if (!value[k].Equals(' '))
                            {
                                date_save += value[k];
                            }
                        }
                    }
                    if (DateValide(date_save))
                    {
                        return save_operator + "'" + date_save + "'";
                    }
                }
            }
            else if (str.Equals("int"))
            {
                for (int k = 0; k <= value.Length - 1; k++)
                {
                    if (value[k].Equals('<') || value[k].Equals('>') || value[k].Equals('='))
                    {
                        save_operator += value[k];
                    }
                    else
                    {
                        if (!value[k].Equals(' '))
                        {
                            value_save += value[k];
                        }
                    }
                }
                return save_operator + value_save;
            }
            else if (str.Equals("bit"))
            {
                return " = " + "'" + value + "'";
            }
            else
            {
                string val = "";
                string is_value;
                if (str.Equals("varchar"))
                {
                    if (value.Contains("'"))
                    {
                        val = Transform_string_value(value);
                    }
                    if (!val.Equals(""))
                    {
                        if (val.Contains("*"))
                        {
                            is_value = Change_operator(val);
                            return " like " + "'" + is_value + "'";
                        }
                    }
                    else
                    {
                        if (value.Contains('*'))
                        {
                            is_value = Change_operator(value);
                            return " like " + "'" + is_value + "'";
                        }
                        else
                        {
                            is_value = value;
                            return " = " + "'" + is_value + "'";
                        }
                    }
                }
            }
            return null;
        }
        public List<string> GetColNameUseful(string name)
        {
            List<string> list = null;
            var dbName = name.ToString();
            try
            {
                connect.Open();
                string query = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N" + "'" + dbName + "'";  //N permet de gérer l'encodage unicode
                SqlCommand sql = new SqlCommand();
                sql.CommandText = query;
                sql.Connection = connect;
                sql.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = sql.ExecuteReader();
                while (reader.Read() != false)
                {
                    if (list == null)
                    {
                        list = new List<String>();
                    }
                    if (!reader[3].ToString().Equals("id"))
                    {
                        list.Add(reader[3].ToString());
                    }
                }
                //var json = JsonConvert.SerializeObject(list, Formatting.None); //pas obliger cette ligne
                reader.Close();
                connect.Close();
                return list;
            }
            catch (Exception e)
            {
                connect.Close();
                Debug.Write(e.Message);
                return null;
            }
        }
        public List<string> GetListOfCount(string name)
        {
            List<string> list = null;
            List<string> list_1;
            var dbName = name.ToString();
            list_1 = GetColNameUseful(dbName);
            string verb_1 = "count(";
            string verb_2 = ") as ";
            string verb_control = "";
            foreach (string item in list_1)
            {
                if (verb_control.Equals(""))
                {
                    verb_control += verb_1 + item.ToString() + verb_2 + item.ToString();
                }
                else
                {
                    verb_control += ", " + verb_1 + item.ToString() + verb_2 + item.ToString();
                }
            }
            verb_control = "select " + verb_control + " from " + dbName;
            try
            {
                int quant;
                connect.Open();
                SqlCommand sql = new SqlCommand();
                sql.CommandText = verb_control;
                sql.Connection = connect;
                sql.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = sql.ExecuteReader();
                while (reader.Read())
                {
                    //y++; 
                    quant = reader.FieldCount;
                    list = new List<string>();
                    for (int i = 0; i < quant; i++)
                    {
                        if ((int)reader.GetValue(i) != 0)
                        {
                            list.Add(reader.GetName(i));
                        }
                    }
                }
                //var json = JsonConvert.SerializeObject(list, Formatting.None); //pas obliger cette ligne
                reader.Close();
                connect.Close();
                return list;
            }
            catch (Exception e)
            {
                connect.Close();
                Debug.WriteLine(e.Message);
                return null;
            }
        }
        /// <summary>
        /// forme le query des types varchar
        /// </summary>
        /// <returns></returns>
        public string Get_QueryOfLike(string str)
        {
            string myStr = "";
            for(int i=0; i<=str.Length-1; i++)
            {
                if (str[i].Equals('*'))
                {
                    myStr += "%";
                }else if ((""+str[i]).Equals("'"))
                {
                    myStr += "''";
                }
                else
                {
                    myStr += str[i];
                }
            }
            return " like " + "'"+ myStr+ "'";
        }
        public string DateChange(string str)
        {
            string sign = "";
            string valueFinal = "";
            int key = 0;
                    
            for(int j=0; j<=str.Length-1; j++)
            {
                if(str[j].Equals('<') || str[j].Equals('>') || str[j].Equals('='))
                {
                    sign += str[j];
                    key = key + 1; 
                }
                else
                {
                    if (str[j].Equals('T'))
                    {
                        valueFinal += " ";
                    }
                    else
                    {
                        valueFinal += str[j];
                    }
                }
            }
            return sign + "'" + valueFinal + "'";
           
        }
        public string PutOperator(string str)
        {
            int key = 0;
            for(int i=0; i<=str.Length-1; i++)
            {
                if(str[i].Equals('<') || str[i].Equals('>') || str[i].Equals('='))
                {
                    key++;
                }
            }
            if(key==0)
            {
                return "=" + str;
            }
            else
            {
                return str;
            }

        }
        [HttpGet("GetTotalRowsValues/{name}")]
        public int GetTotalRowsValues([FromRoute] string name)
        {
            int value = 0;
            try
            {
                connect.Open();
                string query = "select count(*) as total from " + name.ToString();
                SqlCommand sql = new SqlCommand();
                sql.CommandText = query;
                sql.Connection = connect;
                sql.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = sql.ExecuteReader();
                while (reader.Read())
                {
                    value = (int) reader.GetValue(0);
                }
                reader.Close();
                connect.Close();
                return value;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                connect.Close();
                return value;
            }
        }
        /// <summary>
        /// return the detail of dataTable
        /// </summary>
        /// <param name="name">dataTable_name</param>
        /// <returns></returns>
        [HttpGet("GetAllColumsTable/{name}")]
        public IActionResult GetTable([FromRoute] string name)
        {
            List<TableInfo> list = null;
            List<TableInfo> final_List = new List<TableInfo>();
            List<string> myList;
            TableInfo tableInfo = null;
            bool value;
            string type = "";
            object obj;
            var dbName = name.ToString();
            myList = GetListOfCount(dbName);
            try
            {
                connect.Open();
                string query = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N" + "'" + dbName + "'";  //N permet de gérer l'encodage unicode
                SqlCommand sql = new SqlCommand();
                sql.CommandText = query;
                sql.Connection = connect;
                sql.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = sql.ExecuteReader();
                while (reader.Read() != false)
                {
                    if (list == null)
                    {
                        list = new List<TableInfo>();
                    }
                    if (reader[6].ToString().Equals("YES"))
                    {
                        value = true;
                    }
                    else
                    {
                        value = false;
                    }
                    obj = reader[8];
                    if (DBNull.Value.Equals(obj))
                    {
                        type = reader[7].ToString();
                    }else if (reader[8].ToString().Equals("-1"))
                    {
                        type = reader[7].ToString() + "(max)";
                    }
                    else
                    {
                        type = reader[7].ToString() + "(" + reader[8].ToString() + ")";
                    }
                    tableInfo = new TableInfo(reader[3].ToString(), type, value);
                    list.Add(tableInfo);
                }
                //var json = JsonConvert.SerializeObject(list, Formatting.None); //pas obliger cette ligne
                reader.Close();
                connect.Close();
                list.RemoveAt(0); //suppression de id
                /*foreach(TableInfo t in list)
                {
                    foreach(string str in myList)
                    {
                        if (t.Name.Equals(str))
                        {
                            final_List.Add(t);
                        }
                    }
                }*/
                return Ok(list.ToArray());
            }
            catch (Exception e)
            {
                connect.Close();
                return BadRequest(e.Message + " Veuillez svp revoir votre nom de table");
            }
        }
        [HttpGet("GetColTable/{name}")]
        public IActionResult GetColName([FromRoute] string name)
        {
            List<string> list = null;
            List<string> list_1;
            string dbName = name.ToString();
            list_1 = GetColNameUseful(dbName);
            string verb_1 = "count(";
            string verb_2 = ") as ";
            string verb_control = "";
            foreach(string item in list_1)
            {
                if (verb_control.Equals(""))
                {
                    verb_control += verb_1 + item.ToString() + verb_2 + item.ToString();
                }
                else
                {
                    verb_control += ", " + verb_1 + item.ToString() + verb_2 + item.ToString();
                }
            }
            verb_control = "select " + verb_control + " from " + dbName;
            try
            {
                int quant;            
                connect.Open();
                SqlCommand sql = new SqlCommand();
                sql.CommandText = verb_control;
                sql.Connection = connect;
                sql.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = sql.ExecuteReader();
                while (reader.Read())
                {
                   //y++; 
                    quant = reader.FieldCount;
                    list = new List<string>();
                    for(int i=0; i<quant; i++)
                    {
                        if((int)reader.GetValue(i) != 0)
                        {
                            list.Add(reader.GetName(i));
                        }
                    }
                }
                //var json = JsonConvert.SerializeObject(list, Formatting.None); //pas obliger cette ligne
                reader.Close();
                connect.Close();
                return Ok(list.ToArray());
            }
            catch (Exception e)
            {
                connect.Close();
                return BadRequest(e.Message + " Veuillez svp revoir votre nom de table");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="JsonArray"></param>
        /// <returns></returns>
        [HttpPost("GetFilterData/{name}")]
        public IActionResult GetDataFilter([FromRoute] string name, [FromBody] Object JsonObject)
        {
            Dictionary<string, string> dico = new Dictionary<string, string>();
            string value;
            string str_1 = "";
            string my_value = "";
            var json = (JObject)JsonObject;
            foreach (var js in json.Properties())
            {
                if (!js.Value.ToString().Equals(""))
                {
                    dico.Add(js.Name, js.Value.ToString());
                }
            }
            /*foreach (var item in JsonArray)
            {
                var json = (JObject)item;
                foreach (var js in json.Properties())
                {
                    if (!js.Value.ToString().Equals(""))
                    {
                        dico.Add(js.Name, js.Value.ToString());
                    }
                }
            }*/
            foreach (KeyValuePair<string, string> item in dico)
            {
                my_value = GetColumnType(name.ToString(), item.Key);
                value = item.Value.ToString();
                if(my_value.Equals("datetime"))
                {
                        if (str_1.Equals(""))
                        {
                            str_1 += item.Key + PutOperator(DateChange(value));
                        }
                        else
                        {
                            str_1 += " and " + item.Key + PutOperator(DateChange(value));
                    }
                    
                }
                else if (my_value.Equals("int"))
                {
                    if(value.Contains('<') || value.Contains('>') || value.Contains('='))
                    {
                        if (str_1.Equals(""))
                        {
                            str_1 += item.Key + value;
                        }
                        else
                        {
                            str_1 += " and " + item.Key + value;
                        }
                    }
                    else
                    {
                        if (str_1.Equals(""))
                        {
                            str_1 += item.Key +"="+ value;
                        }
                        else
                        {
                            str_1 += " and " + item.Key +"="+ value;
                        }
                    }
                }
                else if(my_value.Equals("varchar") || my_value.Equals("nvarchar"))
                {
                    if (str_1.Equals(""))
                    {
                        str_1 += item.Key + Get_QueryOfLike(value);
                    }
                    else
                    {
                        str_1 += " and " + item.Key + Get_QueryOfLike(value);
                    }
                }
                else{
                    if (str_1.Equals(""))
                    {
                        str_1 += item.Key + "="+ value;
                    }
                    else
                    {
                        str_1 += " and " + item.Key + "="+ value;
                    }
                }
            }
            List<String> listDate;
            listDate = GetOnlyDateOfName(name.ToString());
            try
            {
                string dbName = name.ToString();
                connect.Open();
                string query = "SELECT * FROM " + dbName + " where " + str_1;
                SqlCommand sql = new SqlCommand();
                sql.CommandText = query;
                sql.Connection = connect;
                sql.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = sql.ExecuteReader();
                ArrayList list = new ArrayList();
                int quant;
                string j = "";
                while (reader.Read())
                {
                    quant = reader.FieldCount;
                    Dictionary<String, Object> dic = new Dictionary<String, Object>();
                    for (int i = 0; i < quant; i++)
                    {
                        //dict.Add(reader.GetName(i), reader.GetValue(i));
                        if (listDate.Contains(reader.GetName(i)))
                        {
                            object obj_1 = reader.GetValue(i);
                            string str = "";
                            if (DBNull.Value.Equals(obj_1))
                            {
                                dic.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            else
                            {
                                j = reader.GetValue(i).ToString();
                                for (int k = 0; k <= j.Length - 1; k++)
                                {
                                    if (j[k].Equals(' '))
                                    {
                                        str += "  ";
                                    }
                                    else
                                    {
                                        str += j[k];
                                    }
                                }
                                dic.Add(reader.GetName(i), str);
                            }
                        }
                        else
                        {
                            dic.Add(reader.GetName(i), reader.GetValue(i));
                        }
                    }

                    list.Add(dic);
                }
                reader.Close();
                connect.Close();
                return Ok(list);
            }
            catch (Exception e)
            {
                connect.Close();
                return BadRequest("Il y a eu un problème" + e.Message);
            }
        }
        /// <summary>
        /// return the contains of dataTable
        /// </summary>
        /// <param name="name">dataTable_name</param>
        /// <returns></returns>
        [HttpGet("GetAllDataInTable/{name}")]
        public IActionResult GetDataTable([FromRoute] string name)
        {
            int v1 = 0;
            string dbName = name.ToString();
            List<String> listDate = null;
            listDate = GetOnlyDateOfName(dbName);
            int myCount = GetTotalRowsValues(dbName);
            v1 = myCount - 10;

            try
            {
                connect.Open();
                //string query = "select * from " + dbName + " ORDER BY id OFFSET " + v1 + " ROWS FETCH next 10 ROWS ONLY";
                string query = "select * from " + dbName;
                SqlCommand sql = new SqlCommand();
                sql.CommandText = query;
                sql.Connection = connect;
                sql.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = sql.ExecuteReader();
                ArrayList list = new ArrayList();
                int quant;
                int y = 0;
                string j = "";
                while (reader.Read())
                {
                    //y++;
                    quant = reader.FieldCount;
                    Dictionary<String, Object> dict = new Dictionary<String, Object>();
                    for (int i = 0; i < quant; i++)
                    {

                        if (listDate.Contains(reader.GetName(i)))
                        {
                            object obj_1 = reader.GetValue(i);
                            string str = "";
                            if (DBNull.Value.Equals(obj_1))
                            {
                                dict.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            else
                            {
                                j = reader.GetValue(i).ToString();
                                for (int k = 0; k <= j.Length - 1; k++)
                                {
                                    if (j[k].Equals(' '))
                                    {
                                        str += "  ";
                                    }
                                    else
                                    {
                                        str += j[k];
                                    }
                                }
                                dict.Add(reader.GetName(i), str);
                            }
                        }
                        else
                        {
                            dict.Add(reader.GetName(i), reader.GetValue(i));
                        }

                    }
                    list.Add(dict);
                }
                reader.Close();
                connect.Close();
                return Ok(list);
            }
            catch (Exception e)
            {
                connect.Close();
                return BadRequest(e.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetData/{name}")]
        public IActionResult GetData([FromRoute] string name)
        {
            string dbName = name.ToString();
            try
            {
                connect.Open();
                string query = "SELECT * FROM " + dbName;
                SqlCommand sql = new SqlCommand();
                sql.CommandText = query;
                sql.Connection = connect;
                sql.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = sql.ExecuteReader();
                List<Column> list = new List<Column>();
                List<Column> myList = new List<Column>();
                int quant;
                Column column;
                while (reader.Read())
                {
                 
                    quant = reader.FieldCount;
                    Dictionary<String,Object> dict = new Dictionary<String,Object>();
                    for (int i = 0; i < quant; i++)
                    {
                        //dict.Add(reader.GetName(i), reader.GetValue(i));
                        column = new Column(reader.GetName(i),reader.GetValue(i).ToString());
                        list.Add(column);
                    }

                    //myList.Add();
                }
                reader.Close();
                connect.Close();
                return Ok(list);
            }
            catch (Exception e)
            {
                connect.Close();
                return BadRequest(e.Message);
            }
        }
        [HttpGet("GetOk")]
        public IActionResult GetOk()
        {
            Column c1 = new Column("id","1");
            JsonConvert.SerializeObject(c1, Formatting.Indented);
            List<Column> list = new List<Column>();
            list.Add(c1);
            return Ok(list);
        }
        /// <summary>
        /// return the all name of dataTable in db
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllTablesName")]
        public IActionResult GetAllTables()
        {
            List<String> list = new List<string>();
            try
            {
                int i = 0;
                connect.Open();
                string queryString = "SELECT name FROM sysobjects WHERE TYPE='U' ORDER BY name;";
                SqlCommand sql = new SqlCommand();
                sql.CommandText = queryString;
                sql.Connection = connect;
                sql.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = sql.ExecuteReader();
                while (reader.Read() != false)
                {
                    list.Add((String)reader[i]);
                }
                reader.Close();
                connect.Close();
                // var json = JsonConvert.SerializeObject(list, Formatting.None);
                return Ok(list);
            }
            catch (Exception e)
            {
                connect.Close();
                return BadRequest("Request not found " + e.Message);
            }
        }
        /// <summary>
        /// delete table and his container
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        [HttpDelete("DeleteTable/{tableName}")]
        public IActionResult PostDropTable([FromRoute] string tableName)
        {
            try
            {
                string query = String.Format("DROP TABLE " + tableName.ToString());
                connect.Open();
                SqlCommand sql = new SqlCommand(query, connect);
                int retour = sql.ExecuteNonQuery();
                connect.Close();

                return Ok("Table is successful deleted !! ");
            }
            catch (Exception e)
            {
                connect.Close();
                return BadRequest(e.Message);
            }
        }
        /// <summary>
        /// insert jsonArray data in dataTable
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="jsonArray"></param>
        /// <returns></returns>
        [HttpPost("InsertDataInTable/{tableName}")]
        public IActionResult PostDataInTable([FromRoute] string tableName, [FromBody] JArray jsonArray)
        {
            var column = "";
            var str = "";
            var values = "";
            var data = "";
            string v1;
            string v2;
            string v3 = "";
            int Num;
            string table = tableName.ToString();
            List<String> list = new List<String>();
            Dictionary<String, String> dict = new Dictionary<String, String>();
            Dictionary<String, String> dict_2 = new Dictionary<String, String>();
            list = GetList(table);
            foreach (var el in jsonArray)
            {
                var jsonObject = (JObject)el;
                foreach (var item in jsonObject.Properties())
                {
                    column += $"{item.Name}" + ",";
                    values += $"{item.Value}" + ",";
                    dict.Add(item.Name.ToUpper(), item.Value.ToString());
                }
            }
            foreach (string item in list)
            {
                if (dict.ContainsKey(item))
                {
                    if (dict[item].Contains("'"))
                    {
                        for (int i = 0; i <= dict[item].Length - 1; i++)
                        {
                            v1 = "" + dict[item][i];
                            if (v1.Equals("'"))
                            {
                                v2 = v1 + "'";
                                v3 += v2;
                            }
                            else
                            {
                                v3 += dict[item][i];
                            }
                        }
                        v3 = "'" + v3 + "'";
                        dict_2.Add(item, v3);
                    }
                    else
                    {
                        bool isNum = int.TryParse(dict[item].ToString(), out Num);
                        if (isNum)
                        {
                            dict_2.Add(item, dict[item]);
                        }
                        else
                        {
                            string x = "'" + dict[item] + "'";
                            dict_2.Add(item, x);
                        }
                    }
                }
            }
            string key = "";
            string key_one = "";
            string valueOfKey = "";
            string value_One = "";
            foreach (KeyValuePair<String, String> item in dict_2)
            {
                key += item.Key + ",";
                valueOfKey += item.Value + ",";
            }
            for (int i = 0; i <= key.Length - 2; i++)
            {
                key_one += key[i];
            }
            for (int i = 0; i <= valueOfKey.Length - 2; i++)
            {
                value_One += valueOfKey[i];
            }
            for (int i = 0; i <= column.Length - 2; i++)
            {
                str += column[i];
            }
            for (int i = 0; i <= values.Length - 2; i++)
            {
                data += values[i];
            }
            try
            {
                string query = String.Format("INSERT INTO " + table + "(" + key_one + ") VALUES(" + value_One + ")");
                connect.Open();
                SqlCommand sql = new SqlCommand(query, connect);
                int retour = sql.ExecuteNonQuery();
                connect.Close();
                if (retour != 1)
                {
                    return BadRequest();
                }
                else
                {
                    return Ok("Data has been successfull added !!");
                }
            }
            catch (Exception e)
            {
                connect.Close();
                return BadRequest(key_one + " ----" + value_One + "  " + e.Message);
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="jsonArray"></param>
        /// <returns></returns>
        [HttpPost("CreateTable/{tableName}")]
        public IActionResult CreateTable([FromRoute] string tableName, [FromBody] JArray jsonArray)
        {
            List<Column> list = new List<Column>();
            Column c1 = new Column("id", "int not null identity(1,1)",null);
            Column c2 = new Column("timeStamp", "datetime not null default current_timestamp",null);
            Column c3 = new Column("customerId", "int",null);
            Column c4 = new Column("customerName", "varchar(max)",null);
            Column c5 = new Column("userId", "int",null);
            Column c6 = new Column("userName", "varchar(max)",null);
            Column c7 = new Column("message", "varchar(max)",null);
            list.Add(c1);
            list.Add(c2);
            list.Add(c3);
            list.Add(c4);
            list.Add(c5);
            list.Add(c6);
            list.Add(c7);
            Column column;
            bool bol = true;
            string name="";
            string type;
            foreach (var item in jsonArray)
            {
                var jsonObject = (JObject)item;
                foreach (var prop in jsonObject.Properties())
                {
                    if (bol)
                    {
                        name = prop.Value.ToString();
                        bol = false;
                    }
                    else
                    {
                        type = prop.Value.ToString();
                        bol = true;
                        column = new Column(name,type,null);
                        list.Add(column);
                    }
                }
            }
            Table table = new Table(tableName.ToString(), list);
            string str = "";
            foreach (Column item in list)
            {
                if (str.Equals(""))
                {
                    str += item.getName() + " " + item.getType();
                }
                else
                {
                    str += ", " + item.getName() + " " + item.getType();
                }
            }
            try
            {
                connect.Open();
                string query = "CREATE TABLE " + table.getName().ToString() + "(" + str + ")";
                SqlCommand sqlCommand = new SqlCommand(query, connect);
                int back = sqlCommand.ExecuteNonQuery();
                connect.Close();
                return Ok(back + " table created !!");
            }
            catch (Exception e)
            {
                connect.Close();
                Debug.WriteLine(e.Message);
                return BadRequest(e.Message+ " Erreur");
            }
        }
        /// <summary>
        /// create dataTable and his columns according to the name gave
        /// </summary>
        /// <param name="name"></param>
        /// <param name="jsonArray"></param>
        /// <returns></returns>
        [HttpPost("createtables/{name}")]
        public IActionResult PostDb([FromRoute] string name, [FromBody] JArray jsonArray)
        {
            var result = "";
            //var res = "";
            //var jsonArray = JArray.Parse(str);
            foreach (var el in jsonArray)
            {
                var jsonObject = (JObject)el;
                foreach (var prop in jsonObject.Properties())
                {
                    //jsonObject.Count;
                    //res += $"{prop.Name}: {prop.Value}" + Environment.NewLine; 
                    result += $"{prop.Value}" + ",";
                }
            }
            int j = -1;
            string str = "";
            string str2 = "";
            for (int i = 0; i <= result.Length - 1; i++)
            {
                if (result[i].Equals(','))
                {
                    if (j == -1)
                    {
                        str += " ";
                        j = 1;
                    }
                    else
                    {
                        str += result[i];
                        j = -1;
                    }
                }
                else              
                {
                    str += result[i];
                }
            }
            for (int i = 0; i <= str.Length - 2; i++)
            {
                str2 += str[i];
            }
            try
            {
                string dbName = name.ToString();
                string optionsDeBase = "(id INT NOT NULL IDENTITY(1,1), timeStamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, " + "" + "customerId int, customerName varchar(MAX), userId int, userName varchar(MAX), message varchar(MAX),";
                connect.Open();
                string queryString = "CREATE TABLE " + dbName + optionsDeBase + str2 + ");";
                SqlCommand cmd = new SqlCommand(queryString, connect);
                int retour = cmd.ExecuteNonQuery();
                connect.Close();
                return Ok("datatable is successfull created !!");
            }
            catch (Exception e)
            {
                connect.Close();
                return BadRequest(e.Message);
            }
        }
    }
}

