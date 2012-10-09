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
    public abstract class RecipeComponent
    {
        protected virtual string Value { get; set; }

        public event EventHandler Changed;

        public void ChangeValue(string value)
        {
            ChangeValue(value, true);
        }

        public void ChangeValue(string value, bool raiseEvent)
        {
            this.Value = value;
            if (raiseEvent && null != this.Changed)
                Changed(this, new EventArgs());
        }

        public string GetValue()
        {
            return this.Value;
        }

        public static RecipeComponent FromType(string type)
        {
            if (String.IsNullOrEmpty(type)) throw new ArgumentNullException("type");

            RecipeComponent component = null;
            switch (type)
            {
                case "part":
                    component = new RecipePart();
                    break;
                case "member":
                    component = new RecipePartMember();
                    break;
                case "absolute":
                    component = new AbsoluteValue();
                    break;
                case "text":
                    component = new RecipeText();
                    break;
            }
            return component;
        }

        public static string ToType(RecipeComponent component)
        {
            string type = null;
            if (component is AbsoluteValue)
                type = "absolute";
            else if (component is RecipeText)
                type = "text";
            else if (component is RecipePartMember)
                type = "member";
            else if (component is RecipePart)
                type = "part";
            return type;
        }

        public abstract void ToXml(XmlTextWriter writer);
        public abstract void FromXml(XElement element);
    }
}
