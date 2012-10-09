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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace LWAS.Workflow.Recipes
{
    public class TemplatedCondition : TemplatedFlow
    {
        public string Sender { get; set; }
        public string Milestone { get; set; }

        public override TemplatedFlow Make(Recipe recipe)
        {
            TemplatedCondition condition = new TemplatedCondition();
            condition.Sender = this.Sender;
            condition.Milestone = this.Milestone;
            foreach (VariableRecipeComponent component in recipe.VariableComponents)
            {
                string value = component.GetValue();
                if (component is RecipePartMember && ((RecipePartMember)component).IsValueFromReference)
                    value = ((RecipePartMember)component).ValueMember;
                string componentholder = TemplatedFlow.PLACEHOLDER.Replace(TemplatedFlow.WILDCARD, component.Name);
                condition.Sender = condition.Sender.Replace(componentholder, value);
                condition.Milestone = condition.Milestone.Replace(componentholder, value);
            }
            return condition;
        }

        public override void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("condition");
            writer.WriteAttributeString("key", this.Key);
            writer.WriteAttributeString("sender", this.Sender);
            writer.WriteAttributeString("milestone", this.Milestone);
            writer.WriteEndElement();   // condition
        }

        public override void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Key = element.Attribute("key").Value;
            this.Sender = element.Attribute("sender").Value;
            this.Milestone = element.Attribute("milestone").Value;
        }
    }
}
