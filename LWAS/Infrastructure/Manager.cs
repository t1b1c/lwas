/*
 * Copyright 2006-2013 TIBIC SOLUTIONS
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
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Xml;

using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.WebParts;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Interfaces.Routing;

namespace LWAS.Infrastructure
{
	public class Manager : WebPartManager, IReporter
	{
		private ManagerItems items = new ManagerItems();
		private bool Initialized = false;
        
        IDriver driver;
        public IDriver Driver
		{
			get { return this.driver; }
		}
        
        IAuthorizer authorizer;
        public IAuthorizer Authorizer
		{
			get { return this.authorizer; }
		}
        
        bool _enableAuthorizer = true;
        public bool EnableAuthorizer
		{
			get { return this._enableAuthorizer; }
			set { this._enableAuthorizer = value; }
		}

        private IMonitor _monitor;
        public IMonitor Monitor
		{
			get { return this._monitor; }
			set { this._monitor = value; }
		}
        
        public IExpressionsManager ExpressionsManager { get; set; }
        public IZonesProvider ZonesProvider { get; set; }
        public IRoutingManager RoutingManager { get; set; }

		public Manager()
		{
			string type = ConfigurationManager.AppSettings["DRIVER"];
			if (null == type) throw new ManagerException("Default driver not set in config");

			Type driverType = Type.GetType(type, false);
			if (null == driverType) throw new ManagerException("Default driver type could not be loaded");

			this.driver = (Activator.CreateInstance(driverType) as IDriver);
			if (null == this.driver) throw new ManagerException("Default driver type specified in config is not an IDriver");

			this.driver.Watchdog = new Watchdog();
			string aztype = ConfigurationManager.AppSettings["AUTHORIZER"];
			if (null == aztype) throw new ManagerException("Authorizer not set in config");

			Type azType = Type.GetType(aztype, false);
			if (null == azType) throw new ManagerException("Authorizer type could not be loaded");

			this.authorizer = (Activator.CreateInstance(azType) as IAuthorizer);
			if (null == this.authorizer) throw new ManagerException("Authorizer specified in config is not an IAuthorizer");

			this.driver.StartUp();
		}
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
            
            this.Page.Init += new EventHandler(Page_Init);
			this.Page.InitComplete += new EventHandler(this.Page_InitComplete);
			this.Page.LoadComplete += new EventHandler(this.Page_LoadComplete);
			this.Page.PreRenderComplete += new EventHandler(this.Page_PreRenderComplete);
			this.driver.Page = this.Page;
		}
		public override bool IsAuthorized(Type type, string path, string authorizationFilter, bool isShared)
		{
			return !this.EnableAuthorizer || this.authorizer.IsAuthorized(authorizationFilter);
		}
		private void RegisterItem(Control control)
		{
			if (control is GenericWebPart)
			{
				control = ((GenericWebPart)control).ChildControl;
			}
			IInitializable initializableControl = control as IInitializable;
			if (initializableControl == null || this.Initialized)
			{
				this.driver.Initialize(control);
			}
			else
			{
				this.items.RegisterItem(control, initializableControl.Initialization, "initialize");
				this.items.RegisterItem(control, initializableControl.Creation, "create");
				this.items.RegisterItem(control, initializableControl.Change, "change");
				this.items.RegisterItem(control, initializableControl.Completion, "complete");
				initializableControl.RequestInitialization = new RequestInitializationCallback(this.RequestInitialization);
			}
		}

        void Page_Init(object sender, EventArgs e)
        {
            this.driver.Initialize(this);

            if (null != this.RoutingManager)
                this.RoutingManager.Load();

            if (null != this._monitor)
                this._monitor.Start();

            if (null != this.ZonesProvider)
                this.ZonesProvider.RegisterZones();

        }
		private void Page_InitComplete(object sender, EventArgs e)
		{
			foreach (Control control in base.WebParts)
				this.RegisterItem(control);

            SortedList<int, List<Control>> initializations = this.items.ListItems("initialize");
			if (null != initializations)
			{
				foreach (List<Control> list in this.items.ListItems("initialize").Values)
				{
					foreach (Control control in list)
					{
						this.driver.Initialize(control);
					}
				}
			}
			if (null != initializations)
			{
				initializations.Clear();
			}
			this.Initialized = true;
			this.items.OnMilestone("create");
		}
		private void Page_LoadComplete(object sender, EventArgs e)
		{
			this.items.OnMilestone("change");
		}
		private void Page_PreRenderComplete(object sender, EventArgs e)
		{
			this.items.OnMilestone("complete");
		}
		protected override void AddedControl(Control control, int index)
		{
			base.AddedControl(control, index);
			this.RegisterItem(control);
		}
		protected override void RegisterClientScript()
		{
		}
		public void RequestInitialization(object target)
		{
			this.driver.Initialize(target);
			Control control = target as Control;
			if (null != control)
			{
				foreach (List<Control> list in this.items.ListItems("initialize").Values)
				{
					if (list.Contains(control))
					{
						list.Remove(control);
					}
				}
			}
		}
		protected override void Render(HtmlTextWriter writer)
		{
			if (null != _monitor)
				_monitor.Stop();

			base.Render(writer);
		}
		public override void ExportWebPart(WebPart webPart, XmlWriter writer)
		{
			if (webPart is IConfigurableWebPart)
			{
				writer.WriteStartElement("webPart");
				writer.WriteAttributeString("type", SerializationServices.ShortAssemblyQualifiedName(webPart.GetType().AssemblyQualifiedName));
				IConfiguration config = (webPart as IConfigurableWebPart).Configuration;
				SerializationServices.Serialize(config, writer);
				writer.WriteEndElement();
			}
			else
			{
				base.ExportWebPart(webPart, writer);
			}
		}
        public ISymbolWebPart NewSymbolWebPart()
        {
            string type = ConfigurationManager.AppSettings["SYMBOL"];
            if (null == type) throw new ManagerException("Default webpart symbol not set in config");

            Type driverType = Type.GetType(type, false);
            if (null == driverType) throw new ManagerException("Default webpart symbol could not be loaded");

            ISymbolWebPart symbol = (Activator.CreateInstance(driverType) as ISymbolWebPart);
            if (null == symbol) throw new ManagerException("Default webpart symbol specified in config is not an ISymbolWebPart");

            return symbol;
        }
		protected override void SaveCustomPersonalizationState(PersonalizationDictionary state)
		{
			object[] parts = new object[6 * base.WebParts.Count];
			int count = 0;
			foreach (WebPart webPart in base.WebParts)
			{
				parts[count++] = webPart.ID;
				parts[count++] = webPart.Title;
				parts[count++] = SerializationServices.ShortAssemblyQualifiedName(webPart.GetType().AssemblyQualifiedName);
				if (null != webPart.Zone)
				{
					parts[count++] = webPart.Zone.ID;
				}
				else
				{
					parts[count++] = null;
				}
				parts[count++] = (webPart is IContainerWebPart);
				parts[count++] = (webPart is IProxyWebPart);
			}
			if (!state.Contains("lwas.info"))
			{
				state.Add("lwas.info", new PersonalizationEntry(parts, base.Personalization.Scope));
			}
			else
			{
				state["lwas.info"] = new PersonalizationEntry(parts, base.Personalization.Scope);
			}
			base.SaveCustomPersonalizationState(state);
		}

        public string Title
        {
            get;
            set;
        }
    }
}
