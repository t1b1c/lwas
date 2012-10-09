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
using System.Text;
using System.IO;

using LWAS.Extensible.Interfaces.Storage;
using LWAS.Extensible.Interfaces.Expressions;
using SD = LWAS.Database.ScreenDriven;

namespace LWAS.Database
{
    public class ViewsManager
    {
        string configFile;
        IStorageAgent agent;

        public ViewsCollection Views { get; set; }
        public TablesCollection Tables { get; set; }
        public RelationsCollection Relations { get; set; }
        public IExpressionsManager ExpressionsManager { get; set; }

        public ViewsManager(string aConfigFile, IStorageAgent anAgent, IExpressionsManager expressionsManager)
        {
            if (String.IsNullOrEmpty(aConfigFile)) throw new ArgumentNullException("aConfigFile");
            if (null == anAgent) throw new ArgumentNullException("anAgent");
            if (null == expressionsManager) throw new ArgumentNullException("expressionsManager");

            configFile = aConfigFile;
            agent = anAgent;
            this.ExpressionsManager = expressionsManager;

            this.Views = new ViewsCollection(this);
            this.Tables = new TablesCollection(this);
            this.Relations = new RelationsCollection(this);

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
            using (XmlTextWriter writer = new XmlTextWriter(agent.OpenStream(configFile), Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                ToXml(writer);
            }
        }

        public void FillFromScreenDriven(string a, IEnumerable screens)
        {
            if (String.IsNullOrEmpty(a)) throw new ArgumentNullException("a");

            this.Views.Clear();
            this.Tables.Clear();
            this.Relations.Clear();

            screens.OfType<string>()
                   .ToList()
                   .ForEach(s => FillFromScreenDriven(a, s));
        }

        public void FillFromScreenDriven(string a, string s)
        {
            if (String.IsNullOrEmpty(a)) throw new ArgumentNullException("a");
            if (String.IsNullOrEmpty(s)) throw new ArgumentNullException("s");

            SD.Screen screen = new SD.Screen(a, s);
            foreach (SD.Container container in screen)
            {
                string tableName = String.IsNullOrEmpty(container.Alias) ? screen.Key + "_" + container.Name : container.Alias;
                if (!this.Tables.ContainsKey(tableName))
                    AddTableFromScreenDriven(screen, container, tableName);
            }
        }

        public void AddTableFromScreenDriven(SD.Screen screen, SD.Container container, string tableName)
        {
            if (null == screen) throw new ArgumentNullException("screen");
            if (null == container) throw new ArgumentNullException("container");
            if (String.IsNullOrEmpty(tableName))
                tableName = String.IsNullOrEmpty(container.Alias) ? screen.Key + "_" + container.Name : container.Alias;

            Table table = new Table(this, tableName);
            this.Tables.Add(tableName, table);
            foreach (SD.Field sdfield in container)
            {
                Field field = table.Fields.Add(table, sdfield.Name, null, sdfield.Type, sdfield.Name == "ID");
                Field relatedField = null;

                if (!String.IsNullOrEmpty(sdfield.LinkedScreen) && !String.IsNullOrEmpty(sdfield.LinkedField) && !sdfield.IsExcluded)
                {
                    SD.Container linkedcontainer = screen.All()
                                                         .Select<SD.Screen, SD.Container>(scr =>
                                                             scr.ToList<SD.Container>()
                                                                .Find(c =>
                                                                {
                                                                    if (String.IsNullOrEmpty(sdfield.LinkedContainer))
                                                                        return c.Alias == sdfield.LinkedScreen;
                                                                    else
                                                                        return c.Screen.Key == sdfield.LinkedScreen && c.Alias == sdfield.LinkedContainer;
                                                                }))
                                                         .Where(c => c != null)
                                                         .SingleOrDefault();

                    if (null != linkedcontainer)
                    {
                        string linkedtablename = String.IsNullOrEmpty(sdfield.LinkedContainer) ? linkedcontainer.Alias : sdfield.LinkedScreen + "_" + sdfield.LinkedContainer;

                        if (!String.IsNullOrEmpty(linkedtablename) &&
                            !this.Tables.ContainsKey(linkedtablename))
                        {
                            AddTableFromScreenDriven(screen, linkedcontainer, linkedtablename);
                        }

                        relatedField = this.Tables[linkedtablename]
                                           .Fields
                                           .FirstOrDefault(f => f.Name == sdfield.LinkedField);
                    }
                }

                if (null != relatedField)
                    this.Relations.Add(field, relatedField);
            }
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");
            
            writer.WriteStartElement("config");

            this.Views.ToXml(writer);
            this.Tables.ToXml(writer);
            this.Relations.ToXml(writer);

            writer.WriteEndElement();   // config
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Views.Clear();
            this.Tables.Clear();
            this.Relations.Clear();

            this.Tables.FromXml(element.Element("tables"));
            this.Relations.FromXml(element.Element("relations"));
            this.Views.FromXml(element.Element("views"));
        }

        public void ToSql(StringBuilder builder)
        {
            if (null == builder) throw new ArgumentNullException("builder");
        }
    }
}
