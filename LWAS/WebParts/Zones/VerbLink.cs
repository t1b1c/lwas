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
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace LWAS.WebParts.Zones
{
	public class VerbLink : LinkButton
	{
		private WebPartZoneBase _zone;
		private string _arg;
		public VerbLink(WebPartZoneBase zone, string arg)
		{
			this._zone = zone;
			this._arg = arg;
		}
		protected override PostBackOptions GetPostBackOptions()
		{
			return new PostBackOptions(this._zone, this._arg)
			{
				RequiresJavaScriptProtocol = true
			};
		}
	}
}
