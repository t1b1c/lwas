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
using System.Web.UI.WebControls.WebParts;

using LWAS.CustomControls.DataControls;
using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.WebParts;
using LWAS.Extensible.Interfaces.WorkFlow;

namespace LWAS.WebParts
{
	public class SelectorWebPart : BindableWebPart, IProxyWebPart, IReporter
	{
		private string _activeTarget;
		private IMonitor _monitor;
		private Selector _selector;
		private bool _standalone = false;

		public string ActiveTarget
		{
			get { return _activeTarget; }
			set { _activeTarget = value; }
		}

		public IMonitor Monitor
		{
			get { return _monitor; }
			set { _monitor = value; }
		}

		public Selector Selector
		{
			get
			{
				if (null == _selector)
				{
					this._selector = new Selector(this.Binder);
					this._selector.Init += new EventHandler(this.Selector_Init);
					this._selector.MilestoneHandler += new MilestoneEventHandler(this.Selector_Milestone);
					this._selector.ErrorHandler += new EventHandler<Selector.SelectorErrorArgs>(this.Selector_ErrorHandler);
				}
				return _selector;
			}
		}

		[Personalizable]
		public bool Standalone
		{
			get { return _standalone; }
			set { _standalone = value; }
		}

		public SelectorWebPart()
		{
			base.Initialization = -1;
			base.Change = -1;
			this.Hidden = true;
			this.ExportMode = WebPartExportMode.All;
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			if (_standalone)
			{
				this.Selector.ID = "selector";
				this.Controls.Add(this.Selector);
			}
		}

		protected void Selector_Init(object sender, EventArgs e)
		{
			if (!this.IsInitialized)
				base.RequestInitialization(this);

            this.ConfigurationParser.Parse(this);
			if (this.Selector.CriteriaStorage.Rows.Count == 0)
				this.Selector.CriteriaStorage.Rows.Add(this.Selector.CriteriaStorage.NewRow());
		}

		protected void Selector_Milestone(IChronicler chronicler, string key)
		{
			this.OnMilestone(key);
		}

		protected void Selector_ErrorHandler(object sender, Selector.SelectorErrorArgs e)
		{
			this.Monitor.Register(this, this.Monitor.NewEventInstance("selector error", null, e.Exception, EVENT_TYPE.Error));
		}

		public override void OnMilestone(string key)
		{
			if ("show" == key)
				this.Selector.Show();
			else if ("hide" == key)
				this.Selector.Hide();

            base.OnMilestone(key);
		}

        protected override void OnChange()
        {
            if (null != this.Selector)
                this.Selector.CompleteWithData();
        }

		public override void Initialize()
		{
			if (null == this.Configuration) throw new MissingConfigurationProviderException();
			if (null == this.ConfigurationParser) throw new MissingProviderException("configuration parser");
			if (null == this.Binder) throw new MissingProviderException("data binder");
			if (null == this.Monitor) throw new MissingProviderException("monitor");

			base.Initialize();
		}
	}
}
