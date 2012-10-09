﻿/*
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
using System.Xml;
using System.Xml.Linq;
using System.Text;

namespace LWAS.Database
{
    public class RelationsCollection : IEnumerable<Relation>
    {
        List<Relation> Relations { get; set; }
        public ViewsManager Manager { get; set; }

        public int Count
        {
            get { return this.Relations.Count; }
        }

        public RelationsCollection(ViewsManager manager) 
        {
            this.Relations = new List<Relation>();
            this.Manager = manager;
        }

        public void Add(Relation relation)
        {
            this.Relations.Add(relation);
        }
        public Relation Add(Field masterField, Field detailsField)
        {
            return Add(null, null, masterField, detailsField);
        }
        public Relation Add(string name, string description, Field masterField, Field detailsField)
        {
            Relation relation = new Relation(this.Manager, name, description, masterField, detailsField);
            Add(relation);
            return relation;
        }

        public void Clear()
        {
            this.Relations.Clear();
        }

        public void Remove(Relation relation)
        {
            if (this.Relations != null && this.Relations.Contains(relation))
                this.Relations.Remove(relation);
        }

        public Relation this[string name]
        {
            get
            {
                return this.Relations.FirstOrDefault(r => r.Name == name);
            }
        }

        public IEnumerator<Relation> GetEnumerator()
        {
            return this.Relations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("relations");
            foreach (Relation relation in this)
                relation.ToXml(writer);
            writer.WriteEndElement();   // relations
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            List<XElement> els = new List<XElement>(element.Elements("relation"));
            foreach (XElement relationElement in els)
            {
                Relation relation = new Relation(this.Manager);
                relation.FromXml(relationElement);
                this.Add(relation);
            }
        }

        public void ToSql(StringBuilder builder, Table primaryTable)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            foreach (Relation relation in this.Relations)
            {
                relation.ToSql(builder, primaryTable);
                builder.AppendLine();
            }
        }
    }
}
