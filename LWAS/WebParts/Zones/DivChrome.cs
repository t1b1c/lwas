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

namespace LWAS.WebParts.Zones
{
	public class DivChrome : BaseChrome
	{
		public DivChrome(WebPartZoneBase zone, WebPartManager manager) : base(zone, manager)
		{
		}
		public override void RenderWebPart(HtmlTextWriter writer, WebPart webPart)
		{
			if (webPart == null)
			{
				throw new ArgumentNullException("webPart");
			}
			base.Zone.PartChromeStyle.AddAttributesToRender(writer, base.Zone);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "divzone_part_chrome");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			if (base.WebPartManager.DisplayMode == WebPartManager.EditDisplayMode || webPart.ChromeType != PartChromeType.None || !webPart.Hidden)
			{
				this.RenderTitleBar(writer, webPart);
			}
			base.Zone.PartStyle.AddAttributesToRender(writer, base.Zone);
			if (webPart.Hidden && !base.WebPartManager.DisplayMode.ShowHiddenWebParts)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "divzone_part_hidden");
			}
			else
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "divzone_part");
			}
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            this.RenderPartContents(writer, webPart);
            writer.RenderEndTag();
			writer.RenderEndTag();
		}
		protected override void RenderTitleBar(HtmlTextWriter writer, WebPart webPart)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "divzone_part_title");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			base.Zone.PartTitleStyle.AddAttributesToRender(writer, base.Zone);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "divzone_part_title_text");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.AddAttribute(HtmlTextWriterAttribute.Title, webPart.Description, true);
			writer.WriteEncodedText(webPart.Title);
			writer.RenderEndTag();
			base.Zone.PartTitleStyle.AddAttributesToRender(writer, base.Zone);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "divzone_part_title_verbs");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			if (base.WebPartManager.DisplayMode == WebPartManager.EditDisplayMode)
			{
				this.RenderVerbs(writer, webPart);
			}
			writer.RenderEndTag();
			writer.RenderEndTag();
		}
	}
}
