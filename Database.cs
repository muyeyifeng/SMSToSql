using System.Collections.Generic;
using MySql.Data.MySqlClient;   //NuGet下载MySql.Data 
using System.Text;

namespace esp8266_smsResponse
{
    public class Database
    {
        /// <summary>
        /// 判断文本内容是否已存在
        /// </summary>
        /// <param name="hash">sha1 of {from,,date,data}</param>
        /// <returns></returns>
        public bool IsExist(string strconn, string hash)
        {
            MySqlConnection mySqlConnection = new MySqlConnection(strconn);
            StringBuilder sqlCmd = new StringBuilder("select * from receiveSMS where `hash` = '").Append(hash).Append("'");
            MySqlCommand cmd = new MySqlCommand(sqlCmd.ToString(), mySqlConnection);
            try
            {
                mySqlConnection.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            }
            catch
            {
                return false;
            }
            finally
            {
                mySqlConnection.Close();
            }
        }

        public bool Insert(string strconn, Dictionary<string, string> keyValuePairs)
        {
            {
                keyValuePairs.TryGetValue("hash", out string hash);
                if (IsExist(strconn, hash))
                    return false;
            }
            MySqlConnection mySqlConnection = new MySqlConnection(strconn);
            StringBuilder sqlCmd = new StringBuilder("INSERT INTO receiveSMS (");
            foreach (string key in keyValuePairs.Keys)
            {
                sqlCmd.Append("`").Append(key).Append("`").Append(",");
            }
            sqlCmd.Remove(sqlCmd.Length - 1, 1);
            sqlCmd.Append(") VALUES (");
            foreach (string key in keyValuePairs.Keys)
            {
                keyValuePairs.TryGetValue(key, out string value);
                sqlCmd.Append("'").Append(value).Append("'").Append(",");
            }
            sqlCmd.Remove(sqlCmd.Length - 1, 1);
            sqlCmd.Append(")");
            MySqlCommand cmd = new MySqlCommand(sqlCmd.ToString(), mySqlConnection);
            try
            {
                mySqlConnection.Open();
                return cmd.ExecuteNonQuery() == 1;
            }
            catch
            {
                return false;
            }
            finally
            {
                mySqlConnection.Close();
            }
        }

        public bool Delete(string strconn, string hash)
        {
            MySqlConnection mySqlConnection = new MySqlConnection(strconn);
            StringBuilder sqlCmd = new StringBuilder("DELETE FROM receiveSMS where `hash` = '").Append(hash).Append("'");
            MySqlCommand cmd = new MySqlCommand(sqlCmd.ToString(), mySqlConnection);
            try
            {
                mySqlConnection.Open();
                return cmd.ExecuteNonQuery() == 1;
            }
            catch
            {
                return false;
            }
            finally
            {
                mySqlConnection.Close();
            }
        }
    }
}