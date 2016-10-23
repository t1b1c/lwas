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
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LWAS.CustomControls
{
    public class Autocomplete : Panel, INamingContainer
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public string Database { get; set; }
        public string View { get; set; }
        public string DisplayField { get; set; }

        ListItemCollection _parameters = new ListItemCollection();
        public ListItemCollection Parameters
        {
            get { return _parameters; }
        }

        TextBox textBox;
        HiddenField hiddenField;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.Page.PreLoad += Page_PreLoad;
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            textBox = new TextBox();
            textBox.ID = "textBox";
            textBox.CssClass = "form-control autocomplete";
            textBox.TextChanged += textBox_TextChanged;
            this.Controls.Add(textBox);

            hiddenField = new HiddenField();
            hiddenField.ID = "hiddenField";
            hiddenField.ValueChanged += hiddenField_ValueChanged;
            this.Controls.Add(hiddenField);

            Image img = new Image();
            img.CssClass = "autocomplete-spinner";
            img.ImageUrl = "~/App_Themes/Default/images/spinner.gif";
            this.Controls.Add(img);
        }

        void Page_PreLoad(object sender, EventArgs e)
        {
            this.Text = textBox.Text;
            this.Value = hiddenField.Value;
        }

        void textBox_TextChanged(object sender, EventArgs e)
        {
            this.Text = textBox.Text;
        }

        void hiddenField_ValueChanged(object sender, EventArgs e)
        {
            this.Value = hiddenField.Value;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            this.CssClass = "autocomplete-wrapper";

            textBox.Text = this.Text;
            textBox.Width = this.Width;
            textBox.Height = this.Height;
            textBox.Visible = this.Visible;
            hiddenField.Value = this.Value;

            textBox.Attributes.Add("data-database", this.Database);
            textBox.Attributes.Add("data-view", this.View);
            textBox.Attributes.Add("data-display-field", this.DisplayField);
            string json = "{";
            foreach (ListItem item in this.Parameters)
                json += String.Format(@"""{0}"":""{1}""", item.Value, item.Text);
            if (json.EndsWith(","))
                json = json.Substring(0, json.Length - 1);
            json += "}";
            textBox.Attributes.Add("data-parameters", json);

            base.Render(writer);
        }
    }
}
