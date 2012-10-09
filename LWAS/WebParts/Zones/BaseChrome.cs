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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace LWAS.WebParts.Zones
{
	public class BaseChrome : WebPartChrome
	{
		public BaseChrome(WebPartZoneBase zone, WebPartManager manager) : base(zone, manager)
		{
		}
		public override void RenderWebPart(HtmlTextWriter writer, WebPart webPart)
		{
			if (webPart == null)
			{
				throw new ArgumentNullException("webPart");
			}
			base.Zone.PartChromeStyle.AddAttributesToRender(writer, base.Zone);
			writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
			writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
			writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
			if (webPart.Hidden && !base.WebPartManager.DisplayMode.ShowHiddenWebParts)
			{
				writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
			}
			writer.RenderBeginTag(HtmlTextWriterTag.Table);
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			if (base.WebPartManager.DisplayMode == WebPartManager.EditDisplayMode || webPart.ChromeType != PartChromeType.None)
			{
				this.RenderTitleBar(writer, webPart);
			}
			writer.RenderEndTag();
			writer.RenderEndTag();
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);
			base.Zone.PartStyle.AddAttributesToRender(writer, base.Zone);
			writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, base.Zone.PartChromePadding.ToString());
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			this.RenderPartContents(writer, webPart);
			writer.RenderEndTag();
			writer.RenderEndTag();
			writer.RenderEndTag();
		}
		protected virtual void RenderTitleBar(HtmlTextWriter writer, WebPart webPart)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
			writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
			writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
			writer.RenderBeginTag(HtmlTextWriterTag.Table);
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);
			base.Zone.PartTitleStyle.AddAttributesToRender(writer, base.Zone);
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.AddAttribute(HtmlTextWriterAttribute.Title, webPart.Description, true);
			writer.RenderBeginTag(HtmlTextWriterTag.Span);
			writer.WriteEncodedText(webPart.Title);
			writer.RenderEndTag();
			writer.RenderEndTag();
			base.Zone.PartTitleStyle.AddAttributesToRender(writer, base.Zone);
			writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, TextAlign.Right.ToString());
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			if (base.WebPartManager.DisplayMode == WebPartManager.EditDisplayMode)
			{
				this.RenderVerbs(writer, webPart);
			}
			writer.RenderEndTag();
			writer.RenderEndTag();
			writer.RenderEndTag();
		}
		protected virtual void RenderVerbs(HtmlTextWriter writer, WebPart webPart)
		{
			if (webPart.AllowEdit)
			{
				VerbLink editLink = new VerbLink(base.Zone, "edit:" + webPart.ID);
				editLink.Text = "edit";
				writer.Write("&nbsp;");
				this.RenderVerbControl(writer, editLink);
			}
			if (webPart.AllowEdit && webPart.AllowClose)
			{
				VerbLink deleteLink = new VerbLink(base.Zone, "delete:" + webPart.ID);
				deleteLink.Text = "delete";
				writer.Write("&nbsp;");
				this.RenderVerbControl(writer, deleteLink);
			}
			if (webPart.ExportMode != WebPartExportMode.None)
			{
				HyperLink exportLink = new HyperLink();
				exportLink.Text = "export";
				exportLink.NavigateUrl = base.WebPartManager.ResolveUrl("~/download/Export.aspx" + HttpContext.Current.Request.Url.Query + "&p=" + webPart.ID);
				writer.Write("&nbsp;");
				this.RenderVerbControl(writer, exportLink);
			}
			if (webPart.AllowEdit && webPart.ExportMode != WebPartExportMode.None)
			{
				VerbLink importLink = new VerbLink(base.Zone, "import:" + webPart.ID);
				importLink.Text = "import";
				writer.Write("&nbsp;");
				this.RenderVerbControl(writer, importLink);
			}
		}
		protected virtual void RenderVerbControl(HtmlTextWriter writer, WebControl control)
		{
			control.ApplyStyle(base.Zone.TitleBarVerbStyle);
			control.Page = base.WebPartManager.Page;
			control.RenderControl(writer);
		}
	}
}
