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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LWAS.CustomControls
{
    public class PopupText : CompositeControl
    {
        HiddenField isShownHidden;
        HiddenField okMessageHidden;
        HiddenField cancelMessageHidden;
        protected Panel container;
        protected Panel headerPanel;
        protected Label titleLabel;
        protected TextBox textBox;
        protected Button okButton;
        protected Button cancelButton;
        protected Panel contentPanel;
        public event CommandEventHandler Milestone;

        string _okText;
        [Themeable(true)]
        public string OkText
        {
            get { return _okText; }
            set { _okText = value; }
        }

        string _cancelText;
        [Themeable(true)]
        public string CancelText
        {
            get { return _cancelText; }
            set { _cancelText = value; }
        }

        protected virtual bool IsShown
        {
            get
            {
                bool ret = false;
                if (!String.IsNullOrEmpty(isShownHidden.Value))
                    bool.TryParse(isShownHidden.Value, out ret);

                return ret;
            }
            set { isShownHidden.Value = value.ToString(); }
        }

        public string Title
        {
            get { return this.titleLabel.Text; }
            set { this.titleLabel.Text = value; }
        }

        public string Text
        {
            get { return this.textBox.Text; }
            set { this.textBox.Text = value; }
        }

        public string OkMessage
        {
            get { return okMessageHidden.Value; }
            set { okMessageHidden.Value = value; }
        }
        public string CancelMessage
        {
            get { return cancelMessageHidden.Value; }
            set { cancelMessageHidden.Value = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            EnsureChildControls();

            this.Page.Load += new EventHandler(Page_Load);
            this.Page.LoadComplete += new EventHandler(Page_LoadComplete);
            this.okButton.Click += new EventHandler(okButton_Click);
            this.cancelButton.Click += new EventHandler(cancelButton_Click);

            OnSelfInit();
        }

        protected virtual void OnSelfInit()
        {
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            isShownHidden = new HiddenField();
            isShownHidden.ID = "isShownHidden";
            this.Controls.Add(isShownHidden);

            this.okMessageHidden = new HiddenField();
            this.okMessageHidden.ID = "okMessageHidden";
            this.Controls.Add(okMessageHidden);

            this.cancelMessageHidden = new HiddenField();
            this.cancelMessageHidden.ID = "cancelMessageHidden";
            this.Controls.Add(cancelMessageHidden);

            container = new Panel();
            container.ID = "Modal";
            container.CssClass = "modal fade";
            container.Attributes["tabindex"] = "-1";
            container.Attributes["role"] = "dialog";
            container.Attributes["aria-hidden"] = "true";
            container.Attributes["show"] = "false";
            this.Controls.Add(container);

            container.Controls.Add(new LiteralControl(@"
  <div class=""modal-dialog"">
    <div class=""modal-content"">
      <div class=""modal-header"">
"));

            headerPanel = new Panel();
            headerPanel.ID = "headerPanel";
            container.Controls.Add(headerPanel);

            this.okButton = new Button();
            this.okButton.ID = "okButton";
            this.okButton.Text = _okText;
            this.okButton.CssClass = "btn btn-primary";
            headerPanel.Controls.Add(okButton);

            this.cancelButton = new Button();
            this.cancelButton.ID = "cancelButton";
            this.cancelButton.Text = _cancelText;
            this.cancelButton.CssClass = "btn btn-default";
            headerPanel.Controls.Add(this.cancelButton);

            this.titleLabel = new Label();
            this.titleLabel.ID = "titleLabel";
            this.titleLabel.CssClass = "modal-title";
            this.headerPanel.Controls.Add(titleLabel);

            container.Controls.Add(new LiteralControl(@"
      </div>
      <div class=""modal-body"">
"));

            contentPanel = new Panel();
            contentPanel.ID = "contentPanel";
            container.Controls.Add(contentPanel);

            this.textBox = new TextBox();
            this.textBox.ID = "textBox";
            this.textBox.TextMode = TextBoxMode.MultiLine;
            this.textBox.CssClass = "form-control";
            this.contentPanel.Controls.Add(textBox);

            container.Controls.Add(new LiteralControl(@"
      </div>
    </div>
  </div>
"));
        }

        private void Page_Load(object sender, EventArgs e)
        {
            OnPageLoad();
        }

        protected virtual void OnPageLoad()
        {
            container.Attributes["show"] = "false";

            if (this.IsShown)
                Display();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            OnOk();
        }

        protected virtual void OnOk()
        {
            Hide();
            CommandEventArgs cea = new CommandEventArgs("ok", null);
            if (null != this.Milestone)
                Milestone(this, cea);
            this.textBox.Text = "";
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            OnCancel();
        }

        protected virtual void OnCancel()
        {
            Hide();
            CommandEventArgs cea = new CommandEventArgs("cancel", null);
            if (null != this.Milestone)
                Milestone(this, cea);
        }

        public virtual void Display()
        {
            container.Attributes["show"] = "true";
        }

        public virtual void Show()
        {
            this.IsShown = true;
        }

        public virtual void Hide()
        {
            container.Attributes["show"] = "false";
            this.IsShown = false;
        }

        private void Page_LoadComplete(object sender, EventArgs e)
        {
            OnPageLoadComplete();
        }

        protected virtual void OnPageLoadComplete()
        {
            if (this.IsShown)
                Display();
        }
    }
}
