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
		public virtual void CreateCommanders(Control container, IConfigurationType config, ITemplatable templatable, Dictionary<string, Control> commanders)
		{
			CommandersTemplate.Instance.Create(container, config, templatable, commanders, this.manager);
		}
		public virtual void CreateSelectors(Control container, IConfigurationType config, ITemplatable templatable, Dictionary<string, Control> selectors)
		{
			SelectorsTemplate.Instance.Create(container, config, templatable, selectors, this.manager);
		}
		public virtual void CreateFilter(Control container, IConfigurationType config, ITemplatingItemsCollection filters, IBinder binder, ITemplatable templatable)
		{
			FiltersTemplate.Instance.Create(container, config, templatable, filters, binder, this.manager);
		}
		public virtual void ExtractFilter(Control container, IConfigurationType config, ITemplatingItemsCollection filters)
		{
			FiltersTemplate.Instance.Extract(container, config, filters, this.manager);
		}
		public virtual void CreateHeader(Control container, IConfigurationType config, ITemplatable templatable)
		{
			HeaderTemplate.Instance.Create(container, config, templatable, this.manager);
		}
		public virtual void CreateFooter(Control container, IConfigurationType config, ITemplatable templatable)
		{
			FooterTemplate.Instance.Create(container, config, templatable, this.manager);
		}
		public virtual void InstantiateGroupIn(Control container, IConfigurationType config, IBinder binder, int itemIndex, ITemplatingItem item, ITemplatable templatable)
		{
			GroupingTemplate.Instance.Create(container, config, templatable, binder, item, itemIndex, this.manager);
		}
		public virtual void InstantiateIn(Control container, IConfigurationType config, IBinder binder, int itemIndex, ITemplatingItem item, ITemplatable templatable)
		{
			ItemTemplate.Instance.Create(container, config, templatable, binder, item, itemIndex, this.manager);
		}
		public virtual void InstantiateTotalsIn(Control container, IConfigurationType config, IBinder binder, int itemIndex, ITemplatingItem item, ITemplatable templatable)
		{
			TotalsTemplate.Instance.Create(container, config, templatable, binder, item, itemIndex, this.manager);
		}
		public virtual void ExtractItems(Control container, IConfigurationType config, int itemsCount, ITemplatingItemsCollection items)
		{
			ItemTemplate.Instance.Extract(container, config, null, null, items, null, -1, itemsCount, this.manager);
		}
		public ITemplatingItem NewTemplatingItemInstance()
		{
			return new TemplatingItem();
		}
		public void PopulateItem(Control container, IConfigurationType config, ITemplatingItem item, string prefix)
		{
			ItemTemplate.Instance.PopulateItem(container, config, item, prefix);
		}
	}
}
