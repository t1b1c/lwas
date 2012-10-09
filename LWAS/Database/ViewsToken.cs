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
    public class ViewsToken: IToken
    {
        public string Key
        {
            get { return "views token"; }
        }

        public object Value { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }

        public ViewsToken() { }

        public void Make(IConfigurationType config, IExpressionsManager manager)
        {
            if (null == config) throw new ArgumentNullException("config");
            if (null == manager) throw new ArgumentNullException("manager");

            IConfigurationElement tokenElement = config as IConfigurationElement;
            if (null == tokenElement) throw new ArgumentException("config is not an IConfigurationElement");
            if (!tokenElement.Elements.ContainsKey("source")) throw new ConfigurationException("Bad views token configuration: 'source' element not found");
            IConfigurationElement sourceElement = tokenElement.GetElementReference("source");
            if (!sourceElement.Attributes.ContainsKey("table")) throw new ConfigurationException("Bad views token configuration: 'source' element has no 'table' attribute");
            this.TableName = sourceElement.GetAttributeReference("table").Value.ToString();
            if (!sourceElement.Attributes.ContainsKey("field")) throw new ConfigurationException("Bad views token configuration: 'source' element has no 'field' attribute");
            this.FieldName = sourceElement.GetAttributeReference("field").Value.ToString();
        }

        public IResult Evaluate()
        {
            throw new NotImplementedException();
        }

        public void ToConfiguration(IConfigurationElement config)
        {
            if (null == config) throw new ArgumentNullException("config");

            IConfigurationElement sourceElement = config.AddElement("source");
            sourceElement.AddAttribute("table").Value = this.TableName;
            sourceElement.AddAttribute("field").Value = this.FieldName;
        }

        public void ToSql(StringBuilder builder)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            builder.AppendFormat("[{0}].[{1}]", this.TableName, this.FieldName);
        }
    }
}
