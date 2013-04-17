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
using System.Xml.Linq;
using System.Text;

using LWAS.Expressions.Extensions;

namespace LWAS.Database
{
    public class View
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Table Source { get; set; }
        public RelationsCollection Relationship { get; set; }
        public FiltersCollection Filters { get; set; }
        public FieldsCollection Fields { get; set; }
        public ParametersCollection Parameters { get; set; }
        public ViewsManager Manager { get; set; }

        public View(ViewsManager manager)
        {
            this.Manager = manager;
            this.Relationship = new RelationsCollection(this.Manager);
            this.Filters = new FiltersCollection(this.Manager);
            this.Fields = new FieldsCollection();
            this.Parameters = new ParametersCollection();
        }

        public IEnumerable<Table> RelatedTables()
        {
            if (this.Relationship.Count == 0)
                return new Table[] { this.Source };

            return this.Relationship
                        .Select<Relation, Table>(rel => rel.MasterTable)
                        .Union(this.Relationship
                                    .Select<Relation, Table>(rel => rel.DetailsTable))
                        .Distinct();
        }

        public IEnumerable<Table> RelatableTables()
        {
            ViewsManager vm = this.Manager;

            if (this.Relationship.Count == 0)
                return new Table[] { this.Source };

            return vm.Tables
                     .Values
                     .Where(t =>
                     {
                         return vm.Relations
                                  .Select<Relation, Table>(rel => rel.MasterTable)
                                  .Union(vm.Relations
                                           .Select<Relation, Table>(rel => rel.DetailsTable))
                                  .Where(tb =>
                                  {
                                      return this.Relationship
                                                 .Any(r =>
                                                 {
                                                     return r.DetailsTable == tb ||
                                                            r.MasterTable == tb;
                                                 })
                                             ||
                                             vm.Relations
                                               .Any(r =>
                                               {
                                                   return ((r.MasterTable == tb && r.DetailsTable == this.Source) ||
                                                           (r.DetailsTable == tb && r.MasterTable == this.Source));
                                               });
                                  })
                                  .Contains(t);

                     });
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("view");
            writer.WriteAttributeString("name", this.Name);
            writer.WriteAttributeString("description", this.Description);

            writer.WriteStartElement("fields");
            foreach (Field field in this.Fields)
            {
                writer.WriteStartElement("field");
                writer.WriteAttributeString("name", field.Name);
                writer.WriteAttributeString("table", field.Table.Name);
                if (!String.IsNullOrEmpty(field.Alias))
                    writer.WriteAttributeString("alias", field.Alias);
                writer.WriteEndElement();   // field
            }
            writer.WriteEndElement();   // fields

            writer.WriteStartElement("source");
            if (null != this.Source)
                writer.WriteAttributeString("name", this.Source.Name);
            writer.WriteEndElement();   // source

            writer.WriteStartElement("relationship");
            foreach (Relation relation in this.Relationship)
            {
                writer.WriteStartElement("relation");
                writer.WriteAttributeString("name", relation.Name);
                writer.WriteEndElement();   // relation
            }
            writer.WriteEndElement();   // relationship

            writer.WriteStartElement("filters");
            foreach (Filter filter in this.Filters.Values)
                filter.ToXml(writer);
            writer.WriteEndElement();   // filters

            this.Parameters.ToXml(writer);

            writer.WriteEndElement();   // view
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");
            this.Name = element.Attribute("name").Value;
            this.Description = element.Attribute("description").Value;

            foreach (XElement fieldElement in element.Element("fields").Elements("field"))
            {
                string tableName = fieldElement.Attribute("table").Value;
                string fieldName = fieldElement.Attribute("name").Value;
                if (!this.Manager.Tables.ContainsKey(tableName)) throw new ApplicationException(String.Format("Can't find table '{0}' to add it's field '{1}' to the view '{2}'", tableName, fieldName, this.Name));
                Table table = this.Manager.Tables[tableName];
                Field field = table.Fields[fieldName];
                if (null == field) throw new ApplicationException(String.Format("Table '{0}' does not contain field '{1}' to add it to the view '{2}'", tableName, fieldName, this.Name));
                if (null != fieldElement.Attribute("alias"))
                    field.Alias = fieldElement.Attribute("alias").Value;
                this.Fields.Add(field);
            }

            if (null != element.Element("source") && null != element.Element("source").Attribute("name"))
            {
                string sourceName = element.Element("source").Attribute("name").Value;
                this.Source = this.Manager.Tables[sourceName];
            }

            foreach (XElement relationElement in element.Element("relationship").Elements("relation"))
            {
                string relationName = relationElement.Attribute("name").Value;
                Relation relation = this.Manager.Relations[relationName];
                if (null == relation) throw new ApplicationException(String.Format("Cannot find the relation '{0}' to add it to view '{1}'", relationName, this.Name));
                this.Relationship.Add(relation);
            }

            this.Filters.FromXml(element.Element("filters"));

            if (null != element.Element("parameters"))
                this.Parameters.FromXml(element.Element("parameters"));
        }

        public void ToSql(StringBuilder builder)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            SetUpParameters();

            foreach (string parameter in this.Parameters)
            {
                string identifier = this.Parameters.SqlIdentifier(parameter);
                builder.AppendFormat("declare {0} varchar(max)", identifier);
                builder.AppendLine();
                builder.AppendFormat("set {0} = '{1}'", identifier, this.Parameters[parameter] ?? "");
            }

            builder.AppendLine();
            builder.AppendLine("select");
            this.Fields.ToSql(builder);
            builder.AppendLine();
            builder.AppendLine("from");
            builder.Append("    ");
            builder.AppendFormat("[{0}]", this.Source.Name);
            builder.AppendLine();
            if (this.Relationship.Count > 0)
                this.Relationship.ToSql(builder, this.Source);
            if (this.Filters.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("where");
                this.Filters.ToSql(builder);
            }
        }

        void SetUpParameters()
        {
            var parameters = this.Filters.SelectMany<Filter, ParameterToken>(f =>
                {
                    return f.Expression.Flatten()
                                       .SelectMany(e => e.Operands.OfType<ParameterToken>());
                });

            foreach (ParameterToken token in parameters)
                token.View = this;
        }
    }
}
