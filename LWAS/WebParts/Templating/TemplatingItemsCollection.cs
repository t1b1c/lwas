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

using LWAS.Extensible.Interfaces.WebParts;
using LWAS.Infrastructure;

namespace LWAS.WebParts.Templating
{
	public class TemplatingItemsCollection : List<ITemplatingItem>, ITemplatingItemsCollection, IList<ITemplatingItem>, ICollection<ITemplatingItem>, IEnumerable<ITemplatingItem>, IEnumerable
	{
		public string GroupingMap
		{
			get
			{
				string map = string.Empty;
				foreach (ITemplatingItem item in this)
				{
					if (item.IsGrouping)
					{
						map += "1";
					}
					else
					{
						map += "0";
					}
				}
				return map;
			}
		}
		protected virtual Dictionary<string, object> PrepareData(object data)
		{
			return ReflectionServices.ToDictionary(data);
		}
		public virtual ITemplatingItem Add(bool readOnly, bool isNew, bool isCurrent, bool isValid)
		{
			ITemplatingItem item = new TemplatingItem();
			item.IsReadOnly = readOnly;
			item.IsNew = isNew;
			item.IsCurrent = isCurrent;
			item.IsValid = isValid;
			base.Add(item);
			return item;
		}
		public virtual ITemplatingItem Add(bool readOnly, bool isNew, bool isCurrent, bool isValid, object data)
		{
			ITemplatingItem item = new TemplatingItem();
			item.IsReadOnly = readOnly;
			item.IsNew = isNew;
			item.IsCurrent = isCurrent;
			item.IsValid = isValid;
			item.Data = this.PrepareData(data);
			base.Add(item);
			return item;
		}
	}
}
