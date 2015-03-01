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
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Data;

using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.Translation;
using LWAS.Extensible.Interfaces.WebParts;
using LWAS.Infrastructure;
using LWAS.CustomControls;
using LWAS.CustomControls.DataControls;
using LWAS.CustomControls.DataControls.Events;

namespace LWAS.WebParts
{
	public class ContainerWebPart : BindableWebPart, IContainerWebPart, IConfigurableWebPart, IBindableWebPart, IInitializable, ILifetime, ITemplatable, IReporter
	{
        Container _container;
        public Container Container
        {
            get { return _container; }
        }

        public IConfigurationType TemplateConfig
        {
            get { return _container.TemplateConfig; }
            set { _container.TemplateConfig = value; }
        }

		public override IDataSource DataSource
		{
			get { return base.DataSource; }
			set
			{
				base.DataSource = value;
                _container.DataSource = base.DataSource;
				this.OnMilestone("source");
			}
		}
		public ITemplatingItemsCollection Items
		{
            get { return _container.Items; }
            set { _container.Items = value; }
		}
		public string Filter
		{
            set { _container.Filter = value; }
		}
		public ITemplatingItemsCollection FilterItems
		{
			get { return _container.FilterItems; }
            set { _container.FilterItems = value; }
		}
		public ITemplatingItem CurrentItem
		{
            get { return _container.CurrentItem; }
		}
        public ITemplatingItem ContextItem
        {
            get { return _container.ContextItem; }
        }
		public object Data
		{
            get { return _container.Data; }
		}
		public string Command
		{
            get { return _container.Command; }
            set { _container.Command = value; }
		}
		public Dictionary<string, List<Container.CheckDefinition>> Checks
		{
			get { return _container.Checks; }
            set { _container.Checks = value; }
		}
		public Dictionary<string, Control> Commanders
		{
            get { return _container.Commanders; }
            set { _container.Commanders = value; }
		}
		public Dictionary<string, Control> Selectors
		{
            get { return _container.Selectors; }
            set { _container.Selectors = value; }
		}
		public Dictionary<string, object> DataSources
		{
            get { return _container.DataSources; }
		}
		public bool DisablePaginater
		{
			get { return _container.DisablePaginater; }
            set { _container.DisablePaginater = value; }
		}
		public bool DisableFilters
		{
            get { return _container.DisableFilters; }
            set { _container.DisableFilters = value; }
		}

        public TableStyle SelectorsStyle { get; set; }
        public TableStyle CommandersStyle { get; set; }
        public TableStyle FilterStyle { get; set; }
        public TableStyle HeaderStyle { get; set; }
        public TableStyle GroupingStyle { get; set; }
        public TableStyle TotalsStyle { get; set; }
        public TableStyle DetailsStyle { get; set; }
        public TableStyle FooterStyle { get; set; }
        public TableItemStyle SelectorsRowStyle { get; set; }
        public TableItemStyle CommandersRowStyle { get; set; }
        public TableItemStyle FilterRowStyle { get; set; }
        public TableItemStyle HeaderRowStyle { get; set; }
        public TableItemStyle GroupRowStyle { get; set; }
        public TableItemStyle TotalsRowStyle { get; set; }
        public TableItemStyle RowStyle { get; set; }
        public TableItemStyle EditRowStyle { get; set; }
        public TableItemStyle SelectedRowStyle { get; set; }
        public TableItemStyle AlternatingRowStyle { get; set; }
        public TableItemStyle FooterRowStyle { get; set; }
        public TableItemStyle InvalidItemStyle { get; set; }
        public TableItemStyle MessageStyle
        {
            get { return _container.MessageStyle; }
            set { _container.MessageStyle = value; }
        }

