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
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using AjaxControlToolkit;

namespace LWAS.WebParts.Zones
{
	public class TabZone : TableZone
	{
        private HiddenField _activeWebPartHidden;
        public string ActiveWebPart
        {
            get
            {
                return this._activeWebPartHidden.Value;
            }
            set
            {
                this._activeWebPartHidden.Value = value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this._activeWebPartHidden = new HiddenField();
            this.Controls.Add(this._activeWebPartHidden);
        }
        protected override void RaisePostBackEvent(string eventArgument)
        {
            if (!string.IsNullOrEmpty(eventArgument))
            {
                string[] args = eventArgument.Split(new char[]
				{
					':'
				});
                string verb = args[0];
                if ("activate" == verb)
                {
                    this.ActiveWebPart = args[1];
                }
            }
            base.RaisePostBackEvent(eventArgument);
        }
        protected override void OnPreRender(EventArgs e)
        {
            string script = "";
            string parts = "";
            string firstPart = null;
            foreach (TableRow row in base.Table.Rows)
            {
                foreach (TableCell cell in row.Cells)
                {
                    WebPart webPart = null;
                    string containerID = this.CreateContainerID(cell.ID);
                    if (base.ContainersParts.ContainsKey(containerID))
                    {
                        webPart = base.WebParts[base.ContainersParts[containerID]];
                        if (String.IsNullOrEmpty(firstPart))
                            firstPart = webPart.ID;
                    }
                    if (null != webPart)
                        parts += "'" + containerID + "',";
                }
            }
            if (!String.IsNullOrEmpty(parts))
                parts = parts.Substring(0, parts.LastIndexOf(","));

            if (String.IsNullOrEmpty(this.ActiveWebPart))
                this.ActiveWebPart = firstPart;

            if (!String.IsNullOrEmpty(parts))
            {
                script += "var tabs = [" + parts + "];  \n";
                script += "function Toggle(id){  \n";
                script += "    for(i = 0; i < tabs.length; i++) \n";
                script += "        document.getElementById(tabs[i]).className = 'tabzone_inactivetab';  \n";
                script += "    document.getElementById(id).className = 'tablezone_container'; \n";
                script += "}    \n";
                ToolkitScriptManager.RegisterClientScriptBlock(this, this.GetType(), "toggle tabs", script, true);
            }

            base.OnPreRender(e);
        }
        protected override void Render(HtmlTextWriter writer)
        {
            if (this.WebPartManager.DisplayMode != WebPartManager.EditDisplayMode &&
                this.WebParts.Count == 0)
                return;

            base.Render(writer);
        }
        protected override void RenderBody(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "tabzone_tabcontainer");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            foreach (TableRow row in base.Table.Rows)
            {
                foreach (TableCell cell in row.Cells)
                {
                    WebPart webPart = null;
                    string containerID = this.CreateContainerID(cell.ID);
                    if (base.ContainersParts.ContainsKey(containerID))
                    {
                        webPart = base.WebParts[base.ContainersParts[containerID]];
                    }
                    if (null != webPart)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "tabzone_tabhead");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div);

                        writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                        writer.AddAttribute("onclick", "javascript:Toggle('" + containerID + "')");
                        writer.RenderBeginTag(HtmlTextWriterTag.A);
                        writer.Write(webPart.Title);
                        writer.RenderEndTag();

                        writer.RenderEndTag();
                    }
                }
            }
            writer.RenderEndTag();
            base.RenderBody(writer);
        }
        protected override void RenderContainerClass(HtmlTextWriter writer, WebPart webPart)
        {
            if (webPart == null || webPart.ID == this.ActiveWebPart)
            {
                base.RenderContainerClass(writer, webPart);
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "tabzone_inactivetab");
            }
        }
    }
}
