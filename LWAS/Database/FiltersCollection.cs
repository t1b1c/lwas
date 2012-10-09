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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace LWAS.Database
{
    public class FiltersCollection: Dictionary<string, Filter>, IEnumerable<Filter>
    {
        public ViewsManager Manager { get; set; }
        public string Cluster { get; set; }
        
        public FiltersCollection(ViewsManager manager) 
        {
            this.Manager = manager;
        }

        public new IEnumerator<Filter> GetEnumerator()
        {
            return this.Values.GetEnumerator();
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("filters");
            if (!String.IsNullOrEmpty(this.Cluster))
                writer.WriteAttributeString("cluster", this.Cluster);
            foreach (Filter filter in this.Values)
                filter.ToXml(writer);
            writer.WriteEndElement();   // filters
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            if (null != element.Attribute("cluster"))
                this.Cluster = element.Attribute("cluster").Value;
            foreach (XElement filterElement in element.Elements("filter"))
            {
                Filter filter = new Filter(this.Manager);
                filter.FromXml(filterElement);
                this.Add(filter.Name, filter);
            }
        }

        public void ToSql(StringBuilder builder)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            bool isFirst = true;
            foreach (Filter filter in this.Values)
            {
                builder.Append("    ");
                if (!isFirst)
                {
                    if (!String.IsNullOrEmpty(this.Cluster))
                        builder.AppendFormat("{0} (", this.Cluster);
                    else
                        builder.Append("and (");
                }
                filter.ToSql(builder);
                if (!isFirst)
                    builder.AppendLine(")");
                else
                    builder.AppendLine();
                isFirst = false;
            }
        }
    }
}
