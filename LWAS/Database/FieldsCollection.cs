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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace LWAS.Database
{
    public class FieldsCollection<T>: IEnumerable<T> where T : Field, new()
    {
        public class FieldEventArgs : EventArgs
        {
            public T Field { get; set; }
        }

        List<T> Fields { get; set; }
        public string XmlKey { get; private set; }
        public event EventHandler<FieldEventArgs> InitField;

        public int Count 
        { 
            get { return this.Fields.Count; } 
        }

        public FieldsCollection(string xmlKey)
        {
            this.Fields = new List<T>();
            this.XmlKey = xmlKey;
        }

        public void Add(T field)
        {
            if (null == field) throw new ArgumentNullException("field");
            this.Fields.Add(field);
        }

        public void Clear()
        {
            this.Fields.Clear();
        }

        public void Remove(T field)
        {
            if (this.Fields.Contains(field))
                this.Fields.Remove(field);
        }

        public T this[string name]
        {
            get
            {
                return this.FirstOrDefault(f => f.Name == name);
            }
        }

        public IEnumerator<T> GetEnumerator()
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

            writer.WriteStartElement(this.XmlKey);
            foreach (Field field in this.Fields)
                field.ToXml(writer);
            writer.WriteEndElement();   // fields
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            foreach (XElement fieldElement in element.Elements("field"))
            {
                T field = new T();
                if (null != InitField)
                    InitField(this, new FieldEventArgs() { Field = field });
                field.FromXml(fieldElement);
                this.Add(field);
            }
        }

        public void ToSql(StringBuilder builder, Dictionary<Field, string> aliases)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            bool first = true;
            foreach (T field in this.Fields)
            {
                if (first)
                    first = false;
                else
                    builder.AppendLine(",");
                builder.Append("    ");
                string alias = aliases.ContainsKey(field) ? aliases[field] : null;
                field.ToSql(builder, alias);
            }
        }
    }
}
