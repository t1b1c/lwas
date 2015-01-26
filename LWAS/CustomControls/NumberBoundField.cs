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
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LWAS.CustomControls
{
    public class NumberBoundField : BoundField
	{
        protected override void OnDataBindField(object sender, EventArgs e)
        {
            base.OnDataBindField(sender, e);
        }

		protected override string FormatDataValue(object numberValue, bool encode)
		{
			string format = this.DataFormatString;

            string result;
            if (numberValue != null && !string.IsNullOrEmpty(numberValue.ToString()))
            {
                Decimal d;
                if (numberValue is Decimal)
                    d = (Decimal)numberValue;
                else
                    Decimal.TryParse(numberValue.ToString(), out d);

                result = d.ToString(format);                
            }
            else
                result = "";

            return result;
		}

		public override void ExtractValuesFromCell(IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
		{
            string format = this.DataFormatString;

            object val = null;
            try
            {
                Decimal d = Decimal.Parse(cell.Text, CultureInfo.CurrentCulture);
                val = d;
            }
            catch (Exception ex)
            { }
            
            string dataField = this.DataField;
            if (dictionary.Contains(dataField))
                dictionary[dataField] = val;
            else
            {
                dictionary.Add(dataField, val);
            }
		}
	}
}
