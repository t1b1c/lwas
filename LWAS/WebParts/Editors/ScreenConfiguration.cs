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
using System.IO;
using System.Xml;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Storage;
using LWAS.Extensible.Interfaces.WebParts;
using LWAS.Infrastructure;

namespace LWAS.WebParts.Editors
{
    public class ScreenConfiguration
    {
        public IStorageAgent Agent { get; set; }
        public string Application { get; set; }
        public string Screen { get; set; }
        public PersonalizationCrawler PersonalizationCrawler { get; set; }

        public ScreenConfiguration(IStorageAgent agent, string application, string screen)
        {
            if (null == agent) throw new ArgumentNullException("agent");
            if (String.IsNullOrEmpty(application)) throw new ArgumentNullException("application");
            if (String.IsNullOrEmpty(screen)) throw new ArgumentNullException("screen");

            this.Agent = agent;
            this.Application = application;
            this.Screen = screen;
            this.PersonalizationCrawler = new PersonalizationCrawler(agent, screen + ".xml");
        }

        public IEnumerable<IConfiguration> Configurations()
        {
            foreach (PersonalizationCrawler.PartInfo partinfo in this.PersonalizationCrawler.Parts)
                yield return BuildManager.GetConfiguration(this.Application, this.Screen, partinfo.Id);
        }

        public IEnumerable<string> Parts()
        {
            foreach (PersonalizationCrawler.PartInfo partinfo in this.PersonalizationCrawler.Parts)
                yield return partinfo.Id;
        }

        public IConfiguration this[string part]
        {
            get
            {
                return BuildManager.GetConfiguration(this.Application, this.Screen, part);
            }
        }

        public void Assemble()
        {
            string file = this.Screen + ".xml";
            if (!this.Agent.HasKey(file)) throw new InvalidOperationException(String.Format("Storage key '{0}' not found", this.Screen));

            XDocument doc = XDocument.Parse(this.Agent.Read(this.Screen + ".xml"));

            foreach (PersonalizationCrawler.PartInfo partinfo in this.PersonalizationCrawler.Parts)
            {
                string part = partinfo.Id;
                string editkey = partinfo.Name.Substring(0, partinfo.Name.IndexOf(','));
                if (BuildManager.Editors.ContainsKey(editkey))
                {
                    string path = String.Format("/personalization/part[@id='{0}']/property[@name='Configuration']", part);
                    XElement configElement = doc.XPathSelectElement(path);
                    if (null != configElement)
                    {
                        IConfiguration config = this[part];

                        XElement serializedConfigurationElement = configElement.Element("value").Element("Configuration");

                        StringBuilder serializationBuilder = new StringBuilder();
                        using (StringWriter stringWriter = new StringWriter(serializationBuilder))
                        {
                            using (XmlTextWriter textWriter = new XmlTextWriter(stringWriter))
                            {
                                textWriter.Formatting = Formatting.Indented;
                                XmlSerializer serializer = new XmlSerializer(config.GetType());
                                serializer.Serialize(textWriter, config);
                            }
                        }

                        XElement version = XDocument.Parse(serializationBuilder.ToString()).Root;
                        serializedConfigurationElement.ReplaceWith(version);
                    }
                }
            }

            try
            {
                if (this.Agent.HasKey(file))
                    this.Agent.Erase(file);
                using (XmlTextWriter writer = new XmlTextWriter(this.Agent.OpenStream(file), Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    doc.WriteTo(writer);
                }
            }
            finally
            {
                this.Agent.CloseStream(this.Screen);
            }
        }
 
    }
}
