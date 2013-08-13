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

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Exceptions;
using LWAS.Infrastructure;

namespace LWAS.Database
{
    public class FieldToken: BaseViewsToken
    {
        public override string Key
        {
            get { return "field token"; }
        }

        public string FieldName { get; set; }

        public override void Make(IConfigurationType config, IExpressionsManager manager)
        {
            if (null == config) throw new ArgumentNullException("config");
            if (null == manager) throw new ArgumentNullException("manager");

            IConfigurationElement tokenElement = config as IConfigurationElement;
            if (null == tokenElement) throw new ArgumentException("config is not an IConfigurationElement");
            if (!tokenElement.Elements.ContainsKey("source")) throw new ConfigurationException("Bad views field token configuration: 'source' element not found");
            IConfigurationElement sourceElement = tokenElement.GetElementReference("source");

            Make(sourceElement, manager);
        }

        protected virtual void Make(IConfigurationElement sourceElement, IExpressionsManager manager)
        {
            if (null == sourceElement) throw new ArgumentNullException("sourceElement");
            if (null == manager) throw new ArgumentNullException("manager");

            if (!sourceElement.Attributes.ContainsKey("field")) throw new ConfigurationException("Bad views field token configuration: 'source' element has no 'field' attribute");
            this.FieldName = sourceElement.GetAttributeReference("field").Value.ToString();
        }

        public override IResult Evaluate()
        {
            return null;
        }

        public override void ToConfiguration(IConfigurationElement config)
        {
            if (null == config) throw new ArgumentNullException("config");

            IConfigurationElement sourceElement = config.AddElement("source");
            WriteConfiguration(sourceElement);
        }

        protected virtual void WriteConfiguration(IConfigurationElement sourceElement)
        {
            if (null == sourceElement) throw new ArgumentNullException("sourceElement");

            sourceElement.AddAttribute("field").Value = this.FieldName;
        }

        public override void ToSql(StringBuilder builder)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            builder.AppendFormat("[{0}]", this.FieldName);
        }
    }
}
