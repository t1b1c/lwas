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
using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;

namespace LWAS.WebParts.Zones
{
	public class CustomChromeZone : WebPartZone
	{
		public class PostBackEventArgs : CancelEventArgs
		{
			public string EventArgument;
			public PostBackEventArgs(string eventArgs)
			{
				this.EventArgument = eventArgs;
			}
		}
		public bool RenderDivs = false;
		private WebPartChrome _chrome;
		public event EventHandler<CustomChromeZone.PostBackEventArgs> PostBackEvent;
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			this._chrome = this.CreateWebPartChrome();
		}
		protected override WebPartChrome CreateWebPartChrome()
		{
			if (null == this._chrome)
			{
				if (!this.RenderDivs)
				{
					this._chrome = new BaseChrome(this, base.WebPartManager);
				}
				else
				{
					this._chrome = new DivChrome(this, base.WebPartManager);
				}
			}
			return this._chrome;
		}
		protected override void RaisePostBackEvent(string eventArgument)
		{
			CustomChromeZone.PostBackEventArgs pbea = new CustomChromeZone.PostBackEventArgs(eventArgument);
			if (null != this.PostBackEvent)
			{
				this.PostBackEvent(this, pbea);
			}
			if (!pbea.Cancel)
			{
				base.RaisePostBackEvent(eventArgument);
			}
		}
	}
}
