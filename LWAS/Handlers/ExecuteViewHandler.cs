/*
 * Copyright 2006-2015 TIBIC SOLUTIONS
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;
using System.Xml.Linq;
using System.Text;

using LWAS.Infrastructure.Storage;
using LWAS.Infrastructure.Security;
using LWAS.CustomControls.DataControls;
using LWAS.Database;

namespace LWAS.Handlers
{
    public class ExecuteViewHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            Cache cache = context.Cache;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            var query = context.Request.QueryString.AllKeys.ToList();
            string databaseName = context.Request["database"];      // index 0
            string viewName = context.Request["view"];              // index 1
            foreach(string key in query.SkipWhile(q => query.IndexOf(q) <= 1))
                parameters.Add(key, context.Request.QueryString[key]);

            if (String.IsNullOrEmpty(databaseName))
                databaseName = DefaultDatabase();
            LWAS.Database.Database database = cache.OfType<System.Collections.DictionaryEntry>()
                                                   .Select(entry => entry.Value)
                                                   .OfType<LWAS.Database.Database>()
                                                   .FirstOrDefault(db => db.Name == databaseName);
            if (null != database)
            {
                View view = database.ViewsManager.Views.Values.FirstOrDefault(v => v.Name == viewName);
                if (null != view)
                {
                    //setup parameters
                    foreach (string parameterName in view.Parameters)
                        if (parameters.ContainsKey(parameterName))
                            view.Parameters[parameterName] = parameters[parameterName];

                    SqlDataBridge commander = new SqlDataBridge();
                    commander.ConnectionString = ConnectionString(databaseName);
                    if (commander.Commands.ContainsKey(viewName))
                        commander.Commands.Remove(viewName);
                    StringBuilder sb = new StringBuilder();
                    view.ToSql(sb);
                    commander.Commands.Add(viewName, new SqlCommand(sb.ToString()));
                    SqlDataReader reader = commander.ExecuteCommand(viewName);
                    DataSet ds = new DataSet();
                    ds.Load(reader, LoadOption.OverwriteChanges, viewName);

                    DataTable table = ds.Tables[0];
                    string json = "[";

                    foreach(DataRow dr in table.Rows)
                    {
                        json += "{";
                        foreach(DataColumn dc in table.Columns)
                        {
                            object val = dr[dc.ColumnName];
                            if (null == val)
                                json += String.Format(@"""{0}"":"""",", dc.ColumnName);
                            else if (val is int)
                                json += String.Format(@"""{0}"":{1},", dc.ColumnName, val);
                            else if (val is bool)
                                json += String.Format(@"""{0}"":{1},", dc.ColumnName, val.ToString().ToLower());
                            else if (val is decimal)
                                json += String.Format(@"""{0}"":{1},", dc.ColumnName, ((decimal)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
                            else if (val is DateTime)
                                json += String.Format(@"""{0}"":""{1}"",", dc.ColumnName, ((DateTime)val).ToString("yyyy-MM-ddTHH:mm"));
                            else
                                json += String.Format(@"""{0}"":""{1}"",", dc.ColumnName, val);
                        }
                        if (json.EndsWith(","))
                            json = json.Substring(0, json.Length - 1);
                        json += "},";
                    }

                    if (json.EndsWith(","))
                        json = json.Substring(0, json.Length - 1);

                    json += "]";

                    context.Response.Write(json);
                    commander.Close();
                }
            }
        }

        string DefaultDatabase()
        {
            string file = ConfigurationManager.AppSettings["CONNECTIONS_FILE"];
            FileAgent agent = new FileAgent();
            XDocument xdoc = XDocument.Parse(agent.Read(file));
            return xdoc.Element("connections")
                       .Elements()
                       .First()
                       .Attribute("key")
                       .Value;
        }

        string ConnectionString(string key)
        {
            string file = ConfigurationManager.AppSettings["CONNECTIONS_FILE"];
            FileAgent agent = new FileAgent();
            XDocument xdoc = XDocument.Parse(agent.Read(file));
            return xdoc.Element("connections")
                       .Elements()
                       .First(e => e.Attribute("key").Value == key)
                       .Attribute("string")
                       .Value;
        }
      
    }
}
