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
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.Translation;
using LWAS.Extensible.Interfaces.WebParts;
using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Extensible.Interfaces.Validation;
using LWAS.Infrastructure;
using LWAS.CustomControls;
using LWAS.CustomControls.DataControls.Events;

namespace LWAS.CustomControls.DataControls
{
    public class Container : CompositeControl
    {
		public enum OperationType
		{
			Viewing,
			Editing,
			Inserting,
			Deleting,
			Selecting
		}
        public struct CheckDefinition
        {
            public string Error;
            public string Milestone;
            public IExpression Expression;
        }
		private HiddenField operationHidden;
		private IConfigurationType _templateConfig;
		private int _itemsCount;
		private ITemplatingItemsCollection _items;
		private string[] _filter;
		private ITemplatingItemsCollection _filterItems;
		private string _command;
        private Dictionary<string, List<CheckDefinition>> _checks = new Dictionary<string, List<CheckDefinition>>();
		private Dictionary<string, Control> _commanders = new Dictionary<string, Control>();
		private Dictionary<string, Control> _selectors = new Dictionary<string, Control>();
		private Dictionary<string, object> _dataSources = new Dictionary<string, object>();
		private Paginater _paginater;
		private bool _disablePaginater = false;
		private bool _disableFilters = false;
		private IMonitor _monitor;
		private string _groupByMember;
		private string _totalsByMembers;
		private ITemplatingItem previousGroupItem = null;
		private ITemplatingItem runningTotalsItem = null;
		private bool pageChanged = false;
		protected bool IsRecovered = false;
		protected bool NeedsBuild = true;
		private bool foundNewSources = false;
		protected bool NeedsRefresh = false;
		protected bool NeedsSave = false;
		protected bool NeedsDelete = false;
		private bool IsReset = false;
		protected bool IsPersisted = false;
		private ITemplatingProvider _template;

        private IDataSource _dataSource;
        public event EventHandler<MilestoneEventArgs> Milestone;
        public event EventHandler RequestData;
        public event EventHandler<RecoverEventArgs> Recover;
        public event EventHandler<InsertEventArgs> Insert;
        public event EventHandler<UpdateEventArgs> Update;
        public event EventHandler<DeleteEventArgs> Delete;
        public event EventHandler<CheckEventArgs> Check;

		public OperationType Operation
		{
			get
			{
				OperationType result;
				if (string.IsNullOrEmpty(this.operationHidden.Value))
				{
					result = OperationType.Viewing;
				}
				else
				{
					result = (OperationType)Enum.Parse(typeof(OperationType), this.operationHidden.Value);
				}
				return result;
			}
			set { this.operationHidden.Value = value.ToString(); }
		}
		public IConfigurationType TemplateConfig
		{
			get { return this._templateConfig; }
			set { this._templateConfig = value; }
		}
		public virtual IDataSource DataSource
		{
			get { return _dataSource; }
			set { _dataSource = value; }
		}
		public ITemplatingItemsCollection Items
		{
			get { return this._items; }
			set { this._items = value; }
		}
		public string Filter
		{
			set
			{
				this._filter = value.Split(new char[] { ',' });
				Dictionary<string, object> data = new Dictionary<string, object>();
				string[] filter = this._filter;
				for (int i = 0; i < filter.Length; i++)
				{
					string member = filter[i];
					data.Add(member, null);
				}
				this._filterItems.Add(false, true, true, true, data);
			}
		}
		public ITemplatingItemsCollection FilterItems
		{
			get { return this._filterItems; }
			set { this._filterItems = value; }
		}
		public ITemplatingItem CurrentItem
		{
			get
			{
				ITemplatingItem result;
				for (int i = 0; i < this._items.Count; i++)
				{
					if (this._items[i].IsCurrent)
					{
						result = this._items[i];
						return result;
					}
				}
				result = null;
				return result;
			}
		}
        public ITemplatingItem ContextItem
        {
            get;
            set;
        }
		public object Data
		{
			get
			{
				object result;
				if (null != this.CurrentItem)
    				result = this.CurrentItem.Data;
				else
					result = null;

                return result;
			}
		}
		public string Command
		{
			get { return this._command; }
			set
			{
				this._command = value;
				this.OnCommand(this._command);
			}
		}
        public Dictionary<string, List<CheckDefinition>> Checks
		{
			get { return this._checks; }
			set { this._checks = value; }
		}
        
