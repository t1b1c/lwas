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
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

using LWAS.Extensible.Interfaces.Storage;
using LWAS.Extensible.Interfaces.WorkFlow;

using LWAS.Infrastructure;
using LWAS.Infrastructure.Security;

namespace LWAS.WorkFlow
{
	public class StatePersistence : IStatePersistence
	{
        public const string STORAGE_KEY = "state.xml";

        IStorageAgent _agent;
        public IStorageAgent Agent
        {
            get { return this._agent; }
            set { this._agent = value; }
        }
        
        Dictionary<string, object> _data;
        public Dictionary<string, object> Data
		{
			get { return this._data; }
		}
        
		public void Open()
		{
			if (null == this._agent) throw new InvalidOperationException("Agent not set");

            // this._data = (HttpContext.Current.Cache[User.CurrentUser.Name] as Dictionary<string, object>);

            XDocument doc = null;
            if (_agent.HasKey(STORAGE_KEY))
                doc = XDocument.Parse(_agent.Read(STORAGE_KEY));

			if (null == this._data)
			{
				this._data = new Dictionary<string, object>();
                if (null != doc)
                {
                    foreach (string normalizedkey in this._agent.List())
                    {
                        XElement keyelement = doc.Element("keys")
                                                 .Elements()
                                                 .SingleOrDefault(x => x.Attribute("normalized").Value == normalizedkey);
                        if (null != keyelement)
                        {
                            string key = keyelement.Attribute("full").Value;
                            string persisteddata = _agent.Read(normalizedkey);
                            if (!String.IsNullOrEmpty(persisteddata))
                                this._data.Add(key, SerializationServices.BinaryDeserialize(persisteddata));
                        }
                    }
                }
			}
		}

        public bool ContainsKey(string key)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
			if (null == this._data) throw new InvalidOperationException("State persistance storage is not opened");

			return this._data.ContainsKey(key);
		}

		public void Push(string key, object state)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
			if (null == this._data) throw new InvalidOperationException("State persistance storage is not opened");

			if (this._data.ContainsKey(key))
				this._data[key] = state;
			else
				this._data.Add(key, state);
		}

		public object Pull(string key)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
			if (null == this._data) throw new InvalidOperationException("State persistance storage is not opened");
			if (!this._data.ContainsKey(key)) throw new ArgumentException(string.Format("State persistance storage cannot find the key '{0}'", key));

			if (null != this._data[key] || this._agent.HasKey(key))
				return this._data[key];

            return null;
		}
		public void Erase(string key)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            if (null == this._data) throw new InvalidOperationException("State persistance storage is not opened");

			if (this._data.ContainsKey(key))
				this._data.Remove(key);

            this._agent.Erase(_agent.Sanitize(key));
		}
		public void Close()
		{
			if (null == this._agent) throw new InvalidOperationException("Agent not set");

			if (null != this._data)
			{
				if (this._data.Count > 0)
				{
                    XDocument doc = new XDocument(new XDeclaration("1.0", null, null),
                                                    new XElement("keys"));
					// HttpContext.Current.Cache.Insert(User.CurrentUser.Name, this._data);
					foreach (string key in this._data.Keys)
					{
                        string sanitizedkey = key;
                        if (null != this._data[key])
						{
                            sanitizedkey = _agent.Sanitize(key);
                            _agent.Write(sanitizedkey, SerializationServices.BinarySerialize(_data[key]));
						}
                        doc.Element("keys").Add(new XElement("key",
                                                    new XAttribute("full", key),
                                                    new XAttribute("normalized", sanitizedkey)
                                                ));
                    }
                    if (_agent.HasKey(STORAGE_KEY))
                        _agent.Erase(STORAGE_KEY);
                    _agent.Write(STORAGE_KEY, doc.ToString());
                    _data = null;
				}
			}
		}
	}
}
