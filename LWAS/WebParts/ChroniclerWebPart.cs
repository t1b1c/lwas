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
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces.WorkFlow;

namespace LWAS.WebParts
{
	public class ChroniclerWebPart : WebPart, IChronicler
	{
		public event MilestoneEventHandler MilestoneHandler;
		public virtual string Milestone
		{
			set
			{
				this.OnMilestone(value);
			}
		}
		public virtual void OnMilestone(string key)
		{
			if (key != null)
			{
				if (!(key == "create"))
				{
					if (!(key == "change"))
					{
						if (key == "complete")
						{
							this.OnComplete();
						}
					}
					else
					{
						this.OnChange();
					}
				}
				else
				{
					this.OnCreate();
				}
			}
			if (null != this.MilestoneHandler)
			{
				this.MilestoneHandler(this, key);
			}
		}
		protected virtual void OnCreate()
		{
		}
		protected virtual void OnChange()
		{
		}
		protected virtual void OnComplete()
		{
		}
	}
}
