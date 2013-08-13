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
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LWAS.CustomControls
{
	public class Link : HyperLink
	{
		private Dictionary<string, object> _parameters = new Dictionary<string, object>();
		public Dictionary<string, object> Parameters
		{
			get
			{
				return this._parameters;
			}
			set
			{
                if (null == value)
                    this._parameters = new Dictionary<string, object>();
                else
                    this._parameters = value;
			}
		}
		public string AddParameter
		{
			set
			{
				this._parameters.Add(value, null);
			}
		}
		protected override void Render(HtmlTextWriter writer)
		{
			string url = base.NavigateUrl;
			bool first = true;
			if (url.IndexOf("?") < 0 && this.Parameters.Count > 0)
			{
				url += "?";
			}
			else
			{
				first = false;
			}
			foreach (string parameter in this.Parameters.Keys)
			{
				url += (first ? "" : "&");
				url = url + HttpUtility.UrlEncode(parameter) + "=";
				url += ((this.Parameters[parameter] == null) ? string.Empty : HttpUtility.UrlEncode(this.Parameters[parameter].ToString()));
				if (first)
				{
					first = false;
				}
			}
			base.NavigateUrl = url;
			base.Render(writer);
		}
	}
}
