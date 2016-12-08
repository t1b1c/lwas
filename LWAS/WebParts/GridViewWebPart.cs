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
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LWAS.CustomControls.DataControls;

namespace LWAS.WebParts
{
	public class GridViewWebPart : ContainerWebPart
	{
        public int SkipRecords
        {
            get
            {
                if (base.Container.Paginater.CurrentPage <= 1)
                    return 0;
                else
                    return (base.Container.Paginater.CurrentPage - 1) * base.Container.Paginater.PageSize;
            }
            set { base.Container.Paginater.CurrentPage = value; }
        }

        public int PageSize
        {
            get { return base.Container.Paginater.PageSize; }
            set { base.Container.Paginater.PageSize = value; }
        }

        public DataSet ResultsCount
        {
            set
            {
                if (value.Tables.Count > 0 && value.Tables[0].Rows.Count > 0 && value.Tables[0].Columns.Count > 0)
                    this.Container.Paginater.ResultsCount = (int)value.Tables[0].Rows[0][0];
                else
                    this.Container.Paginater.ResultsCount = 0;
            }
        }

        public string FilterExpression
        {
            get
            {
                return String.Join(" and ", 
                    this.FilterItems.SelectMany(item => item.Data as Dictionary<string, object>)
                                    .Where(kvp => kvp.Value != null && !String.IsNullOrEmpty(kvp.Value.ToString()))
                                    .Select(kvp => {
                                        string value = (kvp.Value ?? "").ToString();
                                        DateTime testDate = default(DateTime);
                                        if (DateTime.TryParse(value, out testDate))
                                            return String.Format("[{0}] = cast('{1}' as smalldatetime)", kvp.Key, testDate.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                        else
                                            return String.Format("[{0}] like '{1}%'", kvp.Key, value.Replace("'", "")); 
                                    })
                                    .ToArray());
                                       
            }
        }

        public IEnumerable ReceiveData2
        {
            set { ((Grid)this.Container).OnReceiveFilteredPaginatedData(value); }
        }

        protected override Container InstantiateContainer()
        {
            return new Grid();
        }
	}
}
