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

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.DataBinding;

namespace LWAS.Extensible.Interfaces.WebParts
{
    public enum TemplatingMode { Grid, Form }

	public interface ITemplatingProvider
	{
        TemplatingMode Mode { get; set; }
        Control InnerContainer { get; set; }
        Control SelectorsHolder { get; }
        Control CommandersHolder { get; }
        Label Message { get; }
        
		void CreateCommanders(IConfigurationType config, ITemplatable templatable, Dictionary<string, Control> commanders);
		void CreateSelectors(IConfigurationType config, ITemplatable templatable, Dictionary<string, Control> selectors);
		void CreateFilter(IConfigurationType config, ITemplatingItemsCollection filters, IBinder binder, ITemplatable templatable);
		void ExtractFilter(IConfigurationType config, ITemplatingItemsCollection filters);
		void CreateHeader(IConfigurationType config, ITemplatable templatable);
		void CreateFooter(IConfigurationType config, ITemplatable templatable);
		void InstantiateGroupIn(IConfigurationType config, IBinder binder, int itemIndex, ITemplatingItem item, ITemplatable templatable);
		void InstantiateIn(IConfigurationType config, IBinder binder, int itemIndex, ITemplatingItem item, ITemplatable templatable);
		void InstantiateTotalsIn(IConfigurationType config, IBinder binder, int itemIndex, ITemplatingItem item, ITemplatable templatable);
		void ExtractItems(IConfigurationType config, int itemsCount, ITemplatingItemsCollection items);
		void PopulateItem(IConfigurationType config, ITemplatingItem item, string prefix);
		ITemplatingItem NewTemplatingItemInstance();

        void Init(Control container, Style messageStyle);
	}
}
