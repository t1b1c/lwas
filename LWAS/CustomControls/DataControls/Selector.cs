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
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using LWAS.Extensible.Interfaces.WorkFlow;
using LWAS.Extensible.Interfaces.DataBinding;

namespace LWAS.CustomControls.DataControls
{
	public class Selector : Panel, IChronicler, INamingContainer
	{
		public class SelectorErrorArgs : EventArgs
		{
			public Exception Exception;
			public SelectorErrorArgs(Exception anException)
			{
				this.Exception = anException;
			}
		}
		private string _title;
		private Label _titleLabel;
		private string _searchText;
		private string _okText;
		private string _cancelText;
		private Panel _container;
		private Panel _buttonsPanel;
		private LinkButton _link;
		private OneClickButton _searchButton;
		private Button _okButton;
		private Button _cancelButton;

        protected UpdatePanelDynamic UpdatePanel;
		
        private FormView _criteria;
		private DataTableDataSource _criteriaDataSource;
		private DataTable _criteriaStorage;
		private DataGridView _results;
		private object _criteriaItem;
		private object _selectorSource;
		private object _selectedItem;
		private HiddenField resultsSelectedIndexHidden;
		private IDictionary<string, object> _boundControls = new Dictionary<string, object>();
		private IDictionary<string, object> _dataSources = new Dictionary<string, object>();
		private bool ResetSelectIndex = false;
		private bool shouldShow = false;
		private bool shouldHide = false;
		public event MilestoneEventHandler MilestoneHandler;
		public event EventHandler<Selector.SelectorErrorArgs> ErrorHandler;
        HiddenField visibilityHidden;

        public bool IsShown
        {
            get
            {
                bool ret = false;
                bool.TryParse(visibilityHidden.Value, out ret);
                return ret;
            }
            set { visibilityHidden.Value = value.ToString(); }
        }

