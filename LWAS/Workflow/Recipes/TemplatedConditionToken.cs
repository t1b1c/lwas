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
using LWAS.Expressions.Extensions;

namespace LWAS.Workflow.Recipes
{
    public class TemplatedConditionToken : TemplatedToken
    {
        public override TransitTokenTypeEnum TokenType
        {
            get { return TransitTokenTypeEnum.Reference; }
            set { ; }
        }

        public override string SerializedName
        {
            get { return null; }
        }

        public TemplatedConditionToken(IExpressionsManager expressionsManager)
            : base(expressionsManager)
        { }

        public override TemplatedFlow Make(Recipe recipe, MakePolicyType makePolicy)
        {
            TemplatedConditionToken result = new TemplatedConditionToken(this.ExpressionsManager);
            result.Id = this.Id;
            result.Member = this.Member;
            result.Expression = this.Expression;

            Make(recipe, makePolicy, result);

            return result;
        }

        public override void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteAttributeString("sender", this.Id);
            writer.WriteAttributeString("milestone", this.Member);

            if (null != this.Expression)
                this.Expression.ToXml(writer);
        }

        public override void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Id = element.Attribute("sender").Value;
            this.Member = element.Attribute("milestone").Value;

            XElement expressionElement = element.Elements()
                                                .SingleOrDefault(x => null != x.Attribute("configKey") &&
                                                                      "expression" == x.Attribute("configKey").Value);
            if (null != expressionElement)
                this.Expression = this.ExpressionsManager.Expression(expressionElement);
        }
    }
}
