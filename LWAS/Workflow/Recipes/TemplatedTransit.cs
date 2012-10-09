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
    public class TemplatedTransit : TemplatedFlow
    {
        public bool Persistent { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }

        public override TemplatedFlow Make(Recipe recipe)
        {
            TemplatedTransit transit = new TemplatedTransit();
            transit.Source = this.Source;
            transit.Destination = this.Destination;
            foreach (VariableRecipeComponent component in recipe.VariableComponents)
            {
                string value = component.GetValue();
                if (component is RecipePartMember && ((RecipePartMember)component).IsValueFromReference)
                    value = ((RecipePartMember)component).ValueMember;
                string componentholder = TemplatedFlow.PLACEHOLDER.Replace(TemplatedFlow.WILDCARD, component.Name);
                transit.Source = transit.Source.Replace(componentholder, value);
                transit.Destination = transit.Destination.Replace(componentholder, value);
            }
            return transit;
        }

        public override void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("transit");
            writer.WriteAttributeString("key", this.Key);
            writer.WriteAttributeString("persistent", this.Persistent.ToString());
            writer.WriteAttributeString("source", this.Source);
            writer.WriteAttributeString("destination", this.Destination);
            writer.WriteEndElement();   // transit
        }

        public override void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Key = element.Attribute("key").Value;
            this.Persistent = bool.Parse(element.Attribute("persistent").Value);
            this.Source = element.Attribute("source").Value;
            this.Destination = element.Attribute("destination").Value;
        }

    }
}
