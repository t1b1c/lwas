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
    public class Field
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DBType { get; set; }
        public Table Table { get; set; }
        public bool IsPrimaryKey { get; set; }
        public string Alias { get; set; }

        public Field() { }
        public Field(Table table, string name, string description)
        {
            if (null == table) throw new ArgumentNullException("table");
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            this.Table = table;
            this.Name = name;
            this.Description = description;
        }
        public Field(Table table, string name, string description, string dbtype)
            : this(table, name, description)
        {
            this.DBType = dbtype;
        }
        public Field(Table table, string name, string description, string dbtype, bool isPrimaryKey)
            : this(table, name, description, dbtype)
        {
            this.IsPrimaryKey = isPrimaryKey;
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("field");
            writer.WriteAttributeString("name", this.Name);
            writer.WriteAttributeString("dbtype", this.DBType);
            if (this.IsPrimaryKey)
                writer.WriteAttributeString("isPrimaryKey", this.IsPrimaryKey.ToString());
            writer.WriteAttributeString("description", this.Description);
            if (!String.IsNullOrEmpty(this.Alias))
                writer.WriteAttributeString("alias", this.Alias);
            writer.WriteEndElement();   // field
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Name = element.Attribute("name").Value;
            if (null != element.Attribute("description"))
                this.Description = element.Attribute("description").Value;
            this.DBType = element.Attribute("dbtype").Value;
            bool isPrimaryKey = false;
            if (null != element.Attribute("isPrimaryKey") && bool.TryParse(element.Attribute("isPrimaryKey").Value, out isPrimaryKey))
                this.IsPrimaryKey = isPrimaryKey;
            if (null != element.Attribute("alias"))
                this.Alias = element.Attribute("alias").Value;
        }

        public void ToSql(StringBuilder builder, string alias)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            if (null != this.Table && !String.IsNullOrEmpty(this.Table.Name) && !String.IsNullOrEmpty(this.Name))
            {
                builder.AppendFormat("[{0}].[{1}]", this.Table.Name, this.Name);
                if (!String.IsNullOrEmpty(alias))
                    builder.AppendFormat(" as [{0}]", alias);
            }
        }
    }
}
