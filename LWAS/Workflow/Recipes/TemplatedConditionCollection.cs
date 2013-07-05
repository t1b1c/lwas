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
using System.IO;

using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Workflow.Recipes
{
    public class TemplatedConditionCollection : IEnumerable<TemplatedCondition>
    {
        List<TemplatedCondition> list;
        
        IExpressionsManager _expressionsManager;
        public IExpressionsManager ExpressionsManager
        {
            get { return _expressionsManager; }
        }

        public TemplatedConditionCollection(IExpressionsManager expressionsManager)
        {
            _expressionsManager = expressionsManager;
            list = new List<TemplatedCondition>();
        }

        public void Add(TemplatedCondition condition)
        {
            list.Add(condition);
        }

        public void AddAt(TemplatedCondition condition, int index)
        {
            this[index] = condition;
        }

        public void Remove(TemplatedCondition condition)
        {
            list.Remove(condition);
        }

        public TemplatedCondition this[int index]
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
        public IEnumerator<TemplatedCondition> GetEnumerator()
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

            writer.WriteStartElement("conditions");
            foreach (TemplatedCondition condition in this)
                condition.ToXml(writer);
            writer.WriteEndElement();   // conditions
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            foreach (XElement conditionElement in element.Elements())
            {
                TemplatedCondition condition = new TemplatedCondition(this.ExpressionsManager);
                condition.FromXml(conditionElement);
                Add(condition);
            }
        }

        public string ToXmlString()
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter stringWriter = new StringWriter(sb))
            {
                using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
                {
                    ToXml(writer);
                }
            }
            return sb.ToString();
        }
    }
}
