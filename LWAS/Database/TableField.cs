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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace LWAS.Database
{
    public class TableField : Field
    {
        public const string XML_KEY = "fields";

        public Table Table { get; set; }
        public bool IsPrimaryKey { get; set; }

        public TableField() { }
        public TableField(Table table, string name, string description) 
            : base(name, description)
        {
            if (null == table) throw new ArgumentNullException("table");

            this.Table = table;
        }
        public TableField(Table table, string name, string description, string dbtype)
            : this(table, name, description)
        {
            this.DBType = dbtype;
        }
        public TableField(Table table, string name, string description, string dbtype, bool isPrimaryKey)
            : this(table, name, description, dbtype)
        {
            this.IsPrimaryKey = isPrimaryKey;
        }

        public override void WriteAttributes(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            if (this.IsPrimaryKey)
                writer.WriteAttributeString("isPrimaryKey", this.IsPrimaryKey.ToString());
        }

        public override void ReadAttributes(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            bool isPrimaryKey = false;
            if (null != element.Attribute("isPrimaryKey") && bool.TryParse(element.Attribute("isPrimaryKey").Value, out isPrimaryKey))
                this.IsPrimaryKey = isPrimaryKey;
        }

        public override void ToSql(StringBuilder builder)
        {
            ToSql(builder, null);
        }

        public override void ToSql(StringBuilder builder, string alias)
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
