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
using System.Text.RegularExpressions;

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

        protected override HtmlTextWriterTag TagKey
        {
            get { return HtmlTextWriterTag.Div; }
        }

        public override Unit Width
        {
            get { return base.Width; }
            set {; } // make width readonly for backward compatibility with erp
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

		public object Date
		{
			get
			{
				object result = null;
				if (string.IsNullOrEmpty(this.txtDate.Text))
				{
					result = null;
				}
				else
				{
					string dtstring = (this.txtDate.Text + " " + this.txtTime.Text).Trim();
                    try
                    {
                        result = DateTime.Parse(dtstring, Thread.CurrentThread.CurrentCulture.DateTimeFormat);
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

        static string DateMask;
        static string TimeMask;

        static MaskedCalendar()
        {
            DateMask = Regex.Replace(Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern, "[A-Za-z ]", "9");
            TimeMask = Regex.Replace(Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortTimePattern, "[A-Za-z ]", "9");
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

			txtDate = new StyledTextBox();
			txtDate.ID = "txtDate";
            txtDate.ReadOnly = _readOnly;
            var div_input_group = new LiteralControl(String.Format(@"<div class=""input-group masked_calendar_date {0}"">", _readOnly ? "" : "date"));
            container.Controls.Add(div_input_group);
            container.Controls.Add(this.txtDate);
            txtDate.Attributes.Add("readonly", "readonly");
            txtDate.CssClass = "form-control disabled";
            if (!_readOnly)
                container.Controls.Add(new LiteralControl(@"<span class=""input-group-addon""><i class=""glyphicon glyphicon-calendar""></i></span>"));
            else
                container.Controls.Add(new LiteralControl(" "));
            container.Controls.Add(new LiteralControl("</div>"));

			txtTime = new StyledTextBox();
			txtTime.ID = "txtTime";
			txtTime.Visible = false;
			txtTime.ReadOnly = _readOnly;
            container.Controls.Add(this.txtTime);
            txtTime.Attributes.Add("data-mask", TimeMask);
            txtTime.Attributes.Add("placeholder", Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortTimePattern);
            txtTime.CssClass += " masked_calendar_time";
            txtTime.TextMode = TextBoxMode.Time;
		}
	}
}
