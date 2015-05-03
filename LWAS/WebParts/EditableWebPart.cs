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
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces.WebParts;

namespace LWAS.WebParts
{
	public class EditableWebPart : TranslatableWebPart, IEditableWebPart
	{
		private ArrayList editorParts;
		public virtual EditorPart EditorPart
		{
			set
			{
				if (null != value)
				{
					if (null == this.editorParts)
					{
						this.editorParts = new ArrayList();
					}
					value.ID = this.ID + "-EditorPart" + this.editorParts.Count;
					this.editorParts.Add(value);
				}
			}
		}
		public virtual void BeginEdit()
		{
		}
		public virtual void EndEdit()
		{
			this.Page.Response.Redirect(this.Page.Request.RawUrl, false);
		}
		public override EditorPartCollection CreateEditorParts()
		{
			if (!this.IsInitialized)
			{
				if (null == base.RequestInitialization)
				{
					throw new InitializationException("Can't request initialization");
				}
				base.RequestInitialization(this);
			}
			return new EditorPartCollection(this.editorParts);
		}
		protected override void Render(HtmlTextWriter writer)
		{
			if (this.Hidden)
			{
				base.Style.Add("display", "none");
			}
			base.Render(writer);
		}
	}
}
