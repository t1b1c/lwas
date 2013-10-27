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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using System.Xml.Linq;

using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Interfaces.Configuration;

using LWAS.Infrastructure.Configuration;

namespace LWAS.Expressions.Extensions
{
    public static class XmlExtensions
    {

        public static void ToXml(this IExpression expression, XmlTextWriter writer)
        {
            if (null == expression) throw new ArgumentNullException("expression");
            if (null == writer) throw new ArgumentNullException("writer");

            Configuration config = new Configuration();
            IConfigurationElement configElement = config.AddSection("expression extensions").AddElement("expression");
            expression.ToConfiguration(configElement);
            configElement.WriteXml(writer);
        }

        public static void FromXml(this IExpression expression, XElement element, IExpressionsManager expressionsManager)
        {
            if (null == expression) throw new ArgumentNullException("expression");
            if (null == element) throw new ArgumentNullException("element");
            if (null == expressionsManager) throw new ArgumentNullException("expressionsManager");

            Configuration config = new Configuration();
            IConfigurationElement configElement = config.AddSection("expression extensions").AddElement("expression");
            configElement.ReadXml(element.CreateReader());

            expression.Make(configElement, expressionsManager);
        }

        public static IExpression Expression(this IExpressionsManager manager, XElement element)
        {
            Configuration config = new Configuration();
            IConfigurationElement configElement = config.AddSection("expression extensions").AddElement("expression");
            configElement.ReadXml(element.CreateReader());
            IExpression expression = manager.Token(configElement.GetAttributeReference("type").Value.ToString()) as IExpression;
            if (null != expression)
                expression.Make(configElement, manager);
            return expression;
        }

        public static void ToConfiguration(this IToken token, IConfigurationElement config)
        {
            if (!config.Attributes.ContainsKey("type"))
                config.AddAttribute("type").Value = token.Key;
            else if ((string)config.GetAttributeReference("type").Value != token.Key)
                config.GetAttributeReference("type").Value = token.Key;

            IExpression expression = token as IExpression;
            if (null != expression)
            {
                IConfigurationElement operandsElement = config.AddElement("operands");
                int index = 0;
                foreach (IToken operand in expression.Operands.Where(o => null != o))
                {
                    IConfigurationElement operandElement = operandsElement.AddElement(index.ToString());
                    operand.ToConfiguration(operandElement);
                    index++;
                }
            }
            else
            {
                IBasicToken basicToken = token as IBasicToken;
                if (null != basicToken)
                {
                    IConfigurationElement sourceElement = config.AddElement("source");
                    if (null != basicToken.Source)
                    {
                        sourceElement.AddAttribute("id").Value = basicToken.Source;
                        if (!String.IsNullOrEmpty(basicToken.Member))
                            sourceElement.AddAttribute("member").Value = basicToken.Member;
                    }
                    else if (null != basicToken.Value)
                        sourceElement.AddAttribute("value").Value = basicToken.Source;
                }
                else
                {
                    System.Reflection.MethodInfo concreteMethod = token.GetType().GetMethod("ToConfiguration", new Type[] { typeof(IConfigurationElement) });
                    if (null != concreteMethod)
                        concreteMethod.Invoke(token, new object[] { config });
                }
            }
        }
    }
}
