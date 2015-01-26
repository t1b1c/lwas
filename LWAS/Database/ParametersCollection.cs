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
using System.Xml;
using System.Xml.Linq;

namespace LWAS.Database
{
    public class ParametersCollection : IEnumerable<string>
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        ParametersCollection linked_collection;

        public ParametersCollection()
        {
        }

        public ParametersCollection(ParametersCollection collection)
        {
            linked_collection = collection;
            foreach (string key in collection)
                parameters.Add(key, collection[key]);
        }

        public object this[string key]
        {
            get
            {
                if (!parameters.ContainsKey(key))
                    parameters.Add(key, null);
                return parameters[key];
            }
            set
            {
                if (!parameters.ContainsKey(key))
                    parameters.Add(key, null);
                parameters[key] = value;

                if (null != linked_collection)
                    linked_collection[key] = value;
            }
        }

        public ParametersCollection Append(IEnumerable<string> list)
        {
            foreach (string p in list)
                if (!parameters.ContainsKey(p))
                    parameters.Add(p, null);

            return this;
        }

        public bool Contains(string name)
        {
            return parameters.ContainsKey(name);
        }

        public void Add(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (!parameters.ContainsKey(name))
                parameters.Add(name, null);
        }

        public void Remove(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (parameters.ContainsKey(name))
                parameters.Remove(name);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return parameters.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsEmpty
        {
            get { return parameters.Count == 0; }
        }

        public string SqlIdentifier(string parameter)
        {
            if (String.IsNullOrEmpty(parameter)) throw new ArgumentNullException("parameter");
            if (!parameters.ContainsKey(parameter)) throw new ArgumentException(String.Format("Unknown parameter '{0}'", parameter));

            return String.Format("@p{0}", Position(parameter).ToString());
        }

        int Position(string parameter)
        {
            int count = 0;
            foreach (string key in parameters.Keys)
            {
                if (key == parameter)
                    return count;
                count++;
            }
            return -1;
        }

        public void ToXml(XmlWriter writer)
        {
            writer.WriteStartElement("parameters");
            foreach (string key in parameters.Keys)
                writer.WriteElementString("parameter", key);
            writer.WriteEndElement();   // parameters
        }

        public void FromXml(XElement element)
        {
            foreach (XElement paramElement in element.Elements("parameter"))
                parameters.Add(paramElement.Value, null);
        }
    }
}
