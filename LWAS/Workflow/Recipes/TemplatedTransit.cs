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
using LWAS.Expressions.Extensions;

namespace LWAS.Workflow.Recipes
{
    public class TemplatedTransit : TemplatedFlow
    {
        public bool Persistent { get; set; }
        public TemplatedTransitSource Source { get; set; }
        public TemplatedTransitDestination Destination { get; set; }
        public IExpression Expression { get; set; }

        public TemplatedTransit(IExpressionsManager expressionsManager)
            : base(expressionsManager)
        { }

        public override TemplatedFlow Make(Recipe recipe, MakePolicyType makePolicy)
        {
            TemplatedTransit transit = new TemplatedTransit(this.ExpressionsManager);
            transit.Persistent = this.Persistent;
            transit.Source = (TemplatedTransitSource)this.Source.Make(recipe, makePolicy);
            transit.Destination = (TemplatedTransitDestination)this.Destination.Make(recipe, makePolicy);
            
            transit.Expression = this.Expression;
            Make(recipe, makePolicy, transit.Expression);

            return transit;
        }

        public override void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("transit");
            writer.WriteAttributeString("key", this.Key);
            writer.WriteAttributeString("persistent", this.Persistent.ToString());

            this.Source.ToXml(writer);
            this.Destination.ToXml(writer);

            if (null != this.Expression)
                this.Expression.ToXml(writer);

            writer.WriteEndElement();   // transit
        }

        public override void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Key = element.Attribute("key").Value;
            this.Persistent = bool.Parse(element.Attribute("persistent").Value);

            this.Source = new TemplatedTransitSource(this.ExpressionsManager);
            if (null != element.Element(this.Source.SerializedName))
                this.Source.FromXml(element.Element(this.Source.SerializedName));

            this.Destination = new TemplatedTransitDestination(this.ExpressionsManager);
            if (null != element.Element(this.Destination.SerializedName))
                this.Destination.FromXml(element.Element(this.Destination.SerializedName));

            XElement expressionElement = element.Elements()
                                                .SingleOrDefault(x => null != x.Attribute("configKey") &&
                                                                      "expression" == x.Attribute("configKey").Value);
            if (null != expressionElement)
                this.Expression = this.ExpressionsManager.Expression(expressionElement);
        }

    }
}
