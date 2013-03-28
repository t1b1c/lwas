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
    public class CompositeRecipe : Recipe
    {
        bool expand_to_xml = false;

        public RecipesCollection Recipes { get; set; }
        public override TemplatedFlowCollection Flows
        {
            get
            {
                TemplatedFlowCollection result = new TemplatedFlowCollection();
                foreach (Recipe recipe in this.Recipes)
                    result.Union(recipe.Flows);
                return result;
            }
            set { ;}
        }
        public override TemplatedFlowCollection AppliedFlows
        {
            get
            {
                TemplatedFlowCollection result = new TemplatedFlowCollection();
                foreach (Recipe recipe in this.Recipes)
                    result.Union(recipe.AppliedFlows);
                return result;
            }
            set { ;}
        }

        public RecipesManager Manager { get; set; }

        public CompositeRecipe(RecipesManager manager, string key)
            : base(key)
        {
            this.Manager = manager;
            this.Recipes = new RecipesCollection(manager, true);
            this.Recipes.ComponentChanged += new EventHandler(Recipes_ComponentChanged);
            this.Template.Components.ComponentChanged += new EventHandler(Components_ComponentChanged);
        }

        void Recipes_ComponentChanged(object sender, EventArgs e)
        {
            VariableRecipeComponent vrc = sender as VariableRecipeComponent;
            if (null != vrc)
                this.Template.Components.SyncComponents(vrc.Name, vrc.GetValue());
        }

        void Components_ComponentChanged(object sender, EventArgs e)
        {
            VariableRecipeComponent vrc = sender as VariableRecipeComponent;
            if (null != vrc)
                this.Recipes.SyncComponents(vrc);
        }

        public override TemplatedFlowCollection Make()
        {
            TemplatedFlowCollection workflow = new TemplatedFlowCollection();
            foreach (Recipe recipe in this.Recipes)
                workflow.Union(recipe.Make());

            return workflow;
        }

        public virtual void Expand()
        {
            expand_to_xml = true;
            foreach (CompositeRecipe composite in this.Recipes.OfType<CompositeRecipe>())
                composite.Expand();
        }

        public override Recipe Clone()
        {
            CompositeRecipe result = new CompositeRecipe(this.Manager, this.Key);
            Clone(result);
            return result;
        }

        public override void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("recipe");

            writer.WriteAttributeString("isComposite", "true");
            writer.WriteAttributeString("key", this.Key);
            writer.WriteAttributeString("name", this.Name);
            writer.WriteAttributeString("description", this.Description);

            this.Template.ToXml(writer);
            if (expand_to_xml)
                this.Recipes.ToXml(writer, false);
            else
                this.Recipes.ToXml(writer, !this.Manager.ExpandTree);

            writer.WriteEndElement();   // recipe

        }

        public override void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Name = element.Attribute("name").Value;
            this.Description = element.Attribute("description").Value;

            this.Template.FromXml(element.Element("text"));
            this.Recipes.FromXml(element.Element("recipes"), !this.Manager.ExpandTree);
        }
    }
}
