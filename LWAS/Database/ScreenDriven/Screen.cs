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
using System.Collections;
using System.Collections.Generic;
using System.Configuration; 
using System.Xml;
using System.IO;
using System.Text;

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Storage;

using LWAS.WebParts.Editors;
using LWAS.Infrastructure.Storage;

namespace LWAS.Database.ScreenDriven
{
    public class Screen : IEnumerable<Container>
    {
        List<string> list;
        IStorageContainer databaseContainer;
        XmlNode extra = null;

        string _application;
        public string Application
        {
            get { return _application; }
        }

        string _key;
        public string Key
        {
            get { return _key; }
        }

        public int Count
        {
            get
            {
                if (null == list)
                    return -1;

                return list.Count;
            }
        }

        public Container this[string container]
        {
            get
            {
                IConfiguration config = BuildManager.GetConfiguration(_application, _key, container);
                XmlNode containerExtra = GetContainerNode(container);
                if (null != config)
                    return new Container(config, this, container, containerExtra);
                else
                    return null;
            }
        }

        public Screen(string application, string screen)
        {
            if (String.IsNullOrEmpty(application)) throw new ArgumentNullException("application");
            if (String.IsNullOrEmpty(screen)) throw new ArgumentNullException("screen");
            string bin = ConfigurationManager.AppSettings["EDIT_REPO"];
            if (String.IsNullOrEmpty(bin)) throw new ApplicationException("'EDIT_REPO' web.config key not set");
            string database = ConfigurationManager.AppSettings["EDIT_REPO_DATABASE"];
            if (String.IsNullOrEmpty(database)) throw new InvalidOperationException("EDIT_REPO_DATABASE key not set in config file");

            _application = application;
            _key = screen;

            list = BuildManager.ListContainers(_application, _key);

            databaseContainer = new DirectoryContainer().CreateContainer(bin).CreateContainer(application).CreateContainer(database);
            XmlDocument doc = new XmlDocument();
            if (!databaseContainer.Agent.HasKey(screen + ".xml"))
                doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));
            else
                doc.LoadXml(databaseContainer.Agent.Read(screen + ".xml"));
            extra = doc.SelectSingleNode("containers");
            if (null == extra)
            {
                extra = doc.CreateNode(XmlNodeType.Element, "containers", null);
                doc.AppendChild(extra);
            }
        }

        public void Save()
        {
        }

        public void Write()
        {
            databaseContainer.Agent.Erase(_key + ".xml");
            databaseContainer.Agent.Write(_key + ".xml", extra.OwnerDocument.OuterXml);

            StringBuilder xmlBuilder = new StringBuilder();
            using (StringWriter stringWriter = new StringWriter(xmlBuilder))
            {
                using (XmlTextWriter textWriter = new XmlTextWriter(stringWriter))
                {
                    textWriter.Formatting = Formatting.Indented;

                    extra.OwnerDocument.Save(textWriter);
                }
            }

            databaseContainer.Agent.Erase(_key + ".xml");
            databaseContainer.Agent.Write(_key + ".xml", xmlBuilder.ToString());
        }

        List<Screen> screens = null;
        public List<Screen> All()
        {
            if (null == screens)
            {
                string file = ConfigurationManager.AppSettings["EDIT_APPS"];
                if (String.IsNullOrEmpty(file)) throw new InvalidOperationException("EDIT_APPS key not set in config file");

                string path = ConfigurationManager.AppSettings["EDIT_REPO"];
                if (String.IsNullOrEmpty(path)) throw new InvalidOperationException("EDIT_REPO key not set in config file");

                string portfolio = ConfigurationManager.AppSettings["EDIT_REPO_PORTFOLIO"];
                if (String.IsNullOrEmpty(portfolio)) throw new InvalidOperationException("EDIT_REPO_PORTFOLIO key not set in config file");

                screens = new List<Screen>();
                PortfolioDataSource dataSource = new PortfolioDataSource(portfolio, path, file);
                foreach (string screen in dataSource.ListScreens(_application))
                    screens.Add(new Screen(_application, screen));
            }
  
            return screens;
        }

        List<Container> links = null;
        public List<Container> ListLinkedContainers(Container root)
        {
            if (null == links)
            {
                links = new List<Container>();

                foreach(Screen screen in All())
                    foreach (Container candidate in screen)
                        foreach (Field field in candidate)
                            if ((!String.IsNullOrEmpty(root.Alias) && root.Alias == field.LinkedScreen) ||
                                (root.Screen.Key == field.LinkedScreen && root.Name == field.LinkedContainer))
                            {
                                links.Add(candidate);
                                break;
                            }
            }

            return links;
        }

        #region IEnumerable<Container> Members

        public IEnumerator<Container> GetEnumerator()
        {
            foreach (string container in list)
            {
                IConfiguration config = BuildManager.GetConfiguration(_application, _key, container);
                if (null == config)
                    continue;
                XmlNode containerExtra = GetContainerNode(container);
                yield return new Container(config, this, container, containerExtra);
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private XmlNode GetContainerNode(string container)
        {
            XmlNode containerExtra = null;
            if (null != extra)
                containerExtra = extra.SelectSingleNode(container);
            if (null == containerExtra)
            {
                containerExtra = extra.OwnerDocument.CreateNode(XmlNodeType.Element, container, null);
                extra.AppendChild(containerExtra);
            }

            return containerExtra;
        }
    }
}
