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

namespace LWAS.Database
{
    public class ViewsCollection : Dictionary<string, View>
    {
        public ViewsManager Manager { get; set;}

        public ViewsCollection(ViewsManager manager) 
        {
            this.Manager = manager;
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("views");
            foreach (View view in this.Values)
                view.ToXml(writer);
            writer.WriteEndElement();   // views
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            foreach (XElement viewElement in element.Elements("view"))
            {
                View view = new View(this.Manager);
                view.FromXml(viewElement);
                this.Add(view.Name, view);
            }
        }

        public void ToSql(StringBuilder builder)
        {
            if (null == builder) throw new ArgumentNullException("builder");
        }
    }
}
