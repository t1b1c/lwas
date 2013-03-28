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
using System.IO;
using System.Xml;

using LWAS.Extensible.Interfaces.Storage;
using LWAS.Infrastructure;

namespace LWAS.WebParts.Editors
{
	public class PersonalizationCrawler
	{
		public class PartInfo
		{
			public string Id;
			public string Title;
			public string Name;
			public string Zone;
			public bool IsContainer;
			public bool IsProxy;
			public PartInfo(string anId, string aTitle, string aName, string aZone, bool isContainer, bool isProxy)
			{
				this.Id = anId;
				this.Title = aTitle;
				this.Name = aName;
				this.Zone = aZone;
				this.IsContainer = isContainer;
				this.IsProxy = isProxy;
			}
		}

		private IStorageAgent agent;
		private string key;
		private List<PersonalizationCrawler.PartInfo> _parts;
		private List<PersonalizationCrawler.PartInfo> _containers;
		private List<PersonalizationCrawler.PartInfo> _proxies;
		
        public List<PersonalizationCrawler.PartInfo> Parts
		{
			get { return this._parts; }
		}
		
        public List<PersonalizationCrawler.PartInfo> Containers
		{
			get { return this._containers; }
		}

		public List<PersonalizationCrawler.PartInfo> Proxies
		{
			get { return this._proxies; }
		}

		public PersonalizationCrawler(IStorageAgent anAgent, string aKey)
		{
			this.agent = anAgent;
			this.key = aKey;
			this._parts = new List<PersonalizationCrawler.PartInfo>();
			this._containers = new List<PersonalizationCrawler.PartInfo>();
			this._proxies = new List<PersonalizationCrawler.PartInfo>();
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(this.agent.Read(this.key));
			XmlNode infoNode = doc.SelectSingleNode("/personalization/part/property[@name='lwas.info']/value");
			if (null != infoNode)
			{
				Type type = Type.GetType(infoNode.Attributes["type"].Value);
				using (StringReader stringReader = new StringReader(infoNode.OuterXml))
				{
					using (XmlTextReader textReader = new XmlTextReader(stringReader))
					{
						textReader.MoveToContent();
						object[] arrObj = SerializationServices.Deserialize(type, textReader) as object[];
						for (int i = 0; i < arrObj.Length; i += 6)
						{
							string id = (string)arrObj[i];
							string title = (string)arrObj[i + 1];
							string name = (string)arrObj[i + 2];
							string zone = (string)arrObj[i + 3];
							bool isContainer = (bool)arrObj[i + 4];
							bool isProxy = (bool)arrObj[i + 5];
							PersonalizationCrawler.PartInfo info = new PersonalizationCrawler.PartInfo(id, title, name, zone, isContainer, isProxy);
							this._parts.Add(info);
							if (isContainer)
								this._containers.Add(info);
							if (isProxy)
								this._proxies.Add(info);
						}
					}
				}
			}
		}

        public static string Version(IStorageAgent agent, string key)
        {
            string version = null;
            XmlDocument doc = new XmlDocument();
            if (agent.HasKey(key))
            {
                string content = agent.Read(key);
                if (!String.IsNullOrEmpty(content))
                {
                    doc.LoadXml(agent.Read(key));
                    XmlNode versionNode = doc.SelectSingleNode("/personalization");
                    if (null != versionNode.Attributes["version"])
                        version = versionNode.Attributes["version"].Value;
                }
            }

            return version;
        }
	}
}