		public virtual string Milestone
		{
			set { this.OnMilestone(value); }
		}

		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				_titleLabel.Text = _title;
			}
		}

		[Themeable(true)]
		private Label TitelLabel
		{
			get { return _titleLabel; }
			set { _titleLabel = value; }
		}

		[Themeable(true)]
		public string SearchText
		{
			get { return _searchText; }
			set { _searchText = value; }
		}

		[Themeable(true)]
		public string OkText
		{
			get { return _okText; }
			set { _okText = value; }
		}

		[Themeable(true)]
		public string CancelText
		{
			get { return _cancelText; }
			set { _cancelText = value; }
		}

		[Themeable(true)]
		public Panel Container
		{
			get { return _container; }
			set { _container = value; }
		}

		[Themeable(true)]
		public Panel ButtonsPanel
		{
			get { return _buttonsPanel; }
			set { _buttonsPanel = value; }
		}

		[Themeable(true)]
		public LinkButton Link
		{
			get { return _link; }
			set { _link = value; }
		}

		[Themeable(true)]
		public OneClickButton SearchButton
		{
			get { return _searchButton; }
			set { _searchButton = value; }
		}

		[Themeable(true)]
		public Button OkButton
		{
			get { return _okButton; }
			set { _okButton = value; }
		}

		[Themeable(true)]
		public Button CancelButton
		{
			get { return _cancelButton; }
			set { _cancelButton = value; }
		}

		public FormView Criteria
		{
			get { return _criteria; }
			set { _criteria = value; }
		}

		public Dictionary<string, Control> CriteriaFields
		{
			get
			{
				Dictionary<string, Control> fields = new Dictionary<string, Control>();
				TemplateHelper templateHelper = _criteria.EditItemTemplate as TemplateHelper;
				if (null != templateHelper)
				{
					templateHelper.BuildControlsMap(fields);
				}
				return fields;
			}
		}

		public DataTableDataSource CriteriaDataSource
		{
			get { return _criteriaDataSource; }
			set { _criteriaDataSource = value; }
		}

		public DataTable CriteriaStorage
		{
			get
			{
				if (null == _criteriaStorage)
				{
					_criteriaStorage = new DataTable("CriteriaStorage");
				}
				return _criteriaStorage;
			}
			set
			{
				_criteriaStorage = value;
				this.OnMilestone("storage");
			}
		}
		
        [Themeable(true)]
		public DataGridView Results
		{
			get { return _results; }
			set { _results = value; }
		}

		public object CriteriaItem
		{
			get { return _criteriaItem; }
			set { _criteriaItem = value; }
		}

		public object SelectorSource
		{
			get { return _selectorSource; }
			set
			{
				_selectorSource = value;
				if (null != _results)
				{
					_results.DataSource = _selectorSource;
				}
				this.OnMilestone("source");
			}
		}

		public object SelectedItem
		{
			get { return _selectedItem; }
			set
			{
				_selectedItem = value;
                if (selectRaised)
                    this.OnMilestone("item");
			}
		}

		public bool Reset
		{
			set
			{
				if (value)
				{
					_results.SelectedIndex = -1;
					_results.Empty();
					this.resultsSelectedIndexHidden.Value = null;
				}
			}
		}

		public IDictionary<string, object> BoundControls
		{
			get { return _boundControls; }
		}

		public IDictionary<string, object> DataSources
		{
			get { return _dataSources; }
		}

		public bool ReadData
		{
			set
			{
                if (value && _criteria != null && !string.IsNullOrEmpty(_criteria.DataSourceID))
                {
                    _criteria.DataBind();
                    foreach (IBindingItem bindingItem in this.Binder.BindingItems)
                        bindingItem.Source = _criteria.DataItem;
                    this.Binder.Bind();
                }
			}
		}

        public IBinder Binder { get; set; }

        public Selector(IBinder binder)
        {
            this.Binder = binder;
        }

        public virtual void OnMilestone(string key)
		{
			if (null != this.MilestoneHandler)
				this.MilestoneHandler(this, key);
		}

        protected virtual void OnCriteria(object criteriaItem, bool raiseMilestone)
        {
            _criteriaItem = criteriaItem;
            if (raiseMilestone)
                this.OnMilestone("criteria");
        }

		protected override void OnInit(EventArgs e)
		{
            // selector's own launcher
            UpdatePanelDynamic linkUpdatePanel = new UpdatePanelDynamic();
            this.Controls.Add(linkUpdatePanel);
			_link = new LinkButton();
			_link.ID = "selectorLauncher";
			_link.Text = "";
            linkUpdatePanel.ContentTemplateContainer.Controls.Add(_link);
            this.Link.Click += new EventHandler(Link_Click);

            // hiddens
            this.UpdatePanel = new UpdatePanelDynamic();
            this.UpdatePanel.ID = "UpdatePanel";
            visibilityHidden = new HiddenField();
            visibilityHidden.ID = "visibilityHidden";
            this.UpdatePanel.ContentTemplateContainer.Controls.Add(visibilityHidden);
            _criteriaDataSource = new DataTableDataSource();
            _criteriaDataSource.ID = "criteriaDataSource";
            this.UpdatePanel.ContentTemplateContainer.Controls.Add(_criteriaDataSource);
            this.resultsSelectedIndexHidden = new HiddenField();
            this.resultsSelectedIndexHidden.ID = "resultsSelectedIndexHidden";
            this.UpdatePanel.ContentTemplateContainer.Controls.Add(this.resultsSelectedIndexHidden);
            
            // bootstrap framing
            _container = new Panel();
			_container.ID = "selectorContainer";
            _container.CssClass = "modal";
            _container.Attributes["tabindex"] = "-1";
            _container.Attributes["role"] = "dialog";
            _container.Attributes["aria-hidden"] = "true";
            _container.Attributes["data-show"] = "false";
            this.UpdatePanel.ContentTemplateContainer.Controls.Add(_container);

            _container.Controls.Add(new LiteralControl(@"
  <div class=""modal-dialog"">
    <div class=""modal-content"">
      <div class=""modal-header"">
"));
            _container.Controls.Add(new LiteralControl(@"<div class=""row"">"));
            _container.Controls.Add(new LiteralControl(@"<div class=""col-xs-12"">"));
            _container.Controls.Add(new LiteralControl("<h3>"));
			_titleLabel = new Label();
            _titleLabel.CssClass = "modal-title";
            _container.Controls.Add(_titleLabel);
            _container.Controls.Add(new LiteralControl("</h3>"));
            _container.Controls.Add(new LiteralControl(@"</div>")); // col
            _container.Controls.Add(new LiteralControl(@"</div>")); // row

            _container.Controls.Add(new LiteralControl(@"<div class=""row"">"));
            Panel criteriaPanel = new Panel();
			criteriaPanel.CssClass = "col-xs-12";
			_criteria = new FormView();
			_criteria.ID = "Criteria";
			_criteria.DefaultMode = FormViewMode.Edit;
			criteriaPanel.Controls.Add(_criteria);
            _container.Controls.Add(criteriaPanel);

            _container.Controls.Add(new LiteralControl(@"</div>")); // row
            _container.Controls.Add(new LiteralControl(@"<div class=""row"">"));

			Panel searchButtonPanel = new Panel();
            searchButtonPanel.CssClass = "col-xs-12";
			_searchButton = new OneClickButton();
			_searchButton.ID = "searchButton";
			_searchButton.Text = _searchText;
			_searchButton.Click += new EventHandler(this.searchButton_Click);
            _searchButton.CssClass = "btn btn-primary";
			searchButtonPanel.Controls.Add(_searchButton);
            _container.Controls.Add(searchButtonPanel);

            _container.Controls.Add(new LiteralControl(@"</div>")); // row

            _container.Controls.Add(new LiteralControl(@"
      </div>
      <div class=""modal-body"">
"));
			Panel resultsContainer = new Panel();
			resultsContainer.ID = "resultsContainer";
            resultsContainer.CssClass = "table-responsive";
			_results = new DataGridView();
			_results.ID = "Results";
            _results.CssClass = "table table-striped table-condensed gridview lwas-selector-results";
			_results.AutoGenerateSelectButton = true;
			
            if (null != _selectorSource)
				_results.DataSource = _selectorSource;

            resultsContainer.Controls.Add(_results);
			_container.Controls.Add(resultsContainer);

            _container.Controls.Add(new LiteralControl(@"
      </div>
      <div class=""modal-footer"">
"));

            _buttonsPanel = new Panel();
            _buttonsPanel.ID = "buttonsPanel";
            _container.Controls.Add(_buttonsPanel);

			_okButton = new Button();
			_okButton.ID = "okButton";
			_okButton.Text = _okText;
            _okButton.CssClass = "btn btn-default";
			_buttonsPanel.Controls.Add(_okButton);
            _okButton.Click += (s, ee) =>
                        {
                            this.Hide();
                        };

			_cancelButton = new Button();
			_cancelButton.ID = "cancelButton";
			_cancelButton.Text = _cancelText;
            _cancelButton.CssClass = "btn btn-default";
            _buttonsPanel.Controls.Add(_cancelButton);
            _cancelButton.Click += (s, ee) =>
                            {
                                this.Hide();
                            };

            _container.DefaultButton = "searchButton";

            _container.Controls.Add(new LiteralControl(@"
      </div>
    </div>
  </div>
"));
            this.Controls.Add(this.UpdatePanel);
			
            if (shouldShow)
				DoShow();
			if (shouldHide)
				DoHide();

            base.OnInit(e);
			this.Results.SelectedIndexChanged += new EventHandler(this.Results_SelectedIndexChanged);
		}

        void Link_Click(object sender, EventArgs e)
        {
            Show();
        }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			_criteriaDataSource.DataTable = _criteriaStorage;
			_criteria.DataSourceID = this.CriteriaDataSource.ID;
			_criteria.DataBind();

			this.OnMilestone("get bound sources");
			
            foreach (string key in _dataSources.Keys)
			{
				if (_boundControls.ContainsKey(key) && _boundControls[key] is BaseDataBoundControl)
				{
					((BaseDataBoundControl)_boundControls[key]).DataSource = _dataSources[key];
				}
				else
				{
					if (_boundControls.ContainsKey(key) && _boundControls[key] is DropDownListField)
					{
						((DropDownListField)_boundControls[key]).DataSource = _dataSources[key];
					}
				}
			}

            int selidx = -2;
			if (!string.IsNullOrEmpty(this.resultsSelectedIndexHidden.Value))
				int.TryParse(this.resultsSelectedIndexHidden.Value, out selidx);
			if (-2 != selidx)
				_results.SelectedIndex = selidx;
		}

        bool selectRaised = false;
		private void Results_SelectedIndexChanged(object sender, EventArgs e)
		{
            this.Container.Attributes["data-show"] = "false";
            this.OnMilestone("select");
            selectRaised = true;
			this.resultsSelectedIndexHidden.Value = _results.SelectedIndex.ToString();
			this.ResetSelectIndex = true;
		}

		private void searchButton_Click(object sender, EventArgs e)
		{
			this.Results.SelectedIndex = -1;
			this.OnMilestone("search");
		}

		public virtual void CompleteWithData()
		{
            if (null != _criteria && this.IsShown)
            {
                _criteria.UpdateItem(false);
                if (null != _criteria.DataItem)
                {
                    OnCriteria(_criteria.DataItem, true);
                    this.ReadData = true;
                    OnCriteria(_criteria.DataItem, false);
                }
            }
			if (null != this.Results && null != this.Results.DataSource)
			{
				try
				{
					this.Results.DataBind();
				}
				catch (Exception ex)
				{
					if (null == this.ErrorHandler)
						throw ex;
					this.ErrorHandler(this, new Selector.SelectorErrorArgs(ex));
				}
			}
			if (this.SelectedItem == null && this.Results != null && this.Results.Rows.Count > this.Results.SelectedIndex && null != this.Results.SelectedRow)
				this.SelectedItem = this.Results.GetSelectedRowData();
			if (this.ResetSelectIndex)
				this.Results.SelectedIndex = -1;
		}

		public void Show()
		{
			if (null != this.Container)
				this.DoShow();
			shouldShow = true;
            this.IsShown = true;
		}

		private void DoShow()
		{
            this.Container.Attributes["data-show"] = "true";
            ScriptManager.GetCurrent(this.Page).SetFocus(_criteria);
		}

		public void Hide()
		{
			if (null != this.Container)
				this.DoHide();
			shouldHide = true;
            this.IsShown = false;
		}

		private void DoHide()
		{
            this.Container.Attributes["data-show"] = "false";
		}

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!this.IsShown)
                DoHide();

            // Won't render to the client when visible toggles to true
            // selector is not in an update panel
            ////if (!this.IsShown && String.IsNullOrEmpty(this.Link.Text))
            ////    this.Visible = false;
        }
	}
}
