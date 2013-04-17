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
using System.Xml;
using System.Xml.Linq;

namespace LWAS.Database
{
    public class ParametersCollection : IEnumerable<string>
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();

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
                if (parameters.ContainsKey(key))
                    parameters[key] = value;
            }
        }

        public void Add(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (!parameters.ContainsKey(name))
                parameters.Add(name, null);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return parameters.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