		public IMonitor Monitor
		{
            get { return _container.Monitor; }
            set { _container.Monitor = value; }
		}
		public string GroupByMember
		{
            get { return _container.GroupByMember; }
            set { _container.GroupByMember = value; }
		}
		public string TotalsByMembers
		{
            get { return _container.TotalsByMembers; }
            set { _container.TotalsByMembers = value; }
		}
		public IEnumerable ReceiveData
		{
            set { _container.ReceiveData = value; }
		}
        public IEnumerable RawData 
        {
            get { return _container.RawData; }
            set { _container.RawData = value; }
        }
        public DataSet FilteredData
        {
            get { return _container.FilteredData; }
        }
		public ITemplatingProvider Template
		{
            get { return _container.Template; }
            set { _container.Template = value; }
		}
        public bool PassLastCheck
        {
            get { return _container.PassLastCheck; }
            set { _container.PassLastCheck = value; }
        }

		public ContainerWebPart()
		{
            this.ExportMode = WebPartExportMode.All;

            _container = InstantiateContainer();
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

            _container.ID = "container";
            this.Controls.Add(_container);

            _container.Recover += new EventHandler<RecoverEventArgs>(_container_Recover);
            _container.RequestData += new EventHandler(_container_RequestData);
            _container.Milestone += new EventHandler<MilestoneEventArgs>(_container_Milestone);
            _container.Insert += new EventHandler<InsertEventArgs>(_container_Insert);
            _container.Update += new EventHandler<UpdateEventArgs>(_container_Update);
            _container.Delete += new EventHandler<DeleteEventArgs>(_container_Delete);
		}

        protected virtual Container InstantiateContainer()
        {
            return new Container();
        }

        void _container_Recover(object sender, RecoverEventArgs e)
        {
            if (!this.IsInitialized)
            {
                if (null == base.RequestInitialization)
                {
                    e.Cancel = true;
                    return;
                }
                base.RequestInitialization(this);
            }
        }
        void _container_RequestData(object sender, EventArgs e)
        {
            OnRequestData();
        }
        void _container_Milestone(object sender, MilestoneEventArgs e)
        {
            OnMilestone(e.Key);
        }
        void _container_Insert(object sender, InsertEventArgs e)
        {
            base.Insert(e.Data);
        }
        void _container_Update(object sender, UpdateEventArgs e)
        {
            base.Update(null, e.Data, null);
        }
        void _container_Delete(object sender, DeleteEventArgs e)
        {
            base.Delete(e.Data, null);
        }

		protected override bool OnBubbleEvent(object source, EventArgs args)
		{
            return _container.ParentEvent(source, args);
		}

		protected virtual void OnRequestData()
		{
			try
			{
				base.Select(DataSourceSelectArguments.Empty);
			}
			catch (Exception ex)
			{
                _container.Monitor.Register(this, _container.Monitor.NewEventInstance("request data error", null, ex, EVENT_TYPE.Error));
			}
		}
		protected override void DataSourceSelectCallback(IEnumerable data)
		{
            _container.ReceiveData = data;
			base.DataSourceSelectCallback(data);
		}
		protected override void OnChange()
		{
            _container.PerformOperation();
		}
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			_container.CompleteWithData();
		}

		public override void Initialize()
		{
			if (null == this.Configuration) throw new MissingProviderException("Configuration");
			if (null == this.ConfigurationParser) throw new MissingProviderException("Configuration parser");
			if (null == this.Binder) throw new MissingProviderException("Binder");
			if (null == this.Template) throw new MissingProviderException("Templating");
			if (null == base.ValidationManager)throw new MissingProviderException("ValidationManager");
			if (null == base.ExpressionsManager)throw new MissingProviderException("ExpressionsManager");
			if (null == this.Items)throw new InitializationException("Items collection not set");
			if (null == this.FilterItems)throw new InitializationException("FilterItems collection not set");
			if (null == this.Monitor)throw new MissingProviderException("Monitor");

            _container.InitEx();
            base.TranslationTargets.Clear();
            base.TranslationTargets.Add("_message", _container.Message);
			this.ConfigurationParser.Parse(this);
			base.Initialize();
            _container.InitProviders(this.Binder, this.ValidationManager, this, this, this.Translator);
			_container.Rebuild();
		}
	}
}
