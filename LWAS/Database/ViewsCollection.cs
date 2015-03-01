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
            List<View> views = this.Values.ToList();
            
            // init a linked list with base views
            LinkedList<View> linkedviews = new LinkedList<View>(views.Where(v => v.Subviews.Count == 0)
                                                                     .OrderBy(v => v.Name));

            foreach (var super in views.Where(v => v.Subviews.Count > 0)
                                        .OrderBy(v =>
                                        {
                                            if (null != views.FirstOrDefault(av => av.Subviews.ContainsKey(v)))
                                            {
                                                int firstparent = views.Where(av => av.Subviews.ContainsKey(v))
                                                                        .Min(ap => views.IndexOf(ap));
                                                return views.IndexOf(v) <= firstparent;
                                            }
                                            return false;
                                        }))
            {
                // find the latest view that's a subview to this view
                var last = linkedviews.LastOrDefault(sv => super.Subviews.Keys.Contains(sv));

                if (null != last)
                {
                    // get the linked node
                    var last_node = linkedviews.Find(last);
                    // add the view after last subview
                    linkedviews.AddAfter(last_node, super);
                }
                else
                    linkedviews.AddLast(super);
            }

            foreach (View view in linkedviews)
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
