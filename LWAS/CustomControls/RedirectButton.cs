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

namespace LWAS.CustomControls
{
	public class RedirectButton : OneClickButton
	{
		private string _url;
		public string Url
		{
			get
			{
				return this._url;
			}
			set
			{
				this._url = value;
			}
		}
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
		}
		protected override void OnClick(EventArgs e)
		{
            Page.Items[this.UniqueID] = true;
			base.OnClick(e);
		}
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.Page.PreRenderComplete += new EventHandler(this.Page_PreRenderComplete);
        }
		private void Page_PreRenderComplete(object sender, EventArgs e)
		{
			if (null != Page.Items[this.UniqueID] && (bool)Page.Items[this.UniqueID])
			{
				this.OnRedirect();
			}
		}
		protected virtual void OnRedirect()
		{
			//throw new ApplicationException(string.Format("redirect to '{0}'", this._url));
            this.Page.Response.Redirect(_url, true);
		}
	}
}
