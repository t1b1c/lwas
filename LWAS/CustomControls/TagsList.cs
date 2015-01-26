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
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LWAS.CustomControls
{
    public class TagsList : ListControl, IPostBackDataHandler
    {
        public BulletedListDisplayMode DisplayMode { get; set; }

        protected override HtmlTextWriterTag TagKey
        {
            get { return HtmlTextWriterTag.Div; }
        }

        string _commandName;
        public string CommandName
        {
            get { return _commandName; }
            set
            {
                _commandName = value;
                if (!String.IsNullOrEmpty(_commandName))
                    this.AutoPostBack = true;
            }
        }

        private string _selectedValue;
        public override string SelectedValue
        {
            get
            {
                return this._selectedValue;
            }
            set
            {
                this._selectedValue = value;
                var item = this.Items.FindByValue(this._selectedValue);
                if (null != item)
                    this.SelectedIndex = this.Items.IndexOf(item);
            }
        }

        public TagsList()
        {
            this.EnableViewState = false;
            this.DisplayMode = BulletedListDisplayMode.Text;
        }

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            this.SelectedValue = postCollection[postDataKey];

            if (this.AutoPostBack)
                return true;
            return false;
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            if (this.AutoPostBack && this.UniqueID == this.Page.Request.Form["__EVENTTARGET"])
                RaiseBubbleEvent(this, new CommandEventArgs(_commandName, null));
        }

        public override void DataBind()
        {
            base.DataBind();
            this.SelectedValue = _selectedValue;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (0 == this.Items.Count)
                return;
            base.Render(writer);
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Li);
                this.RenderText(this.Items[i], i, writer);
                writer.RenderEndTag();
            }
        }

        protected virtual void RenderText(ListItem item, int index, HtmlTextWriter writer)
        {
            switch (this.DisplayMode)
            {
                case BulletedListDisplayMode.Text:
                    if (!item.Enabled)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                        writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    }
                    this.Page.Server.HtmlEncode(item.Text, writer);
                    if (!item.Enabled)
                        writer.RenderEndTag();

                    break;
                case BulletedListDisplayMode.HyperLink:
                    if (item.Enabled)
                        writer.AddAttribute(HtmlTextWriterAttribute.Href, base.ResolveClientUrl(item.Value));
                    else
                        writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");

                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    this.Page.Server.HtmlEncode(item.Text, writer);
                    writer.RenderEndTag();

                    break;
                case BulletedListDisplayMode.LinkButton:
                    if (item.Enabled)
                    {
                        var evref = this.Page.ClientScript.GetPostBackClientHyperlink(this, index.ToString(), true);
                        writer.AddAttribute(HtmlTextWriterAttribute.Href, evref);
                    }
                    else
                        writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");

                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    this.Page.Server.HtmlEncode(item.Text, writer);
                    writer.RenderEndTag();

                    break;
                default:
                    break;
            }
        }
    }
}
