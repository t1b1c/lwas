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
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Exceptions;

namespace LWAS.WebParts
{
	public class WrapperWebPart : BindableWebPart
	{
		private ITemplate _template;
		[TemplateContainer(typeof(WebPart))]
		public ITemplate Template
		{
			get
			{
				return this._template;
			}
			set
			{
				this._template = value;
			}
		}
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			if (null != this._template)
			{
				this._template.InstantiateIn(this);
			}
		}
		public override void Initialize()
		{
			if (null == this.ConfigurationParser)
			{
				throw new MissingProviderException("configuration parser");
			}
			this.ConfigurationParser.Parse(this);
			base.Initialize();
		}
	}
}
