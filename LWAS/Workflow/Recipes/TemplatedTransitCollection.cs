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
 * WITHOUT WARRANTIES OR transitS OF ANY KIND, either express or implied.
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
    public class TemplatedTransitCollection : IEnumerable<TemplatedTransit>
    {
        List<TemplatedTransit> list;

        IExpressionsManager _expressionsManager;
        public IExpressionsManager ExpressionsManager
        {
            get { return _expressionsManager; }
        }

        public TemplatedTransitCollection(IExpressionsManager expressionsManager)
        {
            _expressionsManager = expressionsManager;
            list = new List<TemplatedTransit>();
        }

        public void Add(TemplatedTransit transit)
        {
            list.Add(transit);
        }

        public void AddAt(TemplatedTransit transit, int index)
        {
            this[index] = transit;
        }

        public void Remove(TemplatedTransit transit)
        {
            list.Remove(transit);
        }

        public TemplatedTransit this[int index]
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
        public IEnumerator<TemplatedTransit> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("transits");
            foreach (TemplatedTransit transit in this)
                transit.ToXml(writer);
            writer.WriteEndElement();   // transits
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            foreach (XElement transitElement in element.Elements())
            {
                TemplatedTransit transit = new TemplatedTransit(this.ExpressionsManager);
                transit.FromXml(transitElement);
                Add(transit);
            }
        }
    }
}
