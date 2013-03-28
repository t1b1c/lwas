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
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.IO;

using LWAS.Extensible.Interfaces.Storage;
using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Workflow.Recipes
{
    public abstract class RecipesManager
    {
        string configFile;
        IStorageAgent agent;

        public bool ExpandTree { get; set; }
        public RecipesCollection Recipes { get; set; }

        public RecipesManager(string aConfigFile, IStorageAgent anAgent, bool expandRecipesTree)
        {
            if (String.IsNullOrEmpty(aConfigFile)) throw new ArgumentNullException("aConfigFile");
            if (null == anAgent) throw new ArgumentNullException("anAgent");

            configFile = aConfigFile;
            agent = anAgent;
            this.ExpandTree = expandRecipesTree;

            this.Recipes = new RecipesCollection(this);

            LoadConfiguration();
        }

        public void LoadConfiguration()
        {
            XDocument doc = XDocument.Parse(agent.Read(configFile));
            XElement root = doc.Element("config");
            FromXml(root);
        }

        public void SaveConfiguration()
        {
            agent.Erase(configFile);
            using (XmlTextWriter writer = new XmlTextWriter(agent.OpenStream(configFile), null))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                ToXml(writer);
            }
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("config");

            this.Recipes.ToXml(writer);

            writer.WriteEndElement();   // config
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Recipes.Clear();

            this.Recipes.FromXml(element.Element("recipes"));
        }
    }
}
