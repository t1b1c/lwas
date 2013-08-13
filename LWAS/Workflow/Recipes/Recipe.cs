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
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.IO;

using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Workflow.Recipes
{
    public class Recipe
    {
        public class KeyChangeEventArgs : EventArgs
        {
            public string OldKey { get; set; }
            public string NewKey { get; set; }
        }

        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual TemplatedFlowCollection Flows { get; set; }
        public virtual TemplatedFlowCollection AppliedFlows { get; set; }
        public TemplatedText Template { get; set; }

        IExpressionsManager _expressionsManager;
        public IExpressionsManager ExpressionsManager
        {
            get { return _expressionsManager; }
        }

        public event EventHandler<KeyChangeEventArgs> KeyChanged;
        
        public IEnumerable<RecipeComponent> Components 
        {
            get { return this.Template.Components; }
        }

        public IEnumerable<VariableRecipeComponent> VariableComponents
        {
            get { return this.Template.Components; }
        }

        public Recipe(string key, IExpressionsManager expressionsManager)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException("key");

            _expressionsManager = expressionsManager;

            this.Key = key;
            this.Template = new TemplatedText(this);
            this.Flows = new TemplatedFlowCollection(this.ExpressionsManager);
            this.AppliedFlows = new TemplatedFlowCollection(this.ExpressionsManager);
        }

        public virtual TemplatedFlowCollection Make()
        {
            return Make(MakePolicyType.Full);
        }

        public virtual TemplatedFlowCollection Make(MakePolicyType makePolicy)
        {
            TemplatedFlowCollection workflow = new TemplatedFlowCollection(this.ExpressionsManager);
            foreach (TemplatedFlow flow in this.Flows)
                workflow.Add(flow.Make(this, makePolicy));

            this.AppliedFlows = workflow;

            return workflow;
        }

        public virtual void Rename(string newname)
        {
            string oldkey = this.Key;
            string suffix = "";
            if (this.Key != this.Name)
                suffix = this.Key.Substring(this.Name.Length);
            this.Name = newname;
            this.Key = this.Name + suffix;

            if (null != this.KeyChanged)
                KeyChanged(this, new KeyChangeEventArgs() { OldKey = oldkey, NewKey = this.Key });
        }

        public virtual Recipe Clone()
        {
            Recipe result = new Recipe(this.Key, this.ExpressionsManager);
            Clone(result);
            return result;
        }

        protected void Clone(Recipe clone)
        {
            StringBuilder sb = new StringBuilder();

            using (StringWriter sw = new StringWriter(sb))
            {
                using (XmlTextWriter writer = new XmlTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    ToXml(writer);
                }
            }

            XElement element = XElement.Parse(sb.ToString(), LoadOptions.PreserveWhitespace);
            clone.FromXml(element);
        }

        public virtual bool IsReady()
        {
            return null == this.Template.Components
                                        .OfType<VariableRecipeComponent>()
                                        .FirstOrDefault(vrc => vrc.HasValue == false);
        }

        public bool HasAppliedKey(string key)
        {
            return null != this.AppliedFlows
                               .OfType<TemplatedJob>()
                               .FirstOrDefault(tj => tj.Key == key);
        }

        public virtual void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("recipe");

            writer.WriteAttributeString("key", this.Key);
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

            this.Name = element.Attribute("name").Value;
            this.Description = element.Attribute("description").Value;

            this.Template.FromXml(element.Element("text"));
            this.Flows.FromXml(element.Element("flows"));
            if (null != element.Element("applied"))
            {
                this.AppliedFlows = new TemplatedFlowCollection(this.ExpressionsManager);
                this.AppliedFlows.FromXml(element.Element("applied"));
            }
        }
    }
}
