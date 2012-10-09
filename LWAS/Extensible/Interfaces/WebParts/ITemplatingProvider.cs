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
using System.Web.UI;

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.DataBinding;

namespace LWAS.Extensible.Interfaces.WebParts
{
	public interface ITemplatingProvider
	{
		void CreateCommanders(Control container, IConfigurationType config, ITemplatable templatable, Dictionary<string, Control> commanders);
		void CreateSelectors(Control container, IConfigurationType config, ITemplatable templatable, Dictionary<string, Control> selectors);
		void CreateFilter(Control container, IConfigurationType config, ITemplatingItemsCollection filters, IBinder binder, ITemplatable templatable);
		void ExtractFilter(Control container, IConfigurationType config, ITemplatingItemsCollection filters);
		void CreateHeader(Control container, IConfigurationType config, ITemplatable templatable);
		void CreateFooter(Control container, IConfigurationType config, ITemplatable templatable);
		void InstantiateGroupIn(Control container, IConfigurationType config, IBinder binder, int itemIndex, ITemplatingItem item, ITemplatable templatable);
		void InstantiateIn(Control container, IConfigurationType config, IBinder binder, int itemIndex, ITemplatingItem item, ITemplatable templatable);
		void InstantiateTotalsIn(Control container, IConfigurationType config, IBinder binder, int itemIndex, ITemplatingItem item, ITemplatable templatable);
		void ExtractItems(Control container, IConfigurationType config, int itemsCount, ITemplatingItemsCollection items);
		void PopulateItem(Control container, IConfigurationType config, ITemplatingItem item, string prefix);
		ITemplatingItem NewTemplatingItemInstance();
	}
}
