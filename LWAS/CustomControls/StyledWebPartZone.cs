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
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace LWAS.CustomControls
{
	public class StyledWebPartZone : WebPartZone
	{
		protected override void RenderHeader(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
			writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2");
			writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
			TitleStyle headerStyle = base.HeaderStyle;
			if (!headerStyle.IsEmpty)
			{
				Style style2 = new Style();
				if (!headerStyle.ForeColor.IsEmpty)
				{
					style2.ForeColor = headerStyle.ForeColor;
				}
				style2.Font.CopyFrom(headerStyle.Font);
				if (!headerStyle.Font.Size.IsEmpty)
				{
					style2.Font.Size = new FontUnit(new Unit(100.0, UnitType.Percentage));
				}
				if (!style2.IsEmpty)
				{
					style2.AddAttributesToRender(writer, this);
				}
			}
			writer.RenderBeginTag(HtmlTextWriterTag.Table);
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);
			HorizontalAlign horizontalAlign = headerStyle.HorizontalAlign;
			if (horizontalAlign != HorizontalAlign.NotSet)
			{
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(HorizontalAlign));
				writer.AddAttribute(HtmlTextWriterAttribute.Align, converter.ConvertToString(horizontalAlign));
			}
			writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.Write(this.DisplayTitle);
			writer.RenderEndTag();
			writer.RenderEndTag();
			writer.RenderEndTag();
		}
		protected override void RenderBody(HtmlTextWriter writer)
		{
			base.RenderBody(writer);
		}
	}
}
