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
    public enum TransitTokenTypeEnum { Reference, Value }

    public abstract class TemplatedToken : TemplatedFlow
    {
        public virtual TransitTokenTypeEnum TokenType { get; set; }
        public virtual string Id { get; set; }
        public virtual string Member { get; set; }
        public virtual string Value { get; set; }
        public abstract string SerializedName { get; }
        public IExpression Expression { get; set; }

        public TemplatedToken(IExpressionsManager expressionsManager)
            : base(expressionsManager)
        { }

        protected void Make(Recipe recipe, MakePolicyType makePolicy, TemplatedToken result)
        {
            string r_id = result.Id;
            string r_member = result.Member;
            string r_value = result.Value;

            Make(recipe, makePolicy, ref r_id, ref r_member, ref r_value, result.TokenType == TransitTokenTypeEnum.Reference);

            result.Id = r_id;
            result.Member = r_member;
            result.Value = r_value;

            Make(recipe, makePolicy, result.Expression);
        }

        public override void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement(this.SerializedName);
            writer.WriteAttributeString("type", this.TokenType.ToString());
            if (this.TokenType == TransitTokenTypeEnum.Reference)
            {
                writer.WriteAttributeString("id", this.Id);
                writer.WriteAttributeString("member", this.Member);
            }
            else if (this.TokenType == TransitTokenTypeEnum.Value)
                writer.WriteAttributeString("value", this.Value);

            if (null != this.Expression)
                this.Expression.ToXml(writer);

            writer.WriteEndElement();   // token
        }

        public override void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.TokenType = (TransitTokenTypeEnum)Enum.Parse(typeof(TransitTokenTypeEnum), element.Attribute("type").Value);
            if (this.TokenType == TransitTokenTypeEnum.Reference)
            {
                this.Id = element.Attribute("id").Value;
                this.Member = element.Attribute("member").Value;
            }
            else if (this.TokenType == TransitTokenTypeEnum.Value)
                this.Value = element.Attribute("value").Value;

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
                    IExpression expression = this.ExpressionsManager.Token(typeElement.Attribute("value").Value) as IExpression;
                    if (null != expression)
                        expression.FromXml(expressionElement, this.ExpressionsManager);

                    this.Expression = expression;
                }
            }
        }
    }
}
