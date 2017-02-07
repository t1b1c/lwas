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
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Infrastructure;

namespace LWAS.Expressions
{
	public class ExpressionsManager : IExpressionsManager, IPageComponent, IReporter
	{
		private string _title = "ExpressionsManager";
		private IMonitor _monitor;
		private Dictionary<string, IToken> _registry = new Dictionary<string, IToken>();
		private Page _page;
		public string Title
		{
			get
			{
				return this._title;
			}
			set
			{
			}
		}
		public IMonitor Monitor
		{
			get
			{
				return this._monitor;
			}
			set
			{
				this._monitor = value;
			}
		}
		public Dictionary<string, IToken> Registry
		{
			get
			{
				return this._registry;
			}
		}
		public IToken Add
		{
			set
			{
				this._registry.Add(value.Key, value);
			}
		}
		public Page Page
		{
			get
			{
				return this._page;
			}
			set
			{
				this._page = value;
			}
		}
		public IToken Token(string key)
		{
			if (null == this._monitor)
			{
				throw new ApplicationException("Monitor not set");
			}
			IToken token = null;
			if (!this._registry.ContainsKey(key))
			{
				this._monitor.Register(this, this._monitor.NewEventInstance(string.Format("Unknown expression key '{0}'", key), EVENT_TYPE.Error));
			}
			else
			{
				try
				{
					token = (IToken)ReflectionServices.CreateInstance(this._registry[key].GetType().AssemblyQualifiedName);
				}
				catch (Exception ex)
				{
					this._monitor.Register(this, this._monitor.NewEventInstance(string.Format("failed to create token '{0}'", key), null, ex, EVENT_TYPE.ServerFailure));
				}
			}
			return token;
		}
		public object FindControl(string id)
		{
			if (null == this._page)
			{
                return null;
				//throw new InvalidOperationException("Page not set");
			}
			WebPartManager manager = WebPartManager.GetCurrentWebPartManager(this._page);
			if (null == manager)
			{
				throw new InvalidOperationException("This page has no WebPartManager");
			}
			return ReflectionServices.FindControlEx(id, manager);
		}
	}
}
