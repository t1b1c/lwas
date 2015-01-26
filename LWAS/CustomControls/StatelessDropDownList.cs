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
using System.Web.UI.WebControls;

namespace LWAS.CustomControls
{
	public class StatelessDropDownList : DropDownList
	{
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

        public string CommandArgument { get; set; }

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
				if (null != this.Items.FindByValue(this._selectedValue))
				{
					this.ChangeSelection(this._selectedValue);
				}
			}
		}
		public override bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				base.Enabled = value;
				if (base.Enabled)
				{
					this.CssClass = "dropDown_enabled";
				}
				else
				{
					this.CssClass = "dropDown_disabled";
				}
			}
		}
		public StatelessDropDownList()
		{
			this.EnableViewState = false;
		}
		protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			bool ret = base.LoadPostData(postDataKey, postCollection);
            this.SelectedValue = postCollection[postDataKey];

            if (this.AutoPostBack)
                return true;
			return ret;
		}
        protected override void RaisePostDataChangedEvent()
        {
            if (this.AutoPostBack && this.UniqueID == this.Page.Request.Form["__EVENTTARGET"])
                RaiseBubbleEvent(this, new CommandEventArgs(_commandName, this.CommandArgument));
        }
		public override void DataBind()
		{
			base.DataBind();
			if (null != this.Items.FindByValue(this._selectedValue))
			{
				this.ChangeSelection(this._selectedValue);
			}
		}
		protected virtual void ChangeSelection(string selval)
		{
			this.ClearSelection();
			int selidx = -2;
			if (null != this.Items.FindByValue(selval))
			{
				selidx = this.Items.IndexOf(this.Items.FindByValue(selval));
			}
			if (-2 != selidx)
			{
				base.SelectedIndex = selidx;
			}
		}
    }
}
