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
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Exceptions;
using LWAS.Infrastructure;

namespace LWAS.Database
{
    public class ParameterToken : ViewsToken
    {
        public override string Key
        {
            get { return "views parameter token"; }
        }

        public View View { get; set; }
        public string ParameterName { get; set; }

        public ParameterToken()
        { }

        public ParameterToken(View view)
        {
            if (null == view) throw new ArgumentNullException("view");

            this.View = view;
        }

        public override void Make(IConfigurationType config, IExpressionsManager manager)
        {
            if (null == config) throw new ArgumentNullException("config");
            if (null == manager) throw new ArgumentNullException("manager");

            IConfigurationElement tokenElement = config as IConfigurationElement;
            if (null == tokenElement) throw new ArgumentException("config is not an IConfigurationElement");
            if (!tokenElement.Elements.ContainsKey("source")) throw new ConfigurationException("Bad views token configuration: 'source' element not found");
            IConfigurationElement sourceElement = tokenElement.GetElementReference("source");
            if (!sourceElement.Attributes.ContainsKey("parameter")) throw new ConfigurationException("Bad views token configuration: 'source' element has no 'parameter' attribute");
            this.ParameterName = sourceElement.GetAttributeReference("parameter").Value.ToString();
        }

        public override IResult Evaluate()
        {
            if (null == this.View) throw new InvalidOperationException("View parameter evaluation failed. No view to evaluate upon");

            this.Value = this.View.Parameters[this.ParameterName];
            return null;
        }

        public override void ToConfiguration(IConfigurationElement config)
        {
            if (null == config) throw new ArgumentNullException("config");

            IConfigurationElement sourceElement = config.AddElement("source");
            sourceElement.AddAttribute("parameter").Value = this.ParameterName;
        }

        public override void ToSql(StringBuilder builder)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            builder.AppendFormat("{0}", this.View.Parameters.SqlIdentifier(this.ParameterName));
        }
    }
}
