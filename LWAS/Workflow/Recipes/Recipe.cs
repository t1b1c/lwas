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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace LWAS.Workflow.Recipes
{
    public class Recipe
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual TemplatedFlowCollection Flows { get; set; }
        public virtual TemplatedFlowCollection AppliedFlows { get; set; }
        public TemplatedText Template { get; set; }
        
        public IEnumerable<RecipeComponent> Components 
        {
            get { return this.Template.Components; }
        }

        public IEnumerable<VariableRecipeComponent> VariableComponents
        {
            get { return this.Template.Components; }
        }

        public Recipe(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            this.Name = name;
            this.Template = new TemplatedText(this);
            this.Flows = new TemplatedFlowCollection();
            this.AppliedFlows = new TemplatedFlowCollection();
        }

        public virtual TemplatedFlowCollection Make()
        {
            TemplatedFlowCollection workflow = new TemplatedFlowCollection();
            foreach (TemplatedFlow flow in this.Flows)
                workflow.Add(flow.Make(this));

            this.AppliedFlows = workflow;

            return workflow;
        }

        public virtual void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("recipe");

            writer.WriteAttributeString("name", this.Name);
            writer.WriteAttributeString("description", this.Description);

            this.Template.ToXml(writer);
            this.Flows.ToXml(writer, "flows");
            if (null != this.AppliedFlows)
                this.AppliedFlows.ToXml(writer, "applied");
            
            writer.WriteEndElement();   // recipe

        }

        public virtual void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Description = element.Attribute("description").Value;

            this.Template.FromXml(element.Element("text"));
            this.Flows.FromXml(element.Element("flows"));
            if (null != element.Element("applied"))
            {
                this.AppliedFlows = new TemplatedFlowCollection();
                this.AppliedFlows.FromXml(element.Element("applied"));
            }
        }
    }
}
