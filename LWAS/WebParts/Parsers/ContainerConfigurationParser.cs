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
using System.Web.UI;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Interfaces.Validation;

namespace LWAS.WebParts.Parsers
{
	public class ContainerConfigurationParser : ConfigurationParser
	{
		public override IResult Parse(object source)
		{
			if (null == source)
			{
				throw new ArgumentNullException("source");
			}
			ContainerWebPart webPart = source as ContainerWebPart;
			if (null == webPart)
			{
				throw new ArgumentException("source is not an ContainerWebPart");
			}
			IConfiguration config = webPart.Configuration;
			if (null == config)
			{
				throw new ArgumentException("missing configuration");
			}
			IValidationManager validationManager = webPart.ValidationManager;
			if (null == validationManager)
			{
				throw new ArgumentException("missing validation manager");
			}
			base.Parse(source);
			if (config.Sections.ContainsKey("Template"))
			{
				IConfigurationSection templateConfig = config.GetConfigurationSectionReference("Template");
				webPart.TemplateConfig = templateConfig;
			}
			else
			{
				webPart.TemplateConfig = config.NewConfigurationSectionInstance("Template");
			}
			if (config.Sections.ContainsKey("Validation"))
			{
				foreach (IConfigurationElement element in config.Sections["Validation"].Elements.Values)
				{
					IValidationTask task = validationManager.NewValidationTaskInstance();
					if (element.Attributes.ContainsKey("target"))
					{
						task.Target = element.GetAttributeReference("target").Value;
					}
					if (element.Attributes.ContainsKey("member"))
					{
						task.Member = element.GetAttributeReference("member").Value.ToString();
					}
					foreach (IConfigurationElement handlerElement in element.Elements.Values)
					{
						if (handlerElement.Attributes.ContainsKey("handlerKey"))
						{
							string handlerKey = handlerElement.GetAttributeReference("handlerKey").Value.ToString();
							IValidationHandler handler = validationManager.Handler(handlerKey);
							if ("expression" == handlerKey)
							{
								IExpressionHandler expressionHandler = handler as IExpressionHandler;
								IEnumerator<IConfigurationElement> enumerator = handlerElement.Elements.Values.GetEnumerator();
								if (enumerator.MoveNext())
								{
									IConfigurationElement expressionElement = enumerator.Current;
									expressionHandler.Expression = (validationManager.ExpressionsManager.Token(expressionElement.GetAttributeReference("type").Value.ToString()) as IExpression);
									if (null == expressionHandler.Expression)
									{
										throw new InvalidOperationException("Token is not an IExpression");
									}
									expressionHandler.Expression.Make(expressionElement, validationManager.ExpressionsManager);
								}
							}
							task.Handlers.Add(handler);
						}
					}
					validationManager.RegisterTask(task);
				}
			}
			if (config.Sections.ContainsKey("Checks"))
			{
				foreach (IConfigurationElement element in config.Sections["Checks"].Elements.Values)
				{
					if (element.Attributes.ContainsKey("command"))
					{
						string command = element.GetAttributeReference("command").Value.ToString();
						webPart.Checks.Add(command, new List<Pair>());
						foreach (IConfigurationElement checkElement in element.Elements.Values)
						{
							Pair pair = new Pair();
							if (checkElement.Attributes.ContainsKey("error"))
							{
								pair.First = checkElement.GetAttributeReference("error").Value.ToString();
							}
							IEnumerator<IConfigurationElement> enumerator = checkElement.Elements.Values.GetEnumerator();
							if (enumerator.MoveNext())
							{
								IConfigurationElement expressionElement = enumerator.Current;
								IExpression expression = webPart.ExpressionsManager.Token(expressionElement.GetAttributeReference("type").Value.ToString()) as IExpression;
								if (null == expression)
								{
									throw new InvalidOperationException("Token is not an IExpression");
								}
								expression.Make(expressionElement, webPart.ExpressionsManager);
								pair.Second = expression;
							}
							webPart.Checks[command].Add(pair);
						}
					}
				}
			}
			return null;
		}
	}
}
