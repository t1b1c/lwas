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
    public class RecipePartMember : RecipePart
    {
        public string Member { get; set; }
        public string Part { get; set; }
        public bool IsValueFromReference { get; set; }

        public string ValuePart
        {
            get
            {
                if (String.IsNullOrEmpty(this.Value))
                    return null;
                string[] pathparts = this.Value.Split('.');
                return pathparts[0];
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    this.ChangeValue("");
                else
                {
                    string[] pathparts = this.Value.Split('.');
                    pathparts[0] = value;
                    this.ChangeValue(String.Join(".", pathparts));
                }
            }
        }
        public string ValueMember
        {
            get
            {
                if (String.IsNullOrEmpty(this.Value))
                    return null;
                string val = this.Value;
                if (!String.IsNullOrEmpty(this.ValuePart))
                    val = this.Value.Replace(this.ValuePart, "").Replace(this.Member, "");
                val = val.TrimStart('.');
                return val;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    this.ChangeValue(this.ValuePart);
                else
                    this.ChangeValue(String.Format("{0}.{1}.{2}", this.ValuePart, this.Member, value));
            }
        }

        public override bool HasValue
        {
            get { return !String.IsNullOrEmpty(this.ValueMember); }
        }

        protected override void WriteAttributes(XmlTextWriter writer)
        {
            base.WriteAttributes(writer);
            writer.WriteAttributeString("member", this.Member);
            writer.WriteAttributeString("part", this.Part);
            writer.WriteAttributeString("isValueFromReference", this.IsValueFromReference.ToString());
        }

        public override void FromXml(XElement element)
        {
            base.FromXml(element);
            this.Member = element.Attribute("member").Value;
            if (null != element.Attribute("part"))
                this.Part = element.Attribute("part").Value;
            bool isVfR = false;
            bool.TryParse(element.Attribute("isValueFromReference").Value, out isVfR);
            this.IsValueFromReference = isVfR;
        }
    }
}
