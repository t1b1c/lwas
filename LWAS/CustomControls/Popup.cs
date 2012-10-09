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
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using AjaxControlToolkit;

namespace LWAS.CustomControls
{
    public class Popup : CompositeControl
    {
        HiddenField isShownHidden;
        protected HiddenField dummy;
        protected Table topTable;
        protected Button okButton;
        protected Button cancelButton;
        protected Panel contentPanel;
        protected ModalPopupExtender popupExtender;

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

            this.dummy = new HiddenField();
            this.dummy.ID = "dummy";
            this.Controls.Add(dummy);

            this.contentPanel = new Panel();
            this.contentPanel.ID = "contentPanel";
            this.contentPanel.CssClass = "popup_modalPopup";
            this.contentPanel.Style.Add("display", "none");
            this.Controls.Add(contentPanel);

            this.topTable = new Table();
            this.topTable.ID = "topTable";
            this.topTable.CssClass = "popup_topTable";
            this.contentPanel.Controls.Add(topTable);

            TableRow topRow = new TableRow();
            this.topTable.Rows.Add(topRow);

            TableCell okCell = new TableCell();
            topRow.Cells.Add(okCell);
            this.okButton = new Button();
            this.okButton.ID = "okButton";
            this.okButton.Text = _okText;
            this.okButton.CssClass = "popup_OKButton";
            okCell.Controls.Add(okButton);

            TableCell cancelCell = new TableCell();
            topRow.Cells.Add(cancelCell);
            this.cancelButton = new Button();
            this.cancelButton.ID = "cancelButton";
            this.cancelButton.Text = _cancelText;
            this.cancelButton.CssClass = "popup_CancelButton";
            cancelCell.Controls.Add(this.cancelButton);

            this.contentPanel.Controls.Add(new LiteralControl("<p><hr /></p>"));

            this.popupExtender = new ModalPopupExtender();
            this.popupExtender.ID = "popupExtender";
            this.popupExtender.TargetControlID = this.dummy.ID;
            this.popupExtender.PopupControlID = this.contentPanel.ID;
            this.popupExtender.BackgroundCssClass = "popup_modalBackground";
            this.Controls.Add(this.popupExtender);
        }

        private void Page_Load(object sender, EventArgs e)
        {
            OnPageLoad();
        }

        protected virtual void OnPageLoad()
        {
            this.popupExtender.Hide();

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
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            OnCancel();
        }

        protected virtual void OnCancel()
        {
            Hide();
        }

        public virtual void Display()
        {
            this.popupExtender.Show();
        }

        public virtual void Show()
        {
            this.IsShown = true;
        }

        public virtual void Hide()
        {
            this.popupExtender.Hide();
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
