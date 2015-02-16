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
    public abstract class Field
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DBType { get; set; }
        public string Alias { get; set; }

        public Field() { }
        public Field(string name, string description)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            this.Name = name;
            this.Description = description;
        }
        public Field(string name, string description, string dbtype)
            : this(name, description)
        {
            this.DBType = dbtype;
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("field");
            writer.WriteAttributeString("name", this.Name);
            writer.WriteAttributeString("dbtype", this.DBType);

            WriteAttributes(writer);

            writer.WriteAttributeString("description", this.Description);
            if (!String.IsNullOrEmpty(this.Alias))
                writer.WriteAttributeString("alias", this.Alias);

            WriteSubelements(writer);

            writer.WriteEndElement();   // field
        }

        protected virtual void WriteSubelements(XmlTextWriter writer)
        {
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Name = element.Attribute("name").Value;
            if (null != element.Attribute("description"))
                this.Description = element.Attribute("description").Value;
            if (null != element.Attribute("dbtype"))
                this.DBType = element.Attribute("dbtype").Value;

            ReadAttributes(element);

            if (null != element.Attribute("alias"))
                this.Alias = element.Attribute("alias").Value;

            ReadSubelements(element);
        }

        protected virtual void ReadSubelements(XElement element)
        {
        }

        public abstract void ReadAttributes(XElement element);
        public abstract void WriteAttributes(XmlTextWriter writer);
        public abstract void ToSql(StringBuilder builder);
        public abstract void ToSql(StringBuilder builder, string alias);

    }
}
