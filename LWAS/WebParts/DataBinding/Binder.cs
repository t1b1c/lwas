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

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.DataBinding;

namespace LWAS.WebParts.DataBinding
{
	public class Binder : IBinder, IProvider, IInitializable, ILifetime
	{
		private BindingItemsCollection _bindingItems = new BindingItemsCollection();
		private Dictionary<string, IBindingItemsCollection> _bindingSet = new Dictionary<string, IBindingItemsCollection>();
		private int _initialization;
		private int _creation;
		private int _change;
		private int _completion;
		private RequestInitializationCallback _requestInitialization;
		public IBindingItemsCollection BindingItems
		{
			get
			{
				return this._bindingItems;
			}
			set
			{
				this._bindingItems = new BindingItemsCollection(value);
			}
		}
		public Dictionary<string, IBindingItemsCollection> BindingSet
		{
			get
			{
				return this._bindingSet;
			}
			set
			{
				this._bindingSet = value;
			}
		}
		public IConfiguration ConfigurationProvider
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		public int Initialization
		{
			get
			{
				return this._initialization;
			}
			set
			{
				this._initialization = value;
			}
		}
		public int Creation
		{
			get
			{
				return this._creation;
			}
			set
			{
				this._creation = value;
			}
		}
		public int Change
		{
			get
			{
				return this._change;
			}
			set
			{
				this._change = value;
			}
		}
		public int Completion
		{
			get
			{
				return this._completion;
			}
			set
			{
				this._completion = value;
			}
		}
		public RequestInitializationCallback RequestInitialization
		{
			get
			{
				return this._requestInitialization;
			}
			set
			{
				this._requestInitialization = value;
			}
		}
		public IResult Bind()
		{
			return this.Bind(this._bindingItems);
		}
		public IResult Bind(string set)
		{
			IResult result;
			if (this._bindingSet.ContainsKey(set))
			{
				result = this.Bind(this._bindingSet[set]);
			}
			else
			{
				result = null;
			}
			return result;
		}
		public IResult Bind(IBindingItemsCollection bindingItemsCollection)
		{
			if (null == bindingItemsCollection)
			{
				throw new ArgumentNullException("bindingItemsCollection");
			}
			BindingResult result = new BindingResult();
			foreach (IBindingItem item in bindingItemsCollection)
			{
				result.Concatenate(this.Bind(item));
			}
			return result;
		}
		public IResult Bind(IBindingItem bindingItem)
		{
			if (null == bindingItem)
			{
				throw new ArgumentNullException("bindingItem");
			}
			return bindingItem.Bind();
		}
		public IBindingItem NewBindingItemInstance()
		{
			return new BindingItem();
		}
		public IBindingItemsCollection NewBindingItemCollectionInstance()
		{
			return new BindingItemsCollection();
		}
		public void Initialize()
		{
		}
	}
}
