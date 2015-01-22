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
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Extensible.Interfaces.WebParts;

namespace LWAS.WebParts.Templating
{
	public class TemplatingProvider : ITemplatingProvider, IPageComponent
	{
		private WebPartManager manager = null;
		private Page _page;
		public Page Page
		{
			get
			{
				return this._page;
			}
			set
			{
				this._page = value;
				this.manager = WebPartManager.GetCurrentWebPartManager(this._page);
			}
		}

        public TemplatingMode Mode { get; set; }

        private Control _container;
        public Control InnerContainer
        {
            get { return _container; }
            set { _container = value; }
        }

        private Control _selectorsHolder;
        public Control SelectorsHolder
        {
            get { return _selectorsHolder; }
        }

        private Control _commandersHolder;
        public Control CommandersHolder
        {
            get { return _commandersHolder; }
        }

        private Label _message;
        public Label Message
        {
            get { return _message; }
        }

        public void Init(Control target, Style messageStyle)
        {

            UpdatePanel messageUpdatePanel = new UpdatePanel();
            messageUpdatePanel.ID = "messageUpdatePanel";
            Table messageTable = new Table();
            TableRow messageRow = new TableRow();
            TableCell messageCell = new TableCell();
            messageCell.ApplyStyle(messageStyle);
            _message = new Label();
            messageCell.Controls.Add(this._message);
            messageRow.Cells.Add(messageCell);
            messageTable.Rows.Add(messageRow);
            messageUpdatePanel.ContentTemplateContainer.Controls.Add(messageTable);
            target.Controls.Add(messageUpdatePanel);

            UpdatePanel commandersUpdatePanel = new UpdatePanel();
            commandersUpdatePanel.ID = "commandersUpdatePanel";
            Panel commandersWrapper = new Panel();
            commandersWrapper.CssClass = "table-responsive";
            commandersUpdatePanel.ContentTemplateContainer.Controls.Add(commandersWrapper);
            _commandersHolder = new Table();
            _commandersHolder.ID = "commandersHolder";
            ((Table)_commandersHolder).CssClass = "";
            commandersWrapper.Controls.Add(_commandersHolder);
            target.Controls.Add(commandersUpdatePanel);

            _selectorsHolder = new Table();
            _selectorsHolder.ID = "selectorsHolder";
            target.Controls.Add(this._selectorsHolder);
            if (null == this._container)
            {
                UpdatePanel updatePanel = new UpdatePanel();
                updatePanel.ID = "containerUpdatePanel";
                Panel wrapper = new Panel();
                wrapper.CssClass = "table-responsive";
                updatePanel.ContentTemplateContainer.Controls.Add(wrapper);
                Table innerTable = new Table();
                innerTable.ID = "innerTable";
                innerTable.CssClass = "table table-striped table-hover table-condensed";
                wrapper.Controls.Add(innerTable);
                target.Controls.Add(updatePanel);
                _container = innerTable;
            }
        }

		public virtual void CreateCommanders(IConfigurationType config, ITemplatable templatable, Dictionary<string, Control> commanders)
		{
			CommandersTemplate.Instance.Create(_commandersHolder, config, templatable, commanders, this.manager);
		}
		public virtual void CreateSelectors(IConfigurationType config, ITemplatable templatable, Dictionary<string, Control> selectors)
		{
			SelectorsTemplate.Instance.Create(_selectorsHolder, config, templatable, selectors, this.manager);
		}
		public virtual void CreateFilter(IConfigurationType config, ITemplatingItemsCollection filters, IBinder binder, ITemplatable templatable)
		{
			FiltersTemplate.Instance.Create(_container, config, templatable, filters, binder, this.manager);
		}
		public virtual void ExtractFilter(IConfigurationType config, ITemplatingItemsCollection filters)
		{
			FiltersTemplate.Instance.Extract(_container, config, filters, this.manager);
		}
		public virtual void CreateHeader(IConfigurationType config, ITemplatable templatable)
		{
			HeaderTemplate.Instance.Create(_container, config, templatable, this.manager);
		}
		public virtual void CreateFooter(IConfigurationType config, ITemplatable templatable)
		{
			FooterTemplate.Instance.Create(_container, config, templatable, this.manager);
		}
		public virtual void InstantiateGroupIn(IConfigurationType config, IBinder binder, int itemIndex, ITemplatingItem item, ITemplatable templatable)
		{
			GroupingTemplate.Instance.Create(_container, config, templatable, binder, item, itemIndex, this.manager);
		}
		public virtual void InstantiateIn(IConfigurationType config, IBinder binder, int itemIndex, ITemplatingItem item, ITemplatable templatable)
		{
			ItemTemplate.Instance.Create(_container, config, templatable, binder, item, itemIndex, this.manager);
		}
		public virtual void InstantiateTotalsIn(IConfigurationType config, IBinder binder, int itemIndex, ITemplatingItem item, ITemplatable templatable)
		{
			TotalsTemplate.Instance.Create(_container, config, templatable, binder, item, itemIndex, this.manager);
		}
		public virtual void ExtractItems(IConfigurationType config, int itemsCount, ITemplatingItemsCollection items)
		{
			ItemTemplate.Instance.Extract(_container, config, null, null, items, null, -1, itemsCount, this.manager);
		}
		public ITemplatingItem NewTemplatingItemInstance()
		{
			return new TemplatingItem();
		}
		public void PopulateItem(IConfigurationType config, ITemplatingItem item, string prefix)
		{
			ItemTemplate.Instance.PopulateItem(_container, config, item, prefix);
		}
	}
}
