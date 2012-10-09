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
    public class RecipesCollection : IEnumerable<Recipe>
    {
        List<Recipe> list;
        bool isSyncComponentsActive = false;

        public RecipesManager Manager { get; set; }
        public event EventHandler ComponentChanged;

        public RecipesCollection(RecipesManager manager)
        {
            if (null == manager) throw new ArgumentNullException("manager");

            list = new List<Recipe>();
            this.Manager = manager;
        }

        public RecipesCollection(RecipesManager manager, bool shouldSyncComponents)
            : this(manager)
        {
            isSyncComponentsActive = shouldSyncComponents;
        }

        public void Add(Recipe recipe)
        {
            if (list.Contains(recipe)) return;
            if (this.Contains(recipe.Name)) throw new ArgumentException(String.Format("There's a '{0}' recipe already in this collection", recipe.Name));

            list.Add(recipe);
            RegisterRecipe(recipe);
        }

        public void AddAt(Recipe recipe, int index)
        {
            if (list.Contains(recipe)) return;
            if (this.Contains(recipe.Name)) throw new ArgumentException(String.Format("There's a '{0}' recipe already in this collection", recipe.Name));

            list.Insert(index, recipe);
            RegisterRecipe(recipe);
        }

        public void Remove(Recipe recipe)
        {
            list.Remove(recipe);
            UnregisterRecipe(recipe);
        }

        public void Clear()
        {
            foreach (Recipe recipe in this)
                UnregisterRecipe(recipe);
            list.Clear();
        }

        public bool Contains(string name)
        {
            Recipe recipe = list.SingleOrDefault(r => r.Name == name);
            return null != recipe;
        }

        public Recipe this[string name]
        {
            get { return list.SingleOrDefault(r => r.Name == name); }
        }

        public Recipe this[int index]
        {
            get { return list[index]; }
            set { AddAt(value, index); }
        }

        public void RegisterRecipe(Recipe recipe)
        {
            if (isSyncComponentsActive)
                recipe.Template.Components.ComponentChanged += new EventHandler(Components_ComponentChanged);
        }

        void Components_ComponentChanged(object sender, EventArgs e)
        {
            VariableRecipeComponent vrc = sender as VariableRecipeComponent;
            if (null != vrc)
                SyncComponents(vrc);
            if (null != ComponentChanged)
                ComponentChanged(sender, new EventArgs());
        }

        public void SyncComponents(VariableRecipeComponent vrc)
        {
            string name = vrc.Name;
            string value = vrc.GetValue();
            if (null != vrc)
                foreach (Recipe recipe in this)
                    recipe.Template.Components.SyncComponents(name, value);
        }

        public void UnregisterRecipe(Recipe recipe)
        {
            recipe.Template.Components.ComponentChanged -= new EventHandler(Components_ComponentChanged);
        }

        public IEnumerator<Recipe> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void ToXml(XmlTextWriter writer)
        {
            ToXml(writer, false);
        }

        public void ToXml(XmlTextWriter writer, bool refereneOnly)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("recipes");
            foreach (Recipe recipe in this)
                if (refereneOnly)
                {
                    writer.WriteStartElement("recipe");
                    writer.WriteAttributeString("name", recipe.Name);
                    writer.WriteEndElement();   // recipe
                }
                else
                    recipe.ToXml(writer);
            writer.WriteEndElement();   // recipes
        }

        public void FromXml(XElement element)
        {
            FromXml(element, false);
        }

        public void FromXml(XElement element, bool referenceOnly)
        {
            if (null == element) throw new ArgumentNullException("element");

            foreach (XElement recipeElement in element.Elements("recipe"))
            {
                string name = recipeElement.Attribute("name").Value;
                Recipe recipe = null;
                if (referenceOnly)
                    recipe = this.Manager.Recipes[name];
                else
                {
                    XAttribute compositeAttribute = recipeElement.Attribute("isComposite");
                    bool isComposite = false;
                    if (null != compositeAttribute)
                        bool.TryParse(compositeAttribute.Value, out isComposite);
                    if (isComposite)
                        recipe = new CompositeRecipe(this.Manager, name);
                    else
                        recipe = new Recipe(name);
                    recipe.FromXml(recipeElement);
                }
                this.Add(recipe);
            }
        }
    }
}
