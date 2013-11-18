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
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace LWAS.Database
{
    public class Relation
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Table MasterTable { get; set; }
        public TableField MasterField { get; set; }
        public Table DetailsTable { get; set; }
        public TableField DetailsField { get; set; }
        public ViewsManager Manager { get; set; }

        public Relation(ViewsManager manager)
        {
            this.Manager = manager;
        }

        public Relation(ViewsManager manager, string name, string description)
            : this(manager)
        {
            this.Name = name;
            this.Description = description;
        }
        public Relation(ViewsManager manager, string name, string description, TableField masterField, TableField detailsField)
            : this(manager, name, description)
        {
            if (null == masterField) throw new ArgumentNullException("masterField");
            if (null == detailsField) throw new ArgumentNullException("detailsField");

            this.MasterField = masterField;
            this.MasterTable = masterField.Table;
            this.DetailsField = detailsField;
            this.DetailsTable = detailsField.Table;

            if (String.IsNullOrEmpty(name))
                this.Name = String.Format("{0}.{1} TO {2}.{3}", this.MasterTable.Name, this.MasterField.Name, this.DetailsTable.Name, this.DetailsField.Name);

            this.MasterTable.Relations.Add(this);
            this.DetailsTable.Relations.Add(this);
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("relation");
            writer.WriteAttributeString("name", this.Name);
            writer.WriteAttributeString("description", this.Description);
            
            writer.WriteStartElement("master");
            if (null != this.MasterField)
            {
                writer.WriteAttributeString("table", this.MasterTable.Name);
                writer.WriteAttributeString("field", this.MasterField.Name);
            }
            writer.WriteEndElement();   // master

            writer.WriteStartElement("details");
            if (null != this.DetailsField)
            {
                writer.WriteAttributeString("table", this.DetailsTable.Name);
                writer.WriteAttributeString("field", this.DetailsField.Name);
            }
            writer.WriteEndElement();   // details

            writer.WriteEndElement();   // relation
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Name = element.Attribute("name").Value;
            this.Description = element.Attribute("description").Value;

            if (null != element.Element("master") && null != element.Element("master").Attribute("table"))
            {
                string masterTableName = element.Element("master").Attribute("table").Value;
                this.MasterTable = this.Manager.Tables[masterTableName];
            }
            if (null != element.Element("master") && null != element.Element("master").Attribute("field"))
            {
                string masterFieldName = element.Element("master").Attribute("field").Value;
                this.MasterField = this.MasterTable.Fields[masterFieldName];
            }

            if (null != element.Element("details") && null != element.Element("details").Attribute("table"))
            {
                string detailsTableName = element.Element("details").Attribute("table").Value;
                this.DetailsTable = this.Manager.Tables[detailsTableName];
            }
            if (null != element.Element("details") && null != element.Element("details").Attribute("field"))
            {
                string detailsFieldName = element.Element("details").Attribute("field").Value;
                this.DetailsField = this.DetailsTable.Fields[detailsFieldName];
            }

            if (null != this.MasterTable)
                this.MasterTable.Relations.Add(this);
            if (null != this.DetailsTable)
                this.DetailsTable.Relations.Add(this);
        }

        public void ToSql(StringBuilder builder, Table primaryTable, List<Table> skip)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            if (null != this.MasterTable && !String.IsNullOrEmpty(this.MasterTable.Name) &&
                null != this.MasterField && !String.IsNullOrEmpty(this.MasterField.Name) &&
                null != this.DetailsTable && !String.IsNullOrEmpty(this.DetailsTable.Name) &&
                null != this.DetailsField && !String.IsNullOrEmpty(this.DetailsField.Name))
            {
                Table table = null;
                if (!skip.Contains(this.MasterTable) && primaryTable != this.MasterTable)
                    table = this.MasterTable;
                if (null == table && !skip.Contains(this.DetailsTable) && primaryTable != this.DetailsTable)
                    table = this.DetailsTable;
                if (null != table)
                {
                    builder.Append("    ");
                    builder.AppendFormat("left join [{0}] on ", table.Name);
                    this.MasterField.ToSql(builder, this.MasterField.Alias);
                    builder.Append(" = ");
                    this.DetailsField.ToSql(builder, this.DetailsField.Alias);
                }
            }
        }
    }
}
