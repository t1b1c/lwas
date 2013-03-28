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
using System.Xml;
using System.Text;
using System.Xml.Linq;

namespace LWAS.Database
{
    public class Table
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public RelationsCollection Relations { get; set; }
        public FieldsCollection Fields { get; set; }
        public ViewsManager Manager { get; set; }

        public Table(ViewsManager manager) 
        {
            this.Manager = manager;
            this.Relations = new RelationsCollection(manager);
            this.Fields = new FieldsCollection();
        }
        public Table(ViewsManager manager, string name)
            : this(manager)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("table");
            this.Name = name;
        }
        public Table(ViewsManager manager, string name, string description)
            : this(manager, name)
        {
            this.Description = description;
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("table");
            writer.WriteAttributeString("name", this.Name);
            writer.WriteAttributeString("description", this.Description);

            this.Fields.ToXml(writer);

            writer.WriteStartElement("relations");
            foreach (Relation relation in this.Relations)
            {
                writer.WriteStartElement("relation");
                writer.WriteAttributeString("name", relation.Name);
                writer.WriteEndElement();   // relation
            }
            writer.WriteEndElement();   // relations

            writer.WriteEndElement();   // table
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Name = element.Attribute("name").Value;
            if (null != element.Attribute("description"))
                this.Description = element.Attribute("description").Value;

            this.Fields.FromXml(element.Element("fields"));

            foreach (Field field in this.Fields)
                field.Table = this;
        }

        public void ToSql(StringBuilder builder)
        {
            if (null == builder) throw new ArgumentNullException("builder");
        }
    }
}
