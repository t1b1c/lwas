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
using System.Configuration;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Web.Caching;

namespace LWAS.Infrastructure.Personalization
{
	public class XmlPersonalizationProvider : PersonalizationProvider
	{
        private static object SyncRoot = new object();

        public static string StorageTemplate
		{
			get { return Path.GetFileNameWithoutExtension(HttpContext.Current.Request.PhysicalPath); }
		}
		
        public static string StorageKey
		{
			get
			{
				string screenKey = ConfigurationManager.AppSettings["SCREEN"];
				if (string.IsNullOrEmpty(screenKey)) throw new InvalidOperationException("QueryString SCREEN key not defined in web.config!");

				string screen = HttpContext.Current.Request.QueryString[screenKey];
				string result;
				if (string.IsNullOrEmpty(screen))
					result = XmlPersonalizationProvider.StorageTemplate;
				else
					result = screen;

                return result;
			}
		}

        public bool IsEnabled { get; set; }
        public override string ApplicationName { get; set; }

		public XmlPersonalizationProvider()
		{
            bool enabled = true;
			bool.TryParse(ConfigurationManager.AppSettings["DESIGN"], out enabled);
            this.IsEnabled = enabled;
		}

		protected virtual string GetScreenUniqueIdentifier()
		{
			return Path.Combine(HttpContext.Current.Request.PhysicalPath, XmlPersonalizationProvider.StorageKey);
		}

