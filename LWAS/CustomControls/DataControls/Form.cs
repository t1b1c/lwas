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
using System.Collections;
using LWAS.Extensible.Interfaces.WebParts;

namespace LWAS.CustomControls.DataControls
{
    public class Form : Container
    {
        public override void InitEx()
        {
            this.Template.Mode = TemplatingMode.Form;

            base.InitEx();

            base.DisableFilters = true;
            base.DisablePaginater = true;
        }
        protected override void OnBuild(bool bStructureOnly)
        {
            if (!bStructureOnly)
            {
                if (0 == base.Items.Count)
                {
                    base.OnInsert();
                }
            }
            base.OnBuild(bStructureOnly);
        }
        protected override void OnInsert()
        {
            if (base.Operation != OperationType.Inserting)
            {
                base.Items.Clear();
                base.OnInsert();
            }
        }
        protected override void OnView()
        {
            base.Message.Text = "";
            base.Operation = OperationType.Viewing;
            this.NeedsRefresh = true;
        }
        protected override void OnReceiveData(IEnumerable data)
        {
            base.Items.Clear();
            base.Operation = OperationType.Viewing;
            if (null != data)
            {
                IEnumerator enumerator = data.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    base.Items.Add(true, false, true, true, enumerator.Current);
                    OnMilestone("item");
                }
            }
        }
    }
}
