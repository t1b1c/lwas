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
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Text;
using System.IO;

using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.WebParts
{
	public class XmlDataSourceWebPart : DataSourceProviderWebPart
    {
        public class Values : IEnumerable<string>
        {
            Dictionary<string, string> all = new Dictionary<string, string>();

            public string this[string name]
            {
                get { return all[name]; }
                set
                {
                    if (String.IsNullOrEmpty(name) || !all.ContainsKey(name))
                        all.Add(name, null);
                    all[name] = value;
                }
            }

            public string First
            {
                get { return all.Keys.FirstOrDefault(); }
            }

            public IEnumerator<string> GetEnumerator()
            {
                return all.Keys.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

		XmlDataSource xmlDataSource;
        public Values NewValues { get; set; }
        public IStorageAgent Agent { get; set; }
		
		public override object DataSource
		{
			get { return xmlDataSource;} 
			set 
            {
                if (null != value)
                    this.xmlDataSource.Data = value.ToString();
                base.DataSource = this.xmlDataSource;
            }
		}

        string _key;
        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                xmlDataSource.Data = this.Agent.Read(_key);
            }
        }

        string _transformationKey;
        public string TransformationKey
        {
            get { return _transformationKey; }
            set
            {
                _transformationKey = value;
                xmlDataSource.Transform = this.Agent.Read(_transformationKey);
            }
        }

        string _path;
        public string Path 
        {
            get { return _path; }
            set 
            { 
                _path = value;
                xmlDataSource.XPath = _path;
            }
        }

        string _filePath = null;
        public string FilePath
        {
            get 
            {
                if (null == _filePath)
                    _filePath = GetFilePath();
                return _filePath;
            }
        }

        public bool Cancel { get; set; }

        public IHierarchicalEnumerable Data
        {
            get
            {
                HierarchicalDataSourceView view = ((IHierarchicalDataSource)xmlDataSource).GetHierarchicalView(null);
                if (null != view)
                    return view.Select();
                return null;
            }
        }

        public string Command
        {
            set { OnCommand(value); }
        }

		public XmlDataSourceWebPart()
		{
            xmlDataSource = new XmlDataSource();
            xmlDataSource.EnableCaching = false;
		}

        private void OnCommand(string command)
        {
            OnMilestone("before " + command);
            if (this.Cancel)
                OnMilestone("cancel " + command);
            else
            {
                switch (command)
                {
                    case "new":
                        OnNew();
                        break;
                    case "save":
                        OnSave();
                        break;
                    case "delete":
                        OnDelete();
                        break;
                }

                OnMilestone(command);
            }
        }

        private void OnNew()
        {
            this.NewValues = new Values();
        }

        private void OnSave()
        {
            if (String.IsNullOrEmpty(_key))
                return;

            XDocument doc = LoadDocument();

            string path = this.Path;
            if (path.EndsWith("/*"))
                path = path.Remove(path.LastIndexOf("/*"));
            XElement current = doc.XPathSelectElement(path);

            if (null != current)
            {
                string name = this.NewValues.First;
                if (!String.IsNullOrEmpty(name))
                {
                    XElement element = new XElement(name);
                    foreach (string attribute in this.NewValues)
                    {
                        if (attribute == name)
                            continue;
                        element.Add(new XAttribute(attribute, this.NewValues[attribute]));
                    }
                    current.Add(element);
                }
            }

            SaveDocument(doc);
        }

        XDocument LoadDocument()
        {
            XDocument doc = null;
            using (XmlNodeReader reader = new XmlNodeReader(xmlDataSource.GetXmlDocument()))
            {
                reader.MoveToContent();
                doc = XDocument.Load(reader);
            }
            return doc;
        }

        void SaveDocument(XDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb))
            {
                doc.Save(writer);
            }
            this.Agent.Write(this.Key, sb.ToString());
        }

        private void OnDelete()
        {
            if (String.IsNullOrEmpty(_key))
                return;

            XDocument doc = LoadDocument();

            string path = this.Path;
            if (path.EndsWith("/*"))
                path = path.Remove(path.LastIndexOf("/*"));
            XElement current = doc.XPathSelectElement(path);

            current.Remove();
            SaveDocument(doc);
        }

        string GetFilePath()
        {
            XDocument doc = LoadDocument();

            string path = this.Path;
            if (path.EndsWith("/*"))
                path = path.Remove(path.LastIndexOf("/*"));
            XElement current = doc.XPathSelectElement(path);

            var ancestors = from e in current.AncestorsAndSelf()
                            select "/" + e.Attribute("name").Value + "";


            return String.Join("", ancestors.Reverse()
                                            .Where(el => 
                                                    {
                                                        return "/root" != el;
                                                    })
                                            .ToArray());

        }

        public override void Initialize()
        {
            base.Initialize();

            if (null == this.Agent) throw new ApplicationException("Storage agent not set");
        }
	}
}
