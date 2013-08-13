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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Workflow.Recipes
{
    public class TemplatedFlowCollection : IEnumerable<TemplatedFlow>
    {
        List<TemplatedFlow> list;

        IExpressionsManager _expressionsManager;
        public IExpressionsManager ExpressionsManager
        {
            get { return _expressionsManager; }
        }

        public TemplatedFlowCollection(IExpressionsManager expressionsManager)
        {
            _expressionsManager = expressionsManager;
            list = new List<TemplatedFlow>();
        }

        public void Add(TemplatedFlow path)
        {
            list.Add(path);
        }

        public void AddAt(TemplatedFlow path, int index)
        {
            this[index] = path;
        }

        public void Remove(TemplatedFlow path)
        {
            list.Remove(path);
        }

        public TemplatedFlow this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list.Insert(index, value);
            }
        }

        public void Clear()
        {
            list.Clear();
        }

        public void Union(TemplatedFlowCollection collection)
        {
            list = new List<TemplatedFlow>(list.Union(collection));
        }

        public void Merge(TemplatedFlowCollection collection)
        {
            List<TemplatedFlow> merged = new List<TemplatedFlow>();
            foreach (TemplatedJob job in list.OfType<TemplatedJob>())
            {
                string serialized_conditions = job.Conditions.ToXmlString();
                foreach (TemplatedJob similar in collection.OfType<TemplatedJob>()
                                                           .Where(j => String.Equals(j.Conditions.ToXmlString(), serialized_conditions)))
                {
                    foreach (TemplatedTransit transit in similar.Transits)
                        job.Transits.Add(transit);
                    merged.Add(similar);
                }
            }

            foreach (TemplatedFlow mergedFlow in merged)
                collection.Remove(mergedFlow);
            list = new List<TemplatedFlow>(list.Union(collection));
        }

        public IEnumerator<TemplatedFlow> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void ToXml(XmlTextWriter writer, string root)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement(root);
            foreach (TemplatedFlow path in this)
                path.ToXml(writer);
            writer.WriteEndElement();   // root 
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");
            
            foreach(XElement pathElement in element.Elements())
            {
                TemplatedFlow path = TemplatedFlow.FromType(pathElement.Name.LocalName, this.ExpressionsManager);
                path.FromXml(pathElement);
                Add(path);
            }
        }
    }
}
