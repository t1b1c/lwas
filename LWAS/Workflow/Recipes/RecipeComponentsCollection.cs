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
    public class RecipeComponentsCollection : IEnumerable<RecipeComponent>, IEnumerable<VariableRecipeComponent>
    {
        List<RecipeComponent> list;
        bool unique_members = true;
        public event EventHandler ComponentChanged;
        public Recipe Recipe { get; set; }

        public RecipeComponentsCollection(Recipe recipe)
        {
            list = new List<RecipeComponent>();
            this.Recipe = recipe;
        }

        public RecipeComponentsCollection(Recipe recipe, bool enforceUniqueMembers)
            : this(recipe)
        {
            unique_members = enforceUniqueMembers;
        }

        public void Add(RecipeComponent component)
        {
            if (list.Contains(component)) return;
            if (unique_members)
            {
                VariableRecipeComponent variableComponent = component as VariableRecipeComponent;
                if (null != variableComponent && this.Contains(variableComponent.Name)) throw new ArgumentException(String.Format("There's a '{0}' component already in this collection", variableComponent.Name));
            }
            
            list.Add(component);
            if (component is VariableRecipeComponent)
                RegisterChangedEvent(component as VariableRecipeComponent);
        }

        void RegisterChangedEvent(VariableRecipeComponent vrc)
        {
            if (null == vrc) throw new ArgumentNullException("vrc");

            vrc.Changed += new EventHandler(vrc_Changed);
        }

        void UnregisterChangedEvent(VariableRecipeComponent vrc)
        {
            if (null == vrc) throw new ArgumentNullException("vrc");

            vrc.Changed -= new EventHandler(vrc_Changed);
        }

        void vrc_Changed(object sender, EventArgs e)
        {
            VariableRecipeComponent source = sender as VariableRecipeComponent;
            if (null != source)
                SyncComponents(source.Name, source.GetValue());

            if (null != this.ComponentChanged)
                ComponentChanged(sender, e);
        }

        public void SyncComponents(string name, string value)
        {
            IEnumerable<VariableRecipeComponent> duplicates;
            duplicates = this.Where<VariableRecipeComponent>(vrc => { return vrc.Name == name; });
            foreach (VariableRecipeComponent vrc in duplicates)
                vrc.ChangeValue(value, false);
        }

        public void AddAt(RecipeComponent component, int index)
        {
            if (list.Contains(component)) return;
            if (unique_members)
            {
                VariableRecipeComponent variableComponent = component as VariableRecipeComponent;
                if (null != variableComponent && this.Contains(variableComponent.Name)) throw new ArgumentException(String.Format("There's a '{0}' component already in this collection", variableComponent.Name));
            }

            this[index] = component;
            if (component is VariableRecipeComponent)
                RegisterChangedEvent(component as VariableRecipeComponent);
        }

        public void Remove(RecipeComponent component)
        {
            list.Remove(component);
            if (component is VariableRecipeComponent)
                UnregisterChangedEvent(component as VariableRecipeComponent);

        }

        public VariableRecipeComponent this[string name]
        {
            get
            {
                return this.SingleOrDefault<VariableRecipeComponent>(c => null != c && c.Name == name);
            }
        }

        public RecipeComponent this[int index]
        {
            get { return list[index]; }
            set
            {
                list.Insert(index, value);
                if (value is VariableRecipeComponent)
                    RegisterChangedEvent(value as VariableRecipeComponent);
            }
        }

        public void Clear()
        {
            foreach (VariableRecipeComponent component in (IEnumerable<VariableRecipeComponent>)this)
                UnregisterChangedEvent(component);

            list.Clear();
        }

        public bool Contains(string name)
        {
            return null != this[name];
        }

        public IEnumerator<RecipeComponent> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator<VariableRecipeComponent> IEnumerable<VariableRecipeComponent>.GetEnumerator()
        {
            foreach (RecipeComponent component in list)
                if (component is VariableRecipeComponent)
                    yield return component as VariableRecipeComponent;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("components");

            foreach (RecipeComponent component in this)
                component.ToXml(writer);

            writer.WriteEndElement();   // components

        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            foreach (XElement componentElement in element.Elements("component"))
            {
                RecipeComponent component = RecipeComponent.FromType(componentElement.Attribute("type").Value);
                component.FromXml(componentElement);
                Add(component);
            }
        }
    }
}
