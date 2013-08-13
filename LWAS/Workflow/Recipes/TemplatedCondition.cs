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

using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Workflow.Recipes
{
    public class TemplatedCondition : TemplatedFlow
    {
        public TemplatedConditionToken Token { get; set; }

        public TemplatedCondition(IExpressionsManager expressionsManager)
            : base(expressionsManager)
        { }

        public override TemplatedFlow Make(Recipe recipe, MakePolicyType makePolicy)
        {
            TemplatedCondition condition = new TemplatedCondition(this.ExpressionsManager);
            condition.Token = (TemplatedConditionToken)this.Token.Make(recipe, makePolicy);

            return condition;
        }

        public override void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("condition");
            writer.WriteAttributeString("key", this.Key);

            this.Token.ToXml(writer);

            writer.WriteEndElement();   // condition
        }

        public override void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Key = element.Attribute("key").Value;

            this.Token = new TemplatedConditionToken(this.ExpressionsManager);
            this.Token.FromXml(element);
        }
    }
}
