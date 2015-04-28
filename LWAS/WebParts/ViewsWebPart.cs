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
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Web.Caching;

using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.Translation;
using LWAS.Extensible.Interfaces.Storage;
using LWAS.Extensible.Interfaces.Routing;

using LWAS.Infrastructure;
using LWAS.Database;

namespace LWAS.WebParts
{
    public class ViewsWebPart : ConfigurableWebPart
    {
        private static object SyncRoot = new object();

        public Dictionary<string, LWAS.Database.Database> Databases;

        public IExpressionsManager ExpressionsManager { get; set; }
        public IStorageAgent Agent { get; set; }
        public IRoutingManager RoutingManager { get; set; }
        public ViewsManager ViewsManager { get; set; }
        public LWAS.Database.View CurrentView { get; set; }

        public bool RuntimeAware { get; set; }

        LWAS.Database.Database _currentDatabase;
        public LWAS.Database.Database CurrentDatabase
        {
            get { return _currentDatabase; }
            set
            {
                _currentDatabase = value;
                this.ViewsManager = _currentDatabase.ViewsManager;
                this.CurrentView = null;
            }
        }

        public string SelectView 
        {
            set
            {
                if (!this.ViewsManager.Views.ContainsKey(value))
                    throw new InvalidOperationException(String.Format("view '{0}' not found", value));
                this.CurrentView = this.ViewsManager.Views[value];
            }
        }

        public string SelectDatabase
        {
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    if (!this.Databases.ContainsKey(value)) 
                        throw new InvalidOperationException(String.Format("Unknown database '{0}'", value));
                    this.CurrentDatabase = this.Databases[value];
                }
                else
                {
                    this.CurrentDatabase = this.Databases.First().Value;
                }
            }
        }

        public string[] Command
        {
            get
            {
                if (null == this.CurrentView) throw new InvalidOperationException("CurrentView not set");
                StringBuilder sb = new StringBuilder();
                this.CurrentView.ToSql(sb);
                return new string[] { this.CurrentView.Name, sb.ToString() };
            }
        }

        public string[] UpdateCommand
        {
            get
            {
                if (null == this.CurrentView) throw new InvalidOperationException("CurrentView not set");
                StringBuilder sb = new StringBuilder();
                this.CurrentView.ToUpdateSql(sb);
                return new string[] { this.CurrentView.Name, sb.ToString() };
            }
        }

        public ViewSorting Sort
        {
            get
            {
                if (null == this.CurrentView) throw new InvalidOperationException("CurrentView not set");
                return this.CurrentView.Sorting;
            }
        }

        public ViewsWebPart()
        {
            this.Hidden = true;
            this.RuntimeAware = false;
        }

        public override void Initialize()
        {
            if (null == this.ExpressionsManager) throw new MissingProviderException("ExpressionsManager");
            if (null == this.Agent) throw new MissingProviderException("Agent");
            if (null == this.RoutingManager) throw new MissingProviderException("RoutingManager");

            string views_config = this.RoutingManager.SettingsRoutes["VIEWS_CONFIG"].Path;
            if (this.RuntimeAware && null != this.RoutingManager.RuntimeSettingsRoutes["VIEWS_CONFIG"])
                views_config = this.RoutingManager.RuntimeSettingsRoutes["VIEWS_CONFIG"].Path;

            string connections_config = this.RoutingManager.SettingsRoutes["CONNECTIONS_FILE"].Path;

            if (this.RuntimeAware && null != this.RoutingManager.RuntimeSettingsRoutes["CONNECTIONS_FILE"])
                connections_config = this.RoutingManager.RuntimeSettingsRoutes["CONNECTIONS_FILE"].Path;

            if (String.IsNullOrEmpty(connections_config)) throw new ApplicationException("CONNECTIONS_FILE not found or not set");

            Cache cache = HttpRuntime.Cache;
            lock (SyncRoot)
            {
                this.Databases = new Dictionary<string, LWAS.Database.Database>();

                if (this.Agent.HasKey(connections_config))
                {
                    XDocument dbs = cache["XDocument: " + connections_config] as XDocument;
                    if (null == dbs)
                    {
                        dbs = XDocument.Parse(this.Agent.Read(connections_config));
                        cache.Insert("XDocument: " + connections_config, dbs, new CacheDependency(connections_config));
                    }
                    foreach (XElement element in dbs.Element("connections").Elements("connection"))
                    {
                        string key = element.Attribute("key").Value;
                        if (null != element.Attribute("views"))
                            views_config = element.Attribute("views").Value;

                        if (!String.IsNullOrEmpty(views_config))
                        {
                            LWAS.Database.Database database = cache[views_config] as LWAS.Database.Database;
                            if (null == database)
                            {
                                database = new Database.Database(key, views_config, this.Agent, this.ExpressionsManager);

                                cache.Insert(views_config, database, new CacheDependency(views_config));
                            }

                            if (!this.Databases.ContainsKey(database.Name))
                                this.Databases.Add(database.Name, database);
                        }
                    }
                }
                else
                {
                    LWAS.Database.Database database = new Database.Database(views_config, views_config, this.Agent, this.ExpressionsManager);
                    this.Databases.Add(database.Name, database);
                }
            }

            this.CurrentDatabase = this.Databases.First().Value;

            base.Initialize();
        }
    }
}
