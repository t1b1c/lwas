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
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Interfaces.WorkFlow;
using LWAS.Infrastructure;
using LWAS.WorkFlow;
using LWAS.WorkFlow.Conditions;

namespace LWAS.WebParts.Parsers
{
	public class WorkFlowManagerConfigurationParser : ConfigurationParser
	{
		public override IResult Parse(object source)
		{
			WorkFlowManagerWebPart workflowManagerWebPart = source as WorkFlowManagerWebPart;
			if (null == workflowManagerWebPart)
			{
				throw new ArgumentException("source is not a WorkFlowManagerWebPart");
			}
			IConfiguration configuration = workflowManagerWebPart.Configuration;
			if (null == configuration)
			{
				throw new ArgumentException("source does not have a configuration");
			}
			WebPartManager Manager = WebPartManager.GetCurrentWebPartManager(workflowManagerWebPart.Page);
			if (configuration.Sections.ContainsKey("flows"))
			{
				WorkFlow.WorkFlow flow = new WorkFlow.WorkFlow();
				flow.Monitor = workflowManagerWebPart.Monitor;
				flow.Title = "screen flow";
				IConfigurationSection flowsSection = configuration.GetConfigurationSectionReference("flows");
				foreach (IConfigurationElement flowElement in flowsSection.Elements.Values)
				{
					Job job = new Job();
					job.Monitor = workflowManagerWebPart.Monitor;
					job.Title = flowElement.ConfigKey;
					if (flowElement.Elements.ContainsKey("conditions"))
					{
						foreach (IConfigurationElement conditionElement in flowElement.Elements["conditions"].Elements.Values)
						{
							if (flowElement.Elements["conditions"].Attributes.ContainsKey("type"))
							{
								ReflectionServices.SetValue(job.Conditions, "Type", flowElement.Elements["conditions"].GetAttributeReference("type").Value.ToString());
							}
							if (conditionElement.Attributes.ContainsKey("waitfor") && !("waitfor" == conditionElement.GetAttributeReference("type").Value.ToString()))
							{
								throw new InvalidOperationException("Unknown condition type " + conditionElement.GetAttributeReference("type").Value.ToString());
							}
							WaitForCondition condition = new WaitForCondition();
							condition.Monitor = workflowManagerWebPart.Monitor;
							if (conditionElement.Attributes.ContainsKey("milestone"))
							{
								condition.Milestone = conditionElement.GetAttributeReference("milestone").Value.ToString();
							}
							if (conditionElement.Attributes.ContainsKey("sender"))
							{
								condition.Chronicler = (ReflectionServices.FindControlEx(conditionElement.GetAttributeReference("sender").Value.ToString(), Manager) as IChronicler);
							}
							condition.Target = job;
							job.Conditions.Add(condition);
							if (conditionElement.Elements.ContainsKey("expression"))
							{
								IConfigurationElement expressionElement = conditionElement.GetElementReference("expression");
								IExpression expression = workflowManagerWebPart.ExpressionsManager.Token(expressionElement.GetAttributeReference("type").Value.ToString()) as IExpression;
								if (null != expression)
								{
									expression.Make(expressionElement, workflowManagerWebPart.ExpressionsManager);
									condition.Expression = expression;
								}
							}
						}
					}
					if (flowElement.Elements.ContainsKey("transits"))
					{
						foreach (IConfigurationElement transitElement in flowElement.GetElementReference("transits").Elements.Values)
						{
							if (transitElement.Attributes.ContainsKey("key"))
							{
								Transit transit = new Transit();
								transit.Monitor = workflowManagerWebPart.Monitor;
								transit.Title = transitElement.ConfigKey;
								transit.Key = transitElement.GetAttributeReference("key").Value.ToString();
								transit.Storage = workflowManagerWebPart.StatePersistence;
								if (transitElement.Elements.ContainsKey("expression"))
								{
									IConfigurationElement expressionElement = transitElement.GetElementReference("expression");
									IExpression expression = workflowManagerWebPart.ExpressionsManager.Token(expressionElement.GetAttributeReference("type").Value.ToString()) as IExpression;
									if (null == expression)
									{
										throw new InvalidOperationException("Token is not an IExpression");
									}
									expression.Make(expressionElement, workflowManagerWebPart.ExpressionsManager);
									transit.Expression = expression;
								}
								if (transitElement.Elements.ContainsKey("source"))
								{
									transit.Source = this.CreateTransitPoint(Manager, transitElement.GetElementReference("source"));
								}
								if (transitElement.Elements.ContainsKey("destination"))
								{
									transit.Destination = this.CreateTransitPoint(Manager, transitElement.GetElementReference("destination"));
								}
								if (transitElement.Attributes.ContainsKey("persistent"))
								{
									transit.IsPersistent = bool.Parse(transitElement.GetAttributeReference("persistent").Value.ToString());
								}
								job.Transits.Add(transit);
							}
						}
					}
					flow.Jobs.Add(job);
				}
				workflowManagerWebPart.WorkFlows.Add(flow);
			}
			return null;
		}
		private TransitPoint CreateTransitPoint(WebPartManager manager, IConfigurationElement element)
		{
			bool valid = false;
			TransitPoint transitPoint = new TransitPoint();
			if (element.Attributes.ContainsKey("id"))
			{
				valid = true;
				transitPoint.Chronicler = (ReflectionServices.FindControlEx(element.GetAttributeReference("id").Value.ToString(), manager) as IChronicler);
			}
			if (element.Attributes.ContainsKey("member"))
			{
				transitPoint.Member = element.GetAttributeReference("member").Value.ToString();
			}
			if (element.Attributes.ContainsKey("value"))
			{
				valid = true;
				transitPoint.Value = element.GetAttributeReference("value").Value;
			}
			if (!valid)
			{
				transitPoint = null;
			}
			return transitPoint;
		}
	}
}
