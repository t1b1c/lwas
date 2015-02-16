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
using System.Linq;

using AjaxControlToolkit;

using LWAS.Extensible.Interfaces.WorkFlow;

namespace LWAS.WebParts.Zones
{
    public class PopupZone : TableZone
    {
        HiddenField _activeWebPartClientIDHidden;
        string _activeWebPart;
        public string ActiveWebPart 
        {
            get { return _activeWebPart; }
            set
            {
                _activeWebPart = value;
                if (null != _activeWebPartClientIDHidden && !String.IsNullOrEmpty(value))
                    _activeWebPartClientIDHidden.Value = this.WebParts[value].ClientID;
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
            var updatePanel = FindUpdatePanel();
            if (null == updatePanel) throw new ApplicationException("PopupZone requires an UpdatePanel and none was found");

            _activeWebPartClientIDHidden = new HiddenField();
            updatePanel.ContentTemplateContainer.Controls.Add(_activeWebPartClientIDHidden);
        }

        // handle part.MilestoneHandler and watch for "show popup" and "hide popup" events
        protected override void LoadConfiguration()
        {
            base.LoadConfiguration();
            foreach(WebPart part in this.WebParts)
                if (part is IChronicler)
                    ((IChronicler)part).MilestoneHandler += new MilestoneEventHandler(chronicler_MilestoneHandler);
        }

        void chronicler_MilestoneHandler(IChronicler chronicler, string key)
        {
            WebPart part = chronicler as WebPart;
            if (key == "show popup")
                this.ActiveWebPart = part.ID;
            else if (key == "hide popup" && this.ActiveWebPart == part.ID)
                this.ActiveWebPart = null;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            string id = null;
            if (!String.IsNullOrEmpty(this.ActiveWebPart))
                id = this.WebParts[this.ActiveWebPart].ClientID;

            string js = @"
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequest);
function EndRequest(sender, args)
{
    if (args.get_error() == undefined) {
            var id = $('#" + _activeWebPartClientIDHidden.ClientID + @"').val();
            registerPopup(id);
     }
}

function registerPopup(id) {
    var popup = $('#' + id).parents('.popupzone_part').first();
    popup.switchClass('popup_hidden', 'popup_shown');
    popup.overlay({

    // custom top position
    top: 100,
    left: 750,

    // disable this for modal dialog-type of overlays
    closeOnClick: false,

    // load it immediately after the construction
    load: false

    });
    
    popup.overlay().load();
    popup.draggable();
}
";
            ToolkitScriptManager.RegisterStartupScript(this, typeof(Page), "overlay components", js, true);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (this.WebPartManager.DisplayMode != WebPartManager.EditDisplayMode &&
                this.WebParts.Count == 0)
                return;

            base.Render(writer);
        }

        protected override void RenderContainerClass(HtmlTextWriter writer, WebPart webPart)
        {
            if (webPart != null && webPart.ID == this.ActiveWebPart)
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "popupzone_part popup_shown");
            else
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "popupzone_part popup_hidden");
        }
    }
}
