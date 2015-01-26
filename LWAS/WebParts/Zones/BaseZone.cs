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
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces.WebParts;

using LWAS.Infrastructure;

namespace LWAS.WebParts.Zones
{
	public abstract class BaseZone : CustomChromeZone
	{
		private new Orientation LayoutOrientation = Orientation.Horizontal;
		private string _personalizationProviderID;
		private bool _showHeader = true;
        public string UpdatePanelID
        {
            get;
            set;
        }
		public string PersonalizationProviderID
		{
			get
			{
				return this._personalizationProviderID;
			}
			set
			{
				this._personalizationProviderID = value;
			}
		}
		public bool ShowHeader
		{
			get
			{
				return this._showHeader;
			}
			set
			{
				this._showHeader = value;
			}
		}
		public abstract List<WebPart> PartsNotShown
		{
			get;
		}
		public BaseZone()
		{
			base.LayoutOrientation = this.LayoutOrientation;
		}
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			base.WebPartManager.WebPartAdding += new WebPartAddingEventHandler(this.WebPartManager_WebPartAdding);
			base.WebPartManager.WebPartDeleting += new WebPartCancelEventHandler(this.WebPartManager_WebPartDeleting);

            UpdatePanel updatePanel = FindUpdatePanel();
            if (null != updatePanel)
                updatePanel.Triggers.Add(new AsyncPostBackTrigger() { ControlID = this.ID });
		}
		private void WebPartManager_WebPartAdding(object sender, WebPartAddingEventArgs e)
		{
			WebPartZoneBase zone = e.Zone;
			if (e.ZoneIndex < zone.WebParts.Count)
			{
				WebPart part = zone.WebParts[e.ZoneIndex];
				if (part != null && part is ISymbolWebPart && !part.IsStatic)
				{
					base.WebPartManager.DeleteWebPart(part);
				}
			}
		}
		private void WebPartManager_WebPartDeleting(object sender, WebPartCancelEventArgs e)
		{
			WebPart part = e.WebPart;
			if (part != null && !(part is ISymbolWebPart) && part.Zone == this)
			{
				base.WebPartManager.AddWebPart(this.CreateEmptyWebPart(), part.Zone, part.ZoneIndex);
			}
		}
		protected virtual WebPart CreateEmptyWebPart()
		{
            Manager manager = (Manager)this.WebPartManager;
            ISymbolWebPart swp = manager.NewSymbolWebPart();

			swp.Title = "empty";
            swp.SymbolOf = swp.GetType().AssemblyQualifiedName;

            return swp as WebPart;
		}
        protected virtual UpdatePanel FindUpdatePanel()
        {
            if (!String.IsNullOrEmpty(this.UpdatePanelID))
                return ReflectionServices.FindControlEx(this.UpdatePanelID, this.Page) as UpdatePanel;
            else
                return null;
        }
		protected override void RenderHeader(HtmlTextWriter writer)
		{
			if (this._showHeader)
			{
				base.RenderHeader(writer);
			}
		}
		public abstract void EmptyContainer(string containerID);
		public abstract void FillContainer(string containerID, string partID);
		public abstract string PartContainer(string partID);
		public abstract string ContainerPart(string containerID);
	}
}
