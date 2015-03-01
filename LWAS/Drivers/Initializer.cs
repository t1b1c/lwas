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
using System.Configuration;
using System.Web.UI;

using LWAS.Extensible.Interfaces;
using LWAS.Infrastructure;

namespace LWAS.Drivers
{
	public class Initializer : IDriver, IPageComponent
	{
		private StartUpSection startUpSection;
		private InitializerSection initializerSection;
		private Dictionary<string, object> singletons;
		private IWatchdog _watchdog;
		private Page _page;
		public Dictionary<string, object> Singletons
		{
			get
			{
				return this.singletons;
			}
		}
		public IWatchdog Watchdog
		{
			get
			{
				return this._watchdog;
			}
			set
			{
				this._watchdog = value;
				if (null != this._page)
				{
					this._watchdog.Page = this._page;
				}
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
				if (null != this._watchdog)
				{
					this._watchdog.Page = this._page;
				}
			}
		}
		public Initializer()
		{
			this.startUpSection = (ConfigurationManager.GetSection("StartUpSection") as StartUpSection);
			this.initializerSection = (ConfigurationManager.GetSection("InitializerSection") as InitializerSection);
			this.singletons = new Dictionary<string, object>();
		}
		public virtual void StartUp()
		{
			foreach (ComponentElement componentElement in this.startUpSection.StartUps)
			{
				this.Initialize(ReflectionServices.StaticInstance(componentElement.Type, componentElement.Member));
			}
		}
		public virtual void Initialize(object target)
		{
			if (null == target)
			{
				throw new ArgumentNullException("target");
			}
			string key = target.GetType().FullName;
			if (target is IPageComponent)
			{
				((IPageComponent)target).Page = this.Page;
			}
			if (target is IWatchdogSubscriber)
			{
				((IWatchdogSubscriber)target).Watchdog = this._watchdog;
			}
			if (this.initializerSection.Components.Contains(key))
			{
				ComponentElement componentElement = this.initializerSection.Components[key];
				foreach (PropertyElement propertyElement in componentElement.PropertiesCollection)
				{
					if (string.IsNullOrEmpty(propertyElement.Value))
					{
						foreach (ComponentElement item in propertyElement.ItemsCollection)
						{
							ReflectionServices.SetValue(target, propertyElement.Name, this.GetProvider(item));
						}
					}
					else
					{
						ReflectionServices.SetValue(target, propertyElement.Name, propertyElement.Value);
					}
				}
				foreach (ComponentElement providerElement in componentElement.ProvidersCollection)
				{
					this.SetProviderToComponent(target, this.GetProvider(providerElement), providerElement);
				}
				if (componentElement.IsSingleton)
				{
					this.singletons.Add(componentElement.Type, target);
				}
			}
			if (target is IInitializable)
			{
				((IInitializable)target).Initialize();
			}
		}
		protected virtual object GetProvider(ComponentElement providerElement)
		{
			object provider = null;
			if (this.singletons.ContainsKey(providerElement.Type))
			{
				provider = this.singletons[providerElement.Type];
			}
			else
			{
				provider = ReflectionServices.CreateInstance(providerElement.Type);
				this.Initialize(provider);
			}
			return provider;
		}
		protected virtual void SetProviderToComponent(object target, object provider, ComponentElement componentElement)
		{
			if (null == target)
			{
				throw new ArgumentNullException("target");
			}
			if (null == componentElement)
			{
				throw new ArgumentNullException("componentElement");
			}
			object value = null;
			if (componentElement.SetOnlyWhenNull)
			{
				value = ReflectionServices.ExtractValue(target, componentElement.Name);
			}
			if (!componentElement.SetOnlyWhenNull || (componentElement.SetOnlyWhenNull && null == value))
			{
				object setObject = provider;
				if (!string.IsNullOrEmpty(componentElement.Member))
				{
					setObject = ReflectionServices.ExtractValue(provider, componentElement.Member);
				}
				ReflectionServices.SetValue(target, componentElement.Name, setObject);
			}
		}
	}
}
