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
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LWAS.CustomControls
{
	public class MaskedCalendar : CompositeControl
	{
		private StyledTextBox txtDate;
		private StyledTextBox txtTime;
		private Style _normalStyle = new Style();
		private Style _readOnlyStyle = new Style();
		private bool _readOnly = false;
        private bool _simpleImput = false;

		[Themeable(true)]
		public Style NormalStyle
		{
			get { return this._normalStyle; }
			set { this._normalStyle = value; }
		}

		[Themeable(true)]
		public Style ReadOnlyStyle
		{
			get { return this._readOnlyStyle; }
			set { this._readOnlyStyle = value; }
		}

		public string Text
		{
			get
			{
				return (this.txtDate.Text + " " + this.txtTime.Text).Trim();
			}
			set
			{
				DateTime dt = default(DateTime);
				if (DateTime.TryParse(value, out dt))
				{
					this.txtDate.Text = dt.ToShortDateString();
					this.txtTime.Text = dt.ToString("HH:mm");
				}
			}
		}

		public bool EnableTime
		{
			get
			{
				return this.txtTime != null && this.txtTime.Visible;
			}
			set
			{
				this.EnsureChildControls();
				if (null != this.txtTime)
				{
					this.txtTime.Visible = value;
				}
			}
		}

		public string Date
		{
			get
			{
				string result = null;
				if (string.IsNullOrEmpty(this.txtDate.Text))
				{
					result = null;
				}
				else
				{
					string dtstring = (this.txtDate.Text + " " + this.txtTime.Text).Trim();
                    try
                    {
                        result = DateTime.Parse(dtstring, Thread.CurrentThread.CurrentCulture.DateTimeFormat).ToString();
                    }
                    catch { }
				}
				return result;
			}
			set
			{
				if (null != value)
				{
					this.Text = value.ToString();
				}
			}
		}

		public bool ReadOnly
		{
			get
			{
				return this._readOnly;
			}
			set
			{
				this._readOnly = value;
				if (null != this.txtDate)
				{
					this.txtDate.ReadOnly = value;
				}
				if (null != this.txtTime)
				{
					this.txtTime.ReadOnly = value;
				}
			}
		}

        public bool SimpleInput
        {
            get { return _simpleImput; }
            set
            {
                _simpleImput = value;
            }
        }

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			this.EnsureChildControls();
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

            Panel container = new Panel();
            container.CssClass = "maskedcalendar-container";
            this.Controls.Add(container);

			this.txtDate = new StyledTextBox();
			this.txtDate.ID = "txtDate";
			this.txtDate.ReadOnly = this._readOnly;
			if (this._readOnly)
			{
				this.txtDate.ApplyStyle(this._readOnlyStyle);
			}
			else
			{
				this.txtDate.ApplyStyle(this._normalStyle);
			}
            container.Controls.Add(this.txtDate);
            this.txtDate.CssClass += " masked_calendar_date";

			this.txtTime = new StyledTextBox();
			this.txtTime.ID = "txtTime";
			this.txtTime.Visible = false;
			this.txtTime.ReadOnly = this._readOnly;
			if (this._readOnly)
			{
				this.txtTime.ApplyStyle(this._readOnlyStyle);
			}
			else
			{
				this.txtTime.ApplyStyle(this._normalStyle);
			}
            container.Controls.Add(this.txtTime);
            this.txtTime.CssClass += " masked_calendar_time";
		}
	}
}
