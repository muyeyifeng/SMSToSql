using System.Collections.Generic;
using System.Configuration;         //ConfigurationManager.AppSettings["AppSecret"]
using System.Web;
using static System.Collections.Specialized.NameObjectCollectionBase;

namespace esp8266_smsResponse
{
    /// <summary>
    /// smsResponse 的摘要说明
    /// </summary>
    public class smsResponse : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            Database database = new Database();
            Decode_PDU decode_PDU = new Decode_PDU();
            if (!string.IsNullOrEmpty(context.Request.QueryString["isExist"]))
            {
                context.Response.Write(database.IsExist(ConfigurationManager.AppSettings["mySqlConnstring"], context.Request.QueryString["isExist"]));
            }
            else if (!string.IsNullOrEmpty(context.Request.QueryString["checkUpdate"]))
            {

            }
            else if (!string.IsNullOrEmpty(context.Request.QueryString["raw"]))
            {
                try
                {
                    Dictionary<string, string> keyValuePairs = decode_PDU.PduTosmsData(context.Request.QueryString["raw"]);
                    context.Response.Write(database.Insert(ConfigurationManager.AppSettings["mySqlConnstring"], keyValuePairs));
                }
                catch
                {
                    context.Response.Write("Error data");
                }
            }
            else
            {
                context.Response.Write("unknow requests");
            }


        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


    }
}