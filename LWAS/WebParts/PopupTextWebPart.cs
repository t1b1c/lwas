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
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using LWAS.CustomControls;
using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.WebParts;

namespace LWAS.WebParts
{
    public class PopupTextWebPart : ChroniclerWebPart
    {
        public PopupText TextEditor;
        UpdatePanelDynamic updatePanel;

        [Personalizable(true)]
        public string EditorTitle
        {
            get 
            {
                if (null == this.TextEditor)
                    return null;
                return this.TextEditor.Title; 
            }
            set { this.TextEditor.Title = value; }
        }

        public string EditorText
        {
            get { return this.TextEditor.Text; }
            set { this.TextEditor.Text = value; }
        }

        public string OkMessage
        {
            get { return this.TextEditor.OkMessage; }
            set { this.TextEditor.OkMessage = value; }
        }
        public string CancelMessage
        {
            get { return this.TextEditor.CancelMessage; }
            set { this.TextEditor.CancelMessage = value; }
        }

        string _command;
        public string Command
        {
            get { return _command; }
            set
            {
                _command = value;
                if ("show" == _command)
                    TextEditor.Show();
                else if ("hide" == _command)
                    TextEditor.Hide();
            }
        }

        public PopupTextWebPart()
        {
            this.Hidden = true; 
            this.ExportMode = WebPartExportMode.None;
            this.ChromeType = PartChromeType.None;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            updatePanel = new UpdatePanelDynamic();
            updatePanel.ID = "updatePanel";
            this.Controls.Add(updatePanel);

            this.TextEditor = new PopupText();
            this.TextEditor.Milestone += new CommandEventHandler(TextEditor_Milestone);
            updatePanel.ContentTemplateContainer.Controls.Add(this.TextEditor);
        }

        void TextEditor_Milestone(object sender, CommandEventArgs e)
        {
            if ("ok" == e.CommandName && !String.IsNullOrEmpty(this.OkMessage))
                OnMilestone(this.OkMessage);
            else if ("cancel" == e.CommandName && !String.IsNullOrEmpty(this.CancelMessage))
                OnMilestone(this.CancelMessage);

        }
    }
}
