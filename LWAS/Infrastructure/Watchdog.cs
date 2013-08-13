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
using System.Web.UI;

using LWAS.Extensible.Interfaces;

namespace LWAS.Infrastructure
{
	public class Watchdog : IWatchdog, IPageComponent
	{
		private Page _page;
		public event EventHandler WatchIt;
		public Page Page
		{
			get
			{
				return this._page;
			}
			set
			{
				this._page = value;
				this._page.Error += new EventHandler(this._page_Error);
			}
		}
		private void _page_Error(object sender, EventArgs e)
		{
			if (null != this.WatchIt)
			{
				this.WatchIt(this, null);
			}
		}
	}
}
