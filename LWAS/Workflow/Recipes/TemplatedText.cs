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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace LWAS.Workflow.Recipes
{
    public class TemplatedText
    {
        public RecipeComponentsCollection Components;
        public Recipe Recipe { get; set; }

        public string Template
        {
            get
            {
                IEnumerable<string> components = this.Components.Select<RecipeComponent, string>(c =>
                                                                        {
                                                                            if (c is VariableRecipeComponent)
                                                                                return ((VariableRecipeComponent)c).Name;
                                                                            else
                                                                                return c.GetValue();
                                                                        });
                return String.Join("", components.ToArray());
            }
        }

        public TemplatedText(Recipe recipe)
        {
            this.Recipe = recipe;
            this.Components = new RecipeComponentsCollection(recipe, false);
        }

        public virtual void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("text");

            this.Components.ToXml(writer);

            writer.WriteEndElement();   // text

        }

        public virtual void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Components.FromXml(element.Element("components"));
        }
    }
}
