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
using System.Collections;
using System.Web.UI.WebControls;

namespace LWAS.CustomControls.DataControls
{
    public class Report : Container
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            base.DisableFilters = true;
            //base.DisablePaginater = true;
            this.Paginater.PageSize = 25;
        }
        protected override bool OnBubbleEvent(object source, EventArgs args)
        {
            CommandEventArgs cea = args as CommandEventArgs;
            if (null != cea && !String.IsNullOrEmpty(cea.CommandName))
            {
                string[] cmds = cea.CommandName.Split(',');
                foreach (string initialcommand in cmds)
                {
                    string command = initialcommand.Trim();
                    if ("select" == command)
                    {
                        int index = -1;
                        string candidate = cea.CommandArgument.ToString();
                        if (candidate.Contains("grouping"))
                            candidate = candidate.Replace("grouping", "");
                        int.TryParse(candidate, out index);
                        if (-1 != index && index < base.Items.Count)
                        {
                            if (null != base.CurrentItem)
                            {
                                base.CurrentItem.IsCurrent = false;
                            }
                            base.Items[index].IsCurrent = true;
                            this.OnSelect();
                        }
                    }
                }
            }
            return base.OnBubbleEvent(source, args);
        }
    }
}
