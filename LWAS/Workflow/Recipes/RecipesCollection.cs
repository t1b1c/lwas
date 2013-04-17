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

using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Workflow.Recipes
{
    public class RecipesCollection : IEnumerable<Recipe>
    {
        List<Recipe> list;
        bool isSyncComponentsActive = false;

        public RecipesManager Manager { get; set; }
        public event EventHandler ComponentChanged;

        IExpressionsManager _expressionsManager;
        public IExpressionsManager ExpressionsManager
        {
            get { return _expressionsManager; }
        }

        public RecipesCollection(RecipesManager manager, IExpressionsManager expressionsManager)
        {
            if (null == manager) throw new ArgumentNullException("manager");

            _expressionsManager = expressionsManager;

            list = new List<Recipe>();
            this.Manager = manager;
        }

        public RecipesCollection(RecipesManager manager, bool shouldSyncComponents, IExpressionsManager expressionsManager)
            : this(manager, expressionsManager)
        {
            isSyncComponentsActive = shouldSyncComponents;
        }

        public void Add(Recipe recipe)
        {
            if (null == recipe) throw new ArgumentNullException("recipe");
            if (list.Contains(recipe)) return;
            if (this.ContainsKey(recipe.Key)) throw new ArgumentException(String.Format("There's a '{0}' recipe already in this collection", recipe.Key));

            list.Add(recipe);
            RegisterRecipe(recipe);
        }

        public void AddAt(Recipe recipe, int index)
        {
            if (list.Contains(recipe)) return;
            if (this.ContainsKey(recipe.Key)) throw new ArgumentException(String.Format("There's a '{0}' recipe already in this collection", recipe.Key));

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

        public bool ContainsKey(string key)
        {
            Recipe recipe = list.SingleOrDefault(r => r.Key == key);
            return null != recipe;
        }

        public Recipe this[string key]
        {
            get { return list.SingleOrDefault(r => r.Key == key); }
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
            // don't sync siblings they could be multiplications (copies)
            /*
            VariableRecipeComponent vrc = sender as VariableRecipeComponent;
            if (null != vrc)
                SyncComponents(vrc);
             */
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

        public Recipe MultiplyRecipe(Recipe recipe)
        {
            if (null == recipe) throw new ArgumentNullException("recipe");
            if (!list.Contains(recipe)) throw new ArgumentException(String.Format("This collection does not contain the recipe '{0}'", recipe.Key));

            Recipe clone = recipe.Clone();
            string suffix = clone.Key.Substring(clone.Name.Length).Trim();
            int counter = 0;
            Int32.TryParse(suffix, out counter);
            clone.Key = clone.Name + " " + (++counter).ToString();
            this.Add(clone);
            return clone;
        }

        public IEnumerable<Recipe> OrderRecipes()
        {
            return list.OrderBy(r => r is CompositeRecipe)
                        .ThenBy(r => r is CompositeRecipe &&
                                    null != list.OfType<CompositeRecipe>()
                                                .FirstOrDefault(cr => cr.Recipes.Contains(r)
                                                )
                            )
                        .ThenBy(r => r.Name);
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

        public void ToXml(XmlTextWriter writer, bool referenceOnly)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("recipes");
            foreach (Recipe recipe in this.OrderRecipes())
            {
                if (referenceOnly)
                {
                    writer.WriteStartElement("recipe");
                    writer.WriteAttributeString("key", recipe.Key);
                    writer.WriteEndElement();   // recipe
                }
                else
                    recipe.ToXml(writer);
            }
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
                string key = null;
                if (null != recipeElement.Attribute("key"))
                    key = recipeElement.Attribute("key").Value;
                if (String.IsNullOrEmpty(key))
                    key = recipeElement.Attribute("name").Value;
                Recipe recipe = null;
                if (referenceOnly)
                    recipe = this.Manager.Recipes[key];
                else
                {
                    XAttribute compositeAttribute = recipeElement.Attribute("isComposite");
                    bool isComposite = false;
                    if (null != compositeAttribute)
                        bool.TryParse(compositeAttribute.Value, out isComposite);
                    if (isComposite)
                        recipe = new CompositeRecipe(this.Manager, key, this.ExpressionsManager);
                    else
                        recipe = new Recipe(key, this.ExpressionsManager);
                    recipe.FromXml(recipeElement);
                }
                this.Add(recipe);
            }
        }
    }
}