		public override void SavePersonalizationState(PersonalizationState state)
		{
			if (this.IsEnabled)
			{
				DictionaryPersonalizationState dictionaryState = state as DictionaryPersonalizationState;
				if (null == dictionaryState)
                    throw new ArgumentException("state is not a DictionaryPersonalizationState");

				if (!dictionaryState.ReadOnly)
				{
					StringBuilder personalizationBuilder = new StringBuilder();
					using (StringWriter stringWriter = new StringWriter(personalizationBuilder))
					{
						using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
						{
							writer.Formatting = Formatting.Indented;
							writer.WriteStartDocument();
							writer.WriteStartElement("personalization");
                            writer.WriteAttributeString("version", ReflectionServices.Version());
							foreach (string id in dictionaryState.States.Keys)
							{
								if (dictionaryState.IsPartPresent(id))
								{
									writer.WriteStartElement("part");
									writer.WriteAttributeString("id", id);
									foreach (string propertyName in dictionaryState.States[id].Keys)
									{
										writer.WriteStartElement("property");
										writer.WriteAttributeString("name", propertyName);
										writer.WriteStartAttribute("sensitive");
										writer.WriteValue(dictionaryState.States[id][propertyName].IsSensitive);
										writer.WriteEndAttribute();
										writer.WriteStartAttribute("scope");
										writer.WriteValue((int)dictionaryState.States[id][propertyName].Scope);
										writer.WriteEndAttribute();
										object value = dictionaryState.States[id][propertyName].Value;
										if (null != value)
										{
											writer.WriteStartElement("value");
											writer.WriteStartAttribute("type");
                                            writer.WriteValue(SerializationServices.ShortAssemblyQualifiedName(value.GetType().AssemblyQualifiedName));
											writer.WriteEndAttribute();
											SerializationServices.Serialize(value, writer);
											writer.WriteEndElement();
										}
										writer.WriteEndElement();
									}
									writer.WriteEndElement();
								}
							}
							writer.WriteEndElement();
							writer.WriteEndDocument();
						}
					}
					PersonalizationStorage.Instance.Write(XmlPersonalizationProvider.StorageKey, personalizationBuilder.ToString());
				}
			}
		}
		public override PersonalizationState LoadPersonalizationState(WebPartManager webPartManager, bool ignoreCurrentUser)
		{
			if (null == webPartManager) throw new ArgumentNullException("webPartManager is null");
            DictionaryPersonalizationState state = new DictionaryPersonalizationState(webPartManager);
			string suid = this.GetScreenUniqueIdentifier();

            Cache cache = HttpRuntime.Cache;
            lock (SyncRoot)
            {
                Dictionary<string, PersonalizationDictionary> cachedstates = cache[suid] as Dictionary<string, PersonalizationDictionary>;
                if ((this.IsEnabled && !state.ReadOnly)  || null == cachedstates)
                {
                    string storage = PersonalizationStorage.Instance.Read(XmlPersonalizationProvider.StorageKey, XmlPersonalizationProvider.StorageTemplate);
                    if (!string.IsNullOrEmpty(storage))
                    {
                        using (XmlTextReader reader = new XmlTextReader(new StringReader(storage)))
                        {
                            reader.MoveToContent();
                            if (reader.MoveToAttribute("readOnly"))
                            {
                                bool isReadOnly = false;
                                bool.TryParse(reader.Value, out isReadOnly);
                                state.ReadOnly = isReadOnly;
                                reader.MoveToElement();
                            }
                            if (reader.ReadToDescendant("part"))
                            {
                                int partDepth = reader.Depth;
                                do
                                {
                                    reader.MoveToElement();
                                    reader.MoveToAttribute("id");
                                    string id = reader.Value;
                                    PersonalizationDictionary dictionary = new PersonalizationDictionary();
                                    reader.MoveToContent();
                                    if (reader.ReadToDescendant("property"))
                                    {
                                        int propertyDepth = reader.Depth;
                                        do
                                        {
                                            reader.MoveToElement();
                                            reader.MoveToAttribute("name");
                                            string name = reader.Value;
                                            reader.MoveToAttribute("sensitive");
                                            bool sensitive = bool.Parse(reader.Value);
                                            reader.MoveToAttribute("scope");
                                            PersonalizationScope scope = (PersonalizationScope)int.Parse(reader.Value);
                                            object value = null;
                                            reader.MoveToContent();
                                            if (reader.ReadToDescendant("value"))
                                            {
                                                reader.MoveToAttribute("type");
                                                if (reader.HasValue)
                                                {
                                                    Type type = Type.GetType(reader.Value);
                                                    if (type == null && name == "Configuration")
                                                    {
                                                        type = Type.GetType("LWAS.Infrastructure.Configuration.Configuration, LWAS");
                                                    }
                                                    reader.MoveToContent();
                                                    value = SerializationServices.Deserialize(type, reader);
                                                }
                                            }
                                            dictionary.Add(name, new PersonalizationEntry(value, scope, sensitive));
                                            reader.MoveToElement();
                                            while (propertyDepth < reader.Depth && reader.Read())
                                            {
                                            }
                                        }
                                        while (reader.ReadToNextSibling("property"));
                                    }
                                    state.States.Add(id, dictionary);
                                    reader.MoveToElement();
                                    while (partDepth < reader.Depth && reader.Read())
                                    {
                                    }
                                }
                                while (reader.ReadToNextSibling("part"));
                            }
                        }
                    }

                    string fileToMonitor = PersonalizationStorage.Instance.BuildPath(StorageKey);
                    if (!PersonalizationStorage.Instance.Agent.HasKey(fileToMonitor))
                        fileToMonitor = PersonalizationStorage.Instance.BuildPath(StorageTemplate);
                    cache.Insert(suid, state.States, new CacheDependency(HttpContext.Current.Server.MapPath(fileToMonitor)));
                }
                else
                    state.States = cachedstates;
            }

			return state;
		}
		public override PersonalizationStateInfoCollection FindState(PersonalizationScope scope, PersonalizationStateQuery query, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new Exception("The method or operation is not implemented.");
		}
		public override int GetCountOfState(PersonalizationScope scope, PersonalizationStateQuery query)
		{
			throw new Exception("The method or operation is not implemented.");
		}
		public override int ResetState(PersonalizationScope scope, string[] paths, string[] usernames)
		{
			throw new Exception("The method or operation is not implemented.");
		}
		public override int ResetUserState(string path, DateTime userInactiveSinceDate)
		{
			throw new Exception("The method or operation is not implemented.");
		}
		protected override void LoadPersonalizationBlobs(WebPartManager webPartManager, string path, string userName, ref byte[] sharedDataBlob, ref byte[] userDataBlob)
		{
			throw new Exception("The method or operation is not implemented.");
		}
		protected override void ResetPersonalizationBlob(WebPartManager webPartManager, string path, string userName)
		{
			throw new Exception("The method or operation is not implemented.");
		}
		protected override void SavePersonalizationBlob(WebPartManager webPartManager, string path, string userName, byte[] dataBlob)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
