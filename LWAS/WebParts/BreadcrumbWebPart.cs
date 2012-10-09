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
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using System.IO;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.Storage;
using LWAS.Infrastructure;
using LWAS.CustomControls;

namespace LWAS.WebParts
{
    public class BreadcrumbWebPart : ChroniclerWebPart, IInitializable, IReporter
    {
        public Breadcrumb Breadcrumb;
        readonly string root_storage_path;

        [Personalizable(true)]
        public string Separator
        {
            get 
            {
                if (null == this.Breadcrumb)
                    return null;
                return this.Breadcrumb.Separator; 
            }
            set { this.Breadcrumb.Separator = value; }
        }

        string _key;
        [Personalizable(true)]
        public string Key 
        {
            get { return _key; }
            set 
            {
                _key = value;
                if (!String.IsNullOrEmpty(_key))
                    this.Breadcrumb.Key = this.FullKey;
            }
        }

        [Personalizable(true)]
        public string ShowRoot
        {
            get
            {
                if (null == this.Breadcrumb)
                    return null;
                return this.Breadcrumb.ShowRoot.ToString();
            }
            set
            {
                bool bval = false;
                if (bool.TryParse(value, out bval))
                    this.Breadcrumb.ShowRoot = bval;
            }
        }

        public string FullKey
        {
            get { return HttpContext.Current.Server.MapPath(System.IO.Path.Combine(root_storage_path, _key + ".xml")); }
        }

        public string Path
        {
            get { return this.Breadcrumb.Path; }
            set { this.Breadcrumb.Path = value; }
        }

        public string ChildrenPath
        {
            get { return this.Breadcrumb.Path + "/*"; }
        }

        public string ParentPath
        {
            get { return this.Breadcrumb.Path.Substring(0, this.Breadcrumb.Path.LastIndexOf("/")); }
        }

        IStorageAgent _agent;
        public IStorageAgent Agent 
        {
            get { return _agent; }
            set 
            { 
                _agent = value;
                this.Breadcrumb.Agent = _agent;
            }
        }

        public IMonitor Monitor
        {
            get;
            set;
        }

        public BreadcrumbWebPart()
        {
            root_storage_path = ConfigurationManager.AppSettings["UPLOADS_REPO"];
            if (String.IsNullOrEmpty(root_storage_path)) throw new ApplicationException("Breadcrumb needs UPLOADS_REPO to be defined in config key");
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this.Breadcrumb = new Breadcrumb();
            this.Breadcrumb.ID = "Breadcrumb";
            this.Breadcrumb.Agent = this.Agent;
            this.Breadcrumb.Error += new EventHandler<Breadcrumb.ErrorEventArgs>(Breadcrumb_Error);
            this.Breadcrumb.Displayed += new EventHandler(Breadcrumb_Displayed);
            this.Breadcrumb.Changed += new EventHandler(Breadcrumb_Changed);
            this.Controls.Add(this.Breadcrumb);
        }

        void Breadcrumb_Error(object sender, Breadcrumb.ErrorEventArgs e)
        {
            this.Monitor.Register(this, this.Monitor.NewEventInstance("Breadcrumb error", null, e.Exception, EVENT_TYPE.Error));
        }

        void Breadcrumb_Displayed(object sender, EventArgs e)
        {
            OnMilestone("display");
        }

        void Breadcrumb_Changed(object sender, EventArgs e)
        {
            OnMilestone("path");
        }

        public void Initialize()
        {
            if (null == this.Agent) throw new ApplicationException("Agent not set");
        }

        public RequestInitializationCallback RequestInitialization
        {
            get;
            set;
        }

        public int Initialization
        {
            get;
            set;
        }
        public int Creation
        {
            get;
            set;
        }

        public int Change
        {
            get;
            set;
        }

        public int Completion
        {
            get;
            set;
        }

    }
}