        public bool PassLastCheck { get; set; }

		public Dictionary<string, Control> Commanders
		{
			get { return this._commanders; }
			set { this._commanders = value; }
		}
		public Dictionary<string, Control> Selectors
		{
			get { return this._selectors; }
			set { this._selectors = value; }
		}
		public Dictionary<string, object> DataSources
		{
			get { return this._dataSources; }
		}
		
		public Label Message
		{
			get { return this.Template.Message; }
		}
		public Paginater Paginater
		{
			get { return this._paginater; }
			set { this._paginater = value; }
		}
		public bool DisablePaginater
		{
			get { return this._disablePaginater; }
			set
			{
				this._paginater.Visible = false;
				this._disablePaginater = value;
			}
		}
		public bool DisableFilters
		{
			get { return this._disableFilters; }
			set { this._disableFilters = value; }
		}
		public IMonitor Monitor
		{
			get { return this._monitor; }
			set { this._monitor = value; }
		}
		public string GroupByMember
		{
			get { return this._groupByMember; }
			set { this._groupByMember = value; }
		}
		public string TotalsByMembers
		{
			get { return this._totalsByMembers; }
			set { this._totalsByMembers = value; }
		}
		public IEnumerable ReceiveData
		{
			set { this.OnReceiveData(value); }
		}
        public IEnumerable RawData { get; set; }
        public DataSet FilteredData
        {
            get { return ApplyFilterOnData(); }
        }
		public ITemplatingProvider Template
		{
			get { return this._template; }
			set { this._template = value; }
		}

        public TableItemStyle MessageStyle { get; set; }
        public IBinder Binder { get; set; }
        public IValidationManager ValidationManager { get; set; }
        public IReporter Reporter { get; set; }
        public ITemplatable Templatable { get; set; }
        public ITranslator Translator { get; set; }

