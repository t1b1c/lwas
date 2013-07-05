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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace LWAS.Database
{
    public class FieldsCollection: IEnumerable<Field>
    {
        List<Field> Fields { get; set; }
        public int Count 
        { 
            get { return this.Fields.Count; } 
        }

        public FieldsCollection()
        {
            this.Fields = new List<Field>();
        }

        public void Add(Field field)
        {
            if (null == field) throw new ArgumentNullException("field");
            this.Fields.Add(field);
        }

        public Field Add(Table table, string name, string description)
        {
            Field field = new Field(table, name, description);
            this.Fields.Add(field);
            return field;
        }

        public Field Add(Table table, string name, string description, string dbtype)
        {
            Field field = Add(table, name, description);
            field.DBType = dbtype;
            return field;
        }

        public Field Add(Table table, string name, string description, string dbtype, bool isPrimaryKey)
        {
            Field field = Add(table, name, description, dbtype);
            field.IsPrimaryKey = isPrimaryKey;
            return field;
        }

        public void Clear()
        {
            this.Fields.Clear();
        }

        public void Remove(Field field)
        {
            if (this.Fields.Contains(field))
                this.Fields.Remove(field);
        }

        public Field this[string name]
        {
            get
            {
                return this.FirstOrDefault(f => f.Name == name);
            }
        }

        public IEnumerator<Field> GetEnumerator()
        {
            return this.Fields.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("fields");
            foreach (Field field in this.Fields)
                field.ToXml(writer);
            writer.WriteEndElement();   // fields
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            foreach (XElement fieldElement in element.Elements("field"))
            {
                Field field = new Field();
                field.FromXml(fieldElement);
                this.Add(field);
            }
        }

        public void ToSql(StringBuilder builder, Dictionary<Field, string> aliases)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            foreach (Field field in this.Fields)
            {
                builder.Append("    ");
                string alias = aliases.ContainsKey(field) ? aliases[field] : null;
                field.ToSql(builder, alias);
                builder.AppendLine(",");
            }
            builder.Remove(builder.Length - 3, 3);
        }
    }
}
