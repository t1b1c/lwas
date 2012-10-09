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
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.WebParts;
using LWAS.Infrastructure;
using LWAS.WebParts.Editors;
using LWAS.WebParts.Templating;

namespace LWAS.WebParts.Zones
{
	public class SymbolWebPart : BindableWebPart, ISymbolWebPart
	{
		private string _symbolOf;
		public string SymbolOf
		{
			get
			{
				return this._symbolOf;
			}
			set
			{
				this._symbolOf = value;
			}
		}
		public WebPart Instantiate(WebPartZone zone, int index)
		{
			if (string.IsNullOrEmpty(this._symbolOf))
			{
				throw new InvalidOperationException("SymbolOf is empty");
			}
			WebPart newWebPart = null;
			Control ctrl = null;
			if (ControlFactory.Instance.KnownTypes.ContainsKey(this._symbolOf))
			{
				ctrl = ControlFactory.Instance.CreateControl(this._symbolOf, new bool?(false), zone);
			}
			else
			{
				ctrl = (ReflectionServices.CreateInstance(this._symbolOf) as Control);
			}
			if (null == ctrl)
			{
				throw new InvalidOperationException(string.Format("Cannot create instance of {0}", this._symbolOf));
			}
			if (ctrl is WebPart)
			{
				newWebPart = base.WebPartManager.AddWebPart((WebPart)ctrl, zone, index);
			}
			else
			{
				newWebPart = base.WebPartManager.AddWebPart(base.WebPartManager.CreateWebPart(ctrl), zone, index);
			}
			((Manager)base.WebPartManager).RequestInitialization(newWebPart);
			if (newWebPart is IConfigurableWebPart)
			{
				string app = this.Page.Request["a"];
				IConfiguration config = BuildManager.GetConfigurationStatic(app + "." + this.ID);
				if (null != config)
				{
					((IConfigurableWebPart)newWebPart).Configuration = config;
				}
			}
			if (newWebPart is IInitializable)
			{
				((IInitializable)newWebPart).Initialize();
			}
			newWebPart.AllowClose = true;
			newWebPart.AllowHide = false;
			newWebPart.AllowMinimize = false;
			newWebPart.Title = this.Title;
			return newWebPart;
		}
	}
}
