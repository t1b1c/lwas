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

using LWAS.Extensible.Interfaces.WorkFlow;

namespace LWAS.Infrastructure
{
	public class ManagerItems
	{
		private Dictionary<string, SortedList<int, List<Control>>> items = new Dictionary<string, SortedList<int, List<Control>>>();
		public void RegisterItem(Control item, int priority, string milestone)
		{
			if (null == item)
			{
				throw new ArgumentNullException("item");
			}
			if (string.IsNullOrEmpty(milestone))
			{
				throw new ArgumentNullException("milestone");
			}
			if (!this.items.ContainsKey(milestone))
			{
				this.items.Add(milestone, new SortedList<int, List<Control>>());
			}
			SortedList<int, List<Control>> milestoneItems = this.items[milestone];
			if (!milestoneItems.ContainsKey(priority))
			{
				milestoneItems.Add(priority, new List<Control>());
			}
			List<Control> milestonePriorityItems = milestoneItems[priority];
			if (!milestonePriorityItems.Contains(item))
			{
				milestonePriorityItems.Add(item);
			}
		}
		public void RemoveItem(Control item, string milestone)
		{
			if (null == item)
			{
				throw new ArgumentNullException("item");
			}
			if (string.IsNullOrEmpty(milestone))
			{
				throw new ArgumentNullException("milestone");
			}
			if (this.items.ContainsKey(milestone))
			{
				SortedList<int, List<Control>> milestoneItems = this.items[milestone];
				foreach (List<Control> milestonePriorityItems in milestoneItems.Values)
				{
					if (milestonePriorityItems.Contains(item))
					{
						milestonePriorityItems.Remove(item);
					}
				}
			}
		}
		public SortedList<int, List<Control>> ListItems(string milestone)
		{
			if (string.IsNullOrEmpty(milestone))
			{
				throw new ArgumentNullException("milestone");
			}
			SortedList<int, List<Control>> result;
			if (!this.items.ContainsKey(milestone))
			{
				result = null;
			}
			else
			{
				result = this.items[milestone];
			}
			return result;
		}
		public void OnMilestone(string milestone)
		{
			if (string.IsNullOrEmpty(milestone))
			{
				throw new ArgumentNullException("milestone");
			}
			if (this.items.ContainsKey(milestone))
			{
				foreach (List<Control> milestonePriorityItems in this.items[milestone].Values)
				{
					foreach (Control item in milestonePriorityItems)
					{
						if (item is IChronicler)
						{
							(item as IChronicler).OnMilestone(milestone);
						}
					}
				}
			}
		}
	}
}
