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
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using LWAS.Infrastructure;

namespace LWAS.CustomControls
{
	public class Lookup : DataBoundControl
    {
        public string LookFor
        {
            get;
            set;
        }
        public string LookBy
        {
            get;
            set;
        }
        public object LookByValue
        {
            get;
            set;
        }

        public bool Look
        {
            set { if (value) DataBind(); }
        }

        Label displayControl;

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            displayControl = new Label();
            displayControl.ID = "displayControl";
            this.Controls.Add(displayControl);
        }

        protected override void PerformDataBinding(IEnumerable data)
        {
            DataSet dataSet = data as DataSet;
            if (null != dataSet && dataSet.Tables.Count > 0)
            {
                DataTable table = dataSet.Tables[0];
                if (String.IsNullOrEmpty(this.LookBy)) throw new InvalidOperationException("LookBy is empty");
                if (!table.Columns.Contains(this.LookBy)) throw new InvalidOperationException(String.Format("Invalid LookBy. Data has no '{0}' propery", this.LookBy));
                if (String.IsNullOrEmpty(this.LookFor)) throw new InvalidOperationException("LookFor is empty");
                if (!table.Columns.Contains(this.LookFor)) throw new InvalidOperationException(String.Format("Invalid LookFor. Data has no '{0}' property", this.LookFor));

                string val = this.LookByValue == null ? String.Empty : ReflectionServices.ToString(this.LookByValue);
                DataRow[] found = table.Select(this.LookBy + "='" + val + "'");
                if (found.Length > 0)
                    Display(found[0][this.LookFor]);
            }
        }

        protected virtual void Display(object value)
        {
            if (null != value)
                displayControl.Text = value.ToString();
        }
	}
}