        protected bool IsNewGroupItem(ITemplatingItem item)
		{
			bool isNew = this.CheckGroupingLevel(item);
			if (isNew)
			{
				if (this.previousGroupItem != null && !string.IsNullOrEmpty(this._totalsByMembers))
				{
					this._items.Insert(this._items.IndexOf(item), this.runningTotalsItem);
				}
				this.previousGroupItem = item;
				if (!string.IsNullOrEmpty(this._totalsByMembers))
				{
					this.InitiateRunningTotals();
				}
			}
			this.ComputeRunningTotals(item);
			return isNew;
		}
		protected bool CheckGroupingLevel(ITemplatingItem item)
		{
			bool result = false;
			if (string.IsNullOrEmpty(this._groupByMember))
			{
				result = false;
			}
			else
			{
				if (null == this.previousGroupItem)
				{
					result = true;
				}
				else
				{
                    Dictionary<string, object> oldValues = new Dictionary<string, object>();
                    Dictionary<string, object> newValues = new Dictionary<string, object>();
                    foreach (string member in _groupByMember.Split(','))
                    {
                        oldValues.Add(member, ReflectionServices.ExtractValue(this.previousGroupItem.Data, member));
                        newValues.Add(member, ReflectionServices.ExtractValue(item.Data, member));
                    }
                    foreach (KeyValuePair<string, object> entry in oldValues)
                    {
                        string oldstr = entry.Value == null ? null : entry.Value.ToString();
                        string newstr = newValues[entry.Key] == null ? null : newValues[entry.Key].ToString();
                        if (oldstr != newstr)
                        {
                            result = true;
                            break;
                        }
                    }
				}
			}
			return result;
		}
		protected void InitiateRunningTotals()
		{
			if (!string.IsNullOrEmpty(this._totalsByMembers))
			{
				this.runningTotalsItem = this._template.NewTemplatingItemInstance();
				this.runningTotalsItem.Data = new Dictionary<string, decimal>();
				this.runningTotalsItem.IsTotals = true;
			}
		}
		protected void ComputeRunningTotals(ITemplatingItem item)
		{
			if (!string.IsNullOrEmpty(this._totalsByMembers))
			{
				Dictionary<string, decimal> data = this.runningTotalsItem.Data as Dictionary<string, decimal>;
				string[] totals = this._totalsByMembers.Split(new char[] { ',' });
				string[] array = totals;
				for (int i = 0; i < array.Length; i++)
				{
					string member = array[i];
					object objectval = ReflectionServices.ExtractValue(item.Data, member);
					decimal val = 0m;
					if (objectval != DBNull.Value)
					{
						val = (decimal)ReflectionServices.StrongTypeValue(objectval, typeof(decimal));
					}
					if (data.ContainsKey(member))
					{
						Dictionary<string, decimal> dictionary;
						string key;
						(dictionary = data)[key = member] = dictionary[key] + val;
					}
					else
					{
						data.Add(member, val);
					}
				}
			}
		}
		protected void FinishRunningTotals()
		{
			if (null != this.runningTotalsItem)
			{
				this._items.Add(this.runningTotalsItem);
			}
		}
		protected bool IsFilterMatch(object data)
		{
			bool result;
			if (this._filter == null || 0 == this._filter.Length)
			{
				result = true;
			}
			else
			{
				if (0 == this._filterItems.Count)
				{
					result = true;
				}
				else
				{
					bool isMatch = true;
					foreach (ITemplatingItem item in this._filterItems)
					{
						string[] filter = this._filter;
						for (int i = 0; i < filter.Length; i++)
						{
							string member = filter[i];
                            object comparedValue = null;
                            try
                            {
                                comparedValue = ReflectionServices.ExtractValue(data, member);
                            }
                            catch (Exception ex)
                            {
                                // if the filter is not found on the source, silently skip it
                                continue;
                            }
                            object filterValue = ReflectionServices.ExtractValue(item.Data, member);

							if (filterValue != null && "" != filterValue.ToString())
							{
								if (null == comparedValue)
								{
									isMatch = false;
								}
								else
								{
									decimal filterNumber;
									if (decimal.TryParse(filterValue.ToString(), out filterNumber))
									{
										decimal comparedNumber;
                                        if (decimal.TryParse(comparedValue.ToString(), out comparedNumber))
                                        {
                                            if (item.BoundControls.ContainsKey(member) && filterNumber <= 0)
                                                isMatch = true;
                                            else
                                                isMatch = (filterNumber == comparedNumber);
                                        }
                                        else
                                            isMatch = false;
									}
									else
									{
										Regex re = new Regex(filterValue.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
										isMatch = re.IsMatch(comparedValue.ToString());
									}
								}
							}
							else
							{
								isMatch = true;
							}
							if (!isMatch)
							{
								break;
							}
						}
						if (isMatch)
						{
							break;
						}
					}
					result = isMatch;
				}
			}
			return result;
		}
		public virtual void InitEx()
		{
            this.Template.Init(this, this.MessageStyle);

            // paginater
            if (null == _paginater)
            {
                _paginater = new Paginater();
                this.Paginater.Changed += new EventHandler(this.Paginater_Changed);
                this.Controls.Add(_paginater);
            }

            // operation state
            if (null == operationHidden)
            {
                UpdatePanel operationUpdatePanel = new UpdatePanel();
                operationUpdatePanel.ID = "operationUpdatePanel";
                this.Controls.Add(operationUpdatePanel);
                operationHidden = new HiddenField();
                operationHidden.ID = "operationHidden";
                operationUpdatePanel.ContentTemplateContainer.Controls.Add(this.operationHidden);
            }
		}
		private void Paginater_Changed(object sender, EventArgs e)
		{
            this.pageChanged = true;
            this.OnMilestone("view");
			this.OnView();
			this.OnMilestone("page");
		}
		protected virtual bool OnCommand(string commandstring)
		{
            bool ret = true;
            string[] cmds = commandstring.Split(',');

            foreach (string initialcommand in cmds)
            {
                string command = initialcommand.Trim();

                if ("save" == command)
                    this.OnMilestone("saving");
                else if ("delete" == command)
                    this.OnMilestone("deleting");

                ret = OnCheck(command);
                if (ret)
                {
                    switch (command)
                    {
                        case "view":
                            OnView();
                            _paginater.Reset();
                            break;
                        case "edit":
                            OnEdit();
                            break;
                        case "save":
                            NeedsSave = true;
                            break;
                        case "delete":
                            NeedsDelete = true;
                            break;
                        case "insert":
                            OnInsert();
                            break;
                        case "reset":
                            OnReset();
                            break;
                    }

                    if (null != command)
                        OnMilestone(command);
                }
            }

            return ret;
		}
		protected virtual bool OnCheck(string command)
		{
			if (String.IsNullOrEmpty(command))
				return false;

            if (null != _checks &&
                _checks.ContainsKey(command))
            {
                List<CheckDefinition> list = _checks[command];
                if (list == null || list.Count == 0)
                    return true;
                else
                {
                    foreach (CheckDefinition check in list)
                    {
                        string error = check.Error;
                        if (string.IsNullOrEmpty(error)) throw new InvalidOperationException(string.Format("Check on command '{0}' has no error message", command));

                        IExpression expression = check.Expression;
                        if (null != expression && !expression.Evaluate().IsSuccessful())
                        {
                            _monitor.Register(this.Reporter, this._monitor.NewEventInstance(string.Format("container check '{0}' error", command), null, new ApplicationException(error), EVENT_TYPE.Error));
                            return false;
                        }
                        else if (!String.IsNullOrEmpty(check.Milestone))
                        {
                            this.PassLastCheck = true;
                            OnMilestone(check.Milestone);
                            if (!this.PassLastCheck)
                                _monitor.Register(this.Reporter, this._monitor.NewEventInstance(string.Format("container check '{0}' error", command), null, new ApplicationException(error), EVENT_TYPE.Error));
                            return this.PassLastCheck;
                        }

                    }
                }
            }

            if (null != this.Check)
            {
                CheckEventArgs cea = new CheckEventArgs(command);
                Check(this, cea);
                if (!cea.Pass)
                    return false;
            }

			return true;
		}
		protected virtual void OnCommanders()
		{
			if (null != _templateConfig)
			{
				_template.CreateCommanders(this._templateConfig, this.Templatable, this._commanders);
			}
		}
		protected virtual void OnSelectors()
		{
			if (null != this._templateConfig)
			{
				this._template.CreateSelectors(this._templateConfig, this.Templatable, this._selectors);
			}
		}
		protected virtual void OnFilter()
		{
            if (null != this._templateConfig)
            {
                this._template.CreateFilter(this._templateConfig, this.FilterItems, this.Binder, this.Templatable);
            }
		}
		protected virtual void OnHeader()
		{
            if (null != this._templateConfig)
            {
                this._template.CreateHeader(this._templateConfig, this.Templatable);
            }
		}
		protected virtual void OnFooter()
		{
            if (null != this._templateConfig)
            {
                this._template.CreateFooter(this._templateConfig, this.Templatable);
            }
		}
		protected virtual void OnInstantiateGroup(ITemplatingItem item)
		{
			if (this.IsRecovered || !this.Page.IsPostBack)
			{
				this._template.InstantiateGroupIn(this._templateConfig, this.Binder, this._items.IndexOf(item), item, this.Templatable);
			}
			else
			{
				this._template.InstantiateGroupIn(this._templateConfig, null, this._items.IndexOf(item), item, this.Templatable);
			}
		}
		protected virtual void OnInstantiate(ITemplatingItem item)
		{
			if (this.IsRecovered || !this.Page.IsPostBack)
			{
				this._template.InstantiateIn(this._templateConfig, this.Binder, this._items.IndexOf(item), item, this.Templatable);
			}
			else
			{
				this._template.InstantiateIn(this._templateConfig, null, this._items.IndexOf(item), item, this.Templatable);
			}
		}
		protected virtual void OnBindControls()
		{
            try
            {
                if (this.foundNewSources)
                    this.OnMilestone("get bound sources");

                foreach (ITemplatingItem item in this._items)
                {
                    string[] keys = new string[_dataSources.Keys.Count];
                    this._dataSources.Keys.CopyTo(keys,0);
                    foreach (string key in keys)
                    {
                        this.ContextItem = item;
                        OnMilestone("get bound source " + key);
                        this.ContextItem = null;
                    }

                    foreach (string key in this._dataSources.Keys)
                    {
                        if (item.BoundControls.ContainsKey(key))
                        {
                            foreach (BaseDataBoundControl boundControl in item.BoundControls[key])
                            {
                                boundControl.DataSource = this._dataSources[key];
                                boundControl.DataBind();
                            }
                        }
                    }
                }
                foreach (ITemplatingItem item in this.FilterItems)
                {
                    foreach (string key in this._dataSources.Keys)
                    {
                        if (item.BoundControls.ContainsKey(key))
                        {
                            foreach (BaseDataBoundControl boundControl in item.BoundControls[key])
                            {
                                boundControl.DataSource = this._dataSources[key];
                                boundControl.DataBind();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this._monitor.Register(this.Reporter, this._monitor.NewEventInstance("bind controls error", null, ex, EVENT_TYPE.Error));
            }
		}
		protected virtual void OnInstantiateTotals(ITemplatingItem item)
		{
			if (this.IsRecovered || !this.Page.IsPostBack)
			{
				this._template.InstantiateTotalsIn(this._templateConfig, this.Binder, this._items.IndexOf(item), item, this.Templatable);
			}
			else
			{
				this._template.InstantiateTotalsIn(this._templateConfig, null, this._items.IndexOf(item), item, this.Templatable);
			}
		}
		protected virtual void OnExtract()
		{
			if (!this.DisableFilters)
			{
				this._template.ExtractFilter(this._templateConfig, this._filterItems);
			}
			this._template.ExtractItems(this._templateConfig, this._itemsCount, this._items);
		}
		protected virtual void OnRebuild()
		{
			char[] groupingMap = null;
			if (!string.IsNullOrEmpty(this.Page.Request[this.UniqueID + "-itemsCount"]))
			{
				int.TryParse(this.Page.Request[this.UniqueID + "-itemsCount"], out this._itemsCount);
			}
			if (!string.IsNullOrEmpty(this.Page.Request[this.UniqueID + "-groupingMap"]))
			{
				groupingMap = this.Page.Request[this.UniqueID + "-groupingMap"].ToCharArray();
			}
			for (int i = 0; i < this._itemsCount; i++)
			{
				ITemplatingItem item = this._items.Add(false, false, false, true);
				if (groupingMap != null && i < groupingMap.Length && '1' == groupingMap[i])
				{
					item.IsGrouping = true;
				}
			}
			this.OnBuild();
			this.NeedsBuild = true;
		}
		protected override void OnLoad(EventArgs e)
		{
			if (this.Page.IsPostBack)
			{
				this.OnRecover();
                OnMilestone("reload");
			}
			base.OnLoad(e);
			if (!this.Page.IsPostBack)
			{
				this.OnMilestone("firstload");
			}
		}
		protected virtual void OnRecover()
		{
            RecoverEventArgs rea = new RecoverEventArgs();
            if (null != this.Recover)
                Recover(this, rea);

            if (rea.Cancel)
                return;

            this._paginater.IsFrozen = (this.Operation != OperationType.Viewing);

			this.OnBindControls();
			this.OnExtract();
			this.IsRecovered = true;
		}
		protected override bool OnBubbleEvent(object source, EventArgs args)
		{
			bool result;
			if (args is CommandEventArgs)
			{
                if (!this.OnCommand((args as CommandEventArgs).CommandName))
                {
                    if (source is RedirectButton)
                        Page.Items[((RedirectButton)source).UniqueID] = false;
                }
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}
		protected virtual void OnBuild()
		{
			this.OnBuild(true);
			this.OnBuild(false);
		}
		protected virtual void OnBuild(bool bStructureOnly)
		{
			if (!bStructureOnly)
			{
				this.Binder.BindingItems.Clear();
			}
			if (bStructureOnly)
			{
				if (this.Template.SelectorsHolder is Table)
                    ((Table)this.Template.SelectorsHolder).Rows.Clear();
				else
                    this.Template.SelectorsHolder.Controls.Clear();

                if (this.Template.CommandersHolder is Table)
                    ((Table)this.Template.CommandersHolder).Rows.Clear();
				else
                    this.Template.CommandersHolder.Controls.Clear();

                this.OnCommanders();
				this.OnSelectors();
			}
			else
			{
				if (this.Template.InnerContainer is Table)
                    ((Table)this.Template.InnerContainer).Rows.Clear();
				else
                    this.Template.InnerContainer.Controls.Clear();

                if (!this.DisableFilters)
				{
					this.OnFilter();
				}

				this.OnHeader();
				
                foreach (ITemplatingItem item in this._items)
				{
                    this.ContextItem = item;
					if (item.IsGrouping)
						this.OnInstantiateGroup(item);

                    if (item.IsTotals)
						this.OnInstantiateTotals(item);
					else
						this.OnInstantiate(item);

                    this.ContextItem = null;
				}

				this.OnFooter();
			}
			if (!bStructureOnly)
			{
				this.NeedsBuild = false;
				if (this.Items.Count > 0)
				{
					foreach (string key in this.Items[0].BoundControls.Keys)
					{
						if (!this._dataSources.ContainsKey(key))
						{
							this._dataSources.Add(key, null);
							this.foundNewSources = true;
						}
					}
				}
                if (this.FilterItems.Count > 0)
                {
                    foreach (string key in this.FilterItems[0].BoundControls.Keys)
                    {
                        if (!this._dataSources.ContainsKey(key))
                        {
                            this._dataSources.Add(key, null);
                            this.foundNewSources = true;
                        }
                    }
                }
			}
		}
		protected virtual void OnSelect()
		{
			this.Operation = OperationType.Selecting;
			this.OnMilestone("item");
		}
		protected virtual void OnView()
		{
			this.Message.Text = "";
			this.Operation = OperationType.Viewing;
			this.NeedsRefresh = true;
			for (int i = 0; i < this.Items.Count; i++)
			{
				this.Items[i].IsCurrent = false;
			}
		}
		protected virtual void OnInsert()
		{
			ITemplatingItem newItem = this._items.Add(false, true, true, true);
			this._template.PopulateItem(this._templateConfig, newItem, null);
			newItem.HasChanges = true;
			this.Operation = OperationType.Inserting;
			this.OnMilestone("item");
		}
		protected virtual void OnEdit()
		{
			bool hasCurrentItems = false;
			foreach (ITemplatingItem item in this._items)
			{
				if (item.IsCurrent)
				{
					item.IsReadOnly = false;
					item.HasChanges = true;
					hasCurrentItems = true;
				}
			}
			if (hasCurrentItems)
			{
				this.Operation = OperationType.Editing;
			}
		}
		protected virtual void OnSave()
		{
			string savedone = "save done";
			ITranslationResult trares = this.Translator.Translate(null, savedone, null);
			if (trares.IsSuccessful())
			{
				savedone = trares.Translation;
			}
			try
			{
				ITemplatingItem[] arr = new ITemplatingItem[this._items.Count];
				this._items.CopyTo(arr, 0);
				ITemplatingItem[] array = arr;
				for (int i = 0; i < array.Length; i++)
				{
					ITemplatingItem item = array[i];
					if (item.HasChanges)
					{
						this.OnValidate(item);
						if (!item.IsValid)
						{
							break;
						}
						if (item.IsNew)
						{
                            if (null != this.CurrentItem)
                                this.CurrentItem.IsCurrent = false;
                            item.IsCurrent = true;
							this.OnMilestone("save-insert");
						}
						else
						{
                            if (null != this.CurrentItem)
                                this.CurrentItem.IsCurrent = false;
                            item.IsCurrent = true;
                            this.OnMilestone("save-update");
						}
						if (item.IsNew)
						{
                            if (null != this.Insert)
                                Insert(this, new InsertEventArgs((IDictionary)item.Data));
						}
						else
						{
                            if (null != this.Update)
                                Update(this, new UpdateEventArgs((IDictionary)item.Data));
						}
						if (!this._monitor.HasErrors())
						{
							item.IsReadOnly = true;
							item.IsNew = false;
							item.HasChanges = false;
                            this._monitor.Register(this.Reporter, this._monitor.NewEventInstance(savedone, EVENT_TYPE.Info));
						}
						this.NeedsSave = false;
						this.Operation = OperationType.Viewing;
					}
				}

                if (!this._monitor.HasErrors())
                    this.OnMilestone("saved");
			}
			catch (Exception ex)
			{
                this._monitor.Register(this.Reporter, this._monitor.NewEventInstance("save error", null, ex, EVENT_TYPE.Error));
			}
		}
		protected virtual void OnValidate(ITemplatingItem item)
		{
			IResult result = this.ValidationManager.Validate(item.Data);
			if (null != result)
			{
				item.IsValid = result.IsSuccessful();
			}
			if (item.IsValid)
			{
				item.InvalidMember = null;
				this.OnValidationSucceed(result);
			}
			else
			{
				item.InvalidMember = this.ValidationManager.LastFail().Member;
				this.OnValidationFail(result);
			}
		}
		protected virtual void OnValidationFail(IResult result)
		{
			if (result.Exceptions.Count > 0)
			{
				//this._message.Text = result.Exceptions[0].Message;
                this._monitor.Register(this.Reporter, this._monitor.NewEventInstance("validation error", null, result.Exceptions[0], EVENT_TYPE.Error));
			}
		}
		protected virtual void OnValidationSucceed(IResult result)
		{
			this.Message.Text = "";
		}
		protected virtual void OnDelete()
		{
			this.Operation = OperationType.Deleting;
			string deletedone = "delete done";
			ITranslationResult trares = this.Translator.Translate(null, deletedone, null);
			if (trares.IsSuccessful())
			{
				deletedone = trares.Translation;
			}
			List<ITemplatingItem> itemsToRemove = new List<ITemplatingItem>();
			try
			{
				ITemplatingItem[] arr = new ITemplatingItem[this._items.Count];
				this._items.CopyTo(arr, 0);
				ITemplatingItem[] array = arr;
				for (int i = 0; i < array.Length; i++)
				{
					ITemplatingItem item = array[i];
					if (item.IsCurrent)
					{
						this.OnMilestone("save-delete");
						bool deleted = false;
						if (!item.IsNew)
						{
                            if (null != this.Delete)
                                Delete(this, new DeleteEventArgs((IDictionary)item.Data));
							deleted = true;
						}
						this.OnValidationSucceed(null);
						if (deleted && !this._monitor.HasErrors())
						{
							itemsToRemove.Add(item);
                            this._monitor.Register(this.Reporter, this._monitor.NewEventInstance(deletedone, EVENT_TYPE.Info));
						}
						this.NeedsDelete = false;
						this.Operation = OperationType.Viewing;
					}
                }
                if (!this._monitor.HasErrors())
                    this.OnMilestone("deleted");
			}
			catch (Exception ex)
			{
                this._monitor.Register(this.Reporter, this._monitor.NewEventInstance("delete error", null, ex, EVENT_TYPE.Error));
			}
			foreach (ITemplatingItem item in itemsToRemove)
			{
				this._items.Remove(item);
			}

            _paginater.IsFrozen = false;
            _paginater.OnBackward();
            _paginater.OnForward();
		}
		protected virtual void OnReset()
		{
			this.Operation = OperationType.Viewing;
			this.IsReset = true;
			this._paginater.Reset();
		}
		protected virtual void OnRequestData()
		{
            if (null != this.RequestData)
                RequestData(this, new EventArgs());
		}
		protected virtual void OnReceiveData(IEnumerable data)
		{
			this._items.Clear();
            if (null == data)
                _paginater.Reset();
			else if (null != data)
			{
				this.InitiateRunningTotals();
				if (this._disablePaginater)
				{
					IEnumerator enumerator = data.GetEnumerator();
					while (enumerator.MoveNext())
					{
						if (this.IsFilterMatch(enumerator.Current))
						{
							ITemplatingItem item = this._items.Add(true, false, false, true, enumerator.Current);
							item.IsGrouping = this.IsNewGroupItem(item);
						}
					}
				}
				else
				{
					int index = 0;
					IEnumerator enumerator = data.GetEnumerator();
					bool hasMoreData = enumerator.MoveNext();
					bool isEmpty = !hasMoreData;
					if (hasMoreData && this.IsReset)
					{
						this.Paginater.CurrentPage = 1;
					}
					while (index < this._paginater.StartIndex && hasMoreData)
					{
						hasMoreData = enumerator.MoveNext();
						if (this.IsFilterMatch(enumerator.Current))
						{
							index++;
						}
					}
					while (index < this._paginater.StartIndex + this._paginater.PageSize && hasMoreData)
					{
						if (this.IsFilterMatch(enumerator.Current))
						{
							ITemplatingItem item = this._items.Add(true, false, false, true, enumerator.Current);
							item.IsGrouping = this.IsNewGroupItem(item);
							index++;
						}
						hasMoreData = enumerator.MoveNext();
					}
					if (hasMoreData)
					{
						do
						{
							if (this.IsFilterMatch(enumerator.Current))
							{
								index++;
							}
						}
						while (enumerator.MoveNext());
					}
					this._paginater.ResultsCount = (isEmpty ? 0 : index);
				}
				this.FinishRunningTotals();
			}
		}
        protected DataSet ApplyFilterOnData()
        {
            if (null == this.RawData) throw new InvalidOperationException("No RawData upon to apply filter");
            if (!(this.RawData is IDataReader)) throw new InvalidOperationException("Raw data must be a Reader");

            DataSet ds = new DataSet();
            ds.Load((IDataReader)this.RawData, LoadOption.OverwriteChanges, "data");

            if (ds.Tables.Count > 0)
            {
                DataTable table = ds.Tables[0];
                if (table.Rows.Count > 0)
                {
                    DataRow[] rows = new DataRow[table.Rows.Count];
                    table.Rows.CopyTo(rows, 0);
                    foreach (DataRow dr in rows)
                        if (!this.IsFilterMatch(dr))
                            table.Rows.Remove(dr);
                }
            }
            
            return ds;
        }
		protected virtual void OnPersist()
		{
			if (null != this.Page)
			{
				if (null != ScriptManager.GetCurrent(this.Page))
				{
					int count = this._items.Count;
					ScriptManager.RegisterHiddenField(this.Page, this.UniqueID + "-itemsCount", count.ToString());
					ScriptManager.RegisterHiddenField(this.Page, this.UniqueID + "-groupingMap", this._items.GroupingMap);
				}
				else
				{
					int count = this._items.Count;
					this.Page.ClientScript.RegisterHiddenField(this.UniqueID + "-itemsCount", count.ToString());
					this.Page.ClientScript.RegisterHiddenField(this.UniqueID + "-groupingMap", this._items.GroupingMap);
				}
			}
			this.IsPersisted = true;
		}
		public virtual void PerformOperation()
		{
			if (this.NeedsSave)
				this.OnSave();
			if (this.NeedsDelete)
				this.OnDelete();
			if (this.NeedsRefresh)
				this.OnRequestData();
		}
		public  virtual void CompleteWithData()
		{
			this._paginater.IsFrozen = (this.Operation != OperationType.Viewing);
			if (this.NeedsRefresh)
			{
				this.OnRequestData();
			}
			this.OnMilestone("last-build");
			if (this.NeedsBuild)
			{
				this.OnBuild(false);
			}
			
            this.OnBindControls();

			IResult result = this.Binder.Bind();
            foreach (Exception ex in result.Exceptions)
            {
                this._monitor.Register(this.Reporter, this._monitor.NewEventInstance("binding error", null, ex, EVENT_TYPE.Error));
            }

			this.OnMilestone(this.Operation.ToString());
			if (!this.IsPersisted)
			{
				this.OnPersist();
			}
		}

        protected void OnMilestone(string key)
        {
 	        if (null != this.Milestone)
                Milestone(this, new MilestoneEventArgs(key));
        }
        public bool ParentEvent(object source, EventArgs args)
        {
            return OnBubbleEvent(source, args);
        }
        public void Rebuild()
        {
            OnRebuild();
        }
        public void InitProviders(IBinder binder, IValidationManager validationManager, IReporter reporter, ITemplatable templatable, ITranslator translator)
        {
            this.Binder = binder;
            this.ValidationManager = validationManager;
            this.Reporter = reporter;
            this.Templatable = templatable;
            this.Translator = translator;
        }
    }
}
