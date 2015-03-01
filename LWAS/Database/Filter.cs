/*
 * Copyright 2006-2015 TIBIC SOLUTIONS
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
using System.Text;
using System.Xml.Linq;

using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Expressions.Extensions;

namespace LWAS.Database
{
    public class Filter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IExpression Expression { get; set; }
        public ViewsManager Manager { get; set; }

        public Filter(ViewsManager manager)
        {
            this.Manager = manager;
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("filter");
            writer.WriteAttributeString("name", this.Name);
            writer.WriteAttributeString("description", this.Description);
            if (null != this.Expression)
                this.Expression.ToXml(writer);
            writer.WriteEndElement();   // filter
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");
            if (null == this.Manager.ExpressionsManager) throw new InvalidOperationException("The current ViewsManager has no IExpressionsManager");

            this.Name = element.Attribute("name").Value;
            this.Description = element.Attribute("description").Value;

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
                    IExpression expression = this.Manager.ExpressionsManager.Token(typeElement.Attribute("value").Value) as IExpression;
                    if (null != expression)
                        expression.FromXml(expressionElement, this.Manager.ExpressionsManager);

                    this.Expression = expression;
                }
            }

        }

        public void ToSql(StringBuilder builder)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            if (null != this.Expression)
                this.Expression.ToSql(builder);
        }
    }
}
