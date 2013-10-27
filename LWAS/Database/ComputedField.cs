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
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Expressions;

using LWAS.Expressions;
using LWAS.Expressions.Extensions;
using LWAS.Database.Expressions;

namespace LWAS.Database
{
    public class ComputedField : Field
    {
        public const string XML_KEY = "computedFields";

        public DatabaseExpression Expression { get; set; }
        public ViewsManager Manager { get; set; }

        public ComputedField()
        { }

        public ComputedField(ViewsManager manager)
        {
            if (null == manager) throw new ArgumentNullException("manager");

            this.Manager = manager;
        }

        public override void ReadAttributes(XElement element)
        {
        }

        protected override void ReadSubelements(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");
            if (null == this.Manager) throw new InvalidOperationException("ViewsManager not set");

            XElement expressionElement = element.Elements()
                                                .SingleOrDefault(el =>
                                                {
                                                    return el.Attribute("configKey") != null &&
                                                           el.Attribute("configKey").Value == "expression";
                                                });
            if (null != expressionElement)
            {
                XElement typeElement = expressionElement.Element("attributes")
                                                        .Elements()
                                                        .SingleOrDefault(el =>
                                                        {
                                                            return el.Attribute("configKey") != null &&
                                                                   el.Attribute("configKey").Value == "type";
                                                        });
                if (null != typeElement)
                {
                    DatabaseExpression expression = this.Manager.ExpressionsManager.Token(typeElement.Attribute("value").Value) as DatabaseExpression;
                    if (null != expression)
                        expression.FromXml(expressionElement, this.Manager.ExpressionsManager);

                    this.Expression = expression;
                }
            }

        }

        public override void WriteAttributes(XmlTextWriter writer)
        {
        }

        protected override void WriteSubelements(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            if (null != this.Expression)
                this.Expression.ToXml(writer);
        }

        public override void ToSql(StringBuilder builder, string alias)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            if (null != this.Expression)
                this.Expression.ToSql(builder);

            builder.AppendFormat(" as [{0}]", alias ?? this.Alias ?? this.Name);
        }
    }
}
