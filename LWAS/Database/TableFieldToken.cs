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
    public class TableFieldToken: FieldToken
    {
        public override string Key
        {
            get { return "views field token"; }
        }

        public string TableName { get; set; }

        protected override void Make(IConfigurationElement sourceElement, IExpressionsManager manager)
        {
            if (null == sourceElement) throw new ArgumentNullException("sourceElement");
            if (null == manager) throw new ArgumentNullException("manager");

            if (!sourceElement.Attributes.ContainsKey("table")) throw new ConfigurationException("Bad views field token configuration: 'source' element has no 'table' attribute");
            this.TableName = sourceElement.GetAttributeReference("table").Value.ToString();

            base.Make(sourceElement, manager);
        }

        protected override void WriteConfiguration(IConfigurationElement sourceElement)
        {
            if (null == sourceElement) throw new ArgumentNullException("sourceElement");

            sourceElement.AddAttribute("table").Value = this.TableName;

            base.WriteConfiguration(sourceElement);
        }

        public override void ToSql(StringBuilder builder)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            builder.AppendFormat("[{0}].[{1}]", this.TableName, this.FieldName);
        }
    }
}
