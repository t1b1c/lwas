/*
 * Copyright 2006-2012 TIBIC SOLUTIONS
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
        string views_config;

        public IExpressionsManager ExpressionsManager { get; set; }
        public IStorageAgent Agent { get; set; }
        public IRoutingManager RoutingManager { get; set; }
        public ViewsManager ViewsManager { get; set; }
        public LWAS.Database.View CurrentView { get; set; }

        public string SelectView 
        {
            set
            {
                if (!this.ViewsManager.Views.ContainsKey(value))
                    throw new InvalidOperationException(String.Format("view '{0}' not found", value));
                this.CurrentView = this.ViewsManager.Views[value];
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

        public ViewsWebPart()
        {
            this.Hidden = true;
        }

        public override void Initialize()
        {
            if (null == this.ExpressionsManager) throw new MissingProviderException("ExpressionsManager");
            if (null == this.Agent) throw new MissingProviderException("Agent");
            if (null == this.RoutingManager) throw new MissingProviderException("RoutingManager");

            views_config = this.RoutingManager.SettingsRoutes["VIEWS_CONFIG"].Path;
            if (String.IsNullOrEmpty(views_config)) throw new ApplicationException("VIEWS_CONFIG not found or not set");
            if (this.Agent.HasKey(views_config))
                this.ViewsManager = new ViewsManager(views_config, this.Agent, this.ExpressionsManager);

            base.Initialize();
        }
    }
}
