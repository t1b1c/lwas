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

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Infrastructure;

namespace LWAS.Expressions
{
	public class BasicToken : Token, IBasicToken, IToken
	{
		public override string Key
		{
			get { return "basic"; }
		}

        public object Source { get; set; }
        public string Member { get; set; }

        public override bool IsValid
        {
            get { return null != this.Value; }
        }

		public override IResult Evaluate()
		{
			IResult result = base.Evaluate();
            if (!this.IsValid)
            {
                try
                {
                    if (null == this.Source) throw new InvalidOperationException("Source is not defined");
                    if (null == this.Member)
                        this.Value = this.Source;
                    else
                        this.Value = ReflectionServices.ExtractValue(this.Source, this.Member);

                    result.Status = ResultStatus.Successful;
                }
                catch (Exception e)
                {
                    result.Exceptions.Add(e);
                    result.Status = ResultStatus.Unsuccessful;
                }
            }

			return result;
		}

		public override void Make(IConfigurationType config, IExpressionsManager manager)
		{
			if (null == config) throw new ArgumentNullException("config");
			IConfigurationElement element = config as IConfigurationElement;
			if (null == element) throw new ArgumentException("config must be an IConfigurationElement");

            if (element.Elements.ContainsKey("source"))
            {
                IConfigurationElement sourceElement = element.GetElementReference("source");

                if (sourceElement.Attributes.ContainsKey("id"))
                {
                    if (null == manager) throw new ArgumentNullException("manager");

                    this.Source = manager.FindControl(sourceElement.GetAttributeReference("id").Value.ToString());
                    if (null == this.Source)
                        this.Source = sourceElement.GetAttributeReference("id").Value;
                    if (sourceElement.Attributes.ContainsKey("member"))
                        this.Member = sourceElement.GetAttributeReference("member").Value.ToString();
                }
                else
                {
                    if (sourceElement.Attributes.ContainsKey("value"))
                        this.Source = sourceElement.GetAttributeReference("value").Value;
                }
            }
		}
	}
}
