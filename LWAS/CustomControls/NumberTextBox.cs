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

namespace LWAS.CustomControls
{
	[Themeable(true)]
	public class NumberTextBox : StyledTextBox
	{
		private string _format = string.Empty;
		private NumberType _numberType = NumberType.Money; // backward compatible with erp
		private object _autoIncrementScope = null;
		
        private static Dictionary<object, int> CurrentIncrements = new Dictionary<object, int>();

		[Themeable(true)]
		public string Format
		{
			get
			{
                if (!String.IsNullOrEmpty(this.Decimals))
                {
                    if (this.NumberType == CustomControls.NumberType.Money)
                        return String.Format("N{0}", this.Decimals);
                    else if (this.NumberType == CustomControls.NumberType.Percentage)
                        return String.Format("P{0}", this.Decimals);
                    return null;
                }
                else
                    return _format;
			}
			set { _format = value; }
		}

        public string Decimals { get; set; }
        
        [Themeable(true)]
        public string PercentFormat { get; set; }

		public NumberType NumberType
		{
			get { return this._numberType; }
			set { this._numberType = value; }
		}

		public object AutoIncrementScope
		{
			get { return this._autoIncrementScope; }
			set { this._autoIncrementScope = value; }
		}
		public override bool ReadOnly
		{
			get
			{
				return this._numberType == NumberType.AutoIncrement || base.ReadOnly;
			}
			set
			{
				if (this._numberType != NumberType.AutoIncrement)
				{
					base.ReadOnly = value;
				}
			}
		}
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				if (this._numberType != NumberType.AutoIncrement)
				{
					if (!string.IsNullOrEmpty(value))
					{
						decimal dec;
						if (decimal.TryParse(value, out dec))
						{
							if (this._numberType == NumberType.WholeNumber)
							{
								base.Text = ((int)Math.Floor(dec)).ToString();
							}
                            else if (this._numberType == NumberType.Percentage)
                            {
                                base.Text = dec.ToString(this.PercentFormat);
                            }
                            else
                            {
                                base.Text = dec.ToString(this.Format);
                            }
						}
						else
						{
							base.Text = value;
						}
					}
					else
					{
						base.Text = value;
					}
				}
			}
		}
		public string UnformatedText
		{
			get
			{
				string result;
				if (!string.IsNullOrEmpty(this.Text))
				{
					decimal dec;
                    if (this._numberType == NumberType.Percentage)
                    {
                        decimal.TryParse(this.Text.Replace("%", ""), out dec);
                        result = (dec / 100).ToString();
                    }
                    else if (decimal.TryParse(this.Text, out dec))
                    {
                        if (this._numberType == NumberType.WholeNumber || this._numberType == NumberType.AutoIncrement)
                        {
                            result = ((int)Math.Floor(dec)).ToString();
                        }
                        else
                        {
                            result = dec.ToString();
                        }
                    }
                    else
                    {
                        result = this.Text;
                    }
				}
				else
				{
					result = null;
				}
				return result;
			}
			set
			{
			}
		}
		private static int Increment(object scope)
		{
			if (!NumberTextBox.CurrentIncrements.ContainsKey(scope))
			{
				NumberTextBox.CurrentIncrements.Add(scope, 1);
			}
			Dictionary<object, int> currentIncrements;
			int result;
			(currentIncrements = NumberTextBox.CurrentIncrements)[scope] = (result = currentIncrements[scope]) + 1;
			return result;
		}
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			this.Page.Unload += new EventHandler(this.Page_Unload);
		}
		protected override void Render(HtmlTextWriter writer)
		{
			if (this._numberType == NumberType.AutoIncrement)
			{
				base.Text = NumberTextBox.Increment(this._autoIncrementScope).ToString();
			}

            switch(_numberType)
            {
                case NumberType.AutoIncrement:
                case NumberType.WholeNumber:
                    this.CssClass += " number wholeNumber";
                    break;
                case NumberType.Percentage  :
                    this.CssClass += " number percentage";
                    break;
                case NumberType.Money:
                    this.CssClass += " number money";
                    break;
            }

            base.Render(writer);
		}
		private void Page_Unload(object sender, EventArgs e)
		{
			NumberTextBox.CurrentIncrements.Clear();
		}
	}
}
